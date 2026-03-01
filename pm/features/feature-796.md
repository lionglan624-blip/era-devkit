# Feature 796: BodyDetailInit delegate-to-IVariableStore Migration

## Status: [DONE]
<!-- initialized: 2026-02-18T00:00:00Z -->

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

---

## Summary

Migrate BodySettings.BodyDetailInit from Func/Action delegate parameters to IVariableStore-based variable access, eliminating the dual access pattern introduced by F779.

---

## Review Context
<!-- Written by FL POST-LOOP Step 6.3. Review findings only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F779 |
| Discovery Point | Philosophy Gate (POST-LOOP Step 6.3) |
| Timestamp | 2026-02-17 |

### Identified Gap
BodySettings.cs has dual access pattern: BodyDetailInit uses `Func<int,int,int>` getCflag / `Action<int,int,int>` setCflag delegates, while F779's new methods (Tidy, ValidateBodyOption, etc.) use `IVariableStore`. This creates maintenance burden and prevents unified testing.

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | philosophy-deriver |
| Derived Task | "Migrate BodyDetailInit delegate parameters to IVariableStore-based variable access" |
| Comparison Result | "Dual access pattern: delegates (Phase 3) vs IVariableStore (Phase 20) in same class" |
| DEFER Reason | "Refactoring existing BodyDetailInit was out of F779 scope (F779 only added new methods)" |

### Files Involved
| File | Relevance |
|------|-----------|
| Era.Core/State/BodySettings.cs | Contains BodyDetailInit with delegate params and new IVariableStore methods |
| Era.Core/Interfaces/IBodySettings.cs | Interface defining BodyDetailInit signature with delegates |
| Era.Core/Common/GameInitialization.cs | Caller that creates dummy lambdas for BodyDetailInit |

### Parent Review Observations
F779 FL review identified DEP-001: BodyDetailInit was not retrofitted to use IVariableStore because it was already working and tested. The dual pattern was accepted as deferred debt with F796 as the designated cleanup feature. Pipeline Continuity philosophy (Phase 20) requires this debt to be resolved before Post-Phase Review (F782).

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; F647 decomposed it into actionable sub-features.

### Problem (Current Issue)

BodySettings.BodyDetailInit uses a delegate-based variable access pattern (`Func<int,int,int>` getCflag, `Action<int,int,int>` setCflag, `Func<int,int,int>` getTalent) because it was originally migrated in Phase 3 (F370) before IVariableStore existed for character-scoped operations. When F779 (Phase 20) added new business logic methods (Tidy, ValidateBodyOption, etc.) using IVariableStore, the existing BodyDetailInit was not retrofitted because it was already working and tested (`Era.Core/State/BodySettings.cs:438-500` vs `BodySettings.cs:502-510`). This creates a dual access pattern within the same class: BodyDetailInit accepts raw delegates with int-based indices while all other methods use `_variables: IVariableStore` with strongly-typed `CharacterFlagIndex`/`TalentIndex`. The split forces a nullable `_variables` field (`BodySettings.cs:29`), a parameterless backward-compatibility constructor (`BodySettings.cs:36-37`), and a runtime `RequireVariables()` guard (`BodySettings.cs:502-504`) instead of compile-time safety. Additionally, the delegate-based approach masks a latent TALENT index bug at `BodySettings.cs:456` where index 0 (Virginity) is used instead of the correct index 2 (Gender) for the HAS_VAGINA check, since raw int indices lack type safety.

### Goal (What to Achieve)

Unify BodyDetailInit to use IVariableStore internally (matching the pattern established by F779's Tidy/Validate methods), remove the delegate parameters from the IBodySettings interface signature, eliminate the parameterless constructor, make `_variables` non-nullable, and fix the TALENT index bug -- producing a single, consistent variable access pattern across all BodySettings methods.

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does BodySettings have two incompatible variable access patterns? | BodyDetailInit uses Func/Action delegates while Tidy/Validate methods use IVariableStore | `BodySettings.cs:438-500` (delegates) vs `BodySettings.cs:502-510` (IVariableStore) |
| 2 | Why does BodyDetailInit use delegates instead of IVariableStore? | It was migrated in Phase 3 (F370) before IVariableStore had character-scoped methods | `BodySettings.cs:25` (Feature 377 Phase 4 comment); `IBodySettings.cs:12-16` |
| 3 | Why was BodyDetailInit not retrofitted when F779 added IVariableStore methods? | F779's scope was limited to new business logic methods (lines 350-943), not refactoring existing BodyDetailInit | `IBodySettings.cs:8` ("Extended with business logic methods (F779)") |
| 4 | Why was the refactoring deferred rather than included in F779? | It was tracked as DEP-001 and deferred to a separate feature (F796) to keep F779 scope bounded | `feature-796.md` Review Context: "DEFER from F779 [DEP-001]" |
| 5 | Why (Root)? | The incremental migration strategy adds IVariableStore methods phase-by-phase without mandatory back-porting of older methods in the same class to the new pattern, accumulating access pattern debt | Architectural pattern: Phase 3 (F370) predates Phase 20 (F779) by 17 phases |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | BodySettings has two variable access mechanisms (delegates and IVariableStore) in the same class | Incremental migration adds new IVariableStore methods without retrofitting Phase 3 delegate-based methods |
| Where | `BodySettings.cs:438-500` (BodyDetailInit) vs `BodySettings.cs:502-510` (GetCFlag/SetCFlag) | Phase boundary between F370 (Phase 3) and F779 (Phase 20) left BodyDetailInit un-migrated |
| Fix | Add IVariableStore as another parameter to BodyDetailInit alongside delegates | Replace delegate parameters with internal IVariableStore usage, unifying the access pattern |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F779 | [DONE] | Predecessor -- introduced IVariableStore methods creating the dual pattern |
| F789 | [DONE] | Related -- extended IVariableStore with Phase 20 methods; provides needed interface methods |
| F778 | [DONE] | Related -- verified existing BodyDetailInit behavior; its tests will be rewritten |
| F780 | [PROPOSED] | Related -- Genetics & Growth (Phase 20 sibling); no call chain dependency on BodyDetailInit |
| F794 | [DRAFT] | Related -- Shared validation abstraction (Phase 20 sibling); separate scope from BodyDetailInit |
| F782 | [DRAFT] | Successor -- Post-Phase Review depends on F796 completion |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| IVariableStore has all needed methods (GetCharacterFlag, SetCharacterFlag, GetTalent) | FEASIBLE | `IVariableStore.cs` defines all three; confirmed by F779/F789 |
| Typed indices exist for all 17 CFLAG body parameters | FEASIBLE | `CharacterFlagIndex.cs:35-51` -- HairLength through BodyOption4 |
| TalentIndex.Gender exists for HAS_VAGINA check | FEASIBLE | `TalentIndex.cs:32` -- `Gender = new(2)` |
| No ERB bridge impact (ERB callers call ERB function directly) | FEASIBLE | `SYSTEM.ERB:232`, `CHARA_SET.ERB:138` -- ERB calls ERB, not C# |
| DI already resolves BodySettings with IVariableStore constructor | FEASIBLE | `ServiceCollectionExtensions.cs:149` |
| Test rewrite scope is bounded (3 test files) | FEASIBLE | StateSettingsTests.cs, HeadlessIntegrationTests.cs, GameInitializationTests.cs |
| MockVariableStore pattern established by F779 tests | FEASIBLE | `BodySettingsBusinessLogicTests.cs:14-57` demonstrates mock pattern |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| IBodySettings interface | HIGH | Breaking signature change: BodyDetailInit removes 3 delegate params, keeps only int characterId |
| BodySettings class | HIGH | Rewrite BodyDetailInit internals; remove parameterless constructor; _variables becomes non-nullable |
| GameInitialization caller | MEDIUM | Simplify BodyDetailInit wrapper: remove dummy lambda creation, call with characterId only |
| Test files (3 files) | MEDIUM | Rewrite ~16 BodyDetailInit test methods to use MockVariableStore instead of delegate lambdas |
| ERB callers | LOW | No impact -- ERB callers invoke ERB function directly, not C# BodyDetailInit |
| DI registration | LOW | No change needed -- already resolves with IVariableStore constructor |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IBodySettings is a public interface | `IBodySettings.cs` | Breaking change; all callers (GameInitialization.cs) must update atomically |
| TreatWarningsAsErrors enabled | `Directory.Build.props` | Unused parameters/imports after refactoring will cause build failure |
| Parameterless constructor removal | `BodySettings.cs:36-37` | All tests using `new BodySettings()` must switch to `new BodySettings(mockStore)` |
| Result<int> error handling | `IVariableStore.cs` | GetCharacterFlag returns Result<int>; must handle via .Match() as existing GetCFlag does |
| TALENT index correction (0 to 2) | `BodySettings.cs:456` | Changes observable behavior for character 0 gender check; must document as bug fix |
| MockVariableStore needs GetTalent | `BodySettingsBusinessLogicTests.cs` | Current mock throws NotImplementedException for GetTalent; must implement |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| TALENT index bug fix changes test semantics for character 0 | HIGH | LOW | Tests mock talent data explicitly; update to use index 2 and document as bug fix |
| Parameterless constructor used by unknown callers beyond 3 test files | LOW | MEDIUM | Global grep confirms only StateSettingsTests.cs, HeadlessIntegrationTests.cs, GameInitializationTests.cs use it |
| IBodySettings signature change breaks downstream consumers | LOW | HIGH | Only GameInitialization.cs calls BodyDetailInit; no external consumers exist |
| MockVariableStore complexity increases in test setup | MEDIUM | LOW | Established mock pattern from F779 tests (BodySettingsBusinessLogicTests.cs) can be reused |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Build success | `dotnet build` | PASS | Must remain PASS after refactoring |
| Existing test count | `dotnet test --filter BodyDetailInit` | ~16 tests PASS | Tests will be rewritten but count must be preserved or increased |
| BodyDetailInit delegate params | `grep -c "Func\|Action" Era.Core/Interfaces/IBodySettings.cs` | 3 | Must become 0 after migration |

**Baseline File**: `.tmp/baseline-796.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | IBodySettings.BodyDetailInit must NOT contain Func/Action delegate parameters | Feature scope (dual pattern elimination) | AC must verify interface signature has no delegate types |
| C2 | BodySettings must have only ONE constructor accepting IVariableStore | Dual constructor is root issue | AC must verify parameterless constructor is removed |
| C3 | BodyDetailInit must use IVariableStore internally (not delegates) | Core migration goal | AC must verify _variables usage in BodyDetailInit body |
| C4 | TALENT index for gender check must use TalentIndex.Gender (index 2) | Bug fix: `BodySettings.cs:456` uses index 0 | AC must verify correct index usage |
| C5 | GameInitialization.BodyDetailInit must not create dummy lambdas | TODO cleanup at `GameInitialization.cs:315-318` | AC must verify dummy delegate pattern is removed |
| C6 | All existing BodyDetailInit equivalence tests must pass with same character values | Test preservation (16 test methods) | AC must verify character-specific CFLAG values unchanged |
| C7 | _variables field must be non-nullable (IVariableStore, not IVariableStore?) | Parameterless ctor removal consequence | AC must verify non-nullable field declaration |
| C8 | IVariableStore GetCharacterFlag/SetCharacterFlag/GetTalent methods exist | Interface Dependency Scan | AC must verify these methods are called (not new methods needed) |

### Constraint Details

**C1: No Delegate Parameters in IBodySettings**
- **Source**: Investigation consensus -- IBodySettings.cs:12-16 currently defines delegate parameters
- **Verification**: Grep IBodySettings.cs for Func/Action types
- **AC Impact**: AC must verify the new BodyDetailInit(int characterId) signature contains no delegate types

**C2: Single Constructor**
- **Source**: Investigation consensus -- BodySettings.cs:36-37 parameterless constructor exists for backward compatibility
- **Verification**: Grep BodySettings.cs constructor count
- **AC Impact**: AC must verify only the IVariableStore constructor exists; parameterless constructor removed

**C3: IVariableStore Usage in BodyDetailInit**
- **Source**: Core migration goal -- unify access patterns
- **Verification**: Read BodyDetailInit method body for _variables / GetCFlag / SetCFlag calls
- **AC Impact**: AC must verify BodyDetailInit calls SetCFlag/GetCFlag helper methods or _variables directly

**C4: TALENT Index Bug Fix**
- **Source**: BodySettings.cs:456 uses index 0 (Virginity) but should be index 2 (Gender); confirmed by Talent.csv:5 and TalentIndex.cs:32
- **Verification**: Grep BodyDetailInit for TalentIndex.Gender usage
- **AC Impact**: AC must verify GetTalent call uses TalentIndex.Gender (not raw index 0)

**C5: GameInitialization Cleanup**
- **Source**: GameInitialization.cs:315-318 creates dummy lambdas returning 0 with TODO comment
- **Verification**: Grep GameInitialization.cs for Func/Action lambda creation
- **AC Impact**: AC must verify GameInitialization.BodyDetailInit calls _bodySettings.BodyDetailInit(characterId) directly

**C6: Equivalence Preservation**
- **Source**: 16 existing BodyDetailInit test methods in StateSettingsTests.cs verify character-specific CFLAG values
- **Verification**: Run existing test suite; verify same character parameter values are tested
- **AC Impact**: AC must verify all character-specific body parameter values remain unchanged (chars 0-13)

**C7: Non-nullable _variables**
- **Source**: Parameterless constructor removal makes _variables guaranteed initialized
- **Verification**: Grep BodySettings.cs for field declaration type
- **AC Impact**: AC must verify IVariableStore (not IVariableStore?) field type

**C8: Existing IVariableStore Methods**
- **Source**: Interface Dependency Scan -- all needed methods exist in IVariableStore
- **Verification**: Confirmed GetCharacterFlag, SetCharacterFlag, GetTalent exist
- **AC Impact**: No new interface methods needed; AC verifies existing methods are used

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F779 | [DONE] | Body Settings UI -- introduced IVariableStore methods that created the dual pattern |
| Related | F789 | [DONE] | IVariableStore Phase 20 Extensions -- provides needed interface methods |
| Related | F778 | [DONE] | Verified existing BodyDetailInit behavior; its tests will be rewritten |
| Related | F794 | [DONE] | Shared validation abstraction (Phase 20 sibling); separate scope |
| Successor | F782 | [DRAFT] | Post-Phase Review depends on F796 completion |

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

<!-- fc-phase-3-completed -->

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Unify BodyDetailInit to use IVariableStore internally" | BodyDetailInit must call _variables methods (SetCharacterFlag, GetTalent) not delegate parameters | AC#3, AC#4 |
| "remove the delegate parameters from the IBodySettings interface signature" | IBodySettings.BodyDetailInit must have no Func/Action delegate types in signature | AC#1 |
| "eliminate the parameterless constructor" | BodySettings must have only the IVariableStore constructor | AC#5 |
| "make `_variables` non-nullable" | Field declaration must be IVariableStore not IVariableStore? | AC#6 |
| "fix the TALENT index bug" | Gender check must use TalentIndex.Gender (index 2), not raw index 0 | AC#7 |
| "single, consistent variable access pattern across all BodySettings methods" | No Func/Action delegate types in BodySettings.cs (BodyDetailInit was the sole delegate user per Impact Analysis); RequireVariables() guard removed; GetCFlag/SetCFlag call _variables directly | AC#2, AC#8, AC#14, AC#16a, AC#16b |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IBodySettings.BodyDetailInit has no delegate parameters | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | not_matches | `Func<int\|Action<int` | [x] |
| 2 | IBodySettings.BodyDetailInit signature is int-only | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | matches | `void BodyDetailInit\(\s*int characterId\)` | [x] |
| 3 | BodyDetailInit uses SetCharacterFlag via helper | code | Grep(Era.Core/State/BodySettings.cs) | matches | `SetCFlag\(characterId, CharacterFlagIndex\.HairLength` | [x] |
| 4 | BodyDetailInit uses GetTalent via IVariableStore | code | Grep(Era.Core/State/BodySettings.cs) | matches | `GetTalent.*TalentIndex\.Gender` | [x] |
| 5 | Parameterless constructor removed | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `public BodySettings\(\)\s*\{` | [x] |
| 6 | _variables field is non-nullable | code | Grep(Era.Core/State/BodySettings.cs) | matches | `private readonly IVariableStore _variables` | [x] |
| 7 | TALENT index uses TalentIndex.Gender not raw 0 | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `GetTalent\(characterId,\s*0\)` | [x] |
| 8 | RequireVariables guard removed | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `RequireVariables\(\)` | [x] |
| 9 | GameInitialization has no dummy lambdas for BodyDetailInit | code | Grep(Era.Core/Common/GameInitialization.cs) | not_matches | `BodyDetailInit\(characterId,\s*getCflag` | [x] |
| 10 | GameInitialization calls BodyDetailInit with characterId only | code | Grep(Era.Core/Common/GameInitialization.cs) | matches | `_bodySettings\.BodyDetailInit\(characterId\)` | [x] |
| 11 | Build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 12 | All unit tests pass | test | dotnet test engine.Tests | succeeds | - | [x] |
| 13 | Equivalence: character preset values preserved (with bug-fix delta) | test | dotnet test engine.Tests --filter BodyDetailInit | succeeds | - | [x] |
| 14 | No Func/Action delegate types in BodySettings.cs | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `Func<int, int, int>\|Action<int, int, int>` | [x] |
| 15 | Zero technical debt in BodySettings and IBodySettings | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/Interfaces/IBodySettings.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 16a | GetCFlag helper uses _variables.GetCharacterFlag | code | Grep(Era.Core/State/BodySettings.cs) | matches | `_variables\.GetCharacterFlag` | [x] |
| 16b | SetCFlag helper uses _variables.SetCharacterFlag | code | Grep(Era.Core/State/BodySettings.cs) | matches | `_variables\.SetCharacterFlag` | [x] |
| 17 | Bug-fix behavioral test exists for TalentIndex.Gender | code | Grep(Era.Core.Tests/State/BodySettingsBusinessLogicTests.cs,engine.Tests/Tests/StateSettingsTests.cs) | matches | `TalentIndex\.Gender.*BodyDetailInit\|BodyDetailInit.*TalentIndex\.Gender\|SetTALENT.*Gender\|HasVagina.*Gender\|Gender.*HasVagina` | [x] |
| 18 | Era.Core.Tests pass after MockVariableStore changes | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 19 | BodyDetailInit uses SetCFlag for last CFLAG (BodyOption4) | code | Grep(Era.Core/State/BodySettings.cs) | matches | `SetCFlag\(characterId, CharacterFlagIndex\.BodyOption4` | [x] |
| 20 | GameInitialization BodyDetailInit TODO removed | code | Grep(Era.Core/Common/GameInitialization.cs) | not_matches | `TODO.*BodyDetailInit\|BodyDetailInit.*TODO` | [x] |
| 21 | F797 DRAFT file exists | file | Glob(pm/features/feature-797.md) | exists | `feature-797.md` | [x] |
| 22 | F797 registered in index-features.md | code | Grep(pm/index-features.md) | matches | `797.*DRAFT` | [x] |
| 23 | Bug-fix test pins concrete HairLength for char 0 male path | code | Grep(Era.Core.Tests/State/BodyDetailInitMigrationTests.cs,engine.Tests/Tests/StateSettingsTests.cs) | matches | `Gender.*0.*HairLength.*100\|HairLength.*100.*Gender.*0` | [x] |
| 24 | F797 DRAFT covers UterusVolumeInit scope | code | Grep(pm/features/feature-797.md) | matches | `UterusVolumeInit` | [x] |

### AC Details

**AC#1: IBodySettings.BodyDetailInit has no delegate parameters**
- **Test**: Grep pattern=`Func<int|Action<int` path=`Era.Core/Interfaces/IBodySettings.cs`
- **Expected**: 0 matches (no Func/Action delegate types in interface)
- **Rationale**: C1 constraint. The dual pattern originated from delegate parameters in the interface. Removing them is the core migration goal. (C1)

**AC#2: IBodySettings.BodyDetailInit signature is int-only**
- **Test**: Grep pattern=`void BodyDetailInit\(\s*int characterId\)` path=`Era.Core/Interfaces/IBodySettings.cs`
- **Expected**: 1 match (the new simplified signature)
- **Rationale**: Positive verification that the new signature exists with only int characterId parameter. AC#1 is negative (no delegates); this confirms the correct replacement. (C1)

**AC#3: BodyDetailInit uses SetCharacterFlag via helper**
- **Test**: Grep pattern=`SetCFlag\(characterId, CharacterFlagIndex\.HairLength` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match (BodyDetailInit uses the existing SetCFlag helper to call SetCharacterFlag)
- **Rationale**: C3 constraint. Verifies BodyDetailInit uses IVariableStore internally through the established SetCFlag helper (which calls _variables.SetCharacterFlag). Checking HairLength as representative of all 17 CFLAG writes. (C3, C8)

**AC#4: BodyDetailInit uses GetTalent via IVariableStore**
- **Test**: Grep pattern=`GetTalent.*TalentIndex\.Gender` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match (BodyDetailInit calls _variables.GetTalent with typed TalentIndex.Gender)
- **Rationale**: C3/C4 constraints. Verifies the gender check uses IVariableStore.GetTalent with the strongly-typed TalentIndex.Gender instead of raw delegate with index 0. Simultaneously confirms C4 bug fix and C3 IVariableStore usage. (C3, C4, C8)

**AC#5: Parameterless constructor removed**
- **Test**: Grep pattern=`public BodySettings\(\)\s*\{` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (parameterless constructor no longer exists)
- **Rationale**: C2 constraint. The parameterless constructor existed only for backward compatibility with delegate-based BodyDetailInit. With IVariableStore unification, _variables is always required. (C2)

**AC#6: _variables field is non-nullable**
- **Test**: Grep pattern=`private readonly IVariableStore _variables` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match (non-nullable IVariableStore, not IVariableStore?)
- **Rationale**: C7 constraint. With parameterless constructor removed, _variables is guaranteed initialized by the single IVariableStore constructor. The nullable annotation (?) must be removed for compile-time safety. (C7)

**AC#7: TALENT index uses TalentIndex.Gender not raw 0**
- **Test**: Grep pattern=`GetTalent\(characterId,\s*0\)` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (no raw index 0 passed to GetTalent helper after migration)
- **Rationale**: C4 constraint. Guards against regression where `GetTalent(characterId, 0)` (raw int) is used instead of `GetTalent(characterId, TalentIndex.Gender)`. AC#4 positively verifies TalentIndex.Gender usage; this AC negatively verifies no raw int 0 is passed. (C4)

**AC#8: RequireVariables guard removed**
- **Test**: Grep pattern=`RequireVariables\(\)` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (runtime guard no longer needed)
- **Rationale**: RequireVariables() existed because _variables was nullable (due to parameterless constructor). With non-nullable _variables, this guard is dead code. GetCFlag/SetCFlag helpers should call _variables directly. (C2, C7)

**AC#9: GameInitialization has no dummy lambdas for BodyDetailInit**
- **Test**: Grep pattern=`BodyDetailInit\(characterId,\s*getCflag` path=`Era.Core/Common/GameInitialization.cs`
- **Expected**: 0 matches (old 4-param call pattern removed)
- **Rationale**: C5 constraint. GameInitialization.cs:315-318 creates dummy lambdas that return 0/no-op because the actual GlobalStatic accessors were deferred. With IVariableStore-based BodyDetailInit, the caller passes only characterId. Note: This AC checks the getCflag lambda specifically; setCflag and getTalent lambda removal is implicitly guaranteed by AC#10 (positive verification of simplified call — the old 4-param call `BodyDetailInit(characterId, getCflag, setCflag, getTalent)` cannot coexist with the new 1-param call). UterusVolumeInit has similar lambdas but is out of scope. (C5)

**AC#10: GameInitialization calls BodyDetailInit with characterId only**
- **Test**: Grep pattern=`_bodySettings\.BodyDetailInit\(characterId\)` path=`Era.Core/Common/GameInitialization.cs`
- **Expected**: 1 match (simplified call site)
- **Rationale**: Positive verification of simplified caller. Complements AC#9 (no dummy lambdas) by verifying the new call pattern exists. (C5)

**AC#11: Build succeeds**
- **Test**: `dotnet build Era.Core`
- **Expected**: Build succeeds with zero errors (TreatWarningsAsErrors enforced)
- **Rationale**: Breaking interface change (IBodySettings) and constructor removal must compile cleanly. TreatWarningsAsErrors ensures no unused imports/parameters remain. (C1, C2)

**AC#12: All unit tests pass**
- **Test**: `dotnet test engine.Tests`
- **Expected**: All tests pass (includes rewritten BodyDetailInit tests using MockVariableStore)
- **Rationale**: Test rewrite validation. StateSettingsTests, HeadlessIntegrationTests, and GameInitializationTests all use `new BodySettings()` parameterless constructor which will be removed. Tests must be updated to use `new BodySettings(mockStore)`. (C6)

**AC#13: Equivalence: character preset values preserved (with bug-fix delta)**
- **Test**: `dotnet test engine.Tests --filter BodyDetailInit`
- **Expected**: All BodyDetailInit tests pass with CFLAG values matching updated expectations (identical for all parameters except HAS_VAGINA where the TALENT index bug fix changes Gender vs Virginity lookup)
- **Rationale**: C6 constraint. The 16 existing BodyDetailInit test methods verify specific CFLAG values per character. After migration, the same values must be produced via IVariableStore. The TALENT index bug fix (index 0→2) may change HAS_VAGINA outcomes for characters whose Gender talent differs from Virginity talent — tests must be updated to reflect corrected behavior. This is intentional behavior change, not a regression. (C4, C6)

**AC#14: No Func/Action delegate types in BodySettings.cs**
- **Test**: Grep pattern=`Func<int, int, int>|Action<int, int, int>` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (all delegate parameter types eliminated from implementation)
- **Rationale**: Verifies the dual access pattern is fully eliminated from the entire BodySettings.cs file (file-scope grep, not method-scope). This covers BodyDetailInit method parameters, null checks, and any other potential delegate references. Per Impact Analysis, BodyDetailInit was the sole delegate user; this AC confirms no residual delegate types remain anywhere in the file. (C1, C3)

**AC#15: Zero technical debt in BodySettings and IBodySettings**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths=`Era.Core/State/BodySettings.cs`, `Era.Core/Interfaces/IBodySettings.cs`
- **Expected**: 0 matches across both files
- **Rationale**: Zero debt upfront principle. GameInitialization.cs is excluded because it has pre-existing TODOs for UterusVolumeInit (line 335) and TemperatureToleranceInit (line 354) that are out of F796 scope — tracked as leak prevention issue. The BodyDetailInit TODO at line 315 is removed by Task#3; removal is implicitly verified by AC#10 (positive match of simplified `_bodySettings.BodyDetailInit(characterId)` call — the old code block including the TODO comment is replaced entirely). (C5)

**AC#16a: GetCFlag helper uses _variables.GetCharacterFlag**
- **Test**: Grep pattern=`_variables\.GetCharacterFlag` path=`Era.Core/State/BodySettings.cs`
- **Expected**: At least 1 match (GetCFlag calls `_variables.GetCharacterFlag` directly)
- **Rationale**: After RequireVariables() removal (AC#8), GetCFlag must call `_variables.GetCharacterFlag` directly. Split from original AC#16 to independently verify each helper — a single alternation pattern allowed one match of either to satisfy both. (C3, C7)

**AC#16b: SetCFlag helper uses _variables.SetCharacterFlag**
- **Test**: Grep pattern=`_variables\.SetCharacterFlag` path=`Era.Core/State/BodySettings.cs`
- **Expected**: At least 1 match (SetCFlag calls `_variables.SetCharacterFlag` directly)
- **Rationale**: After RequireVariables() removal (AC#8), SetCFlag must call `_variables.SetCharacterFlag` directly. Split from original AC#16 to independently verify each helper. Without this AC, removing RequireVariables() could leave SetCFlag calling nothing while AC#16a passes from GetCFlag alone. (C3, C7)

**AC#17: Bug-fix behavioral test exists for TalentIndex.Gender**
- **Test**: Grep pattern=`TalentIndex\.Gender.*BodyDetailInit|BodyDetailInit.*TalentIndex\.Gender|SetTALENT.*Gender|HasVagina.*Gender|Gender.*HasVagina` paths=`Era.Core.Tests/State/BodySettingsBusinessLogicTests.cs`, `engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: At least 1 match (a test that seeds TalentIndex.Gender and asserts HAS_VAGINA/BodyOption outcome exists)
- **Rationale**: Task#4 requires a behavioral test for the TALENT index bug fix (index 0→2). AC#4 and AC#7 verify the code change statically. This AC verifies that a test was actually written to confirm the corrected behavior at runtime. The first three alternatives match test setup/seeding; the last two alternatives (`HasVagina.*Gender`, `Gender.*HasVagina`) match assertion lines that verify the bug-fix outcome. Without this, the bug fix could be structurally present but behaviorally unverified. (C4)

**AC#18: Era.Core.Tests pass after MockVariableStore changes**
- **Test**: `dotnet test Era.Core.Tests`
- **Expected**: All tests pass (MockVariableStore GetTalent/SetTalent changes do not break existing tests)
- **Rationale**: Task#4 modifies MockVariableStore in `Era.Core.Tests/State/BodySettingsBusinessLogicTests.cs`. AC#12 only runs `engine.Tests`. This AC ensures the separate `Era.Core.Tests` project also compiles and passes after the mock updates.

**AC#19: BodyDetailInit uses SetCFlag for last CFLAG (BodyOption4)**
- **Test**: Grep pattern=`SetCFlag\(characterId, CharacterFlagIndex\.BodyOption4` path=`Era.Core/State/BodySettings.cs`
- **Expected**: At least 1 match (BodyDetailInit sets BodyOption4 via SetCFlag helper)
- **Rationale**: AC#3 spot-checks HairLength (first CFLAG index). This AC spot-checks BodyOption4 (last CFLAG index). Together they bracket the 17 CFLAG writes — if both first and last are migrated, the intermediate writes are extremely likely to be migrated too. Combined with AC#14 (no delegate types in file), this provides high-confidence coverage without file-scope count ambiguity. (C3)

**AC#20: GameInitialization BodyDetailInit TODO removed**
- **Test**: Grep pattern=`TODO.*BodyDetailInit\|BodyDetailInit.*TODO` path=`Era.Core/Common/GameInitialization.cs`
- **Expected**: 0 matches (BodyDetailInit-specific TODO at line 315 removed by Task#3)
- **Rationale**: GameInitialization.cs is excluded from AC#15's broad TODO check due to pre-existing out-of-scope TODOs. This narrow AC explicitly verifies the in-scope BodyDetailInit TODO is removed without being affected by UterusVolumeInit/TemperatureToleranceInit TODOs. (C5)

**AC#21: F797 DRAFT file exists**
- **Test**: Glob pattern=`pm/features/feature-797.md`
- **Expected**: File exists
- **Rationale**: Mandatory Handoff Option A requires DRAFT feature file creation. (Leak Prevention)

**AC#22: F797 registered in index-features.md**
- **Test**: Grep pattern=`797.*DRAFT` path=`pm/index-features.md`
- **Expected**: 1 match (F797 row with [DRAFT] status in Active Features table)
- **Rationale**: Mandatory Handoff Option A requires index registration. Combined with AC#21, this verifies the DRAFT Creation Checklist. (Leak Prevention)

**AC#23: Bug-fix test pins concrete HairLength for char 0 male path**
- **Test**: Grep pattern=`Gender.*0.*HairLength.*100|HairLength.*100.*Gender.*0` paths=`Era.Core.Tests/State/BodyDetailInitMigrationTests.cs`, `engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: At least 1 match (a test that sets Gender=0 and asserts HairLength=100 for char 0 male preset)
- **Rationale**: AC#13 runs rewritten tests but cannot distinguish correct from incorrect expected values (circular: Task#4 rewrites tests, AC#13 checks they pass). This AC breaks the circularity by pinning a concrete ground-truth value: Character0Male.HairLength=100 (source: `BodySettings.cs:101`). With Gender talent=0 (male), BodyDetailInit should select `Character0Male` preset and set HairLength=100. The Gender check at `BodySettings.cs:452` only applies to characterId==0; characters 1-13 use fixed presets from CharacterBodySettings dictionary with no GetTalent call. Combined with AC#17 (Gender test exists) and AC#13 (tests pass), this ensures the bug-fix behavioral test asserts correct values. (C4, C6)

**AC#24: F797 DRAFT covers UterusVolumeInit scope**
- **Test**: Grep pattern=`UterusVolumeInit` path=`pm/features/feature-797.md`
- **Expected**: At least 1 match (F797 DRAFT references UterusVolumeInit as in-scope)
- **Rationale**: AC#21 and AC#22 verify F797 exists and is registered, but not that it captures the deferred scope. The Mandatory Handoffs table specifies "UterusVolumeInit (IPregnancySettings) and TemperatureToleranceInit (IWeatherSettings)" as the deferred items. This AC pins the content of F797 to verify the leak prevention handoff actually captures UterusVolumeInit (representative of both deferred items). Without this, F797 could be created with different scope, silently losing the tracked debt.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Unify BodyDetailInit to use IVariableStore internally | AC#3, AC#4, AC#8, AC#14, AC#19 |
| 2 | Remove delegate parameters from IBodySettings interface signature | AC#1, AC#2 |
| 3 | Eliminate parameterless constructor | AC#5 |
| 4 | Make _variables non-nullable | AC#6 |
| 5 | Fix TALENT index bug | AC#4, AC#7, AC#17, AC#23 |
| 6 | Single consistent variable access pattern across all BodySettings methods | AC#3, AC#4, AC#8, AC#14, AC#16a, AC#16b |
| 7 | Build and test validation | AC#11, AC#12, AC#13, AC#18 |
| 8 | GameInitialization caller cleanup | AC#9, AC#10, AC#20 |

<!-- fc-phase-4-completed -->

## Technical Design

### Approach

Unify BodySettings.BodyDetailInit by removing its three delegate parameters and routing all variable access through the already-present `_variables: IVariableStore` field. Two private helper methods already exist for character flag access (`GetCFlag`/`SetCFlag` at lines 506-510). A third helper `GetTalent` is added following the same pattern. `BodyDetailInit` is then rewritten to call these helpers, matching the exact pattern used by `Tidy`, `ValidateBodyOption`, etc.

The approach requires four coordinated changes:

1. **IBodySettings interface** (`IBodySettings.cs`): Replace the four-parameter `BodyDetailInit` signature with `void BodyDetailInit(int characterId)`.
2. **BodySettings implementation** (`BodySettings.cs`): Remove delegate parameters and null checks from `BodyDetailInit`; add `GetTalent` private helper; replace delegate calls with helper calls; fix TALENT index from `0` to `TalentIndex.Gender`; remove `RequireVariables()` guard (callers use `_variables` directly via helpers); remove parameterless constructor; change `_variables` field from nullable `IVariableStore?` to non-nullable `IVariableStore`.
3. **GameInitialization caller** (`GameInitialization.cs`): Remove dummy lambda creation and call `_bodySettings.BodyDetailInit(characterId)` directly.
4. **Test files** (3 files): Replace any `new BodySettings()` calls with `new BodySettings(mockStore)`. Update MockVariableStore in `BodySettingsBusinessLogicTests.cs` to implement `GetTalent` returning data from the store (replacing `throw new NotImplementedException()`).

This approach satisfies all 25 ACs (AC#1–15, 16a, 16b, 17–24):
- AC#1–2: Interface signature change to `void BodyDetailInit(int characterId)`.
- AC#3–4, AC#14: BodyDetailInit body uses SetCFlag/GetTalent helpers backed by `_variables`.
- AC#5–6: Parameterless constructor removed; `_variables` field becomes non-nullable.
- AC#7–8: Raw index 0 replaced with `TalentIndex.Gender`; `RequireVariables()` deleted.
- AC#9–10: GameInitialization dummy lambdas removed; direct `(characterId)` call added.
- AC#11–13: Build and equivalence tests validate the complete migration.
- AC#15: Zero technical debt in BodySettings and IBodySettings.
- AC#16a/16b: GetCFlag/SetCFlag helpers independently verified to call `_variables` directly after RequireVariables removal.
- AC#17: Bug-fix behavioral test for TalentIndex.Gender exists.
- AC#18: Era.Core.Tests pass after MockVariableStore changes.
- AC#19: BodyDetailInit uses SetCFlag for BodyOption4 (last CFLAG index, bracketing with AC#3).
- AC#20: GameInitialization BodyDetailInit TODO removed (narrow scope, excludes out-of-scope TODOs).
- AC#21–22, 24: F797 DRAFT file created, registered in index, and content verified to reference UterusVolumeInit (leak prevention with scope pinning).
- AC#23: Bug-fix test pins concrete HairLength=100 for char 0 male path (breaks AC#13 circularity).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Remove `Func<int,int,int> getCflag`, `Action<int,int,int> setCflag`, `Func<int,int,int> getTalent` from `IBodySettings.BodyDetailInit` declaration |
| 2 | New `IBodySettings.BodyDetailInit` declaration is `void BodyDetailInit(int characterId);` — single int parameter |
| 3 | `BodyDetailInit` body calls `SetCFlag(characterId, CharacterFlagIndex.HairLength, bodyParams.HairLength)` (and all 17 CFLAG writes via `SetCFlag` helper) |
| 4 | Add `private int GetTalent(int characterId, TalentIndex talent) => _variables.GetTalent(new CharacterId(characterId), talent).Match(v => v, _ => 0);` and use it as `GetTalent(characterId, TalentIndex.Gender)` in gender branch |
| 5 | Delete the `public BodySettings() { }` parameterless constructor body and declaration |
| 6 | Change field declaration from `private readonly IVariableStore? _variables;` to `private readonly IVariableStore _variables;` |
| 7 | Replace delegate `getTalent(characterId, 0)` with `GetTalent(characterId, TalentIndex.Gender)` — no raw index 0 passed to GetTalent helper |
| 8 | Delete `private IVariableStore RequireVariables()` method; update `GetCFlag`/`SetCFlag` helpers to call `_variables.GetCharacterFlag` / `_variables.SetCharacterFlag` directly (no `RequireVariables()` indirection) |
| 9 | Remove the three dummy lambda lines (`Func<int,int,int> getCflag = ...`, `Action<int,int,int> setCflag = ...`, `Func<int,int,int> getTalent = ...`) from `GameInitialization.BodyDetailInit` |
| 10 | Replace `_bodySettings.BodyDetailInit(characterId, getCflag, setCflag, getTalent)` with `_bodySettings.BodyDetailInit(characterId)` in `GameInitialization.cs` |
| 11 | After all changes, run `dotnet build Era.Core` — must succeed with zero errors/warnings |
| 12 | After test rewrite, run `dotnet test engine.Tests` — all tests pass |
| 13 | Run `dotnet test engine.Tests --filter BodyDetailInit` — all equivalence tests pass with unchanged CFLAG values |
| 14 | After removing delegate parameters from `BodyDetailInit` body, `BodySettings.cs` contains no `Func<int, int, int>` or `Action<int, int, int>` type references |
| 15 | Remove TODO comment at `GameInitialization.cs:315`; no new TODO/FIXME/HACK in BodySettings.cs and IBodySettings.cs (GameInitialization.cs excluded due to pre-existing out-of-scope TODOs) |
| 16a | After RequireVariables() removal, GetCFlag helper body contains `_variables.GetCharacterFlag` call |
| 16b | After RequireVariables() removal, SetCFlag helper body contains `_variables.SetCharacterFlag` call |
| 17 | Add bug-fix behavioral test that seeds `TalentIndex.Gender` and verifies HAS_VAGINA body option outcome — verifies TALENT index correction at runtime |
| 18 | Run `dotnet test Era.Core.Tests` — all Era.Core tests pass after MockVariableStore GetTalent/SetTalent updates |
| 19 | BodyDetailInit sets BodyOption4 (last CFLAG index) via `SetCFlag(characterId, CharacterFlagIndex.BodyOption4, ...)` — brackets the 17 CFLAG writes with AC#3 (HairLength, first index) |
| 20 | Remove BodyDetailInit-specific TODO comment from `GameInitialization.cs`; verify no `TODO.*BodyDetailInit` matches remain |
| 21 | Create `pm/features/feature-797.md` [DRAFT] file for UterusVolumeInit/TemperatureToleranceInit delegate migration handoff |
| 22 | Register F797 in `index-features.md` Active Features table with [DRAFT] status and increment Next Feature number |
| 23 | Bug-fix behavioral test must pin Character0Male.HairLength=100 when Gender=0 — breaks AC#13 circularity with concrete ground-truth value |
| 24 | Verify F797 DRAFT references `UterusVolumeInit` to confirm handoff captures deferred scope |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| GetTalent helper placement | A: inline `_variables.GetTalent(...)` call in BodyDetailInit body; B: private helper method following GetCFlag/SetCFlag pattern | B: private helper | Consistent with established GetCFlag/SetCFlag pattern (lines 506-510); improves readability; single place to change error handling |
| RequireVariables removal strategy | A: Keep RequireVariables() and update callers to use it; B: Remove RequireVariables() and update GetCFlag/SetCFlag to call `_variables` directly | B: Remove RequireVariables() | With non-nullable `_variables`, the null guard is dead code. Keeping dead code violates TreatWarningsAsErrors and Zero Debt Upfront principle |
| Test MockVariableStore GetTalent implementation | A: Leave as `throw new NotImplementedException()` and create separate mock; B: Implement GetTalent in the existing MockVariableStore using the same `_store` dictionary pattern | B: Implement in existing mock | The store already has ConcurrentDictionary<string,int> keyed by "varType:charId:index". Adding TALENT entries follows the identical pattern used for CFLAG and BASE. Minimal code change; no new class needed |
| GameInitialization TODO comment handling | A: Replace TODO with explanatory comment about the simplification; B: Remove comment entirely (code is self-explanatory) | B: Remove entirely | The simplified `_bodySettings.BodyDetailInit(characterId)` is self-explanatory. Adding a comment where none is needed creates noise. AC#15 requires zero TODO/FIXME/HACK markers |
| `_variables` field nullability | A: Keep nullable (`IVariableStore?`) and add `!` assertion; B: Make non-nullable (`IVariableStore`) after removing parameterless constructor | B: Non-nullable | Compile-time safety. With single constructor requiring `IVariableStore`, the field is always initialized. Non-nullable is the correct C# idiom |

### Interfaces / Data Structures

No new interfaces or data structures are needed.

**New private helper method in BodySettings.cs** (following GetCFlag/SetCFlag pattern at lines 506-510):

```csharp
private int GetTalent(int characterId, TalentIndex talent)
    => _variables.GetTalent(new CharacterId(characterId), talent).Match(v => v, _ => 0);
```

**Updated GetCFlag/SetCFlag helpers** (after removing `RequireVariables()` indirection):

```csharp
private int GetCFlag(int characterId, CharacterFlagIndex flag)
    => _variables.GetCharacterFlag(new CharacterId(characterId), flag).Match(v => v, _ => 0);

private void SetCFlag(int characterId, CharacterFlagIndex flag, int value)
    => _variables.SetCharacterFlag(new CharacterId(characterId), flag, value);
```

**Updated MockVariableStore.GetTalent in BodySettingsBusinessLogicTests.cs** (replaces `throw new NotImplementedException()`):

```csharp
public Result<int> GetTalent(CharacterId character, TalentIndex talent)
{
    var key = Key("TALENT", character.Value, talent.Value);
    return _store.TryGetValue(key, out var value)
        ? Result<int>.Ok(value)
        : Result<int>.Ok(0);
}
```

A corresponding `SetTalent` implementation should also be added (same pattern) so tests can seed talent values for character 0 gender-branch testing.

### Upstream Issues

<!-- No upstream issues found. All required interface methods confirmed present in IVariableStore:
     - GetCharacterFlag(CharacterId, CharacterFlagIndex): Result<int> — IVariableStore.cs:28
     - SetCharacterFlag(CharacterId, CharacterFlagIndex, int): void — IVariableStore.cs:29
     - GetTalent(CharacterId, TalentIndex): Result<int> — IVariableStore.cs:34
     - TalentIndex.Gender = new(2) confirmed in TalentIndex.cs:32
     - CharacterFlagIndex.HairLength through BodyOption4 confirmed in CharacterFlagIndex.cs:35-51
     MockVariableStore.SetTalent currently throws NotImplementedException (IVariableStore.cs:80).
     This is addressed in Technical Design (Interfaces section) as a required mock update, not an upstream gap. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| MockVariableStore.SetTalent also throws NotImplementedException but tests seeding talent values for char 0 gender branch require it | AC Design Constraints (C8) / Test files | Implement SetTalent in MockVariableStore using the same `_store` dictionary pattern as GetTalent above. Add helper `public void SetTALENT(int charId, TalentIndex talent, int value)` for test setup ergonomics |

<!-- fc-phase-5-completed -->

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2 | Update `Era.Core/Interfaces/IBodySettings.cs`: replace the four-parameter `BodyDetailInit` declaration with `void BodyDetailInit(int characterId)` | | [x] |
| 2 | 3,4,5,6,7,8,14,16a,16b,19 | Rewrite `Era.Core/State/BodySettings.cs`: remove delegate parameters and null checks from `BodyDetailInit`; add `GetTalent` private helper following GetCFlag/SetCFlag pattern; update `GetCFlag`/`SetCFlag` to call `_variables` directly (remove `RequireVariables()` indirection); fix TALENT index from raw `0` to `TalentIndex.Gender`; remove parameterless constructor; change `_variables` field from `IVariableStore?` to `IVariableStore` | | [x] |
| 3 | 9,10,15,20 | Update `Era.Core/Common/GameInitialization.cs`: remove dummy lambda creation (`Func<int,int,int> getCflag`, `Action<int,int,int> setCflag`, `Func<int,int,int> getTalent`) and the existing TODO comment; replace `_bodySettings.BodyDetailInit(characterId, getCflag, setCflag, getTalent)` with `_bodySettings.BodyDetailInit(characterId)` | | [x] |
| 4 | 12,13,17,23 | Update test files: implement `GetTalent` and `SetTalent` in `MockVariableStore` (in `BodySettingsBusinessLogicTests.cs`) using the `_store` dictionary pattern; rewrite all `BodyDetailInit` test methods across `StateSettingsTests.cs`, `HeadlessIntegrationTests.cs`, and `GameInitializationTests.cs` to use `new BodySettings(mockStore)` instead of `new BodySettings()` and delegate lambdas; add at least one new test that seeds `TalentIndex.Gender=0` via `SetTALENT` helper and asserts Character0Male preset (HairLength=100) outcome, verifying bug-fix behavioral correctness with pinned ground-truth value | | [x] |
| 5 | 11 | Run `dotnet build Era.Core` and verify zero errors and zero warnings | | [x] |
| 6 | 12,13,18 | Run `dotnet test engine.Tests` (all tests), `dotnet test Era.Core.Tests` (Era.Core tests), and `dotnet test engine.Tests --filter BodyDetailInit` (equivalence tests) and verify all pass | | [x] |
| 7 | 21,22,24 | Create `feature-797.md` [DRAFT] for UterusVolumeInit/TemperatureToleranceInit delegate-to-IVariableStore migration; register in `index-features.md`; increment "Next Feature number" to 798 | | [x] |

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
| 1 | implementer | sonnet | feature-796.md (Tasks T1–T3), `Era.Core/Interfaces/IBodySettings.cs`, `Era.Core/State/BodySettings.cs`, `Era.Core/Common/GameInitialization.cs` | Updated IBodySettings.cs, BodySettings.cs, GameInitialization.cs |
| 2 | implementer | sonnet | feature-796.md (Task T4), `Era.Core.Tests/State/BodySettingsBusinessLogicTests.cs`, `engine.Tests/Tests/StateSettingsTests.cs`, `engine.Tests/Tests/HeadlessIntegrationTests.cs`, `engine.Tests/Tests/GameInitializationTests.cs` | Updated test files with MockVariableStore GetTalent/SetTalent and rewritten BodyDetailInit tests |
| 3 | tester | sonnet | feature-796.md (Tasks T5–T6), WSL dotnet environment | Build pass confirmation, test pass confirmation |

### Pre-conditions

- F779 is [DONE]: IVariableStore methods (GetCharacterFlag, SetCharacterFlag, GetTalent) confirmed present in `IVariableStore.cs`
- F789 is [DONE]: All required typed index types confirmed (`CharacterFlagIndex.HairLength`–`BodyOption4`, `TalentIndex.Gender`)
- Baseline captured in `.tmp/baseline-796.txt`

### Execution Order

Tasks must execute in sequence: T1 → T2 → T3 (interface and all call sites consistent before any build), then T4 (test update), then T5 (build), then T6 (test run).

**T1 before T2**: IBodySettings interface must be updated before BodySettings implementation to align the contract boundary.

**T1+T2+T3 atomic group**: These three changes form a breaking change set. All must be applied before running the build (T5); partial application causes compilation errors.

### New Private Helper (BodySettings.cs)

Add following the `SetCFlag` pattern at lines 506–510:

```csharp
private int GetTalent(int characterId, TalentIndex talent)
    => _variables.GetTalent(new CharacterId(characterId), talent).Match(v => v, _ => 0);
```

### Updated Helpers (BodySettings.cs — after RequireVariables removal)

```csharp
private int GetCFlag(int characterId, CharacterFlagIndex flag)
    => _variables.GetCharacterFlag(new CharacterId(characterId), flag).Match(v => v, _ => 0);

private void SetCFlag(int characterId, CharacterFlagIndex flag, int value)
    => _variables.SetCharacterFlag(new CharacterId(characterId), flag, value);
```

### MockVariableStore Updates (BodySettingsBusinessLogicTests.cs)

Replace `throw new NotImplementedException()` for `GetTalent`:

```csharp
public Result<int> GetTalent(CharacterId character, TalentIndex talent)
{
    var key = Key("TALENT", character.Value, talent.Value);
    return _store.TryGetValue(key, out var value)
        ? Result<int>.Ok(value)
        : Result<int>.Ok(0);
}
```

Add `SetTalent` (same pattern, needed for test seeding of char 0 gender-branch):

```csharp
public void SetTalent(CharacterId character, TalentIndex talent, int value)
{
    var key = Key("TALENT", character.Value, talent.Value);
    _store[key] = value;
}
```

Also add helper for test setup ergonomics:

```csharp
public void SetTALENT(int charId, TalentIndex talent, int value)
    => SetTalent(new CharacterId(charId), talent, value);
```

### Build Verification (T5)

Run via WSL:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core'
```

Expected: `Build succeeded. 0 Error(s). 0 Warning(s).`

### Test Verification (T6)

Run via WSL:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests'
```

Then Era.Core.Tests:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests'
```

Then filter run:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests --filter BodyDetailInit'
```

Expected: All tests pass. BodyDetailInit filter must return ≥16 passing tests (count preserved from baseline).

### Success Criteria

All 25 ACs pass (AC#1–15, 16a, 16b, 17–24):
- AC#1–2: Interface signature verified (no delegates, int-only parameter)
- AC#3–4, 7–8, 14, 16a, 16b: BodySettings.cs implementation verified (SetCFlag usage, GetTalent usage, no raw index 0, no RequireVariables, no Func/Action, _variables direct usage in helpers)
- AC#5–6: Structural changes verified (no parameterless ctor, non-nullable field)
- AC#9–10, 15, 20: GameInitialization.cs verified (no dummy lambdas, simplified call, no TODO/FIXME/HACK, BodyDetailInit TODO removed)
- AC#11–13, 17–18, 23: Build succeeds, all tests pass (engine.Tests and Era.Core.Tests), equivalence tests pass, bug-fix behavioral test exists with pinned ground-truth HairLength=100
- AC#19: BodyOption4 spot-check (brackets 17 CFLAG writes with AC#3)
- AC#21–22, 24: F797 DRAFT file created, registered, and content verified (leak prevention with scope pinning)

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| MockVariableStore.SetTalent currently throws NotImplementedException | SetTalent must be implemented to enable test seeding for char 0 gender-branch coverage; addressed within this feature's test Task (T4) | Feature | F796 (self — resolved in T4) | Task#4 |
| UterusVolumeInit (IPregnancySettings) and TemperatureToleranceInit (IWeatherSettings) have identical delegate-based dual-pattern | Same dual access pattern as BodyDetailInit; dummy lambdas at GameInitialization.cs lines 335, 354; identified during F796 maintainability review | Feature | F797 (new DRAFT) | Task#7 |

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
| 2026-02-18 | START | initializer | Phase 1 Initialize | READY |
| 2026-02-18 | START | implementer | Phase 3 TDD RED | RED confirmed (4 CS7036) |
| 2026-02-18 | START | implementer | Tasks T1-T3 (atomic) | SUCCESS |
| 2026-02-18 | END | implementer | Tasks T1-T3 | Build 0 errors 0 warnings |
| 2026-02-18 | START | implementer | Task T4 (tests) | SUCCESS |
| 2026-02-18 | END | implementer | Task T4 | 585/585 engine, 2244/2244 Era.Core |
| 2026-02-18 | GREEN | orchestrator | T5 build verify | Build succeeded |
| 2026-02-18 | GREEN | orchestrator | T6 test verify | 585+2244 PASS, 17 BodyDetailInit |
| 2026-02-18 | START | implementer | Task T7 (F797 DRAFT) | SUCCESS |
| 2026-02-18 | DEVIATION | ac-static-verifier | code ACs | AC#9,22,23 FAIL (15/18) |
| 2026-02-18 | DEVIATION | feature-reviewer | Phase 8.1 | NEEDS_REVISION: AC#9,22,23 matchers |
| 2026-02-21 | FIX | orchestrator | AC Details alignment | AC#9,22,23 Details→Table pattern sync |
| 2026-02-21 | DEVIATION | Bash | dotnet test --filter | exit 1: ExclusiveRanges CS0103 (NuGet restore partial) |
| 2026-02-21 | DEVIATION | Bash | dotnet test --no-restore | exit 1: NETSDK1064 package not found |
| 2026-02-21 | DEVIATION | Bash | dotnet restore | exit 1: MSB1011 multiple projects (retry with specific project succeeded) |
| 2026-02-21 | GREEN | orchestrator | Phase 7 AC verify | 24/24 ACs PASS (585 engine, 2277 Era.Core, 17 BodyDetailInit) |
| 2026-02-21 | DEVIATION | feature-reviewer | Phase 8.1 | NEEDS_REVISION: Status [WIP] but all tasks done (expected: Phase 10 sets [DONE]) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Links section | Added F647 (Phase 20 Planning) to Links — referenced in Philosophy section but missing from Links
- [fix] Phase1-RefCheck iter1: Implementation Contract Phase 2 | Fixed test file paths: BodySettingsBusinessLogicTests.cs is in Era.Core.Tests/State/ not engine.Tests/; other test files are in engine.Tests/Tests/ not engine.Tests/
- [fix] Phase2-Review iter1: Review Context section | Restructured to template format (### Origin, ### Identified Gap, ### Review Evidence, ### Files Involved, ### Parent Review Observations)
- [fix] Phase2-Review iter1: ## Created line | Removed non-template Created date field
- [fix] Phase2-Review iter1: ## Tasks header | Added missing blank line before Tasks section
- [fix] Phase2-Uncertain iter1: AC#7 matcher | Changed from lowercase getTalent(characterId, 0) to uppercase GetTalent(characterId, 0) to guard against post-migration regression
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#16 (GetCFlag/SetCFlag helpers use _variables directly) to verify unified access pattern after RequireVariables removal
- [fix] Phase2-Review iter1: Task#4 description | Added behavioral test requirement for TalentIndex.Gender bug-fix verification
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#17 (Bug-fix behavioral test for TalentIndex.Gender) to verify test existence
- [resolved-applied] Phase3-Maintainability iter3: Leak Prevention — UterusVolumeInit (IPregnancySettings) and TemperatureToleranceInit (IWeatherSettings) have identical dual-pattern (delegate params with dummy lambdas). → User chose: Create new DRAFT feature (Step 6.3 DEFER destination A)
- [fix] Phase3-Maintainability iter3: AC#15 scope | Narrowed from 3 files to 2 (excluded GameInitialization.cs due to pre-existing out-of-scope TODOs at lines 335, 354)
- [fix] Phase3-Maintainability iter3: AC Definition Table | Added AC#18 (Era.Core.Tests pass after MockVariableStore changes)
- [fix] Phase3-Maintainability iter3: Approach narrative | Updated stale "15 ACs" to "18 ACs" with AC#15-18 bullets
- [fix] Phase3-Maintainability iter3: AC#9 rationale | Documented that setCflag/getTalent lambda removal is implicitly covered by AC#10
- [resolved-applied] Phase2-Pending iter4: AC#3 completeness — reviewer re-raised concern that AC#3 only spot-checks HairLength. → User chose: Add SetCFlag count check AC (≥17 calls)
- [fix] Phase4-ACValidation iter4: AC#9 matcher | Improved whitespace handling: `Func<int,\s*int,\s*int>.*getCflag` (handles variable spacing)
- [fix] Phase4-ACValidation iter4: AC#15, AC#16, AC#17 matchers | Fixed pipe escaping: `\|` → `|` (regex alternation, not literal pipe)
- [fix] PostLoop-UserFix post-loop: AC Definition Table | Added AC#19 (SetCFlag count >= 17) for positive completeness of all 17 CFLAG writes
- [fix] Phase2-Review iter6: AC#15 rationale | Documented AC#10 implicit coverage of GameInitialization.cs TODO removal
- [fix] Phase2-Review iter6: AC#19 rationale | Documented file-scope limitation (counts all SetCFlag, not just BodyDetailInit)
- [fix] Phase2-Review iter6: Philosophy Derivation | Narrowed "all methods" claim scope — BodyDetailInit was sole delegate user per Impact Analysis
- [fix] Phase2-Review iter7: AC#19 | Changed from file-wide count to BodyOption4 spot-check (brackets 17 CFlags with AC#3 HairLength)
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#20 (GameInitialization BodyDetailInit TODO removed) — narrow scope excluding out-of-scope TODOs
- [fix] Phase2-Review iter7: AC#13 | Amended "identical values" to "with bug-fix delta" — TALENT index fix changes HAS_VAGINA for affected characters
- [resolved-skipped] Phase2-Pending iter8: AC#19 count vs spot-check — A→B→A loop: User confirmed: keep BodyOption4 spot-check (AC#3+AC#19 bracket + AC#14 negative is sufficient)
- [fix] Phase2-Review iter8: AC#14 rationale | Clarified file-scope grep coverage (not method-scope)
- [fix] Phase2-Review iter9: Goal Coverage row 8 | Moved AC#15 from row 8 (GameInitialization) to row 6 (BodySettings pattern); AC#20 remains for GameInitialization TODO
- [fix] Phase2-Review iter10: Goal Coverage row 6 | Added AC#4 to cover GetTalent _variables access path
- [fix] Phase3-Maintainability iter10: Mandatory Handoffs | Added F797 DRAFT handoff for UterusVolumeInit/TemperatureToleranceInit + Task#7 + AC#21/22
- [fix] Phase4-ACValidation iter10: AC#21 | Split compound AC into AC#21 (file exists) + AC#22 (index registration)
- [fix] Phase2-Review iter1: AC Definition Table | Escaped pipe characters in AC#1, AC#14, AC#15, AC#16, AC#17, AC#20 Expected columns (markdown table delimiter conflict)
- [fix] Phase2-Review iter1: AC Details section | Reordered AC#20 before AC#21/AC#22 for ascending numeric sequence
- [fix] Phase2-Review iter1: AC Coverage table | Added missing rows for AC#18, AC#20, AC#21, AC#22
- [fix] Phase2-Review iter2: Goal Coverage row 6 | Removed AC#15 (TODO/FIXME/HACK check ≠ access pattern verification)
- [fix] Phase2-Review iter2: AC#17 matcher | Added HasVagina.*Gender and Gender.*HasVagina alternatives for assertion-side verification
- [fix] Phase2-Review iter3: AC#16 | Split into AC#16a (GetCharacterFlag) + AC#16b (SetCharacterFlag) to prevent partial-match false positive
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#23 (concrete HairLength=100 ground-truth for char 0 male path) to break AC#13 circularity
- [fix] Phase2-Review iter5: AC#23 matcher | Removed degenerate `Male.*100|100.*Male` alternatives (false-positive risk from method names/comments)
- [fix] Phase2-Review iter6: Approach narrative + Success Criteria | Updated AC count from 22 to 24 (AC#16a/16b split + AC#23 added); added missing AC#19/21/22/23 to Success Criteria bullets
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#24 (F797 content verification — UterusVolumeInit scope pinning for leak prevention)
- [fix] Phase8-NEEDS_REVISION iter1: AC#9 Details | Aligned pattern to AC table: `BodyDetailInit\(characterId,\s*getCflag` (was `Func<int,\s*int,\s*int>.*getCflag` — matched out-of-scope UterusVolumeInit)
- [fix] Phase8-NEEDS_REVISION iter1: AC#22 Details | Aligned pattern to AC table: `797.*DRAFT` (was `F797.*DRAFT` — index uses numeric ID without F prefix)
- [fix] Phase8-NEEDS_REVISION iter1: AC#23 Details | Aligned path to AC table: `BodyDetailInitMigrationTests.cs` (was `BodySettingsBusinessLogicTests.cs` — test was created in migration-specific test file)

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F779](feature-779.md) - Body Settings UI — introduced IVariableStore methods creating the dual pattern
- [Related: F789](feature-789.md) - IVariableStore Phase 20 Extensions — provides needed interface methods
- [Related: F778](feature-778.md) - BodyDetailInit verification — verified existing behavior; its tests will be rewritten
- [Related: F780](feature-780.md) - Genetics & Growth (Phase 20 sibling); no call chain dependency on BodyDetailInit
- [Related: F794](feature-794.md) - Shared validation abstraction (Phase 20 sibling); separate scope
- [Successor: F782](feature-782.md) - Post-Phase Review depends on F796 completion
- [Related: F647](feature-647.md) - Phase 20 Planning — decomposed Phase 20 into actionable sub-features
