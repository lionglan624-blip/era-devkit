# Feature 789: IVariableStore Phase 20 Extensions

## Status: [DONE]
<!-- fl-reviewed: 2026-02-15T00:00:00Z -->

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

Extend IVariableStore interface with string variable accessors (SAVESTR) and 3D character array accessors (TA:chr:slot:sub-index) required by Phase 20 ERB migrations.

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity -- Each phase completion triggers next phase planning. The VariableStore system (IVariableStore and its ISP-segregated sub-interfaces) is the single source of truth for ERA variable access in C# migrated code. Phase 20 (Equipment & Shop Systems) introduces the first consumers requiring string variable arrays and 3D integer arrays, which must be exposed through ISP-segregated sub-interfaces to maintain interface cohesion and avoid breaking existing implementations.

### Problem (Current Issue)

IVariableStore (Era.Core/Interfaces/IVariableStore.cs) was designed incrementally across Phases 9-19 to serve only integer access patterns: 1D global arrays (FLAG, TFLAG, PALAMLV) and 2D character-scoped arrays (CFLAG, ABL, TALENT, etc.). All 38 methods return `int` or `Result<int>`. Phase 20 Shop Core (F774) migrated ShopSystem and ShopDisplay, which require two fundamentally different variable access patterns that do not exist in the interface:

1. **String variable access (SAVESTR)**: ShopSystem.HasCollectionUnlocked() (ShopSystem.cs:410-411) throws `NotImplementedException` because it needs SAVESTR:10 (a 1D global string array, size 100). VariableCode.cs:180 defines SAVESTR with `__STRING__ | __ARRAY_1D__` flags, but no interface exposes string Get/Set.

2. **3D integer array access (TA)**: ShopDisplay.CalculateSpermAmount() (ShopDisplay.cs:385-388) throws `NotImplementedException` because it needs TA:charA:charB:subIndex (a 3D integer array [305,305,10]). VariableCode.cs:267 defines TA with `__INTEGER__ | __ARRAY_3D__` flags, but IVariableStore only supports up to 2D access.

The root cause is that IVariableStore's incremental extension philosophy adds new ERA variable types only when migrated features actually need them. String variables and 3D arrays are structurally different access patterns (different return type, different dimensionality) that no prior phase demanded.

### Goal (What to Achieve)

Create ISP-segregated sub-interfaces (IStringVariables for SAVESTR Get/Set, I3DArrayVariables for TA/TB Get/Set) with backing storage in VariableStore, DI registration following the established pattern (ServiceCollectionExtensions.cs:86-92), and replace the two NotImplementedException stubs in ShopSystem and ShopDisplay with real accessor calls.

<!-- Sub-Feature Requirements (architecture.md): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

---

## Scope Reference

### Source Files

| File | Lines | Functions | Note |
|------|------:|-----------|------|
| Era.Core/Interfaces/IVariableStore.cs | 104 | 38 methods (19 Get + 19 Set) | Existing interface; NOT extended directly |
| Era.Core/Variables/VariableStore.cs | ~486 | Implements 5 ISP interfaces | Add string[] and int[,,] backing storage |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | ~92 | ISP DI registration | Add new interface registrations |
| Era.Core/Shop/ShopSystem.cs | ~455 | HasCollectionUnlocked() stub | Stub replacement target |
| Era.Core/Shop/ShopDisplay.cs | ~485 | CalculateSpermAmount() stub | Stub replacement target |

### F774 Mandatory Handoff Origin

| Issue | Location |
|-------|----------|
| IVariableStore string extension (SAVESTR) | ShopSystem.cs:410-411 HasCollectionUnlocked() stub |
| IVariableStore 3D accessor (TA array) | ShopDisplay.cs:385-388 CalculateSpermAmount() stub |

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do ShopSystem.HasCollectionUnlocked() and ShopDisplay.CalculateSpermAmount() throw NotImplementedException? | They need SAVESTR (string) and TA (3D int) access, but IVariableStore has no methods for either pattern | Era.Core/Shop/ShopSystem.cs:410-411, Era.Core/Shop/ShopDisplay.cs:385-388 |
| 2 | Why does IVariableStore lack string and 3D array methods? | All 38 methods are integer-typed with max 2D dimensionality (1D global or 2D character-scoped) | Era.Core/Interfaces/IVariableStore.cs:1-104 |
| 3 | Why was IVariableStore built with only integer 1D/2D patterns? | It was extended incrementally per migration phase: F393 (BASE, TCVAR), F399 (SOURCE, MARK, NOWEX, MAXBASE, CUP), F400 (JUEL, GOTJUEL, PALAMLV), F469 (STAIN, DOWNBASE) -- all integer-typed | IVariableStore.cs:44,49,54,59,64,79,83,89,94,98-101 |
| 4 | Why did no prior phase require string or 3D access? | Phases 9-19 migrated training, state, and juel systems that exclusively use integer variables (CFLAG, ABL, TALENT, PALAM, EXP, etc.) | pm/index-features.md |
| 5 | Why (Root)? | The ERA variable system has 4 dimensionalities (scalar, 1D, 2D, 3D) and 2 value types (int, string), but the incremental extension philosophy only adds patterns when concrete consumers arrive. Phase 20 is the first phase with consumers (SAVESTR for shop collections, TA for sperm tracking) requiring the previously-unneeded string and 3D patterns | Era.Core/Variables/VariableCode.cs:180 (SAVESTR = __STRING__), VariableCode.cs:267 (TA = __ARRAY_3D__) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | NotImplementedException thrown in HasCollectionUnlocked() and CalculateSpermAmount() | IVariableStore interface lacks string variable and 3D array variable accessor methods because the incremental extension pattern only adds types when consumers need them |
| Where | Era.Core/Shop/ShopSystem.cs:410-411, Era.Core/Shop/ShopDisplay.cs:385-388 | Era.Core/Interfaces/IVariableStore.cs (interface design), Era.Core/Variables/VariableCode.cs (type flags defined but not exposed) |
| Fix | Add TODO comments or hardcoded workarounds in stubs | Create ISP-segregated sub-interfaces (IStringVariables, I3DArrayVariables) with backing storage and DI registration, then replace stubs with real accessor calls |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Predecessor -- Phase 20 Planning (parent) |
| F774 | [DONE] | Predecessor -- Shop Core (Mandatory Handoff origin for both SAVESTR and TA) |
| F775 | [DRAFT] | Successor -- Collection feature uses SAVESTR:10 heavily (20+ occurrences in SHOP_COLLECTION.ERB) |
| F776 | [DRAFT] | Related -- Items feature (Phase 20 sibling) |
| F782 | [DRAFT] | Successor -- Post-Phase 20 Review |
| F788 | [DONE] | Related -- IConsoleOutput Extensions (parallel, no dependency) |
| F790 | [DONE] | Related -- Engine Data Access Layer (parallel, no dependency) |
| F791 | [PROPOSED] | Related -- Engine State Transitions (parallel, no dependency) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface design precedent | FEASIBLE | ISP segregation pattern established by F404; ServiceCollectionExtensions.cs:86-92 registers 5 interfaces for single VariableStore |
| String variable infrastructure | FEASIBLE | SAVESTR is 1D global string array (size 100); VariableSizeConfig.cs:63 already models SAVESTR property |
| 3D array infrastructure | FEASIBLE | TA is global 3D int array [305,305,10]; VariableSizeConfig.cs:126-127 already models TA/TB as int[] |
| Type system readiness | FEASIBLE | Result<T> is generic (supports Result<string>); SaveStrIndex needs creation (follows FlagIndex pattern) |
| Consumer sites identified | FEASIBLE | Exactly 2 stubs to replace: ShopSystem.cs:410-411 and ShopDisplay.cs:385-388 |
| Test infrastructure | FEASIBLE | Existing VariableStoreTests pattern applicable; new ISP interfaces avoid mock breakage |
| Dependencies satisfied | FEASIBLE | Both predecessors (F647, F774) are [DONE] |
| Mock breakage risk | FEASIBLE | ISP sub-interfaces mean zero breakage to existing 16 IVariableStore implementations |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Interfaces/ | MEDIUM | Two new interface files (IStringVariables.cs, I3DArrayVariables.cs) |
| Era.Core/Variables/VariableStore.cs | MEDIUM | New backing storage (string[] for SAVESTR, int[,,] for TA/TB) and interface implementations |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | LOW | Two new singleton registrations following existing pattern |
| Era.Core/Shop/ShopSystem.cs | LOW | Replace HasCollectionUnlocked() stub with IStringVariables call |
| Era.Core/Shop/ShopDisplay.cs | LOW | Replace CalculateSpermAmount() stub with I3DArrayVariables call |
| Era.Core/Types/ | LOW | New SaveStrIndex strongly-typed index |
| Existing IVariableStore implementations | NONE | ISP segregation avoids breaking any of the 16 existing implementations |
| F775 Collection (downstream) | HIGH | Unblocks SAVESTR:10 access needed by 20+ SHOP_COLLECTION.ERB occurrences |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| SAVESTR is 1D global string array (not character-scoped) | VariableCode.cs:180 (no __CHARACTER_DATA__ flag) | String interface uses simple index parameter, not CharacterId + index |
| TA is 3D non-character-scoped integer array | VariableCode.cs:267 (__ARRAY_3D__ without __CHARACTER_DATA__) | Three integer indices; storage in VariableStore directly, not CharacterVariables |
| TA dimensions are [305, 305, 10] | Game/config/variable_sizes.yaml:121 | Storage must accommodate 930,250 integers (~3.6MB) |
| TB dimensions are [305, 305, 10] | Game/config/variable_sizes.yaml:122 | Identical structure to TA; include in same interface |
| SAVESTR size is 100 | Game/config/variable_sizes.yaml:61 | 100 string slots |
| TreatWarningsAsErrors=true | Directory.Build.props (F708) | All new code must compile without warnings |
| ISP segregation pattern established | ServiceCollectionExtensions.cs:86-92 | New variable types must use separate interfaces, not extend IVariableStore |
| Result<T> pattern for 2D+ getters | IVariableStore.cs error handling design | 3D getters should return Result<int>; string getters should return string (1D global, static bounds) or Result<string> |
| TA used as relational data (charA:charB:attribute) | SHOP2.ERB:100, GameOptions.cs:258 | Character-to-character relationship matrix with sub-attributes |
| GameOptions.cs already accesses TA via delegate | GameOptions.cs:146-161 (Func<int,int,int,int> getTa) | Existing workaround; future refactor target but out of scope |
| Era.Core cannot reference engine layer | Architecture constraint (Issue 29) | All implementations stay in Era.Core; no GlobalStatic references |
| VariableStore lacks Reset/Clear method | Pre-existing limitation (all ISP interfaces) | New mutable state (_saveStr, _ta, _tb ~7.2MB) inherits this gap; game load/reset must be addressed holistically, not per-feature |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| TA 3D array memory overhead (305x305x10 = ~3.6MB per array) | MEDIUM | LOW | Dense int[,,] is acceptable for singleton lifetime; same allocation engine already performs |
| Scope creep to other string/3D variables (RESULTS, GLOBALS, DA-DE) | MEDIUM | MEDIUM | Strict YAGNI: only SAVESTR, TA, TB have concrete stub consumers |
| F775 Collection blocked without F789 | HIGH | HIGH | F775 Dependencies must list F789 as Predecessor |
| SAVESTR return type ambiguity (string vs Result<string>) | LOW | MEDIUM | Follow FLAG precedent: strongly-typed index (SaveStrIndex) provides type-safety against index mixing (not bounds safety). String return justified because out-of-bounds has well-defined non-error semantic default (string.Empty = ERA uninitialized SAVESTR). Note: PALAMLV (also 1D global) returns Result<int> but uses raw int index -- the differentiator is strongly-typed vs raw index, not dimensionality alone |
| CalculateSpermAmount loop complexity (301 entries x 5 slots) | LOW | LOW | Display-only computation; performance acceptable |
| TB has no current consumer but same interface | LOW | LOW | Include in I3DArrayVariables for completeness; zero implementation cost since structure matches TA |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| IVariableStore method count | Grep "Result<int>\|void Set\|int Get" Era.Core/Interfaces/IVariableStore.cs | 38 | 19 Get + 19 Set, all integer-typed |
| IVariableStore implementations | Grep "IVariableStore" --type cs | 16 files | Not broken by ISP approach |
| NotImplementedException stubs (F789-scoped) | Grep "NotImplementedException" Era.Core/Shop/ShopSystem.cs:410-411, ShopDisplay.cs:385-388 | 2 | F789 targets only; Era.Core/Shop/ has 16 total NotImplementedException stubs (other stubs are out of scope) |
| Existing ISP interfaces | Grep "ITrainingVariables\|ICharacterStateVariables\|IJuelVariables\|ITEquipVariables" Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | 4 | Current ISP count |

**Baseline File**: `.tmp/baseline-789.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | ISP segregation: new interfaces must NOT extend IVariableStore | 16 existing implementations; ServiceCollectionExtensions.cs:86-92 | AC must verify new interfaces are separate (IStringVariables, I3DArrayVariables) |
| C2 | SAVESTR is 1D global string array, size 100 | VariableCode.cs:180, variable_sizes.yaml:61 | AC must verify GetSaveStr(SaveStrIndex) returns string; SetSaveStr(SaveStrIndex, string) |
| C3 | TA/TB are 3D globally-stored integer arrays [305,305,10] (character-to-character relationship matrix, not stored per-character in CharacterVariables) | VariableCode.cs:267-268, variable_sizes.yaml:121-122 | AC must verify Get/Set with 3 integer parameters; storage not in CharacterVariables |
| C4 | Two F789-scoped stub replacements | ShopSystem.cs:410-411, ShopDisplay.cs:385-388 | AC must verify these two stubs are removed and replaced with real interface calls (other stubs in Shop/ are out of scope) |
| C5 | DI registration follows existing ISP pattern | ServiceCollectionExtensions.cs:86-92 | AC must verify singleton registration of IStringVariables and I3DArrayVariables |
| C6 | VariableStore is single implementation | VariableStore.cs:17-22 | AC must verify VariableStore implements both new interfaces |
| C7 | Strongly-typed SaveStrIndex required | FlagIndex pattern (Era.Core/Types/) | AC must verify SaveStrIndex type exists |
| C8 | Zero technical debt | architecture.md Zero Debt Upfront | AC must verify no TODO/FIXME/HACK in feature files |
| C9 | TDD RED-GREEN | CLAUDE.md | Tests before implementation |
| C10 | F774 stub comments reference "Phase 14" | ShopSystem.cs:411, ShopDisplay.cs:388 | Stub replacement should correct/remove Phase 14 reference |
| C11 | String getters use direct return (not Result<string>) | IVariableStore.cs:19 (GetFlag returns int directly); SAVESTR has compile-time-known size 100 | String getter for SAVESTR should return string directly (not Result<string>). Defensive bounds checking with string.Empty default is a separate concern from return type design |
| C12 | Interface Dependency: IStringVariables needed by ShopSystem | Interface Dependency Scan | ShopSystem must inject IStringVariables; AC must verify constructor parameter |
| C13 | Interface Dependency: I3DArrayVariables needed by ShopDisplay | Interface Dependency Scan | ShopDisplay must inject I3DArrayVariables; AC must verify constructor parameter |

### Constraint Details

**C1: ISP Segregation**
- **Source**: ServiceCollectionExtensions.cs:86-92 registers VariableStore as 5 separate interfaces; 16 classes implement IVariableStore
- **Verification**: Grep for new interface names in separate files; verify IVariableStore.cs is unchanged
- **AC Impact**: ac-designer must include file existence ACs for new interface files and verify IVariableStore is NOT modified

**C2: SAVESTR String Semantics**
- **Source**: VariableCode.cs:180 defines SAVESTR = __STRING__ | __ARRAY_1D__; variable_sizes.yaml:61 defines size 100
- **Verification**: Verify GetSaveStr signature returns string; SetSaveStr accepts string parameter
- **AC Impact**: ac-designer must include interface signature AC verifying string return type

**C3: TA/TB 3D Array Semantics**
- **Source**: VariableCode.cs:267-268 defines TA/TB = __INTEGER__ | __ARRAY_3D__; variable_sizes.yaml:121-122 defines [305,305,10]
- **Verification**: Verify Get method takes 3 int parameters and returns Result<int>; Set takes 3 int + value
- **AC Impact**: ac-designer must include interface signature AC verifying 3-parameter access pattern

**C4: F789-Scoped Stub Replacement**
- **Source**: ShopSystem.cs:410-411 (HasCollectionUnlocked, SAVESTR stub) and ShopDisplay.cs:385-388 (CalculateSpermAmount, TA stub) throw NotImplementedException
- **Verification**: These two specific stubs no longer throw NotImplementedException after implementation. Note: Era.Core/Shop/ contains 14 other NotImplementedException stubs (engine integration, sibling features) that are out of F789 scope and will remain
- **AC Impact**: ac-designer must verify the two F789-scoped stubs (HasCollectionUnlocked and CalculateSpermAmount) are replaced with real interface calls; do NOT assert that total NotImplementedException count in Shop/ drops to 0

**C5: DI Registration**
- **Source**: ServiceCollectionExtensions.cs:86-92 establishes the pattern
- **Verification**: Grep for AddSingleton with new interface names in ServiceCollectionExtensions.cs
- **AC Impact**: ac-designer must include DI registration AC with specific Grep pattern

**C6: VariableStore Single Implementation**
- **Source**: VariableStore.cs:17-22 (single class implementing 5 ISP interfaces; now 7 with F789)
- **Verification**: Grep for class declaration showing IStringVariables and I3DArrayVariables in inheritance list
- **AC Impact**: ac-designer must verify VariableStore implements both new interfaces (AC#6a, AC#6b)

**C7: SaveStrIndex Type**
- **Source**: FlagIndex, CharacterFlagIndex, AbilityIndex etc. patterns in Era.Core/Types/
- **Verification**: Glob for Era.Core/Types/SaveStrIndex.cs
- **AC Impact**: ac-designer must include file existence AC for SaveStrIndex.cs

**C8: Zero Technical Debt**
- **Source**: architecture.md Zero Debt Upfront principle; Sub-Feature Requirements mandate 負債解消
- **Verification**: Grep for TODO/FIXME/HACK in all F789-touched files
- **AC Impact**: ac-designer must include zero-debt AC across all new and modified files (AC#15)

**C9: TDD RED-GREEN**
- **Source**: CLAUDE.md test-first requirement
- **Verification**: Implementation Contract Phase 1 creates tests before Phase 2 implements
- **AC Impact**: ac-designer must include unit test pass AC (AC#14)

**C10: F774 Stub Phase 14 Reference**
- **Source**: ShopSystem.cs:411 and ShopDisplay.cs:388 stub messages reference "Phase 14"
- **Verification**: Stub replacement removes the incorrect Phase 14 reference along with the NotImplementedException
- **AC Impact**: AC#10 and AC#11 verify NotImplementedException removal; Phase 14 reference is removed as part of stub replacement

**C11: String Getter Return Convention**
- **Source**: IVariableStore.cs:19 -- GetFlag(FlagIndex) returns int directly (not Result<int>) because FLAG uses a strongly-typed FlagIndex. SAVESTR similarly uses a strongly-typed SaveStrIndex (C7), so the return type should be string directly (not Result<string>).
- **Bounds checking distinction**: The Result<T> wrapper is unnecessary (callers don't need to handle failure cases). However, defensive bounds checking that returns string.Empty for out-of-bounds indices is a separate concern — string.Empty is the ERA semantic default for uninitialized SAVESTR slots, making it a safe and meaningful fallback.
- **Counter-example**: PALAMLV (IVariableStore.cs:90) is also a 1D global array but returns Result<int>. PALAMLV uses a raw `int index` parameter (no strongly-typed index), so it needs Result for failure signaling. The design split: strongly-typed index = direct return type, raw int index = Result return type.
- **Verification**: SAVESTR will use SaveStrIndex (strongly-typed), so the direct-return convention (like FLAG) applies rather than the Result convention (like PALAMLV)
- **AC Impact**: ac-designer should verify GetSaveStr returns string (not Result<string>), with empty string default for uninitialized slots

**C12: Interface Dependency: IStringVariables needed by ShopSystem**
- **Source**: Interface Dependency Scan; ShopSystem.HasCollectionUnlocked() requires SAVESTR access
- **Verification**: Grep for IStringVariables in ShopSystem.cs constructor
- **AC Impact**: AC#12 verifies ShopSystem injects IStringVariables as constructor parameter

**C13: Interface Dependency: I3DArrayVariables needed by ShopDisplay**
- **Source**: Interface Dependency Scan; ShopDisplay.CalculateSpermAmount() requires TA access
- **Verification**: Grep for I3DArrayVariables in ShopDisplay.cs constructor
- **AC Impact**: AC#13 verifies ShopDisplay injects I3DArrayVariables as constructor parameter

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (parent) |
| Predecessor | F774 | [DONE] | Shop Core -- originated both Mandatory Handoffs (ShopSystem.cs:410-411, ShopDisplay.cs:385-388) |
| Successor | F775 | [DONE] | Collection -- uses SAVESTR:10 in 20+ occurrences (SHOP_COLLECTION.ERB); must list F789 as Predecessor |
| Successor | F782 | [DRAFT] | Post-Phase 20 Review |
| Related | F776 | [DONE] | Items feature (Phase 20 sibling) |
| Related | F788 | [DONE] | IConsoleOutput Extensions (parallel, no dependency) |
| Related | F790 | [DONE] | Engine Data Access Layer (parallel, no dependency) |
| Related | F791 | [DONE] | Engine State Transitions (parallel, no dependency) |

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
| "IVariableStore is the single source of truth for ERA variable access" | New variable types (string, 3D) must be accessible through the variable store system with correct behavioral contracts | AC#3, AC#4, AC#6, AC#18a, AC#18b, AC#18c, AC#18d, AC#18e, AC#18f, AC#18g, AC#19a, AC#19b, AC#19c, AC#19d, AC#19e, AC#19f |
| "must be exposed through ISP-segregated sub-interfaces" | IStringVariables and I3DArrayVariables must be separate interfaces, not extensions of IVariableStore | AC#1, AC#2, AC#7, AC#8, AC#9 |
| "maintain interface cohesion" | Each sub-interface serves a single access pattern (string 1D or integer 3D) | AC#1, AC#2, AC#3, AC#3b, AC#4, AC#4b, AC#4c, AC#4d |
| "avoid breaking existing implementations" | IVariableStore.cs must remain unchanged; existing 16 implementations unaffected | AC#7, AC#14 |
| "Pipeline Continuity -- Each phase completion triggers next phase planning" | Organizational practice; no implementation artifact to verify | N/A (resolved-skipped PHI-001) |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IStringVariables interface file exists | file | Glob(Era.Core/Interfaces/IStringVariables.cs) | exists | Era.Core/Interfaces/IStringVariables.cs | [ ] |
| 2 | I3DArrayVariables interface file exists | file | Glob(Era.Core/Interfaces/I3DArrayVariables.cs) | exists | Era.Core/Interfaces/I3DArrayVariables.cs | [ ] |
| 3 | IStringVariables declares GetSaveStr returning string | code | Grep(Era.Core/Interfaces/IStringVariables.cs) | matches | `string GetSaveStr\(SaveStrIndex` | [ ] |
| 3b | IStringVariables declares SetSaveStr | code | Grep(Era.Core/Interfaces/IStringVariables.cs) | matches | `void SetSaveStr\(SaveStrIndex.*string` | [ ] |
| 4 | I3DArrayVariables declares GetTa returning Result<int> | code | Grep(Era.Core/Interfaces/I3DArrayVariables.cs) | matches | `Result<int> GetTa\(int.*int.*int` | [ ] |
| 4b | I3DArrayVariables declares GetTb returning Result<int> | code | Grep(Era.Core/Interfaces/I3DArrayVariables.cs) | matches | `Result<int> GetTb\(int.*int.*int` | [ ] |
| 4c | I3DArrayVariables declares SetTa | code | Grep(Era.Core/Interfaces/I3DArrayVariables.cs) | matches | `void SetTa\(int.*int.*int.*int` | [ ] |
| 4d | I3DArrayVariables declares SetTb | code | Grep(Era.Core/Interfaces/I3DArrayVariables.cs) | matches | `void SetTb\(int.*int.*int.*int` | [ ] |
| 5 | SaveStrIndex strongly-typed index exists | file | Glob(Era.Core/Types/SaveStrIndex.cs) | exists | Era.Core/Types/SaveStrIndex.cs | [ ] |
| 6a | VariableStore implements IStringVariables | code | Grep(Era.Core/Variables/VariableStore.cs) | matches | `IStringVariables` | [ ] |
| 6b | VariableStore implements I3DArrayVariables | code | Grep(Era.Core/Variables/VariableStore.cs) | matches | `I3DArrayVariables` | [ ] |
| 7 | IVariableStore.cs is unchanged (38 methods, no new methods) | code | Grep(Era.Core/Interfaces/IVariableStore.cs) | not_matches | `IStringVariables\|I3DArrayVariables\|GetSaveStr\|GetTa\|GetTb` | [ ] |
| 8 | DI registration for IStringVariables | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | contains | `IStringVariables>(sp => sp.GetRequiredService<VariableStore>())` | [ ] |
| 9 | DI registration for I3DArrayVariables | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | contains | `I3DArrayVariables>(sp => sp.GetRequiredService<VariableStore>())` | [ ] |
| 10 | HasCollectionUnlocked stub replaced (no NotImplementedException) | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | `NotImplementedException.*SAVESTR` | [ ] |
| 11 | CalculateSpermAmount stub replaced (no NotImplementedException) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | not_matches | `NotImplementedException.*TA 3D array` | [ ] |
| 12 | ShopSystem injects IStringVariables | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `IStringVariables _stringVariables\|IStringVariables stringVariables` | [ ] |
| 13 | ShopDisplay injects I3DArrayVariables | code | Grep(Era.Core/Shop/ShopDisplay.cs) | matches | `I3DArrayVariables _arrayVariables\|I3DArrayVariables arrayVariables` | [ ] |
| 14 | Unit tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [ ] |
| 15 | Zero technical debt in all F789-touched source files | code | Grep(Era.Core/Interfaces/IStringVariables.cs,Era.Core/Interfaces/I3DArrayVariables.cs,Era.Core/Types/SaveStrIndex.cs,Era.Core/Variables/VariableStore.cs,Era.Core/Shop/ShopSystem.cs,Era.Core/Shop/ShopDisplay.cs,Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | not_matches | `TODO\|FIXME\|HACK` | [ ] |
| 16 | Build succeeds with zero warnings | build | dotnet build Era.Core | succeeds | - | [ ] |
| 17a | engine-dev SKILL.md contains SaveStrIndex | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | `SaveStrIndex` | [ ] |
| 17b | engine-dev SKILL.md contains IStringVariables | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | `IStringVariables` | [ ] |
| 17c | engine-dev SKILL.md contains I3DArrayVariables | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | `I3DArrayVariables` | [ ] |
| 18a | GetSaveStr returns empty string for uninitialized or out-of-bounds index | code | Grep(Era.Core.Tests/) | matches | `GetSaveStr.*string\.Empty\|GetSaveStr.*""` | [ ] |
| 18b | GetTa returns failure for out-of-bounds index | code | Grep(Era.Core.Tests/) | matches | `GetTa.*Fail\|GetTa.*out.*bound\|GetTa.*invalid` | [ ] |
| 18c | Unit test exercises TA maximum valid indices (304, 304, 9) | code | Grep(Era.Core.Tests/) | matches | `GetTa.*304` | [ ] |
| 18d | Unit test exercises SAVESTR maximum valid index (99) | code | Grep(Era.Core.Tests/) | matches | `SaveStrIndex.*99\|GetSaveStr.*99` | [ ] |
| 18e | Unit test exercises SetSaveStr null coalescing | code | Grep(Era.Core.Tests/) | matches | `SetSaveStr.*null\|null.*SetSaveStr` | [ ] |
| 18f | Unit test exercises SetSaveStr with out-of-bounds index (no exception) | code | Grep(Era.Core.Tests/) | matches | `SetSaveStr.*-1\|SetSaveStr.*100\|SetSaveStr.*out.*bound` | [ ] |
| 18g | Unit test exercises SetTa with out-of-bounds indices (no exception) | code | Grep(Era.Core.Tests/) | matches | `SetTa.*305\|SetTa.*-1\|SetTa.*out.*bound` | [ ] |
| 19a | HasCollectionUnlocked calls GetSaveStr | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `GetSaveStr` | [ ] |
| 19b | CalculateSpermAmount calls GetTa | code | Grep(Era.Core/Shop/ShopDisplay.cs) | matches | `GetTa` | [ ] |
| 19c | HasCollectionUnlocked uses STRLENS > 1 semantics | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `\.Length > 1` | [ ] |
| 19d | CalculateSpermAmount uses correct loop upper bound (301) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | matches | `< 301` | [ ] |
| 19e | CalculateSpermAmount iterates 5 slots | code | Grep(Era.Core/Shop/ShopDisplay.cs) | matches | `< 5\|slot < 5` | [ ] |
| 19f | CalculateSpermAmount uses conditional accumulation (> 0) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | matches | `value > 0` | [ ] |
| 20 | VariableStore summary comment lists 7 interfaces | code | Grep(Era.Core/Variables/VariableStore.cs) | matches | `7 interfaces\|IStringVariables.*I3DArrayVariables` | [ ] |

### AC Details

**AC#1: IStringVariables interface file exists**
- **Test**: Glob pattern="Era.Core/Interfaces/IStringVariables.cs"
- **Expected**: File exists
- **Rationale**: ISP segregation (C1) requires a separate interface file for string variable access. Must not extend IVariableStore.

**AC#2: I3DArrayVariables interface file exists**
- **Test**: Glob pattern="Era.Core/Interfaces/I3DArrayVariables.cs"
- **Expected**: File exists
- **Rationale**: ISP segregation (C1) requires a separate interface file for 3D integer array access. Must not extend IVariableStore.

**AC#3: IStringVariables declares GetSaveStr returning string**
- **Test**: Grep pattern=`string GetSaveStr\(SaveStrIndex` path="Era.Core/Interfaces/IStringVariables.cs"
- **Expected**: 1+ match confirming string return type with SaveStrIndex parameter
- **Rationale**: SAVESTR is a 1D global string array (C2). Per C11, the string getter returns string directly (not Result<string>) because SaveStrIndex is strongly-typed (following FLAG precedent). Defensive bounds checking returning string.Empty is acceptable as a separate concern from return type. SetSaveStr(SaveStrIndex, string) must also be declared.

**AC#3b: IStringVariables declares SetSaveStr**
- **Test**: Grep pattern=`void SetSaveStr\(SaveStrIndex.*string` path="Era.Core/Interfaces/IStringVariables.cs"
- **Expected**: 1+ match confirming void return type with SaveStrIndex and string parameters
- **Rationale**: Goal requires SAVESTR Get/Set. SetSaveStr completes the write side of the string variable interface.

**AC#4: I3DArrayVariables declares GetTa returning Result<int>**
- **Test**: Grep pattern=`Result<int> GetTa\(int.*int.*int` path="Era.Core/Interfaces/I3DArrayVariables.cs"
- **Expected**: 1+ match confirming Result<int> return type with 3 integer parameters
- **Rationale**: TA is a 3D non-character-scoped integer array [305,305,10] (C3). Three integer indices are required. Result<int> used because raw int parameters need runtime bounds checking. TB must also be declared with the same signature pattern.

**AC#4b: I3DArrayVariables declares GetTb returning Result<int>**
- **Test**: Grep pattern=`Result<int> GetTb\(int.*int.*int` path="Era.Core/Interfaces/I3DArrayVariables.cs"
- **Expected**: 1+ match confirming TB getter with same signature pattern as TA
- **Rationale**: Goal requires TA/TB Get/Set. TB has identical structure to TA per VariableCode.cs:267-268.

**AC#4c: I3DArrayVariables declares SetTa**
- **Test**: Grep pattern=`void SetTa\(int.*int.*int.*int` path="Era.Core/Interfaces/I3DArrayVariables.cs"
- **Expected**: 1+ match confirming TA setter with void return (fire-and-forget convention per IVariableStore.cs:11-13)
- **Rationale**: Goal requires TA Get/Set. SetTa completes the write side for TA array. Void return follows established setter convention — all 19 existing setters return void. Invalid indices silently ignored.

**AC#4d: I3DArrayVariables declares SetTb**
- **Test**: Grep pattern=`void SetTb\(int.*int.*int.*int` path="Era.Core/Interfaces/I3DArrayVariables.cs"
- **Expected**: 1+ match confirming TB setter with void return (fire-and-forget convention per IVariableStore.cs:11-13)
- **Rationale**: Goal requires TB Get/Set. SetTb completes the write side for TB array. Void return follows established setter convention.

**AC#5: SaveStrIndex strongly-typed index exists**
- **Test**: Glob pattern="Era.Core/Types/SaveStrIndex.cs"
- **Expected**: File exists
- **Rationale**: C7 requires a strongly-typed SaveStrIndex following the FlagIndex pattern (readonly record struct with Value property). This enables compile-time type safety and justifies the direct string return (C11). Note: SaveStrIndex intentionally diverges from FlagIndex by adding a MaxValue=99 constant for self-documenting bounds (enhancement added in Phase3-Maintainability iter7).

**AC#6a: VariableStore implements IStringVariables**
- **Test**: Grep pattern=`IStringVariables` path="Era.Core/Variables/VariableStore.cs"
- **Expected**: 1+ match in the class declaration inheritance list
- **Rationale**: C6 requires VariableStore as the single implementation for IStringVariables, following the existing ISP pattern.

**AC#6b: VariableStore implements I3DArrayVariables**
- **Test**: Grep pattern=`I3DArrayVariables` path="Era.Core/Variables/VariableStore.cs"
- **Expected**: 1+ match in the class declaration inheritance list
- **Rationale**: C6 requires VariableStore as the single implementation for I3DArrayVariables, following the existing ISP pattern.

**AC#7: IVariableStore.cs is unchanged**
- **Test**: Grep pattern=`IStringVariables\|I3DArrayVariables\|GetSaveStr\|GetTa\|GetTb` path="Era.Core/Interfaces/IVariableStore.cs"
- **Expected**: 0 matches
- **Rationale**: ISP segregation (C1) means IVariableStore must not be modified. New methods belong in separate interfaces. This ensures all 16 existing IVariableStore implementations remain unaffected.

**AC#8: DI registration for IStringVariables**
- **Test**: Grep pattern=`IStringVariables>(sp => sp.GetRequiredService<VariableStore>())` path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- **Expected**: 1 match
- **Rationale**: C5 requires DI registration following the established ISP pattern at ServiceCollectionExtensions.cs:86-92, where VariableStore is registered once and resolved as each ISP interface via GetRequiredService forwarding.

**AC#9: DI registration for I3DArrayVariables**
- **Test**: Grep pattern=`I3DArrayVariables>(sp => sp.GetRequiredService<VariableStore>())` path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- **Expected**: 1 match
- **Rationale**: Same DI pattern as AC#8 for the 3D array interface (C5).

**AC#10: HasCollectionUnlocked stub replaced**
- **Test**: Grep pattern=`NotImplementedException.*SAVESTR` path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: 0 matches (the stub throw on line 411 containing "SAVESTR:10 string check" is replaced with a real IStringVariables call)
- **Rationale**: C4 requires this specific F789-scoped stub (ShopSystem.cs:410-411) to be replaced. Pattern matches the throw line content directly (not the method name + exception on separate lines). The method should use _stringVariables.GetSaveStr() to check SAVESTR:10. C10 notes the stub incorrectly references "Phase 14" which should be removed.

**AC#11: CalculateSpermAmount stub replaced**
- **Test**: Grep pattern=`NotImplementedException.*TA 3D array` path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 0 matches (the stub throw at line 388 containing "TA 3D array" is replaced with a real I3DArrayVariables call)
- **Rationale**: C4 requires this specific F789-scoped stub (ShopDisplay.cs:385-388) to be replaced. Pattern matches the throw line content directly (method signature on line 385 is separate from throw on line 388, so cross-line matchers cannot be used). The method should use _arrayVariables.GetTa() to access TA:chr:i:count.

**AC#12: ShopSystem injects IStringVariables**
- **Test**: Grep pattern=`IStringVariables _stringVariables\|IStringVariables stringVariables` path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: 1+ match (field or constructor parameter)
- **Rationale**: C12 requires ShopSystem to inject IStringVariables to replace the HasCollectionUnlocked stub. The constructor must accept IStringVariables as a parameter.

**AC#13: ShopDisplay injects I3DArrayVariables**
- **Test**: Grep pattern=`I3DArrayVariables _arrayVariables\|I3DArrayVariables arrayVariables` path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 1+ match (field or constructor parameter)
- **Rationale**: C13 requires ShopDisplay to inject I3DArrayVariables to replace the CalculateSpermAmount stub. The constructor must accept I3DArrayVariables as a parameter.

**AC#14: Unit tests pass**
- **Test**: `dotnet test Era.Core.Tests`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: C9 requires TDD RED-GREEN. New interface implementations, SaveStrIndex type, DI registrations, and stub replacements must all be covered by passing unit tests. Existing tests must continue passing to confirm zero breakage.

**AC#15: Zero technical debt in all F789-touched source files**
- **Test**: Grep pattern=`TODO\|FIXME\|HACK` paths="Era.Core/Interfaces/IStringVariables.cs, Era.Core/Interfaces/I3DArrayVariables.cs, Era.Core/Types/SaveStrIndex.cs, Era.Core/Variables/VariableStore.cs, Era.Core/Shop/ShopSystem.cs, Era.Core/Shop/ShopDisplay.cs, Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- **Expected**: 0 matches across all F789-touched files (3 new + 4 modified)
- **Rationale**: C8 (Zero Debt Upfront) requires no technical debt markers in all files created or modified by F789. Sub-Feature Requirements mandate 負債解消 (debt resolution) including TODO/FIXME/HACK comment deletion.

**AC#16: Build succeeds with zero warnings**
- **Test**: `dotnet build Era.Core`
- **Expected**: Build succeeds (exit code 0) with zero warnings
- **Rationale**: TreatWarningsAsErrors=true (Directory.Build.props, F708). All new code including interfaces, SaveStrIndex, VariableStore extensions, and constructor changes must compile cleanly.

**AC#17a: engine-dev SKILL.md contains SaveStrIndex**
- **Test**: Grep pattern=`SaveStrIndex` path=".claude/skills/engine-dev/SKILL.md"
- **Expected**: 1+ match
- **Rationale**: ssot-update-rules.md requires engine-dev SKILL.md updates when new Era.Core/Types/*.cs files are created. SaveStrIndex must be documented in the strongly-typed index reference.

**AC#17b: engine-dev SKILL.md contains IStringVariables**
- **Test**: Grep pattern=`IStringVariables` path=".claude/skills/engine-dev/SKILL.md"
- **Expected**: 1+ match
- **Rationale**: ssot-update-rules.md requires engine-dev SKILL.md updates when new Era.Core/Interfaces/*.cs files are created. IStringVariables must be documented in the interface reference.

**AC#17c: engine-dev SKILL.md contains I3DArrayVariables**
- **Test**: Grep pattern=`I3DArrayVariables` path=".claude/skills/engine-dev/SKILL.md"
- **Expected**: 1+ match
- **Rationale**: ssot-update-rules.md requires engine-dev SKILL.md updates when new Era.Core/Interfaces/*.cs files are created. I3DArrayVariables must be documented in the interface reference.

**AC#18a: GetSaveStr returns empty string for uninitialized or out-of-bounds index**
- **Test**: Grep pattern=`GetSaveStr.*string\.Empty\|GetSaveStr.*""` path="Era.Core.Tests/"
- **Expected**: 1+ match confirming a unit test asserts string.Empty (or "") for uninitialized or out-of-bounds SaveStrIndex
- **Rationale**: Technical Design specifies GetSaveStr returns string.Empty for out-of-bounds indices. This is a core behavioral contract of IStringVariables derived from C11 (ERA semantic default for uninitialized SAVESTR slots). AC#14 verifies tests pass but does not mandate this specific boundary test exists. Note: tests for valid indices on a freshly-constructed VariableStore also confirm the zero-state contract (string.Empty for uninitialized slots), which is the default runtime state.

**AC#18b: GetTa returns failure for out-of-bounds index**
- **Test**: Grep pattern=`GetTa.*Fail\|GetTa.*out.*bound\|GetTa.*invalid` path="Era.Core.Tests/"
- **Expected**: 1+ match confirming a unit test asserts Result.Fail for out-of-bounds TA indices
- **Rationale**: Technical Design specifies GetTa returns Result.Fail for out-of-bounds indices [0-304, 0-304, 0-9]. This is a core behavioral contract of I3DArrayVariables. AC#14 verifies tests pass but does not mandate this specific boundary test exists.

**AC#18c: Unit test exercises TA maximum valid indices (304, 304, 9)**
- **Test**: Grep pattern=`GetTa.*304` path="Era.Core.Tests/"
- **Expected**: 1+ match confirming a unit test exercises GetTa with index 304 (the maximum valid value for TA_DIM1/TA_DIM2 = 305)
- **Rationale**: Technical Constraint C3 specifies TA dimensions [305,305,10]. AC#18b verifies some out-of-bounds test exists but does not mandate the exact boundary. This AC ensures a test exercises the maximum valid indices (304, 304, 9), confirming the backing array is correctly sized. Without this, an implementation with wrong dimensions (e.g., [300,300,10]) could still pass AC#18b if tests only check obviously-invalid indices.

**AC#18d: Unit test exercises SAVESTR maximum valid index (99)**
- **Test**: Grep pattern=`SaveStrIndex.*99\|GetSaveStr.*99` path="Era.Core.Tests/"
- **Expected**: 1+ match confirming a unit test exercises SaveStrIndex(99) or GetSaveStr with index 99 (the maximum valid value for SAVESTR_SIZE = 100)
- **Rationale**: Technical Constraint C2 specifies SAVESTR size 100. AC#18a verifies some string.Empty test exists but does not mandate the exact boundary. This AC ensures a test exercises the maximum valid index (99), confirming the backing array is correctly sized to 100 elements.

**AC#18e: Unit test exercises SetSaveStr null coalescing**
- **Test**: Grep pattern=`SetSaveStr.*null\|null.*SetSaveStr` path="Era.Core.Tests/"
- **Expected**: 1+ match confirming a unit test exercises SetSaveStr with null value input
- **Rationale**: IStringVariables interface documents "Null values are coalesced to empty string" as a behavioral contract (SetSaveStr doc comment). The implementation uses `value ?? string.Empty`. Without this AC, an implementation omitting null coalescing would pass AC#3b (signature check) but allow null values in the _saveStr array, causing NullReferenceException when Length or other string operations are performed on stored values.

**AC#18f: Unit test exercises SetSaveStr with out-of-bounds index (no exception)**
- **Test**: Grep pattern=`SetSaveStr.*-1\|SetSaveStr.*100\|SetSaveStr.*out.*bound` path="Era.Core.Tests/"
- **Expected**: 1+ match confirming a unit test exercises SetSaveStr with out-of-bounds SaveStrIndex (e.g., -1 or 100) and no exception is thrown
- **Rationale**: IStringVariables interface documents "Fire-and-forget pattern" for SetSaveStr. The implementation uses `if (index.Value < 0 || index.Value >= SAVESTR_SIZE) return;` to silently ignore out-of-bounds writes. Without this AC, an implementation omitting the bounds check would throw IndexOutOfRangeException, violating the fire-and-forget contract. AC#18a covers getter boundary but not setter boundary.

**AC#18g: Unit test exercises SetTa with out-of-bounds indices (no exception)**
- **Test**: Grep pattern=`SetTa.*305\|SetTa.*-1\|SetTa.*out.*bound` path="Era.Core.Tests/"
- **Expected**: 1+ match confirming a unit test exercises SetTa with out-of-bounds indices (e.g., 305 or -1) and no exception is thrown
- **Rationale**: I3DArrayVariables interface documents "Invalid indices silently ignored (fire-and-forget convention)" for SetTa. Without this AC, an implementation that throws on invalid setter indices would pass all other ACs. AC#18b covers getter out-of-bounds (Result.Fail) but not setter out-of-bounds (silent ignore). SetTb follows the same pattern and is covered by the same test infrastructure.

**AC#19a: HasCollectionUnlocked calls GetSaveStr**
- **Test**: Grep pattern=`GetSaveStr` path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: 1+ match confirming HasCollectionUnlocked() calls GetSaveStr (not a hardcoded return value)
- **Rationale**: AC#10 verifies NotImplementedException removal and AC#12 verifies IStringVariables injection, but neither confirms the stub was replaced with an actual interface call. This AC closes the gap: a hardcoded `return true` replacement would pass AC#10+AC#12 but fail AC#19a, ensuring the SSOT claim (variable access through the VariableStore system) is verified.

**AC#19b: CalculateSpermAmount calls GetTa**
- **Test**: Grep pattern=`GetTa` path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 1+ match confirming CalculateSpermAmount() calls GetTa (not a hardcoded return value)
- **Rationale**: AC#11 verifies NotImplementedException removal and AC#13 verifies I3DArrayVariables injection, but neither confirms the stub was replaced with an actual interface call. This AC closes the gap: a hardcoded `return 0` replacement would pass AC#11+AC#13 but fail AC#19b, ensuring the SSOT claim is verified.

**AC#19c: HasCollectionUnlocked uses STRLENS > 1 semantics**
- **Test**: Grep pattern=`\.Length > 1` path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: 1+ match confirming HasCollectionUnlocked() uses `.Length > 1` (ERB STRLENS > 1 semantics)
- **Rationale**: ERB source (SHOP.ERB:20,39) uses `STRLENS(SAVESTR:10) > 1`. AC#19a verifies GetSaveStr is called but does not verify the comparison semantics. An implementation using `.Length > 0` or `!= ""` would pass AC#19a but produce semantically different results. This AC ensures the exact ERB-equivalent behavior.

**AC#19d: CalculateSpermAmount uses correct loop upper bound (301)**
- **Test**: Grep pattern=`< 301` path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 1+ match confirming the loop iterates 0..300 (ERA FOR exclusive upper bound 301)
- **Rationale**: ERB source (SHOP2.ERB:98) uses `FOR LOOP_I, 0, 301` (301 exclusive upper bound = 301 iterations over 0..300). AC#19b verifies GetTa is called but does not verify the loop bounds. An implementation with wrong bounds (e.g., 0..304) would pass AC#19b but access incorrect TA indices.

**AC#19e: CalculateSpermAmount iterates 5 slots**
- **Test**: Grep pattern=`< 5\|slot < 5` path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 1+ match confirming the inner loop iterates 5 slots (0..4)
- **Rationale**: ERB source (SHOP2.ERB:99) uses `REPEAT 5` (5 iterations over slots 0..4). AC#19b verifies GetTa is called but does not verify the slot count. An implementation with wrong slot count would produce incorrect sperm totals.

**AC#19f: CalculateSpermAmount uses conditional accumulation (> 0)**
- **Test**: Grep pattern=`value > 0` path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 1+ match confirming conditional accumulation only when value > 0
- **Rationale**: ERB source (SHOP2.ERB:100) uses `IF TA:LOOP_CHR:LOOP_I:COUNT > 0` for conditional accumulation. AC#19b verifies GetTa is called but does not verify the conditional. An unconditional sum would include zero values (harmless arithmetically) but diverge from ERB semantics. Variable name `value` is constrained by Implementation Contract (Technical Design code snippet).

**AC#20: VariableStore summary comment lists 7 interfaces**
- **Test**: Grep pattern=`7 interfaces\|IStringVariables.*I3DArrayVariables` path="Era.Core/Variables/VariableStore.cs"
- **Expected**: 1+ match confirming the class summary comment has been updated to reflect all 7 interfaces (1 core IVariableStore + 6 ISP-segregated sub-interfaces)
- **Rationale**: Task#4 includes updating the stale VariableStore summary comment from "4 ISP-segregated interfaces" (actually 5) to list all 7 interfaces. Without this AC, the comment update sub-deliverable within Task#4 has no verification path, violating "ACs must comprehensively verify all Tasks."

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Create IStringVariables sub-interface for SAVESTR Get/Set | AC#1, AC#3, AC#3b, AC#5 |
| 2 | Create I3DArrayVariables sub-interface for TA/TB Get/Set | AC#2, AC#4, AC#4b, AC#4c, AC#4d |
| 3 | Backing storage in VariableStore | AC#6a, AC#6b, AC#14, AC#18a, AC#18b, AC#18c, AC#18d, AC#18e, AC#18f, AC#18g, AC#20 |
| 4 | DI registration following established pattern | AC#8, AC#9 |
| 5 | Replace HasCollectionUnlocked() stub with real IStringVariables call | AC#10, AC#12, AC#19a, AC#19c |
| 6 | Replace CalculateSpermAmount() stub with real I3DArrayVariables call | AC#11, AC#13, AC#19b, AC#19d, AC#19e, AC#19f |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

**ISP Segregation with Two New Interfaces**

Following the established pattern (ServiceCollectionExtensions.cs:86-92), create two separate interfaces that VariableStore will implement:

1. **IStringVariables**: Handles SAVESTR (1D global string array, size 100)
   - `string GetSaveStr(SaveStrIndex index)` - Returns string directly (strongly-typed index, static bounds per C11)
   - `void SetSaveStr(SaveStrIndex index, string value)` - Fire-and-forget pattern matching existing setters

2. **I3DArrayVariables**: Handles TA/TB (3D global integer arrays [305,305,10])
   - `Result<int> GetTa(int idx1, int idx2, int idx3)` - Returns Result<int> for runtime bounds checking (raw int indices)
   - `void SetTa(int idx1, int idx2, int idx3, int value)` - Fire-and-forget pattern matching existing setter convention (IVariableStore.cs:11-13)
   - Same pattern for TB

**Backing Storage**: VariableStore gains two new fields:
- `private readonly string[] _saveStr = new string[100];` - Initialized with empty strings
- `private readonly int[,,] _ta = new int[305, 305, 10];` - Dense 3D array
- `private readonly int[,,] _tb = new int[305, 305, 10];` - Dense 3D array

**DI Registration**: Add two singleton registrations following GetRequiredService<VariableStore>() forwarding pattern.

**Stub Replacement**:
- ShopSystem.HasCollectionUnlocked(): Inject IStringVariables, call `_stringVariables.GetSaveStr(new SaveStrIndex(10)).Length > 1` (matches ERB STRLENS semantics)
- ShopDisplay.CalculateSpermAmount(): Inject I3DArrayVariables, loop 0..300 (ERA FOR exclusive upper bound) x 5 slots, accumulate only `GetTa() > 0` values (matching ERB `IF TA > 0` conditional)

This approach satisfies all 38 ACs by creating the infrastructure (AC#1-9), replacing the stubs (AC#10-13), maintaining zero technical debt with passing tests (AC#14-16), updating SSOT documentation (AC#17a-17c), verifying behavioral contracts including setter boundary behavior (AC#18a-18g), confirming actual interface usage and ERB-equivalent semantics in stubs (AC#19a-19f), and verifying VariableStore summary comment update (AC#20).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create Era.Core/Interfaces/IStringVariables.cs file with GetSaveStr/SetSaveStr methods |
| 2 | Create Era.Core/Interfaces/I3DArrayVariables.cs file with GetTa/SetTa/GetTb/SetTb methods |
| 3 | IStringVariables declares `string GetSaveStr(SaveStrIndex index)` returning string directly (C11 strongly-typed index pattern) |
| 3b | IStringVariables declares `void SetSaveStr(SaveStrIndex index, string value)` |
| 4 | I3DArrayVariables declares `Result<int> GetTa(int idx1, int idx2, int idx3)` with 3 integer parameters |
| 4b | I3DArrayVariables declares `Result<int> GetTb(int idx1, int idx2, int idx3)` |
| 4c | I3DArrayVariables declares `void SetTa(int idx1, int idx2, int idx3, int value)` following fire-and-forget convention |
| 4d | I3DArrayVariables declares `void SetTb(int idx1, int idx2, int idx3, int value)` following fire-and-forget convention |
| 5 | Create Era.Core/Types/SaveStrIndex.cs following FlagIndex pattern (readonly record struct with Value property) |
| 6a | VariableStore class declaration adds IStringVariables to interface list |
| 6b | VariableStore class declaration adds I3DArrayVariables to interface list |
| 7 | IVariableStore.cs remains unchanged (Grep verifies no new methods added) |
| 8 | ServiceCollectionExtensions.cs adds `services.AddSingleton<IStringVariables>(sp => sp.GetRequiredService<VariableStore>());` |
| 9 | ServiceCollectionExtensions.cs adds `services.AddSingleton<I3DArrayVariables>(sp => sp.GetRequiredService<VariableStore>());` |
| 10 | ShopSystem.HasCollectionUnlocked() replaced with `_stringVariables.GetSaveStr(new SaveStrIndex(10)).Length > 1` (STRLENS semantics, no NotImplementedException) |
| 11 | ShopDisplay.CalculateSpermAmount() replaced with I3DArrayVariables.GetTa() loop (no NotImplementedException) |
| 12 | ShopSystem constructor gains `IStringVariables stringVariables` parameter and `_stringVariables` field |
| 13 | ShopDisplay constructor gains `I3DArrayVariables arrayVariables` parameter and `_arrayVariables` field |
| 14 | Unit tests for SaveStrIndex, IStringVariables/I3DArrayVariables implementations, DI wiring, stub replacements |
| 15 | All new files (IStringVariables.cs, I3DArrayVariables.cs, SaveStrIndex.cs) contain zero TODO/FIXME/HACK |
| 16 | dotnet build Era.Core succeeds with zero warnings (TreatWarningsAsErrors=true compliance) |
| 17a, 17b, 17c | engine-dev SKILL.md updated with SaveStrIndex, IStringVariables, I3DArrayVariables per ssot-update-rules.md |
| 18a | Unit test verifies GetSaveStr returns string.Empty for out-of-bounds SaveStrIndex |
| 18b | Unit test verifies GetTa returns Result.Fail for out-of-bounds indices |
| 18c | Unit test exercises TA maximum valid indices (304, 304, 9) confirming correct array dimensions |
| 18d | Unit test exercises SAVESTR maximum valid index (99) confirming correct array size |
| 18e | Unit test exercises SetSaveStr null coalescing (set null → get returns string.Empty) |
| 18f | Unit test exercises SetSaveStr with out-of-bounds index (fire-and-forget: no exception) |
| 18g | Unit test exercises SetTa with out-of-bounds indices (fire-and-forget: no exception) |
| 19a | ShopSystem.cs contains GetSaveStr call (confirms stub replaced with real interface call, not hardcoded value) |
| 19b | ShopDisplay.cs contains GetTa call (confirms stub replaced with real interface call, not hardcoded value) |
| 19c | ShopSystem.cs contains `.Length > 1` (confirms ERB STRLENS > 1 semantics) |
| 19d | ShopDisplay.cs contains loop upper bound 301 (confirms ERA FOR exclusive upper bound) |
| 19e | ShopDisplay.cs contains 5-slot iteration (confirms REPEAT 5 semantics) |
| 19f | ShopDisplay.cs contains conditional accumulation `> 0` (confirms ERA IF TA > 0 semantics) |
| 20 | VariableStore.cs summary comment updated to list all 7 interfaces (1 core + 6 ISP sub-interfaces) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| String getter return type | A) `Result<string>` (match 2D pattern), B) `string` (match FLAG 1D pattern) | B) `string` | C11: SAVESTR uses strongly-typed SaveStrIndex (like FLAG), making runtime bounds validation unnecessary. Return empty string for uninitialized slots. |
| 3D setter return type | A) `void` (match existing pattern), B) `Result<int>` (enable validation feedback) | A) `void` | IVariableStore.cs:11-13 explicitly documents setter convention: void (fire-and-forget), invalid indices silently ignored. All 19 existing setters follow this. TA consumers (CalculateSpermAmount) run in tight loops (301×5 iterations). Consistency over validation. |
| TA/TB storage structure | A) `int[,,]` dense array, B) `Dictionary<(int,int,int), int>` sparse | A) `int[,,]` dense | TA usage (GameOptions.cs:146-161, SHOP2.ERB:100) suggests dense access patterns. 305x305x10 = 930,250 integers (~3.6MB) is acceptable for singleton lifetime. Matches engine's existing allocation. |
| SaveStrIndex type | A) `int` (no wrapper), B) `readonly record struct` (FlagIndex pattern) | B) `readonly record struct` | C7 constraint requires strongly-typed index. Following FlagIndex pattern ensures compile-time type safety and justifies direct string return (C11). |
| Interface naming | A) `IStringVariableStore`, B) `IStringVariables` | B) `IStringVariables` | Matches existing ISP naming: ITrainingVariables, IJuelVariables, ITEquipVariables. Shorter, focuses on capability not implementation. |
| TB inclusion | A) TA only (no current consumer), B) Both TA and TB | B) Both TA and TB | VariableCode.cs:267-268 defines both with identical structure. Zero marginal cost to include TB in I3DArrayVariables. Avoids future interface extension when TB consumer arrives. |

### Interfaces / Data Structures

**IStringVariables Interface** (Era.Core/Interfaces/IStringVariables.cs):

```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces
{
    /// <summary>
    /// String variable storage interface (ISP segregation from IVariableStore).
    /// Handles SAVESTR (1D global string array, size 100).
    /// Feature 789 - Phase 20 string variable access.
    /// </summary>
    public interface IStringVariables
    {
        /// <summary>
        /// Get string value from SAVESTR array.
        /// Returns empty string for uninitialized slots.
        /// </summary>
        string GetSaveStr(SaveStrIndex index);

        /// <summary>
        /// Set string value in SAVESTR array.
        /// Fire-and-forget pattern (void return).
        /// Null values are coalesced to empty string.
        /// </summary>
        void SetSaveStr(SaveStrIndex index, string value);
    }
}
```

**I3DArrayVariables Interface** (Era.Core/Interfaces/I3DArrayVariables.cs):

```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces
{
    /// <summary>
    /// 3D integer array variable storage interface (ISP segregation from IVariableStore).
    /// Handles TA/TB (3D global integer arrays [305,305,10]).
    /// Feature 789 - Phase 20 relationship matrix tracking.
    /// </summary>
    public interface I3DArrayVariables
    {
        /// <summary>
        /// Get integer value from TA 3D array.
        /// Returns Result.Fail if any index is out of bounds [0-304, 0-304, 0-9].
        /// </summary>
        Result<int> GetTa(int idx1, int idx2, int idx3);

        /// <summary>
        /// Set integer value in TA 3D array.
        /// Invalid indices silently ignored (fire-and-forget convention).
        /// </summary>
        void SetTa(int idx1, int idx2, int idx3, int value);

        /// <summary>
        /// Get integer value from TB 3D array.
        /// Returns Result.Fail if any index is out of bounds [0-304, 0-304, 0-9].
        /// </summary>
        Result<int> GetTb(int idx1, int idx2, int idx3);

        /// <summary>
        /// Set integer value in TB 3D array.
        /// Invalid indices silently ignored (fire-and-forget convention).
        /// </summary>
        void SetTb(int idx1, int idx2, int idx3, int value);
    }
}
```

**SaveStrIndex Type** (Era.Core/Types/SaveStrIndex.cs):

```csharp
namespace Era.Core.Types
{
    /// <summary>
    /// Strongly typed index for SAVESTR array (1D global string array, size 100).
    /// Prevents mixing SAVESTR indices with other integer types.
    /// Feature 789 - Phase 20 string variable access.
    /// </summary>
    public readonly record struct SaveStrIndex
    {
        /// <summary>
        /// Maximum valid index value (SAVESTR array size 100, zero-indexed).
        /// </summary>
        public const int MaxValue = 99;

        public int Value { get; }

        public SaveStrIndex(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Implicit conversion to int for backward compatibility.
        /// </summary>
        public static implicit operator int(SaveStrIndex index) => index.Value;

        /// <summary>
        /// Explicit conversion from int to SaveStrIndex.
        /// Explicit to encourage type safety and prevent accidental conversions.
        /// </summary>
        public static explicit operator SaveStrIndex(int value) => new(value);
    }
}
```

**VariableStore Storage Fields** (additions to Era.Core/Variables/VariableStore.cs):

```csharp
// String variable storage (SAVESTR: 1D global, size 100)
private readonly string[] _saveStr = new string[100];

// 3D integer array storage (TA/TB: global 3D, [305,305,10])
private readonly int[,,] _ta = new int[305, 305, 10];
private readonly int[,,] _tb = new int[305, 305, 10];
```

**Constructor Initialization**:
```csharp
// In VariableStore constructor:
for (int i = 0; i < 100; i++)
{
    _saveStr[i] = string.Empty;
}
// TA/TB arrays zero-initialized by default
```

**Implementation Pattern** (GetSaveStr example):
```csharp
public string GetSaveStr(SaveStrIndex index)
{
    if (index.Value < 0 || index.Value >= 100)
        return string.Empty; // Out of bounds returns empty string
    return _saveStr[index.Value];
}
```

**Implementation Pattern** (GetTa example):
```csharp
public Result<int> GetTa(int idx1, int idx2, int idx3)
{
    if (idx1 < 0 || idx1 >= TA_DIM1)
        return Result<int>.Fail($"TA index 1 out of bounds: {idx1} (valid: 0-{TA_DIM1 - 1})");
    if (idx2 < 0 || idx2 >= TA_DIM2)
        return Result<int>.Fail($"TA index 2 out of bounds: {idx2} (valid: 0-{TA_DIM2 - 1})");
    if (idx3 < 0 || idx3 >= TA_DIM3)
        return Result<int>.Fail($"TA index 3 out of bounds: {idx3} (valid: 0-{TA_DIM3 - 1})");
    return Result<int>.Ok(_ta[idx1, idx2, idx3]);
}
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| (none) | - | - |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 5 | Create SaveStrIndex strongly-typed index following FlagIndex pattern | | [x] |
| 2 | 1,3,3b | Create IStringVariables interface with GetSaveStr/SetSaveStr methods | | [x] |
| 3 | 2,4,4b,4c,4d | Create I3DArrayVariables interface with GetTa/SetTa/GetTb/SetTb methods | | [x] |
| 4 | 6a,6b,14,18a,18b,18c,18d,18e,18f,18g,20 | Implement IStringVariables and I3DArrayVariables in VariableStore with backing storage (including constructor initialization: _saveStr to string.Empty; null coalescing in SetSaveStr; setter out-of-bounds silent ignore; update class summary comment from current stale "4 ISP-segregated interfaces" (actually 5: F412 ITEquipVariables was not reflected) to list all 7 interfaces) | | [x] |
| 5 | 8,9 | Register IStringVariables and I3DArrayVariables in DI following ISP pattern | | [x] |
| 6 | 10,12,19a,19c | Replace HasCollectionUnlocked stub with IStringVariables call in ShopSystem (STRLENS > 1 semantics: .Length > 1) | | [x] |
| 7 | 11,13,19b,19d,19e,19f | Replace CalculateSpermAmount stub with I3DArrayVariables call in ShopDisplay (ERB-equivalent: loop 0..300, 5 slots, conditional > 0) | | [x] |
| 8 | 7 | Verify IVariableStore.cs unchanged (ISP segregation maintained) | | [x] |
| 9 | 15 | Verify zero technical debt in new interface and type files | | [x] |
| 10 | 16 | Verify build succeeds with zero warnings | | [x] |
| 11 | 17a,17b,17c | Update engine-dev SKILL.md with SaveStrIndex, IStringVariables, I3DArrayVariables per ssot-update-rules.md | | [x] |

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
| 1 | tester | sonnet | AC#1-20 (38 ACs) | TDD test suite (RED state) |
| 2 | implementer | sonnet | Tasks #1-11 | Implementation + passing tests (GREEN state) |

### Pre-conditions

- F647 [DONE] - Phase 20 Planning complete
- F774 [DONE] - Shop Core with NotImplementedException stubs at ShopSystem.cs:410-411 and ShopDisplay.cs:385-388

### Execution Order

1. **Phase 1 (TDD RED)**: tester creates unit tests for all ACs (except AC#7, AC#8, AC#9, AC#10, AC#11, AC#15, AC#17a, AC#17b, AC#17c, AC#19a, AC#19b, AC#19c, AC#19d, AC#19e, AC#19f, AC#20 which are code/file-level Grep verifications, not unit-testable)
2. **Phase 2 (Implementation)**:
   - Task 1: Create SaveStrIndex.cs
   - Task 2: Create IStringVariables.cs
   - Task 3: Create I3DArrayVariables.cs
   - Task 4: Extend VariableStore to implement both new interfaces
   - Task 5: Add DI registrations
   - Task 6: Modify ShopSystem constructor and HasCollectionUnlocked
   - Task 7: Modify ShopDisplay constructor and CalculateSpermAmount
   - Task 8-10: Verification tasks (run during Phase 2)
   - Task 11: Update engine-dev SKILL.md with new types/interfaces

### Interface Signatures

**IStringVariables** (Era.Core/Interfaces/IStringVariables.cs):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces
{
    /// <summary>
    /// String variable storage interface (ISP segregation from IVariableStore).
    /// Handles SAVESTR (1D global string array, size 100).
    /// Feature 789 - Phase 20 string variable access.
    /// </summary>
    public interface IStringVariables
    {
        /// <summary>
        /// Get string value from SAVESTR array.
        /// Returns empty string for uninitialized slots.
        /// </summary>
        string GetSaveStr(SaveStrIndex index);

        /// <summary>
        /// Set string value in SAVESTR array.
        /// Fire-and-forget pattern (void return).
        /// Null values are coalesced to empty string.
        /// </summary>
        void SetSaveStr(SaveStrIndex index, string value);
    }
}
```

**I3DArrayVariables** (Era.Core/Interfaces/I3DArrayVariables.cs):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces
{
    /// <summary>
    /// 3D integer array variable storage interface (ISP segregation from IVariableStore).
    /// Handles TA/TB (3D global integer arrays [305,305,10]).
    /// Feature 789 - Phase 20 relationship matrix tracking.
    /// </summary>
    public interface I3DArrayVariables
    {
        /// <summary>
        /// Get integer value from TA 3D array.
        /// Returns Result.Fail if any index is out of bounds [0-304, 0-304, 0-9].
        /// </summary>
        Result<int> GetTa(int idx1, int idx2, int idx3);

        /// <summary>
        /// Set integer value in TA 3D array.
        /// Invalid indices silently ignored (fire-and-forget convention).
        /// </summary>
        void SetTa(int idx1, int idx2, int idx3, int value);

        /// <summary>
        /// Get integer value from TB 3D array.
        /// Returns Result.Fail if any index is out of bounds [0-304, 0-304, 0-9].
        /// </summary>
        Result<int> GetTb(int idx1, int idx2, int idx3);

        /// <summary>
        /// Set integer value in TB 3D array.
        /// Invalid indices silently ignored (fire-and-forget convention).
        /// </summary>
        void SetTb(int idx1, int idx2, int idx3, int value);
    }
}
```

**SaveStrIndex** (Era.Core/Types/SaveStrIndex.cs):
```csharp
namespace Era.Core.Types
{
    /// <summary>
    /// Strongly typed index for SAVESTR array (1D global string array, size 100).
    /// Prevents mixing SAVESTR indices with other integer types.
    /// Feature 789 - Phase 20 string variable access.
    /// </summary>
    public readonly record struct SaveStrIndex
    {
        /// <summary>
        /// Maximum valid index value (SAVESTR array size 100, zero-indexed).
        /// </summary>
        public const int MaxValue = 99;

        public int Value { get; }

        public SaveStrIndex(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Implicit conversion to int for backward compatibility.
        /// </summary>
        public static implicit operator int(SaveStrIndex index) => index.Value;

        /// <summary>
        /// Explicit conversion from int to SaveStrIndex.
        /// Explicit to encourage type safety and prevent accidental conversions.
        /// </summary>
        public static explicit operator SaveStrIndex(int value) => new(value);
    }
}
```

### DI Registration

Add to `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` following the existing ISP pattern at lines 86-92:

```csharp
// String variable access (SAVESTR) - ISP segregation (F789)
services.AddSingleton<IStringVariables>(sp => sp.GetRequiredService<VariableStore>());

// 3D array variable access (TA/TB) - ISP segregation (F789)
services.AddSingleton<I3DArrayVariables>(sp => sp.GetRequiredService<VariableStore>());
```

### VariableStore Backing Storage

Add to `Era.Core/Variables/VariableStore.cs`:

```csharp
// Named constants following existing pattern (FLAG_SIZE, TFLAG_SIZE, PALAMLV_SIZE) - F789
private const int SAVESTR_SIZE = 100;
private const int TA_DIM1 = 305;
private const int TA_DIM2 = 305;
private const int TA_DIM3 = 10;

// String variable storage (SAVESTR: 1D global, size 100) - F789
private readonly string[] _saveStr = new string[SAVESTR_SIZE];

// 3D integer array storage (TA/TB: global 3D, [305,305,10]) - F789
private readonly int[,,] _ta = new int[TA_DIM1, TA_DIM2, TA_DIM3];
private readonly int[,,] _tb = new int[TA_DIM1, TA_DIM2, TA_DIM3];
```

Constructor initialization:
```csharp
// In VariableStore constructor - F789
for (int i = 0; i < SAVESTR_SIZE; i++)
{
    _saveStr[i] = string.Empty;
}
// TA/TB arrays zero-initialized by default
```

Implementation example (GetSaveStr):
```csharp
public string GetSaveStr(SaveStrIndex index)
{
    if (index.Value < 0 || index.Value >= SAVESTR_SIZE)
        return string.Empty; // Out of bounds returns empty string
    return _saveStr[index.Value];
}
```

Implementation example (SetSaveStr):
```csharp
public void SetSaveStr(SaveStrIndex index, string value)
{
    if (index.Value < 0 || index.Value >= SAVESTR_SIZE)
        return; // Fire-and-forget: out of bounds silently ignored
    _saveStr[index.Value] = value ?? string.Empty; // Null coalesced to empty string
}
```

Implementation example (GetTa):
```csharp
public Result<int> GetTa(int idx1, int idx2, int idx3)
{
    if (idx1 < 0 || idx1 >= TA_DIM1)
        return Result<int>.Fail($"TA index 1 out of bounds: {idx1} (valid: 0-{TA_DIM1 - 1})");
    if (idx2 < 0 || idx2 >= TA_DIM2)
        return Result<int>.Fail($"TA index 2 out of bounds: {idx2} (valid: 0-{TA_DIM2 - 1})");
    if (idx3 < 0 || idx3 >= TA_DIM3)
        return Result<int>.Fail($"TA index 3 out of bounds: {idx3} (valid: 0-{TA_DIM3 - 1})");
    return Result<int>.Ok(_ta[idx1, idx2, idx3]);
}
```

### Stub Replacement

**ShopSystem.HasCollectionUnlocked()** (Era.Core/Shop/ShopSystem.cs:410-411):
- Add constructor parameter: `IStringVariables stringVariables`
- Add field: `private readonly IStringVariables _stringVariables;`
- Replace NotImplementedException with:
```csharp
private bool HasCollectionUnlocked()
{
    return _stringVariables.GetSaveStr(new SaveStrIndex(10)).Length > 1;
}
```

**ShopDisplay.CalculateSpermAmount()** (Era.Core/Shop/ShopDisplay.cs:385-388):
- Add constructor parameter: `I3DArrayVariables arrayVariables`
- Add field: `private readonly I3DArrayVariables _arrayVariables;`
- ERB source: SHOP2.ERB:98-103 (`FOR LOOP_I, 0, 301` → 301 iterations (ERA FOR exclusive upper bound); `REPEAT 5` → 5 slots; `IF TA:LOOP_CHR:LOOP_I:COUNT > 0` conditional accumulation)
- Replace NotImplementedException with:
```csharp
private int CalculateSpermAmount(int chr)
{
    // Loop bounds from SHOP2.ERB:98-103: FOR LOOP_I, 0, 301 (301 iterations, exclusive upper bound), REPEAT 5 (5 slots)
    // ERA FOR uses exclusive upper bound (LoopEnd > counter), so 0..300 inclusive
    int total = 0;
    for (int i = 0; i < 301; i++)
    {
        for (int slot = 0; slot < 5; slot++)
        {
            int value = _arrayVariables.GetTa(chr, i, slot).Match(v => v, _ => 0);
            if (value > 0) total += value;
        }
    }
    return total;
}
```

### Build Verification

1. Run `dotnet build Era.Core` - Must succeed with zero warnings
2. Run `dotnet test Era.Core.Tests` - All tests must pass
3. Verify IVariableStore.cs unchanged (Grep verification in Task 8)

### Success Criteria

- All 38 ACs pass
- Zero TODO/FIXME/HACK in new files
- Build succeeds with zero warnings
- All unit tests pass

### Error Handling

If any step fails:
1. **STOP** - Do not proceed to next step
2. Document failure in Execution Log
3. Report to user with specific error details
4. Wait for user guidance before retry

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| GameOptions.cs TA delegate workaround (Func<int,int,int,int> getTa at lines 146-161) bypasses I3DArrayVariables | Pre-existing workaround; Philosophy states VariableStore is SSOT for ERA variable access; delegate contradicts ISP architecture | Feature | F782 | N/A (Option B: existing feature) |
| AC#7: `GetTa` pattern matches `GetTalent` (false positive in not_matches) | AC定義精度問題: サブストリングマッチで偽陽性。パターンを `GetTa[^l]` 等に修正必要 | Feature | F789 | F792 Phase 9 handoff |
| VariableStore lacks Reset/Clear method for new mutable state (_saveStr ~400B, _ta/_tb ~7.2MB) | Pre-existing limitation inherited by F789; game load/reset requires holistic state clearing across all ISP interfaces | Feature | F782 | N/A (Option B: existing feature) |

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
| 2026-02-15 | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: CalculateSpermAmount used type cast instead of .Match() pattern. Fixed. |
| 2026-02-15 | DEVIATION | feature-reviewer | Phase 8.2 doc-check | NEEDS_REVISION: engine-dev SKILL.md comment said "5 interfaces" instead of "7 interfaces". Fixed. |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-applied] Phase2-Uncertain iter1: [FMT-004] Tasks table missing Task Tags subsection per template (Task Tags with [I] tag explanation)
- [resolved-applied] Phase2-Uncertain iter1: [FMT-004] Execution Log section minor formatting (fixed by removing spurious --- separators in iter2)
- [resolved-applied] Phase2-Pending iter1: [CON-002] HasCollectionUnlocked implementation in Implementation Contract diverges from ERB semantics — proposes `HasCollectionUnlocked(int collectionId)` with Contains logic, but ERB (SHOP.ERB:20,39) uses `STRLENS(SAVESTR:10) > 1` (parameterless, length check) and ShopSystem.cs:408 declares parameterless. Fix: change to `private bool HasCollectionUnlocked() { return _stringVariables.GetSaveStr(new SaveStrIndex(10)).Length > 1; }`
- [resolved-skipped] Phase2-Pending iter1: [AC-003] Sub-Feature Requirements mandates 等価性検証 but stubs throw NotImplementedException (no legacy C# behavior to compare). ERB equivalence is specified in Implementation Contract (STRLENS > 1, loop 0..300, conditional accumulation). Phase 1 TDD tests verify ERB-equivalent behavior via AC#14. Separate equivalence ACs would duplicate unit test coverage.
- [resolved-applied] Phase2-Pending iter1: [AC-002] AC#6 matcher `IStringVariables,` (trailing comma) is fragile and ordering-dependent; does not verify I3DArrayVariables. Split into two ACs or use robust matcher
- [fix] Phase2-Review iter2: Tasks section | Added Task Tags subsection per template
- [fix] Phase2-Review iter2: line 19 | Removed non-template ## Created field
- [fix] Phase2-Review iter2: Implementation Contract Stub Replacement | Fixed HasCollectionUnlocked to parameterless with .Length > 1 (STRLENS semantics)
- [fix] Phase2-Review iter2: AC#6 | Split into AC#6a (IStringVariables) and AC#6b (I3DArrayVariables) for robust verification
- [fix] Phase2-Review iter3: Sections | Removed spurious --- separators inside Tasks, Implementation Contract, Mandatory Handoffs, Execution Log sections
- [fix] Phase2-Review iter3: Mandatory Handoffs | Added template comments (CRITICAL, Option A/B/C)
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#3b (SetSaveStr), AC#4b (GetTb), AC#4c (SetTa), AC#4d (SetTb) matchers
- [fix] Phase2-Review iter3: Implementation Contract | Fixed CalculateSpermAmount visibility from public to private
- [resolved-skipped] Phase2-Pending iter3: [FMT-004] Summary and Scope Reference were populated by consensus-synthesizer in Phase 1 and provide useful structured context. Template permits additional sections. Merging into Background/Technical Constraints would reduce readability without benefit.
- [resolved-applied] Phase2-Uncertain iter3: [CON-003] C11 rationale updated: Result<T> wrapper unnecessary (strongly-typed index), but defensive bounds checking returning string.Empty is a separate concern (ERA semantic default for uninitialized strings). Distinct from FLAG's unchecked access because string.Empty is a meaningful safe fallback.
- [fix] Phase2-Review iter4: AC#15 | Expanded Grep scope from 3 new files to all 7 F789-touched files (added VariableStore.cs, ShopSystem.cs, ShopDisplay.cs, ServiceCollectionExtensions.cs)
- [fix] Phase2-Review iter5: Tasks + AC Definition Table | Added Task#11 and AC#17 for engine-dev SKILL.md update per ssot-update-rules.md
- [fix] Phase2-Review iter5: Implementation Contract CalculateSpermAmount | Added ERB source evidence (SHOP2.ERB:98-103) for loop bounds 302 and 5
- [fix] Phase2-Review iter6: Implementation Contract Phase table | Updated inputs AC#1-16→AC#1-17, Tasks #1-10→#1-11
- [fix] Phase2-Review iter6: Section separators | Added missing --- before ## Execution Log and ## Review Notes per template
- [resolved-applied] Phase2-Pending iter6: [CON-004] SetTa/SetTb changed to void. IVariableStore.cs:11-13 setter convention (void, fire-and-forget) applies — all 19 existing setters follow this. TA consumers run in tight loops (301×5 iterations). Invalid indices silently ignored.
- [resolved-applied] Phase2-Uncertain iter6: [DOC-001] Philosophy updated: 'IVariableStore' → 'The VariableStore system (IVariableStore and its ISP-segregated sub-interfaces)' to match ISP architecture where VariableStore implements multiple separate interfaces.
- [fix] Phase2-Review iter7: Implementation Contract CalculateSpermAmount | Fixed FOR loop from 302→301 iterations (ERA FOR exclusive upper bound: LoopEnd > counter), added > 0 conditional per SHOP2.ERB:100
- [fix] Phase2-Review iter7: Technical Design Approach | Updated CalculateSpermAmount description with correct loop range and conditional
- [fix] Phase2-Review iter7: Risks table | Fixed 302→301 entries
- [fix] Phase2-Review iter7: Dependencies section | Added Dependency Types SSOT comment block per template
- [fix] Phase2-Review iter7: Mandatory Handoffs section | Added Validation and DRAFT Creation Checklist comment blocks per template
- [fix] Phase2-Review iter8: AC#10 | Fixed vacuous matcher from `HasCollectionUnlocked.*NotImplementedException` (cross-line, never matches) to `NotImplementedException.*SAVESTR` (matches throw line 405)
- [fix] Phase2-Review iter8: Philosophy Derivation row 4 | Added AC#16 to 'avoid breaking existing implementations' coverage
- [fix] Phase2-Review iter9: Tasks AC# column | Task#2 added 3b, Task#3 added 4b,4c,4d
- [fix] Phase2-Review iter9: Tasks section | Added AC Coverage Rule template comment
- [fix] Phase2-Review iter9: Upstream Issues | Changed prose to template table format
- [fix] Phase2-Review iter9: AC#17 | Split single-line matcher into AC#17a/17b/17c for individual name verification
- [fix] Phase2-Review iter9: Goal Coverage | Added AC#5 to Goal item 1
- [resolved-skipped] Phase2-Pending iter9: [PHI-001] Pipeline Continuity is an organizational practice (phase completion → next phase planning), not a testable feature behavior. Derivation table correctly covers only implementable/verifiable claims. Adding AC for F775's dependency list would verify a different feature's documentation, not F789's implementation.
- [fix] Phase3-Maintainability iter7: Task#4 | Added constructor initialization detail (_saveStr elements to string.Empty)
- [fix] Phase3-Maintainability iter7: Implementation Contract backing storage | Replaced magic numbers with named constants (SAVESTR_SIZE, TA_DIM1/2/3) following existing FLAG_SIZE/TFLAG_SIZE/PALAMLV_SIZE pattern
- [resolved-skipped] Phase3-Maintainability iter7: [EXT-001] IStringVariables follows established ISP naming convention (ITrainingVariables, IJuelVariables group by capability, not by specific variable). Future 1D global string variables (STR, TSTR) naturally fit IStringVariables since they share the same access pattern. I3DArrayVariables similarly groups all 3D integer arrays. YAGNI: don't redesign for hypothetical requirements.
- [fix] Phase3-Maintainability iter8: Implementation Contract CalculateSpermAmount | Changed `result is Result<int>.Success s` to `.Match(v => v, _ => 0)` matching ShopDisplay.cs existing pattern
- [fix] Phase3-Maintainability iter8: Task#4 + Implementation Contract | Added VariableStore summary comment update (list all 7 interfaces) to prevent stale documentation
- [fix] Phase2-Review iter1: AC Coverage AC#4c/4d | Changed stale `Result<int> SetTa/SetTb with bounds validation` to `void SetTa/SetTb following fire-and-forget convention`
- [fix] Phase2-Review iter1: AC Coverage AC#17 | Changed `AC#17` to `AC#17a, 17b, 17c` matching split AC Definition Table entries
- [fix] Phase2-Review iter1: Philosophy Derivation row 4 | Added AC#14 alongside AC#7, AC#16 for non-breakage verification
- [fix] Phase2-Review iter1: Philosophy Derivation | Added Pipeline Continuity row with N/A coverage (resolved-skipped PHI-001)
- [fix] Phase2-Review iter1: AC#15 Description | Clarified 'all F789-touched files' to 'all F789-touched source files'
- [resolved-skipped] Phase2-Uncertain iter2: [FMT-004] Execution Log table body is empty with no placeholder row; Mandatory Handoffs uses (none). Template does not mandate (none) for Execution Log. Re-evaluated: non-critical, non-loop, non-decidable + uncertain = skip per improved PHASE-2.md Step 2.6.
- [resolved-applied] Phase2-Uncertain iter2: [AC-002] No AC verifies behavioral contracts: GetSaveStr returns string.Empty for out-of-bounds, GetTa returns Result.Fail for out-of-bounds. Re-evaluated: AC row addition is orchestrator-decidable per improved PHASE-2.md Step 2.6. Added AC#18a/18b.
- [fix] PostLoop-Uncertain iter3: AC Definition Table + Details | Added AC#18a (GetSaveStr out-of-bounds returns string.Empty) and AC#18b (GetTa out-of-bounds returns Result.Fail) for behavioral contract verification
- [fix] PostLoop-Uncertain iter3: Tasks, Goal Coverage, Philosophy Derivation, AC Coverage, Implementation Contract | Updated for AC#18a/18b coverage
- [fix] Phase3-Maintainability iter2: Technical Constraints | Added VariableStore Reset/Clear gap as pre-existing limitation (new mutable state inherits gap)
- [fix] Phase3-Maintainability iter2: SaveStrIndex (both occurrences) | Added MaxValue=99 constant for self-documenting bounds
- [fix] Phase2-Uncertain iter1: Mandatory Handoffs | Added GameOptions.cs TA delegate workaround entry with destination F782 (Option B)
- [fix] Phase3-Maintainability iter1: Task#4 description | Corrected stale interface count reference from implicit "4→7" to explicit "5→7" noting F412 ITEquipVariables was not reflected in existing comment
- [fix] Phase3-Maintainability iter1: AC#18a Details | Added zero-state contract note (fresh VariableStore returns string.Empty for valid indices)
- [fix] Phase3-Maintainability iter1: AC#5 Details | Added SaveStrIndex MaxValue=99 divergence documentation (intentional enhancement from FlagIndex pattern)
- [fix] Phase2-Review iter2: Line references | Updated all ShopSystem.cs:404-405 to ShopSystem.cs:408-409 and ShopDisplay.cs:381-384 to ShopDisplay.cs:385-388 (stale line numbers corrected against actual source)
- [fix] Phase2-Review iter10: Line references | Updated ShopSystem.cs:408-409 to ShopSystem.cs:408-409 (no change, F788 committed, actual lines now 408-409) and ShopDisplay.cs:385-388 (no change)
- [fix] Phase2-Review iter2: Implementation Contract CalculateSpermAmount | Preserved existing parameter name `int chr` instead of undocumented rename to `int characterId`
- [fix] Phase2-Review iter2: AC#18a | Broadened description from "out-of-bounds index" to "uninitialized or out-of-bounds index" for matcher-description consistency
- [fix] Phase2-Review iter3: AC Definition Table + Details | Added AC#18c (TA boundary test at max valid indices 304,304,9) and AC#18d (SAVESTR boundary test at max valid index 99) for dimension enforcement per C2/C3
- [fix] Phase2-Review iter3: Goal Coverage, AC Coverage, Tasks, Philosophy Derivation | Updated for AC#18c/18d coverage
- [fix] Phase2-Review iter4: Technical Design Approach | Updated stale "16 ACs" to "28 ACs" with AC#17/18 groups
- [fix] Phase2-Review iter4: Success Criteria | Updated stale "16 ACs" to "28 ACs"
- [fix] Phase2-Review iter4: AC Definition Table | Reordered AC#3b after AC#3 (before AC#4) for sub-item grouping
- [fix] Phase2-Review iter5: Line references | Reverted ShopSystem.cs:408-409 → 404-405 and ShopDisplay.cs:385-388 → 381-384 (committed HEAD values; iter2 incorrectly used uncommitted F788 working tree) [REVERSED in iter10: F788 has since been committed, so correct values are 408-409 and 385-388]
- [fix] Phase2-Review iter5: AC Definition Table + Details | Added AC#19a (GetSaveStr call in ShopSystem.cs) and AC#19b (GetTa call in ShopDisplay.cs) to verify actual interface usage in stub replacements
- [fix] Phase2-Review iter5: Goal Coverage, AC Coverage, Tasks, Approach, Success Criteria | Updated for AC#19a/19b (30 ACs total)
- [fix] Phase2-Review iter5: AC#10 Details | Fixed stale "line 409" to "line 405" (committed HEAD)
- [fix] Phase2-Review iter6: Line references | Re-updated ShopSystem.cs:404-405 → 408-409 (F788 commit cfc63aa7 now committed); reverted ShopDisplay.cs:385-388 → 381-384 (CalculateSpermAmount confirmed at 381-384, F788 does not shift these lines)
- [fix] Phase2-Review iter6: AC#10 Details | Fixed "line 405" back to "line 409" (F788 committed, HasCollectionUnlocked throw at line 409)
- [fix] Phase2-Review iter6: AC Design Constraints C3 | Clarified "non-character-scoped" to "globally-stored (character-to-character relationship matrix, not stored per-character in CharacterVariables)"
- [fix] Phase2-Review iter7: AC#11 | Simplified matcher from `CalculateSpermAmount.*NotImplementedException\|NotImplementedException.*TA 3D array` to `NotImplementedException.*TA 3D array` (removed dead cross-line branch)
- [fix] Phase2-Review iter8: Philosophy Derivation row 1 | Added AC#19a, AC#19b (consumer-side SSOT verification)
- [fix] Phase2-Review iter8: Philosophy Derivation row 2 | Added AC#8, AC#9 (DI exposure verification)
- [fix] Phase2-Review iter8: AC Definition Table + Details | Added AC#20 (VariableStore summary comment lists 7 interfaces) for Task#4 comment update verification
- [fix] Phase2-Review iter8: Goal Coverage, AC Coverage, Tasks, Approach, Success Criteria | Updated for AC#20 (31 ACs total)
- [fix] Phase2-Review iter9: Implementation Contract Phase table | Updated input from "AC#1-18" to "AC#1-20 (31 ACs)"
- [fix] Phase2-Review iter9: Execution Order exception list | Expanded from AC#7-11 to include AC#15, AC#17a-c, AC#19a-b, AC#20 (code/file Grep verifications)
- [fix] Phase2-Review iter9: Philosophy Derivation row 3 | Added AC#3, AC#3b, AC#4, AC#4b, AC#4c, AC#4d for interface cohesion verification
- [fix] Phase2-Review iter10: SetSaveStr null coalescing | Added "Null values are coalesced to empty string" to interface doc + SetSaveStr implementation example with `value ?? string.Empty`
- [info] Phase1-DriftChecked: F788 (Related)
- [fix] Phase1-DriftSync iter1: Dependencies/Related Features | Synced F788 [WIP]→[DONE], F790 [PROPOSED]→[WIP]
- [fix] Phase1-DriftFix iter1: Line references | ShopSystem.cs:408-409→410-411, ShopDisplay.cs:381-384→385-388 (F788 shifted lines by adding delegate methods)
- [fix] Phase1-DriftFix iter1: Baseline Measurement | NotImplementedException total 37→32 (F788 removed 5 IConsoleOutput stubs)
- [fix] Phase1-DriftFix iter1: Scope Reference | ShopSystem.cs ~405→~411, ShopDisplay.cs ~384→~389
- [fix] Phase2-Review iter1: AC Design Constraints | Added missing Constraint Details for C6, C8, C9, C10, C12, C13 (template completeness)
- [fix] Phase4-ACValidation iter2: AC#18a-18d Type | Changed from "test" to "code" (Grep-based matchers use code type, not test type)
- [fix] Phase2-Review iter1: AC section | Removed redundant --- separator between AC header and Philosophy Derivation (template compliance)
- [fix] Phase2-Review iter1: AC Definition Table + Details | Added AC#18e (SetSaveStr null coalescing test), AC#19c (STRLENS > 1 semantics), AC#19d (loop upper bound 301), AC#19e (5-slot iteration), AC#19f (conditional accumulation > 0) for ERB behavioral contract verification
- [fix] Phase2-Review iter1: Philosophy Derivation row 1 | Extended AC Coverage with AC#18e, AC#19c-19f for behavioral contracts
- [fix] Phase2-Review iter1: Goal Coverage | Updated Goal 3 (+AC#18e), Goal 5 (+AC#19c), Goal 6 (+AC#19d,19e,19f)
- [fix] Phase2-Review iter1: Tasks | Updated Task#4 (+AC#18e), Task#6 (+AC#19c), Task#7 (+AC#19d,19e,19f)
- [fix] Phase2-Review iter1: Technical Design + Implementation Contract | Updated AC counts 31→36
- [fix] Phase2-Review iter2: Mandatory Handoffs | Clarified Creation Task from '-' to 'N/A (Option B: existing feature)' for F782 handoff
- [fix] Phase2-Review iter2: AC#19d | Narrowed matcher from `< 301\|301` to `< 301` (removed overly broad branch)
- [fix] Phase2-Uncertain iter2: AC#19f | Narrowed matcher from `value > 0\|> 0\)` to `value > 0` (removed overly broad branch; variable name constrained by Implementation Contract)
- [fix] Phase2-Review iter3: AC Definition Table + Details | Added AC#18f (SetSaveStr out-of-bounds no exception) and AC#18g (SetTa out-of-bounds no exception) for setter fire-and-forget contract verification
- [fix] Phase2-Uncertain iter3: Philosophy Derivation row 4 | Removed AC#16 (build success verifies compilation, not behavioral non-breakage; AC#7 and AC#14 already cover)
- [fix] Phase2-Review iter3: Philosophy Derivation row 1, Goal Coverage, AC Coverage, Tasks, Approach, Success Criteria, Implementation Contract | Updated for AC#18f/18g (38 ACs total)
- [fix] Phase2-Uncertain iter4: Risks table | Fixed SAVESTR return type rationale: 'makes runtime bounds checking unnecessary' → 'provides type-safety against index mixing (not bounds safety). String return justified by string.Empty non-error semantic default'
- [fix] Phase2-Review iter4: AC#20 | Changed '7 ISP-segregated' to '7 interfaces' (IVariableStore is the original, not ISP-segregated; 6 are ISP sub-interfaces)
- [fix] Phase3-Maintainability iter5: Mandatory Handoffs | Added VariableStore Reset/Clear gap entry with destination F782 (pre-existing limitation, concrete tracking per Leak Prevention checklist)
- [info] Phase1-DriftChecked: F790 (Related)
- [fix] Phase1-DriftSync iter1: Dependencies/Related Features | Synced F790 [WIP]→[DONE]
- [fix] Phase1-DriftFix iter1: Baseline Measurement | NotImplementedException total 32→16 (F790 resolved 16 engine/CSV stubs in Shop/)
- [fix] Phase1-DriftFix iter1: Scope Reference | ShopSystem.cs ~411→~455, ShopDisplay.cs ~389→~485 (F790 expanded with IEngineVariables/ICsvNameResolver)
- [fix] Phase1-DriftFix iter1: AC Design Constraints C4 | Updated '30+ other' → '14 other' NotImplementedException stubs

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning
- [Predecessor: F774](feature-774.md) - Shop Core (Mandatory Handoff origin)
- [Successor: F775](feature-775.md) - Collection (SAVESTR consumer)
- [Successor: F782](feature-782.md) - Post-Phase 20 Review
- [Related: F776](feature-776.md) - Items
- [Related: F788](feature-788.md) - IConsoleOutput Extensions
- [Related: F790](feature-790.md) - Engine Data Access Layer
- [Related: F791](feature-791.md) - Engine State Transitions
