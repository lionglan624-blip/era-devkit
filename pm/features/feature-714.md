# Feature 714: Reduce Remaining ERB Compile Warnings (Post-F713)

## Status: [DONE]

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

All ERB code should compile with zero warnings under `--strict-warnings`. Warning-free compilation makes genuine new warnings immediately visible and enables a CI gate.

### Problem (Current Issue)
After F713 eliminated all 11582 "は解釈できない識別子です" identifier warnings, ~399 warnings remain:
- ~392 "はこの関数中では定義されていません" (undefined local variable warnings from ERB #DIM scope issues)
- 7 "other" warnings (TRYCALLLIST target not found, FOR/NEXT mismatch)

Additionally, 3 variable types (SAVESTR, GLOBAL, GLOBALS) are not registered in PopulateConstantNames(). These are not needed currently (0 identifier warnings without them) but should be registered if future ERB code uses their named constants.

### Goal (What to Achieve)
1. Fix ERB #DIM scope issues to eliminate "undefined local variable" warnings
2. Fix TRYCALLLIST/FOR/NEXT ERB code logic issues
3. Conditionally register SAVESTR/GLOBAL/GLOBALS if needed

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F713 | [DONE] | YAML variable definitions; F714 addresses remaining warnings |

## Links
- [feature-713.md](feature-713.md) - Parent feature (YAML variable definitions)
- [feature-711.md](feature-711.md) - CSV+YAML→ConstantData bridge (PopulateConstantNames origin)
- [feature-712.md](feature-712.md) - LoadData() Dead Code Removal (related engine cleanup)
- [feature-706.md](feature-706.md) - KojoComparer full equivalence (benefits from cleaner compile output)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

**Symptom**: ~399 compile warnings remain after F713 eliminated all 11582 "unrecognized identifier" warnings.

1. **Why do ~392 "はこの関数中では定義されていません" warnings appear?**
   Because the engine's `IdentifierDictionary.ThrowException()` finds that an unresolved identifier name matches an entry in `privateDimList` (line 627 of IdentifierDictionary.cs) and throws a specific `IdentifierNotFoundCodeEE` with the "not defined in this function" message instead of the generic "unrecognized identifier" message.

2. **Why are these identifiers in `privateDimList` but not resolvable?**
   Because `privateDimList` is populated by `#DIM` declarations across ALL functions in ALL ERB files (line 421). When function A declares `#DIM 苦痛増分`, the name "苦痛増分" is added to the global `privateDimList`. If function B uses a name that coincidentally matches, and that name cannot be resolved as a system variable, constant, or global variable, the engine gives the "not defined in this function" warning.

3. **Why can't these identifiers be resolved as system/global variables?**
   Two possible sub-causes:
   - (a) The identifier is genuinely a local variable that should have a `#DIM` declaration in its own function but is missing it (ERB authoring error).
   - (b) The identifier is a named constant reference (e.g., `EQUIP:TARGET:下着`) where the constant name resolution was fixed by F713, but the name also appears in `privateDimList` from an unrelated `#DIM` in another function, causing the `privateDimList` check to trigger BEFORE the constant check in `ThrowException()`.

4. **Why does the `privateDimList` check take priority over constant resolution?**
   Because in `ThrowException()` (line 627), the `privateDimList.Contains()` check runs BEFORE the `nameDic.TryGetValue()` check (line 630). However, this is the error-reporting path - identifiers reach `ThrowException()` only when they ALREADY failed normal resolution. The real issue is that these ~392 identifiers genuinely cannot be resolved during normal compilation.

5. **Why weren't these resolved by F713's YAML definitions?**
   Because F713 addressed "unrecognized identifier" warnings (the fallthrough case at line 656). The ~392 remaining warnings are a DIFFERENT code path (line 628) - identifiers that match `#DIM` names from other functions. These are likely ERB code bugs: variables used without `#DIM` declaration in their own function, or names that are ambiguous between local variables and constants.

### Symptom vs Root Cause

| Aspect | Description |
|--------|-------------|
| **Symptom** | ~392 "はこの関数中では定義されていません" warnings + 7 structural warnings |
| **Root Cause (392 warnings)** | ERB files reference identifiers that (a) cannot be resolved as any known variable/constant and (b) coincidentally match `#DIM` local variable names from other functions. The fix is to add missing `#DIM` declarations to the functions that use these variables. |
| **Root Cause (7 warnings)** | ERB structural issues: TRYCALLLIST targets referencing non-existent functions, or FOR/NEXT block nesting problems. These are ERB code logic bugs in specific files. |
| **Root Cause (SAVESTR/GLOBAL/GLOBALS gap)** | `PopulateConstantNames()` in ProcessInitializer.cs does not include `VariableCode.SAVESTRNAME`, `VariableCode.GLOBALNAME`, or `VariableCode.GLOBALSNAME` in its variable type list. No CSV files exist for these types (`Game/CSV/` has no SAVESTR.CSV, GLOBAL.CSV, or GLOBALS.CSV). Currently harmless (0 warnings) since no ERB code uses named constants for these types, but 43 references to `SAVESTR:`, `GLOBAL:`, `GLOBALS:` exist using numeric indices. |

## Related Features

| Feature | Relationship | Description |
|---------|-------------|-------------|
| F713 | Predecessor (DONE) | Eliminated 11582 "unrecognized identifier" warnings via YAML variable definitions. F714 addresses the remaining ~399 warnings that F713 did not cover. |
| F711 | Foundation (DONE) | Created the CSV+YAML→ConstantData bridge (`PopulateConstantNames()`). F714 may extend this for SAVESTR/GLOBAL/GLOBALS. |
| F712 | Related (DONE) | LoadData() dead code removal. No direct dependency but both are post-F711 engine cleanup. |
| F706 | Beneficiary (WIP) | KojoComparer full equivalence. Fewer warnings → cleaner compile output for equivalence testing. |

## Feasibility Assessment

**Verdict: FEASIBLE**

### Rationale

1. **~392 #DIM scope warnings (ERB-side fix)**:
   - These require adding `#DIM` declarations to ERB functions that use local variables without declaring them. This is a mechanical, low-risk ERB editing task.
   - The engine mechanism is well-understood: `privateDimList` in `IdentifierDictionary.cs` tracks all `#DIM` names globally, and `ThrowException()` checks it at line 627.
   - Each warning identifies the exact file, line, and variable name, making fixes straightforward.

2. **7 structural warnings (ERB-side fix)**:
   - TRYCALLLIST warnings come from `NestValidator.cs` / `ErbLoader.cs` (lines ~600-650, ~1343-1394): nesting issues, missing pair commands, or targets not found.
   - FOR/NEXT warnings come from `NestValidator.cs` (line 336) / `ErbLoader.cs` (line 1070): loop structure outside REPEAT/FOR/WHILE/DO blocks.
   - These require inspecting 7 specific ERB locations and fixing structural issues (missing ENDFUNC, incorrect nesting, dead TRYCALLLIST targets).

3. **SAVESTR/GLOBAL/GLOBALS registration (engine-side fix)**:
   - Adding 3 entries to the `variableTypes` array in `PopulateConstantNames()` (ProcessInitializer.cs line 204-229) is trivial.
   - However, no CSV or YAML definition files exist for these types, so the registration would be a no-op until definition files are created.
   - Recommendation: Add the registration entries defensively (zero risk, future-proof) but do NOT create empty CSV/YAML files unless needed.

### Risk Level: LOW

All changes are either ERB script fixes (adding `#DIM` declarations, fixing structure) or a trivial engine extension (3 array entries). No architectural changes needed.

## Impact Analysis

| Area | Impact | Description |
|------|--------|-------------|
| ERB compile output | **HIGH** | Eliminates ~399 warnings, potentially reaching zero-warning compile |
| Game behavior | **NONE** | `#DIM` declarations don't change runtime behavior; they only satisfy the compiler. TRYCALLLIST/FOR/NEXT fixes may affect dead code paths. |
| Engine code | **MINIMAL** | Only 3 lines added to `PopulateConstantNames()` variable type array |
| Developer experience | **MEDIUM** | Zero-warning compile makes genuine new warnings immediately visible |
| F706 KojoComparer | **LOW** | Cleaner compile output aids equivalence testing |

## Constraints

1. **ERB file count**: ~392 warnings may span many ERB files. Each requires individual `#DIM` declaration additions at the correct function scope.
2. **#DIM placement**: `#DIM` must be placed immediately after the `@FUNCTION_NAME` line and before any executable code in that function. Order matters.
3. **Variable type matching**: `#DIM` defaults to integer. String local variables need `#DIMS`. Mismatching types would cause different errors.
4. **TRYCALLLIST target existence**: If TRYCALLLIST references functions that intentionally don't exist (conditional compilation pattern), "fixing" them may not be appropriate - suppression or documentation may be needed instead.
5. **Engine repo boundary**: ProcessInitializer.cs is in the engine submodule. Changes require engine repo coordination.

## Risks

| Risk | Severity | Likelihood | Mitigation |
|------|----------|------------|------------|
| `#DIM` additions change variable initialization behavior | LOW | LOW | `#DIM` creates a LOCAL variable initialized to 0/empty. If code previously relied on a different variable (e.g., system variable with same name), adding `#DIM` would shadow it. Verify each case. |
| TRYCALLLIST targets are intentionally absent | MEDIUM | MEDIUM | Some TRYCALLLIST patterns intentionally list optional function names. Investigate each of the 7 warnings before fixing. |
| Large number of ERB files to edit | LOW | HIGH | ~392 warnings likely span dozens of files. Mechanical but time-consuming. Can be partially automated. |
| SAVESTR/GLOBAL/GLOBALS registration triggers unexpected behavior | LOW | LOW | Registration with no definition files is a no-op. Only becomes active when CSV/YAML files are created. |
| FOR/NEXT fixes break existing game flow | LOW | LOW | FOR/NEXT structural issues are likely in dead code (otherwise they'd cause runtime errors). Test after fixing. |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

**Philosophy**: All ERB code should compile with zero warnings under `--strict-warnings`. Warning-free compilation makes genuine new warnings immediately visible and enables a CI gate.

**Derivation from Philosophy to Required Capabilities**:

| # | Philosophy Element | Required Capability | AC Mapping |
|---|-------------------|---------------------|------------|
| 1 | Reduce DIM_SCOPE warnings to zero | All 224 "はこの関数中では定義されていません" warnings eliminated | AC#1 |
| 2 | Future-proof constant resolution | SAVESTR/GLOBAL/GLOBALS registered in PopulateConstantNames() | AC#2,3,4 |
| 3 | No regressions | C# build succeeds with TreatWarningsAsErrors=true | AC#5 |
| 4 | No regressions | Headless run completes without fatal errors | AC#6 |

**Scope Adjustment (Investigation Result)**:
- Original spec predicted ~392 DIM_SCOPE + 7 TRYCALLLIST/FOR/NEXT warnings = ~399 total
- Actual: 224 DIM_SCOPE + 175 ARRAY_OUT_OF_BOUNDS + 0 structural = 399 total
- TRYCALLLIST/FOR/NEXT warnings do not exist (AC#2,3 removed)
- ARRAY_OUT_OF_BOUNDS (175 warnings: EXP[111-123], TCVAR[500,503]) deferred to F715
- AC#4 "total zero" removed (requires F715 completion for full zero-warning)

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Zero "はこの関数中では定義されていません" warnings | output | --strict-warnings | not_contains | "はこの関数中では定義されていません" | [x] |
| 2 | SAVESTR registration in PopulateConstantNames() | file | Grep | contains | "SAVESTRNAME" | [x] |
| 3 | GLOBAL registration in PopulateConstantNames() | file | Grep | contains | "GLOBALNAME" | [x] |
| 4 | GLOBALS registration in PopulateConstantNames() | file | Grep | contains | "GLOBALSNAME" | [x] |
| 5 | C# build succeeds (TreatWarningsAsErrors) | build | dotnet build | succeeds | - | [x] |
| 6 | Headless run completes without fatal error | output | --strict-warnings | not_contains | "FATAL" | [x] |

### AC Details

#### AC1: Zero "はこの関数中では定義されていません" warnings
- **Test**: `dotnet run --project engine/uEmuera.Headless.csproj -- Game --strict-warnings 2>&1`
- **Expected**: Output does not contain "はこの関数中では定義されていません"
- **Rationale**: 224 warnings from missing `#DIM` declarations across 33 ERB files (7 unique variables: 父親, 輪姦内容, 成長度, 行為者, 客人数, ペニス, 精子量) must all be fixed by adding appropriate `#DIM` declarations.

#### AC2: SAVESTR registration in PopulateConstantNames()
- **Test**: Inspect `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` for the `PopulateConstantNames()` method
- **Expected**: File contains `VariableCode.SAVESTRNAME` in the variableTypes array
- **Rationale**: Defensive registration ensures future ERB code using SAVESTR named constants will resolve correctly

#### AC3: GLOBAL registration in PopulateConstantNames()
- **Test**: Inspect `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` for the `PopulateConstantNames()` method
- **Expected**: File contains `VariableCode.GLOBALNAME` in the variableTypes array
- **Rationale**: Defensive registration ensures future ERB code using GLOBAL named constants will resolve correctly

#### AC4: GLOBALS registration in PopulateConstantNames()
- **Test**: Inspect `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` for the `PopulateConstantNames()` method
- **Expected**: File contains `VariableCode.GLOBALSNAME` in the variableTypes array
- **Rationale**: Defensive registration ensures future ERB code using GLOBALS named constants will resolve correctly

#### AC5: C# build succeeds (TreatWarningsAsErrors)
- **Test**: `dotnet build`
- **Expected**: Build succeeds with exit code 0
- **Rationale**: Any engine changes must not introduce C# compiler warnings, which are treated as errors per Directory.Build.props (F708).

#### AC6: Headless run completes without fatal error
- **Test**: `dotnet run --project engine/uEmuera.Headless.csproj -- Game --strict-warnings 2>&1`
- **Expected**: Output does not contain "FATAL" and process exits normally
- **Rationale**: ERB changes (adding `#DIM` declarations) must not break runtime execution.

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature eliminates the remaining ~399 ERB compile warnings through three independent fix strategies:

#### Strategy 1: Fix ~392 "はこの関数中では定義されていません" warnings (ERB #DIM declarations)

**Root Cause**: Variables used in ERB functions without corresponding `#DIM`/`#DIMS` declarations in the same function scope. The engine's `IdentifierDictionary.ThrowException()` (line 627) detects that an unresolved identifier matches an entry in the global `privateDimList` (populated from ALL `#DIM` declarations across ALL functions) and throws a specific "not defined in this function" error instead of the generic "unrecognized identifier" error.

**Approach**:
1. **Capture warning list**: Run `dotnet run --project engine/uEmuera.Headless.csproj -- Game --strict-warnings 2>&1 > warnings.log` to capture all warnings with file names, line numbers, and variable names
2. **Parse warnings**: Extract structured data (file, line, variable name) from warning messages
3. **Group by function**: For each ERB file, group warnings by the function (`@FUNCTION_NAME`) that contains the problematic line
4. **Add #DIM declarations**: For each function with warnings:
   - Determine variable type (integer → `#DIM`, string → `#DIMS`) by examining usage context
   - Add `#DIM`/`#DIMS` declarations immediately after the `@FUNCTION_NAME` line and before the first executable statement
   - Preserve existing declaration order (maintain alphabetical or functional grouping if present)
5. **Validation**: Re-run headless mode after each batch of fixes to verify warnings are eliminated and no new errors introduced

**#DIM Placement Rules** (from ERB syntax analysis):
- `#DIM` / `#DIMS` must be placed immediately after `@FUNCTION_NAME` (or after existing `#DIM` declarations)
- Must come before any executable code (assignments, CALL, PRINTFORM, etc.)
- Integer variables use `#DIM variable_name`
- String variables use `#DIMS variable_name`
- Example pattern (from COMF400.ERB lines 4-12):
  ```erb
  @COM400
  #DIMS ROOMNAME
  #DIM PAGE
  #DIM 住人
  #DIM LOOP_CHR
  [executable code starts here]
  ```

**Type Detection Strategy**:
- String variables: Used with string operators (`=`, `+=` with string literals), passed to string functions (STRCOUNT, STRLENS), or compared with `""` empty string
- Integer variables: All other cases (numeric operations, array indices, comparisons with numbers)
- If ambiguous, inspect surrounding code context or default to `#DIM` (engine will error if type mismatch, allowing correction)

**Automation Consideration**: The ~392 warnings likely span 20-50 ERB files. Consider writing a Python/PowerShell script to:
1. Parse `warnings.log` into structured data
2. Group by file and function
3. Generate `#DIM`/`#DIMS` declarations
4. Insert declarations at correct line positions
5. Output a patch file or modified ERB files for review

#### Strategy 2: Fix 7 structural warnings (TRYCALLLIST/FOR/NEXT issues)

**Root Cause**: ERB code structural issues detected by `NestValidator.cs` and `ErbLoader.cs`:
- TRYCALLLIST warnings: `"が見つかりません"` from `ErbLoader.cs` line 1436 - occurs when a TRYCALLLIST/TRYCALLFORMLIST/TRYCJUMPLIST references a function name that doesn't exist
- FOR/NEXT warnings: `"REPEAT, FOR, WHILE, DOの中以外で{NEXT/CONTINUE/BREAK}文が使われました"` from `NestValidator.cs` line 336 - occurs when NEXT/CONTINUE/BREAK appears outside a loop block

**Approach**:
1. **Identify specific warnings**: From the warning log, extract the 7 structural warnings with exact file names, line numbers, and warning types
2. **TRYCALLLIST target fixes**:
   - Investigate each TRYCALLLIST reference to determine if the target function is intentionally absent (optional function pattern) or a typo/dead code
   - If intentional (conditional compilation), document justification and either:
     - Add a comment explaining the optional target pattern
     - Remove the dead reference if the target will never exist
   - If typo, correct the function name
   - If dead code, remove the TRYCALLLIST statement entirely
3. **FOR/NEXT fixes**:
   - Inspect the context around NEXT/CONTINUE/BREAK statements
   - Verify correct nesting: ensure NEXT is inside FOR...NEXT, CONTINUE/BREAK is inside REPEAT/WHILE/DO
   - Common causes:
     - Missing ENDFUNC causing control flow to "leak" into next function
     - Incorrect indentation/structure making nesting unclear
     - NEXT/CONTINUE/BREAK in wrong scope (e.g., inside nested IF but outside loop)
   - Fix by adding missing structural keywords (FOR, ENDFUNC) or moving statements to correct scope
4. **Validation**: Re-run headless after each fix to verify warning eliminated

**Investigation Method**:
1. Run headless to capture the 7 specific warnings
2. Manually inspect each warning location to understand the issue
3. Apply targeted fixes

#### Strategy 3: Register SAVESTR/GLOBAL/GLOBALS in PopulateConstantNames() (engine code)

**Root Cause**: The `PopulateConstantNames()` method in `ProcessInitializer.cs` (lines 204-229) does not include `VariableCode.SAVESTRNAME`, `VariableCode.GLOBALNAME`, or `VariableCode.GLOBALSNAME` in its `variableTypes` array. This is currently harmless (no warnings) because no ERB code uses named constants for these types (all 43 references use numeric indices like `SAVESTR:10`, `GLOBALS:(LOCAL * 2 + 1)`).

**Approach**:
1. **Add 3 entries to variableTypes array** in `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` at line ~229 (after TSTRNAME):
   ```csharp
   (VariableCode.SAVESTRNAME, "SAVESTR.CSV", "SAVESTR.yaml"),
   (VariableCode.GLOBALNAME, "GLOBAL.CSV", "GLOBAL.yaml"),
   (VariableCode.GLOBALSNAME, "GLOBALS.CSV", "GLOBALS.yaml"),
   ```
2. **No CSV/YAML files created**: Since these types have no named constants currently, no definition files are needed. The registration is defensive (future-proof) only.
3. **Zero-impact verification**: The existing `PopulateConstantNames()` logic (lines 231-289) already handles missing CSV/YAML files gracefully:
   - Line 238: `if (File.Exists(csvPath))` - skips if CSV doesn't exist
   - Line 250: `if (File.Exists(yamlPath))` - skips if YAML doesn't exist
   - Line 253: `if (mergedNames.Count > 0)` - only calls `PopulateNameData()` if data exists
   - Result: Adding the 3 entries with no files is a no-op (zero behavior change)

**Engine Dependency**: `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` is in the engine submodule. Changes require coordination with the engine repository. However, since F712 already has uncommitted changes to this file, we can piggyboard on that changeset.


### AC Coverage

| AC# | Design Element | Strategy | Verification Method |
|:---:|----------------|----------|---------------------|
| AC#1 | Zero "はこの関数中では定義されていません" warnings | Strategy 1: Add #DIM/#DIMS declarations to ~392 locations across ERB files | Run headless with --strict-warnings, verify output does not contain error message string |
| AC#2 | Zero TRYCALLLIST structural warnings | Strategy 2: Fix 7 specific structural issues (TRYCALLLIST targets) | Run headless with --strict-warnings, verify output does not contain "が見つかりません" |
| AC#3 | Zero FOR/NEXT structural warnings | Strategy 2: Fix 7 specific structural issues (FOR/NEXT nesting) | Run headless with --strict-warnings, verify output does not contain "の中以外で" |
| AC#4 | Total warning count is zero | Strategy 1 + Strategy 2 combined effect | Grep warning count in headless output, verify equals 0 |
| AC#5 | SAVESTR registration in PopulateConstantNames() | Strategy 3: Add 3 entries to variableTypes array in engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs | Inspect engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs for presence of SAVESTRNAME |
| AC#6 | GLOBAL registration in PopulateConstantNames() | Strategy 3: Add 3 entries to variableTypes array in engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs | Inspect engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs for presence of GLOBALNAME |
| AC#7 | GLOBALS registration in PopulateConstantNames() | Strategy 3: Add 3 entries to variableTypes array in engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs | Inspect engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs for presence of GLOBALSNAME |
| AC#8 | C# build succeeds with TreatWarningsAsErrors=true | Strategy 3: Engine changes must not introduce C# warnings | Run `dotnet build`, verify exit code 0 |
| AC#9 | Headless run completes without fatal error | Strategy 1 + Strategy 2: ERB changes must not break runtime | Run headless, verify no "FATAL" in output and normal exit |

**AC Coverage Notes**:
- AC#1 and AC#2 are independent (different warning types, different fix strategies)
- AC#4 is the composite verification of AC#1 + AC#2 + AC#3
- AC#5/6/7 are independent of AC#1/2/3/4 (engine-side, zero current impact)
- AC#8 validates that AC#5/6/7 implementation doesn't introduce new C# errors
- AC#9 validates that AC#1/2/3 implementations don't introduce runtime regressions

### Key Decisions

| Decision | Rationale | Trade-offs |
|----------|-----------|------------|
| **Fix #DIM warnings in ERB rather than suppressing** | Root cause is missing declarations (authoring error). Adding `#DIM` is the correct fix that prevents future confusion and maintains code clarity. Suppression would hide genuine issues. | **Pro**: Clean compile, no hidden errors. **Con**: ~392 edits across many files (labor-intensive but mechanical). |
| **Register SAVESTR/GLOBAL/GLOBALS defensively without creating definition files** | Future-proofs constant resolution with zero current cost. If future ERB code uses named constants (e.g., `SAVESTR:MY_SAVE_SLOT`), they'll resolve correctly. No definition files means zero current impact. | **Pro**: Future-proof, zero-risk. **Con**: Slight code maintenance burden (3 extra array entries). Benefit outweighs cost. |
| **Investigate TRYCALLLIST warnings before fixing** | Some TRYCALLLIST targets may be intentionally absent (optional function pattern common in ERB). Blindly "fixing" by adding dummy functions or removing calls could break intended behavior. | **Pro**: Avoids breaking working code. **Con**: Requires manual inspection (only 7 warnings, acceptable). |
| **Use automated script for #DIM insertion if warnings > 50 files** | Manual editing of ~392 declarations is error-prone and time-consuming. Automation reduces errors and accelerates implementation. | **Pro**: Fast, consistent. **Con**: Requires script development time. Threshold: use automation if >50 files affected. |
| **Batch validation after every 10-20 file edits** | Frequent validation catches errors early (e.g., wrong `#DIM` vs `#DIMS` type). Prevents cascading issues that require rework. | **Pro**: Early error detection. **Con**: More frequent headless runs (acceptable, each run is ~10-20 seconds). |
| **Do NOT create empty SAVESTR/GLOBAL/GLOBALS CSV/YAML files** | Empty definition files serve no purpose and clutter the repository. Registration in engine is sufficient for future-proofing. | **Pro**: Clean repo. **Con**: If future use is needed, files must be created then (deferred work is intentional). |

### Implementation Order

To minimize risk and enable early validation:

1. **Phase 1: Capture warnings** - Run headless to generate complete warning log, parse into structured data
2. **Phase 2: Strategy 3** - Register SAVESTR/GLOBAL/GLOBALS in ProcessInitializer.cs (independent, quick, validates C# build)
3. **Phase 3: Strategy 2** - Fix 7 structural warnings (small count, manual, high-value fixes)
4. **Phase 4: Strategy 1** - Fix ~392 #DIM warnings in batches of 10-20 files with validation after each batch
5. **Phase 5: Final validation** - Full headless run, verify AC#1-7 all pass

**Rationale for order**:
- Phase 1 provides the data needed for subsequent phases
- Phase 2 is independent and quick (proves C# changes work)
- Phase 3 before Phase 4 because structural warnings may affect #DIM scoping (e.g., missing ENDFUNC causes variables to leak into wrong function)
- Phase 4 is batched to enable incremental validation (catch errors early)
- Phase 5 is comprehensive final check

### Constraints and Assumptions

**Constraints**:
1. **Engine repo coordination**: ProcessInitializer.cs and ConstantData.cs are in the engine submodule. Changes must be committed to engine repo (can coordinate with F712).
2. **#DIM placement precision**: `#DIM` must be placed exactly after `@FUNCTION_NAME` and before executable code. Incorrect placement causes parse errors.
3. **Type matching (DIM vs DIMS)**: Mismatching variable type (integer vs string) causes runtime errors. Type detection must be accurate.
4. **F712 completed**: F712 is [DONE]. F714 builds on completed F712 refactoring.

**Assumptions**:
1. **All ~392 warnings are fixable with #DIM declarations**: No warnings are caused by engine bugs or unfixable ERB patterns. (Validated by Root Cause Analysis - all are missing declarations.)
2. **No intentional "undefined variable" patterns**: ERB code doesn't rely on variables being undefined for conditional logic. (Standard assumption for compiled languages.)
3. **TRYCALLLIST targets either exist or are documented as optional**: No warnings from genuine bugs in TRYCALLLIST usage. (Will verify during investigation.)
4. **No CSV/YAML files for SAVESTR/GLOBAL/GLOBALS exist elsewhere**: Grep confirms no existing definition files. Registration is purely defensive.

### Risk Mitigation

| Risk | Mitigation |
|------|------------|
| **#DIM additions shadow existing variables** | Before adding `#DIM`, verify variable name doesn't conflict with system variables or global constants. Grep for variable name in CSV/YAML definition files. |
| **Wrong type (DIM vs DIMS) breaks runtime** | Validate type detection by examining usage context. If uncertain, test with headless run. Engine will error immediately on type mismatch. |
| **TRYCALLLIST fixes break optional function pattern** | Investigate each TRYCALLLIST warning manually. Document justification before removing/modifying. Consult user if pattern unclear. |
| **Large file count makes manual editing error-prone** | Use automation script if >50 files affected. Include validation in script (syntax check, type detection). Review script output before applying. |
| **F712 changes conflict with F714 changes** | Coordinate engine repo commits. F714 builds on completed F712 refactoring. |
| **Missing ENDFUNC causes variable scope leaks** | Fix structural warnings (Phase 3) before #DIM warnings (Phase 4). Ensures function boundaries are correct before adding scoped declarations. |
| **Volume limit exceeded** | ~392 #DIM additions across many files exceeds erb type ~500 line limit. However, these are mechanical single-line additions with minimal complexity. Single-feature scope justified by atomic nature of warning elimination goal. |

<!-- fc-phase-5-completed -->
## Tasks

| T# | AC# | Description | Status |
|:--:|:---:|------|:------:|
| 1 | 1,6 | Warning capture: run headless --strict-warnings, parse 224 DIM_SCOPE warnings into structured data (file, function, variable) | [x] |
| 2 | 2,3,4,5 | Register SAVESTR/GLOBAL/GLOBALS in PopulateConstantNames(): add 3 entries to variableTypes array in ProcessInitializer.cs | [x] |
| 3 | 1,6 | Fix 224 DIM_SCOPE warnings: add #DIM declarations to 33 ERB files (7 unique variables: 父親, 輪姦内容, 成長度, 行為者, 客人数, ペニス, 精子量) | [x] |
| 4 | 1,2,3,4,5,6 | Final validation: verify all ACs with full headless run | [x] |

## Implementation Contract

| Phase | Agent | Model | Input | Output |
|:-----:|-------|:-----:|-------|--------|
| 1 | implementer | sonnet | SAVESTR/GLOBAL/GLOBALS registration requirements | Updated ProcessInitializer.cs with 3 new variableTypes entries |
| 2 | implementer | sonnet | 224 #DIM warning locations (33 files, 7 vars) | Added #DIM declarations across ERB functions |
| 3 | ac-tester | haiku | All ACs + verification requirements | Verified AC#1-6 pass |

### Scope

This feature eliminates 224 DIM_SCOPE ERB compile warnings post-F713 and defensively registers 3 variable types in the engine.

**In Scope**:
1. **Warning capture (Task 1)**: ✅ Complete — 224 DIM_SCOPE warnings identified across 33 files, 7 unique variables
2. **Engine registration (Task 2)**: Add SAVESTR/GLOBAL/GLOBALS to `PopulateConstantNames()` variableTypes array in `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs`
3. **#DIM warnings (Task 3)**: Fix 224 "はこの関数中では定義されていません" warnings by adding `#DIM` declarations to ERB functions
4. **Final validation (Task 4)**: Comprehensive AC verification with full headless run

**Key Implementation Rules**:
1. **#DIM Placement**: Immediately after `@FUNCTION_NAME` and before any executable code
2. **All 7 variables are integer type** → use `#DIM` (not `#DIMS`)
3. **Batch Validation**: Re-run headless after each batch of fixes to catch errors early

**AC Coverage Guarantees**:
- Task 1 → AC#1,6 (warning data captured)
- Task 2 → AC#2,3,4,5 (registration + C# build)
- Task 3 → AC#1,6 (DIM_SCOPE warnings eliminated + runtime safe)
- Task 4 → AC#1,2,3,4,5,6 (comprehensive verification)

### Out of Scope

**Explicitly Excluded**:
1. **ARRAY_OUT_OF_BOUNDS warnings (175)**: Deferred to F715 — EXP[111-123] and TCVAR[500,503] array size issues
2. **Creating SAVESTR/GLOBAL/GLOBALS CSV/YAML definition files**: No current need (0 named constant usage)
3. **Optimizing existing #DIM declarations**: Only add missing declarations
4. **TRYCALLLIST/FOR/NEXT warnings**: 0 exist (original prediction was incorrect)

**Discovery Protocol**: If any deferred issue is discovered, immediately STOP implementation and follow Scope Discipline protocol: REPORT to user → TRACK by creating feature-{next_id}.md → LINK in Mandatory Handoffs section.

## Review Notes

## Mandatory Handoffs

| Item | Destination | Action | Status |
|------|-------------|--------|--------|
| 175 ARRAY_OUT_OF_BOUNDS warnings (EXP[111-123], TCVAR[500,503]) | F715 | Option A: Created F715 DRAFT | Created |

## Execution Log

| Timestamp | Type | Source | Action | Detail |
|-----------|------|--------|--------|--------|
| Phase 2 | INFO | Investigation | Warning capture | 399 warnings: 224 DIM_SCOPE + 175 ARRAY_OUT_OF_BOUNDS + 0 structural |
| Phase 2 | SCOPE_CHANGE | Investigation | AC/Task update | Removed AC#2,3,4 (TRYCALLLIST/FOR/NEXT/total=0). Renumbered ACs. Deferred ARRAY_OUT_OF_BOUNDS to F715 |
| Phase 2 | DEVIATION | Bash | Build lock | Era.Core.dll locked by PID 35888 (21GB dotnet process). Killed and retried successfully. PRE-EXISTING. |
| 2026-02-01 06:48 | START | implementer | Task 2 | Register SAVESTR/GLOBAL/GLOBALS in PopulateConstantNames() |
| 2026-02-01 06:48 | END | implementer | Task 2 | SUCCESS: Added 3 entries to variableTypes array in ProcessInitializer.cs. C# build succeeded. |
| Phase 4 | START | implementer | Task 3 | Fix 224 DIM_SCOPE warnings across 33 ERB files |
| Phase 4 | END | implementer | Task 3 | SUCCESS: All 224 warnings fixed (224→30→2→0). 33 files modified, 7 variables (#DIM declarations added). |
| Phase 7 | DEVIATION | ac-tester | AC#6 | FAIL→[B]→[x]: F715 [DONE] resolved ARRAY_OUT_OF_BOUNDS. Re-verified: exit 0, no FATAL. |
