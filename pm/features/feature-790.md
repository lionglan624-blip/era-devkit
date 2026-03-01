# Feature 790: Engine Data Access Layer

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

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. Phase 20 ERB migrations universally use engine built-in variables and CSV name arrays that have no Era.Core interface. These are distinct from IVariableStore array accessors — they include computed values (CHARANUM, RAND), state references (MASTER, ASSI), and character string properties (NAME, CALLNAME). SSOT: `designs/phases/phase-20-27-game-systems.md` Phase 20 section.

### Problem (Current Issue)

Era.Core's interface layer was designed incrementally with IVariableStore serving only typed array accessors (1D/2D arrays like FLAG, ABL, TALENT). The ERA engine handles variables through three fundamentally distinct access patterns: (a) stored 1D arrays accessed at index 0 (MONEY, DAY, RESULT, COUNT, MASTER, ASSI via `DataIntegerArray[code & __LOWERCASE__][0]`), (b) computed/derived values (CHARANUM = `CharacterList.Count`, RAND = `GetNextRand(32768)`), and (c) CSV constant string arrays (ABLNAME, EXPNAME, MARKNAME, PALAMNAME loaded from `Game/CSV/` files via `ConstantData.names[]`). Patterns (b) and (c) have no Era.Core interface, and pattern (a) items used as scalars also have no accessor. Additionally, character-scoped non-array properties (NAME, CALLNAME as `__STRING__ | __CHARACTER_DATA__`, ISASSI as `__INTEGER__ | __CHARACTER_DATA__`) have no typed access. This gap produces 17 unique NotImplementedException stubs across ShopSystem.cs (5 stubs: `Era.Core/Shop/ShopSystem.cs:370-383`) and ShopDisplay.cs (12 stubs: `Era.Core/Shop/ShopDisplay.cs:403-432`).

Three existing interfaces partially overlap with F790 scope: ICharacterDataAccess.GetCharacterCount() covers CHARANUM (`Era.Core/Functions/ICharacterDataAccess.cs:17`), ICharacterDataService.GetCallName(CharacterId) covers CALLNAME (`Era.Core/Characters/ICharacterDataService.cs:18`), and IRandomProvider.Next(long) covers RAND (`Era.Core/Random/IRandomProvider.cs:15`). However, GetName() is missing from ICharacterDataService, and the ShopDisplay stubs use raw `int` while ICharacterDataService uses `CharacterId` — a type mismatch requiring resolution.

F774 Shop Core documented both gaps as Mandatory Handoffs (originally misdirected to "Phase 14").

### Goal (What to Achieve)

Create two ISP-segregated interfaces: (1) IEngineVariables for engine built-in variable access (RESULT, COUNT, CHARANUM, MASTER, ASSI, ISASSI, NAME, CALLNAME, MONEY, DAY, RAND) that delegates to existing interfaces where overlap exists (ICharacterDataAccess for CHARANUM, IRandomProvider for RAND, ICharacterDataService for CALLNAME), and (2) ICsvNameResolver for CSV constant name array read access (ABLNAME, EXPNAME, MARKNAME, PALAMNAME). Both with DI-registered null/stub implementations following the established pattern (NullCharacterDataAccess, NullCharacterDataService). Replace all 17 F790-scoped NotImplementedException stubs in ShopSystem.cs (5) and ShopDisplay.cs (8 engine + 4 CSV) with interface calls. Note: PrintDateMoneyStatus stub is cross-feature (F790 data access + F788 display) and excluded from the 17 count; see C13. Enable Phase 20 downstream features (F775, F776, F777, F778) to consume engine data through typed interfaces.

<!-- Sub-Feature Requirements (architecture.md): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

<!-- F788 Mandatory Handoffs (受け入れ側):
  - PrintPlayerBars スタブ解決 (ShopSystem.cs:401-402): BAR (F788提供済) + BASE/MAXBASE (既存IVariableStore) + MASTER (F790)
-->

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do ShopSystem/ShopDisplay have 17 NotImplementedException stubs for engine variables? | F774 migrated SHOP.ERB/SHOP2.ERB to C# but had no interface to wire engine built-in variables and CSV name arrays through | `Era.Core/Shop/ShopSystem.cs:370-383`, `Era.Core/Shop/ShopDisplay.cs:403-432` |
| 2 | Why is there no interface for these variables? | IVariableStore only provides typed 1D/2D array accessors (GetFlag, GetAbility, etc.) — scalar/computed built-ins and CSV constants are different access patterns | `Era.Core/Interfaces/IVariableStore.cs:15-103` |
| 3 | Why does IVariableStore not cover these patterns? | IVariableStore was designed during Phase 9 for character training variables (CFLAG, ABL, TALENT) using strongly-typed index wrappers; the architecture extends interfaces incrementally per consumer demand | `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs:86-92` (ISP registration pattern) |
| 4 | Why are engine built-in variables a different pattern? | They span four distinct categories: stored 1D arrays at index 0 (MONEY/DAY/RESULT/COUNT/MASTER/ASSI), computed values (CHARANUM/RAND), character-scoped non-array data (NAME/CALLNAME/ISASSI), and CSV constants (ABLNAME/EXPNAME/MARKNAME/PALAMNAME) | `Era.Core/Variables/VariableCode.cs:56-98,205,237-238,276-284` |
| 5 | Why (Root)? | The ERA engine's variable system is monolithic (all accessed through VariableData/VariableEvaluator), but Era.Core follows ISP segregation per consumer. Phase 20 is the first consumer phase requiring these heterogeneous variable categories, and no previous phase created an interface to bridge them | `engine/GameData/Variable/VariableEvaluator.cs:2640-2763` (all patterns in one class) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 17 NotImplementedException stubs in ShopSystem.cs and ShopDisplay.cs for engine variables | Era.Core has no interface for engine built-in scalar/computed variables or CSV constant name arrays — only IVariableStore for typed arrays |
| Where | `Era.Core/Shop/ShopSystem.cs:370-399`, `Era.Core/Shop/ShopDisplay.cs:403-432` | `Era.Core/Interfaces/` — missing IEngineVariables and ICsvNameResolver interfaces |
| Fix | Add methods to IVariableStore or IGameState | Create new ISP-segregated interfaces (IEngineVariables + ICsvNameResolver) with DI-registered null implementations, following existing patterns |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Predecessor — Phase 20 Planning parent |
| F774 | [DONE] | Predecessor — Shop Core, Mandatory Handoff origin for engine built-ins + CSV names |
| F788 | [DONE] | Related — IConsoleOutput Phase 20 Extensions (parallel, independent domain) |
| F789 | [PROPOSED] | Related — IVariableStore Phase 20 Extensions for SAVESTR/TA (parallel, independent domain) |
| F791 | [PROPOSED] | Related — Engine State Transitions & Entry Point Routing (parallel, independent domain) |
| F775 | [DRAFT] | Successor — Collection (SHOP_COLLECTION.ERB) will consume IEngineVariables for CHARANUM, CALLNAME, RESULT |
| F776 | [DRAFT] | Successor — Items (SHOP_ITEM.ERB) will consume IEngineVariables for RESULT, MONEY, MASTER |
| F777 | [DRAFT] | Successor — Customization (SHOP_CUSTOM.ERB) will consume IEngineVariables for RESULT, MASTER |
| F778 | [DRAFT] | Successor — Body Initialization will consume IEngineVariables for CHARANUM, CALLNAME, RESULT |
| F782 | [DRAFT] | Successor — Post-Phase Review verifies all Mandatory Handoffs resolved |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface design precedent | FEASIBLE | Existing ISP pattern (IVariableStore + ITrainingVariables + ICharacterStateVariables) in `ServiceCollectionExtensions.cs:86-92` |
| Null/stub implementation | FEASIBLE | Established pattern: `NullCharacterDataAccess.cs`, `NullCharacterDataService.cs` provide precedent |
| DI registration | FEASIBLE | Standard singleton registration with interface forwarding in `ServiceCollectionExtensions.cs` |
| Consumer readiness | FEASIBLE | All 17 stubs have clear method signatures; replacement is mechanical |
| Overlap resolution | NEEDS_REVISION | Three existing interfaces partially cover scope (ICharacterDataAccess, ICharacterDataService, IRandomProvider); design must decide delegation vs. duplication |
| Test infrastructure | FEASIBLE | Shop tests exist with mock patterns; adding null implementations is standard |

**Verdict**: NEEDS_REVISION — fundamentally feasible but requires design decisions on interface overlap strategy before ACs can be properly defined. The consensus recommends Option C: create new interfaces that delegate to existing ones where overlap exists.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Interfaces/ | HIGH | Two new interface files (IEngineVariables, ICsvNameResolver) |
| Era.Core/Shop/ | HIGH | ShopSystem and ShopDisplay constructor signatures change; 17 stubs replaced |
| Era.Core/DependencyInjection/ | MEDIUM | ServiceCollectionExtensions gains 2 new DI registrations |
| Era.Core.Tests/ | MEDIUM | Shop tests need mock updates for new constructor parameters |
| Downstream Phase 20 features | HIGH | F775, F776, F777, F778 will consume these interfaces when migrated |
| Existing consumers | LOW | No changes to existing interface consumers (IVariableStore, IGameState unchanged) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| TreatWarningsAsErrors=true | `Directory.Build.props` | All new interfaces/classes need XML doc comments |
| ICharacterDataAccess.GetCharacterCount() overlaps CHARANUM | `Era.Core/Functions/ICharacterDataAccess.cs:17` | Must delegate to existing, not duplicate |
| ICharacterDataService.GetCallName(CharacterId) overlaps CALLNAME | `Era.Core/Characters/ICharacterDataService.cs:18` | Type mismatch: CharacterId vs raw int in stubs |
| IRandomProvider.Next(long) overlaps RAND | `Era.Core/Random/IRandomProvider.cs:15` | Must delegate to existing, not duplicate |
| GetName() missing from ICharacterDataService | `Era.Core/Characters/ICharacterDataService.cs` | New IEngineVariables must provide GetName(int characterIndex) |
| NAME/CALLNAME/ISASSI are per-character data | `VariableCode.cs:205,237-238` | Interface methods need character index parameter |
| MONEY/DAY/RESULT/COUNT/MASTER/ASSI are 1D arrays (index 0 used) | `VariableCode.cs:56-98` | Interface simplifies to scalar access (index 0 only) |
| CSV name arrays are immutable constants | `VariableCode.cs:279-284` (`__UNCHANGEABLE__ | __CONSTANT__`) | ICsvNameResolver must be read-only (getters only) |
| ISP codebase convention | `ServiceCollectionExtensions.cs:86-92` | Two interfaces (IEngineVariables + ICsvNameResolver), not one monolithic interface |
| InfoState uses callback pattern for CSV names | `Era.Core/State/InfoState.cs:515` (`Func<int, string> getPalamName`) | ICsvNameResolver should be compatible with callback injection |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Interface duplication with ICharacterDataAccess/ICharacterDataService/IRandomProvider | HIGH | MEDIUM | Use delegation pattern (Option C): IEngineVariables delegates to existing interfaces where overlap exists |
| ShopDisplay constructor parameter explosion (currently 3 deps, adding 2 more) | MEDIUM | LOW | ISP keeps interfaces focused; consider parameter object if >6 deps |
| Type mismatch between stubs (raw int) and ICharacterDataService (CharacterId) | HIGH | MEDIUM | IEngineVariables accepts raw int and converts internally via ICharacterDataAccess |
| RAND semantic mismatch (engine: GetNextRand(32768) % N vs IRandomProvider: uniform [0, N)) | MEDIUM | LOW | Delegate to IRandomProvider.Next() with [0, N) semantics; document deviation from engine modulo behavior |
| PrintDateMoneyStatus stub straddles F790 (MONEY, DAY) and F788 (display formatting) | MEDIUM | LOW | F790 provides data access; composite stub resolution tracked as cross-feature dependency |
| COUNT may be translation artifact in ShopDisplay.cs:50 | LOW | LOW | Verify during implementation; keep in interface scope for now |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| F790-scoped stubs in ShopSystem.cs | `grep -c "NotImplementedException.*engine integration\|NotImplementedException.*MONEY\|NotImplementedException.*DAY" Era.Core/Shop/ShopSystem.cs` | 5 | GetResult, GetCharaNum, GetMaster, GetCallName, GetRandom |
| F790-scoped stubs in ShopDisplay.cs | `grep -c "NotImplementedException.*engine integration\|NotImplementedException.*CSV array" Era.Core/Shop/ShopDisplay.cs` | 12 | 8 engine built-in + 4 CSV name stubs |
| Existing Era.Core interfaces | `ls Era.Core/Interfaces/*.cs \| wc -l` | 37 | Before adding IEngineVariables + ICsvNameResolver |
| DI registrations | `grep -c "AddSingleton" Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` | ~20 | Before adding new registrations |

**Baseline File**: `.tmp/baseline-790.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | ISP split: IEngineVariables (built-ins) + ICsvNameResolver (CSV names) | Consensus: 3/3 investigations recommend split | AC must verify two separate interfaces, not one monolithic |
| C2 | Delegation to existing interfaces: ICharacterDataAccess (CHARANUM), IRandomProvider (RAND) | `Era.Core/Functions/ICharacterDataAccess.cs:17`, `Era.Core/Random/IRandomProvider.cs:15` | AC must verify no duplicate method semantics; delegation verified |
| C3 | Type reconciliation: stubs use raw int, ICharacterDataService uses CharacterId | `Era.Core/Characters/ICharacterDataService.cs:18`, `Era.Core/Shop/ShopDisplay.cs:409-411` | AC must verify IEngineVariables accepts int and converts internally |
| C4 | CSV name arrays are read-only constants | `VariableCode.cs:279-284` (`__UNCHANGEABLE__ | __CONSTANT__`) | AC must verify ICsvNameResolver has getters only, no setters |
| C5 | All 17 F790-scoped stubs must map to interface methods | `ShopSystem.cs:370-399`, `ShopDisplay.cs:403-432` | AC must verify each stub has corresponding interface method |
| C6 | DI registration with null/stub implementations | `ServiceCollectionExtensions.cs` pattern | AC must verify DI registration + NullXxx implementations exist |
| C7 | TreatWarningsAsErrors compliance | `Directory.Build.props` | AC must verify build succeeds with zero warnings (XML docs required) |
| C8 | Zero technical debt in new/modified code | Sub-Feature Requirements (architecture.md) | AC must verify no TODO/FIXME/HACK in F790 deliverables |
| C9 | Stub replacement in ShopSystem/ShopDisplay in-scope | F774 Mandatory Handoff | AC must include stub replacement verification |
| C10 | Per-character methods (NAME, CALLNAME, ISASSI) need character index parameter | `VariableCode.cs:205,237-238` | AC must verify character-scoped method signatures |
| C11 | Philosophy inheritance from Phase 20 | Sub-Feature Requirements (architecture.md) | AC Philosophy Derivation must trace from Phase 20 philosophy |
| C12 | Equivalence verification required | Sub-Feature Requirements (architecture.md) | AC must include interface contract equivalence tests |
| C13 | PrintDateMoneyStatus stub partial resolution | `ShopSystem.cs:398-399` (needs MONEY+DAY from F790 AND display from F788) | AC scope boundary must document cross-feature dependency |

### Constraint Details

**C1: ISP Interface Split**
- **Source**: All 3 independent investigations recommend separating engine built-in variables from CSV constant name arrays
- **Verification**: Check that Era.Core/Interfaces/ contains both IEngineVariables.cs and ICsvNameResolver.cs
- **AC Impact**: AC must test each interface independently; cannot merge into single interface

**C2: Delegation to Existing Interfaces**
- **Source**: ICharacterDataAccess.GetCharacterCount() already provides CHARANUM; IRandomProvider.Next(long) already provides RAND
- **Verification**: Check that IEngineVariables implementation delegates to these, not duplicates
- **AC Impact**: AC should verify that GetCharaNum() returns same value as ICharacterDataAccess.GetCharacterCount()

**C3: Type Reconciliation**
- **Source**: ShopDisplay stubs use `int characterIndex`; ICharacterDataService uses `CharacterId`
- **Verification**: Check IEngineVariables method signatures accept `int` and convert internally
- **AC Impact**: AC must test with raw int inputs matching ShopDisplay usage patterns

**C4: CSV Read-Only**
- **Source**: VariableCode ABLNAME/EXPNAME/MARKNAME/PALAMNAME have `__UNCHANGEABLE__ | __CONSTANT__` flags
- **Verification**: Check ICsvNameResolver has only `string GetXxxName(int index)` methods, no setters
- **AC Impact**: AC must verify interface has no Set/Write methods for CSV names

**C5: Stub Coverage**
- **Source**: F774 Mandatory Handoff documented 17 unique stubs requiring F790 resolution
- **Verification**: Count NotImplementedException stubs in ShopSystem.cs/ShopDisplay.cs attributed to F790
- **AC Impact**: AC must map each of 17 stubs to a corresponding interface method

**C6: DI Registration**
- **Source**: Established pattern in ServiceCollectionExtensions.cs
- **Verification**: Check AddSingleton<IEngineVariables> and AddSingleton<ICsvNameResolver> exist
- **AC Impact**: AC must verify both interfaces DI-registered with functional null implementations

**C7: TreatWarningsAsErrors compliance**
- **Source**: Directory.Build.props
- **Verification**: Check build succeeds with zero warnings; XML docs required on all new interfaces/classes
- **AC Impact**: AC must verify build succeeds with zero warnings (XML docs required)

**C8: Zero Technical Debt**
- **Source**: Sub-Feature Requirements in architecture.md require zero debt
- **Verification**: grep for TODO/FIXME/HACK in all F790 deliverables
- **AC Impact**: AC must include debt scan verification

**C9: Stub replacement in ShopSystem/ShopDisplay in-scope**
- **Source**: F774 Mandatory Handoff
- **Verification**: Check all 17 F790-scoped NotImplementedException stubs are replaced with interface calls
- **AC Impact**: AC must include stub replacement verification

**C10: Per-character methods (NAME, CALLNAME, ISASSI) need character index parameter**
- **Source**: VariableCode.cs:205,237-238
- **Verification**: Check IEngineVariables interface has 3 methods accepting int characterIndex
- **AC Impact**: AC must verify character-scoped method signatures

**C11: Philosophy inheritance from Phase 20**
- **Source**: Sub-Feature Requirements (architecture.md)
- **Verification**: Check Philosophy Derivation traces from Phase 20 philosophy
- **AC Impact**: AC Philosophy Derivation must trace from Phase 20 philosophy

**C12: Equivalence verification required**
- **Source**: Sub-Feature Requirements (architecture.md)
- **Verification**: Check unit tests verify interface contract equivalence (delegation behavior)
- **AC Impact**: AC must include interface contract equivalence tests

**C13: PrintDateMoneyStatus Cross-Feature Boundary**
- **Source**: ShopSystem.cs:398-399 stub requires MONEY/DAY (F790) AND PrintForml display (F788)
- **Verification**: Check stub resolution status after both F788 and F790 complete
- **AC Impact**: AC must document this stub as partially resolved by F790 (data access side only)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (parent) |
| Predecessor | F774 | [DONE] | Shop Core — Mandatory Handoff origin for engine built-ins + CSV name arrays |
| Related | F788 | [DONE] | IConsoleOutput Phase 20 Extensions (parallel, resolves separate stubs) |
| Related | F789 | [DONE] | IVariableStore Phase 20 Extensions (parallel, resolves SAVESTR/TA stubs) |
| Related | F791 | [DONE] | Engine State Transitions (parallel, resolves BeginTrain/SaveGame/LoadGame stubs) |
| Successor | F775 | [DONE] | Collection will consume IEngineVariables |
| Successor | F776 | [DONE] | Items will consume IEngineVariables |
| Successor | F777 | [DONE] | Customization will consume IEngineVariables |
| Successor | F778 | [DONE] | Body Initialization will consume IEngineVariables |
| Successor | F782 | [DRAFT] | Post-Phase Review verifies Mandatory Handoffs resolved |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Phase 20 ERB migrations universally use engine built-in variables and CSV name arrays that have no Era.Core interface" | Two new interfaces must exist: IEngineVariables and ICsvNameResolver | AC#1, AC#2 |
| "These are distinct from IVariableStore array accessors" | ISP-segregated interfaces separate from IVariableStore (C1) | AC#1, AC#2 |
| "SSOT: designs/phases/phase-20-27-game-systems.md Phase 20 section" | Phase 20 design establishes scope: engine built-ins + CSV names required by downstream features (F775-F778) | AC#1, AC#2, AC#3, AC#4 |
| "delegates to existing interfaces where overlap exists (ICharacterDataAccess for CHARANUM, IRandomProvider for RAND, ICharacterDataService for CALLNAME)" | Delegation pattern verified; no duplicate semantics (C2) | AC#5 |
| "Both with DI-registered null/stub implementations following the established pattern" | DI registration with null implementations exists | AC#3, AC#4 |
| "Replace all 17 F790-scoped NotImplementedException stubs" | Zero remaining F790-scoped stubs | AC#7, AC#8, AC#9 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IEngineVariables.cs exists | file | Glob(Era.Core/Interfaces/IEngineVariables.cs) | exists | - | [x] |
| 2 | ICsvNameResolver.cs exists | file | Glob(Era.Core/Interfaces/ICsvNameResolver.cs) | exists | - | [x] |
| 3 | IEngineVariables DI registration | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `AddSingleton<IEngineVariables` | [x] |
| 4 | ICsvNameResolver DI registration | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `AddSingleton<ICsvNameResolver` | [x] |
| 5 | IEngineVariables delegation and scaffolding verification | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~EngineVariables | succeeds | - | [x] |
| 6 | ICsvNameResolver read-only contract | code | Grep(Era.Core/Interfaces/ICsvNameResolver.cs) | not_matches | `void Set\|void Write\|void Update` | [x] |
| 7 | ShopSystem.cs zero F790-scoped stubs | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | `NotImplementedException.*RESULT read requires engine integration|NotImplementedException.*CHARANUM requires engine integration|NotImplementedException.*MASTER requires engine integration|NotImplementedException.*CALLNAME requires engine integration|NotImplementedException.*RAND requires engine integration` | [x] |
| 8 | ShopDisplay.cs zero F790-scoped engine built-in stubs | code | Grep(Era.Core/Shop/ShopDisplay.cs) | not_matches | `NotImplementedException.*RESULT read requires engine integration|NotImplementedException.*CHARANUM requires engine integration|NotImplementedException.*MASTER requires engine integration|NotImplementedException.*NAME requires engine integration|NotImplementedException.*CALLNAME requires engine integration|NotImplementedException.*ASSI requires engine integration|NotImplementedException.*COUNT requires engine integration|NotImplementedException.*ISASSI requires engine integration` | [x] |
| 9 | ShopDisplay.cs zero F790-scoped CSV name stubs | code | Grep(Era.Core/Shop/ShopDisplay.cs) | not_matches | `NotImplementedException.*ABLNAME CSV array|NotImplementedException.*EXPNAME CSV array|NotImplementedException.*MARKNAME CSV array|NotImplementedException.*PALAMNAME CSV array` | [x] |
| 10 | IEngineVariables has character-scoped methods with int parameter | code | Grep(path=Era.Core/Interfaces/IEngineVariables.cs, pattern=`int characterIndex`) | count_equals | 3 | [x] |
| 11 | Null IEngineVariables implementation exists | file | Glob(Era.Core/Interfaces/NullEngineVariables.cs) | exists | - | [x] |
| 12 | Null ICsvNameResolver implementation exists | file | Glob(Era.Core/Interfaces/NullCsvNameResolver.cs) | exists | - | [x] |
| 13 | Era.Core builds with zero warnings | build | dotnet build Era.Core | succeeds | - | [x] |
| 14 | Zero technical debt in F790 deliverables | code | Grep(Era.Core/Interfaces/IEngineVariables.cs,Era.Core/Interfaces/ICsvNameResolver.cs,Era.Core/Interfaces/NullEngineVariables.cs,Era.Core/Interfaces/NullCsvNameResolver.cs,Era.Core/Interfaces/EngineVariables.cs,Era.Core/Shop/ShopSystem.cs,Era.Core/Shop/ShopDisplay.cs) | not_matches | `TODO|FIXME|HACK` | [x] |
| 15 | Era.Core.Tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [x] |

### AC Details

**AC#1: IEngineVariables.cs exists**
- **Test**: Glob pattern=`Era.Core/Interfaces/IEngineVariables.cs`
- **Expected**: File exists
- **Rationale**: C1 requires ISP-segregated interface for engine built-in variables (RESULT, COUNT, CHARANUM, MASTER, ASSI, ISASSI, NAME, CALLNAME, MONEY, DAY, RAND). Separate from IVariableStore which handles typed arrays.

**AC#2: ICsvNameResolver.cs exists**
- **Test**: Glob pattern=`Era.Core/Interfaces/ICsvNameResolver.cs`
- **Expected**: File exists
- **Rationale**: C1 requires separate interface for CSV constant name array read access (ABLNAME, EXPNAME, MARKNAME, PALAMNAME). Read-only per C4.

**AC#3: IEngineVariables DI registration**
- **Test**: Grep pattern=`AddSingleton<IEngineVariables` path=`Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- **Expected**: Pattern found (DI registration exists)
- **Rationale**: C6 requires DI registration following established singleton pattern in ServiceCollectionExtensions.cs. Must register with null/stub implementation.

**AC#4: ICsvNameResolver DI registration**
- **Test**: Grep pattern=`AddSingleton<ICsvNameResolver` path=`Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- **Expected**: Pattern found (DI registration exists)
- **Rationale**: C6 requires DI registration for ICsvNameResolver with null/stub implementation following same pattern as ICharacterDataAccess/NullCharacterDataAccess.

**AC#5: IEngineVariables delegation and scaffolding verification**
- **Test**: `dotnet test Era.Core.Tests --filter FullyQualifiedName~EngineVariables`
- **Expected**: Tests pass verifying: (1) Delegation: GetCharaNum() delegates to ICharacterDataAccess.GetCharacterCount(), GetRandom(int) delegates to IRandomProvider.Next(long), GetCallName(int) converts index via ICharacterDataAccess.GetCharacterByIndex() and delegates to ICharacterDataService.GetCallName(CharacterId). Per C2, no duplicate method semantics. (2) Scaffolding: GetName(int) and GetIsAssi(int) perform index→CharacterId conversion via GetCharacterByIndex() then return safe defaults (string.Empty/0) since ICharacterDataService lacks these methods. Tests verify via mock.Verify: valid index triggers GetCharacterByIndex call and returns default, invalid index (null CharacterId) returns default.
- **Rationale**: C2 mandates delegation to ICharacterDataAccess (CHARANUM) and IRandomProvider (RAND). Technical Design also documents GetCallName delegation to ICharacterDataService. GetName/GetIsAssi use preparatory scaffolding (same index conversion pattern) returning defaults until ICharacterDataService is extended. C12 requires equivalence verification via unit tests. Note: This is contract-only verification (unit tests with mocked dependencies on the concrete EngineVariables class). DI registers NullEngineVariables as default; integration-level verification is out of F790 scope.

**AC#6: ICsvNameResolver read-only contract**
- **Test**: Grep pattern=`void Set|void Write|void Update` path=`Era.Core/Interfaces/ICsvNameResolver.cs`
- **Expected**: 0 matches (no setter/writer/updater methods)
- **Rationale**: C4 specifies CSV name arrays are immutable constants (`__UNCHANGEABLE__ | __CONSTANT__`). Interface must provide only getter methods.

**AC#7: ShopSystem.cs zero F790-scoped stubs**
- **Test**: Grep for F790-scoped NotImplementedException messages in `Era.Core/Shop/ShopSystem.cs` using partial match patterns (actual messages end with ` (Phase 14)`)
- **Expected**: 0 matches for all 5 F790-scoped stub messages (RESULT, CHARANUM, MASTER, CALLNAME, RAND engine integration stubs)
- **Rationale**: C5 and C9 require all 17 F790-scoped stubs replaced. ShopSystem.cs has 5 engine built-in stubs. Patterns use `.*` to match full messages including ` (Phase 14)` suffix. Note: Other stubs (SHOW_COLLECTION, ITEM_BUY, DRAWLINE, CLEARLINE, etc.) are out of F790 scope.
- **C13 Scope Boundary**: PrintDateMoneyStatus stub (`ShopSystem.cs:398-399`) is NOT in the 5-stub count. It requires both F790 (GetDay/GetMoney data access) AND F788 (PrintForml display). F790 provides the data access side via IEngineVariables; full resolution deferred to F788 completion.

**AC#8: ShopDisplay.cs zero F790-scoped engine built-in stubs**
- **Test**: Grep for F790-scoped NotImplementedException messages in `Era.Core/Shop/ShopDisplay.cs` using partial match patterns (actual messages end with ` (Phase 14)`)
- **Expected**: 0 matches for all 8 engine built-in stub messages (RESULT, CHARANUM, MASTER, NAME, CALLNAME, ASSI, COUNT, ISASSI)
- **Rationale**: C5 and C9 require replacement. ShopDisplay.cs has 8 engine built-in stubs. Patterns use `.*` to match full messages. Note: Other stubs (PRINT_STATE, TA 3D array, DRAWLINE, BAR, PRINTLC) are out of F790 scope.

**AC#9: ShopDisplay.cs zero F790-scoped CSV name stubs**
- **Test**: Grep for CSV array NotImplementedException messages in `Era.Core/Shop/ShopDisplay.cs` using partial match patterns (actual messages end with ` requires engine integration (Phase 14)`)
- **Expected**: 0 matches for all 4 CSV name stub messages (ABLNAME, EXPNAME, MARKNAME, PALAMNAME)
- **Rationale**: C5 and C9 require replacement. These 4 CSV name array stubs must be replaced with ICsvNameResolver calls. Patterns use `.*` to match full messages.

**AC#10: IEngineVariables has character-scoped methods with int parameter**
- **Test**: Grep pattern=`int characterIndex` path=`Era.Core/Interfaces/IEngineVariables.cs` | count
- **Expected**: 3 matches (NAME, CALLNAME, ISASSI methods each take `int characterIndex`)
- **Rationale**: C3 requires IEngineVariables accepts raw `int` (matching ShopDisplay stub signatures), not `CharacterId`. C10 requires per-character methods (NAME, CALLNAME, ISASSI) have character index parameter. Internal conversion to CharacterId via ICharacterDataAccess.

**AC#11: Null IEngineVariables implementation exists**
- **Test**: Glob pattern=`Era.Core/Interfaces/NullEngineVariables.cs`
- **Expected**: File exists
- **Rationale**: C6 requires null/stub implementations following NullCharacterDataAccess/NullCharacterDataService pattern. NullEngineVariables provides safe defaults (0 for int, empty string for string) when engine layer is not wired.

**AC#12: Null ICsvNameResolver implementation exists**
- **Test**: Glob pattern=`Era.Core/Interfaces/NullCsvNameResolver.cs`
- **Expected**: File exists
- **Rationale**: C6 requires null/stub implementation for ICsvNameResolver. Returns empty string or index-based placeholder for CSV name lookups.

**AC#13: Era.Core builds with zero warnings**
- **Test**: `dotnet build Era.Core`
- **Expected**: Build succeeds (exit code 0) with zero warnings
- **Rationale**: C7 mandates TreatWarningsAsErrors compliance. All new interfaces and implementations must have XML doc comments.

**AC#14: Zero technical debt in F790 deliverables**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths=`Era.Core/Interfaces/IEngineVariables.cs`, `Era.Core/Interfaces/ICsvNameResolver.cs`, `Era.Core/Interfaces/NullEngineVariables.cs`, `Era.Core/Interfaces/NullCsvNameResolver.cs`, `Era.Core/Interfaces/EngineVariables.cs`, `Era.Core/Shop/ShopSystem.cs`, `Era.Core/Shop/ShopDisplay.cs`
- **Expected**: 0 matches across all F790 deliverable and modified files
- **Rationale**: C8 requires zero technical debt. Includes both new files and files modified by F790 (ShopSystem.cs, ShopDisplay.cs) to detect newly introduced debt. Existing non-F790 stubs (NotImplementedException for other features) are not TODO/FIXME/HACK and won't match. Cross-reference C13 for partial PrintDateMoneyStatus resolution.

**AC#15: Era.Core.Tests pass**
- **Test**: `dotnet test Era.Core.Tests`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Ensures no regression in existing tests after adding new interfaces, DI registrations, and replacing stubs in ShopSystem/ShopDisplay. C12 equivalence tests for delegation are covered by AC#5 filter.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Create IEngineVariables interface for engine built-in variable access (RESULT, COUNT, CHARANUM, MASTER, ASSI, ISASSI, NAME, CALLNAME, MONEY, DAY, RAND) | AC#1, AC#10 |
| 2 | Create ICsvNameResolver interface for CSV constant name array read access (ABLNAME, EXPNAME, MARKNAME, PALAMNAME) | AC#2, AC#6 |
| 3 | IEngineVariables delegates to existing interfaces where overlap exists (ICharacterDataAccess for CHARANUM, IRandomProvider for RAND, ICharacterDataService for CALLNAME) | AC#5 |
| 4 | DI-registered null/stub implementations following established pattern | AC#3, AC#4, AC#11, AC#12 |
| 5 | Replace all 17 F790-scoped NotImplementedException stubs in ShopSystem.cs and ShopDisplay.cs | AC#7, AC#8, AC#9 |
| 6 | Enable Phase 20 downstream features (F775-F778) to consume engine data through typed interfaces | AC#1, AC#2, AC#3, AC#4 |
| 7 | Build succeeds (TreatWarningsAsErrors) | AC#13 |
| 8 | Zero technical debt in new code | AC#14 |
| 9 | No regression in existing tests | AC#15 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Create two ISP-segregated interfaces following the established DI pattern:

**1. IEngineVariables** (11 methods for engine built-in variables):
- Scalar methods: `GetResult()`, `GetMoney()`, `GetDay()`, `GetMaster()`, `GetAssi()`, `GetCount()`
- Delegation methods: `GetCharaNum()` delegates to `ICharacterDataAccess.GetCharacterCount()`, `GetRandom(int max)` delegates to `IRandomProvider.Next(max)`
- Character-scoped methods (3): `GetName(int characterIndex)`, `GetCallName(int characterIndex)`, `GetIsAssi(int characterIndex)`
  - Accept raw `int` per C3 (ShopDisplay stub signatures), convert internally to `CharacterId` via `ICharacterDataAccess.GetCharacterByIndex()`
  - `GetCallName()` delegates to `ICharacterDataService.GetCallName(CharacterId)` after conversion

**2. ICsvNameResolver** (4 read-only methods):
- `string GetAblName(int index)`, `string GetExpName(int index)`, `string GetMarkName(int index)`, `string GetPalamName(int index)`
- No setter methods per C4 (CSV constants are immutable)

**Null implementations** (following NullCharacterDataAccess / NullCharacterDataService pattern):
- `NullEngineVariables`: Returns safe defaults (0 for int, empty string for string methods)
- `NullCsvNameResolver`: Returns empty strings for all CSV name lookups

**Concrete implementation** (for delegation testing per AC#5):
- `EngineVariables`: Accepts ICharacterDataAccess, ICharacterDataService, IRandomProvider as constructor deps
- GetCharaNum() delegates to ICharacterDataAccess.GetCharacterCount()
- GetRandom(int max) delegates to IRandomProvider.Next(max) with (int) narrowing cast
- GetCallName(int) converts index via GetCharacterByIndex(), delegates to ICharacterDataService.GetCallName(CharacterId) with .Match(v => v, _ => string.Empty) unwrapping
- Character-scoped methods null-handle CharacterId? from GetCharacterByIndex (return string.Empty/0 on null)
- Scalar methods (GetResult, GetMoney, etc.) return 0 — deferred to engine adapter wiring

**DI registration** in `ServiceCollectionExtensions.cs`:
```csharp
services.AddSingleton<IEngineVariables, NullEngineVariables>();
services.AddSingleton<ICsvNameResolver, NullCsvNameResolver>();
```

**Stub replacement**:
- `ShopSystem.cs` (5 stubs): Replace GetResult, GetCharaNum, GetMaster, GetCallName, GetRandom with IEngineVariables calls
- `ShopDisplay.cs` (12 stubs): Replace 8 engine built-in stubs + 4 CSV name stubs with IEngineVariables / ICsvNameResolver calls
- Add constructor parameters to ShopSystem and ShopDisplay for both interfaces

**Interface delegation verification** (AC#5):
- Unit test `EngineVariablesDelegationTests.cs` verifies GetCharaNum() returns same value as ICharacterDataAccess.GetCharacterCount()
- Unit test verifies GetRandom(int) delegates to IRandomProvider.Next(long) with correct type conversion

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Interfaces/IEngineVariables.cs` with 11 methods (6 scalar + 2 delegation + 3 character-scoped) |
| 2 | Create `Era.Core/Interfaces/ICsvNameResolver.cs` with 4 read-only getter methods |
| 3 | Add `services.AddSingleton<IEngineVariables, NullEngineVariables>();` to ServiceCollectionExtensions.cs |
| 4 | Add `services.AddSingleton<ICsvNameResolver, NullCsvNameResolver>();` to ServiceCollectionExtensions.cs |
| 5 | Create `Era.Core.Tests/Interfaces/EngineVariablesDelegationTests.cs` with unit tests verifying delegation (GetCharaNum, GetRandom, GetCallName) and scaffolding (GetName, GetIsAssi return safe defaults after index conversion) |
| 6 | ICsvNameResolver contains only `string GetXxxName(int)` methods, no setters/writers/updaters |
| 7 | Replace 5 stubs in ShopSystem.cs with IEngineVariables calls (inject via constructor) |
| 8 | Replace 8 engine built-in stubs in ShopDisplay.cs with IEngineVariables calls (inject via constructor) |
| 9 | Replace 4 CSV name stubs in ShopDisplay.cs with ICsvNameResolver calls (inject via constructor) |
| 10 | IEngineVariables has 3 methods with `int characterIndex` parameter: GetName, GetCallName, GetIsAssi |
| 11 | Create `Era.Core/Interfaces/NullEngineVariables.cs` with safe default implementations |
| 12 | Create `Era.Core/Interfaces/NullCsvNameResolver.cs` with placeholder implementations |
| 13 | Add XML doc comments to all interfaces/classes per TreatWarningsAsErrors requirement |
| 14 | No TODO/FIXME/HACK in IEngineVariables.cs, ICsvNameResolver.cs, NullEngineVariables.cs, NullCsvNameResolver.cs |
| 15 | Update existing Shop tests to inject null implementations via constructor; no functional changes to shop behavior |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Interface split strategy | A: Single IEngineDataAccess for all 15 methods; B: Two interfaces (engine built-ins + CSV names); C: Three interfaces (scalar, computed, CSV) | B: IEngineVariables + ICsvNameResolver | C1 requires ISP split. Option B matches semantic boundary: engine built-in variables (stateful/computed) vs CSV constants (immutable read-only). Option C over-segregates (delegation methods like CHARANUM/RAND mix with scalars naturally). |
| Delegation vs duplication | A: IEngineVariables.GetCharaNum() duplicates ICharacterDataAccess.GetCharacterCount(); B: IEngineVariables delegates to existing interfaces internally; C: Remove GetCharaNum/GetRandom from IEngineVariables, force consumers to use existing interfaces | B: Delegation | C2 mandates delegation to avoid semantic duplication. Option A violates DRY. Option C breaks LSP (consumers expect CHARANUM via engine variable interface). Delegation preserves Era.Core ISP benefits while exposing ERA engine semantics. |
| Type mismatch resolution (int vs CharacterId) | A: IEngineVariables uses CharacterId; B: IEngineVariables uses raw int, converts internally; C: Create overloads for both int and CharacterId | B: Internal conversion | C3 requires interface accepts `int` matching ShopDisplay stub signatures. Option A forces consumers to convert (burdens 4 downstream features). Option C creates API confusion. Option B isolates conversion inside IEngineVariables implementation. |
| GetName() interface placement | A: Extend ICharacterDataService with GetName(); B: Add GetName() to IEngineVariables only | B: IEngineVariables only | ICharacterDataService is DDD character domain service (Phase 18/F628). GetName() is engine built-in variable access (stateless accessor, not domain logic). Mixing concerns violates ISP. Future: when Phase 20 migrations complete, ICharacterDataService may add GetName() for domain use; F790 scopes to engine variable interface only. |
| Random semantics | A: Match engine `GetNextRand(32768) % N` semantics exactly; B: Use IRandomProvider.Next(N) uniform [0, N) semantics; C: Add IRandomProvider.NextModulo(max, modulus) | B: IRandomProvider.Next() | Risk analysis acknowledges semantic mismatch (engine uses modulo). Option A requires duplicating RandomProvider logic. Option C adds complexity for negligible benefit (ShopSystem uses RAND for selection, modulo bias irrelevant). Option B delegates cleanly; document deviation in IEngineVariables XML comment. |
| Null implementation return values | A: Return 0/empty string; B: Throw NotImplementedException; C: Return sentinel values (e.g., -1 for int, "[UNDEFINED]" for string) | A: Return safe defaults | NullCharacterDataAccess precedent returns 0. Null implementations support unit testing without engine wiring. Option B defeats purpose of null pattern. Option C risks consumers treating sentinel as valid data. |
| PrintDateMoneyStatus stub resolution | A: Fully resolve in F790 (add GetDay, GetMoney, date formatting); B: Partially resolve (provide GetDay/GetMoney only, defer display to F788); C: Skip entirely (out of scope) | B: Partial resolution | C13 documents cross-feature dependency. F790 provides data access (GetDay, GetMoney). F788 provides PrintForml display formatting. Stub requires both. Option A duplicates F788 scope. Option C leaves stub unresolved. Option B satisfies F790 scope boundary; stub resolved when both F788 + F790 complete. |

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| (none) | - | All 15 ACs satisfiable with current constraints. Interface deps (ICharacterDataAccess, ICharacterDataService, IRandomProvider) verified present. |

**Cross-Feature Dependencies** (not blocking F790, informational only):

| Dependency | F790 Scope | External Scope | Resolution |
|-----------|------------|----------------|------------|
| PrintDateMoneyStatus stub (`ShopSystem.cs:398-399`) | GetDay/GetMoney (data access) | F788: PrintForml display formatting | Partially resolved by F790; fully resolved when F788 completes |
| GetCallName() → ICharacterDataService (F628) | Interface contract via delegation | NullCharacterDataService returns empty | Full character data wiring out of F790 scope |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,10 | Create IEngineVariables interface with 11 methods (6 scalar + 2 delegation + 3 character-scoped) in Era.Core/Interfaces/IEngineVariables.cs | | [x] |
| 2 | 2,6 | Create ICsvNameResolver interface with 4 read-only getter methods in Era.Core/Interfaces/ICsvNameResolver.cs | | [x] |
| 3 | 11 | Create NullEngineVariables class with safe default implementations in Era.Core/Interfaces/NullEngineVariables.cs | | [x] |
| 4 | 12 | Create NullCsvNameResolver class with placeholder implementations in Era.Core/Interfaces/NullCsvNameResolver.cs | | [x] |
| 5 | 3,4 | Register both interfaces in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs with null implementations | | [x] |
| 6 | 5 | Add `<InternalsVisibleTo Include="Era.Core.Tests" />` to Era.Core.csproj to enable testing of internal classes | | [x] |
| 7 | 5 | Create EngineVariablesDelegationTests.cs in Era.Core.Tests/Interfaces/ verifying delegation (GetCharaNum, GetRandom, GetCallName) and scaffolding (GetName, GetIsAssi return safe defaults after index conversion) | | [x] |
| 8 | 7,8,9 | Replace all 17 F790-scoped NotImplementedException stubs in ShopSystem.cs and ShopDisplay.cs with IEngineVariables/ICsvNameResolver calls | | [x] |
| 9 | 13,14,15 | Verify Era.Core builds with zero warnings, zero debt in deliverables, and no regression in existing Era.Core.Tests | | [x] |

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

### Interface Definitions

**IEngineVariables** (11 methods) in `Era.Core/Interfaces/IEngineVariables.cs`:
```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Interface for accessing ERA engine built-in variables.
/// Provides typed access to scalar variables (RESULT, MONEY, DAY, MASTER, ASSI, COUNT),
/// computed values (CHARANUM via ICharacterDataAccess, RAND via IRandomProvider),
/// and character-scoped properties (NAME, CALLNAME, ISASSI).
/// Note: GetRandom uses IRandomProvider uniform [0, max) semantics, not engine modulo.
/// Feature 790 - Engine Data Access Layer
/// </summary>
public interface IEngineVariables
{
    /// <summary>Get RESULT value (function return value / user input result)</summary>
    int GetResult();

    /// <summary>Get MONEY value (player money, stored in MONEY:0)</summary>
    int GetMoney();

    /// <summary>Get DAY value (elapsed days, stored in DAY:0)</summary>
    int GetDay();

    /// <summary>Get MASTER value (master character index, stored in MASTER:0)</summary>
    int GetMaster();

    /// <summary>Get ASSI value (assistant character index, stored in ASSI:0)</summary>
    int GetAssi();

    /// <summary>Get COUNT value (general counter, stored in COUNT:0)</summary>
    int GetCount();

    /// <summary>Get total character count (delegates to ICharacterDataAccess.GetCharacterCount)</summary>
    int GetCharaNum();

    /// <summary>
    /// Generate random number [0, max) (delegates to IRandomProvider.Next).
    /// Note: Uses uniform distribution [0, max), not engine modulo semantics.
    /// </summary>
    int GetRandom(int max);

    /// <summary>Get character NAME (runtime value) by 0-based index</summary>
    string GetName(int characterIndex);

    /// <summary>Get character CALLNAME (runtime value) by 0-based index</summary>
    string GetCallName(int characterIndex);

    /// <summary>Get character ISASSI flag (is assistant) by 0-based index</summary>
    int GetIsAssi(int characterIndex);
}
```

**ICsvNameResolver** (4 methods) in `Era.Core/Interfaces/ICsvNameResolver.cs`:
```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Interface for resolving CSV constant name arrays.
/// Provides read-only access to ABLNAME, EXPNAME, MARKNAME, PALAMNAME.
/// These are immutable constants loaded from Game/CSV/ files.
/// Feature 790 - Engine Data Access Layer
/// </summary>
public interface ICsvNameResolver
{
    /// <summary>Get ability name by index (ABLNAME)</summary>
    string GetAblName(int index);

    /// <summary>Get experience name by index (EXPNAME)</summary>
    string GetExpName(int index);

    /// <summary>Get mark name by index (MARKNAME)</summary>
    string GetMarkName(int index);

    /// <summary>Get parameter name by index (PALAMNAME)</summary>
    string GetPalamName(int index);
}
```

**NullEngineVariables** in `Era.Core/Interfaces/NullEngineVariables.cs`:
```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Null implementation of IEngineVariables for testing/stub scenarios.
/// Returns safe default values (0 for int, empty string for string).
/// Feature 790 - Engine Data Access Layer
/// </summary>
internal sealed class NullEngineVariables : IEngineVariables
{
    /// <inheritdoc />
    public int GetResult() => 0;

    /// <inheritdoc />
    public int GetMoney() => 0;

    /// <inheritdoc />
    public int GetDay() => 0;

    /// <inheritdoc />
    public int GetMaster() => 0;

    /// <inheritdoc />
    public int GetAssi() => 0;

    /// <inheritdoc />
    public int GetCount() => 0;

    /// <inheritdoc />
    public int GetCharaNum() => 0;

    /// <inheritdoc />
    public int GetRandom(int max) => 0;

    /// <inheritdoc />
    public string GetName(int characterIndex) => string.Empty;

    /// <inheritdoc />
    public string GetCallName(int characterIndex) => string.Empty;

    /// <inheritdoc />
    public int GetIsAssi(int characterIndex) => 0;
}
```

**NullCsvNameResolver** in `Era.Core/Interfaces/NullCsvNameResolver.cs`:
```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Null implementation of ICsvNameResolver for testing/stub scenarios.
/// Returns empty strings for all CSV name lookups.
/// Feature 790 - Engine Data Access Layer
/// </summary>
internal sealed class NullCsvNameResolver : ICsvNameResolver
{
    /// <inheritdoc />
    public string GetAblName(int index) => string.Empty;

    /// <inheritdoc />
    public string GetExpName(int index) => string.Empty;

    /// <inheritdoc />
    public string GetMarkName(int index) => string.Empty;

    /// <inheritdoc />
    public string GetPalamName(int index) => string.Empty;
}
```

**EngineVariables** (concrete delegation implementation):
```csharp
using Era.Core.Characters;
using Era.Core.Functions;
using Era.Core.Random;

namespace Era.Core.Interfaces;

/// <summary>
/// Concrete implementation of IEngineVariables with delegation to existing interfaces.
/// Delegates CHARANUM to ICharacterDataAccess, RAND to IRandomProvider,
/// CALLNAME to ICharacterDataService. Character-scoped methods convert int index
/// to CharacterId via ICharacterDataAccess.GetCharacterByIndex().
/// Note: GetRandom uses (int) narrowing cast from long — safe for ERA RAND range.
/// Feature 790 - Engine Data Access Layer
/// </summary>
internal sealed class EngineVariables : IEngineVariables
{
    private readonly ICharacterDataAccess _characterAccess;
    private readonly ICharacterDataService _characterService;
    private readonly IRandomProvider _randomProvider;

    public EngineVariables(
        ICharacterDataAccess characterAccess,
        ICharacterDataService characterService,
        IRandomProvider randomProvider)
    {
        _characterAccess = characterAccess;
        _characterService = characterService;
        _randomProvider = randomProvider;
    }

    // Scalar variables — deferred to engine adapter (return 0 until wired)
    /// <inheritdoc />
    public int GetResult() => 0;
    /// <inheritdoc />
    public int GetMoney() => 0;
    /// <inheritdoc />
    public int GetDay() => 0;
    /// <inheritdoc />
    public int GetMaster() => 0;
    /// <inheritdoc />
    public int GetAssi() => 0;
    /// <inheritdoc />
    public int GetCount() => 0;

    // Delegation methods
    /// <inheritdoc />
    public int GetCharaNum() => _characterAccess.GetCharacterCount();

    /// <inheritdoc />
    public int GetRandom(int max) => (int)_randomProvider.Next(max);

    // Character-scoped methods (int index → CharacterId conversion)
    /// <inheritdoc />
    public string GetName(int characterIndex)
    {
        // Preparatory scaffolding: index→CharacterId conversion matches GetCallName pattern.
        // Returns default until ICharacterDataService gains GetName (same deferred pattern as scalars).
        var charId = _characterAccess.GetCharacterByIndex(characterIndex);
        if (charId is null) return string.Empty;
        return string.Empty;
    }

    /// <inheritdoc />
    public string GetCallName(int characterIndex)
    {
        var charId = _characterAccess.GetCharacterByIndex(characterIndex);
        if (charId is null) return string.Empty;
        return _characterService.GetCallName(charId.Value).Match(v => v, _ => string.Empty);
    }

    /// <inheritdoc />
    public int GetIsAssi(int characterIndex)
    {
        // Preparatory scaffolding: index→CharacterId conversion matches GetCallName pattern.
        // Returns default until ICharacterDataService gains GetIsAssi (same deferred pattern as scalars).
        var charId = _characterAccess.GetCharacterByIndex(characterIndex);
        if (charId is null) return 0;
        return 0;
    }
}
```

### DI Registration

Add to `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:
```csharp
services.AddSingleton<IEngineVariables, NullEngineVariables>();
services.AddSingleton<ICsvNameResolver, NullCsvNameResolver>();
```

### InternalsVisibleTo

Add to `Era.Core/Era.Core.csproj` (enables Era.Core.Tests to access internal classes like EngineVariables):
```xml
<ItemGroup>
  <InternalsVisibleTo Include="Era.Core.Tests" />
</ItemGroup>
```

### Stub Replacement

**ShopSystem.cs** constructor change (5 stubs):
```csharp
// Before (F774):
public ShopSystem(ShopDisplay display, IVariableStore variables, IConsoleOutput console, IGameState gameState, IInputHandler inputHandler)

// After (F790):
public ShopSystem(
    ShopDisplay display,
    IVariableStore variables,
    IConsoleOutput console,
    IGameState gameState,
    IInputHandler inputHandler,
    IEngineVariables engineVars)
```
Note: ICsvNameResolver REMOVED from ShopSystem (ISP: no CSV stubs in ShopSystem).

**ShopDisplay.cs** constructor change (12 stubs):
```csharp
// Before (F774):
public ShopDisplay(IVariableStore variables, IConsoleOutput console, IInputHandler inputHandler)

// After (F790):
public ShopDisplay(
    IVariableStore variables,
    IConsoleOutput console,
    IInputHandler inputHandler,
    IEngineVariables engineVars,
    ICsvNameResolver csvNames)
```

Replace engine built-in stubs (8):
- `GetResult()` → `_engineVars.GetResult()`
- `GetCharaNum()` → `_engineVars.GetCharaNum()`
- `GetMaster()` → `_engineVars.GetMaster()`
- `GetName(int characterIndex)` → `_engineVars.GetName(characterIndex)`
- `GetCallName(int characterIndex)` → `_engineVars.GetCallName(characterIndex)`
- `GetAssi()` → `_engineVars.GetAssi()`
- `GetCount()` → `_engineVars.GetCount()`
- `GetIsAssi(int characterIndex)` → `_engineVars.GetIsAssi(characterIndex)`

Replace CSV name stubs (4):
- `GetAblName(int index)` → `_csvNames.GetAblName(index)`
- `GetExpName(int index)` → `_csvNames.GetExpName(index)`
- `GetMarkName(int index)` → `_csvNames.GetMarkName(index)`
- `GetPalamName(int index)` → `_csvNames.GetPalamName(index)`

### Delegation Tests

Create `Era.Core.Tests/Interfaces/EngineVariablesDelegationTests.cs`:

**Mocking Framework**: Use Moq (already available in Era.Core.Tests — see `CallbackFactoriesTests.cs`, `CustomComLoaderTests.cs`).

```csharp
using Era.Core.Interfaces;
using Moq;
using Xunit;

namespace Era.Core.Tests.Interfaces;

public class EngineVariablesDelegationTests
{
    [Fact]
    public void GetCharaNum_DelegatesToICharacterDataAccess()
    {
        // Verify IEngineVariables.GetCharaNum() returns same value as ICharacterDataAccess.GetCharacterCount()
        // Use Moq to mock ICharacterDataAccess and verify delegation
    }

    [Fact]
    public void GetRandom_DelegatesToIRandomProvider()
    {
        // Verify IEngineVariables.GetRandom(int) delegates to IRandomProvider.Next(long)
        // Verify type conversion int→long
    }

    [Fact]
    public void GetCallName_DelegatesToICharacterDataService()
    {
        // Verify IEngineVariables.GetCallName(int characterIndex) converts index
        // via ICharacterDataAccess.GetCharacterByIndex() to CharacterId,
        // then delegates to ICharacterDataService.GetCallName(CharacterId)
    }

    [Fact]
    public void GetName_ReturnsDefaultAfterIndexConversion()
    {
        // Verify GetName(int) performs index→CharacterId conversion via GetCharacterByIndex()
        // Returns string.Empty (ICharacterDataService lacks GetName)
        // Test: valid index (CharacterId resolves) → string.Empty
        //   + mock.Verify(m => m.GetCharacterByIndex(expectedIndex), Times.Once()) — proves conversion executed
        // Test: invalid index (CharacterId null) → string.Empty
    }

    [Fact]
    public void GetIsAssi_ReturnsDefaultAfterIndexConversion()
    {
        // Verify GetIsAssi(int) performs index→CharacterId conversion via GetCharacterByIndex()
        // Returns 0 (ICharacterDataService lacks GetIsAssi)
        // Test: valid index (CharacterId resolves) → 0
        //   + mock.Verify(m => m.GetCharacterByIndex(expectedIndex), Times.Once()) — proves conversion executed
        // Test: invalid index (CharacterId null) → 0
    }
}
```

### Cross-Feature Dependencies

**PrintDateMoneyStatus Partial Resolution** (`ShopSystem.cs:398-399`):
- F790 provides: `GetDay()`, `GetMoney()` (data access)
- F788 provides: `PrintForml` display formatting
- Stub will be partially resolved by F790 (data side), fully resolved when F788 completes

**GetCallName Type Conversion**:
- ShopDisplay stubs use `int characterIndex`
- ICharacterDataService uses `CharacterId`
- IEngineVariables accepts `int`, converts internally via `ICharacterDataAccess.GetCharacterByIndex()`

**COUNT Semantic Verification** (ShopDisplay:50):
- `GetCount() == assi` in LifeList assistant-select loop may be a migration artifact
- In ERB, REPEAT sets COUNT as loop counter; C# migration uses `chr` as loop variable
- Implementer must verify during Task 7: if COUNT should be `chr`, fix during stub replacement

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| (none) | All 17 F790-scoped stubs resolved within feature scope | - | - | - |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-15 07:21 | START | implementer | Tasks 1-6 | - |
| 2026-02-15 07:21 | END | implementer | Tasks 1-6 | SUCCESS |
| 2026-02-15 07:30 | START | implementer | Task 7 (tests) | - |
| 2026-02-15 07:30 | END | implementer | Task 7 (tests) | SUCCESS |
| 2026-02-15 07:31 | DEVIATION | Bash | dotnet build Era.Core.Tests | CS0234: Interfaces.IVariableStore namespace collision caused by InternalsVisibleTo + Era.Core.Tests.Interfaces namespace. Fixed: FQ name in AbilitySystemTests.cs (4) + CharacterFunctionsTests.cs (1) |
| 2026-02-15 07:32 | TDD-RED | Bash | dotnet build Era.Core.Tests | Expected: 9 CS0246 errors (EngineVariables not found) = RED confirmed |
| 2026-02-15 07:34 | START | implementer | Task 7 (concrete impl) | - |
| 2026-02-15 07:34 | TDD-GREEN | Bash | dotnet test Era.Core.Tests --filter EngineVariables | 9 tests PASS |
| 2026-02-15 07:34 | NO-REGRESSION | Bash | dotnet test Era.Core.Tests | 1929 tests PASS (no regression) |
| 2026-02-15 07:34 | END | implementer | Task 7 (concrete impl) | SUCCESS |
| 2026-02-15 07:38 | START | implementer | Task 8 (stub replacement) | - |
| 2026-02-15 07:38 | BUILD-PASS | Bash | dotnet build Era.Core | 0 warnings, 0 errors |
| 2026-02-15 07:38 | NO-REGRESSION | Bash | dotnet test Era.Core.Tests | 1929 tests PASS (no regression) |
| 2026-02-15 07:38 | END | implementer | Task 8 (stub replacement) | SUCCESS |
| 2026-02-15 07:40 | START | orchestrator | Task 9 (verification) | - |
| 2026-02-15 07:40 | BUILD-PASS | Bash | dotnet build Era.Core | 0 warnings, 0 errors |
| 2026-02-15 07:40 | DEBT-ZERO | Grep | TODO/FIXME/HACK scan | 0 matches across 7 F790 deliverables |
| 2026-02-15 07:40 | NO-REGRESSION | Bash | dotnet test Era.Core.Tests | 1929 tests PASS |
| 2026-02-15 07:40 | END | orchestrator | Task 9 (verification) | SUCCESS |
| 2026-02-15 07:42 | AC-VERIFY | ac-static-verifier | file ACs | 4/4 PASS |
| 2026-02-15 07:42 | AC-VERIFY | ac-static-verifier | code ACs | 7/8 PASS (AC#10 count_equals not supported by tool, manually verified: 3 matches) |
| 2026-02-15 07:42 | AC-VERIFY | ac-static-verifier | build ACs | 1/1 PASS |
| 2026-02-15 07:42 | AC-VERIFY | dotnet test | AC#5 EngineVariables tests | 9/9 PASS |
| 2026-02-15 07:42 | AC-VERIFY | dotnet test | AC#15 full test suite | 1929/1929 PASS |
| 2026-02-15 07:45 | DEVIATION | ac-tester | feature-790.md | ac-tester (haiku) destructively overwrote feature-790.md to [DRAFT] state. File reconstructed from context. |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | F628 referenced in body (Key Decisions, Upstream Issues) but missing from Links section
- [resolved-applied] Phase2-Pending iter1: [INV-003] Implementation Contract constructor signatures do not match actual code. ShopSystem actual: (ShopDisplay, IVariableStore, IConsoleOutput, IGameState, IInputHandler) but Contract says: (IConsoleOutput, IVariableStore, IGameState). ShopDisplay actual: (IVariableStore, IConsoleOutput, IInputHandler) but Contract says: (IConsoleOutput, IVariableStore, IGameState). Both Technical Design and Implementation Contract sections need correction. → Fixed: Updated all constructor signatures to match actual code.
- [resolved-applied] Phase2-Pending iter1: [AC-006] AC#5 requires delegation testing but no concrete EngineVariables class is defined in Implementation Contract. NullEngineVariables returns 0 (no delegation). Need to add concrete EngineVariables class definition accepting ICharacterDataAccess+IRandomProvider+ICharacterDataService as constructor deps, with delegation logic. → Fixed: Added concrete EngineVariables class to Technical Design and Implementation Contract with delegation to ICharacterDataAccess, IRandomProvider, ICharacterDataService. DI registers NullEngineVariables as default; engine project provides concrete adapter.
- [resolved-skipped] Phase2-Uncertain iter1: [FMT-001] Implementation Contract uses free-form subsections instead of template Phase|Agent|Model|Input|Output table. Template may or may not require this table for engine-type features with detailed code contracts. → Engine-type features with detailed code blocks use code-as-spec pattern. Phase|Agent|Model table not required.
- [fix] Phase2-Review iter1: [AC-002] AC#14 scope | Expanded debt scan paths to include Era.Core/Shop/ShopSystem.cs and Era.Core/Shop/ShopDisplay.cs (F790-modified files)
- [fix] Phase2-Review iter1: [FMT-001] Mandatory Handoffs section | Converted free-text to template table format
- [fix] Phase2-Review iter1: [FMT-001] Created field | Removed non-template Created field
- [fix] Phase2-Review iter2: [AC-002] AC#7,AC#8,AC#9 patterns | Fixed grep patterns to use .* partial match instead of literal \( \) that didn't match actual stub messages ending with " (Phase 14)"
- [resolved-applied] Phase2-Pending iter2: [FMT-001] Summary section not in feature template. Content should be merged into Background > Goal or removed. → Fixed: Summary already duplicated in Background > Goal. Removed Summary section.
- [fix] Phase2-Review iter2: [AC-005] AC#5 scope | Added GetCallName delegation test (int→CharacterId conversion via ICharacterDataAccess, then delegate to ICharacterDataService) to AC#5 description
- [resolved-applied] Phase2-Uncertain iter2: [CON-003] IEngineVariables.GetRandom returns int but delegates to IRandomProvider.Next(long) returning long. Long-to-int narrowing undocumented in interface contract. → Fixed: Concrete EngineVariables uses (int)_randomProvider.Next(max). ERA RAND values (max 32768) fit safely in int. Documented in concrete class XML comment.
- [fix] Phase2-Review iter3: [TSK-002] Tasks table | Added orphan AC mappings: Task 1→AC#1,10; Task 2→AC#2,6; Task 6 description updated for ICharacterDataService; Task 8→AC#13,14,15
- [fix] Phase2-Review iter4: [INV-003] NullCsvNameResolver Approach text | Changed 'Returns empty string or index-based placeholder' to match code blocks ('Returns empty strings for all CSV name lookups')
- [fix] Phase2-Review iter4: [AC-005] Implementation Contract Delegation Tests | Added GetCallName_DelegatesToICharacterDataService test method to match AC#5 scope
- [resolved-applied] Phase2-Uncertain iter4: [FMT-001] AC Design Constraints Details incomplete — C7, C9, C10, C11, C12 lack Details entries (8/13 documented)
- [fix] Phase2-Review iter5: [AC-005] Goal + Philosophy Derivation | Added ICharacterDataService for CALLNAME to Goal delegation list and Philosophy Derivation absolute claim, aligning with Problem section and AC#5 scope
- [resolved-applied] Phase2-Loop iter6: [SCP-003] ShopSystem ICsvNameResolver injection — reviewer raised 3 times (iter4,5,6), validator rejected twice (Zero Debt Upfront). User decision needed: keep (forward-looking) or remove (ISP strict). → Fixed: Removed. ShopSystem has zero CSV name stubs. ISP-strict: inject only dependencies consumed by current code. Downstream features add ICsvNameResolver to ShopSystem when needed.
- [fix] Phase2-Review iter6: [AC-005] Goal Coverage Verification Item 3 | Added ICharacterDataService for CALLNAME to delegation list, aligning with Goal and Philosophy Derivation updates from iter5
- [fix] Phase2-Review iter7: [DEP-002] Dependencies + Related Features | F789 status synced from [DRAFT] to [PROPOSED]
- [fix] Phase2-Review iter8: [AC-005] Philosophy Derivation rows | Added AC#9 to stub replacement rows (17 total = 5+8+4, AC#9 covers 4 CSV stubs)
- [resolved-applied] Phase1-RefCheck iter1: [DEP-002] F628 referenced in Links and body (Key Decisions, Upstream Issues) but feature-628.md does not exist. Either create feature-628.md or update references to point to actual source. → Fixed: Removed F628 from Links section. Body references retained as documentation-only (reference actual interface code, not feature file).
- [fix] Phase2-Review iter1: [FMT-001] AC Design Constraints Details | Added missing Details entries for C7, C9, C10, C11, C12 (now 13/13 documented)
- [resolved-applied] Phase2-Pending iter2: [CON-003] GetCallName delegation ignores Result<string> unwrapping. ICharacterDataService.GetCallName(CharacterId) returns Result<string> but IEngineVariables.GetCallName returns string. Technical Design, AC#5, and Implementation Contract need failure handling specification (e.g., return empty string on Failure). → Fixed: Concrete EngineVariables uses .Match(v => v, _ => string.Empty) pattern matching established GetCharacterFlag helper pattern in ShopDisplay.cs:444.
- [resolved-skipped] Phase2-Uncertain iter2: [SCP-003] Concrete EngineVariables scalar data source (RESULT, MONEY, DAY, MASTER, ASSI, COUNT via DataIntegerArray[code][0]) untracked. No Mandatory Handoff for engine adapter wiring. May follow existing convention (NullCharacterDataAccess precedent) or need explicit tracking. → Follows established NullXxx convention. Era.Core provides null implementation; engine project provides concrete adapter (like HeadlessCharacterDataAccess). No Mandatory Handoff needed.
- [fix] Phase3-Maintainability iter2: [CON-003] NullEngineVariables + NullCsvNameResolver | Added /// <inheritdoc /> to all methods (11+4) in both Technical Design and Implementation Contract code blocks for TreatWarningsAsErrors compliance
- [resolved-applied] Phase3-Maintainability iter2: [CON-003] GetName(int characterIndex) has no delegation target or documented data access path. NullEngineVariables returns empty string but concrete implementation path undefined. Key Decisions chose IEngineVariables-only but no data source specified. → Fixed: GetName uses same delegation mechanism as GetCallName (index→CharacterId conversion + ICharacterDataService). ICharacterDataService currently lacks GetName — concrete EngineVariables accesses runtime NAME via engine adapter (same deferred pattern as scalar variables).
- [resolved-skipped] Phase3-Maintainability iter2: [TSK-002] Task 7 covers all 17 stub replacements across 2 files — too coarse-grained. Consider splitting into Task 7a (ShopSystem: constructor + 5 stubs) and Task 7b (ShopDisplay: constructor + 12 stubs). → Mechanical replacements sharing same approach. AC#7,8,9 already provide file-level verification granularity. Single task appropriate.
- [resolved-skipped] Phase3-Maintainability iter2: [EXT-001] ICsvNameResolver limited to 4 hardcoded methods. ERA engine has additional CSV name arrays (TRAINNAME, STAINNAME, etc.). Current design requires new method per array, violating OCP. Consider generic method pattern or document rationale. → ISP principle: design for current consumers (ShopDisplay needs 4 arrays). Future features extend interface when consuming TRAINNAME etc. Generic method adds complexity without current benefit.
- [fix] Phase2-Review iter3: [CON-003] Implementation Contract IEngineVariables | Removed unused `using Era.Core.Types;` to match Technical Design and avoid CS8019 build failure
- [fix] Phase2-Review iter3: [AC-005] Philosophy Derivation row 3 | Changed from SSOT→stub replacement (duplicate of row 6) to SSOT→Phase 20 scope grounding (AC#1,2,3,4)
- [resolved-applied] Phase2-Uncertain iter3: [CON-003] COUNT semantic risk (ShopDisplay LifeList line 50) documented in Risks table but no [pending] Review Note ensures verification during implementation. → Fixed: Added implementation verification note. ShopDisplay:50 GetCount() usage in LifeList assistant-select loop likely should be chr (loop variable) not COUNT. Implementer must verify against ERB original during Task 7 stub replacement.
- [fix] Phase2-Review iter4: [FMT-001] Upstream Issues section | Converted free-text to template table format; added Cross-Feature Dependencies table
- [fix] Phase2-Review iter4: [AC-005] AC#7 Details | Added C13 Scope Boundary note documenting PrintDateMoneyStatus partial resolution per C13 constraint
- [resolved-skipped] Phase3-Maintainability iter5: [ISP-001] IEngineVariables conflates 3 responsibilities (global scalars, delegations, character-scoped). Key Decisions chose Option B over C, but reviewer argues ISP principle used for IEngineVariables/ICsvNameResolver split should apply internally too. User decision: keep 11-method interface or further split. → Key Decisions evaluated Option C (3 interfaces) vs B (2 interfaces) with documented rationale. All 11 methods serve same consumer pattern (ERA engine variable access). Further splitting creates micro-interfaces every consumer needs together, increasing boilerplate without practical benefit.
- [resolved-skipped] Phase3-Maintainability iter5: [INV-003] Stub replacement access pattern inconsistency. Existing ShopSystem private helpers (GetCharacterFlag, GetTalent) wrap IVariableStore, but F790 IEngineVariables calls bypass wrappers (_engineVars.GetResult() directly). Document rationale or add matching wrappers. → Pattern difference is correct. IVariableStore returns Result<T> requiring .Match() unwrapping (failable per-character operations). IEngineVariables returns raw values because failure is handled inside the implementation (NullEngineVariables returns safe defaults). Different contracts warrant different wrapper patterns.
- [fix] Phase3-Maintainability iter5: [INV-003] Delegation Tests | Added Moq framework specification to Implementation Contract (already available in Era.Core.Tests)
- [fix] Phase2-Review iter6: [FMT-001] Task Tags subsection | Added missing template-required Task Tags subsection after Tasks table
- [resolved-applied] Phase2-Uncertain iter6: [CON-003] CharacterId? nullable from GetCharacterByIndex unhandled for character-scoped delegation methods. Subordinate to [AC-006] concrete class pending but null-handling contract needed. → Fixed: Concrete EngineVariables null-handles CharacterId?: returns string.Empty for string methods, 0 for int methods when GetCharacterByIndex returns null. Matches NullEngineVariables default behavior.
- [fix] Phase3-Maintainability iter7: [CON-003] NullEngineVariables + NullCsvNameResolver | Changed `public class` to `internal sealed class` in Technical Design and Implementation Contract (4 occurrences) to match NullCharacterDataAccess/NullCharacterDataService pattern
- [fix] Phase4-ACValidation iter8: [AC-002] AC#10 | Separated pattern from count in count_equals matcher — moved `int characterIndex` to Method column, Expected=3
- [fix] Phase3-Maintainability iter1: [CON-003] EngineVariables GetName/GetIsAssi | Replaced ambiguous "deferred to engine adapter" comments with explicit "Preparatory scaffolding" comments explaining index→CharacterId conversion matches GetCallName pattern, in both Technical Design and Implementation Contract
- [fix] Phase2-Review iter2: [AC-005] AC#5 scope | Expanded to cover GetName/GetIsAssi scaffolding verification (safe default returns after index→CharacterId conversion). Added 2 test methods to Implementation Contract delegation tests.
- [fix] Phase2-Uncertain iter2: [FMT-001] Goal text | Clarified "17 F790-scoped" count breakdown (5+8+4) and PrintDateMoneyStatus cross-feature exclusion per C13
- [fix] Phase2-Review iter2: [AC-002] AC#14 scan path | Added Era.Core/Interfaces/EngineVariables.cs to debt scan (concrete class is F790 deliverable)
- [fix] Phase2-Review iter3: [AC-005] AC#5 rationale | Added contract-only verification clarification (unit tests with mocks, not integration)
- [fix] Phase2-Review iter3: [AC-002] AC#7,8,9,14 patterns | Changed \\| to | for ripgrep alternation syntax (\\| is literal in ripgrep, making ACs vacuously true)
- [fix] Phase3-Maintainability iter4: [CON-003] EngineVariables using directives | Added using Era.Core.Characters/Functions/Random/Types to concrete EngineVariables code block in both Technical Design and Implementation Contract
- [fix] Phase2-Review iter5: [AC-006] InternalsVisibleTo | Added Task 6 (InternalsVisibleTo for Era.Core.Tests) as prerequisite for Task 7 delegation tests. Added InternalsVisibleTo section to Implementation Contract. Renumbered Tasks 6→7, 7→8, 8→9.
- [info] Phase1-DriftChecked: F788 (Related)
- [resolved-skipped] Phase2-Pending iter1: [SCP-003] Scalar methods (GetResult, GetMoney, GetDay, GetMaster, GetAssi, GetCount) return 0 in both NullEngineVariables and concrete EngineVariables. DI registers NullEngineVariables. No Mandatory Handoff tracks engine adapter wiring. Previously resolved-skipped (iter2) as NullXxx convention, but re-raised: 'Track What You Skip' may require explicit destination for downstream F775-F778 consumption path. → NullXxxコンベンションに従いスキップ。エンジンプロジェクトがconcrete adapterを提供する確立パターン。
- [fix] Phase2-Review iter1: [AC-005] GetName/GetIsAssi scaffolding tests | Added mock.Verify(GetCharacterByIndex) calls to Implementation Contract test pseudocode and AC#5 Details to prove conversion code path executes

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning
- [Predecessor: F774](feature-774.md) - Shop Core (Mandatory Handoff origin)
- [Related: F788](feature-788.md) - IConsoleOutput Phase 20 Extensions
- [Related: F789](feature-789.md) - IVariableStore Phase 20 Extensions
- [Related: F791](feature-791.md) - Engine State Transitions & Entry Point Routing
- [Successor: F775](feature-775.md) - Collection
- [Successor: F776](feature-776.md) - Items
- [Successor: F777](feature-777.md) - Customization
- [Successor: F778](feature-778.md) - Body Initialization
- [Successor: F782](feature-782.md) - Post-Phase Review
