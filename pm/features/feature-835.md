<!-- fc-phase-1-completed -->

# Feature 835: IEngineVariables Abstract Method Stubs — Real VariableData Delegation

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T04:57:17Z -->

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

## Type: engine

## Background

### Philosophy (Mid-term Vision)
Era.Core interfaces are the SSOT for cross-repo contracts between the core library and the engine runtime. Every interface method must have a concrete runtime implementation that delegates to actual engine data, ensuring that COM/dialogue systems and game logic receive real values from VariableData rather than hardcoded defaults.

### Problem (Current Issue)
F833 deliberately scoped its implementation to 9 DIM stub methods only (EngineVariablesImpl.cs:96-98 comment: "Real delegation is out of scope (tracked in F835)"), because the remaining 23 methods require three distinct delegation patterns beyond the simple GetArray approach used for DIM variables. Specifically: (1) global scalar variables (RESULT, MONEY, MASTER, ASSI, COUNT, TARGET, PLAYER, SELECTCOM) need GetArray(VariableCode.X)[0] access, (2) character-scoped variables (NAME, CALLNAME, ISASSI, NO) need CharacterList[index].DataString/DataInteger lookup, and (3) computed variables (CHARANUM, RAND) need CharacterList.Count and VEvaluator.GetNextRand() respectively. All 23 methods currently return 0 or empty string, causing any downstream consumer to receive incorrect default values instead of actual engine state.

### Goal (What to Achieve)
Replace all 23 remaining no-op stub methods in EngineVariablesImpl with real VariableData delegation, implementing the three category-specific patterns (scalar via GetArray, character-scoped via CharacterList, computed via Count/GetNextRand), with null-guard safety for all paths.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do 23 IEngineVariables methods return 0/no-op? | Because F833 created them as compilation placeholders with stub bodies | EngineVariablesImpl.cs:101-167 |
| 2 | Why did F833 create stubs instead of real implementations? | Because F833 was scoped to DIM variable stubs only (9 methods) | EngineVariablesImpl.cs:96-98 tracking comment |
| 3 | Why was F833 limited to DIM stubs? | Because the remaining methods require three distinct delegation patterns beyond DIM's GetArray approach | VariableCode enum, CharacterList access, VEvaluator |
| 4 | Why do the methods need different patterns? | Because the engine stores scalars in DataIntegerArray, character data in CharacterList[idx].DataString/DataInteger, and computed values (CHARANUM/RAND) have special accessor logic | VariableEvaluator.cs:2640-2762, VariableDataAccessor.cs:25-78 |
| 5 | Why (Root)? | Incremental delivery strategy: F833 proved the adapter pattern with DIM stubs, deferring the remaining three categories to a dedicated feature (F835) | F833 design decision, IEngineVariables.cs:1-131 (31 total methods) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 23 methods return 0/empty string instead of real engine data | F833 incremental scope limited to DIM stubs; remaining methods need category-specific delegation patterns |
| Where | EngineVariablesImpl.cs:101-167 (stub method bodies) | Three distinct data access patterns in engine: DataIntegerArray, CharacterList, VEvaluator |
| Fix | Hardcode expected values per method | Implement proper delegation using GetArray (scalars), CharacterList (character-scoped), Count/GetNextRand (computed) |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F833 | [DONE] | Predecessor: created EngineVariablesImpl with DIM stubs and stub bodies for remaining methods |
| F838 | [PROPOSED] | Sibling: test infrastructure improvements (no blocking dependency) |
| F790 | [DONE] | Historical: IEngineVariables interface definition |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Scalar delegation pattern | FEASIBLE | GetArray(VariableCode.X)[0] proven by F833 DIM stubs (EngineVariablesImpl.cs:25-93) |
| Character-scoped delegation | FEASIBLE | CharacterList[idx] pattern proven in VariableDataAccessor.cs:25-78 |
| CHARANUM computed value | FEASIBLE | Direct CharacterList.Count (VariableEvaluator.cs:2712-2714) |
| RAND computed value | FEASIBLE | GlobalStatic.VEvaluator.GetNextRand(max) available (GlobalStatic.cs:41) |
| Test coverage | FEASIBLE | Null-path tests primary; round-trip tests require game filesystem (Skip-annotated) |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| EngineVariablesImpl | HIGH | 23 methods modified from no-op stubs to real delegation |
| EngineVariablesImplTests | HIGH | Null-path tests added for all 23 methods; round-trip tests Skip-annotated |
| IEngineVariables consumers | MEDIUM | Downstream code will receive real values instead of 0/empty |
| IEngineVariables.cs | LOW | No changes needed to interface definition |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| VariableData requires game filesystem for construction | Engine architecture | Round-trip tests must be Skip-annotated in xUnit |
| VariableDataAccessor is #if HEADLESS_MODE only | Conditional compilation | Must inline CharacterList access pattern, not call accessor directly |
| GlobalStatic.VariableData can be null at runtime | Engine initialization order | All methods must null-guard and return default on null |
| RAND requires VEvaluator or separate random source | GlobalStatic.VEvaluator field | GetRandom needs null-guard on VEvaluator separately from VariableData |
| Character methods need bounds checking | CharacterList is indexed | Out-of-bounds index must return default, not throw |
| CAN_FORBID variables return -1 when forbidden | Engine convention (VariableEvaluator.cs:2720-2762) | IEngineVariables uses simpler GetArray pattern; -1 vs 0 semantic difference noted |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| RAND delegation architecture: VEvaluator may be null separately from VariableData | HIGH | MEDIUM | Add separate null-guard for GlobalStatic.VEvaluator; return 0 when null |
| Character index out-of-bounds on CharacterList access | MEDIUM | HIGH | Bounds-check index against CharacterList.Count before access; return default on invalid |
| CAN_FORBID -1 vs 0 semantic mismatch between engine convention and interface | LOW | MEDIUM | Use GetArray pattern consistently; document semantic difference in code comment |
| Test isolation: null-path tests may not catch delegation wiring errors | MEDIUM | LOW | Combine null-path tests (primary) with Skip-annotated round-trip tests (secondary) for future validation |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Stub method count | Grep for "return 0\|return string.Empty\|return null" in EngineVariablesImpl.cs | 23 methods (lines 101-167) | All should be replaced with real delegation |
| Existing DIM delegation count | Grep for "GetArray" in EngineVariablesImpl.cs | 9 methods (lines 25-93) | Already implemented by F833, not in scope |
| Test count | dotnet test EngineVariablesImplTests | F833 baseline | New null-path tests to be added |

**Baseline File**: `_out/tmp/baseline-835.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | All 23 methods must null-guard on GlobalStatic.VariableData | Engine initialization order | Each method needs null-path test returning default value |
| C2 | Round-trip tests not feasible in xUnit | VariableData requires game filesystem | Round-trip tests must be Skip-annotated; null-path is primary verification |
| C3 | Three delegation categories need distinct verification | EngineVariablesImpl architecture | ACs must cover scalar (GetArray), character-scoped (CharacterList), and computed (Count/GetNextRand) separately |
| C4 | GetRandom needs separate VEvaluator null-guard | GlobalStatic.VEvaluator independent of VariableData | AC must verify GetRandom behavior when VEvaluator is null |
| C5 | Character-scoped methods need bounds checking | CharacterList is indexed collection | AC must verify out-of-bounds index returns default |
| C6 | Exactly 23 methods to convert | IEngineVariables.cs method count minus 9 DIM stubs | AC must verify no stub bodies remain (not_matches for return 0 pattern in stub region) |
| C7 | No IEngineVariables.cs changes | Interface is stable from F790/F833 | AC must verify interface file unchanged |

### Constraint Details

**C1: Null-Guard on VariableData**
- **Source**: GlobalStatic.VariableData is null before engine initialization completes (EngineVariablesImpl.cs:27 existing null check)
- **Verification**: Grep for null-guard pattern in each method body
- **AC Impact**: Every method AC must include null-path behavior verification

**C2: Round-Trip Test Limitation**
- **Source**: VariableData constructor requires game filesystem paths that are unavailable in xUnit CI
- **Verification**: Run dotnet test without GAME_PATH -- Skip-annotated tests should not fail
- **AC Impact**: Primary verification via null-path tests; round-trip tests are Skip-annotated secondary

**C3: Three Delegation Categories**
- **Source**: Investigation consensus -- scalar (14 methods), character-scoped (7 methods), computed (2 methods)
- **Verification**: Code review of delegation targets per category
- **AC Impact**: ACs must include at least one representative method from each category

**C4: VEvaluator Independence**
- **Source**: GlobalStatic.VEvaluator (GlobalStatic.cs:41) is a separate field from VariableData
- **Verification**: GetRandom with null VEvaluator returns 0
- **AC Impact**: Dedicated AC for GetRandom null-path

**C5: Character Bounds Checking**
- **Source**: CharacterList is indexed; invalid index must not throw
- **Verification**: Test with index >= CharacterList.Count
- **AC Impact**: AC for boundary behavior of character-scoped methods

**C6: Complete Stub Elimination**
- **Source**: Method-to-VariableCode mapping from investigation (23 methods enumerated)
- **Verification**: Grep stub region for remaining no-op returns
- **AC Impact**: count_equals or not_matches AC to verify no stubs remain

**C7: Interface Stability**
- **Source**: IEngineVariables.cs is defined by Era.Core NuGet 1.0.0
- **Verification**: File hash or method count unchanged
- **AC Impact**: Negative AC verifying no interface modifications

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F833 | [DONE] | Creates EngineVariablesImpl with DIM stubs and stub bodies for abstract methods |
| Related | F838 | [WIP] | Test infrastructure improvements (no blocking dependency) |
| Related | F790 | [DONE] | IEngineVariables interface definition |

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

---

<!-- fc-phase-2-completed -->

<!-- fc-phase-3-completed -->

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Every interface method must have a concrete runtime implementation that delegates to actual engine data" | All 23 stub methods must be replaced with real VariableData delegation | AC#1, AC#2, AC#3 |
| "ensuring that COM/dialogue systems and game logic receive real values from VariableData rather than hardcoded defaults" | No stub return patterns (=> 0, => string.Empty, { }) remain in the stub region | AC#4 |
| "Every interface method must have a concrete runtime implementation" | Null-guard safety on all delegation paths | AC#5, AC#6, AC#7, AC#8, AC#9, AC#11 |
| "Every interface method must have a concrete runtime implementation that delegates to actual engine data" | Setter methods must write to VariableData (not remain no-op) | AC#10, AC#16 |
| "Every interface method must have a concrete runtime implementation that delegates to actual engine data" | Character-scoped methods must bounds-check indexed access | AC#12 |
| "Every interface method must have a concrete runtime implementation that delegates to actual engine data" | F833 tracking comment must be removed since stubs are replaced | AC#13 |
| "Every interface method must have a concrete runtime implementation" | Implementation must compile successfully | AC#14 |
| "Era.Core interfaces are the SSOT for cross-repo contracts" | IEngineVariables.cs interface must remain unmodified | AC#15 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Scalar methods delegate via GetArray with VariableCode | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetArray\(VariableCode\.(RESULT|MONEY|MASTER|ASSI|COUNT|TARGET|PLAYER|SELECTCOM)\)") | gte | 14 | [x] |
| 2 | Character-scoped methods delegate via GetCharacter helper | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetCharacter\(") | gte | 7 | [x] |
| 3 | Computed methods: GetCharaNum uses CharacterList.Count, GetRandom uses GetNextRand | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="CharacterList\.Count|GetNextRand") | gte | 2 | [x] |
| 4 | No stub return patterns remain (whole-file; F833 DIM stubs use full-body syntax, not expression-body) | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="(=> 0;|=> string\.Empty;|=> \"\";|\) \{ \})") | not_matches | N/A | [x] |
| 5 | All methods null-guard on GlobalStatic.VariableData | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GlobalStatic\.VariableData == null") | gte | 23 | [x] |
| 6 | GetRandom null-guards on VEvaluator separately | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="VEvaluator.*null|VEvaluator == null") | matches | N/A | [x] |
| 7 | Null-path tests: scalar getter methods return 0 when VariableData is null | test | dotnet test --filter "FullyQualifiedName~EngineVariablesImplTests.Get" --blame-hang-timeout 10s | succeeds | N/A | [x] |
| 8 | Null-path tests: character-scoped and computed methods return defaults when VariableData is null | test | dotnet test --filter "FullyQualifiedName~EngineVariablesImplTests" --blame-hang-timeout 10s | succeeds | N/A | [x] |
| 9 | Null-path tests: setter methods do not throw when VariableData is null | test | dotnet test --filter "FullyQualifiedName~EngineVariablesImplTests.Set" --blame-hang-timeout 10s | succeeds | N/A | [x] |
| 10 | Scalar setter methods write to GetArray target | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetArray\(VariableCode\.\w+\)\[.*\] =") | gte | 5 | [x] |
| 11 | Total test count includes all 23 new null-path tests | test | dotnet test --filter "FullyQualifiedName~EngineVariablesImplTests" --blame-hang-timeout 10s | gte | 33 | [x] |
| 12 | Character-scoped methods bounds-check index against CharacterList.Count | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="characterIndex.*<.*0|characterIndex.*>=|CharacterList\.Count") | gte | 2 | [x] |
| 13 | F833 tracking comment removed (no longer stubs) | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="Real delegation is out of scope") | not_matches | N/A | [x] |
| 14 | Build succeeds with no errors | build | dotnet build engine/tests/uEmuera.Tests/uEmuera.Tests.csproj | matches | Build succeeded | [x] |
| 15 | IEngineVariables.cs interface unchanged (no modifications) | exit_code | Bash("cd /c/Era/core && git diff --exit-code HEAD -- src/Era.Core/Interfaces/IEngineVariables.cs") | succeeds | N/A | [x] |
| 16 | Character-scoped setter methods write to CharacterData fields | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="chara\.Data(String|Integer)\[.*\] =") | gte | 3 | [x] |

### AC Details

**AC#1: Scalar methods delegate via GetArray with VariableCode**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetArray\(VariableCode\.(RESULT|MONEY|MASTER|ASSI|COUNT|TARGET|PLAYER|SELECTCOM)\)")`
- **Expected**: `gte 14`
- **Derivation**: 14 scalar methods use GetArray: GetResult(1), GetMoney(1), SetMoney(1), GetMaster(1), SetMaster(1), GetAssi(1), GetCount(1), GetTarget()(1), SetTarget(value)(1), GetTarget(index)(1), SetTarget(index,value)(1), GetPlayer(1), SetPlayer(1), GetSelectCom(1) = 14 occurrences. 8 VariableCode enums x multiple accessors = 14 total methods.
- **Rationale**: Each scalar method must delegate to GetArray(VariableCode.X) pattern proven by F833 DIM stubs.

**AC#2: Character-scoped methods delegate via GetCharacter helper**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetCharacter\(")`
- **Expected**: `gte 7`
- **Derivation**: 7 character-scoped methods: GetName(1), GetCallName(1), GetIsAssi(1), GetCharacterNo(1), SetName(1), SetCallName(1), SetCharacterNo(1). Each calls GetCharacter(characterIndex) which centralizes CharacterList access and bounds-checking.
- **Rationale**: Character-scoped variables are stored per-character in CharacterList entries, accessed via the shared GetCharacter() helper. Constraint C3 requires distinct verification for this category.

**AC#3: Computed methods: GetCharaNum uses CharacterList.Count, GetRandom uses GetNextRand**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="CharacterList\.Count|GetNextRand")`
- **Expected**: `gte 2`
- **Derivation**: GetCharaNum delegates to CharacterList.Count (1), GetRandom delegates to GetNextRand (1) = 2 occurrences.
- **Rationale**: Computed variables have no VariableCode backing; they compute values dynamically. Constraint C3 requires separate verification.

**AC#5: All methods null-guard on GlobalStatic.VariableData**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GlobalStatic\.VariableData == null")`
- **Expected**: `gte 23`
- **Derivation**: 7 existing F833 null-guards + 14 new scalar methods (each with own null-guard) + 1 GetCharacter helper (shared by 7 character methods) + 1 GetCharaNum = 23 minimum. GetRandom excluded: RAND depends only on VEvaluator, not VariableData.
- **Rationale**: Constraint C1 requires all methods to null-guard on VariableData. Character-scoped methods share a GetCharacter() helper that centralizes the null-guard. GetRandom has no VariableData dependency (uses VEvaluator only). Conservative floor: 23.

**AC#10: Scalar setter methods write to GetArray target**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetArray\(VariableCode\.\w+\)\[.*\] =")`
- **Expected**: `gte 5`
- **Derivation**: SetMoney(1), SetMaster(1), SetTarget(int)(1), SetTarget(int,int)(1), SetPlayer(1) = 5 scalar setter writes.
- **Rationale**: V2 positive write verification — ensures setters actually write to VariableData, not remain empty-body no-ops.

**AC#11: Total test count includes all 23 new null-path tests**
- **Test**: `dotnet test --filter "FullyQualifiedName~EngineVariablesImplTests" --blame-hang-timeout 10s`
- **Expected**: `gte 33`
- **Derivation**: 10 existing F833 tests + 23 new null-path tests (one per stub method) = 33 total tests.
- **Rationale**: Each of the 23 stub methods requires a null-path test verifying default return value (getters) or no-throw behavior (setters). Constraint C1 drives this requirement.

**AC#12: Character-scoped methods bounds-check index against CharacterList.Count**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="characterIndex.*<.*0|characterIndex.*>=|CharacterList\.Count")`
- **Expected**: `gte 2`
- **Derivation**: Bounds checking requires at least lower-bound check (< 0) and upper-bound check (>= Count). May appear in a helper method shared by all 7 character methods, or inline in each. Minimum 2 distinct pattern matches for the two boundary conditions.
- **Rationale**: Constraint C5 requires out-of-bounds index to return default, not throw. CharacterList is indexed and invalid indices must be handled gracefully.

**AC#15: IEngineVariables.cs interface unchanged (no modifications)**
- **Test**: `Bash("cd /c/Era/core && git diff --exit-code HEAD -- src/Era.Core/Interfaces/IEngineVariables.cs")`
- **Expected**: `succeeds` (exit code 0 = no differences)
- **Derivation**: Constraint C7 requires the interface file to remain unmodified. `git diff --exit-code` returns exit code 0 when no differences exist, exit code 1 when changes are found.
- **Rationale**: IEngineVariables.cs is defined by Era.Core NuGet 1.0.0. This feature modifies only the engine-side implementation (EngineVariablesImpl.cs), not the interface contract.

**AC#16: Character-scoped setter methods write to CharacterData fields**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="chara\.Data(String|Integer)\[.*\] =")`
- **Expected**: `gte 3`
- **Derivation**: SetName writes to `chara.DataString[...]` (1), SetCallName writes to `chara.DataString[...]` (1), SetCharacterNo writes to `chara.DataInteger[...]` (1) = 3 write assignments.
- **Rationale**: Positive write verification for character-scoped setters, analogous to AC#10 for scalar setters. Ensures setters actually mutate CharacterData fields, not remain no-op after bounds-checking.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Replace all 23 remaining no-op stub methods with real VariableData delegation | AC#1, AC#2, AC#3, AC#4, AC#13 |
| 2 | Implement scalar category via GetArray pattern | AC#1, AC#10 |
| 3 | Implement character-scoped category via CharacterList | AC#2, AC#12, AC#16 |
| 4 | Implement computed category via Count/GetNextRand | AC#3, AC#6 |
| 5 | Null-guard safety for all paths | AC#5, AC#6, AC#7, AC#8, AC#9, AC#11 |
| 6 | Implementation compiles successfully | AC#14 |
| 7 | Interface contract preserved (no IEngineVariables.cs changes) | AC#15 |

<!-- fc-phase-4-completed -->

---

## Technical Design

### Approach

Extend `EngineVariablesImpl.cs` by replacing all 23 no-op stub method bodies with real engine data delegation, organized into three category-specific patterns that mirror the existing F833 DIM stub pattern:

**Category A — Scalar methods (14 methods)**: Delegate via the existing `GetArray(VariableCode.X)[0]` helper already proven by F833. Each method null-guards on `GlobalStatic.VariableData`, then calls `GetArray` and reads/writes index `[0]`. For indexed variants (`GetTarget(int)`, `SetTarget(int,int)`), bounds-check against the array length just as F833 does for `GetDay(int)` and `GetTime(int)`.

**Category B — Character-scoped methods (7 methods)**: Delegate via `GlobalStatic.VariableData.CharacterList[characterIndex]`. Name/CallName use `CharacterData.DataString[bitmask_index]`; IsAssi and CharacterNo use `CharacterData.DataInteger[bitmask_index]`. A private helper method `GetCharacter(int characterIndex)` centralizes the null-guard and bounds-check, returning `null` on invalid input and letting each caller return the category-appropriate default.

**Category C — Computed methods (2 methods)**: `GetCharaNum` returns `GlobalStatic.VariableData.CharacterList.Count` (same pattern as `VariableEvaluator.CHARANUM`). `GetRandom` delegates to `GlobalStatic.VEvaluator.GetNextRand(max)` with a null-guard on `GlobalStatic.VEvaluator` only (no VariableData guard — RAND has no VariableData dependency).

All existing F833 DIM stubs remain untouched. The F833 tracking comment at lines 95-98 is removed since stubs are now fully replaced.

**Tests**: 23 new xUnit `[Fact]` tests follow the exact pattern established by F833 in `EngineVariablesImplTests.cs`. Each test calls `GlobalStatic.Reset()`, instantiates `EngineVariablesImpl`, and asserts either default return value (getters) or no-throw (setters) when `VariableData` is null. `GetRandom` gets an additional test asserting `0` when `VEvaluator` is also null. Character-scoped methods get an additional test asserting default when character index is out-of-bounds (CharacterList.Count == 0).

This approach satisfies all 16 ACs by: pattern-matching the existing F833 delegation style (AC#1, AC#2, AC#3), eliminating all stub return expressions (AC#4), adding VariableData null-guards on all VariableData-dependent paths (AC#5), adding a VEvaluator-only null-guard for GetRandom (AC#6), adding 23 new passing xUnit tests (AC#7, AC#8, AC#9, AC#11), verifying setter write delegation (AC#10), centralizing character bounds-checking in a helper (AC#12), removing the F833 tracking comment (AC#13), and ensuring the build succeeds (AC#14).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Implement 14 scalar methods (GetResult, GetMoney, SetMoney, GetMaster, SetMaster, GetAssi, GetCount, GetTarget x4, GetPlayer, SetPlayer, GetSelectCom) each calling `GetArray(VariableCode.X)[0]` or `[index]`. Grep will find 14+ VariableCode references matching the pattern. |
| 2 | Implement 7 character-scoped methods (GetName, SetName, GetCallName, SetCallName, GetIsAssi, GetCharacterNo, SetCharacterNo) each calling `GetCharacter(characterIndex)`. Grep will find 7+ `GetCharacter(` references. |
| 3 | Implement `GetCharaNum` via `CharacterList.Count` and `GetRandom` via `GetNextRand(max)`. Grep will find both patterns. |
| 4 | Replace all `=> 0;`, `=> string.Empty;`, and empty-body stubs `{ }` in the stub region with real delegation bodies. No stub patterns remain. |
| 5 | Every new method body starts with `if (GlobalStatic.VariableData == null) return <default>;`. With 7 F833 guards + 14 scalar + 1 GetCharacter helper + 1 GetCharaNum = 23+ total (GetRandom excluded: depends only on VEvaluator). AC requires gte 23, which is met. |
| 6 | `GetRandom` body includes `if (GlobalStatic.VEvaluator == null) return 0;` (VEvaluator-only guard, no VariableData guard needed). Grep for `VEvaluator.*null` matches this line. |
| 7 | 14 new null-path getter tests for scalar methods call `GlobalStatic.Reset()`, invoke getter, assert `0` or `string.Empty`. Tests pass under `dotnet test --filter "~Get"`. |
| 8 | 9 new null-path tests for character-scoped and computed methods (GetName, GetCallName, GetIsAssi, GetCharacterNo, GetCharaNum, GetRandom x2, plus bounds-check test) pass under full filter. |
| 9 | New setter null-path tests for SetMoney, SetMaster, SetTarget, SetPlayer, SetCharacterNo, SetName, SetCallName do not throw; verified via `Record.Exception`. |
| 10 | 5 scalar setter writes use `GetArray(VariableCode.X)[0] = value` or `[index] = value`. Grep finds 5+ assignment patterns. |
| 11 | 10 existing F833 tests + 23 new tests = 33+ active tests. All pass. |
| 12 | `GetCharacter(int characterIndex)` helper checks `characterIndex < 0` and `characterIndex >= CharacterList.Count`, returning `null` on invalid. Callers return defaults. Grep finds both boundary expressions. |
| 13 | Remove the comment block at lines 95-98 ("Real delegation is out of scope (tracked in F835)"). Grep for the string returns no match. |
| 14 | No new types, no interface changes; all delegation targets already compiled in F833. `dotnet build` succeeds. |
| 15 | Verify IEngineVariables.cs has no modifications via `git diff HEAD -- src/Era.Core/Interfaces/IEngineVariables.cs` in the core repo. Empty diff confirms interface contract preserved. |
| 16 | SetName writes `chara.DataString[...] = name`, SetCallName writes `chara.DataString[...] = callName`, SetCharacterNo writes `chara.DataInteger[...] = value`. Grep finds 3+ `chara.Data(String|Integer)[...] =` patterns. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Character-scoped null/bounds guard | A: Inline guard in each of 7 methods; B: Shared private helper `GetCharacter(int)` | B: Shared helper | Single place for bounds logic; reduces code duplication across 7 methods; AC#12 satisfied via 2 pattern occurrences in one helper |
| ISASSI and CharacterNo storage | A: DataIntegerArray[varCode][index]; B: DataInteger[varCode] | B: DataInteger | CharaIntVariableToken source confirms `chara.DataInteger[VarCodeInt]`; ISASSI = 0x00 bit index |
| GetRandom null-guard ordering | A: Check VariableData first, then VEvaluator; B: Check VEvaluator only | B: VEvaluator only | GetRandom depends only on VEvaluator.GetNextRand(), not VariableData; guarding VariableData would introduce false coupling and cause GetRandom to incorrectly return 0 when VEvaluator is available but VariableData is null |
| VEvaluator access pattern | A: `GlobalStatic.VEvaluator.GetNextRand(max)`; B: Cast to IVariableEvaluator | A: Direct field | `GlobalStatic.VEvaluator` is already typed as `VariableEvaluator` which implements `GetNextRand(Int64 max)`; no cast needed |
| Scalar CAN_FORBID semantics | A: Use `get_Variable_canforbid` pattern (returns -1 on empty array); B: Use existing `GetArray()[0]` | B: GetArray | Interface semantics are simpler (no -1 convention). F833 DIM stubs use GetArray. Constraint row notes semantic difference but GetArray is the consistent pattern for this adapter layer. |
| SetCharacterNo data target | A: `DataIntegerArray`; B: `DataInteger` | B: DataInteger | CharaIntVariableToken for NO uses `chara.DataInteger[VarCodeInt]` — same as GetCharacterNo getter, consistent set/get pair |

### Interfaces / Data Structures

No new interfaces or types are introduced. This feature modifies only `EngineVariablesImpl.cs` (implementation) and `EngineVariablesImplTests.cs` (tests).

**Private helper added to EngineVariablesImpl**:

```csharp
// Returns the CharacterData at characterIndex, or null if VariableData is null
// or the index is out of bounds. All character-scoped methods use this helper.
private static CharacterData? GetCharacter(int characterIndex)
{
    if (GlobalStatic.VariableData == null) return null;
    var list = GlobalStatic.VariableData.CharacterList;
    if (characterIndex < 0 || characterIndex >= list.Count) return null;
    return list[characterIndex];
}
```

**Representative scalar pattern** (for context, matches F833 style):

```csharp
public int GetResult()
{
    if (GlobalStatic.VariableData == null) return 0;
    return (int)GetArray(VariableCode.RESULT)[0];
}
```

**Representative character-scoped pattern**:

```csharp
public string GetName(int characterIndex)
{
    var chara = GetCharacter(characterIndex);
    if (chara?.DataString == null) return string.Empty;
    int idx = (int)(VariableCode.__LOWERCASE__ & VariableCode.NAME);
    if (idx >= chara.DataString.Length) return string.Empty;
    return chara.DataString[idx] ?? string.Empty;
}

public int GetIsAssi(int characterIndex)
{
    var chara = GetCharacter(characterIndex);
    if (chara?.DataInteger == null) return 0;
    int idx = (int)(VariableCode.__LOWERCASE__ & VariableCode.ISASSI);
    if (idx >= chara.DataInteger.Length) return 0;
    return (int)chara.DataInteger[idx];
}
```

**Computed pattern**:

```csharp
public int GetCharaNum()
{
    if (GlobalStatic.VariableData == null) return 0;
    return GlobalStatic.VariableData.CharacterList.Count;
}

public int GetRandom(int max)
{
    if (GlobalStatic.VEvaluator == null) return 0;
    return (int)GlobalStatic.VEvaluator.GetNextRand(max);
}
```

**Note on defensive access**: Character-scoped methods must guard against three failure modes (matching `VariableDataAccessor` pattern): (1) `chara?.DataString == null` / `chara?.DataInteger == null` — uninitialized CharacterData internal arrays, (2) bitmask index `>= array.Length` — out-of-bounds on internal array, (3) `DataString[idx]` returning null — uninitialized string elements (use `?? string.Empty`).

### Upstream Issues

<!-- No upstream issues discovered. All referenced interfaces and methods verified against source. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#5 Expected `gte 23` accounts for shared GetCharacter helper | AC Definition Table, AC#5 | Technical Design uses shared GetCharacter() helper centralizing 7 character null-guards into 1, yielding 7 existing + 14 scalar + 1 helper + 1 GetCharaNum = 23 minimum; GetRandom excluded (VEvaluator-only dependency) |

<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 7, 8, 9, 11 | Write 23 null-path xUnit tests in EngineVariablesImplTests.cs (one per stub method: scalar getters assert 0/empty, scalar setters assert no-throw, character-scoped getters assert defaults, character-scoped setters assert no-throw, computed methods assert 0) — TDD RED step | | [x] |
| 2 | 1, 4, 5, 10, 13 | Implement 14 scalar methods in EngineVariablesImpl.cs using GetArray(VariableCode.X)[0] pattern with null-guard on VariableData; remove F833 tracking comment (lines 95-98) | | [x] |
| 3 | 2, 5, 12, 16 | Implement private GetCharacter(int) helper and 7 character-scoped methods (GetName, SetName, GetCallName, SetCallName, GetIsAssi, GetCharacterNo, SetCharacterNo) with bounds-checking and setter write delegation | | [x] |
| 4 | 3, 5, 6 | Implement GetCharaNum (CharacterList.Count) and GetRandom (VEvaluator.GetNextRand) with null-guards on VariableData and VEvaluator separately | | [x] |
| 5 | 14, 15 | Build engine test project, verify no errors, and confirm IEngineVariables.cs unchanged | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-835.md Tasks T1, AC#7-9, AC#11 | 23 new [Fact] tests in EngineVariablesImplTests.cs (RED — tests fail, implementations not yet changed) |
| 2 | implementer | sonnet | feature-835.md Tasks T2-T4, AC#1-6, AC#10, AC#12-13 | 23 stub methods replaced in EngineVariablesImpl.cs; F833 tracking comment removed |
| 3 | implementer | sonnet | feature-835.md Task T5, AC#14 | Build verification — dotnet build succeeds with no errors |

### Pre-conditions

- F833 is [DONE]: EngineVariablesImpl.cs exists with 9 DIM stubs and 23 no-op stubs
- EngineVariablesImplTests.cs exists with 10 F833 baseline tests
- Target repo: engine (C:\Era\engine)
- Target files:
  - `engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs`
  - `engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs`

### Execution Order

1. **Phase 1 (T1 — TDD RED)**: Write all 23 null-path tests before touching EngineVariablesImpl.cs. Tests must compile but FAIL (stubs still return 0/empty, so getter tests would pass spuriously — write them as failing by asserting after `GlobalStatic.Reset()` with non-null VariableData setup, OR confirm null-path getters already pass and focus setters on no-throw). Implementer must confirm test count ≥ 33 total after Phase 1.
2. **Phase 2 (T2 — scalar impl)**: Replace 14 scalar stub bodies with `GetArray(VariableCode.X)[0]` pattern. Remove comment block "Real delegation is out of scope (tracked in F835)" at lines 95-98. All scalar null-path tests must turn GREEN.
3. **Phase 2 (T3 — character-scoped impl)**: Add `private static CharacterData? GetCharacter(int characterIndex)` helper. Replace 7 character-scoped stub bodies. All character-scoped null-path tests must turn GREEN.
4. **Phase 2 (T4 — computed impl)**: Replace GetCharaNum and GetRandom stub bodies. GetRandom gets VEvaluator-only null-guard (no VariableData dependency). All computed null-path tests must turn GREEN.
5. **Phase 3 (T5 — build)**: Run `dotnet build engine/tests/uEmuera.Tests/uEmuera.Tests.csproj` via WSL. Confirm "Build succeeded" with zero errors.

### Build Verification Steps

```bash
# T5: Build verification (via WSL from devkit root)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/engine && /home/siihe/.dotnet/dotnet build tests/uEmuera.Tests/uEmuera.Tests.csproj'

# AC#7-9, AC#11: Run EngineVariablesImpl tests
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/engine && /home/siihe/.dotnet/dotnet test --filter "FullyQualifiedName~EngineVariablesImplTests" --blame-hang-timeout 10s'
```

### Success Criteria

- All 23 no-op stub methods replaced (AC#4: no `=> 0;`, `=> string.Empty;`, empty-body stubs in stub region)
- Grep for `GetArray(VariableCode.(RESULT|MONEY|...))` returns ≥ 14 matches (AC#1)
- Grep for `CharacterList` returns ≥ 7 matches (AC#2)
- Grep for `CharacterList\.Count|GetNextRand` returns ≥ 2 matches (AC#3)
- Grep for `GlobalStatic\.VariableData == null` returns ≥ 23 matches (AC#5)
- Grep for `VEvaluator.*null` returns at least 1 match (AC#6)
- Grep for `GetArray(VariableCode.\w+)\[.*\] =` returns ≥ 5 matches (AC#10)
- Total test count ≥ 33 and all pass (AC#11)
- Character bounds-check patterns present (AC#12)
- F833 tracking comment absent (AC#13)
- Build succeeded (AC#14)

### Error Handling

- If `GetArray(VariableCode.X)` returns an empty array: add bounds-check before `[0]` access, return 0 if empty
- If `CharacterData.DataString[index]` returns null: use `?? string.Empty` null-coalescing
- If `GlobalStatic.VEvaluator.GetNextRand` signature differs from expected: check `GlobalStatic.cs:41` for exact type and method name
- If build fails with CS errors: STOP and report to user before any further changes

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T05:10 | PHASE_START | orchestrator | Phase 1 Initialize | Status [REVIEWED]->[WIP], build OK |
<!-- run-phase-1-completed -->
| 2026-03-06T05:15 | PHASE_START | orchestrator | Phase 2 Investigation | Explorer: 23 stubs confirmed, CharacterData/VEvaluator patterns verified |
<!-- run-phase-2-completed -->
| 2026-03-06T05:20 | PHASE_START | orchestrator | Phase 3 TDD RED | 23 tests written, 33 total pass (null-path pass on stubs as expected) |
<!-- run-phase-3-completed -->
| 2026-03-06T05:30 | PHASE_START | orchestrator | Phase 4 Implementation | T2-T4 implemented, T5 build OK, IEngineVariables unchanged |
<!-- run-phase-4-completed -->
| 2026-03-06T05:40 | PHASE_START | orchestrator | Phase 7 Verification | All 16 ACs PASS, 33 tests pass |
<!-- run-phase-7-completed -->
| 2026-03-06T05:45 | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: stale class doc comment (F833 only → updated to F833+F835) |
| 2026-03-06T05:45 | FIX | orchestrator | Phase 8.1 doc comment fix | Updated EngineVariablesImpl class summary |
| 2026-03-06T05:47 | PHASE_START | orchestrator | Phase 8 Post-Review | 8.1 NEEDS_REVISION fixed, 8.2 skipped (no extensibility), 8.3 N/A |
<!-- run-phase-8-completed -->
| 2026-03-06T05:50 | PHASE_START | orchestrator | Phase 9 Report | All 16 ACs [x], 1 DEVIATION (D: fixed) |
<!-- run-phase-9-completed -->
| 2026-03-06T05:55 | PHASE_START | orchestrator | Phase 10 Finalize | Finalizer [DONE], commits: engine cc72488, devkit 6156f5b |
| 2026-03-06T05:55 | CodeRabbit | CLI timeout (engine) | CodeRabbit review hung during "Reviewing" phase after 10+ min |
<!-- run-phase-10-completed -->

---

## Review Notes

- [fix] Phase2-Review iter1: AC Details blocks (AC#1,2,3,5,10,11,12) | Missing mandatory Derivation field in threshold AC Details blocks
- [fix] Phase3-Maintainability iter2: Technical Design, AC#5, Key Decisions | GetRandom false coupling — removed unnecessary VariableData null-guard (RAND depends only on VEvaluator), AC#5 gte 24→23
- [fix] Phase3-Maintainability iter2: AC#4 pattern and description | Fixed inconsistent pipe escaping in not_matches regex; clarified whole-file verification scope
- [fix] Phase2-Review iter3: Execution Order step 4 | Residual SSOT inconsistency — changed dual null-guard to VEvaluator-only for GetRandom
- [fix] Phase2-Review iter4: AC#2 pattern and details | Changed from CharacterList gte 7 to GetCharacter\( gte 7 — aligns with Technical Design shared helper pattern
- [fix] Phase2-Review iter6: AC#4 pattern | Fixed grep-style escaped alternation (\|) to Python regex alternation (|) for not_matches correctness
- [fix] Phase2-Review iter6: AC Definition Table + Philosophy Derivation + Goal Coverage | Added AC#15 for Constraint C7 (IEngineVariables.cs interface unchanged)
- [fix] Phase2-Review iter7: AC Definition Table + Philosophy Derivation + Goal Coverage + AC Coverage + Task 3 | Added AC#16 for character-scoped setter write verification (analogous to AC#10 for scalar setters)
- [fix] Phase3-Maintainability iter8: Technical Design representative character-scoped pattern | Added DataString/DataInteger null-guard and bitmask index bounds-check (matching VariableDataAccessor defensive pattern)
- [fix] Phase4-ACValidation iter9: AC#15 type and matcher | Changed from code/not_matches/Bash to exit_code/succeeds/Bash(--exit-code) per ac-matcher-mapping SSOT

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 835 (2026-03-06)
- [applied] C35: Threshold AC Details missing Derivation field check → `.claude/agents/quality-fixer.md`
- [revised] 8e: Key Decision-Stub Consistency Check (broadened from null-guard-only to all Key Decisions) → `.claude/agents/tech-designer.md`
- [applied] V2 setter coverage symmetry across delegation categories → `.claude/agents/ac-validator.md`
- [revised] C5: Over-escaped regex pipe (broadened scope to matches/not_matches Python regex) → `.claude/agents/quality-fixer.md`
- [rejected] Predecessor context propagation — F835の119回読み込みはF808であり、F835のpredecessorはF833。既存最適化で対応済み

---

<!-- fc-phase-6-completed -->

## Links
- [Predecessor: F833](feature-833.md) - IEngineVariables DIM Stubs Engine Adapter Implementation
- [Related: F838](feature-838.md) - Test Infrastructure
- [Related: F790](feature-790.md) - IEngineVariables Interface Definition
