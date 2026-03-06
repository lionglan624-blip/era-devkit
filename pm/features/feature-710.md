# Feature 710: Fix Engine VariableData.SetDefaultValue ArgumentNullException

## Status: [DONE]

## Scope Discipline

**Out-of-scope issues must be tracked concretely:**

| Option | Destination | Validation |
|:------:|-------------|------------|
| A | `F{ID}` (new) | **DRAFT file exists** OR Creation Task exists |
| B | `F{ID}#T{N}` (existing) | Referenced Feature exists |
| C | `Phase N` | Phase exists in architecture.md |

**Handoff Protocol:** Choose ONE option, add to Mandatory Handoffs table, ensure actionable Task exists.

## Type: engine

## Background

### Philosophy (Mid-term Vision)

Correct initialization order - restore data array allocation that was lost during YAML migration (F575), maintaining YAML as the single source of truth for variable sizes while ensuring all required runtime data structures are allocated (non-null). Zero regression: the fix must work across all execution modes (GUI, headless, --unit) without re-introducing CSV-based size overrides.

### Problem (Current Issue)
The engine's `--unit` mode (kojo test execution) crashes with `ArgumentNullException` in `VariableData.SetDefaultValue(ConstantData)` during `Process.Initialize()`. This prevents any kojo test case from executing successfully.

**Error trace:**
```
System.ArgumentNullException: Value cannot be null. (Parameter 'src')
   at System.Buffer.BlockCopy(Array src, ...)
   at VariableData.SetDefaultValue(ConstantData constant) [VariableData.cs:547]
   at VariableData..ctor(GameBase gamebase, ConstantData constant) [VariableData.cs:142]
   at VariableEvaluator..ctor(GameBase gamebase, ConstantData constant) [VariableEvaluator.cs:33]
   at Process.Initialize() [Process.cs:96]
[KojoTest] Game data not initialized
```

**Reproduction:** Any `--unit` invocation fails:
```bash
cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit <any.json> --output-mode json
```

### Impact
- Blocks F706 AC5/7 (KojoComparer batch equivalence verification, 650/650 PASS)
- Blocks F706 AC3a/3b (integration tests requiring headless subprocess)
- All kojo test execution via `--unit` mode is broken

### Goal (What to Achieve)

1. Fix the ArgumentNullException crash in all modes (GUI, headless, --unit) by restoring data array allocation
2. Preserve YAML as SSOT for variable sizes (do not re-read sizes from CSV)
3. Restore data array allocation (not population) for ItemPrice, names[], nameToIntDics[] that LoadData() previously provided
4. Ensure zero technical debt in changed files

**Note**: This fix addresses the crash (null reference) only, not data population. Runtime name resolution will return empty/null strings until CSV data loading is restored (F711).

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: `VariableData.SetDefaultValue()` throws `ArgumentNullException` at line 547 when calling `Buffer.BlockCopy(constant.ItemPrice, ...)`
2. Why: `constant.ItemPrice` is `null` because the `ItemPrice` field (declared at `ConstantData.cs:101`) is never allocated
3. Why: `ItemPrice` was previously allocated inside `ConstantData.LoadData()` (line 596: `ItemPrice = new Int64[MaxDataList[itemIndex]]`) which also loaded CSV files
4. Why: F575 (commit `d671fa4`) removed the `constant.LoadData()` call from `ProcessInitializer.LoadConstantData()` to make YAML the single source of truth for VariableSize/GameBase
5. Why: F575 removed CSV loading but did not provide an alternative initialization for `ItemPrice`, `names[]` arrays, and other data structures that `LoadData()` populated beyond just variable sizes

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `ArgumentNullException` in `VariableData.SetDefaultValue()` on `Buffer.BlockCopy(constant.ItemPrice, ...)` | F575 removed `ConstantData.LoadData()` call without replacing the array/data initialization it provided (`ItemPrice`, `names[]`, `nameToIntDics[]`) |

### Conclusion

F575 (commit `d671fa4`, "Remove CSV loading for VariableSize/GameBase, make YAML fatal") removed the `constant.LoadData()` call from `ProcessInitializer.LoadConstantData()`. This was intended to make YAML the single source of truth for variable sizes. However, `LoadData()` was responsible for more than just applying variable sizes -- it also:

1. **Allocated `ItemPrice`** (line 596): `ItemPrice = new Int64[MaxDataList[itemIndex]]`
2. **Allocated `names[]` arrays** (line 593): `names[i] = new string[MaxDataList[i]]` for all CSV name lists
3. **Allocated `nameToIntDics[]`** (line 594): `nameToIntDics[i] = new Dictionary<string, int>()` for reverse lookups
4. **Loaded CSV data** into these arrays (ITEM.CSV prices, ABL/EXP/TALENT names, etc.)

The `VariableSizeService` (F558) only populates size metadata (`MaxDataList`, `*ArrayLength` fields) -- it does not allocate the actual data arrays. When `VariableData` constructor calls `SetDefaultValue(constant)`, it tries to `BlockCopy` from `constant.ItemPrice` which is still `null`.

**This bug affects ALL modes** (GUI, headless, --unit) since they all share the same `Process.Initialize()` -> `ProcessInitializer.LoadConstantData()` path. The error message `[KojoTest] Game data not initialized` in the trace is the `KojoTestRunner` detecting that initialization failed (because `Process.Initialize()` returned `false` after the exception).

**Multiple null references exist beyond `ItemPrice`:**
- `constant.ItemPrice` (line 547): `Buffer.BlockCopy` source is null -- **first crash point**
- `constant.GetCsvNameList(VariableCode.__DUMMY_STR__)` (line 567): returns `names[index]` which is null
- `constant.ItemPrice` (line 253): passed to `Int1DConstantToken` constructor as null

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F558 | [DONE] | Root cause precursor | Engine Integration Services for Critical Config - introduced VariableSizeService YAML-based initialization |
| F575 | [DONE] | Direct cause | Remove CSV loading for VariableSize/GameBase - removed `constant.LoadData()` without replacing data array initialization |
| F592 | [DONE] | Related | Fatal error exit handling for YAML failures - added fatal error handling to the same code path |
| F706 | [BLOCKED] | Blocked by this | KojoComparer Full Equivalence Verification - cannot run --unit tests |

### Pattern Analysis

This is a **migration gap** pattern: F558 introduced YAML-based size configuration, F575 removed CSV loading to complete the YAML migration, but the removal was overly aggressive. `LoadData()` served two purposes: (1) applying variable sizes from CSV (replaced by YAML) and (2) allocating/populating data arrays (`ItemPrice`, `names[]`, etc.). Only purpose (1) was replaced; purpose (2) was lost.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | The fix requires restoring data array allocation that was removed. Multiple valid approaches exist. |
| Scope is realistic | YES | Core fix is in 1-2 files (`ProcessInitializer.cs` and/or `ConstantData.cs`). ~50 lines of change. |
| No blocking constraints | YES | No external dependencies or blocking prerequisites. |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Successor | F706 | [BLOCKED] | KojoComparer Full Equivalence Verification |
| Predecessor | F558 | [DONE] | Introduced VariableSizeService; this feature fixes gap left by its successor F575 |
| Predecessor | F575 | [DONE] | Direct cause - removed LoadData() without replacing data initialization |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| CSV files (ITEM.CSV, ABL.CSV, etc.) | Runtime data | Low | Still present in Game/CSV/; only the loading call was removed |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `engine/.../Variable/VariableData.cs:547` | CRITICAL | `SetDefaultValue()` uses `constant.ItemPrice` for BlockCopy |
| `engine/.../Variable/VariableData.cs:253` | CRITICAL | Constructor passes `constant.ItemPrice` to `Int1DConstantToken` |
| `engine/.../Variable/VariableData.cs:567` | CRITICAL | `SetDefaultValue()` uses `constant.GetCsvNameList()` which returns null `names[]` elements |
| `engine/.../Variable/VariableEvaluator.cs:1840` | HIGH | `ItemPrice` property access |
| `engine/.../Variable/VariableEvaluator.cs:2694` | HIGH | `ItemPrice` property getter |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` | Update | Restore data array initialization (ItemPrice, names[], nameToIntDics[]) after YAML size initialization |
| `engine/Assets/Scripts/Emuera/GameData/ConstantData.cs` | Update | Add AllocateDataArrays() public method - becomes part of initialization contract |
| `engine/Assets/Scripts/Emuera/GameData/ConstantData.cs (LoadData)` | Future Impact | LoadData() has partial overlap with AllocateDataArrays() - future changes to LoadData() must account for this |

**Note**: AllocateDataArrays() becomes a required step in the initialization sequence. All callers of Process.Initialize() are impacted consumers.

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Cannot simply restore full `LoadData()` call | F575 design intent: YAML is SSOT for sizes | MEDIUM - Must selectively restore data allocation without re-reading VariableSize from CSV |
| `ItemPrice` requires `MaxDataList[itemIndex]` to be set first | Initialization order | LOW - VariableSizeService already sets MaxDataList before this point |
| `names[]` array allocation requires `MaxDataList` values | Initialization order | LOW - Same as above |
| Engine is a separate git repo | Repository structure | LOW - Changes must be committed in engine repo |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Partial fix misses some null arrays | Medium | High | Systematically audit all fields populated by LoadData() vs VariableSizeService |
| Re-introducing CSV reading defeats F575 purpose | Low | Medium | Only restore allocation/initialization, not CSV-based size overrides |
| names[] data (character names, item names) missing at runtime | Medium | High | May need to restore CSV name loading separately from size loading |

<!-- fc-phase-3-completed -->
<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Option A: Add `AllocateDataArrays()` method to ConstantData**

Extract the data array allocation logic (ItemPrice, names[], nameToIntDics[]) from `LoadData()` into a new public method `AllocateDataArrays()` that ProcessInitializer calls after YAML initialization. This approach:

1. Respects encapsulation - `names` and `nameToIntDics` are `private readonly` fields (ConstantData.cs:93-94)
2. Provides a clean API for allocation-only operation (no CSV reading)
3. Reuses the exact allocation pattern from LoadData() lines 591-596
4. Maintains YAML as SSOT - allocation uses `MaxDataList` already populated by VariableSizeService

**Rejected alternatives:**

- **Option B** (Restore partial LoadData() call): Would require splitting LoadData() into multiple methods or adding boolean flags to skip CSV reading. Too invasive for a bug fix.
- **Option C** (Direct allocation in ProcessInitializer): Impossible - `names[]` and `nameToIntDics[]` are `private readonly` fields, not accessible outside ConstantData.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Engine build succeeds after adding AllocateDataArrays() method and call site |
| 2 | Engine unit tests will pass with allocated arrays (no null reference crashes) |
| 3 | Headless mode will initialize successfully with allocated arrays |
| 4 | --unit mode will execute test cases without ArgumentNullException |
| 5 | AllocateDataArrays method exists |
| 6 | AllocateDataArrays called from ProcessInitializer |
| 7 | AllocateDataArrays contains ItemPrice allocation |
| 8 | YAML SSOT preserved - no CSV size reading restored |
| 9 | Zero technical debt in changed files |
| 10 | AllocateDataArrays contains names[] allocation |
| 11 | AllocateDataArrays contains nameToIntDics[] allocation |
| 12 | AllocateDataArrays contains guard clause |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where to place allocation logic | A: New ConstantData method, B: Partial LoadData(), C: Direct in ProcessInitializer | A | `names[]` and `nameToIntDics[]` are private readonly - must use ConstantData method |
| When to allocate | Before YAML init, After YAML init | After YAML init | MaxDataList must be populated from YAML first (VariableSizeService sets these values) |
| Method visibility | public, internal | public | ProcessInitializer is in different namespace (MinorShift.Emuera.Sub) |

### Implementation Details

**File 1: ConstantData.cs** - Add new method after `setDefaultArrayLength()` (around line 192):

```csharp
/// <summary>
/// Allocate data arrays (ItemPrice, names, nameToIntDics) based on MaxDataList sizes.
/// Must be called exactly once after MaxDataList is populated (e.g., by VariableSizeService).
/// </summary>
private bool dataArraysAllocated;

public void AllocateDataArrays()
{
    // Guard against multiple calls to prevent data loss
    if (dataArraysAllocated)
        throw new InvalidOperationException("AllocateDataArrays() can only be called once");

    dataArraysAllocated = true;

    // Allocate name and dictionary arrays for all CSV types
    for (int i = 0; i < countNameCsv; i++)
    {
        names[i] = new string[MaxDataList[i]];
        nameToIntDics[i] = new Dictionary<string, int>();
    }

    // Allocate ItemPrice array based on ITEM size
    ItemPrice = new Int64[MaxDataList[itemIndex]];
}
```

**File 2: ProcessInitializer.cs** - Add call after line 178 (after VariableSizeService.Initialize()):

```csharp
// After line 178: if (result is Result<Unit>.Failure rf) { ... }

// Allocate data arrays now that MaxDataList is populated from YAML (allocation separated from CSV loading)
constant.AllocateDataArrays();

// ... existing code continues ...
return constant;
```

**Future extensibility note**: When LoadData() is eventually restored (F711), it should be refactored to call AllocateDataArrays() for its allocation step (lines 591-596), so allocation logic exists in exactly one place.

**Why this satisfies AC#8 (YAML SSOT)**:
- We do NOT call `constant.LoadData()` which reads CSV files via `loadVariableSizeData()` (line 590)
- `AllocateDataArrays()` only allocates arrays - no CSV reading
- Allocation uses `MaxDataList` values already set by `VariableSizeService` from YAML

### Verification Strategy

| Test | What it verifies |
|------|------------------|
| AC#1 (build) | Code compiles with new method and call site |
| AC#2 (unit tests) | Engine unit tests pass without null reference errors |
| AC#3 (headless) | Process.Initialize() completes successfully in headless mode |
| AC#4 (--unit) | KojoTestRunner executes test cases without ArgumentNullException |
| AC#5-7 (Grep) | AllocateDataArrays() contains the three allocation patterns |
| AC#8 (Grep not_matches) | ProcessInitializer does not call constant.LoadData() |
| AC#9 (Grep not_matches) | No TODO/FIXME/HACK in changed files |

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "restore data array allocation" | ItemPrice, names[], nameToIntDics[] must be allocated after fix | AC#5, AC#6, AC#7 |
| "maintaining YAML as the single source of truth for variable sizes" | Fix must not call CSV-based size reading; VariableSizeService remains sole size provider | AC#8 |
| "all required runtime data structures are allocated (non-null)" | Engine starts without ArgumentNullException in all modes | AC#3, AC#4 |
| "all execution modes (GUI, headless, --unit)" | Headless and --unit verified; build covers GUI compilation | AC#1, AC#3, AC#4 |
| "zero technical debt" | No TODO/FIXME/HACK in changed files | AC#9 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Engine build succeeds | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 2 | Engine unit tests pass | test | dotnet test engine.Tests | succeeds | - | [x] |
| 3 | Headless mode initializes without crash | exit_code | cd Game && echo "" | dotnet run --project ../engine/uEmuera.Headless.csproj -- . | succeeds | - | [x] |
| 4 | Unit test mode executes successfully | exit_code | cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/unit-test-detection.json --output-mode json | succeeds | - | [x] |
| 5 | AllocateDataArrays method exists | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | matches | "public void AllocateDataArrays\\(\\)" | [x] |
| 6 | AllocateDataArrays called from ProcessInitializer | code | Grep(engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs) | matches | "\\.AllocateDataArrays\\(\\)" | [x] |
| 7 | AllocateDataArrays contains ItemPrice allocation | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | matches | "AllocateDataArrays[\\s\\S]*?ItemPrice\\s*=\\s*new" | [x] |
| 8 | YAML SSOT preserved - no CSV size reading restored | code | Grep(engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs) | not_matches | "constant\\.LoadData\\(\\)" | [x] |
| 9 | Zero technical debt in changed files | code | Grep(engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs,engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | not_matches | "TODO|FIXME|HACK" | [x] |
| 10 | AllocateDataArrays contains names[] allocation | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | matches | "AllocateDataArrays[\\s\\S]*?names\\[.*\\]\\s*=\\s*new\\s+string" | [x] |
| 11 | AllocateDataArrays contains nameToIntDics[] allocation | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | matches | "AllocateDataArrays[\\s\\S]*?nameToIntDics\\[.*\\]\\s*=\\s*new\\s+Dictionary" | [x] |
| 12 | AllocateDataArrays contains guard clause | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | matches | "AllocateDataArrays[\\s\\S]*?throw new InvalidOperationException" | [x] |

### AC Details

**AC#1: Engine build succeeds**
- Test: `dotnet build engine/uEmuera.Headless.csproj`
- Verifies the fix compiles without errors. Since GUI and headless share the same engine code, a successful headless build confirms GUI compilation as well.

**AC#2: Engine unit tests pass**
- Test: `dotnet test engine.Tests`
- Verifies no existing engine unit tests regress from the fix. This is critical because the fix touches core initialization code (`ProcessInitializer`, `ConstantData`).

**AC#3: Headless mode initializes without crash**
- Test: `cd Game && echo "" | dotnet run --project ../engine/uEmuera.Headless.csproj -- .`
- The headless mode must initialize without `ArgumentNullException`. Uses empty input pipe to cause immediate EOF and graceful exit.
- Expected: Exit code 0 (successful initialization without crash).

**AC#4: Unit test mode executes successfully**
- Test: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/unit-test-detection.json --output-mode json`
- The `--unit` mode (which triggered the original bug report) must execute a test case without crashing. This is the primary reproduction scenario.
- Expected: Exit code 0 with valid JSON output.
- Note: Uses existing `unit-test-detection.json` test file to verify the crash is fixed.

**AC#5: AllocateDataArrays method exists**
- Test: Grep pattern=`public void AllocateDataArrays\(\)` path=`engine/Assets/Scripts/Emuera/GameData/ConstantData.cs`
- Verifies the new AllocateDataArrays method was added to ConstantData.cs as specified in the technical design.

**AC#6: AllocateDataArrays called from ProcessInitializer**
- Test: Grep pattern=`\.AllocateDataArrays\(\)` path=`engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs`
- Verifies that ProcessInitializer calls the AllocateDataArrays method after VariableSizeService initialization.

**AC#7: AllocateDataArrays contains ItemPrice allocation**
- Test: Multiline grep to verify AllocateDataArrays method body contains ItemPrice allocation
- Confirms the method allocates ItemPrice array, addressing the root cause of the ArgumentNullException crash.

**AC#8: YAML SSOT preserved - no CSV size reading restored**
- Test: Grep pattern=`constant\.LoadData\(\)` path=`engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs`
- Expected: 0 matches. The full `LoadData()` call must NOT be restored because it re-reads variable sizes from CSV, which would conflict with YAML as SSOT (F575 design intent).
- The fix should allocate data arrays without calling `LoadData()`, either via a new method or inline allocation.

**AC#9: Zero technical debt in changed files**
- Test: Grep pattern=`TODO|FIXME|HACK` paths=`engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs`, `engine/Assets/Scripts/Emuera/GameData/ConstantData.cs`
- Expected: 0 matches across both files.
- Note: Checks both files in the Impact Analysis scope.

**AC#10: AllocateDataArrays contains names[] allocation**
- Test: Multiline grep to verify AllocateDataArrays method body contains names[] array allocation
- Confirms the method allocates names[] arrays for CSV name resolution, preventing null references in GetCsvNameList().

**AC#11: AllocateDataArrays contains nameToIntDics[] allocation**
- Test: Multiline grep to verify AllocateDataArrays method body contains nameToIntDics[] dictionary allocation
- Confirms the method allocates nameToIntDics[] dictionaries for reverse CSV name lookups.

**AC#12: AllocateDataArrays contains guard clause**
- Test: Multiline grep to verify AllocateDataArrays method body contains guard clause with InvalidOperationException
- Confirms the method prevents multiple calls to avoid data loss and maintains single-call semantics.

### Goal Coverage Verification

| Goal Item | Covering AC(s) |
|-----------|----------------|
| (1) Fix ArgumentNullException crash in all modes | AC#1 (build), AC#3 (headless), AC#4 (--unit) |
| (2) Preserve YAML as SSOT for variable sizes | AC#8 |
| (3) Restore data array initialization | AC#5, AC#6, AC#7, AC#10, AC#11 |
| (4) Zero technical debt | AC#9 |

## Review Notes

<!-- Use this section to track FL review findings -->
<!-- Format: - [pending/resolved-applied/resolved-invalid] {phase} iter{N}: {issue description} -->

## Links
- [feature-558.md](feature-558.md) - Engine Integration Services for Critical Config
- [feature-575.md](feature-575.md) - Remove CSV loading for VariableSize/GameBase
- [feature-592.md](feature-592.md) - Fatal error exit handling for YAML failures
- [feature-706.md](feature-706.md) - Blocked feature
- [feature-711.md](feature-711.md) - CSV Data Loading Restoration (Names/Prices)

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 5,7,10,11,12 | Add AllocateDataArrays() method to ConstantData.cs | [x] |
| 2 | 1,6,8 | Call AllocateDataArrays() from ProcessInitializer.cs | [x] |
| 3 | 2,3,4 | Run build, unit tests, and verify headless/--unit modes | [x] |
| 4 | 9 | Verify zero technical debt in changed files | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T2 | Technical Design code snippets | AllocateDataArrays() method + call site |
| 2 | ac-tester | haiku | T3-T4 | AC verification commands | Test results + code verification (Note: AC#3 PowerShell complexity may require manual verification if automated test fails) |

**Constraints** (from Technical Design):
1. Must allocate arrays AFTER VariableSizeService sets MaxDataList (initialization order dependency)
2. Cannot restore full LoadData() call - YAML must remain SSOT for variable sizes
3. AllocateDataArrays() must be public (called from different namespace)
4. names[] and nameToIntDics[] are private readonly - must use ConstantData method

**Pre-conditions**:
- Engine builds successfully before changes
- F558 VariableSizeService is functional (sets MaxDataList from YAML)
- F575 LoadData() call has been removed from ProcessInitializer

**Success Criteria**:
- All 12 ACs pass
- Headless mode starts without ArgumentNullException
- --unit mode executes test cases successfully
- No regression in engine unit tests

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| CSV name/price data loading | Only allocation, not population | Feature | F711 | Already exists (F711 [DRAFT]) |
| F711 registration in index-features | Feature creation workflow requirement | index-features.md | F711 entry | Already completed |
| LoadData() allocation refactoring | Prevent duplicate allocation logic when F711 restores CSV loading | Feature | F711 Task 1 | F711 Task 1: Modify LoadData() to use AllocateDataArrays() (already captured) |

**Code Snippets**:

### File 1: ConstantData.cs
Add method after `setDefaultArrayLength()` (around line 192):

```csharp
/// <summary>
/// Allocate data arrays (ItemPrice, names, nameToIntDics) based on MaxDataList sizes.
/// Must be called exactly once after MaxDataList is populated (e.g., by VariableSizeService).
/// </summary>
private bool dataArraysAllocated;

public void AllocateDataArrays()
{
    // Guard against multiple calls to prevent data loss
    if (dataArraysAllocated)
        throw new InvalidOperationException("AllocateDataArrays() can only be called once");

    dataArraysAllocated = true;

    // Allocate name and dictionary arrays for all CSV types
    for (int i = 0; i < countNameCsv; i++)
    {
        names[i] = new string[MaxDataList[i]];
        nameToIntDics[i] = new Dictionary<string, int>();
    }

    // Allocate ItemPrice array based on ITEM size
    ItemPrice = new Int64[MaxDataList[itemIndex]];
}
```

### File 2: ProcessInitializer.cs
Add call after line 178 (after VariableSizeService.Initialize()):

```csharp
// After line 178: if (result is Result<Unit>.Failure rf) { ... }

// Allocate data arrays now that MaxDataList is populated from YAML (allocation separated from CSV loading)
constant.AllocateDataArrays();

// ... existing code continues ...
return constant;
```

**Why YAML SSOT is preserved** (AC#8):
- We do NOT call `constant.LoadData()` which reads CSV files
- AllocateDataArrays() only allocates arrays - no CSV reading
- Allocation uses MaxDataList values already set by VariableSizeService from YAML

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| CSV name/price data loading - LoadData() also populated names[] with ABL/EXP/TALENT names and ItemPrice with prices from CSV files | F711 (create) | AllocateDataArrays() only allocates empty arrays but runtime may need actual CSV data for names/prices |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 16:03 | Task 1-2 | implementer: Added AllocateDataArrays() method and call site. Build succeeded. |
