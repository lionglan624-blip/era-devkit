# Feature 795: External ERB Function Migration (CLOTHES.ERB / COMF446.ERB)

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

<!-- Created: 2026-02-16 -->
<!-- Review Context: FL POST-LOOP Step 6.3 (F775 deferred stub tracking) -->

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section defines the scope; F647 decomposed it into actionable sub-features. Phase 20: Equipment & Shop Systems philosophy inherited per Sub-Feature Requirements.

### Problem (Current Issue)

F775 migrated SHOP_COLLECTION.ERB to C# (CollectionTracker.cs) but three external ERB functions -- PANTSNAME (from CLOTHES.ERB:1489), PHOTO_NAME and PHOTO_DETAIL_NAME (from COMF446.ERB:166-371) -- reside in ERB files outside any Phase 20 sub-feature scope. Because the Phase 20 decomposition (F647) organized sub-features by shop ERB module rather than by utility function origin file, these cross-cutting display functions were left unassigned. F775 placed NotImplementedException stubs at CollectionTracker.cs:548-567 as a deliberate deferral, tracked via Mandatory Handoff to F795.

### Goal (What to Achieve)

Replace the three NotImplementedException stubs in CollectionTracker.cs with C# implementations equivalent to their ERB source functions: (1) PANTSNAME -- 56-entry SELECTCASE string lookup from CLOTHES.ERB:1489-1605, (2) PHOTO_NAME -- NTR type dispatch from COMF446.ERB:166-211, and (3) PHOTO_DETAIL_NAME -- nested SELECTCASE for 11 NTR categories with detail variants from COMF446.ERB:214-371. Add missing prostitution detail constants (20-26) to PhotoConstants.cs. Zero NotImplementedException stubs remaining after completion.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do three NotImplementedException stubs exist in CollectionTracker.cs? | F775 migrated SHOP_COLLECTION.ERB but could not implement PANTSNAME, PHOTO_NAME, and PHOTO_DETAIL_NAME because their source ERB files are outside F775 scope | CollectionTracker.cs:548-567 |
| 2 | Why are PANTSNAME and PHOTO_NAME outside F775 scope? | PANTSNAME is defined in CLOTHES.ERB and PHOTO_NAME/PHOTO_DETAIL_NAME in COMF446.ERB, not in SHOP_COLLECTION.ERB | CLOTHES.ERB:1489, COMF446.ERB:166 |
| 3 | Why don't sibling Phase 20 features (F776, F777) own these files? | F776 owns SHOP_ITEM.ERB and F777 owns SHOP_CUSTOM.ERB; neither owns CLOTHES.ERB or COMF446.ERB | feature-776.md, feature-777.md |
| 4 | Why are CLOTHES.ERB and COMF446.ERB unassigned to any Phase 20 feature? | They are multi-purpose utility ERB files serving many callers (PANTSNAME has 90+ call sites across 20+ ERB files), not shop-specific | Grep results across Game/ |
| 5 | Why (Root)? | The Phase 20 decomposition (F647) organized sub-features by shop ERB module, not by utility function origin file, leaving cross-cutting utility functions like PANTSNAME and PHOTO_NAME orphaned without a migration owner | feature-775.md:888 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | CollectionTracker.cs throws NotImplementedException for 3 display name functions | Phase 20 decomposition organized by shop ERB module, not by utility function origin, leaving PANTSNAME/PHOTO_NAME unassigned |
| Where | CollectionTracker.cs:548-567 (PantsName at 548, PhotoName at 556, PhotoNameDouble at 564) | F647 Phase 20 planning scope boundary (shop module decomposition) |
| Fix | Hardcode return values in stubs | Implement C# equivalents of PANTSNAME (CLOTHES.ERB), PHOTO_NAME and PHOTO_DETAIL_NAME (COMF446.ERB) |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F775 | [DONE] | Predecessor -- created the 3 NotImplementedException stubs this feature replaces |
| F776 | [PROPOSED] | Sibling Phase 20; SHOP_ITEM.ERB migration; no cross-calls to F795 functions |
| F777 | [PROPOSED] | Sibling Phase 20; SHOP_CUSTOM.ERB migration; no cross-calls to F795 functions |
| F782 | [DRAFT] | Related -- Post-Phase Review depends on all Phase 20 features |
| F647 | [DONE] | Related -- Phase 20 Planning that created the sub-feature decomposition |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Stub locations identified | FEASIBLE | Three stubs at CollectionTracker.cs:548, 556, 564 |
| Source ERB data available | FEASIBLE | PANTSNAME at CLOTHES.ERB:1489-1605; PHOTO_NAME at COMF446.ERB:166-211; PHOTO_DETAIL_NAME at COMF446.ERB:214-371 |
| No new interfaces needed | FEASIBLE | Pure string lookup functions; no DI registration required |
| Existing constants reusable | FEASIBLE | PhotoConstants.cs has photo type and detail constants (Detail_None through Detail_Lover) |
| Missing constants addable | FEASIBLE | Prostitution detail constants (20-26) from DIM.ERH:256-262 can be added to PhotoConstants.cs |
| Scope manageable | FEASIBLE | ~200-250 lines new C# code; 3 pure functions with no side effects |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| CollectionTracker.cs | HIGH | 3 stubs replaced with real implementations; display names rendered correctly |
| PhotoConstants.cs | LOW | 7 new prostitution detail constants added (20-26) |
| Test coverage | MEDIUM | New unit tests for PANTSNAME (56 entries), PHOTO_NAME dispatch, PHOTO_DETAIL_NAME (11 categories) |
| Other callers | LOW | C# implementations only serve CollectionTracker for now; future callers may reuse |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| ERB #FUNCTIONS return "" on no CASE match | ERB semantics | C# must return empty string for unmatched indices (no CASEELSE in PANTSNAME) |
| PHOTO_NAME delegates to PHOTO_DETAIL_NAME for NTR types | COMF446.ERB:171-172 | C# must replicate NTR type dispatch logic |
| PHOTO_NAME has optional second parameter (default 0) | COMF446.ERB:166 | PhotoNameDouble uses detail=0; must handle default |
| Prostitution detail constants (20-26) not yet in PhotoConstants.cs | DIM.ERH:256-262 | Must add 7 constants before PHOTO_DETAIL_NAME implementation |
| Display-mode only functions | CollectionTracker.cs:154-177, 247-263 | Functions only affect display output, no game state mutation |
| TreatWarningsAsErrors | Directory.Build.props | All new code must compile warning-free |
| Contact/Embrace have only 3 detail variants | COMF446.ERB:331-350 | Asymmetric variant count across NTR categories |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Missing prostitution detail constants (20-26) | HIGH | LOW | Add 7 constants to PhotoConstants.cs as prerequisite task |
| Scope creep to full CLOTHES.ERB/COMF446.ERB migration | MEDIUM | HIGH | Explicitly scope to 3 functions only; other functions remain ERB |
| F775 completed; stub signatures finalized | LOW | LOW | F775 is [DONE]; stub signatures at CollectionTracker.cs:548,556,564 confirmed stable |
| Future callers beyond CollectionTracker need access | MEDIUM | LOW | Architecture decision (private vs public) deferred to tech-designer |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| NotImplementedException stubs | `Grep "NotImplementedException.*F795" Era.Core/Shop/CollectionTracker.cs` | 3 matches | PantsName, PhotoName, PhotoNameDouble |
| Prostitution detail constants | `Grep "Detail_Prostitution" Era.Core/Shop/PhotoConstants.cs` | 0 matches | Constants 20-26 not yet defined |
| Unit test count | `dotnet test --filter CollectionTracker --list-tests` | 0 tests for name functions | No tests for stub methods (methods will be `internal static`, testable via InternalsVisibleTo) |

**Baseline File**: `.tmp/baseline-795.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | PantsName stub must be replaced | CollectionTracker.cs:548-551 | not_matches NotImplementedException.*PANTSNAME |
| C2 | PhotoName stub must be replaced | CollectionTracker.cs:556-559 | not_matches NotImplementedException.*PHOTO_NAME |
| C3 | Return "" for unmatched indices | ERB semantics (no CASEELSE) | Unit test verifies empty string return for unmapped index |
| C4 | PHOTO_NAME dispatches NTR types to PHOTO_DETAIL_NAME | COMF446.ERB:171-172 | Verify dispatch logic produces correct names |
| C5 | PHOTO_DETAIL_NAME covers 11 NTR categories | COMF446.ERB:214-371 | Spot-check representative categories |
| C6 | Prostitution detail constants (20-26) must exist | DIM.ERH:256-262 | PhotoConstants additions verified via matches |
| C7 | All 56 PANTSNAME entries from CLOTHES.ERB | CLOTHES.ERB:1492-1605 | Dictionary completeness (count or spot-check) |
| C8 | PhotoNameDouble = PhotoName with detail 0 | COMF446.ERB:191-210 | Unit test for double photo simple strings |
| C9 | Zero NotImplementedException in CollectionTracker after completion | Completion criteria | not_matches NotImplementedException in CollectionTracker.cs |
| C10 | TreatWarningsAsErrors compliance | Directory.Build.props | Build succeeds without warnings |
| C11 | PhotoNameDouble delegates to PhotoName(type, 0) | COMF446.ERB:166 interface | Unify implementation; avoid duplication |
| C12 | Contact/Embrace have only 3 detail variants | COMF446.ERB:331-350 | Boundary AC for asymmetric category sizes |

### Constraint Details

**C1: PantsName Stub Replacement**
- **Source**: CollectionTracker.cs:548-551 contains `throw new NotImplementedException("PANTSNAME - See F795")`
- **Verification**: Grep for NotImplementedException.*PANTSNAME must return 0 matches after implementation
- **AC Impact**: AC must use not_matches to verify stub removal

**C2: PhotoName Stub Replacement**
- **Source**: CollectionTracker.cs:556-559 contains `throw new NotImplementedException("PHOTO_NAME - See F795")`
- **Verification**: Grep for NotImplementedException.*PHOTO_NAME must return 0 matches after implementation
- **AC Impact**: AC must use not_matches to verify stub removal

**C3: Empty String Default Return**
- **Source**: ERB #FUNCTIONS semantics -- when no CASE matches, RESULTS defaults to "" (empty string)
- **Verification**: Unit test with out-of-range index (e.g., index 999) returns ""
- **AC Impact**: Boundary test AC required for unmapped indices

**C4: PHOTO_NAME NTR Dispatch**
- **Source**: COMF446.ERB:171-172 dispatches NTR types (CASE 10-20) to PHOTO_DETAIL_NAME
- **Verification**: Unit test verifies dispatch produces correct detail names
- **AC Impact**: AC must test at least one NTR type with non-zero detail

**C5: PHOTO_DETAIL_NAME 11 Categories**
- **Source**: COMF446.ERB:214-371 has SELECTCASE for 11 NTR categories
- **Verification**: Spot-check at least 3 categories with their detail variants
- **AC Impact**: Representative coverage, not exhaustive (55+ combinations)

**C6: Prostitution Detail Constants**
- **Source**: DIM.ERH:256-262 defines 7 prostitution detail constants (20-26)
- **Verification**: Grep PhotoConstants.cs for Detail_Prostitution or equivalent names
- **AC Impact**: matches AC for constant existence

**C7: PANTSNAME Dictionary Completeness**
- **Source**: CLOTHES.ERB:1492-1605 has 56 CASE branches (indices 2-50, 100-106)
- **Verification**: Count of entries in C# implementation matches ERB source count
- **AC Impact**: Spot-check or count-based AC

**C8: PhotoNameDouble Simple Strings**
- **Source**: COMF446.ERB:191-210 has direct string returns for non-NTR types
- **Verification**: Unit test for double photo type returns expected string
- **AC Impact**: Unit test AC

**C9: Zero NotImplementedException**
- **Source**: Completion criteria -- all 3 stubs must be replaced
- **Verification**: Grep CollectionTracker.cs for NotImplementedException returns 0
- **AC Impact**: Final verification AC (superset of C1+C2)

**C10: Warning-Free Build**
- **Source**: Directory.Build.props TreatWarningsAsErrors=true
- **Verification**: dotnet build succeeds with zero warnings
- **AC Impact**: Build AC

**C11: PhotoNameDouble Unification**
- **Source**: COMF446.ERB:166 shows PhotoNameDouble is semantically PhotoName(type, 0)
- **Verification**: Implementation delegates rather than duplicates
- **AC Impact**: Code structure AC (matches delegation pattern)

**C12: Contact/Embrace Asymmetry**
- **Source**: COMF446.ERB:331-350 shows Contact/Embrace have only 3 detail variants vs 4-5 for other categories
- **Verification**: Unit test with detail index beyond valid range returns "" for Contact/Embrace
- **AC Impact**: Boundary AC for asymmetric categories

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F775 | [DONE] | Created the 3 NotImplementedException stubs this feature replaces |

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

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "equivalent to their ERB source functions" | C# implementations must produce identical output to ERB PANTSNAME, PHOTO_NAME, PHOTO_DETAIL_NAME for all inputs | AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#15, AC#16 |
| "56-entry SELECTCASE string lookup" | PantsName dictionary must contain all 56 entries from CLOTHES.ERB:1492-1605 | AC#3, AC#4 |
| "Zero NotImplementedException stubs remaining" | All 3 NotImplementedException throws must be replaced with real implementations | AC#1, AC#12 |
| "11 NTR categories with detail variants" | PhotoDetailName must handle all 11 NTR categories from COMF446.ERB | AC#7, AC#8, AC#9, AC#10 |
| "Add missing prostitution detail constants (20-26)" | 7 new constants must be added to PhotoConstants.cs | AC#2 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PantsName stub removed | code | Grep(Era.Core/Shop/CollectionTracker.cs) | not_matches | `NotImplementedException.*PANTSNAME` | [x] |
| 2 | Prostitution detail constants exist | code | Grep(Era.Core/Shop/PhotoConstants.cs) | count_equals | `Detail_Prostitution` = 7 | [x] |
| 3 | PantsName dictionary has 56 entries | code | Grep(Era.Core/Shop/CollectionTracker.cs) | count_equals | `\[\d+\]\s*=\s*"` = 56 | [x] |
| 4 | PantsName returns known entry (spot-check) | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~PantsName_KnownIndex | succeeds | - | [x] |
| 5 | PantsName returns empty string for unmapped index | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~PantsName_UnmappedIndex | succeeds | - | [x] |
| 6 | PhotoName returns correct simple photo names | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoName_SimpleTypes | succeeds | - | [x] |
| 7 | PhotoName dispatches NTR types to PhotoDetailName | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoName_NtrDispatch | succeeds | - | [x] |
| 8 | PhotoDetailName returns correct detail variants | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoDetailName_DetailVariants | succeeds | - | [x] |
| 9 | PhotoDetailName Contact/Embrace asymmetric categories | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoDetailName_AsymmetricCategories | succeeds | - | [x] |
| 10 | PhotoNameDouble delegates to PhotoName with detail 0 | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | `PhotoName\(photoType,\s*0\)` | [x] |
| 11 | Build succeeds without warnings | build | dotnet build Era.Core | succeeds | - | [x] |
| 12 | Zero NotImplementedException in CollectionTracker | code | Grep(Era.Core/Shop/CollectionTracker.cs) | not_matches | `NotImplementedException` | [x] |
| 13 | Zero technical debt in modified files | code | Grep(Era.Core/Shop/CollectionTracker.cs,Era.Core/Shop/PhotoConstants.cs) | not_matches | `TODO|FIXME|HACK` | [x] |
| 14 | Unit tests pass | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~CollectionTracker | succeeds | - | [x] |
| 15 | PhotoName calls PhotoDetailName for NTR types | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | `PhotoDetailName\(photoType` | [x] |
| 16 | PantsName uses dictionary lookup | code | Grep(Era.Core/Shop/CollectionTracker.cs) | matches | `PantsNameDictionary` | [x] |

### AC Details

**AC#1: PantsName stub removed**
- **Test**: Grep pattern=`NotImplementedException.*PANTSNAME` path=`Era.Core/Shop/CollectionTracker.cs`
- **Expected**: 0 matches (stub replaced with real implementation)
- **Rationale**: Constraint C1. Verifies the PantsName NotImplementedException stub is replaced. (C9 subsumes this via AC#12, but AC#1 provides specific stub-level traceability.)

**AC#2: Prostitution detail constants exist**
- **Test**: Grep pattern=`Detail_Prostitution` path=`Era.Core/Shop/PhotoConstants.cs` | count
- **Expected**: 7 matches (Detail_Prostitution_VirginAuction0 through Detail_Prostitution_Wedding3)
- **Rationale**: Constraint C6. Seven prostitution detail constants (values 20-26) from DIM.ERH:256-262 must be added before PHOTO_DETAIL_NAME can handle the prostitution category.

**AC#3: PantsName dictionary has 56 entries**
- **Test**: Grep pattern=`\[\d+\]\s*=\s*"` path=`Era.Core/Shop/CollectionTracker.cs` | count
- **Expected**: 56 matches (49 entries for indices 2-50 plus 7 entries for indices 100-106)
- **Rationale**: Constraint C7. Ensures completeness of the 56-entry SELECTCASE lookup from CLOTHES.ERB:1492-1605. Pattern `[N] = "` matches dictionary initialization entries uniquely (switch expressions use `Type => "string"` format without square brackets). Pre-existing `[N] = "` count in CollectionTracker.cs: 0 (verified; only PantsName dictionary will use this format).

**AC#4: PantsName returns known entry (spot-check)**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~PantsName_KnownIndex`
- **Expected**: Test passes. Unit test verifies PantsName(2) returns "ばんそうこう", PantsName(100) returns "貞操帯", PantsName(50) returns "小窓付きのレースショーツ（赤）".
- **Rationale**: Constraint C7 spot-check. Verifies representative entries from beginning, end, and high-range of the lookup table.

**AC#5: PantsName returns empty string for unmapped index**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~PantsName_UnmappedIndex`
- **Expected**: Test passes. Unit test verifies PantsName(0), PantsName(1), PantsName(999) all return "".
- **Rationale**: Constraint C3. ERB #FUNCTIONS return "" when no CASE matches; C# must replicate this behavior for unmapped indices.

**AC#6: PhotoName returns correct simple photo names**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoName_SimpleTypes`
- **Expected**: Test passes. Unit test verifies: PhotoName(Daily, 0) returns "普段", PhotoName(Changing, 0) returns "お着替え", PhotoName(Bathing, 0) returns "入浴", PhotoName(Toilet, 0) returns "おトイレ姿", PhotoName(Masturbation, 0) returns "自慰姿", PhotoName(Double_Conversation, 0) returns "会話している姿", PhotoName(Double_Tea, 0) returns "お茶を飲んでいる姿", and PhotoName(999, 0) returns "" (unmatched).
- **Rationale**: Constraint C8. Verifies non-NTR types return correct direct strings and unmatched types return empty string. Spot-checks cover low-range singles (Daily=1, Changing=2), high-range singles (Toilet=8, Masturbation=17), and double types (Double_Conversation=100, Double_Tea=101). Masturbation(17) is NOT an NTR dispatch type — must return "自慰姿" directly, not delegate to PhotoDetailName.

**AC#7: PhotoName dispatches NTR types to PhotoDetailName**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoName_NtrDispatch`
- **Expected**: Test passes. Unit test verifies: PhotoName(NTR_Kiss, Detail_Begging) returns "舌を出して自分からキスをねだる姿", PhotoName(NTR_VaginalSex, Detail_Orgasm) returns "激しいセックスで絶頂に震える姿".
- **Rationale**: Constraint C4. Confirms NTR type dispatch produces correct detail names via PHOTO_DETAIL_NAME delegation.

**AC#8: PhotoDetailName returns correct detail variants**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoDetailName_DetailVariants`
- **Expected**: Test passes. Unit test spot-checks at least 3 NTR categories: Kiss (4 detail variants + CASEELSE), Fellatio (4 + CASEELSE), Prostitution (7 named + CASEELSE).
- **Rationale**: Constraint C5. Representative coverage of the 11 NTR categories with their detail variants.

**AC#9: PhotoDetailName Contact/Embrace asymmetric categories**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~PhotoDetailName_AsymmetricCategories`
- **Expected**: Test passes. Unit test verifies: Contact has only Begging and Lover detail variants (no Pleasure/Orgasm), Embrace has only Begging and Lover detail variants, and out-of-range detail (e.g., Detail_Pleasure for Contact) falls through to CASEELSE.
- **Rationale**: Constraint C12. Contact and Embrace have only 3 detail variants (Begging, Lover, CASEELSE) vs 5 for other categories. Verifies boundary behavior for asymmetric category sizes.

**AC#10: PhotoNameDouble delegates to PhotoName with detail 0**
- **Test**: Grep pattern=`PhotoName\(photoType,\s*0\)` path=`Era.Core/Shop/CollectionTracker.cs`
- **Expected**: Pattern found (PhotoNameDouble body calls PhotoName(photoType, 0))
- **Rationale**: Constraint C11. Verifies PhotoNameDouble delegates rather than duplicates, matching COMF446.ERB:166 semantics where the default second parameter is 0.

**AC#11: Build succeeds without warnings**
- **Test**: `dotnet build Era.Core`
- **Expected**: Build succeeds with exit code 0 (TreatWarningsAsErrors enforced by Directory.Build.props)
- **Rationale**: Constraint C10. All new code must compile warning-free.

**AC#12: Zero NotImplementedException in CollectionTracker**
- **Test**: Grep pattern=`NotImplementedException` path=`Era.Core/Shop/CollectionTracker.cs`
- **Expected**: 0 matches (all 3 stubs replaced)
- **Rationale**: Constraint C9. Final verification that no NotImplementedException remains in CollectionTracker.cs after all stub replacements. Superset of AC#1.

**AC#13: Zero technical debt in modified files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path=`Era.Core/Shop/CollectionTracker.cs,Era.Core/Shop/PhotoConstants.cs`
- **Expected**: 0 matches
- **Rationale**: Standard quality gate. No technical debt markers in files modified by this feature.

**AC#14: Unit tests pass**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~CollectionTracker`
- **Expected**: All tests pass (existing F775 tests + new F795 name function tests)
- **Rationale**: Regression safety. Ensures new implementations do not break existing CollectionTracker behavior while validating new function correctness.

**AC#15: PhotoName calls PhotoDetailName for NTR types**
- **Test**: Grep pattern=`PhotoDetailName\(photoType` path=`Era.Core/Shop/CollectionTracker.cs`
- **Expected**: Pattern found (PhotoName method body calls PhotoDetailName for NTR type dispatch)
- **Rationale**: Positive call verification (ENGINE.md Issue 66). Prevents hardcoded returns from passing stub removal + unit test ACs. Verifies the dispatch mechanism from COMF446.ERB:171-172 is implemented.

**AC#16: PantsName uses dictionary lookup**
- **Test**: Grep pattern=`PantsNameDictionary` path=`Era.Core/Shop/CollectionTracker.cs`
- **Expected**: Pattern found (PantsName method uses PantsNameDictionary for lookups)
- **Rationale**: Positive call verification (ENGINE.md Issue 66). Prevents hardcoded returns from passing stub removal ACs. Verifies the dictionary-based approach from Technical Design is used.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Replace PANTSNAME stub with 56-entry SELECTCASE string lookup from CLOTHES.ERB:1489-1605 | AC#1, AC#3, AC#4, AC#5, AC#16 |
| 2 | Replace PHOTO_NAME stub with NTR type dispatch from COMF446.ERB:166-211 | AC#6, AC#7, AC#12, AC#15 |
| 3 | Replace PHOTO_DETAIL_NAME stub with nested SELECTCASE for 11 NTR categories from COMF446.ERB:214-371 | AC#7, AC#8, AC#9, AC#15 |
| 4 | Add missing prostitution detail constants (20-26) to PhotoConstants.cs | AC#2 |
| 5 | Zero NotImplementedException stubs remaining after completion | AC#12 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Replace the three NotImplementedException stubs in CollectionTracker.cs with inline C# implementations using static readonly dictionaries and switch expressions. This approach preserves the pure function semantics of the ERB originals while leveraging C# language features for compile-time validation and performance.

**PantsName Implementation**: Use a static readonly Dictionary<int, string> initialized with a collection expression containing all 56 entries from CLOTHES.ERB:1492-1605. The method returns dictionary.GetValueOrDefault(itemNumber, "") to match ERB's empty string default when no CASE matches.

**PhotoName Implementation**: Use a switch expression on photoType that delegates NTR types (9-16, 18-19, 21) to PhotoDetailName and returns direct strings for non-NTR types. This directly mirrors COMF446.ERB:170-211 structure.

**PhotoNameDouble Implementation**: Delegate to PhotoName(photoType, 0) as a one-line implementation, matching COMF446.ERB:166 where the second parameter defaults to 0.

**PhotoDetailName Implementation**: Use nested switch expressions - outer switch on photoType (11 NTR categories), inner switch on detail (4-5 variants per category, except Contact/Embrace with only 3). This matches COMF446.ERB:214-371 SELECTCASE structure.

**PhotoConstants Additions**: Add 7 new public const int fields (values 20-26) to PhotoConstants.cs for prostitution detail constants from DIM.ERH:256-262.

This approach satisfies all ACs by providing exact string equivalence to ERB sources (AC#4-9), verifiable stub removal (AC#1, AC#12), correct entry counts (AC#2, AC#3), delegation patterns (AC#10), and zero technical debt (AC#11, AC#13, AC#14).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | PantsName implementation replaces NotImplementedException stub; Grep verifies 0 matches for pattern |
| 2 | Add 7 const int fields to PhotoConstants.cs with Detail_Prostitution prefix; Grep counts 7 matches |
| 3 | PantsName dictionary initializer contains 56 `=> "` entries; Grep counts matches in method scope |
| 4 | Unit test calls PantsName(2), PantsName(100), PantsName(50) and asserts expected Japanese strings |
| 5 | Unit test calls PantsName with unmapped indices (0, 1, 999) and asserts empty string return |
| 6 | Unit test calls PhotoName with non-NTR types (Daily, Bathing, Double_Conversation, 999) and asserts expected strings |
| 7 | Unit test calls PhotoName with NTR types (NTR_Kiss, NTR_VaginalSex) and non-zero detail, asserts detail variant strings |
| 8 | Unit test calls PhotoDetailName for 3+ categories (Kiss, Fellatio, Prostitution) with detail variants, asserts strings |
| 9 | Unit test calls PhotoDetailName for Contact/Embrace with out-of-range detail (Detail_Pleasure), asserts CASEELSE fallback |
| 10 | PhotoNameDouble body contains `PhotoName(photoType, 0)`; Grep matches delegation pattern |
| 11 | dotnet build Era.Core succeeds; TreatWarningsAsErrors enforces zero warnings |
| 12 | All 3 stubs replaced; Grep for NotImplementedException in CollectionTracker.cs returns 0 |
| 13 | Grep for TODO/FIXME/HACK in modified files returns 0 |
| 14 | dotnet test Era.Core.Tests --filter CollectionTracker runs all tests (existing F775 + new F795) and passes |
| 15 | PhotoName method body calls PhotoDetailName(photoType, ...); Grep verifies call site exists (positive call verification) |
| 16 | PantsName method uses PantsNameDictionary for lookups; Grep verifies dictionary reference exists (positive call verification) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Data structure for PantsName | A) Dictionary<int, string>, B) switch expression with 56 arms, C) int[] lookup with offset calculation | Dictionary<int, string> | A handles sparse indices (2-50, 100-106) cleanly; B is verbose (56 case arms); C requires complex offset logic for non-contiguous ranges |
| PhotoName delegation | A) Inline all detail strings in PhotoName, B) Delegate NTR types to PhotoDetailName | Delegate to PhotoDetailName | B matches ERB structure (COMF446.ERB:171-172) and reduces duplication; A violates DRY for 50+ detail strings |
| PhotoNameDouble implementation | A) Duplicate PhotoName logic, B) Delegate to PhotoName(type, 0) | Delegate to PhotoName | B is one line and matches ERB semantics; A duplicates 20+ case arms unnecessarily |
| PhotoDetailName structure | A) Flat switch with 55+ composite cases, B) Nested switch (type → detail) | Nested switch | B mirrors ERB SELECTCASE nesting and provides clear category boundaries; A loses semantic grouping |
| Constant naming | A) Detail_Prostitution_VirginAuction0, B) Detail_VirginAuction0, C) Detail_20 | Detail_Prostitution_VirginAuction0 | A includes category prefix for clarity and grep-ability; B risks collision with future categories; C loses semantic meaning |
| Method visibility | A) private, B) public, C) internal | internal | C enables (a) unit testing via InternalsVisibleTo (Era.Core.csproj:17) and (b) future C# callers within Era.Core when CLOTHES.ERB/COMF446.ERB callers are migrated, without exposing to external assemblies. Avoids deferred refactoring cost per Zero Debt Upfront principle |
| Method qualifier | A) static, B) instance | static | All three functions are pure (no instance state); existing F775 stubs are instance methods, so `static` keyword must be added during implementation |
| Dictionary initialization | A) static readonly with new Dictionary { {2, "..."}, ... }, B) static readonly with collection expression [2] = "...", C) lazy initialization | static readonly collection expression | B is most concise in C# 12; A is verbose; C adds unnecessary complexity for static data |

### Interfaces / Data Structures

No new interfaces or public APIs. Three existing stubs (PantsName, PhotoName, PhotoNameDouble) are replaced and one new method (PhotoDetailName) is added, all as internal static methods within CollectionTracker.cs. They are consumed by the existing display logic in ShowCollectionPanties, ShowCollectionPhotoSingle, ShowCollectionPhotoCheating, ShowCollectionPhotoProstitute, and ShowCollectionPhotoDouble.

PhotoConstants.cs additions:

```csharp
// Prostitution detail constants (DIM.ERH:256-262)
public const int Detail_Prostitution_VirginAuction0 = 20;  // 写真詳細_売春_処女競売0
public const int Detail_Prostitution_VirginAuction1 = 21;  // 写真詳細_売春_処女競売1
public const int Detail_Prostitution_VirginAuction2 = 22;  // 写真詳細_売春_処女競売2
public const int Detail_Prostitution_Wedding0 = 23;        // 写真詳細_売春_結婚式0
public const int Detail_Prostitution_Wedding1 = 24;        // 写真詳細_売春_結婚式1
public const int Detail_Prostitution_Wedding2 = 25;        // 写真詳細_売春_結婚式2
public const int Detail_Prostitution_Wedding3 = 26;        // 写真詳細_売春_結婚式3
```

### Upstream Issues

No upstream issues discovered. All required interfaces exist (CollectionTracker is self-contained for these display functions), all AC constraints are satisfied by the design, and all ERB source data is available and complete.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 2 | Add 7 prostitution detail constants to PhotoConstants.cs | | [x] |
| 2 | 1,3,4,5,16 | Replace PantsName stub with 56-entry dictionary implementation | | [x] |
| 3 | 6,7,8,9,15 | Replace PhotoName and PhotoDetailName stubs with switch-based implementations | | [x] |
| 4 | 10 | Implement PhotoNameDouble delegation to PhotoName | | [x] |
| 5 | 11,12,13,14 | Verify build, tests, and quality gates | | [x] |

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
| 1 | implementer | sonnet | Tasks 1-2 | PhotoConstants.cs updated + PantsName implemented |
| 2 | implementer | sonnet | Tasks 3-4 | PhotoName, PhotoDetailName, PhotoNameDouble implemented |
| 3 | test-creator | sonnet | ACs 4-9 | Unit tests for name functions |
| 4 | implementer | sonnet | Task 5 | Quality verification |

### Execution Steps

**Phase 1: Constants and PantsName (implementer)**

1. Add 7 prostitution detail constants to `Era.Core/Shop/PhotoConstants.cs`:
   ```csharp
   // Prostitution detail constants (DIM.ERH:256-262)
   public const int Detail_Prostitution_VirginAuction0 = 20;  // 写真詳細_売春_処女競売0
   public const int Detail_Prostitution_VirginAuction1 = 21;  // 写真詳細_売春_処女競売1
   public const int Detail_Prostitution_VirginAuction2 = 22;  // 写真詳細_売春_処女競売2
   public const int Detail_Prostitution_Wedding0 = 23;        // 写真詳細_売春_結婚式0
   public const int Detail_Prostitution_Wedding1 = 24;        // 写真詳細_売春_結婚式1
   public const int Detail_Prostitution_Wedding2 = 25;        // 写真詳細_売春_結婚式2
   public const int Detail_Prostitution_Wedding3 = 26;        // 写真詳細_売春_結婚式3
   ```

2. Replace `PantsName` stub (CollectionTracker.cs:548-551) with dictionary-based implementation:
   ```csharp
   internal static readonly Dictionary<int, string> PantsNameDictionary = new()
   {
       [2] = "ばんそうこう",
       [3] = "ドロワーズ（白）",
       // ... (56 total entries from CLOTHES.ERB:1492-1605)
       [100] = "貞操帯",
       [101] = "トランクス",
       // ... remaining entries up to [106]
   };

   internal static string PantsName(int itemNumber)
   {
       return PantsNameDictionary.GetValueOrDefault(itemNumber, "");
   }
   ```

   **Source**: Copy all 56 entries from CLOTHES.ERB:1492-1605 (indices 2-50, 100-106).

**Phase 2: Photo Name Functions (implementer)**

3. Replace `PhotoName` stub (CollectionTracker.cs:556-559) with switch expression:
   ```csharp
   internal static string PhotoName(int photoType, int detail)
   {
       return photoType switch
       {
           1 => "普段",           // Daily (写真_普段=1)
           3 => "入浴",           // Bathing (写真_入浴=3)
           4 => "寝顔",           // Sleeping (写真_寝顔=4)
           // ... non-NTR types (COMF446.ERB:191-210)
           17 => "自慰姿",        // Masturbation (写真_自慰=17)
           100 => "会話している姿", // Double_Conversation (写真_住人同士_会話=100)
           // NTR types delegate to PhotoDetailName (COMF446.ERB:171-172)
           // Values: 9-16 (Kiss through AnalSex), 18 (Contact), 19 (Embrace), 21 (Prostitution)
           9 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 18 or 19 or 21
               => PhotoDetailName(photoType, detail),
           _ => ""
       };
   }
   ```

4. Replace `PhotoDetailName` stub with nested switch:
   ```csharp
   internal static string PhotoDetailName(int photoType, int detail)
   {
       return photoType switch
       {
           9 => detail switch  // Kiss (写真_NTR_キス=9, COMF446.ERB:219-232)
           {
               1 => "舌を出して自分からキスをねだる姿",  // Detail_Begging (写真詳細_おねだり)
               2 => "蕩けた顔でキスされるがままの姿",    // Detail_Pleasure (写真詳細_快楽)
               3 => "キスだけで絶頂に達してしまう姿",    // Detail_Orgasm (写真詳細_絶頂)
               4 => "恋人のように熱いキスを交わす姿",    // Detail_Lover (写真詳細_恋人)
               _ => "キスしている姿"                     // CASEELSE
           },
           // ... 9 more NTR categories (COMF446.ERB:233-350)
           // All strings MUST be copied verbatim from COMF446.ERB source
           21 => detail switch  // Prostitution (写真_NTR_売春=21, COMF446.ERB:351-370)
           {
               20 => "ステージで競りにかけられている姿", // Detail_Prostitution_VirginAuction0
               21 => "客に抱き寄せられている姿",         // Detail_Prostitution_VirginAuction1
               22 => "客とキスしている姿",               // Detail_Prostitution_VirginAuction2
               23 => "ウェディングドレス姿",             // Detail_Prostitution_Wedding0
               24 => "客と指輪交換している姿",           // Detail_Prostitution_Wedding1
               25 => "客と誓いのキスをしている姿",       // Detail_Prostitution_Wedding2
               26 => "ドレスのまま客に抱かれている姿",   // Detail_Prostitution_Wedding3
               _ => "客に抱きついている姿"               // CASEELSE
           },
           _ => ""
       };
   }
   ```
   **CRITICAL**: All Japanese strings MUST be copied verbatim from COMF446.ERB:218-370. The code example above shows corrected Kiss and Prostitution strings. For the remaining 9 categories, read the ERB source directly.

5. Replace `PhotoNameDouble` stub (CollectionTracker.cs:563-567) with delegation:
   ```csharp
   internal static string PhotoNameDouble(int photoType)
   {
       return PhotoName(photoType, 0);
   }
   ```

**Phase 3: Unit Tests (test-creator)**

6. Create unit tests in `Era.Core.Tests/Shop/CollectionTrackerTests.cs`:
   - `PantsName_KnownIndex`: Verify PantsName(2) = "ばんそうこう", PantsName(100) = "貞操帯", PantsName(50) = "小窓付きのレースショーツ（赤）"
   - `PantsName_UnmappedIndex`: Verify PantsName(0/1/999) returns ""
   - `PhotoName_SimpleTypes`: Verify non-NTR types return correct strings
   - `PhotoName_NtrDispatch`: Verify NTR types delegate to PhotoDetailName
   - `PhotoDetailName_DetailVariants`: Spot-check 3+ categories with detail variants
   - `PhotoDetailName_AsymmetricCategories`: Verify Contact/Embrace only have 3 detail variants

**Phase 4: Quality Verification (implementer)**

7. Run `dotnet build Era.Core` (AC#11)
8. Run `dotnet test Era.Core.Tests --filter FullyQualifiedName~CollectionTracker` (AC#14)
9. Verify no NotImplementedException in CollectionTracker.cs (AC#12)
10. Verify no TODO/FIXME/HACK in modified files (AC#13)

### Success Criteria

All 16 ACs pass:
- AC#1: PantsName stub removed
- AC#2: 7 prostitution constants exist
- AC#3: 56 dictionary entries
- AC#4-5: PantsName unit tests
- AC#6-9: PhotoName/PhotoDetailName unit tests
- AC#10: PhotoNameDouble delegation pattern
- AC#11: Build succeeds
- AC#12: Zero NotImplementedException
- AC#13: Zero technical debt
- AC#14: All tests pass
- AC#15: PhotoName calls PhotoDetailName (positive call verification)
- AC#16: PantsName uses dictionary lookup (positive call verification)

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| (none) | - | - | - | - |

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists -> OK (file created during /run)
- Option B: Referenced Feature exists -> OK
- Option C: Phase exists in architecture.md -> OK
- Missing Task for Option A -> FL FAIL
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
| 2026-02-17 | Phase 1 | initializer | Status [REVIEWED]→[WIP] | READY |
| 2026-02-17 | Phase 2 | explorer | Investigate source ERB files | All 3 function sources confirmed |
| 2026-02-17 | Phase 3 | implementer | Create 6 unit tests (TDD RED) | 42 compile errors (expected) |
| 2026-02-17 | Phase 4 | implementer | Tasks 1-4: Constants + PantsName + PhotoName/PhotoDetailName + PhotoNameDouble | SUCCESS: 24/24 tests pass |
| 2026-02-17 | Phase 5 | orchestrator | Refactoring review | SKIP (no refactoring needed) |
| 2026-02-17 | Phase 7 | ac-static-verifier | Code ACs (1-3,10,12-13,15-16) | 8/8 PASS |
| 2026-02-17 | Phase 7 | ac-static-verifier | Build AC (11) | 1/1 PASS |
| 2026-02-17 | Phase 7 | ac-tester | Test ACs (4-9,14) | 7/7 PASS |
| 2026-02-17 | START | implementer | Tasks 1-4 - PhotoConstants + CollectionTracker stubs | - |
| 2026-02-17 | END | implementer | Tasks 1-4 - PhotoConstants + CollectionTracker stubs | SUCCESS (build 0 warnings, 24/24 tests pass) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A->B->A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-DriftCheck iter1: Related Features table | F775 status [WIP] → [DONE] synced
- [fix] Phase1-DriftCheck iter1: Feasibility/Constraint line refs | PhotoName stub line 555→556, PhotoNameDouble 563→564 corrected to match source
- [fix] Phase1-DriftCheck iter1: Key Decisions table | Added Method qualifier row: static chosen, noting F775 stubs are instance methods needing `static` addition
- [info] Phase1-DriftChecked: F775 (Predecessor)
- [fix] Phase2-Review iter1: feature-795.md | Added missing ## Dependencies section (template compliance)
- [fix] Phase2-Review iter1: Implementation Contract Step 3 | NTR dispatch range >= 10 and <= 21 → explicit case list {9-16,18,19,21}; added Masturbation(17) to non-NTR; fixed constant values (Daily=1, Bathing=3, etc.)
- [fix] Phase2-Review iter1: AC#6 | Added Masturbation(17) spot-check to PhotoName_SimpleTypes test
- [fix] Phase2-Review iter1: Implementation Contract Step 4 | Corrected all PhotoDetailName strings (Kiss + Prostitution) to match COMF446.ERB source; added CRITICAL note about verbatim ERB copy
- [fix] Phase2-Review iter1: Risks table | F775 status updated to reflect [DONE] completion
- [fix] Phase2-Review iter2: Implementation Contract Step 3 | Sleeping string 睡眠中 → 寝顔 to match COMF446.ERB:180
- [fix] Phase2-Review iter2: AC#6 | Expanded spot-checks: added Changing(2), Toilet(8), Double_Tea(101) for broader non-NTR coverage
- [fix] Phase2-Review iter3: Implementation Contract Step 2 | PantsName example [3]="ノーパン"→"ドロワーズ（白）", [101]="拘束貞操帯"→"トランクス" to match CLOTHES.ERB source
- [fix] Phase3-Maintainability iter2: Key Decisions + Implementation Contract | Method visibility private→internal for testability (InternalsVisibleTo) and Zero Debt Upfront (90+ ERB callers)
- [fix] Phase3-Maintainability iter2: Interfaces/Data Structures | "three methods"→"three stubs + one new method"; fixed caller names to match source (ShowCollectionPanties etc.)
- [fix] Phase3-Maintainability iter2: Impact Analysis | Entry count 55→56 to match spec's own AC#3/C7

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F775](feature-775.md) - CollectionTracker.cs migration (stub origin for PANTSNAME/PHOTO_NAME)
- [Related: F776](feature-776.md) - SHOP_ITEM.ERB migration
- [Related: F777](feature-777.md) - SHOP_CUSTOM.ERB migration
- [Related: F782](feature-782.md) - Post-Phase Review
- [Related: F647](feature-647.md) - Phase 20 Planning
