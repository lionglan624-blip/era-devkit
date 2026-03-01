# Feature 732: Headless Files VariableDataAccessor Adoption

## Status: [DONE]

## Type: engine

## Background

### Philosophy (Mid-term Vision)
F731 creates VariableDataAccessor as a centralized helper for VariableCode bitwise access patterns. The remaining 78 occurrences of raw DataString/DataIntegerArray access across multiple Headless files should be refactored to use this helper for consistency and maintainability.

### Problem (Current Issue)
After F731, only KojoTestRunner.ApplyCharacterConfig uses VariableDataAccessor. The remaining files still use raw bitwise access patterns (78 occurrences total): StateInjector (27), InteractiveRunner (23), KojoTestRunner non-ApplyCharacterConfig (17), KojoExpectValidator (5), VariableResolver (5), ScenarioParser (1).

### Goal (What to Achieve)
1. Add VariableData overloads to VariableDataAccessor (for global variables like FLAG, TARGET, RESULT, ARG)
2. Refactor StateInjector.cs (27 occurrences) to use VariableDataAccessor
3. Refactor InteractiveRunner.cs (23 occurrences) to use VariableDataAccessor
4. Refactor KojoTestRunner.cs non-ApplyCharacterConfig methods (17 occurrences: 10 VariableData, 6 CharacterData, 1 unanalyzed)
5. Refactor KojoExpectValidator.cs (5 occurrences) to use VariableDataAccessor
6. Refactor VariableResolver.cs (5 occurrences) to use VariableDataAccessor
7. Refactor ScenarioParser.cs (1 occurrence) to use VariableDataAccessor

## Links
- [feature-731.md](feature-731.md) - Creates VariableDataAccessor (predecessor)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Multiple Headless files (78 occurrences) still use raw `DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & code)]` access patterns
2. Why: F731 only refactored KojoTestRunner.ApplyCharacterConfig, leaving 78 occurrences in 6 other files untouched
3. Why: F731 was scoped to create the helper and prove it works in one method, not to adopt it everywhere
4. Why: Adopting across all files requires both CharacterData overloads (done) AND new VariableData overloads (not done)
5. Why: VariableDataAccessor only has CharacterData overloads -- the global variable access pattern `varData.DataIntegerArray[...]` has no helper, and `chara.DataInteger[...]` (single integer, not array) also has no helper

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| 78 raw bitwise access patterns scattered across 6 files | VariableDataAccessor is incomplete: missing VariableData overloads for global variables (FLAG, TARGET, RESULT, ARG, TFLAG, MASTER, ASSI) and missing CharacterData.DataInteger single-value accessor (for NO field) |

### Conclusion

The root cause is that VariableDataAccessor was intentionally created with only CharacterData overloads in F731. To refactor the remaining 78 occurrences, two categories of new overloads are needed:

1. **VariableData overloads** (32 of 78): `SetIntegerArray(VariableData, code, index, value)` and `GetIntegerArray(VariableData, code, index)` for global variables accessed via `varData.DataIntegerArray[...]`
2. **CharacterData single-integer accessor** (5 of 78): `GetInteger(CharacterData, code)` for `chara.DataInteger[...]` pattern (used for NO field lookup)
3. **Existing CharacterData overloads** (41 of 78): Already supported by current VariableDataAccessor methods

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F731 | [DONE] | Predecessor | Created VariableDataAccessor with CharacterData overloads; this feature extends it |
| F727 | [DONE] | Related | Added YAML character loading to KojoTestRunner; ApplyCharacterConfig was refactored by F731 |
| F729 | [REVIEWED] | Related | Game runtime YAML character loading; may create similar accessor patterns in future |

### Pattern Analysis

The 78 remaining occurrences break down into 3 distinct pattern categories:

**Category A: VariableData global variable access (32 occurrences)**
Pattern: `varData.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.XXX)]`
Where XXX = FLAG, TARGET, MASTER, TFLAG, RESULT, ARG, ASSI
Files: StateInjector (9), InteractiveRunner (12), KojoTestRunner (9), KojoExpectValidator (1), VariableResolver (1)

**Category B: CharacterData integer array access (41 occurrences)**
Pattern: `chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.XXX)]` or `chara.DataString[...]`
Already supported by existing VariableDataAccessor.SetIntegerArray/GetIntegerArray/SetString/GetString
Files: StateInjector (18), InteractiveRunner (11), KojoTestRunner (7 non-ApplyCharacterConfig), KojoExpectValidator (3), VariableResolver (4)

**Category C: CharacterData single integer access (5 occurrences)**
Pattern: `chara.DataInteger[(int)(VariableCode.__LOWERCASE__ & VariableCode.NO)]`
Used only for NO field lookup (character identification)
Files: InteractiveRunner (1 in FindCharacterIndex), KojoTestRunner (1), VariableResolver (1), ScenarioParser (1), KojoExpectValidator (1 via NO lookup)

Note: KojoExpectValidator count (5) = 1 VariableData (via GetCharacterRegister) + 3 CharacterData (DataString for NAME/CALLNAME) + 1 CharacterData (DataInteger for NO)

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Pure refactoring -- adding overloads and replacing patterns; no logic changes |
| Scope is realistic | YES | 78 occurrences across 6 files; each is a mechanical 1-line replacement; ~300 line estimate within engine feature limit |
| No blocking constraints | YES | F731 is [DONE]; VariableDataAccessor.cs exists and is proven |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F731 | [DONE] | Created VariableDataAccessor helper class with CharacterData overloads |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| VariableData class | Runtime | Low | Engine internal class; stable API (DataIntegerArray, DataInteger, DataString) |
| VariableCode enum | Runtime | Low | Stable bitwise flags; no changes expected |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| KojoTestRunner.cs | HIGH | 17 remaining raw accesses (non-ApplyCharacterConfig methods) |
| StateInjector.cs | HIGH | 27 raw accesses for Set/Get operations |
| InteractiveRunner.cs | HIGH | 23 raw accesses in HandleCall, HandleUsercom, GetVariableValue, SetupCharacters, FindCharacterIndex |
| KojoExpectValidator.cs | MEDIUM | 5 raw accesses in GetCallName, ResolveCharacterIndexWithVarData, GetCharacterRegister |
| VariableResolver.cs | MEDIUM | 5 raw accesses in ResolveCharacterIndex, ResolveSpecialCharacterRegister, GetAvailableCharacters |
| ScenarioParser.cs | LOW | 1 raw access for character NO lookup in TryAddPendingCharacters |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs | Update | Add VariableData overloads (SetIntegerArray, GetIntegerArray) + CharacterData GetInteger |
| engine/Assets/Scripts/Emuera/Headless/StateInjector.cs | Update | Replace 27 raw accesses with VariableDataAccessor calls |
| engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs | Update | Replace 23 raw accesses with VariableDataAccessor calls |
| engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs | Update | Replace 17 raw accesses in non-ApplyCharacterConfig methods |
| engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs | Update | Replace 5 raw accesses |
| engine/Assets/Scripts/Emuera/Headless/VariableResolver.cs | Update | Replace 5 raw accesses |
| engine/Assets/Scripts/Emuera/Headless/ScenarioParser.cs | Update | Replace 1 raw access |
| Era.Core.Tests/ (or engine.Tests/) | Update | Add/update VariableDataAccessor tests for new overloads |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| VariableDataAccessor is `internal static` | F731 design | All consumers are in same namespace (Headless) -- no issue |
| `#if HEADLESS_MODE` conditional compilation | All Headless files | New overloads must also be within `#if HEADLESS_MODE` block |
| TreatWarningsAsErrors=true | Directory.Build.props | Must not introduce unused parameter warnings or other warnings |
| Existing tests must continue to pass | Test policy | Pure refactoring -- behavior must be identical |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Behavioral regression from refactoring | Low | High | Each replacement is mechanical; run existing tests after each file |
| Missing edge case in VariableData overloads | Low | Medium | Follow exact same pattern as CharacterData overloads (null checks, bounds checks) |
| Scope creep to non-Headless files | Low | Low | F732 scope is explicitly limited to 6 Headless files |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "centralized helper for VariableCode bitwise access patterns" | VariableDataAccessor must be the SSOT for all bitwise access | AC#1, AC#2, AC#3, AC#4-AC#9 |
| "remaining 78 occurrences...should be refactored" | All 78 raw `__LOWERCASE__` patterns in 6 files must be eliminated | AC#4, AC#5, AC#6, AC#7, AC#8, AC#9 |
| "Add VariableData overloads" | New SetIntegerArray/GetIntegerArray for VariableData must exist | AC#1 |
| "CharacterData single-integer accessor" | New GetInteger for CharacterData.DataInteger must exist | AC#2 |
| "consistency and maintainability" | Zero raw bitwise access outside VariableDataAccessor; zero tech debt | AC#10, AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VariableData overloads exist | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `TrySetIntegerArray\(VariableData` | [x] |
| 2 | GetInteger for CharacterData exists | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `GetInteger\(CharacterData` | [x] |
| 3 | GetIntegerArray VariableData overload exists | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `TryGetIntegerArray\(VariableData` | [x] |
| 4 | StateInjector uses VariableDataAccessor only | code | Grep(engine/Assets/Scripts/Emuera/Headless/StateInjector.cs) | not_matches | `DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__\|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__\|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` | [x] |
| 5 | InteractiveRunner uses VariableDataAccessor only | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | not_matches | `DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__\|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__\|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` | [x] |
| 6 | KojoTestRunner uses VariableDataAccessor only | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs) | not_matches | `DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__\|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__\|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` | [x] |
| 7 | KojoExpectValidator uses VariableDataAccessor only | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs) | not_matches | `DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__\|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__\|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` | [x] |
| 8 | VariableResolver uses VariableDataAccessor only | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableResolver.cs) | not_matches | `DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__\|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__\|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` | [x] |
| 9 | ScenarioParser uses VariableDataAccessor only | code | Grep(engine/Assets/Scripts/Emuera/Headless/ScenarioParser.cs) | not_matches | `DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__\|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__\|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` | [x] |
| 10 | Build succeeds | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | 0 | [x] |
| 11 | Existing tests pass | test | dotnet test engine.Tests --filter FullyQualifiedName~VariableDataAccessorTests | succeeds | 0 | [x] |
| 12a | TrySetIntegerArray_VariableData tests exist | code | Grep(engine.Tests/Tests/VariableDataAccessorTests.cs) | matches | `TrySetIntegerArray_VariableData` | [x] |
| 12b | TryGetIntegerArray_VariableData tests exist | code | Grep(engine.Tests/Tests/VariableDataAccessorTests.cs) | matches | `TryGetIntegerArray_VariableData` | [x] |
| 13 | New VariableData overload tests pass | test | dotnet test engine.Tests --filter FullyQualifiedName~VariableDataAccessorTests | succeeds | 0 | [x] |
| 14 | Zero tech debt | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 15 | __LOWERCASE__ only in VariableDataAccessor | code | Grep(engine/Assets/Scripts/Emuera/Headless/StateInjector.cs,engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs,engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs,engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs,engine/Assets/Scripts/Emuera/Headless/VariableResolver.cs,engine/Assets/Scripts/Emuera/Headless/ScenarioParser.cs) | not_matches | `__LOWERCASE__` | [x] |

### AC Details

**AC#1: VariableData overloads exist**
- Verifies `TrySetIntegerArray(VariableData, VariableCode, int, long)` exists in VariableDataAccessor.cs
- This is Category A enabler: 32 occurrences of `varData.DataIntegerArray[...]` need this overload
- Returns bool for success/failure to preserve StateInjector error reporting and return false behavior
- Test: Grep pattern=`TrySetIntegerArray\(VariableData` path=engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs
- Expected: At least 1 match (method signature)

**AC#2: GetInteger for CharacterData exists**
- Verifies `GetInteger(CharacterData, VariableCode)` exists for single-value DataInteger access
- This is Category C enabler: 5 occurrences of `chara.DataInteger[...]` (NO field) need this method
- Test: Grep pattern=`GetInteger\(CharacterData` path=engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs
- Expected: At least 1 match (method signature)

**AC#3: GetIntegerArray VariableData overload exists**
- Verifies `TryGetIntegerArray(VariableData, VariableCode, int, out long)` exists for global variable reads
- Completes Category A pair (Set + Get) with bool return for bounds checking preservation
- Test: Grep pattern=`TryGetIntegerArray\(VariableData` path=engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs
- Expected: At least 1 match (method signature)

**AC#4: StateInjector uses VariableDataAccessor only**
- Verifies all 27 raw bitwise access patterns eliminated from StateInjector.cs
- Covers Goal item #2
- Test: Grep pattern=`DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` path=engine/Assets/Scripts/Emuera/Headless/StateInjector.cs
- Expected: 0 matches

**AC#5: InteractiveRunner uses VariableDataAccessor only**
- Verifies all 23 raw bitwise access patterns eliminated from InteractiveRunner.cs
- Covers Goal item #3
- Test: Grep pattern=`DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` path=engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs
- Expected: 0 matches

**AC#6: KojoTestRunner uses VariableDataAccessor only**
- Verifies all 17 raw bitwise access patterns eliminated from KojoTestRunner.cs (non-ApplyCharacterConfig)
- Covers Goal item #4
- Note: ApplyCharacterConfig was already refactored in F731; this covers the remaining methods
- Test: Grep pattern=`DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` path=engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs
- Expected: 0 matches

**AC#7: KojoExpectValidator uses VariableDataAccessor only**
- Verifies all 5 raw bitwise access patterns eliminated from KojoExpectValidator.cs
- Covers Goal item #5
- Test: Grep pattern=`DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` path=engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs
- Expected: 0 matches

**AC#8: VariableResolver uses VariableDataAccessor only**
- Verifies all 5 raw bitwise access patterns eliminated from VariableResolver.cs
- Covers Goal item #6
- Test: Grep pattern=`DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` path=engine/Assets/Scripts/Emuera/Headless/VariableResolver.cs
- Expected: 0 matches

**AC#9: ScenarioParser uses VariableDataAccessor only**
- Verifies the 1 raw bitwise access pattern eliminated from ScenarioParser.cs
- Covers Goal item #7
- Test: Grep pattern=`DataIntegerArray\[\(int\)\(VariableCode\.__LOWERCASE__|DataInteger\[\(int\)\(VariableCode\.__LOWERCASE__|DataString\[\(int\)\(VariableCode\.__LOWERCASE__` path=engine/Assets/Scripts/Emuera/Headless/ScenarioParser.cs
- Expected: 0 matches

**AC#10: Build succeeds**
- Verifies no compilation errors after refactoring 78 occurrences
- TreatWarningsAsErrors=true ensures no warnings either
- Test: dotnet build engine/uEmuera.Headless.csproj
- Expected: Exit code 0

**AC#11: Existing tests pass**
- Verifies behavioral equivalence: refactoring does not change runtime behavior
- Test: dotnet test engine.Tests --filter FullyQualifiedName~VariableDataAccessorTests
- Expected: All tests pass

**AC#12a: TrySetIntegerArray_VariableData tests exist**
- Verifies test methods for TrySetIntegerArray VariableData overload exist in test file
- Ensures SetIntegerArray functionality for global variables has test coverage before execution
- Test: Grep pattern=`TrySetIntegerArray_VariableData` path=engine.Tests/Tests/VariableDataAccessorTests.cs
- Expected: At least 1 match (indicating TrySetIntegerArray_VariableData test methods exist)

**AC#12b: TryGetIntegerArray_VariableData tests exist**
- Verifies test methods for TryGetIntegerArray VariableData overload exist in test file
- Ensures GetIntegerArray functionality for global variables has test coverage before execution
- Test: Grep pattern=`TryGetIntegerArray_VariableData` path=engine.Tests/Tests/VariableDataAccessorTests.cs
- Expected: At least 1 match (indicating TryGetIntegerArray_VariableData test methods exist)

**AC#13: New VariableData overload tests pass**
- Verifies new overloads (TrySetIntegerArray/TryGetIntegerArray for VariableData, GetInteger for CharacterData) tests execute successfully
- Covers positive cases (valid access) and negative cases (null data, out-of-bounds index) with bool return verification
- Test: dotnet test engine.Tests --filter FullyQualifiedName~VariableDataAccessorTests
- Expected: All tests pass (including new tests for VariableData overloads and GetInteger)

**AC#14: Zero tech debt**
- Verifies no TODO/FIXME/HACK markers in VariableDataAccessor.cs
- Test: Grep pattern=`TODO|FIXME|HACK` path=engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs
- Expected: 0 matches

**AC#15: __LOWERCASE__ only in VariableDataAccessor**
- Cross-cutting verification: confirms ALL 6 target files have zero `__LOWERCASE__` references
- This is the aggregate check that subsumes AC#4-AC#9 with a broader pattern
- After refactoring, `VariableCode.__LOWERCASE__` should only exist in VariableDataAccessor.cs (the SSOT)
- Test: Grep pattern=`__LOWERCASE__` across all 6 target files
- Expected: 0 matches

### Goal Coverage Verification

| Goal# | Description | Covering ACs |
|:-----:|-------------|:------------:|
| 1 | Add VariableData overloads to VariableDataAccessor | AC#1, AC#3 |
| 2 | Refactor StateInjector.cs (27 occurrences) | AC#4 |
| 3 | Refactor InteractiveRunner.cs (23 occurrences) | AC#5 |
| 4 | Refactor KojoTestRunner.cs non-ApplyCharacterConfig (17 occurrences) | AC#6 |
| 5 | Refactor KojoExpectValidator.cs (5 occurrences) | AC#7 |
| 6 | Refactor VariableResolver.cs (5 occurrences) | AC#8 |
| 7 | Refactor ScenarioParser.cs (1 occurrence) | AC#9 |
| (implicit) | Add CharacterData.GetInteger for NO field | AC#2 |
| (implicit) | Build and tests pass | AC#10, AC#11, AC#12a, AC#12b, AC#13 |
| (implicit) | Zero tech debt | AC#14 |
| (implicit) | SSOT enforcement | AC#15 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature extends VariableDataAccessor with two new categories of overloads to complete the refactoring of 78 raw bitwise access patterns:

1. **VariableData overloads** (Category A: 32 occurrences): Add `TrySetIntegerArray(VariableData, VariableCode, int, long)` and `TryGetIntegerArray(VariableData, VariableCode, int, out long)` with bool returns to preserve StateInjector error reporting behavior for global variable access (FLAG, TARGET, RESULT, ARG, TFLAG, MASTER, ASSI)
2. **CharacterData single-integer accessor** (Category C: 5 occurrences): Add `GetInteger(CharacterData, VariableCode)` for single DataInteger access (NO field lookup)
3. **Systematic refactoring**: Replace all 78 raw patterns file-by-file, validating build after each file

The implementation follows F731's exact pattern (null checks, bounds checks, conditional compilation) and maintains 100% behavioral equivalence.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `public static bool TrySetIntegerArray(VariableData varData, VariableCode code, int arrayIndex, long value)` method to VariableDataAccessor.cs |
| 2 | Add `public static long GetInteger(CharacterData chara, VariableCode code)` method to VariableDataAccessor.cs for single DataInteger access |
| 3 | Add `public static bool TryGetIntegerArray(VariableData varData, VariableCode code, int arrayIndex, out long value)` method to VariableDataAccessor.cs |
| 4 | Replace all 27 raw bitwise patterns in StateInjector.cs with VariableDataAccessor calls (9 VariableData, 18 CharacterData) |
| 5 | Replace all 23 raw bitwise patterns in InteractiveRunner.cs with VariableDataAccessor calls (12 VariableData, 10 CharacterData array, 1 CharacterData single) |
| 6 | Replace all 17 raw bitwise patterns in KojoTestRunner.cs with VariableDataAccessor calls (9 VariableData, 7 CharacterData array, 1 CharacterData single) |
| 7 | Replace all 5 raw bitwise patterns in KojoExpectValidator.cs with VariableDataAccessor calls (1 VariableData via GetCharacterRegister, 3 CharacterData array via GetString, 1 CharacterData single for NO) |
| 8 | Replace all 5 raw bitwise patterns in VariableResolver.cs with VariableDataAccessor calls (1 VariableData, 3 CharacterData array, 1 CharacterData single for NO) |
| 9 | Replace the 1 raw bitwise pattern in ScenarioParser.cs with VariableDataAccessor.GetInteger(chara, VariableCode.NO) |
| 10 | Run `dotnet build engine/uEmuera.Headless.csproj` after all refactoring is complete; TreatWarningsAsErrors=true ensures no warnings |
| 11 | Run `dotnet test engine.Tests --filter FullyQualifiedName~VariableDataAccessorTests` to verify existing CharacterData tests continue to pass |
| 12a,12b | Add test methods to verify TrySetIntegerArray_VariableData and TryGetIntegerArray_VariableData patterns exist in VariableDataAccessorTests.cs |
| 13 | Add test methods for TrySetIntegerArray/TryGetIntegerArray VariableData overloads and GetInteger for CharacterData with bool return verification (TrySetIntegerArray_VariableData_ValidCode, TrySetIntegerArray_VariableData_NullData, TrySetIntegerArray_VariableData_OutOfBounds, TryGetIntegerArray_VariableData_ValidCode, TryGetIntegerArray_VariableData_NullData, TryGetIntegerArray_VariableData_OutOfBounds, GetInteger_CharacterData_ValidCode, GetInteger_CharacterData_NullCharacter) to VariableDataAccessorTests.cs |
| 14 | Avoid adding TODO/FIXME/HACK comments; new overloads are straightforward implementations |
| 14 | After refactoring all 6 files, grep for `__LOWERCASE__` to verify zero occurrences outside VariableDataAccessor.cs |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| New overload naming | (A) SetVariableDataIntegerArray, (B) SetGlobalIntegerArray, (C) SetIntegerArray with VariableData signature | C | Matches existing CharacterData overload naming pattern; C# overload resolution handles type disambiguation |
| Null handling for VariableData overloads | (A) Throw ArgumentNullException, (B) Return silently/return 0, (C) Return with Console.Error | B | Matches existing CharacterData overload pattern (lines 27-28, 47-48); silent failure for null/invalid |
| GetInteger return type | (A) long? (nullable), (B) long with 0 default | B | Matches existing GetIntegerArray pattern (line 88-102); 0 is valid default for integer variables |
| Refactoring order | (A) All files parallel, (B) Sequential file-by-file with build validation | B | Fail-fast: isolate regressions to specific file; 6 small batches safer than 1 large batch |
| Test structure | (A) Separate test class for VariableData overloads, (B) Add to existing VariableDataAccessorTests | B | Maintains test cohesion; existing test structure already has region grouping for each method |

### Implementation Contract

#### New Method Signatures

Add these three methods to `VariableDataAccessor.cs` (inside `#if HEADLESS_MODE` block, after existing GetIntegerArray method):

```csharp
/// <summary>
/// Set an integer array element in VariableData (global variables).
/// Used for FLAG, TARGET, RESULT, ARG, TFLAG, MASTER, ASSI, etc.
/// </summary>
/// <param name="varData">Variable data to modify</param>
/// <param name="code">Variable code (e.g., VariableCode.FLAG)</param>
/// <param name="arrayIndex">Array index within the variable array</param>
/// <param name="value">Value to set</param>
/// <returns>True if set successfully, false if invalid data/bounds</returns>
public static bool TrySetIntegerArray(VariableData varData, VariableCode code, int arrayIndex, long value)
{
    if (varData == null || varData.DataIntegerArray == null)
        return false;

    int varIndex = (int)(VariableCode.__LOWERCASE__ & code);
    if (varIndex >= varData.DataIntegerArray.Length)
        return false;

    var array = varData.DataIntegerArray[varIndex];
    if (array == null || arrayIndex < 0 || arrayIndex >= array.Length)
        return false;

    array[arrayIndex] = value;
    return true;
}

/// <summary>
/// Get an integer array element from VariableData (global variables).
/// Used for FLAG, TARGET, RESULT, ARG, TFLAG, MASTER, ASSI lookups.
/// </summary>
/// <param name="varData">Variable data to read</param>
/// <param name="code">Variable code (e.g., VariableCode.FLAG)</param>
/// <param name="arrayIndex">Array index within the variable array</param>
/// <param name="value">Output value if successful</param>
/// <returns>True if read successfully, false if invalid data/bounds</returns>
public static bool TryGetIntegerArray(VariableData varData, VariableCode code, int arrayIndex, out long value)
{
    value = 0;
    if (varData == null || varData.DataIntegerArray == null)
        return false;

    int varIndex = (int)(VariableCode.__LOWERCASE__ & code);
    if (varIndex >= varData.DataIntegerArray.Length)
        return false;

    var array = varData.DataIntegerArray[varIndex];
    if (array == null || arrayIndex < 0 || arrayIndex >= array.Length)
        return false;

    value = array[arrayIndex];
    return true;
}

/// <summary>
/// Get a single integer value from CharacterData.
/// Used for NO field lookup (character identification).
/// </summary>
/// <param name="chara">Character data to read</param>
/// <param name="code">Variable code (e.g., VariableCode.NO)</param>
/// <returns>Integer value or 0 if invalid</returns>
public static long GetInteger(CharacterData chara, VariableCode code)
{
    if (chara == null || chara.DataInteger == null)
        return 0;

    int index = (int)(VariableCode.__LOWERCASE__ & code);
    if (index >= chara.DataInteger.Length)
        return 0;

    return chara.DataInteger[index];
}
```

#### Refactoring Pattern Examples

**Pattern A: VariableData write (Category A)**

Before:
```csharp
var flagArray = varData.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.FLAG)];
if (index >= flagArray.Length)
{
    Console.Error.WriteLine($"[Inject] FLAG index out of range: {index}");
    return false;
}
flagArray[index] = value;
```

After:
```csharp
if (!VariableDataAccessor.TrySetIntegerArray(varData, VariableCode.FLAG, index, value))
{
    Console.Error.WriteLine($"[Inject] FLAG index out of range: {index}");
    return false;
}
```

**Pattern A-2: VariableData read with bounds check (Category A)**

Before:
```csharp
var targetArray = varData.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.TARGET)];
if (index >= targetArray.Length)
{
    Console.Error.WriteLine($"[Get] TARGET index out of range: {index}");
    return 0; // or return false if method returns bool
}
long currentValue = targetArray[index];
```

After:
```csharp
if (!VariableDataAccessor.TryGetIntegerArray(varData, VariableCode.TARGET, index, out long currentValue))
{
    Console.Error.WriteLine($"[Get] TARGET index out of range: {index}");
    return 0; // or return false if method returns bool
}
```

**Pattern B: VariableData simple read (Category A)**

Before:
```csharp
var targetArray = varData.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.TARGET)];
long currentValue = targetArray[index];
```

After:
```csharp
// For simple cases without error reporting, can ignore return value
VariableDataAccessor.TryGetIntegerArray(varData, VariableCode.TARGET, index, out long currentValue);
```

**Pattern C: CharacterData single integer read (Category C)**

Before:
```csharp
long charaNo = chara.DataInteger[(int)(VariableCode.__LOWERCASE__ & VariableCode.NO)];
```

After:
```csharp
long charaNo = VariableDataAccessor.GetInteger(chara, VariableCode.NO);
```

**Pattern D: CharacterData array access (already supported)**

Before:
```csharp
var cflagArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.CFLAG)];
cflagArray[index] = value;
```

After:
```csharp
VariableDataAccessor.SetIntegerArray(chara, VariableCode.CFLAG, index, value);
```

#### Test Naming Convention

New tests follow existing pattern: `{MethodName}_{Scenario}_{ExpectedBehavior}`

Add to `VariableDataAccessorTests.cs` in new regions:

```csharp
#region TrySetIntegerArray VariableData Tests

[Fact]
public void TrySetIntegerArray_VariableData_ValidCode_ReturnsTrue()

[Fact]
public void TrySetIntegerArray_VariableData_NullData_ReturnsFalse()

[Fact]
public void TrySetIntegerArray_VariableData_OutOfBounds_ReturnsFalse()

[Fact]
public void TrySetIntegerArray_VariableData_NegativeIndex_ReturnsFalse()

#endregion

#region TryGetIntegerArray VariableData Tests

[Fact]
public void TryGetIntegerArray_VariableData_ValidCode_ReturnsTrue()

[Fact]
public void TryGetIntegerArray_VariableData_NullData_ReturnsFalse()

[Fact]
public void TryGetIntegerArray_VariableData_OutOfBounds_ReturnsFalse()

[Fact]
public void TryGetIntegerArray_VariableData_NegativeIndex_ReturnsFalse()

#endregion

#region GetInteger CharacterData Tests

[Fact]
public void GetInteger_CharacterData_ValidCode_ReturnsValue()

[Fact]
public void GetInteger_CharacterData_NullCharacter_ReturnsZero()

#endregion
```

Test setup helper: Use existing `CreateTestCharacterData()` for CharacterData tests. For VariableData tests, access `varData` via `GlobalStatic.VariableData` (initialized by GlobalStatic collection fixture).

#### Refactoring Checklist

File-by-file refactoring order (smallest to largest for quick wins):

1. ScenarioParser.cs (1 occurrence) - Single NO lookup
2. KojoExpectValidator.cs (5 occurrences) - 1 VariableData + 3 GetString + 1 NO
3. VariableResolver.cs (5 occurrences) - 1 VariableData + 3 CharacterData array + 1 NO
4. KojoTestRunner.cs (17 occurrences) - 9 VariableData + 7 CharacterData array + 1 NO
5. InteractiveRunner.cs (23 occurrences) - 12 VariableData + 10 CharacterData array + 1 NO
6. StateInjector.cs (27 occurrences) - 9 VariableData + 18 CharacterData array

After each file: `dotnet build engine/uEmuera.Headless.csproj` to validate no regressions.

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 12a,12b | Write unit tests for new VariableData overloads and CharacterData.GetInteger (TDD RED) | [x] |
| 2 | 1,2,3 | Implement VariableData overloads (SetIntegerArray, GetIntegerArray) and CharacterData.GetInteger in VariableDataAccessor.cs (TDD GREEN) | [x] |
| 3 | 4,5,6,7,8,9,15 | Refactor all 78 occurrences across 6 Headless files to use VariableDataAccessor | [x] |
| 4 | 10,11,13,14 | Verify build succeeds, all tests pass, and zero tech debt | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Test specifications from Technical Design | Unit tests (TDD RED) |
| 2 | implementer | sonnet | T2 | Method specifications from Technical Design | VariableDataAccessor new overloads (TDD GREEN) |
| 3 | implementer | sonnet | T3 | Refactoring patterns from Technical Design, file list from Refactoring Checklist | Refactored 6 Headless files with zero raw bitwise access |
| 4 | ac-tester | haiku | T4 | AC commands from AC Details | Verification results |

**Constraints** (from Technical Design):

1. All new methods must be within `#if HEADLESS_MODE` conditional compilation block
2. Follow exact null-check and bounds-check pattern from existing CharacterData overloads (F731)
3. Refactoring is purely mechanical - no logic changes allowed
4. TreatWarningsAsErrors=true enforced - zero warnings permitted
5. Build verification required after each file refactoring for fail-fast error isolation

**Pre-conditions**:

- F731 completed ([DONE]) - VariableDataAccessor.cs exists with CharacterData overloads
- engine.Tests/Tests/VariableDataAccessorTests.cs exists (created in F731)
- All 6 target Headless files compile without errors

**Success Criteria**:

1. AC#1-3: New methods exist and pass Grep verification
2. AC#4-9,14: Zero raw bitwise access patterns (`__LOWERCASE__`) in 6 target files
3. AC#10: Build succeeds with zero warnings
4. AC#11-12: All tests pass (existing CharacterData tests + new VariableData/GetInteger tests)
5. AC#13: Zero TODO/FIXME/HACK in VariableDataAccessor.cs

**Rollback Plan**:

If issues arise after deployment:

1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with investigation of root cause (likely: behavioral difference in edge case handling)

**Test Naming Convention**: Test methods follow F731 established pattern `{MethodName}_{Scenario}_{ExpectedBehavior}` format (e.g., `SetIntegerArray_VariableData_ValidCode_SetsValue`, `GetInteger_CharacterData_NullCharacter_ReturnsZero`). This ensures AC filter patterns match correctly.

## Deferred Items

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| | | |
