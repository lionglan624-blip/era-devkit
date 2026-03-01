# Feature 775: Collection (SHOP_COLLECTION.ERB)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-16T00:00:00Z -->

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

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section defines the scope; F647 decomposed it into actionable sub-features. Phase 20: Equipment & Shop Systems philosophy inherited per Sub-Feature Requirements.

### Problem (Current Issue)

ShopSystem.ShowCollection() is a NotImplementedException stub (ShopSystem.cs:346-347) created by F774. The underlying SHOP_COLLECTION.ERB contains 7 functions with complex string manipulation (STRCOUNT/REPLACE on SAVESTR:10 "/" delimiters), probabilistic risk check formulas, MONEY mutation via direct assignment, and external ERB function dependencies (PANTSNAME, PHOTO_NAME, NTR_NAME). The MONEY += pattern requires a SetMoney or AddMoney method that does not exist on IEngineVariables, which was designed as read-only by F790 because no Phase 20 consumer needed write access until this feature.

### Goal (What to Achieve)

1. Create CollectionTracker.cs (architecture deliverable) migrating all 7 SHOP_COLLECTION.ERB functions to C#
2. Replace ShopSystem.ShowCollection() stub with delegation to CollectionTracker
3. Add SetMoney (or AddMoney) to IEngineVariables to support MONEY mutation
4. Rename ShopDisplay.PrintLineC to PrintColumnLeft (F788 Mandatory Handoff)
5. Preserve SAVESTR:10 "/" delimiter format, risk check probabilistic formula, and EXP index 113 semantics
6. Achieve zero technical debt (no TODO/FIXME/HACK) per Phase 20 Sub-Feature Requirements
7. Include equivalence tests verifying behavioral parity with legacy ERB implementation

<!-- Sub-Feature Requirements (architecture.md:90-97): Applied
  1. Philosophy: Phase 20: Equipment & Shop Systems - inherited in Philosophy section
  2. Tasks: Debt cleanup (TODO/FIXME/HACK deletion) - reflected in C8 constraint
  3. Tasks: Equivalence tests - reflected in C12 constraint
  4. AC: Zero debt verification - reflected in C8 constraint
-->

<!-- F788 Mandatory Handoffs (received):
  - F788 Task 10: F788 added as Predecessor in Dependencies
  - ShopDisplay.PrintLineC -> PrintColumnLeft rename tracking (F788 Mandatory Handoffs Row 3) - reflected in C7 constraint
-->

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is SHOP_COLLECTION.ERB not yet migrated? | ShopSystem.ShowCollection() is a NotImplementedException stub | ShopSystem.cs:346-347 |
| 2 | Why was it deferred from F774? | Complex string manipulation (STRCOUNT/REPLACE on SAVESTR:10) and pricing logic required dedicated feature | SHOP_COLLECTION.ERB:94-98 |
| 3 | Why can't it be trivially ported? | MONEY += direct mutation requires setter method not on IEngineVariables | SHOP_COLLECTION.ERB:74, IEngineVariables.cs |
| 4 | Why is SetMoney missing? | F790 designed IEngineVariables as read-only (no Phase 20 consumer needed write access until F775) | IEngineVariables.cs:5 |
| 5 | Why (Root)? | F775 is the first Phase 20 feature requiring MONEY mutation, exposing a gap in the read-only engine variable interface | no SetMoney in entire codebase |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | ShowCollection() throws NotImplementedException | IEngineVariables lacks SetMoney; external ERB functions (PANTSNAME, PHOTO_NAME) lack C# equivalents |
| Where | ShopSystem.cs:346-347 (stub) | IEngineVariables.cs interface definition; external ERB files CLOTHES.ERB, COMF446.ERB |
| Fix | Remove stub, inline code | Add SetMoney to IEngineVariables, create CollectionTracker class with DI, stub external functions |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Predecessor - Phase 20 Planning (decomposed this feature) |
| F774 | [DONE] | Predecessor - Shop Core, ShowCollection() stub created here |
| F788 | [DONE] | Predecessor - IConsoleOutput extensions + PrintLineC rename handoff |
| F789 | [DONE] | Related - IStringVariables for SAVESTR:10 access |
| F790 | [DONE] | Related - IEngineVariables (read-only; needs SetMoney extension) |
| F791 | [DONE] | Related - IGameState mode transitions (not used by SHOP_COLLECTION) |
| F776 | [DRAFT] | Related - Sibling Phase 20 (also needs MONEY mutation) |
| F777 | [DRAFT] | Related - Sibling Phase 20, zero cross-calls |
| F782 | [DRAFT] | Successor - Post-Phase Review depends on F775 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface availability | FEASIBLE | All needed interfaces (IConsoleOutput, IStringVariables, IVariableStore, IEngineVariables, IInputHandler) exist; single gap (SetMoney) resolvable within scope |
| Architecture deliverable defined | FEASIBLE | CollectionTracker.cs specified in phase-20-27-game-systems.md:72 |
| Sibling isolation | FEASIBLE | Zero cross-calls to Phase 20 siblings (F776, F777); only inbound from F774 |
| External function dependencies | FEASIBLE | PANTSNAME, PHOTO_NAME, NTR_NAME can use NotImplementedException stubs (F774 pattern) |
| Predecessor completion | FEASIBLE | F647, F774, F788, F789, F790 all [DONE] |

**Verdict**: FEASIBLE -- The single interface gap (SetMoney on IEngineVariables) is a 1-line addition within this feature's scope. External ERB functions use NotImplementedException stubs matching F774 pattern.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| IEngineVariables interface | MEDIUM | Adding SetMoney method requires updating all implementors (limited count) |
| ShopSystem.cs | LOW | Replace stub with delegation call to CollectionTracker |
| ShopDisplay.cs | LOW | Rename PrintLineC to PrintColumnLeft (F788 handoff) |
| Era.Core.Tests | MEDIUM | New CollectionTrackerTests.cs with equivalence tests |
| Phase 20 siblings (F776, F777) | LOW | Zero cross-calls; F776 benefits from SetMoney addition |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IEngineVariables has no SetMoney | IEngineVariables.cs | Must add SetMoney(int) or AddMoney(int) to interface and all implementors |
| External ERB functions lack C# equivalents | CLOTHES.ERB, COMF446.ERB, NTR_MESSAGE.ERB | PANTSNAME, PHOTO_NAME: NotImplementedException stubs; NTR_NAME(0): inline as "訪問者" |
| PRINTFORM column alignment | ERB %text,38,LEFT% | C# string formatting PadRight/PadLeft; full-width character width may differ |
| STRCOUNT no direct C# equivalent | ERB built-in | Implement as utility method or use LINQ Count |
| Multi-value RETURN pattern | ERB RESULT:0/1/2 | C# tuples (int, int) or (int, int, int) |
| F788 Mandatory Handoff | feature-788.md Mandatory Handoffs Row 3 | Rename ShopDisplay.PrintLineC to PrintColumnLeft |
| EXP index for ポルノ経験 = 113 | EXP.yaml:161 | Use ExpIndex(113) or well-known constant |
| Architecture deliverable name | phase-20-27-game-systems.md:72 | Must be CollectionTracker.cs (not CollectionSystem.cs) |
| TreatWarningsAsErrors | Directory.Build.props | All code must compile warning-free |
| RESTART pattern | SHOP_COLLECTION.ERB:78 | while(true) loop (precedent: ShopSystem.Schedule()) |
| SAVESTR:10 "/" delimiter format | SHOP_COLLECTION.ERB:93-98 | String splitting/joining must preserve exact format |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| SetMoney interface extension ripples to implementations | MEDIUM | LOW | Limited implementors; update all in same feature |
| PANTSNAME/PHOTO_NAME scope expansion | MEDIUM | MEDIUM | Use NotImplementedException stubs; defer full migration |
| Column alignment mismatch (full-width character counting) | MEDIUM | LOW | Document as known limitation or add width-aware utility |
| SAVESTR:10 string encoding edge cases | LOW | MEDIUM | Test empty string, single item, and duplicate entries |
| Double photo command encoding complexity | LOW | MEDIUM | Unit test ID calculation thoroughly |
| ShopSystem constructor parameter growth | LOW | LOW | Adding CollectionTracker as constructor param increases DI count to 8. F776/F777 will add 2 more (10 total). Mitigation: F782 (Post-Phase Review) owns ShopSystem refactoring; constructor consolidation tracked there |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Era.Core build | `dotnet build Era.Core/` | 0 errors, 0 warnings | Must maintain |
| Era.Core.Tests pass | `dotnet test Era.Core.Tests/` | All pass | Must maintain |
| ShowCollection stub exists | `Grep("NotImplementedException", "Era.Core/Shop/ShopSystem.cs")` | 1 match (ShowCollection) | Must become 0 |

**Baseline File**: `.tmp/baseline-775.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | ShowCollection() stub must be replaced | ShopSystem.cs:346-347 | not_matches NotImplementedException in ShopSystem.ShowCollection |
| C2 | MONEY must be mutated via SetMoney/AddMoney | SHOP_COLLECTION.ERB:74 | Verify SetMoney or AddMoney call exists in CollectionTracker |
| C3 | EXP:ポルノ経験 write uses index 113 | EXP.yaml:161 | Verify ExpIndex(113) or named constant in code |
| C4 | SAVESTR:10 "/" delimiter format preserved | SHOP_COLLECTION.ERB:93-98 | Verify string manipulation equivalence in unit tests |
| C5 | Risk check consumption formula preserved | SHOP_COLLECTION.ERB:345-351 | Verify (販売リスク/10) + RAND conditional logic in SellCollectionRiskCheck (see C13 for risk generation) |
| C6 | 7 ERB functions have C# equivalents | SHOP_COLLECTION.ERB (all 7 functions) | Verify all function signatures exist in CollectionTracker |
| C7 | PrintLineC renamed to PrintColumnLeft in ShopDisplay | F788 Mandatory Handoff Row 3 | not_matches PrintLineC in ShopDisplay.cs |
| C8 | Zero TODO/FIXME/HACK in new code | Phase 20 debt-zero requirement | not_matches TODO/FIXME/HACK in new files |
| C9 | Photo TYPE constants match DIM.ERH:219-249; DETAIL constants limited to those used in pricing logic (0-4) | DIM.ERH:219-249, 253-255 | Verify PhotoConstants class has all photo TYPE values (Single/NTR/Double) and pricing-relevant DETAIL values (None/Begging/Pleasure/Orgasm/Lover). Prostitution-specific details (20-26, DIM.ERH:256-262) excluded — created/used by photo-taking code, not SHOP_COLLECTION |
| C10 | while(true) loop for RESTART pattern | SHOP_COLLECTION.ERB:78 | Loop structure matches RESTART semantics |
| C11 | IEngineVariables.SetMoney/AddMoney added | Interface dependency scan | New method exists on IEngineVariables interface |
| C12 | Phase 20 Sub-Feature Requirements | architecture.md:90-97 | Debt cleanup task, equivalence tests, zero debt AC |
| C13 | Risk generation formulas differ by photo category (risk uses totalCopies=収集品枚数, not duplicates=収集品枚数-1) | SHOP_COLLECTION.ERB:199-211,259-264 | Cheating=RAND-based 3-tier for Kiss/Caress/BreastCaress(totalCopies), Fellatio/SixtyNine/Intercrural(totalCopies*2), VaginalSex/AnalSex(totalCopies*4); Contact/Embrace/CASEELSE=zero risk. Prostitute=deterministic totalCopies*10. Cheating pricing: 50/60/70+orgasm 3x, Contact/Embrace=10+orgasm 3x, CASEELSE=10 NO orgasm |
| C14 | Photo detail iteration ranges differ by function (FOR upper bound exclusive) | SHOP_COLLECTION.ERB:135,190,251 | Single=0-19, Cheating=0-29, Prostitute=0-29; verify in equivalence tests |
| C15 | Photo category separation requires CONTINUE filters | SHOP_COLLECTION.ERB:132-134,187-189 | Single skips NTR range (9-16,18-21), Cheating skips Masturbation (17) |
| C16 | Double photo type iteration range 100-108 (FOR exclusive upper bound) | SHOP_COLLECTION.ERB:299 | FOR 写真_住人同士_会話(100), 写真_住人同士_Ａ性交(109) exclusive = 100-108. Double_AnalSex(109) excluded; SELECTCASE entry for 109 is dead code |
| C17 | RESTART re-initializes all #DIM locals including 販売リスク | SHOP_COLLECTION.ERB:5-10,35-36,78 | ERB lines 35-36 reset 販売枚数/販売価格 but NOT 販売リスク. RESTART handles risk reset. C# while(true) must declare risk inside loop body or reset to 0 explicitly |
| C18 | ShowCollection SELECTCASE has no CASEELSE | SHOP_COLLECTION.ERB:37-71 | Invalid commands fall through silently to RESTART. C# must NOT throw on unmatched commands |
| C19 | Double photo dual-key pattern ("と"/"が" particles) | SHOP_COLLECTION.ERB:300-302,306-307 | ShowCollectionPhotoDouble constructs TWO key strings per photo: 収集品文字列Ａ uses "と" (CALLNAME:char1+"と"+CALLNAME:char2+photoId+"PH/"), 収集品文字列Ｂ uses "が" (same with "が"). STRCOUNT sums both. Dedup removes both, re-adds only "と" variant |

### Constraint Details

**C1: ShowCollection Stub Replacement**
- **Source**: F774 created NotImplementedException stub at ShopSystem.cs:346-347
- **Verification**: Grep for NotImplementedException in ShopSystem.ShowCollection method
- **AC Impact**: Must verify stub is removed and replaced with delegation to CollectionTracker

**C2: MONEY Mutation via Interface**
- **Source**: SHOP_COLLECTION.ERB:74 uses `MONEY += value` pattern for collection sales
- **Verification**: Check IEngineVariables has SetMoney or AddMoney method
- **AC Impact**: Must verify MONEY is mutated through interface method, not direct field access

**C3: EXP Index 113 for ポルノ経験**
- **Source**: EXP.yaml:161 defines index 113 as ポルノ経験
- **Verification**: Grep for 113 or named constant referencing ポルノ経験
- **AC Impact**: Must verify correct EXP index is used when writing experience values

**C4: SAVESTR:10 Delimiter Preservation**
- **Source**: SHOP_COLLECTION.ERB:93-98 uses "/" as delimiter in SAVESTR:10 for collection tracking
- **Verification**: Unit test with split/join operations on "/" delimited strings
- **AC Impact**: Must verify string manipulation produces identical results to ERB STRCOUNT/REPLACE

**C5: Risk Check Formula**
- **Source**: SHOP_COLLECTION.ERB:345-351 implements `ポルノ経験上昇量 = (販売リスク / 10)` then `IF (販売リスク % 10) < RAND(10) → ポルノ経験上昇量++` (integer division + modulo probabilistic rounding)
- **Verification**: Unit test with deterministic random seed
- **AC Impact**: Must verify probabilistic formula is preserved exactly (division by 10 + modulo remainder < RAND(10))

**C6: All 7 Functions Migrated**
- **Source**: SHOP_COLLECTION.ERB defines 7 functions (SHOW_COLLECTION, ShowCollection_Panties, ShowCollection_PhotoSingle, ShowCollection_PhotoCheating, ShowCollection_PhotoProstitute, ShowCollection_PhotoDouble, SellCollection_RiskCheck)
- **Verification**: Grep for method signatures in CollectionTracker.cs
- **AC Impact**: Must verify all 7 function equivalents exist

**C7: PrintLineC Rename (F788 Handoff)**
- **Source**: F788 Mandatory Handoffs Row 3 requires renaming PrintLineC to PrintColumnLeft
- **Verification**: Grep for PrintLineC in ShopDisplay.cs (must not exist)
- **AC Impact**: Must verify rename is complete; no references to old name remain

**C8: Zero Technical Debt**
- **Source**: Phase 20 Sub-Feature Requirements (architecture.md:90-97)
- **Verification**: Grep for TODO/FIXME/HACK in new files
- **AC Impact**: Must include AC verifying zero debt markers in all new code

**C9: Photo Constants from DIM.ERH**
- **Source**: DIM.ERH:219-262 defines photo-related constants
- **Verification**: Unit test comparing constant values against DIM.ERH definitions
- **AC Impact**: Must verify constants are accurate, not approximated

**C10: RESTART Loop Pattern**
- **Source**: SHOP_COLLECTION.ERB:78 uses RESTART for loop continuation
- **Verification**: Code review for while(true) with break pattern
- **AC Impact**: Must verify loop follows established ShopSystem.Schedule() precedent

**C11: IEngineVariables Interface Extension**
- **Source**: Interface dependency scan found no SetMoney/AddMoney on IEngineVariables
- **Verification**: Grep for SetMoney or AddMoney in IEngineVariables.cs
- **AC Impact**: Must verify new method is added to interface and all implementors

**C12: Phase 20 Sub-Feature Compliance**
- **Source**: architecture.md:90-97 defines mandatory requirements for all Phase 20 sub-features
- **Verification**: Check debt cleanup task exists, equivalence tests exist, zero debt AC exists
- **AC Impact**: Must include tasks for debt cleanup, equivalence tests; must include AC for zero debt

**C19: Double Photo Dual-Key Pattern**
- **Source**: SHOP_COLLECTION.ERB:300-302,306-307
- **Verification**: ShowCollectionPhotoDouble must construct TWO key strings per photo using different particles ("と" and "が"). STRCOUNT sums both keys. Dedup removes both variants, re-adds only the "と" variant.
- **AC Impact**: Must verify dual-key construction and summation in CollectionTracker. Equivalence tests must cover dual-key counting and dedup consolidation.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Predecessor | F774 | [DONE] | Shop Core -- SHOP.ERB @USERSHOP calls SHOW_COLLECTION (this feature's entry point) |
| Predecessor | F788 | [DONE] | IConsoleOutput Extensions (DRAWLINE etc.) + PrintLineC rename handoff |
| Related | F789 | [DONE] | IStringVariables for SAVESTR:10 access |
| Related | F790 | [DONE] | IEngineVariables (read-only; needs SetMoney extension in this feature) |
| Related | F791 | [DONE] | IGameState mode transitions (not used by SHOP_COLLECTION) |
| Related | F776 | [DONE] | Items (SHOP_ITEM.ERB) -- sibling Phase 20, zero cross-calls |
| Related | F777 | [DONE] | Customization (SHOP_CUSTOM.ERB) -- sibling Phase 20, zero cross-calls |
| Successor | F782 | [DRAFT] | Post-Phase Review depends on F775 |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Pipeline Continuity - Each phase completion triggers next phase planning" | CollectionTracker.cs must be the architecture deliverable completing the SHOP_COLLECTION migration unit | AC#1, AC#2, AC#7 |
| "SSOT: designs/full-csharp-architecture.md Phase 20 section defines the scope" | All 7 ERB functions must have C# equivalents in CollectionTracker | AC#7 |
| "Phase 20: Equipment & Shop Systems philosophy inherited" | Zero technical debt per Sub-Feature Requirements; equivalence tests included | AC#14, AC#15, AC#16 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CollectionTracker.cs exists | file | Glob(Era.Core/Shop/CollectionTracker.cs) | exists | - | [x] |
| 2 | CollectionTrackerTests.cs exists | file | Glob(Era.Core.Tests/Shop/CollectionTrackerTests.cs) | exists | - | [x] |
| 3 | ShowCollection stub removed (Neg) | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | NotImplementedException.*SHOW_COLLECTION | [x] |
| 4 | ShowCollection delegates to CollectionTracker (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | CollectionTracker | [x] |
| 5 | ShopSystem injects CollectionTracker (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | CollectionTracker.*collectionTracker | [x] |
| 6 | IEngineVariables declares SetMoney | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | void SetMoney | [x] |
| 7 | CollectionTracker has all 7 method equivalents | code | Grep(Era.Core/Shop/CollectionTracker.cs) | count_equals | public.*(ShowCollection\|ShowCollectionPanties\|ShowCollectionPhotoSingle\|ShowCollectionPhotoCheating\|ShowCollectionPhotoProstitute\|ShowCollectionPhotoDouble\|SellCollectionRiskCheck)\( = 7 | [x] |
| 8 | IEngineVariables existing methods preserved | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | count_equals | (GetResult\|GetMoney\|GetDay\|GetMaster\|GetAssi\|GetCount\|GetCharaNum\|GetRandom\|GetName\|GetCallName\|GetIsAssi)\( = 11 | [x] |
| 9 | ExpIndex.PornExperience well-known constant | code | Grep(Era.Core/Types/ExpIndex.cs) | matches | PornExperience.*new.*113 | [x] |
| 10 | Risk check uses integer division by 10 (Pos) | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | / 10 | [x] |
| 11 | Risk check uses modulo 10 probabilistic rounding (Pos) | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | % 10 < | [x] |
| 12 | CollectionTracker calls SetMoney | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | SetMoney | [x] |
| 13 | CollectionTracker calls SetExp with PornExperience | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | SetExp.*PornExperience\|PornExperience.*SetExp | [x] |
| 14 | Zero technical debt in CollectionTracker | code | Grep(Era.Core/Shop/CollectionTracker.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 15 | Zero technical debt in CollectionTrackerTests | code | Grep(Era.Core.Tests/Shop/CollectionTrackerTests.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 16 | Unit tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 17 | PrintLineC renamed in ShopDisplay (Neg) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | not_matches | PrintLineC | [x] |
| 18 | ShopDisplay uses PrintColumnLeft directly (Pos) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | matches | PrintColumnLeft | [x] |
| 19 | SAVESTR:10 delimiter "/" in string building | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | \+ "/" | [x] |
| 20 | CollectionTracker uses GetSaveStr | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | GetSaveStr | [x] |
| 21 | CollectionTracker uses SetSaveStr | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | SetSaveStr | [x] |
| 22 | while(true) loop for RESTART pattern (Pos) | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | while.*true | [x] |
| 23 | Era.Core build succeeds | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 24 | CollectionTracker calls GetExp (read-before-write) | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | GetExp | [x] |
| 25 | CollectionTracker calls GetMoney (read-before-write) | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | GetMoney | [x] |
| 26 | PhotoConstants has all DIM.ERH photo type values | code | Grep(Era.Core/Shop/PhotoConstants.cs) | matches | NTR_TakeHome | [x] |
| 27 | Command routing uses SELECTCASE encoding constants | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | % 100 | [x] |
| 28 | SellCollectionRiskCheck has zero-risk guard | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | risk.*==.*0\|販売リスク.*==.*0 | [x] |
| 29 | Double photo command uses % 10000 encoding | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | % 10000 | [x] |
| 30 | Cheating risk accumulation uses GetRandom | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | GetRandom | [x] |
| 31 | Orgasm price multiplier uses Detail_Orgasm | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | Detail_Orgasm | [x] |
| 32 | SellCollectionRiskCheck guards SetExp behind increment > 0 | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | increase.*>.*0\|Increase.*>.*0 | [x] |
| 33 | Prostitute risk uses deterministic count * 10 | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | \* 10\) | [x] |
| 34 | Double photo dual-key uses "と" and "が" particles | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | と.*が\|が.*と | [x] |
| 35 | CollectionTracker registered in DI container | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | CollectionTracker | [x] |

### AC Details

**AC#1: CollectionTracker.cs exists**
- **Test**: Glob pattern="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: File exists
- **Rationale**: Architecture deliverable per phase-20-27-game-systems.md:72

**AC#2: CollectionTrackerTests.cs exists**
- **Test**: Glob pattern="Era.Core.Tests/Shop/CollectionTrackerTests.cs"
- **Expected**: File exists
- **Rationale**: Equivalence tests required per Phase 20 Sub-Feature Requirements (C12)

**AC#3: ShowCollection stub removed (Neg)**
- **Test**: Grep pattern="NotImplementedException.*SHOW_COLLECTION" path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: 0 matches (pattern not found)
- **Rationale**: Stub replacement triple AC part 1 -- verify exception is removed (C1)

**AC#4: ShowCollection delegates to CollectionTracker (Pos)**
- **Test**: Grep pattern="CollectionTracker" path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: At least 1 match (field declaration or method call)
- **Rationale**: Stub replacement triple AC part 2 -- verify delegation exists, prevents hardcoded return (C1)

**AC#5: ShopSystem injects CollectionTracker (Pos)**
- **Test**: Grep pattern="CollectionTracker.*collectionTracker" path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: At least 1 match (constructor parameter or field assignment)
- **Rationale**: Stub replacement triple AC part 3 -- verify DI injection, prevents manual instantiation (C1)

**AC#6: IEngineVariables declares SetMoney**
- **Test**: Grep pattern="void SetMoney" path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1 match
- **Rationale**: MONEY mutation requires setter method on IEngineVariables (C2, C11)

**AC#7: CollectionTracker has all 7 method equivalents**
- **Test**: Grep pattern="public.*(ShowCollection|ShowCollectionPanties|ShowCollectionPhotoSingle|ShowCollectionPhotoCheating|ShowCollectionPhotoProstitute|ShowCollectionPhotoDouble|SellCollectionRiskCheck)\(" path="Era.Core/Shop/CollectionTracker.cs" | count
- **Expected**: 7 matches (one public method declaration per ERB function)
- **Rationale**: All 7 SHOP_COLLECTION.ERB functions must have C# equivalents (C6)
- ERB-to-C# mapping:
  - @SHOW_COLLECTION -> ShowCollection
  - @ShowCollection_Panties -> ShowCollectionPanties
  - @ShowCollection_PhotoSingle -> ShowCollectionPhotoSingle
  - @ShowCollection_PhotoCheating -> ShowCollectionPhotoCheating
  - @ShowCollection_PhotoProstitute -> ShowCollectionPhotoProstitute
  - @ShowCollection_PhotoDouble -> ShowCollectionPhotoDouble
  - @SellCollection_RiskCheck -> SellCollectionRiskCheck

**AC#8: IEngineVariables existing methods preserved**
- **Test**: Grep pattern="(GetResult|GetMoney|GetDay|GetMaster|GetAssi|GetCount|GetCharaNum|GetRandom|GetName|GetCallName|GetIsAssi)\(" path="Era.Core/Interfaces/IEngineVariables.cs" | count
- **Expected**: 11 matches (all pre-existing methods from F790)
- **Rationale**: Interface extension backward compatibility (ENGINE.md Issue 63)

**AC#9: ExpIndex.PornExperience well-known constant**
- **Test**: Grep pattern="PornExperience.*new.*113" path="Era.Core/Types/ExpIndex.cs"
- **Expected**: 1 match
- **Rationale**: EXP index 113 for ポルノ経験 must be a named constant (C3)

**AC#10: Risk check uses integer division by 10 (Pos)**
- **Test**: Grep pattern="/ 10" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: Risk check formula preservation: ポルノ経験上昇量 = (販売リスク / 10) (C5)

**AC#11: Risk check uses modulo 10 probabilistic rounding (Pos)**
- **Test**: Grep pattern="% 10 <" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match (pattern matches `% 10 <` which is the comparison `risk % 10 < GetRandom(10)`, disambiguated from `% 100` and `% 10000` by the trailing `<`)
- **Rationale**: Risk check formula preservation: IF (販売リスク % 10) < RAND(10) -> increment (C5)

**AC#12: CollectionTracker calls SetMoney**
- **Test**: Grep pattern="SetMoney" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: MONEY mutation via interface method, not direct field access (C2)

**AC#13: CollectionTracker calls SetExp with PornExperience**
- **Test**: Grep pattern="SetExp.*PornExperience|PornExperience.*SetExp" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: EXP:キャラ番号:ポルノ経験 += mutation must use SetExp WITH ExpIndex.PornExperience constant (C3). Pattern verifies co-location to prevent calling SetExp with wrong index.

**AC#14: Zero technical debt in CollectionTracker**
- **Test**: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: 0 matches
- **Rationale**: Phase 20 Sub-Feature zero debt requirement (C8, C12)

**AC#15: Zero technical debt in CollectionTrackerTests**
- **Test**: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Tests/Shop/CollectionTrackerTests.cs"
- **Expected**: 0 matches
- **Rationale**: Phase 20 Sub-Feature zero debt requirement (C8, C12)

**AC#16: Unit tests pass**
- **Test**: dotnet test Era.Core.Tests
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Equivalence tests and unit tests must pass (C12). Tests verify SAVESTR:10 delimiter preservation (C4), risk check formula (C5), photo constant values (C9), double photo iteration boundary (starts at character 1, not 0, per ERB line 20), photo detail ranges (Single=0-19, Cheating/Prostitute=0-29, C14), photo category separation (Single excludes NTR, Cheating excludes Masturbation, C15), per-type pricing tiers (Single=20/40/50/10, Cheating=5-path: 50/60/70+orgasm, Contact/Embrace=10+orgasm, CASEELSE=10 no orgasm, Prostitute=100+CASEELSE 10, Double=20/30/50/70/10), Cheating selective risk (Contact/Embrace/CASEELSE=zero risk, C13), Double photo type range 100-108 excludes 109 (C16), risk uses totalCopies not duplicates (C13), ShowCollection risk reset between iterations (C17: after Cheating sale, next Panties sale must pass risk=0), and invalid command silent-ignore (C18: unmatched command loops back without throwing)

**AC#17: PrintLineC renamed in ShopDisplay (Neg)**
- **Test**: Grep pattern="PrintLineC" path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 0 matches (PrintLineC no longer referenced)
- **Rationale**: F788 Mandatory Handoff requires rename to PrintColumnLeft (C7). Scope: private wrapper method `PrintLineC` (line 458) and all ~20 call sites must be renamed to `PrintColumnLeft`.

**AC#18: ShopDisplay uses PrintColumnLeft directly (Pos)**
- **Test**: Grep pattern="PrintColumnLeft" path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: At least 1 match
- **Rationale**: Positive verification that PrintColumnLeft is used after PrintLineC rename (C7)

**AC#19: SAVESTR:10 delimiter "/" in string building**
- **Test**: Grep pattern='\+ "/"' path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match (string concatenation with "/" delimiter)
- **Rationale**: SAVESTR:10 "/" delimiter format must be preserved in collection key construction (C4)

**AC#20: CollectionTracker uses GetSaveStr**
- **Test**: Grep pattern="GetSaveStr" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: SAVESTR:10 access requires IStringVariables.GetSaveStr (C4)

**AC#21: CollectionTracker uses SetSaveStr**
- **Test**: Grep pattern="SetSaveStr" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: SAVESTR:10 mutation (deduplication via REPLACE) requires SetSaveStr (C4)

**AC#22: while(true) loop for RESTART pattern (Pos)**
- **Test**: Grep pattern="while.*true" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: RESTART at line 78 of SHOP_COLLECTION.ERB must be migrated as while(true) loop (C10)

**AC#23: Era.Core build succeeds**
- **Test**: dotnet build Era.Core/
- **Expected**: Build succeeds with 0 errors, 0 warnings (TreatWarningsAsErrors)
- **Rationale**: All new code must compile warning-free

**AC#24: CollectionTracker calls GetExp (read-before-write)**
- **Test**: Grep pattern="GetExp" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB line 351 does `EXP:キャラ番号:ポルノ経験 += ポルノ経験上昇量` (increment). Read-before-write requires GetExp to read current value before SetExp writes incremented value. Pairs with AC#13 to enforce += semantics.

**AC#25: CollectionTracker calls GetMoney (read-before-write)**
- **Test**: Grep pattern="GetMoney" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB line 74 does `MONEY += 販売価格` (increment). Read-before-write requires GetMoney to read current value before SetMoney writes incremented value. Pairs with AC#12 to enforce += semantics.

**AC#26: PhotoConstants has all DIM.ERH photo type values**
- **Test**: Grep pattern="NTR_TakeHome" path="Era.Core/Shop/PhotoConstants.cs"
- **Expected**: At least 1 match
- **Rationale**: C9 requires photo constants match DIM.ERH exactly. NTR_TakeHome (value 20, 写真_NTR_お持ち帰り) is the most commonly omitted constant (marked 未使用 in DIM.ERH but within iteration range 9-21 of ShowCollectionPhotoCheating). Spot-check verifies completeness.

**AC#27: Command routing uses SELECTCASE encoding constants**
- **Test**: Grep pattern="% 100" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: SHOW_COLLECTION dispatches via command encoding (101-series=panties, 201-series=single, 301-series=cheating, 401-series=prostitute, 90101-series=double). Character index is decoded as `command % 100`. This AC verifies the modulo decoding pattern is implemented.

**AC#28: SellCollectionRiskCheck has zero-risk guard**
- **Test**: Grep pattern="risk.*==.*0|販売リスク.*==.*0" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB line 342 has `IF (販売リスク == 0) RETURN` as early exit when risk is zero. Without this guard, risk=0 would produce baseIncrease=0 but RAND could cause unintended increment. Guards correctness at boundary.

**AC#29: Double photo command uses % 10000 encoding**
- **Test**: Grep pattern="% 10000" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB line 65 uses `(コマンド%10000)/100` to extract char1 index from double photo commands (90000-series). AC#27 covers `% 100` for char2, this AC covers the `% 10000` part of the compound encoding.

**AC#30: Cheating risk accumulation uses GetRandom**
- **Test**: Grep pattern="GetRandom" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB ShowCollectionPhotoCheating (lines 199-211) uses RAND-based risk accumulation (RAND(count), RAND(count*2), RAND(count*4)). This must be migrated as GetRandom calls. Distinct from Prostitute's deterministic `count * 10` formula (C13). Verifies risk generation is randomized where ERB requires it.

**AC#31: Orgasm price multiplier uses Detail_Orgasm**
- **Test**: Grep pattern="Detail_Orgasm" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB cheating/photo pricing uses `(1 + (収集品詳細 == 写真詳細_絶頂) * 2)` for 3x multiplier on orgasm photos. Detail_Orgasm (=3, from DIM.ERH:254) must be referenced in the pricing logic.

**AC#32: SellCollectionRiskCheck guards SetExp behind increment > 0**
- **Test**: Grep pattern="increase.*>.*0|Increase.*>.*0" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB line 349 `IF ポルノ経験上昇量 > 0` guards SetExp and the rumor message. Without this guard, risk values 1-9 where random check fails would produce baseIncrease=0 but still call SetExp (writing current + 0) and print spurious rumor message.

**AC#33: Prostitute risk uses deterministic count * 10**
- **Test**: Grep pattern="\* 10\)" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match
- **Rationale**: ERB ShowCollectionPhotoProstitute (line 262) uses `販売リスク += (収集品枚数 * 10)` — deterministic multiplication, distinct from Cheating's RAND-based accumulation (C13). Pattern matches `* 10)` to catch the multiplication within parentheses.

**AC#34: Double photo dual-key uses "と" and "が" particles**
- **Test**: Grep pattern="と.*が|が.*と" path="Era.Core/Shop/CollectionTracker.cs"
- **Expected**: At least 1 match (both particles present in ShowCollectionPhotoDouble key construction)
- **Rationale**: ERB lines 300-301 construct two key strings per double photo: 収集品文字列Ａ uses "と" particle, 収集品文字列Ｂ uses "が" particle. STRCOUNT sums both (line 302). Dedup removes both variants, re-adds only "と" (lines 306-307). Without both particles, photos stored with one variant would be invisible to the other's count (C19).

**AC#35: CollectionTracker registered in DI container**
- **Test**: Grep pattern="CollectionTracker" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- **Expected**: At least 1 match (DI registration line)
- **Rationale**: ShopSystem constructor injection (AC#5) requires CollectionTracker to be resolved from the DI container. Without registration in ServiceCollectionExtensions.cs, runtime would fail with DI resolution exception. Unit tests use direct construction, so AC#16 would not catch a missing registration.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Create CollectionTracker.cs migrating all 7 functions | AC#1, AC#7, AC#27, AC#29 |
| 2 | Replace ShowCollection() stub with delegation | AC#3, AC#4, AC#5, AC#35 |
| 3 | Add SetMoney to IEngineVariables | AC#6, AC#8, AC#12, AC#25 |
| 4 | Rename PrintLineC to PrintColumnLeft (F788 Handoff) | AC#17, AC#18 |
| 5 | Preserve SAVESTR:10 delimiter, risk check formula, EXP index 113 | AC#9, AC#10, AC#11, AC#13, AC#19, AC#20, AC#21, AC#24, AC#28, AC#30, AC#31, AC#32, AC#33, AC#34 |
| 6 | Zero technical debt | AC#14, AC#15 |
| 7 | Equivalence tests | AC#2, AC#16 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature migrates SHOP_COLLECTION.ERB to C# by creating a new CollectionTracker class that handles collection gallery UI for panties and photos. The implementation follows the Phase 20 architecture pattern established by F774 (ShopSystem) and uses DI for all dependencies.

**Core Architecture**:
1. **CollectionTracker.cs** (new file): Main class with 7 public methods corresponding to ERB functions
2. **IEngineVariables extension**: Add `void SetMoney(int value)` to support MONEY mutation
3. **ShopSystem integration**: Replace stub with delegation to `_collectionTracker.ShowCollection()`
4. **External function stubs**: PANTSNAME, PHOTO_NAME stub with NotImplementedException (tracked to new DRAFT feature for CLOTHES.ERB/COMF446.ERB migration)
5. **Photo constants**: Create Era.Core/Shop/PhotoConstants.cs (standalone static class for DIM.ERH constants)
6. **ExpIndex extension**: Add well-known constant `PornExperience = new(113)`
7. **ShopDisplay rename**: PrintLineC → PrintColumnLeft (F788 handoff)

**String Manipulation Strategy**:
- SAVESTR:10 uses "/" as delimiter for collection keys (e.g., "レミリア0/" for panty #0)
- STRCOUNT → `string.Split("/").Count(s => s == searchKey)`
- REPLACE → `string.Replace(oldValue, newValue)` with deduplication logic

**Multi-value Return Pattern**:
- ERB `RESULT:0/1/2` → C# tuples `(int count, int price)` or `(int count, int price, int risk)`
- SellCollectionRiskCheck returns void (only side effects: SetExp and rumor PRINT). SetMoney is called in ShowCollection after sale confirmation (ERB line 74), not in SellCollectionRiskCheck.

**Risk Check Formula Preservation** (C5):
```csharp
int baseIncrease = risk / 10;           // Integer division
if (risk % 10 < _engineVars.GetRandom(10))
    baseIncrease++;                      // Probabilistic rounding
```

**RESTART Loop Pattern** (C10):
```csharp
public void ShowCollection()
{
    while (true)
    {
        // Display collections + input
        if (input == 0) return;
        // Process sale
    }
}
```

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create Era.Core/Shop/CollectionTracker.cs with namespace Era.Core.Shop |
| 2 | Create Era.Core.Tests/Shop/CollectionTrackerTests.cs with unit tests |
| 3 | Remove `throw new NotImplementedException(...)` from ShopSystem.ShowCollection() |
| 4 | Add `_collectionTracker.ShowCollection()` call in ShopSystem.ShowCollection() |
| 5 | Add `CollectionTracker _collectionTracker` field + constructor parameter in ShopSystem |
| 6 | Add `void SetMoney(int value);` to IEngineVariables interface |
| 7 | Define 7 public methods in CollectionTracker: ShowCollection, ShowCollectionPanties, ShowCollectionPhotoSingle, ShowCollectionPhotoCheating, ShowCollectionPhotoProstitute, ShowCollectionPhotoDouble, SellCollectionRiskCheck |
| 8 | Verify all 11 existing GetXxx methods remain in IEngineVariables after SetMoney addition |
| 9 | Add `public static readonly ExpIndex PornExperience = new(113);` to ExpIndex.cs |
| 10 | Implement `int baseIncrease = risk / 10;` in SellCollectionRiskCheck |
| 11 | Implement `if (risk % 10 < random) baseIncrease++;` in SellCollectionRiskCheck |
| 12 | Call `_engineVars.SetMoney(_engineVars.GetMoney() + totalPrice);` after sales |
| 13 | Call `_variables.SetExp(characterId, ExpIndex.PornExperience, currentExp + increase);` ensuring SetExp and PornExperience co-locate |
| 14 | Write clean code with no TODO/FIXME/HACK comments in CollectionTracker.cs |
| 15 | Write clean code with no TODO/FIXME/HACK comments in CollectionTrackerTests.cs |
| 16 | Write comprehensive unit tests covering: (1) SAVESTR:10 delimiter parsing, (2) Risk formula, (3) Deduplication logic, (4) Photo constant accuracy |
| 17 | Rename all occurrences of `PrintLineC` to `PrintColumnLeft` in ShopDisplay.cs |
| 18 | Verify `_console.PrintColumnLeft(...)` is called in ShopDisplay (already done as part of AC#17) |
| 19 | Use `+ "/"` in SAVESTR:10 key construction: `collectionKey = callName + itemId + "/"` |
| 20 | Call `_stringVariables.GetSaveStr(new SaveStrIndex(10))` to read SAVESTR:10 |
| 21 | Call `_stringVariables.SetSaveStr(new SaveStrIndex(10), newValue)` after deduplication |
| 22 | Implement `while (true)` loop in ShowCollection with `if (input == 0) return;` break |
| 23 | Ensure all code compiles with TreatWarningsAsErrors=true (dotnet build Era.Core/) |
| 24 | Call `_variables.GetExp(characterId, ExpIndex.PornExperience)` to read current EXP before incrementing via SetExp |
| 25 | Call `_engineVars.GetMoney()` to read current MONEY before incrementing via SetMoney |
| 26 | Include NTR_TakeHome (value 20) in PhotoConstants, verifiable as spot-check for DIM.ERH completeness (C9) |
| 27 | Implement command decoding with `command % 100` for character index extraction in ShowCollection dispatch |
| 28 | Add `if (risk == 0) return;` guard at start of SellCollectionRiskCheck |
| 29 | Implement double photo command decoding with `(command % 10000) / 100` for char1 index |
| 30 | Use `_engineVars.GetRandom(count)` for Cheating risk accumulation (RAND-based 3-tier) |
| 31 | Reference `PhotoConstants.Detail_Orgasm` in pricing formula for 3x orgasm multiplier |
| 32 | Add `if (increase > 0)` guard before SetExp and rumor message in SellCollectionRiskCheck |
| 33 | Implement Prostitute risk accumulation as deterministic `count * 10` (no GetRandom), distinct from Cheating |
| 34 | Implement dual-key construction in ShowCollectionPhotoDouble: key A uses "と" particle, key B uses "が" particle. STRCOUNT sums both. Dedup removes both, re-adds only "と" variant |
| 35 | Register `CollectionTracker` in `ServiceCollectionExtensions.cs` DI container (e.g., `services.AddSingleton<CollectionTracker>()`) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Photo constants location | (A) Inline magic numbers, (B) Enum PhotoType, (C) Static class PhotoConstants | C: Static class | Matches DIM.ERH structure; allows grouping by category (Single/NTR/Double); enum forces sequential values but DIM.ERH has gaps (17→18, 21→100) |
| Multi-value return | (A) Tuple (int, int), (B) Record struct, (C) Multiple out params | A: Tuple | Simple, idiomatic C# for 2-3 related values; record struct overhead not justified |
| SetMoney vs AddMoney | (A) SetMoney(int), (B) AddMoney(int) | A: SetMoney | Matches ERB `MONEY += value` semantics via `SetMoney(GetMoney() + value)`; explicit state mutation |
| External function stubs | (A) Inline placeholder strings, (B) NotImplementedException | B: NotImplementedException | Follows F774 pattern; forces explicit dependency resolution in F776/F777 |
| STRCOUNT implementation | (A) Regex.Matches().Count, (B) string.Split("/").Count, (C) Manual loop | B: Split + LINQ Count | Readable, efficient for "/" delimiter; Regex overhead not needed for literal match |
| NTR_NAME(0) handling | (A) NotImplementedException, (B) Inline "訪問者" | B: Inline | ERB always calls with arg 0; return value is predictable constant "訪問者" per eraTW reference |
| IEngineVariables mixed contract | (A) Document mixed read/write, (B) ISP split (Reader/Writer) | A: Document mixed | F790's read-only was provisional (no Phase 20 consumer needed writes at design time). F775 (SetMoney), F776 (SetMoney + 6 item methods), F777 (9 setters: SetName/SetCallName/SetMaster/SetTarget/SetPlayer/GetNo/SetNo) all extend the interface. ISP split deferred to F782 (Post-Phase Review) when all consumer patterns are known |

### Interfaces / Data Structures

**IEngineVariables Extension** (new method):
```csharp
public interface IEngineVariables
{
    // ... existing 11 methods ...

    /// <summary>Set MONEY value (player money, stored in MONEY:0)</summary>
    void SetMoney(int value);
}
```

**PhotoConstants Static Class** (new file):
```csharp
namespace Era.Core.Shop;

public static class PhotoConstants
{
    // Single photo types
    public const int Daily = 1;
    public const int Changing = 2;
    public const int Bathing = 3;
    public const int Sleeping = 4;
    public const int Cooking = 5;
    public const int Napping = 6;
    public const int Reading = 7;
    public const int Toilet = 8;
    public const int Masturbation = 17;

    // NTR photo types
    public const int NTR_Kiss = 9;
    public const int NTR_Caress = 10;
    public const int NTR_BreastCaress = 11;
    public const int NTR_Fellatio = 12;
    public const int NTR_SixtyNine = 13;
    public const int NTR_Intercrural = 14;
    public const int NTR_VaginalSex = 15;
    public const int NTR_AnalSex = 16;
    public const int NTR_Contact = 18;
    public const int NTR_Embrace = 19;
    public const int NTR_TakeHome = 20;    // 写真_NTR_お持ち帰り (DIM.ERH:238, marked 未使用 but in iteration range)
    public const int NTR_Prostitution = 21;

    // Double photo types
    public const int Double_Conversation = 100;
    public const int Double_Tea = 101;
    public const int Double_Contact = 102;
    public const int Double_Embrace = 103;
    public const int Double_Kiss = 104;
    public const int Double_Caress = 105;
    public const int Double_AnalCaress = 106;
    public const int Double_BreastCaress = 107;
    public const int Double_VaginalSex = 108;
    public const int Double_AnalSex = 109;

    // Photo detail types
    public const int Detail_None = 0;
    public const int Detail_Begging = 1;
    public const int Detail_Pleasure = 2;
    public const int Detail_Orgasm = 3;
    public const int Detail_Lover = 4;
}
```

**CollectionTracker Method Signatures**:
```csharp
public class CollectionTracker
{
    private readonly IConsoleOutput _console;
    private readonly IStringVariables _stringVariables;
    private readonly IVariableStore _variables;
    private readonly IEngineVariables _engineVars;
    private readonly IInputHandler _inputHandler;

    public CollectionTracker(IConsoleOutput console, IStringVariables stringVariables,
        IVariableStore variables, IEngineVariables engineVars, IInputHandler inputHandler)
    {
        // DI initialization
    }

    public void ShowCollection();                                                // @SHOW_COLLECTION
    public (int count, int price) ShowCollectionPanties(int charIndex, bool dedupe = false);
    public (int count, int price) ShowCollectionPhotoSingle(int charIndex, bool dedupe = false);
    public (int count, int price, int risk) ShowCollectionPhotoCheating(int charIndex, bool dedupe = false);
    public (int count, int price, int risk) ShowCollectionPhotoProstitute(int charIndex, bool dedupe = false);
    public (int count, int price) ShowCollectionPhotoDouble(int char1Index, int char2Index, bool dedupe = false);
    public void SellCollectionRiskCheck(int charIndex, int risk);                // @SellCollection_RiskCheck
}
```

**Unit Test Coverage Areas**:
1. **SAVESTR:10 parsing**: Test key construction "レミリア0/" and STRCOUNT equivalence
2. **Risk formula**: Test `risk=15 → base=1, prob=40%` (remainder 5, P(5 < RAND(10)) = 4/10), `risk=20 → base=2, prob=0%`
3. **Deduplication**: Test REPLACE logic removes duplicates, keeps 1 instance
4. **Photo constants**: Verify PhotoConstants values match DIM.ERH:219-262
5. **Price calculation**: Test conditional pricing (orgasm photos = 3x base price)
6. **Multi-value return**: Test tuple unpacking matches ERB RESULT:0/1/2

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| NTR_NAME(0) inline vs stub | AC Design Constraints | Add C13: NTR_NAME(0) always returns "訪問者" (verified from eraTW reference). Implementation can inline this constant without NotImplementedException stub. |

### Scope Reference

#### Source Files

| File | Lines | Functions | Note |
|------|------:|-----------|------|
| Game/ERB/SHOP_COLLECTION.ERB | 353 | 7 | @SHOW_COLLECTION, @ShowCollection_Panties, @ShowCollection_PhotoSingle, @ShowCollection_PhotoCheating, @ShowCollection_PhotoProstitute, @ShowCollection_PhotoDouble, @SellCollection_RiskCheck |

#### Affected Files

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Shop/CollectionTracker.cs | CREATE | New class (architecture deliverable): 7 migrated methods |
| Era.Core/Shop/PhotoConstants.cs | CREATE | Static class with DIM.ERH photo type/detail constants (standalone, not nested in CollectionTracker) |
| Era.Core/Shop/ShopSystem.cs | MODIFY | Replace ShowCollection() stub with delegation to CollectionTracker |
| Era.Core/Shop/ShopDisplay.cs | MODIFY | Rename PrintLineC to PrintColumnLeft (F788 handoff) |
| Era.Core/Interfaces/IEngineVariables.cs | MODIFY | Add SetMoney(int) or AddMoney(int) |
| Era.Core/Types/ExpIndex.cs | MODIFY | Add well-known constant for ポルノ経験 (index 113) |
| Era.Core.Tests/Shop/CollectionTrackerTests.cs | CREATE | Unit tests and equivalence tests |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | MODIFY | Register CollectionTracker in DI container |
| Engine mock implementations | MODIFY | Add SetMoney to mock IEngineVariables |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2 | Create CollectionTracker.cs and CollectionTrackerTests.cs skeleton files | | [x] |
| 2 | 9 | Add ExpIndex.PornExperience constant (value 113) | | [x] |
| 3 | 6, 8 | Add SetMoney method to IEngineVariables interface and update all implementors | | [x] |
| 4 | 17, 18 | Rename ShopDisplay.PrintLineC to PrintColumnLeft (F788 handoff) | | [x] |
| 5 | 26, 31 | Create PhotoConstants static class with DIM.ERH values (single/NTR/double/detail types) | | [x] |
| 6 | 7, 19, 20, 21 | Implement SAVESTR:10 string manipulation (GetSaveStr/SetSaveStr, "/" delimiter, STRCOUNT/REPLACE equivalents) | | [x] |
| 7 | 7, 12, 22, 25, 27, 29 | Implement ShowCollection main loop with command routing (while(true), SELECTCASE dispatch, % 100/% 10000 decoding, MONEY mutation via SetMoney after sale confirmation) | | [x] |
| 8 | 7, 24, 25 | Implement ShowCollectionPanties (panty iteration, quadratic pricing, deduplication) | | [x] |
| 9 | 7, 30, 33, 34 | Implement ShowCollectionPhoto* methods (Single/Cheating/Prostitute/Double with risk accumulation and dual-key "と"/"が" pattern) | | [x] |
| 10 | 7, 10, 11, 13, 28, 32 | Implement SellCollectionRiskCheck (zero-risk guard, increment>0 guard, division/modulo formula, SetExp+PornExperience) | | [x] |
| 11 | 3, 4, 5, 35 | Replace ShopSystem.ShowCollection() stub with delegation to CollectionTracker + DI registration | | [x] |
| 12 | 14, 15 | Cleanup technical debt markers (TODO/FIXME/HACK) in all new files | | [x] |
| 13 | 16 | Write equivalence tests per-function: (ShowCollection: command routing dispatch), (ShowCollectionPanties: quadratic pricing cumulative 重複枚数*100 e.g. item A 3copies→重複枚数=2 price=200 then item B 2copies→重複枚数=3 price=300 total=500, dedup), (ShowCollectionPhotoSingle: 4-tier pricing 20/40/50/10, detail range 0-19), (ShowCollectionPhotoCheating: 5-path pricing 50/60/70+orgasm 3x, Contact/Embrace=10+orgasm 3x, CASEELSE=10 no orgasm; RAND 3-tier risk using totalCopies, Contact/Embrace/CASEELSE=zero risk), (ShowCollectionPhotoProstitute: flat 100 + CASEELSE 10, deterministic totalCopies*10 risk), (ShowCollectionPhotoDouble: 5-tier pricing 20/30/50/70/10, type range 100-108 excludes 109, char index starts at 1, dual-key "と"/"が" counting and dedup consolidation), (SellCollectionRiskCheck: risk=0 guard, division/modulo formula, increment>0 guard). Cross-cutting: SAVESTR:10 parsing, photo constants, deduplication | | [x] |
| 14 | 23 | Verify Era.Core build succeeds with TreatWarningsAsErrors | | [x] |
| 15 | 16 | Verify all unit tests pass | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Orphan Tasks forbidden. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | Sonnet 4.5 | feature-775.md Tasks 1-15 | SUCCESS or BUILD_FAIL |

### Pre-conditions

- All predecessors [DONE]: F647, F774, F788
- Related features [DONE] provide required interfaces: F789 (IStringVariables), F790 (IEngineVariables)
- AC#1-35 are all Type=code/build/file/test (no output/variable ACs requiring --unit tests)

### Execution Order

1. **Task 1**: Create skeleton files (CollectionTracker.cs, CollectionTrackerTests.cs) with namespace declarations and class stubs
2. **Task 2**: Add ExpIndex.PornExperience constant to ExpIndex.cs
3. **Task 3**: Extend IEngineVariables with SetMoney method; update mock implementations in engine.Tests
4. **Task 4**: Rename PrintLineC to PrintColumnLeft in ShopDisplay.cs (find-replace + verify call sites)
5. **Task 5**: Create PhotoConstants static class (matching DIM.ERH:219-262 including NTR_TakeHome=20)
6. **Task 6**: Implement SAVESTR:10 string manipulation (GetSaveStr/SetSaveStr, "/" delimiter, STRCOUNT via Split+Count)
7. **Task 7**: Implement ShowCollection main loop with command routing (while(true), SELECTCASE, % 100/% 10000 decoding)
8. **Task 8**: Implement ShowCollectionPanties (panty iteration 0-98, quadratic pricing, deduplication)
9. **Task 9**: Implement ShowCollectionPhoto* methods (Single/Cheating/Prostitute/Double with distinct risk accumulation formulas and dual-key "と"/"が" pattern for Double)
10. **Task 10**: Implement SellCollectionRiskCheck (zero-risk guard, risk/10 + risk%10 < RAND, SetMoney, SetExp+PornExperience)
11. **Task 11**: Update ShopSystem.ShowCollection() to inject CollectionTracker via constructor and delegate call
12. **Task 12**: Search all new files for TODO/FIXME/HACK markers and remove or resolve
13. **Task 13**: Write CollectionTrackerTests.cs equivalence tests (per-function + cross-cutting):
    - **ShowCollection**: Command routing dispatch (101/201/301/401/90101 series), risk reset between iterations (C17), invalid command silent-ignore (C18)
    - **ShowCollectionPanties**: Quadratic pricing (cumulative 重複枚数*100), deduplication
    - **ShowCollectionPhotoSingle**: 4-tier pricing (20/40/50/10), detail range 0-19
    - **ShowCollectionPhotoCheating**: 5-path pricing (50/60/70+orgasm 3x, Contact/Embrace=10+orgasm 3x, CASEELSE=10 no orgasm), RAND 3-tier risk using totalCopies, Contact/Embrace/CASEELSE=zero risk
    - **ShowCollectionPhotoProstitute**: Flat 100 + CASEELSE 10, deterministic totalCopies*10 risk
    - **ShowCollectionPhotoDouble**: 5-tier pricing (20/30/50/70/10), type range 100-108 (excludes 109), char index starts at 1, dual-key "と"/"が" counting and dedup consolidation
    - **SellCollectionRiskCheck**: Risk=0 guard, risk=15→base=1+40%prob, risk=20→base=2+0%prob, increment>0 guard
    - **Cross-cutting**: SAVESTR:10 parsing (key construction, STRCOUNT equivalence), photo constants (DIM.ERH), deduplication logic
14. **Task 14**: Run dotnet build Era.Core/ and verify 0 errors, 0 warnings
15. **Task 15**: Run dotnet test Era.Core.Tests/ and verify all tests pass

### Build Verification

**Build Command** (via WSL):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
```

**Test Command** (via WSL):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'
```

**ERB Warnings Check** (not required for Type=engine features):
- Skipped (no ERB changes)

### Success Criteria

- All ACs pass (verified by ac-static-verifier.py for code/build/file types; dotnet test for test type)
- dotnet build Era.Core/ succeeds (0 errors, 0 warnings)
- dotnet test Era.Core.Tests/ succeeds (all tests pass)
- Zero technical debt markers (TODO/FIXME/HACK) in new code
- ShopSystem.ShowCollection() stub replaced with delegation
- IEngineVariables.SetMoney method added and used by CollectionTracker

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| PANTSNAME/PHOTO_NAME NotImplementedException stubs | CLOTHES.ERB and COMF446.ERB not in F776/F777 scope | Feature | F795 | N/A (DRAFT created in FL POST-LOOP) |

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
| 2026-02-17 | Phase 1 | initializer | Status [REVIEWED]→[WIP] | READY:775:erb |
| 2026-02-17 | Phase 2 | explorer | Codebase investigation | READY |
| 2026-02-17 | Phase 4 | implementer | Tasks 1-15 implementation | SUCCESS (2038 tests pass) |
| 2026-02-17 | Phase 5 | orchestrator | Test fix (StrCount test dedupe mode) | GREEN confirmed |
| 2026-02-17 | Phase 7 | ac-tester | AC verification all 35 ACs | OK:35/35 |
| 2026-02-17 | DEVIATION | feature-reviewer | Phase 8.1 NEEDS_REVISION: ShowCollectionPhotoCheating loop bound <= should be < (NTR_Prostitution included erroneously) | Fix required |
| 2026-02-17 | Phase 8.1 | implementer | Fix: CollectionTracker.cs:286 <= to < for Cheating loop bound | SUCCESS (2038 tests pass) |
| 2026-02-17 | Phase 8.1 | feature-reviewer | Re-review quality post-fix | READY |
| 2026-02-17 | DEVIATION | feature-reviewer | Phase 8.2 NEEDS_REVISION: engine-dev SKILL.md missing ExpIndex.PornExperience and SetMoney docs | Fix required |
| 2026-02-17 | Phase 8.2 | orchestrator | SSOT fix: engine-dev SKILL.md updated (PornExperience + SetMoney) | Fixed |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2 iter1: Dependencies section | Moved from after Technical Design to between AC Design Constraints and AC (template compliance)
- [fix] Phase2 iter1: AC Definition Table | Added AC#24 (GetExp read-before-write for EXP increment)
- [fix] Phase2 iter1: AC Definition Table | Added AC#25 (GetMoney read-before-write for MONEY increment)
- [fix] Phase2 iter1: AC#16 Details | Added double photo iteration boundary test case (char index 1 not 0)
- [fix] Phase2 iter2: PhotoConstants | Added NTR_TakeHome = 20 (DIM.ERH:238, within iteration range 9-21)
- [fix] Phase2 iter2: AC Definition Table | Added AC#26 (PhotoConstants NTR_TakeHome spot-check for C9)
- [fix] Phase2 iter2: AC Design Constraints C9 | Clarified AC implication to reference PhotoConstants class
- [resolved-applied] Phase2 iter3: [FMT-XXX] AC count 34 exceeds erb type guideline of 8-15 — Volume waiver granted: SHOP_COLLECTION.ERB has 7 functions with distinct formulas (risk, pricing, string manipulation) requiring individual verification. All 34 ACs are non-redundant.
- [fix] Phase2 iter3: AC Definition Table | Added AC#27 (command routing % 100 decoding)
- [fix] Phase2 iter3: AC Definition Table | Added AC#28 (zero-risk guard in SellCollectionRiskCheck)
- [fix] Phase2 iter3: AC#13 | Updated matcher to verify SetExp+PornExperience co-location
- [fix] Phase2 iter4: Success Criteria | Fixed stale AC count "23" → generic "All ACs"
- [fix] Phase2 iter4: Implementation Contract | Removed duplicate bold Success Criteria block
- [resolved-applied] Phase2 iter4: [FMT-XXX] Scope Reference at H2 between Technical Design and Tasks is non-template (loop: same from iter2/3; fix location ambiguous)
- [fix] Phase2 iter4: AC Definition Table | Added AC#29 (% 10000 double photo encoding)
- [fix] Phase2 iter4: AC Definition Table | Added AC#30 (GetRandom for Cheating risk accumulation)
- [fix] Phase2 iter4: AC Design Constraints | Added C13 (risk generation formulas) and updated C5 scope
- [fix] Phase2 iter5: Pre-conditions | Fixed stale AC range "AC#1-23" → "AC#1-30"
- [fix] Phase2 iter6: AC Table | Escaped pipe characters in AC#7, AC#8, AC#13, AC#14, AC#15 regex patterns
- [fix] Phase2 iter6: AC#11 | Changed pattern from `% 10` to `risk % 10|Risk % 10` (anchored to risk variable)
- [fix] Phase2 iter6: AC Definition Table | Added AC#31 (Detail_Orgasm orgasm price multiplier)
- [fix] Phase2 iter6: Task 8 | Added panty quadratic pricing and orgasm 3x multiplier test cases
- [fix] Phase2 iter6: AC Design Constraints | Updated C5 scope description
- [resolved-applied] Phase3 iter7: [DEP-XXX] PANTSNAME (CLOTHES.ERB) and PHOTO_NAME (COMF446.ERB) stubs tracked to F776/F777 but those features don't own those files. → User chose: Create new DRAFT Feature for CLOTHES.ERB/COMF446.ERB migration. Deferred to Step 6.3.
- [resolved-applied] Phase3 iter7: [DES-XXX] SetMoney on IEngineVariables violates read-only design (F790). → User chose: Document mixed contract. F790 read-only was provisional; 3 features (F775/F776/F777) extend with setters. ISP split deferred to F782.
- [fix] Phase3 iter7: Tasks | Split Task 5 (17 ACs) into Tasks 5-10 (PhotoConstants, SAVESTR, ShowCollection loop, Panties, Photo*, RiskCheck). Tasks renumbered 1-15.
- [fix] Phase2 iter8: AC Definition Table | Added AC#32 (increment > 0 guard in SellCollectionRiskCheck, ERB line 349)
- [fix] Phase2 iter9: AC#11 | Changed pattern from `risk % 10` to `% 10 <` (trailing < disambiguates from % 100 and % 10000)
- [fix] Phase2 iter9: Execution Order | Fixed risk=15 probability from 50% to 40% (remainder 5, P(5<RAND(10))=4/10)
- [fix] Phase2 iter9: AC Definition Table | Added AC#33 (Prostitute deterministic count * 10 risk formula, C13 enforcement)
- [fix] Phase2 iter10: Technical Design | Fixed SetMoney misattribution (ShowCollection not SellCollectionRiskCheck)
- [fix] Phase2 iter10: AC Design Constraints | Added C14 (photo detail ranges) and C15 (photo category CONTINUE filters)
- [fix] Phase2 iter10: AC#16 | Added photo detail range and category separation test coverage
- [fix] Phase2 iter11: ## Created | Converted to HTML comment (non-template H2 section)
- [fix] Phase2 iter11: ## Summary | Removed non-template H2 section (content was redundant with Background Goal)
- [fix] Phase2 iter11: C14 | Fixed FOR upper bound notation: Single=0-20→0-19, Cheating=0-30→0-29, Prostitute=0-30→0-29 (ERA FOR exclusive upper bound)
- [fix] Phase2 iter11: Task 8 | Fixed panty iteration range 0-99→0-98 (FOR 0,99 exclusive upper bound)
- [fix] Phase2 iter11: AC#16 | Updated photo detail ranges to match corrected C14 values
- [fix] Phase2 iter12: C13 | Added selective risk detail: Contact/Embrace/CASEELSE=zero risk; 3-tier RAND applies only to specific types
- [fix] Phase2 iter12: Task 13 | Reorganized to per-function equivalence tests + cross-cutting. Added pricing tier tests per collection type
- [fix] Phase2 iter12: AC#16 | Added per-type pricing tiers and Cheating selective risk to test coverage description
- [fix] Phase2 iter13: C13 | Clarified risk uses totalCopies (収集品枚数) not duplicates; added Cheating CASEELSE=10 no orgasm distinct from Contact/Embrace=10+orgasm
- [fix] Phase2 iter13: AC Constraints | Added C16 (Double photo type range 100-108, excludes 109)
- [fix] Phase2 iter13: Task 13 | Updated Cheating to 5-path pricing, Prostitute/Cheating risk to totalCopies, Double type range 100-108
- [fix] Phase2 iter14: C9 | Narrowed scope: Photo TYPE constants (DIM.ERH:219-249) + pricing-relevant DETAIL (0-4). Excluded prostitution details 20-26
- [fix] Phase2 iter14: AC Constraints | Added C17 (RESTART risk re-init) and C18 (no CASEELSE silent-ignore)
- [fix] Phase2 iter14: AC#16 + Task 13 | Added risk reset test (C17) and invalid command test (C18)
- [fix] Phase2 iter15: AC Details | Bolded all field labels (Test/Expected/Rationale) across 33 AC entries
- [fix] Phase2 iter15: Task Tags | Replaced bullet list with template 4-column table + guidance subsections
- [fix] Phase2 iter15: Mandatory Handoffs | Updated HTML comments to current template (TBD violation, DRAFT Creation Checklist)
- [fix] Phase3 iter16: PhotoConstants | Extracted to separate Era.Core/Shop/PhotoConstants.cs (ISP: standalone vs nested in CollectionTracker). Updated AC#26 grep path, Affected Files, Technical Design
- [fix] Phase3 iter16: Risks | Added ShopSystem constructor growth risk (DI count, future dispatcher pattern)
- [fix] Phase2 iter17: Scope Reference | Demoted ## Scope Reference to ### (H3 under Technical Design) + Affected Files to #### (template compliance)
- [fix] Phase2 iter17: Pending note | Updated stale AC count 26→33 in [pending] Phase2 iter3
- [fix] Phase2 iter17: Task 13 | Added explicit cumulative 重複枚数 test case (item A 3copies=200, item B 2copies=300, total=500)
- [fix] Phase2 iter18: AC Constraints | Added C19 (Double photo dual-key "と"/"が" pattern, ERB lines 300-302,306-307)
- [fix] Phase2 iter18: AC Definition Table | Added AC#34 (dual-key "と"/"が" particles in ShowCollectionPhotoDouble)
- [fix] Phase2 iter18: Task 9 | Added AC#34 and dual-key pattern description
- [fix] Phase2 iter18: Task 13 | Added dual-key counting and dedup consolidation to Double photo test spec
- [fix] Phase2 iter18: Goal Coverage | Added AC#34 to Goal 5 (preserve formulas)
- [fix] Phase3 iter19: AC#17 Details | Clarified rename scope: private wrapper + ~20 call sites
- [fix] Phase3 iter19: Risks | Made constructor growth mitigation concrete: F782 owns refactoring (removed "consider if" language)
- [fix] Phase2 iter20: Tasks | Moved AC#12 (SetMoney) and AC#25 (GetMoney) from Task 10 to Task 7 (MONEY mutation is in ShowCollection, not SellCollectionRiskCheck per Technical Design)
- [fix] Phase2 iter20: AC Definition Table | Added AC#35 (CollectionTracker DI registration in ServiceCollectionExtensions.cs)
- [fix] Phase2 iter20: Affected Files | Added ServiceCollectionExtensions.cs to Affected Files table
- [fix] Phase2 iter20: Task 11 | Added AC#35 and DI registration to description

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (parent)
- [Predecessor: F774](feature-774.md) - Shop Core (stub origin)
- [Predecessor: F788](feature-788.md) - IConsoleOutput Extensions (PrintLineC handoff)
- [Related: F789](feature-789.md) - IStringVariables (SAVESTR:10)
- [Related: F790](feature-790.md) - IEngineVariables (SetMoney gap)
- [Related: F791](feature-791.md) - IGameState mode transitions
- [Related: F776](feature-776.md) - Items (sibling Phase 20)
- [Related: F777](feature-777.md) - Customization (sibling Phase 20)
- [Successor: F782](feature-782.md) - Post-Phase Review
