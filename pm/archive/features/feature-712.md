# Feature 712: LoadData() Dead Code Removal

## Status: [DONE]

## Links

- [feature-558.md](feature-558.md) - YAML-based VariableSizeService superseding CSV size loading
- [feature-575.md](feature-575.md) - Removed LoadData() call site, making the method unreachable
- [feature-708.md](feature-708.md) - TreatWarningsAsErrors=true build constraint
- [feature-710.md](feature-710.md) - AllocateDataArrays() superseding allocation portion of LoadData()
- [feature-711.md](feature-711.md) - Bridge YAML variable definitions to engine ConstantData (predecessor)

## Type: engine

## Review Notes
- [resolved-applied] Phase0-RefCheck iter1: Missing Links section. Feature-712.md references F558, F575, F708, F710, F711 in Background/Problem and Related Features sections but has no dedicated Links section for explicit cross-referencing.
- [resolved-applied] Phase2-Maintainability iter1: F712's entire premise is factually incorrect. F712 claims F711 creates LoadCsvData() method and csvDataLoaded guard field, but F711 (Fix Engine --unit Mode CSV Constant Resolution) does neither. Grep confirms LoadCsvData does not exist anywhere in the codebase (zero .cs files). csvDataLoaded also does not exist. AC#2 (LoadCsvData remains as sole CSV loading method) verifies a non-existent method. AC#9 (csvDataLoaded guard field preserved) verifies a non-existent field. The Root Cause Analysis, Related Features table, Dependencies table, Pattern Analysis, Feasibility Assessment, Technical Design, and Implementation Contract all reference LoadCsvData/csvDataLoaded as F711 artifacts that must be preserved.
- [resolved-applied] Phase2-Maintainability iter1: Philosophy Derivation row 2 claims 'Only LoadCsvData() (F711) must remain as the CSV loading method' but LoadCsvData does not exist. Goal Coverage row 1 claims 'Eliminate code duplication between LoadData() and LoadCsvData()' but there is no duplication since LoadCsvData does not exist. The entire 'duplication elimination' framing is invalid -- this is pure dead code removal, not deduplication.
- [resolved-applied] Phase2-Maintainability iter1: F711 is listed as Predecessor with description 'Creates LoadCsvData() method that contains the data population logic extracted from LoadData()'. F711 does not create LoadCsvData. F711's actual scope is fixing --unit mode CSV constant resolution. The predecessor relationship may still be valid if F711 modified LoadData or ConstantData, but the stated reason is factually wrong.
- [resolved-applied] Phase2-Maintainability iter2: AC#2 verifies non-existent method LoadCsvData() and will always FAIL. LoadCsvData() does not exist anywhere in the codebase.
- [resolved-applied] Phase2-Maintainability iter2: AC#9 verifies non-existent field csvDataLoaded and will always FAIL. csvDataLoaded does not exist anywhere in the codebase.
- [resolved-applied] Phase2-Maintainability iter2: Feature title 'LoadData() Duplication Elimination' and framing are factually incorrect. LoadCsvData() does not exist. There is no duplication. This is pure dead code removal of unreachable LoadData() method.
- [resolved-applied] Phase2-Maintainability iter2: Dependencies table F711 predecessor rationale is fabricated. F711 does not create LoadCsvData() and does not extract anything from LoadData().
- [resolved-applied] Phase2-Maintainability iter2: AC#10 verifies non-existent field dataArraysAllocated and will always FAIL. dataArraysAllocated does not exist anywhere in the codebase.
- [resolved-applied] Phase2-Maintainability iter2: Technical Design references non-existent LoadCsvData() and csvDataLoaded throughout Key Decisions and Implementation Contract.
- [resolved-applied] Phase2-Maintainability iter2: 5 Whys and Root Cause Analysis reference non-existent LoadCsvData() artifacts. Entire duplication framing is invalid.
- [resolved-applied] Phase2-Maintainability iter2: Task 3 references non-existent regression targets (LoadCsvData, guard fields). AC#2, AC#9, AC#10 verify non-existent artifacts.
- [resolved-applied] Phase2-Maintainability iter2: Feasibility Assessment references impossible Option B (delegate to non-existent LoadCsvData).
- [resolved-invalid] Phase2-Maintainability iter2: dataArraysAllocated guard field does not exist - INVALID, field exists at ConstantData.cs line 197
- [resolved-invalid] Phase2-Maintainability iter2: AC#10 verifies non-existent field dataArraysAllocated - INVALID, AC#10 is valid since field exists
- [resolved-applied] Phase3-ACValidation iter3: AC#2 verifies non-existent method LoadCsvData(). This AC will always FAIL since the method does not exist
- [resolved-applied] Phase3-ACValidation iter3: AC#9 verifies non-existent field csvDataLoaded. This AC will always FAIL since the field does not exist
- [resolved-applied] Phase3-ACValidation iter3: Feature title 'LoadData() Duplication Elimination' is factually incorrect. LoadCsvData() does not exist. There is no duplication. This is pure dead code removal
- [resolved-applied] Phase3-ACValidation iter3: F711 predecessor rationale states F711 creates LoadCsvData() but F711 does not create this method. The predecessor reason is fabricated
- [resolved-applied] Phase3-ACValidation iter3: Missing negative ACs for engine type feature. All ACs are positive-removal or positive-preservation
- [resolved-applied] Phase3-ACValidation iter3: AC#8 checks for TODO/FIXME/HACK markers that already don't exist pre-implementation, provides no incremental verification
- [resolved-applied] Phase4-Feasibility iter3: AC#2 (LoadCsvData) and AC#9 (csvDataLoaded) verify non-existent artifacts and will always FAIL
- [resolved-applied] Phase4-Feasibility iter3: Task#3 verifies LoadCsvData method and csvDataLoaded field, neither of which exist. Task is partially untestable
- [resolved-applied] Phase4-Feasibility iter3: F711 predecessor rationale is fabricated. F711 did not create LoadCsvData()
- [resolved-applied] Phase4-Feasibility iter3: Feature Title 'LoadData() Duplication Elimination' is factually incorrect. LoadCsvData() does not exist. No duplication exists
- [resolved-applied] Phase4-Feasibility iter3: Root Cause Analysis references non-existent LoadCsvData() throughout. Entire duplication framing is invalid
- [resolved-applied] Phase4-Feasibility iter3: Technical Design references LoadCsvData() preservation and csvDataLoaded guard field. These artifacts do not exist
- [resolved-applied] Phase4-Feasibility iter3: Goal Coverage references LoadCsvData as covering ACs. Goal Coverage Verification is invalid for non-existent artifacts
- [resolved-applied] Phase4-Feasibility iter3: Feasibility Assessment Option B references non-existent LoadCsvData(). Option B is not viable
- [resolved-applied] Phase4-Feasibility iter3: AC#8 checks for markers that already don't exist pre-implementation, provides no incremental verification value
- [resolved-applied] Phase6-FinalRefCheck iter3: ACs reference non-existent LoadCsvData() method and csvDataLoaded field. F711 [DONE] does not create LoadCsvData() - Grep confirms zero matches in codebase. AC#2 and AC#9 will always FAIL
- [resolved-applied] Phase6-FinalRefCheck iter3: Feature 712 premise is factually incorrect. Claims F711 creates LoadCsvData() method that duplicates LoadData() logic, but LoadCsvData() does not exist in codebase
- [resolved-applied] Phase6-FinalRefCheck iter3: F711 predecessor description states 'Creates LoadCsvData() method' but this is factually incorrect. F711 [DONE] has no such scope
- [resolved-applied] Phase6-FinalRefCheck iter3: Goal 'Eliminate code duplication between LoadData() and LoadCsvData()' references non-existent LoadCsvData(). Goal assumes duplication exists when no such method is in codebase

POST-LOOP: Feature rewritten to reflect dead code removal framing

## Background

### Problem

LoadData() method in ConstantData.cs is unreachable dead code with zero callers. F575 removed its only call site. All three concerns it served (size reading, allocation, data population) have been superseded by F558/F710/F711. The method and its cascading private helpers (~452 lines) remain as maintenance burden.

### Goal

Remove the unreachable LoadData() method and all cascading dead code (private helper methods, orphaned fields) from ConstantData.cs to eliminate maintenance burden and complete the incremental migration started by F558/F575/F710/F711.

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: LoadData() method (~64 lines) and its 4 private helper methods (~388 lines) remain in ConstantData.cs despite having zero callers
2. Why: F575 removed the sole call site (`constant.LoadData()` from ProcessInitializer) making the entire method tree unreachable dead code
3. Why: F575 deliberately only removed the call site, not the method definition, to minimize regression risk during the migration
4. Why: LoadData() was a monolithic method mixing three concerns (size reading, allocation, data population); removing it required verifying all concerns were superseded first
5. Why: The incremental migration (F558→F575→F710→F711) decomposed concerns one at a time rather than atomically, leaving the original method intact as scaffolding until all replacements were confirmed working

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| LoadData() and 4 helper methods (~452 lines) exist despite having zero callers | F575 removed the call site but preserved the method definition for safety during incremental migration |
| Private helper methods (loadVariableSizeData, changeVariableSizeData, decideActualArraySize) are orphaned | Their sole caller LoadData() is dead code, making the entire call chain dead |
| changedCode field has no consumers | All 13 references are within the dead helper methods |

### Conclusion

The root cause is **dead code preserved during incremental migration**. F575 removed the sole call site for LoadData(), making it unreachable. The subsequent features (F710, F711) created replacement mechanisms without touching the now-dead LoadData() method.

The `LoadData()` method (lines 631-694) is dead code:
- **Size reading** (line 634): Superseded by YAML (F558)
- **Array allocation** (lines 635-640): Superseded by `AllocateDataArrays()` (F710)
- **Data population** (lines 641-693): No longer needed; F711 bridged YAML definitions to ConstantData via PopulateConstantNames()

No callers of `LoadData()` exist in the codebase (confirmed by Grep search). The method is unreachable dead code.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F711 | [DONE] | Predecessor | Bridged YAML variable definitions to engine ConstantData via PopulateConstantNames(). Confirms LoadData() data population concern is superseded |
| F710 | [DONE] | Related | Created `AllocateDataArrays()` that supersedes allocation portion (lines 616-621) of `LoadData()` |
| F575 | [DONE] | Root cause origin | Removed `constant.LoadData()` call from ProcessInitializer, making `LoadData()` unreachable |
| F558 | [DONE] | Related | Introduced YAML-based VariableSizeService, superseding `loadVariableSizeData()` call (line 615) in `LoadData()` |
| F708 | [DONE] | Constraint | TreatWarningsAsErrors=true -- any compilation warnings will fail the build |

### Pattern Analysis

This is the **cleanup phase** of a four-step incremental migration:
- **F558**: YAML replaces CSV for variable sizes → `loadVariableSizeData()` in `LoadData()` becomes obsolete
- **F575**: Removes `LoadData()` call entirely → method becomes dead code but preserved
- **F710**: Restores allocation via `AllocateDataArrays()` → allocation portion of `LoadData()` superseded
- **F711**: Bridges YAML definitions to ConstantData via PopulateConstantNames() → data population concern of `LoadData()` superseded
- **F712** (this): Eliminates the now-fully-superseded `LoadData()` → completes the migration

The pattern is "incremental replacement where each step supersedes one concern of the monolithic method." F712 is the final cleanup that removes the scaffolding.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | `LoadData()` has zero callers (confirmed by Grep). It can be safely removed or refactored. |
| Scope is realistic | YES | Single-file change in ConstantData.cs: remove `LoadData()` and all cascading dead code. Estimated ~452 lines removed. |
| No blocking constraints | YES | All predecessor features (F558, F575, F710, F711) are [DONE]. No blocking constraints. |

**Verdict**: FEASIBLE

**Recommendation**: Complete removal is the only viable approach because:
1. Zero callers confirmed by codebase search
2. Dead code is maintenance burden per Zero Debt Upfront principle
3. The `AllocateDataArrays()` guard prevents `LoadData()` from working correctly anyway (double allocation would throw)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F711 | [DONE] | Bridged YAML variable definitions to engine ConstantData. Confirms data population concern of LoadData() is fully superseded. Must be [DONE] before dead code removal. |
| Related | F710 | [DONE] | Created `AllocateDataArrays()` that supersedes allocation lines 616-621 of `LoadData()` |
| Related | F575 | [DONE] | Removed `LoadData()` call site, making the method unreachable dead code |
| Related | F558 | [DONE] | YAML supersedes `loadVariableSizeData()` in `LoadData()` line 615 |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| None | - | - | This is a pure refactoring of internal engine code with no external dependencies |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| None (zero callers) | NONE | `LoadData()` has no callers in the codebase. Grep for `\.LoadData\(` returns zero results in all .cs files. |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `engine/Assets/Scripts/Emuera/GameData/ConstantData.cs` | Update | Remove `LoadData()` method and all cascading dead code (~452 lines). Pure deletion, no new code. |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| All predecessor features completed | F558/F575/F710/F711 all [DONE] | LOW - No blocking constraints remain |
| `TreatWarningsAsErrors=true` | F708 Directory.Build.props | MEDIUM - Removal must not leave dangling references that cause compilation warnings/errors |
| `LoadData()` is `public` on `internal` class | ConstantData.cs:612, class is `internal sealed` | LOW - Removing a public method on an internal class has no external API impact |
| `loadVariableSizeData()` is `private` | Called only from `LoadData()` line 615 | LOW - If LoadData() is removed, `loadVariableSizeData()` also becomes dead code (no other callers). Could be removed too. |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Hidden callers of LoadData() not found by Grep | Very Low | High | Grep search confirmed zero callers. The method is on an `internal sealed` class, limiting call sites to the same assembly. |
| Removing LoadData() breaks reflection-based callers | Very Low | Medium | No reflection-based invocation patterns exist in this codebase (not a plugin architecture). |
| loadVariableSizeData() orphaned as dead code | High | Low | If LoadData() is removed, loadVariableSizeData() has zero callers. Should be removed in same feature to avoid leaving dead code. Track in Tasks. |
| Post-F711 changes to ConstantData.cs shift line numbers | Low | Low | Verify line numbers against current file state before implementation |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "remove unreachable dead code" (Goal) | The dead `LoadData()` method must be removed entirely since it has zero callers and all three concerns are superseded by F558/F710/F711 | AC#1 |
| "all cascading dead code" (Goal) | All private helper methods orphaned by `LoadData()` removal must also be removed to prevent dead code accumulation | AC#2, AC#3, AC#4 |
| "Zero Debt Upfront" principle | The `changedCode` field used exclusively by removed methods must also be removed | AC#5 |
| F708 TreatWarningsAsErrors constraint | Build must succeed with zero warnings after removal | AC#6 |
| "completes the migration" (Pattern Analysis) | No residual dead code from the LoadData() chain remains; F710's dataArraysAllocated guard field is preserved | AC#2, AC#3, AC#4, AC#5, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | LoadData() method removed from ConstantData.cs | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | not_matches | `public\s+void\s+LoadData\(` | [x] |
| 2 | loadVariableSizeData() removed | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | not_contains | `loadVariableSizeData` | [x] |
| 3 | changeVariableSizeData() removed | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | not_contains | `changeVariableSizeData` | [x] |
| 4 | decideActualArraySize methods removed | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | not_contains | `decideActualArraySize` | [x] |
| 5 | changedCode field removed | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | not_contains | `changedCode` | [x] |
| 6 | Engine builds successfully with zero warnings | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 7 | AllocateDataArrays guard field preserved | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | contains | `dataArraysAllocated` | [x] |

### AC Details

**AC#1: LoadData() method removed from ConstantData.cs**
- The `LoadData()` method (lines 631-694) is dead code with zero callers (confirmed by Grep for `\.LoadData\(` across all engine .cs files).
- All three concerns it served are superseded: size reading (F558 YAML), allocation (F710 AllocateDataArrays), data population (F711 PopulateConstantNames).
- The entire method body must be deleted, not just marked `[Obsolete]`, per Zero Debt Upfront principle.
- Verification: Grep for `public\s+void\s+LoadData\(` must return zero matches.

**AC#2: loadVariableSizeData() removed**
- Private method that reads VariableSize.CSV. Only called from LoadData().
- With LoadData() removed, this method has zero callers and is dead code.
- YAML (F558 VariableSizeService) is the SSOT for variable sizes; this CSV-based method is obsolete.
- Verification: No occurrence of `loadVariableSizeData` anywhere in the file.

**AC#3: changeVariableSizeData() removed**
- Private method (~235 lines) that parses VariableSize.CSV entries.
- Only called from loadVariableSizeData(). Cascading dead code.
- Verification: No occurrence of `changeVariableSizeData` anywhere in the file.

**AC#4: decideActualArraySize methods removed**
- Two private methods: `decideActualArraySize()` and `_decideActualArraySize_sub()`.
- Only called from loadVariableSizeData(). Cascading dead code.
- Contains complex PALAM/JUEL/CDFLAG size reconciliation logic that is entirely superseded by YAML-based sizing.
- Verification: No occurrence of `decideActualArraySize` anywhere in the file.

**AC#5: changedCode field removed**
- `readonly HashSet<VariableCode> changedCode` is used exclusively by changeVariableSizeData() and decideActualArraySize().
- With all consuming methods removed, the field is dead.
- Verification: No occurrence of `changedCode` anywhere in the file.

**AC#6: Engine builds successfully with zero warnings**
- TreatWarningsAsErrors=true is enforced by Directory.Build.props (F708).
- Removal of dead code must not introduce compilation errors (e.g., removing methods still referenced elsewhere).
- The zero-caller status of all removed elements was confirmed by codebase Grep.
- Verification: `dotnet build engine/uEmuera.Headless.csproj` exits with code 0.

**AC#7: AllocateDataArrays guard field preserved**
- The `dataArraysAllocated` guard field (added by F710) must not be accidentally removed during dead code cleanup.
- This is a regression guard for F710's work.
- Verification: `dataArraysAllocated` string exists in ConstantData.cs.

### Goal Coverage Verification

| Goal# | Goal Item | Covering ACs | Coverage Type |
|:-----:|-----------|:------------:|---------------|
| 1 | Remove unreachable LoadData() method | AC#1 | LoadData method deleted |
| 2 | Remove all cascading dead code (loadVariableSizeData, helper methods, changedCode) | AC#2, AC#3, AC#4, AC#5 | All orphaned private methods and field removed |
| 3 | Build succeeds with zero warnings | AC#6 | Build verification |
| 4 | Preserve F710 artifacts (dataArraysAllocated guard) | AC#7 | Regression guard for AllocateDataArrays |

All Goal items are covered. No gaps detected.

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This is a pure dead code removal task with no behavioral changes. All removed elements have zero external callers and were superseded by F558/F710/F711.

**Removal Strategy**: Delete six elements in dependency order from leaf to root.

1. **Method removal (5 methods)**:
   - `LoadData()` (lines 631-694, 64 lines) - Dead entry point with zero callers
   - `loadVariableSizeData()` (lines 237-274, 38 lines) - Only caller is LoadData line 634
   - `changeVariableSizeData()` (lines 277-511, 235 lines) - Only caller is loadVariableSizeData line 256
   - `decideActualArraySize()` (lines 537-628, 92 lines) - Only caller is loadVariableSizeData line 273
   - `_decideActualArraySize_sub()` (lines 513-535, 23 lines) - Only caller is decideActualArraySize (called 19 times)

2. **Field removal (1 field)**:
   - `changedCode` (line 79, `readonly HashSet<VariableCode>`) - Used exclusively by the five removed methods (13 references total: lines 509, 510, 517, 531, 533, 563, 564, 568, 586, 599, 604, 610, 621, 627)

**Total deletion**: ~452 lines of dead code.

**No additions**: This feature only removes code. No new code is added.

### AC Coverage

| AC# | Verification Method | Design Rationale |
|:---:|---------------------|------------------|
| 1 | Grep `public\s+void\s+LoadData\(` returns zero matches | Delete `LoadData()` method (lines 631-694). This is the root of the dead code tree. |
| 2 | Grep `loadVariableSizeData` returns zero matches | Delete `loadVariableSizeData()` method. Only caller is LoadData. |
| 3 | Grep `changeVariableSizeData` returns zero matches | Delete `changeVariableSizeData()` method. Only caller is loadVariableSizeData. |
| 4 | Grep `decideActualArraySize` returns zero matches | Delete both `decideActualArraySize()` and `_decideActualArraySize_sub()` methods. Only caller is loadVariableSizeData. |
| 5 | Grep `changedCode` returns zero matches | Delete `changedCode` field. All references are within removed methods. |
| 6 | Build succeeds with exit code 0 | TreatWarningsAsErrors=true ensures zero warnings. All removed elements have zero external callers. |
| 7 | Grep `dataArraysAllocated` matches (guard field from F710) | No changes to `dataArraysAllocated` field. Regression guard for F710. |

### Key Decisions

**Decision 1: Complete removal vs. [Obsolete] marking**
- **Choice**: Complete removal (delete all 6 elements)
- **Rationale**:
  - Zero callers confirmed by Grep (LoadData has no invocations in entire codebase)
  - All three concerns superseded by F558/F710/F711 (size reading, allocation, data population)
  - Zero Debt Upfront principle: dead code is maintenance burden
  - `AllocateDataArrays()` double-call guard (line 202) prevents LoadData from working correctly anyway (would throw InvalidOperationException if called after AllocateDataArrays)

**Decision 2: Removal order**
- **Choice**: Sequential deletion of all 6 elements, build verification after all deletions complete
- **Rationale**:
  - Each element can be deleted independently since all are dead code with zero callers
  - Build verification after completion confirms no dangling references
  - All ACs verified after the full deletion sequence

**Decision 3: Handling changedCode field**
- **Choice**: Remove the field entirely (line 79)
- **Rationale**:
  - All 13 references to `changedCode` are within the five removed methods
  - No external usage exists (confirmed by manual inspection: lines 509, 510, 517, 531, 533, 563, 564, 568, 586, 599, 604, 610, 621, 627 are all within removed method bodies)
  - Keeping an unused field violates Zero Debt Upfront principle

**Decision 4: Preservation of F710 artifacts**
- **Choice**: Preserve `AllocateDataArrays()` and `dataArraysAllocated` guard field
- **Rationale**:
  - AllocateDataArrays() is the SSOT replacement for allocation concern
  - AC#7 explicitly verifies preservation
  - Removal would break the YAML-based initialization chain

**Implementation Note**: The removal will be performed via sequential Edit operations, deleting each dead code element. Build verification after all deletions are complete.

<!-- fc-phase-5-completed -->
## Tasks

| T# | Description | ACs | Est | Status |
|:--:|-------------|-----|:---:|:------:|
| 1 | Remove dead code: Delete LoadData(), loadVariableSizeData(), changeVariableSizeData(), decideActualArraySize(), _decideActualArraySize_sub() methods and changedCode field from ConstantData.cs | AC#1, AC#2, AC#3, AC#4, AC#5 | M | [x] |
| 2 | Verify build succeeds with zero warnings and AllocateDataArrays guard field preserved | AC#6, AC#7 | S | [x] |

### Implementation Contract

**Scope Boundaries**:
- **IN SCOPE**: Removal of 6 dead code elements from ConstantData.cs (LoadData method, 4 private helper methods, 1 field) totaling ~452 lines
- **OUT OF SCOPE**: Any modifications to AllocateDataArrays() or its guard field (this is F710 artifact that must be preserved)
- **OUT OF SCOPE**: Changes to any files other than ConstantData.cs
- **OUT OF SCOPE**: Refactoring or optimization of remaining code (this is pure deletion only)

**Sequential Deletion**:
- Sequential deletion of 6 dead code elements. Build verification after all deletions complete.

**Regression Guards**:
- dataArraysAllocated field must remain untouched (AC#7)
- This is verified by AC test to prevent accidental deletion of F710 work

**Zero Behavioral Change**:
- All removed elements have zero callers (confirmed by F712 investigation phase)
- This is pure dead code removal with no runtime behavior changes
- TreatWarningsAsErrors=true (F708) ensures build verification catches any missed references