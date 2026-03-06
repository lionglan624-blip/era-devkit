# Feature 731: Character Data Structure Encapsulation

## Status: [DONE]

## Scope Discipline

**Within Scope (WS)**:
- Creating VariableDataAccessor helper class
- Refactoring KojoTestRunner.ApplyCharacterConfig only
- Encapsulating CharacterData access patterns only

**Out Of Scope (OOS) - Explicit Defer**:
- Remaining Headless files refactoring (deferred to F732)
- VariableData overloads (deferred to F732)
- KojoTestRunner non-ApplyCharacterConfig methods (deferred to F732)

## Type: engine

## Background

### Philosophy (Mid-term Vision)
F727's ApplyCharacterConfig method directly manipulates CharacterData internals (DataString, DataIntegerArray) using VariableCode bitwise operations. This creates tight coupling between KojoTestRunner and CharacterData's internal structure. The manipulation pattern should be encapsulated in an engine helper for reusability (CharacterData patterns in F731, VariableData patterns in F732) and maintainability.

### Problem (Current Issue)
ApplyCharacterConfig in KojoTestRunner directly accesses CharacterData.DataString and DataIntegerArray using `(int)(VariableCode.__LOWERCASE__ & VariableCode.X)` pattern. This is fragile and creates coupling to internal data structure layout.

### Goal (What to Achieve)
1. Create an engine helper class for character data manipulation
2. Encapsulate VariableCode access patterns for CharacterData (VariableData patterns deferred to F732)
3. Refactor KojoTestRunner.ApplyCharacterConfig to use the helper

## Links
- [feature-727.md](feature-727.md) - Source of ApplyCharacterConfig pattern
- [feature-728.md](feature-728.md) - CharacterConfig model extension (will add more fields to apply)
- [feature-729.md](feature-729.md) - Related refactoring (operates on CharacterTemplate, not CharacterData, so does not need VariableDataAccessor)
- [feature-105.md](feature-105.md) - Extended pattern with character name/callname lookup

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why does KojoTestRunner.ApplyCharacterConfig directly manipulate CharacterData internals?**
   Because F727 needed to populate character data from YAML and there was no existing helper method to do so. The quickest path was to replicate the pattern already used elsewhere in the Headless codebase.

2. **Why is there no existing helper for character data manipulation?**
   Because the original engine design treats CharacterData as a low-level data container with raw array accessors (DataString, DataIntegerArray). All access was historically done through the VariableEvaluator during ERB script execution, not programmatically.

3. **Why do Headless mode files access CharacterData directly instead of going through VariableEvaluator?**
   Because the Headless subsystem (KojoTestRunner, InteractiveRunner, StateInjector, VariableResolver, KojoExpectValidator) needs to read/write game state without executing ERB scripts. The VariableEvaluator API is designed for ERB interpretation, not for programmatic state injection/inspection.

4. **Why hasn't this access pattern been centralized before?**
   Because the Headless subsystem was built incrementally across multiple features (F002 StateInjector, F105 KojoTestRunner character support, F727 YAML loading). Each feature independently implemented the same `(int)(VariableCode.__LOWERCASE__ & VariableCode.X)` bitwise pattern without extracting a common helper.

5. **Why is the bitwise pattern `(int)(VariableCode.__LOWERCASE__ & VariableCode.X)` used instead of a named accessor?**
   Because `VariableCode` is a flags enum where the upper bits encode type information (integer vs string, array vs scalar) and `__LOWERCASE__` masks to extract the array index. This is an engine-internal design that was never abstracted. The pattern appears **78 times across 5 Headless files**, creating significant maintenance burden.

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| ApplyCharacterConfig in KojoTestRunner has tight coupling to CharacterData internals | No engine-level abstraction exists for programmatic character data access outside of ERB evaluation |
| Same bitwise pattern `(int)(VariableCode.__LOWERCASE__ & VariableCode.X)` is duplicated 78 times across 5 files | Headless subsystem was built incrementally without extracting common data access patterns |
| F729 (game runtime YAML loading) will need to duplicate the same pattern | Lack of reusable helper forces every new consumer to re-implement raw array access |

### Conclusion

The root cause is **missing abstraction for programmatic variable data access**. The VariableCode bitwise pattern `(int)(VariableCode.__LOWERCASE__ & VariableCode.X)` is an engine implementation detail that has leaked into 5 Headless subsystem files (78 total occurrences). Each file independently implements the same pattern for reading/writing CharacterData.DataString and CharacterData.DataIntegerArray. There is no helper, extension method, or service that encapsulates this pattern.

The problem extends beyond ApplyCharacterConfig alone. The same pattern is used for:
- **Global variable access** (FLAG, TFLAG, TARGET, MASTER, ASSI, RESULT, ARG) on VariableData
- **Character variable access** (NAME, CALLNAME, CFLAG, TALENT, ABL, BASE, MAXBASE, TEQUIP, PALAM, MARK, EXP, TCVAR) on CharacterData
- Both **read** and **write** operations

**Total Pattern Occurrences**: 85 across 6 Headless files. F731 refactors 14 occurrences in ApplyCharacterConfig (2 DataString + 12 DataIntegerArray including Experience/Relation), leaving 71 for F732.

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F727 | [DONE] | Created the pattern | ApplyCharacterConfig introduced in F727; 23 occurrences in KojoTestRunner |
| F729 | [PROPOSED] | Will benefit | Game runtime YAML loading will need same character data write operations |
| F728 | [WIP] | Will extend scope | CharacterConfig model extension adds more fields (EXP, MARK, etc.) that need applying via this helper |
| F105 | [DONE] | Extended pattern | Added character name/callname lookup using DataString access in KojoTestRunner |

### Pattern Analysis

This is an **incremental duplication pattern**. Each new Headless feature copied the bitwise access pattern from existing code rather than extracting a common utility. The duplication grew across 5 features/files:

| File | Occurrences | Purpose |
|------|:-----------:|---------|
| StateInjector.cs | 27 | Read/write global and character variables via named setter methods |
| KojoTestRunner.cs | 24 | Character setup, variable reading, YAML config application |
| InteractiveRunner.cs | 23 | Interactive mode variable reading/writing |
| KojoExpectValidator.cs | 5 | Expected value resolution for test validation |
| VariableResolver.cs | 5 | Character name lookup and variable resolution |
| ScenarioParser.cs | 1 | Character data access using DataInteger |
| **Total** | **85** | |

The pattern keeps growing because there is no alternative API. F728 will add more character fields (EXP, MARK, TEQUIP, PALAM, etc.), and F729 will need write access for game runtime loading - both will add more duplicated access code without this encapsulation.

F732 will need to add VariableData overloads to handle global variable access patterns in StateInjector and InteractiveRunner (50 of the 85 occurrences operate on VariableData for FLAG, TARGET, RESULT, ARG, etc.).

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | The bitwise pattern is mechanical and well-understood; extracting to a helper is straightforward |
| Scope is realistic | YES | Helper class creation + refactoring ApplyCharacterConfig is ~100-150 lines of new code; within ~300 line limit |
| No blocking constraints | YES | CharacterData and VariableData are in engine project; helper can be placed alongside them |

**Verdict**: FEASIBLE

**Justification**:

1. **Pattern is well-defined**: Every occurrence follows the exact same form: `obj.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.X)]` or `obj.DataString[(int)(VariableCode.__LOWERCASE__ & VariableCode.X)]`. This is a textbook case for extraction to a helper method.

2. **No external dependencies**: The helper only needs access to `VariableCode`, `CharacterData`, and `VariableData` - all within the engine project. No cross-project references needed.

3. **Incremental refactoring possible**: The helper can be created and ApplyCharacterConfig refactored first (F731 scope). Other Headless files (StateInjector, InteractiveRunner, etc.) can be refactored in follow-up features if desired.

4. **StateInjector provides precedent**: StateInjector already wraps the pattern in named methods (SetFlag, SetTarget, SetCflag, etc.) but implements each method with raw array access. The helper would allow StateInjector's named methods to delegate to it.

5. **Scope decision**: The feature should focus on creating the helper and refactoring ApplyCharacterConfig only (Goal #3). Refactoring all 78 occurrences across 5 files would exceed scope. Other files can adopt the helper incrementally.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F727 | [DONE] | Created ApplyCharacterConfig with the pattern to encapsulate |
| Related | F728 | [WIP] | Will extend CharacterConfig model; helper must support additional field types |
| Related | F729 | [PROPOSED] | Game runtime YAML loading; operates on CharacterTemplate (not CharacterData), so does not use VariableDataAccessor |
| Successor | F732 | [DRAFT] | Adopts VariableDataAccessor in remaining 4 Headless files (55 occurrences) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| VariableCode enum | Engine internal | LOW | Stable enum, no changes expected |
| CharacterData class | Engine internal | LOW | Internal sealed class; helper placed in same project |
| VariableData class | Engine internal | LOW | Same project; same access pattern |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs | HIGH | ApplyCharacterConfig (write), FindCharacterByNameOrNo (read), GetVariableValue (read) - 24 occurrences |
| engine/Assets/Scripts/Emuera/Headless/StateInjector.cs | MEDIUM | SetFlag, SetTarget, SetMaster, SetTflag, SetCflag, SetBase, SetTalent, SetAbl, SetTequip, SetPalam, SetMark, SetExp, SetTcvar, SetResult, SetArg - 27 occurrences |
| engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs | MEDIUM | Variable reading/writing for interactive mode - 23 occurrences |
| engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs | LOW | CallName resolution and variable lookup - 5 occurrences |
| engine/Assets/Scripts/Emuera/Headless/VariableResolver.cs | LOW | Character name lookup and global variable reading - 5 occurrences |
| F729 (future) | LOW | Operates on CharacterTemplate (not CharacterData), so does not use VariableDataAccessor |

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs` (or similar) | Create | New helper class encapsulating VariableCode bitwise access patterns |
| `engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs` | Update | Refactor ApplyCharacterConfig to use helper instead of raw DataString/DataIntegerArray access |
| `engine.Tests/` (new test file) | Create | Unit tests for the helper class |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| CharacterData is `internal sealed` | engine/GameData/Variable/CharacterData.cs | MEDIUM - Helper must be in same assembly or use InternalsVisibleTo |
| VariableCode is a flags enum with bitwise semantics | engine/GameData/Variable/ | LOW - Helper encapsulates this complexity |
| Headless code is conditional (`#if HEADLESS_MODE`) | Engine build configuration | LOW - Helper can be unconditional since it uses core types |
| Engine is a separate git repo | Project structure | LOW - Standard engine modification pattern |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Helper API doesn't cover all access patterns (read vs write, global vs character) | LOW | MEDIUM | Survey all 78 occurrences to design comprehensive API; start with write-only for ApplyCharacterConfig |
| Refactoring ApplyCharacterConfig breaks KojoTestRunner tests | LOW | HIGH | Run F181 regression tests (160/160 PASS baseline from F727); F105 tests (21 scenarios) |
| F728/F729 need different helper API than designed | LOW | MEDIUM | Design helper with extensibility in mind (dictionary-based bulk operations); F728 adds fields, not new patterns |
| Performance regression from method call overhead | LOW | LOW | Inline-able methods; no observable performance impact for test/initialization code |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "manipulation pattern **should be encapsulated** in an engine helper" | Helper class must exist with encapsulation methods | AC#1, AC#2, AC#3, AC#4, AC#5 |
| "creates **tight coupling** between KojoTestRunner and CharacterData's internal structure" | ApplyCharacterConfig must no longer use raw bitwise access | AC#7, AC#8 |
| "for **reusability** and maintainability" | Helper must be usable by other Headless consumers (not private/nested) | AC#6 |
| "Encapsulate **VariableCode access patterns**" (Goal #2) | Helper methods abstract away `(int)(VariableCode.__LOWERCASE__ & VariableCode.X)` pattern | AC#3, AC#4, AC#5, AC#14 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VariableDataAccessor.cs exists | file | Glob | exists | engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs | [x] |
| 2 | VariableDataAccessor class defined with XML doc | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `internal static class VariableDataAccessor` | [x] |
| 3 | SetString method exists | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `public static void SetString\(CharacterData` | [x] |
| 4 | SetIntegerArray method exists | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `public static void SetIntegerArray\(CharacterData` | [x] |
| 5 | GetString method exists | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `public static string GetString\(CharacterData` | [x] |
| 6 | Namespace is MinorShift.Emuera.Headless | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | contains | `namespace MinorShift.Emuera.Headless` | [x] |
| 7 | ApplyCharacterConfig uses helper for string writes | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs) | matches | `VariableDataAccessor\.SetString\(` | [x] |
| 8 | ApplyCharacterConfig no longer uses raw DataString write | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs) | not_matches | `chara\.DataString\[.*VariableCode\.__LOWERCASE__.*\] =` | [x] |
| 9 | Unit test file exists | file | Glob | exists | engine.Tests/Tests/VariableDataAccessorTests.cs | [x] |
| 10 | Unit tests pass | test | dotnet test engine.Tests --filter FullyQualifiedName~VariableDataAccessorTests | succeeds | - | [x] |
| 11 | Build succeeds | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 12 | Negative test: invalid VariableCode handled | code | Grep(engine.Tests/Tests/VariableDataAccessorTests.cs) | matches | `NullCharacter|NullArray|OutOfBounds|NegativeIndex` | [x] |
| 13 | Zero technical debt | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs,engine.Tests/Tests/VariableDataAccessorTests.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 14 | GetIntegerArray method exists | code | Grep(engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs) | matches | `public static long GetIntegerArray\(CharacterData` | [x] |
| 15 | InternalsVisibleTo attribute exists | code | Grep(engine) | matches | `InternalsVisibleTo.*uEmuera\.Tests` | [x] |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Create a new `VariableDataAccessor` helper class in the Headless subsystem to encapsulate the VariableCode bitwise access pattern `(int)(VariableCode.__LOWERCASE__ & code)`. The helper provides strongly-typed methods for reading and writing CharacterData fields without exposing the internal DataString/DataIntegerArray structure.

**Design Philosophy**:
- **Encapsulation**: Hide the bitwise masking pattern `(int)(VariableCode.__LOWERCASE__ & code)` behind well-named methods
- **Type Safety**: Separate methods for string vs integer array access to prevent type errors
- **Null Safety**: Follow the existing pattern in ApplyCharacterConfig with null checks and bounds validation
- **Minimal Surface Area**: Start with write operations (SetString, SetIntegerArray) and read operations (GetString) needed by ApplyCharacterConfig. Read operations for integer arrays can be added in future features if needed.
- **No Exceptions for Invalid Input**: Mirror the existing defensive pattern (null array checks, bounds checks) without throwing exceptions. Invalid operations silently fail after logging.

**Placement**:
- File: `engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Namespace: `MinorShift.Emuera.Headless`
- Class: `internal static class VariableDataAccessor`

**Method Signatures**:

```csharp
namespace MinorShift.Emuera.Headless
{
    /// <summary>
    /// Helper class for accessing CharacterData internal arrays using VariableCode.
    /// Encapsulates the bitwise pattern (int)(VariableCode.__LOWERCASE__ & code).
    /// </summary>
    internal static class VariableDataAccessor
    {
        // Write operations
        public static void SetString(CharacterData chara, VariableCode code, string value);
        public static void SetIntegerArray(CharacterData chara, VariableCode code, int index, long value);

        // Read operations
        public static string GetString(CharacterData chara, VariableCode code);
        public static long GetIntegerArray(CharacterData chara, VariableCode code, int index);
    }
}
```

**Why `internal static`**:
- `internal`: CharacterData is `internal sealed`, so the accessor must be in the same assembly to access it
- `static`: No instance state needed; all methods are stateless utilities

**Error Handling Strategy**:
- **No exceptions thrown**: Follow the existing pattern in ApplyCharacterConfig which uses defensive null/bounds checks
- **Validation**: Each method validates null references and array bounds before accessing
- **Silent failure**: Invalid operations return early without throwing (write methods) or return default values (read methods)
- **Logging**: Error conditions are not logged by the helper itself (logging is the caller's responsibility)

### AC Coverage Table

| Design Decision | Satisfies AC# | Rationale |
|-----------------|---------------|-----------|
| Create VariableDataAccessor.cs in Headless folder | AC#1, AC#6 | File existence + namespace requirement |
| Define `internal static class VariableDataAccessor` | AC#2 | Class visibility and static utility pattern |
| Implement SetString method | AC#3 | Encapsulates DataString write pattern |
| Implement SetIntegerArray method | AC#4 | Encapsulates DataIntegerArray write pattern |
| Implement GetString method | AC#5 | Encapsulates DataString read pattern (used by FindCharacterIndex) |
| Refactor ApplyCharacterConfig to use SetString | AC#7 | Proves helper adoption |
| Remove raw DataString write pattern from ApplyCharacterConfig | AC#8 | Proves encapsulation success |
| Create VariableDataAccessorTests.cs | AC#9 | TDD requirement |
| Implement negative tests (null checks, bounds) | AC#12 | Validates defensive pattern |
| Build succeeds | AC#11 | No broken references |
| Tests pass | AC#10 | Helper works correctly |
| No TODO/FIXME/HACK markers | AC#13 | Zero technical debt |

### Key Decisions

**Decision 1: Separate methods for string vs integer arrays (instead of generic method)**

**Rationale**: Type safety and API clarity. The existing code uses two distinct access patterns:
- `DataString[(int)(VariableCode.__LOWERCASE__ & code)]` for NAME, CALLNAME
- `DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & code)][index]` for BASE, MAXBASE, ABL, TALENT, CFLAG

A generic method `Set<T>(code, value)` would require runtime type checking and lose compile-time safety. Separate methods make the API self-documenting.

**Decision 2: Methods accept VariableCode directly (not pre-computed index)**

**Rationale**: Encapsulation of the bitwise pattern. The helper's purpose is to hide `(int)(VariableCode.__LOWERCASE__ & code)` from callers. If methods accepted `int index`, callers would still need to know the masking pattern.

**Decision 3: No bulk/dictionary-based setter (yet)**

**Rationale**: Scope control. While ApplyCharacterConfig iterates over dictionaries (BaseStats, Abilities, etc.), adding a bulk operation like `SetIntegerArrayBulk(code, Dictionary<int, long>)` would exceed the current feature scope. The helper can be extended in future features (F728, F729) if bulk operations prove valuable.

**Decision 4: Return void for write operations (not bool success)**

**Rationale**: Match the existing ApplyCharacterConfig pattern which does not propagate errors upward. The method performs defensive checks (null array, bounds) and silently skips invalid operations. This design choice prioritizes robustness over error reporting for test/initialization code.

**Decision 5: GetString returns string (not string?)**

**Rationale**: Match CharacterData.DataString array behavior which returns `string` (not nullable). The method returns `null` if the code is invalid or array is null, which is the same as accessing an uninitialized DataString slot.

**Decision 6: Scope limited to ApplyCharacterConfig refactoring**

**Rationale**: Feature feasibility constraint (~300 line limit). The bitwise pattern appears 78 times across 5 files. Refactoring all occurrences would exceed scope. This feature creates the helper and demonstrates its value by refactoring ApplyCharacterConfig (23 occurrences). Other files can adopt the helper incrementally:
- StateInjector (26 occurrences) - future feature
- InteractiveRunner (21 occurrences) - future feature
- KojoExpectValidator (4 occurrences) - future feature
- VariableResolver (4 occurrences) - future feature

### Implementation Contract

**File 1: `engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`**

```csharp
// Feature 731: Character Data Structure Encapsulation
// Helper for accessing CharacterData internal arrays using VariableCode

#if HEADLESS_MODE
using System;
using MinorShift.Emuera.GameData.Variable;
using Era.Core.Variables;

namespace MinorShift.Emuera.Headless
{
    /// <summary>
    /// Helper class for accessing CharacterData internal arrays using VariableCode.
    /// Encapsulates the bitwise pattern (int)(VariableCode.__LOWERCASE__ & code).
    /// Feature 731: Created to reduce coupling between Headless code and CharacterData internals.
    /// </summary>
    internal static class VariableDataAccessor
    {
        /// <summary>
        /// Set a string value in CharacterData.
        /// Used for NAME, CALLNAME, etc.
        /// </summary>
        /// <param name="chara">Character data to modify</param>
        /// <param name="code">Variable code (e.g., VariableCode.NAME)</param>
        /// <param name="value">Value to set</param>
        public static void SetString(CharacterData chara, VariableCode code, string value)
        {
            if (chara == null || chara.DataString == null)
                return;

            int index = (int)(VariableCode.__LOWERCASE__ & code);
            if (index >= chara.DataString.Length)
                return;

            chara.DataString[index] = value;
        }

        /// <summary>
        /// Set an integer array element in CharacterData.
        /// Used for BASE, MAXBASE, ABL, TALENT, CFLAG, etc.
        /// </summary>
        /// <param name="chara">Character data to modify</param>
        /// <param name="code">Variable code (e.g., VariableCode.BASE)</param>
        /// <param name="arrayIndex">Array index within the variable array</param>
        /// <param name="value">Value to set</param>
        public static void SetIntegerArray(CharacterData chara, VariableCode code, int arrayIndex, long value)
        {
            if (chara == null || chara.DataIntegerArray == null)
                return;

            int varIndex = (int)(VariableCode.__LOWERCASE__ & code);
            if (varIndex >= chara.DataIntegerArray.Length)
                return;

            var array = chara.DataIntegerArray[varIndex];
            if (array == null || arrayIndex < 0 || arrayIndex >= array.Length)
                return;

            array[arrayIndex] = value;
        }

        /// <summary>
        /// Get a string value from CharacterData.
        /// Used for NAME, CALLNAME lookups.
        /// </summary>
        /// <param name="chara">Character data to read</param>
        /// <param name="code">Variable code (e.g., VariableCode.NAME)</param>
        /// <returns>String value or null if invalid</returns>
        public static string GetString(CharacterData chara, VariableCode code)
        {
            if (chara == null || chara.DataString == null)
                return null;

            int index = (int)(VariableCode.__LOWERCASE__ & code);
            if (index >= chara.DataString.Length)
                return null;

            return chara.DataString[index];
        }

        /// <summary>
        /// Get an integer array element from CharacterData.
        /// Used for BASE, ABL, TALENT, CFLAG lookups.
        /// </summary>
        /// <param name="chara">Character data to read</param>
        /// <param name="code">Variable code (e.g., VariableCode.BASE)</param>
        /// <param name="arrayIndex">Array index within the variable array</param>
        /// <returns>Integer value or 0 if invalid</returns>
        public static long GetIntegerArray(CharacterData chara, VariableCode code, int arrayIndex)
        {
            if (chara == null || chara.DataIntegerArray == null)
                return 0;

            int varIndex = (int)(VariableCode.__LOWERCASE__ & code);
            if (varIndex >= chara.DataIntegerArray.Length)
                return 0;

            var array = chara.DataIntegerArray[varIndex];
            if (array == null || arrayIndex < 0 || arrayIndex >= array.Length)
                return 0;

            return array[arrayIndex];
        }
    }
}
#endif
```

**File 2: Refactored `engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs` (ApplyCharacterConfig method only)**

**Before** (lines 1017-1067):
```csharp
private static void ApplyCharacterConfig(int characterListIndex, CharacterConfig config)
{
    var varData = GlobalStatic.VariableData;
    var charList = varData.CharacterList;

    if (characterListIndex < 0 || characterListIndex >= charList.Count) return;

    var chara = charList[characterListIndex];

    // Set Name
    chara.DataString[(int)(VariableCode.__LOWERCASE__ & VariableCode.NAME)] = config.Name;

    // Set CallName
    chara.DataString[(int)(VariableCode.__LOWERCASE__ & VariableCode.CALLNAME)] = config.CallName;

    // Set BaseStats (both BASE and MAXBASE arrays like original CSV loading)
    foreach (var kvp in config.BaseStats)
    {
        var baseArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.BASE)];
        if (baseArray != null && kvp.Key < baseArray.Length)
            baseArray[kvp.Key] = kvp.Value;

        var maxbaseArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.MAXBASE)];
        if (maxbaseArray != null && kvp.Key < maxbaseArray.Length)
            maxbaseArray[kvp.Key] = kvp.Value;
    }

    // Set Abilities
    foreach (var kvp in config.Abilities)
    {
        var ablArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.ABL)];
        if (ablArray != null && kvp.Key < ablArray.Length)
            ablArray[kvp.Key] = kvp.Value;
    }

    // Set Talents
    foreach (var kvp in config.Talents)
    {
        var talentArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.TALENT)];
        if (talentArray != null && kvp.Key < talentArray.Length)
            talentArray[kvp.Key] = kvp.Value;
    }

    // Set Character Flags
    foreach (var kvp in config.Flags)
    {
        var cflagArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.CFLAG)];
        if (cflagArray != null && kvp.Key < cflagArray.Length)
            cflagArray[kvp.Key] = kvp.Value;
    }
}
```

**After** (refactored with helper):
```csharp
private static void ApplyCharacterConfig(int characterListIndex, CharacterConfig config)
{
    var varData = GlobalStatic.VariableData;
    var charList = varData.CharacterList;

    if (characterListIndex < 0 || characterListIndex >= charList.Count) return;

    var chara = charList[characterListIndex];

    // Set Name
    VariableDataAccessor.SetString(chara, VariableCode.NAME, config.Name);

    // Set CallName
    VariableDataAccessor.SetString(chara, VariableCode.CALLNAME, config.CallName);

    // Set BaseStats (both BASE and MAXBASE arrays like original CSV loading)
    foreach (var kvp in config.BaseStats)
    {
        VariableDataAccessor.SetIntegerArray(chara, VariableCode.BASE, kvp.Key, kvp.Value);
        VariableDataAccessor.SetIntegerArray(chara, VariableCode.MAXBASE, kvp.Key, kvp.Value);
    }

    // Set Abilities
    foreach (var kvp in config.Abilities)
    {
        VariableDataAccessor.SetIntegerArray(chara, VariableCode.ABL, kvp.Key, kvp.Value);
    }

    // Set Talents
    foreach (var kvp in config.Talents)
    {
        VariableDataAccessor.SetIntegerArray(chara, VariableCode.TALENT, kvp.Key, kvp.Value);
    }

    // Set Character Flags
    foreach (var kvp in config.Flags)
    {
        VariableDataAccessor.SetIntegerArray(chara, VariableCode.CFLAG, kvp.Key, kvp.Value);
    }
}
```

**File 3: `engine.Tests/Tests/VariableDataAccessorTests.cs`**

```csharp
using Xunit;
using MinorShift.Emuera.Headless;
using MinorShift.Emuera.GameData.Variable;
using Era.Core.Variables;

namespace MinorShift.Emuera.Tests
{
    /// <summary>
    /// Unit tests for VariableDataAccessor helper class.
    /// Feature 731: Validates encapsulation of VariableCode bitwise access pattern.
    /// </summary>
    public class VariableDataAccessorTests
    {
        [Fact]
        public void SetString_ValidCode_SetsValue()
        {
            // Test that SetString correctly writes to DataString array
            // Expected: chara.DataString[(int)(VariableCode.__LOWERCASE__ & VariableCode.NAME)] == "TestName"
        }

        [Fact]
        public void SetIntegerArray_ValidCode_SetsValue()
        {
            // Test that SetIntegerArray correctly writes to DataIntegerArray
            // Expected: chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.BASE)][0] == 100
        }

        [Fact]
        public void GetString_ValidCode_ReturnsValue()
        {
            // Test that GetString correctly reads from DataString array
        }

        [Fact]
        public void GetIntegerArray_ValidCode_ReturnsValue()
        {
            // Test that GetIntegerArray correctly reads from DataIntegerArray
        }

        [Fact]
        public void SetString_NullCharacter_DoesNotThrow()
        {
            // Negative test: null CharacterData should be handled gracefully
            // Expected: No exception thrown
        }

        [Fact]
        public void SetIntegerArray_NullArray_DoesNotThrow()
        {
            // Negative test: null array within CharacterData should be handled gracefully
            // Expected: No exception thrown
        }

        [Fact]
        public void SetIntegerArray_OutOfBounds_DoesNotThrow()
        {
            // Negative test: array index out of bounds should be handled gracefully
            // Expected: No exception thrown
        }

        [Fact]
        public void SetIntegerArray_NegativeIndex_DoesNotThrow()
        {
            // Negative test: negative array index should be handled gracefully
            // Expected: No exception thrown
        }

        [Fact]
        public void GetString_NullCharacter_ReturnsNull()
        {
            // Negative test: null CharacterData should return null
        }

        [Fact]
        public void GetIntegerArray_OutOfBounds_ReturnsZero()
        {
            // Negative test: array index out of bounds should return 0
        }

        [Fact]
        public void GetIntegerArray_NegativeIndex_ReturnsZero()
        {
            // Negative test: negative array index should return 0
        }
    }
}
```

**Test Naming Convention**: `{MethodName}_{Scenario}_{ExpectedResult}` (e.g., `SetString_ValidCode_SetsValue`)

**Test Strategy**:
- Positive tests verify correct write/read operations using actual VariableCode values
- Negative tests verify defensive null/bounds checks don't throw exceptions
- All tests use real CharacterData instances (created via constructor, not mocked) to validate actual array access
- AC#12 is satisfied by the presence of negative test method names matching the pattern `null.*array|bounds|ArgumentNullException|IndexOutOfRange|NullReference`

**Build Order**:
1. Create VariableDataAccessor.cs with helper methods
2. Create VariableDataAccessorTests.cs with unit tests (RED state - tests fail because implementation is incomplete)
3. Implement helper methods fully (GREEN state - tests pass)
4. Refactor KojoTestRunner.ApplyCharacterConfig to use helper
5. Run F181 regression tests (160/160 PASS) and F105 tests (21 scenarios) to verify no breakage

**Code Placement**: All files are within the `engine` project (separate git repo). No cross-project dependencies needed.

**Verification Strategy**:
- AC#1-6: File/class structure verification via Glob/Grep
- AC#7-8: KojoTestRunner refactoring verification via Grep patterns
- AC#9: Test file existence via Glob
- AC#10: Test execution via `dotnet test --filter FullyQualifiedName~VariableDataAccessorTests`
- AC#11: Build verification via `dotnet build engine/uEmuera.Headless.csproj`
- AC#12: Negative test existence via Grep for test method names
- AC#13: Technical debt check via Grep for `TODO|FIXME|HACK`

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 15 | Add InternalsVisibleTo attribute for engine.Tests access to VariableDataAccessor | [x] |
| 1 | 9 | Create VariableDataAccessorTests.cs with test structure (using xunit.v3) | [x] |
| 2 | 1,2,3,4,5,6,14 | Create VariableDataAccessor.cs with SetString, SetIntegerArray, GetString, GetIntegerArray methods | [x] |
| 3 | 10,12 | Implement unit tests (positive and negative cases) and verify all tests pass (11 test methods) | [x] |
| 4 | 7,8 | Refactor KojoTestRunner.ApplyCharacterConfig to use VariableDataAccessor helper | [x] |
| 5 | 11 | Verify build succeeds with refactored code | [x] |
| 6 | 13 | Verify zero technical debt in created files | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Test structure from Technical Design | VariableDataAccessorTests.cs skeleton |
| 2 | implementer | sonnet | T2 | Helper method signatures from Technical Design | VariableDataAccessor.cs implementation |
| 3 | implementer | sonnet | T3 | Test implementation from Technical Design | Complete unit tests (9 test methods) passing |
| 4 | implementer | sonnet | T4 | Refactoring pattern from Technical Design | Updated KojoTestRunner.ApplyCharacterConfig |
| 5 | ac-tester | haiku | T5,T6 | Build and tech debt commands from ACs | Verification results |

**Test Naming Convention**: Test methods follow `{MethodName}_{Scenario}_{ExpectedResult}` format:
- `SetString_ValidCode_SetsValue`
- `SetIntegerArray_ValidCode_SetsValue`
- `GetString_ValidCode_ReturnsValue`
- `GetIntegerArray_ValidCode_ReturnsValue`
- `SetString_NullCharacter_DoesNotThrow`
- `SetIntegerArray_NullArray_DoesNotThrow`
- `SetIntegerArray_OutOfBounds_DoesNotThrow`
- `SetIntegerArray_NegativeIndex_DoesNotThrow`
- `GetString_NullCharacter_ReturnsNull`
- `GetIntegerArray_OutOfBounds_ReturnsZero`
- `GetIntegerArray_NegativeIndex_ReturnsZero`

**Constraints** (from Technical Design):
1. Helper methods must encapsulate `(int)(VariableCode.__LOWERCASE__ & code)` pattern
2. Defensive null/bounds checks required (no exceptions thrown)
3. `internal static` class in MinorShift.Emuera.Headless namespace
4. File must be within `#if HEADLESS_MODE` conditional

**Pre-conditions**:
- CharacterData class accessible (same assembly)
- VariableCode enum available in Era.Core.Variables
- F727 baseline: ApplyCharacterConfig exists with raw bitwise access pattern
- engine.Tests project configured with NUnit

**Success Criteria**:
- All 13 ACs pass verification
- Unit tests: 11 test methods, all passing (AC#10)
- Build succeeds with no errors (AC#11)
- ApplyCharacterConfig no longer uses raw DataString write pattern (AC#8)
- F181 regression tests still pass (160/160 PASS baseline maintained)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|---------------|
| Refactor remaining Headless files (71 occurrences total): StateInjector, InteractiveRunner, KojoExpectValidator, VariableResolver, ScenarioParser, and KojoTestRunner non-ApplyCharacterConfig methods; add VariableData overloads | Out of scope for F731; incremental adoption approach | Feature | F732 | - (file exists) |

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-01 | AC Verification | All 15 ACs verified and passing (OK:15/15) |

---

### AC Details

**AC#1: VariableDataAccessor.cs exists**
- Test: Glob pattern=`engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Expected: File exists
- Rationale: The helper class is the primary artifact of this feature. File must exist before any content verification.

**AC#2: VariableDataAccessor class defined with XML doc**
- Test: Grep pattern=`internal static class VariableDataAccessor` path=`engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Expected: Match found
- Rationale: Class must be `internal static` to match engine conventions (same as StateInjector). `internal` because CharacterData is `internal sealed` and the helper must access it within the same assembly. `static` because it is a stateless utility with no instance state.

**AC#3: SetString method exists**
- Test: Grep pattern=`public static void SetString\(CharacterData` path=`engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Expected: Match found
- Rationale: Encapsulates `chara.DataString[(int)(VariableCode.__LOWERCASE__ & code)] = value` pattern. Used by ApplyCharacterConfig for NAME and CALLNAME writes.

**AC#4: SetIntegerArray method exists**
- Test: Grep pattern=`public static void SetIntegerArray\(CharacterData` path=`engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Expected: Match found
- Rationale: Encapsulates `chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & code)][index] = value` pattern. Used by ApplyCharacterConfig for BASE, MAXBASE, ABL, TALENT, CFLAG writes. This is the most frequently duplicated pattern (array element write).

**AC#5: GetString method exists**
- Test: Grep pattern=`public static string GetString\(CharacterData` path=`engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Expected: Match found
- Rationale: Read complement of SetString. Used by FindCharacterIndex for NAME and CALLNAME lookups. Encapsulates `chara.DataString[(int)(VariableCode.__LOWERCASE__ & code)]` read pattern.

**AC#6: Namespace is MinorShift.Emuera.Headless**
- Test: Grep pattern=`namespace MinorShift.Emuera.Headless` path=`engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Expected: Match found
- Rationale: Must be in same namespace as KojoTestRunner, StateInjector, and other Headless consumers for discoverability and consistent architecture. Same assembly as CharacterData for internal access.

**AC#7: ApplyCharacterConfig uses helper for string writes**
- Test: Grep pattern=`VariableDataAccessor\.SetString\(` path=`engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs`
- Expected: Match found
- Rationale: Verifies the refactoring actually happened — ApplyCharacterConfig delegates to the helper instead of using raw DataString indexing.

**AC#8: ApplyCharacterConfig no longer uses raw DataString write**
- Test: Grep pattern=`chara\.DataString\[.*VariableCode\.__LOWERCASE__.*\] =` path=`engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs`
- Expected: 0 matches (not_matches)
- Rationale: Negative test — confirms the raw bitwise access pattern for DataString writes has been replaced in ApplyCharacterConfig. Note: Pattern targets write operations only (` = ` suffix). All DataString writes in KojoTestRunner are within ApplyCharacterConfig scope as of F727 baseline. Other methods (FindCharacterIndex, GetVariableValue) use reads (no assignment), so they do not match this pattern.

**AC#9: Unit test file exists**
- Test: Glob pattern=`engine.Tests/Tests/VariableDataAccessorTests.cs`
- Expected: File exists
- Rationale: TDD requirement — unit tests must exist for the helper class.

**AC#10: Unit tests pass**
- Test: `dotnet test engine.Tests --filter FullyQualifiedName~VariableDataAccessorTests`
- Expected: All tests pass
- Rationale: Verifies helper methods work correctly with actual VariableCode bitwise operations.
- Test naming convention: Test methods follow `{MethodName}_{Scenario}_{ExpectedResult}` format (e.g., `SetString_ValidCode_SetsValue`, `SetIntegerArray_NullArray_DoesNotThrow`).

**AC#11: Build succeeds**
- Test: `dotnet build engine/uEmuera.Headless.csproj`
- Expected: Build succeeds with 0 errors
- Rationale: Verifies the refactored KojoTestRunner compiles correctly with helper references and no broken dependencies.

**AC#12: Negative test — invalid input handled**
- Test: Grep pattern=`NullCharacter|NullArray|OutOfBounds|NegativeIndex` path=`engine.Tests/Tests/VariableDataAccessorTests.cs`
- Expected: Match found
- Rationale: Helper methods must handle edge cases gracefully: null CharacterData, null arrays, out-of-bounds indices. At least one negative test must exist verifying the helper does not throw unhandled exceptions on invalid input. This mirrors the null-check pattern already present in ApplyCharacterConfig (`if (baseArray != null && kvp.Key < baseArray.Length)`).

**AC#13: Zero technical debt**
- Test: Grep pattern=`TODO|FIXME|HACK` paths=[engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs, engine.Tests/Tests/VariableDataAccessorTests.cs]
- Expected: 0 matches
- Rationale: All created files must be free of debt markers. Covers both the helper class and its test file.

**AC#14: GetIntegerArray method exists**
- Test: Grep pattern=`public static long GetIntegerArray\(CharacterData` path=`engine/Assets/Scripts/Emuera/Headless/VariableDataAccessor.cs`
- Expected: Match found
- Rationale: Read complement of SetIntegerArray. Encapsulates `chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & code)][index]` read pattern. Defined in Technical Design but was missing from AC table in earlier iterations.

**AC#15: InternalsVisibleTo attribute exists**
- Test: Grep pattern=`InternalsVisibleTo.*uEmuera\.Tests` path=`engine`
- Expected: Match found
- Rationale: Required for unit tests to access internal VariableDataAccessor class from engine.Tests project. Without this attribute, test compilation fails with accessibility errors.
