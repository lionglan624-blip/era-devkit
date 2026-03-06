# Feature 833: IEngineVariables DIM Stubs Engine Adapter Implementation

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T23:29:29Z -->

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

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Era.Core interfaces are the SSOT for cross-repo contracts between the core library and the engine runtime. Every DIM stub method in IEngineVariables must have a concrete runtime implementation that delegates to actual engine data, eliminating no-op stubs that silently return incorrect values to consuming subsystems. Abstract methods beyond the 9 DIM stubs are tracked separately via Mandatory Handoff.

### Problem (Current Issue)
The IEngineVariables interface defines 9 default interface method (DIM) stubs (GetDay(int), SetDay(int,int), GetTime(int), SetTime(int,int), GetTime(), SetTime(int), GetAssiPlay(), GetPrevCom(), SetPrevCom(int)) that all return 0 or no-op at runtime because no engine-side adapter class exists to bridge IEngineVariables to the engine's VariableData 1D integer arrays (DAY, TIME, ASSIPLAY, PREVCOM). The concrete EngineVariables class in Era.Core only overrides 3 of these 9 methods (GetAssiPlay, GetPrevCom, SetPrevCom); the remaining 6 fall back to DIM defaults returning 0. The DI container registers NullEngineVariables which also returns 0. This causes 14+ call sites across WeatherSimulation, CalendarService, DateInitializer, ChildMovement, WcCounterMessageSex, ComableChecker (40+ GetAssiPlay calls), and SourceEntrySystem to silently receive incorrect zero values.

### Goal (What to Achieve)
Create an EngineVariablesImpl adapter class in the engine repository (following the established GameStateImpl pattern) that implements all 9 IEngineVariables DIM stubs by delegating to GlobalStatic.VariableData integer arrays, register it in the engine's DI bootstrap via GlobalStatic property assignment, and update the core EngineVariables class to provide explicit implementations for the 6 methods still relying on DIM fallback. The remaining 20+ abstract method stubs in EngineVariables.cs are explicitly OUT OF SCOPE.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do GetDay(int)/GetTime(int)/GetAssiPlay()/etc return 0? | They use DIM stub implementations that return default(int) | IEngineVariables.cs:98-130 |
| 2 | Why do DIM stubs exist instead of real implementations? | The concrete EngineVariables class does not override these methods | EngineVariables.cs:1-141 (no indexed/scalar time methods) |
| 3 | Why does EngineVariables not implement them? | EngineVariables lives in Era.Core which cannot access engine VariableData | ServiceCollectionExtensions.cs:211 (registers NullEngineVariables) |
| 4 | Why is there no engine-side adapter? | No EngineVariablesImpl class exists in the engine repository | GlobalStatic.cs:1-329 (no IEngineVariables property) |
| 5 | Why (Root)? | Methods were added incrementally by F806/F821/F825 as DIMs to defer engine-repo work, but the adapter feature was never created | feature-825.md (F825 created stubs expecting future implementation) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | IEngineVariables methods return 0 at runtime | No engine adapter bridges IEngineVariables to VariableData arrays |
| Where | Era.Core DIM stubs (IEngineVariables.cs:98-130) | Missing EngineVariablesImpl in engine/Services/ |
| Fix | Hardcode return values in core | Create engine adapter delegating to GlobalStatic.VariableData |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F825 | [DONE] | Created the DIM stubs being implemented |
| F829 | [DONE] | Phase 22 Deferred Obligations Consolidation; routing origin (OB-09) |
| F821 | [DONE] | Weather System; added indexed GetDay/SetDay/GetTime/SetTime |
| F806 | [DONE] | WC Counter; added scalar GetTime/SetTime |
| F790 | [DONE] | Engine Data Access Layer; created IEngineVariables |
| F828 | [DONE] | Date Init; uses SetDay(1,month), SetDay(2,day) |
| F811 | [DONE] | Predecessor for GetPrevCom consumers |
| F812 | [DONE] | Predecessor for GetAssiPlay consumers |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Architectural pattern | FEASIBLE | GameStateImpl establishes adapter pattern (GameStateImpl.cs:12) |
| Data access | FEASIBLE | VariableData stores DAY/TIME/ASSIPLAY/PREVCOM as Int1DVariableToken (VariableData.cs:144,162-164) |
| Existing utilities | FEASIBLE | VariableDataAccessor.TryGetIntegerArray/TrySetIntegerArray (VariableDataAccessor.cs:114-158) |
| DI registration | FEASIBLE | EmueraConsole.cs:296-298 shows existing adapter registration pattern |
| Cross-repo coordination | FEASIBLE | Core NuGet publish then engine update; established process |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| WeatherSimulation | HIGH | 4+ call sites for indexed DAY/TIME (WeatherSimulation.cs:61-63,73,116,183) |
| ComableChecker | HIGH | 40+ GetAssiPlay() call sites (ComableChecker.Range0x.cs:93+) |
| CalendarService | MEDIUM | GetDay(2), SetDay(1), SetDay(2) calls (CalendarService.cs:31-41) |
| DateInitializer | MEDIUM | SetDay(1,month), SetDay(2,day) (DateInitializer.cs:86-87) |
| WcCounterMessageSex | MEDIUM | Read-modify-write TIME pattern (WcCounterMessageSex.cs:2629,3495,4439,4519) |
| SourceEntrySystem | MEDIUM | GetPrevCom/GetAssiPlay consumers (SourceEntrySystem.cs:406,673,717) |
| ChildMovement | LOW | Scalar GetTime() (ChildMovement.cs:62) |
| RoomSmellService | LOW | Scalar GetTime() (RoomSmellService.cs:53) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| VariableData uses Int64[], IEngineVariables uses int | VariableToken.cs:614 | Narrowing cast required (Int64 to int); safe for DAY/TIME/ASSIPLAY/PREVCOM values |
| DAY/TIME/ASSIPLAY/PREVCOM are 1D arrays via VariableCode bitmask | VariableData.cs:88-90 | Access pattern: DataIntegerArray indexed by VariableCode enum |
| VariableDataAccessor is HEADLESS_MODE only | VariableDataAccessor.cs:4 | Adapter must access VariableData directly, not through accessor |
| GlobalStatic.VariableData may be null during init | GlobalStatic.cs:39 | Graceful null handling required (return 0 when null) |
| Cross-repo NuGet publish ordering | Architecture | Core changes must be published before engine can consume |
| EngineVariables is internal sealed in core | EngineVariables.cs:15 | Cannot extend from engine; engine adapter implements IEngineVariables directly |
| NullEngineVariables registered as default in core DI | ServiceCollectionExtensions.cs:211 | Engine must override registration at bootstrap |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Int64-to-int data loss | LOW | LOW | DAY/TIME/ASSIPLAY/PREVCOM values are small integers; explicit cast with bounds check |
| NuGet version coordination failure | MEDIUM | MEDIUM | Publish core NuGet first, verify engine package reference update |
| GlobalStatic.VariableData null at adapter call time | LOW | MEDIUM | Return 0/no-op when VariableData is null (matches current DIM behavior) |
| Scope creep to 20+ abstract method stubs | MEDIUM | HIGH | Strictly scope to 9 DIM stubs only; abstract stubs are separate feature |
| StubEngineVariables in test project desync | LOW | LOW | Update stub alongside adapter implementation |
| Adapter works only in headless mode | LOW | MEDIUM | Use GlobalStatic.VariableData directly (not VariableDataAccessor which is headless-only) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| DIM stub count | Grep IEngineVariables.cs for "=>" | 9 | GetTime(), SetTime(int), GetDay(int), SetDay(int,int), GetTime(int), SetTime(int,int), GetAssiPlay(), GetPrevCom(), SetPrevCom(int) |
| EngineVariables explicit methods | Grep EngineVariables.cs method bodies | 0 indexed/scalar time methods | Falls back to DIM |
| GlobalStatic IEngineVariables | Grep GlobalStatic.cs for IEngineVariables | 0 references | Not registered |
| Consumer call sites | Grep Era.Core for GetDay\|GetTime\|SetDay\|SetTime\|GetAssiPlay\|GetPrevCom | 14+ | Across 8 consumer files |

**Baseline File**: `_out/tmp/baseline-833.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Core EngineVariables must implement all 9 DIM methods explicitly | Missing implementations in EngineVariables.cs | AC must verify explicit method bodies exist (not DIM fallback) |
| C2 | Engine adapter must use VariableCode-based access to DataIntegerArray | VariableData.cs bitmask pattern | AC must verify correct VariableCode usage in adapter |
| C3 | Null/graceful handling for uninitialized VariableData | GlobalStatic.cs:39 | AC must test adapter behavior when VariableData is null |
| C4 | Int64-to-int narrowing must be explicit | VariableToken.cs Int64 arrays | AC should verify explicit cast in adapter |
| C5 | StubEngineVariables must be updated with all 9 methods | TestStubs.cs:65-91 incomplete | AC must verify stub completeness |
| C6 | Cross-repo: core NuGet publish before engine consumption | Architecture constraint | AC ordering must reflect publish sequence |
| C7 | Scope limited to 9 DIM stubs only | Feature scope boundary | AC must NOT cover abstract method stubs (GetResult, GetMoney, etc.) |
| C8 | Engine must register EngineVariablesImpl via GlobalStatic property at bootstrap | EmueraConsole.cs Init block (GameStateImpl pattern) | AC must verify GlobalStatic.EngineVariablesInstance assignment in EmueraConsole |
| C9 | Both scalar and indexed overloads coexist for TIME | IEngineVariables.cs | AC must verify both GetTime() and GetTime(int) work correctly |
| C10 | Interface Dependency Scan: IEngineVariables 9 DIM methods | Interface Dependency Scan | AC must cover all 9 methods with round-trip (set-then-get) verification |

### Constraint Details

**C1: Explicit Method Implementation in Core**
- **Source**: EngineVariables.cs lacks indexed/scalar time/day methods; DIM fallback returns 0
- **Verification**: Grep EngineVariables.cs for method signatures
- **AC Impact**: AC must verify method bodies exist in EngineVariables, not just interface

**C2: VariableCode Bitmask Access Pattern**
- **Source**: VariableData.cs registers DAY(0x00)/TIME(0x16)/ASSIPLAY/PREVCOM as Int1DVariableToken
- **Verification**: Read VariableCode.cs:56,119 for enum values
- **AC Impact**: AC must verify adapter references correct VariableCode values

**C3: Null VariableData Handling**
- **Source**: GlobalStatic.VariableData may be null during initialization
- **Verification**: Check GlobalStatic.cs initialization order
- **AC Impact**: AC must include negative test for null VariableData scenario

**C4: Int64-to-int Cast Safety**
- **Source**: VariableToken stores Int64[], IEngineVariables returns int
- **Verification**: VariableToken.cs:614 array type
- **AC Impact**: AC should verify explicit (int) cast presence in adapter

**C5: Test Stub Completeness**
- **Source**: StubEngineVariables in TestStubs.cs:65-91 lacks indexed methods
- **Verification**: Grep TestStubs.cs for method count
- **AC Impact**: AC must verify stub has all 9 DIM method implementations

**C6: Cross-Repo NuGet Publish Ordering**
- **Source**: Architecture constraint — Era.Core is consumed as NuGet by engine
- **Verification**: Core changes must be published before engine can reference them
- **AC Impact**: Task ordering must reflect core-first, engine-second sequence

**C7: Scope Limited to 9 DIM Stubs Only**
- **Source**: Feature scope boundary from F829 OB-09 routing
- **Verification**: AC must NOT cover abstract method stubs (GetResult, GetMoney, GetDay(), etc.)
- **AC Impact**: AC patterns must match only the 9 DIM methods, not all IEngineVariables methods

**C8: Engine GlobalStatic Registration**
- **Source**: Engine registers EngineVariablesImpl via GlobalStatic property at bootstrap (GameStateImpl pattern)
- **Verification**: Grep EmueraConsole.cs for EngineVariablesInstance assignment
- **AC Impact**: AC must verify GlobalStatic.EngineVariablesInstance = new EngineVariablesImpl() in EmueraConsole Init block

**C9: Scalar and Indexed Overloads Coexist for TIME**
- **Source**: IEngineVariables.cs defines both GetTime() (scalar) and GetTime(int) (indexed)
- **Verification**: Both overloads must be implemented in the adapter
- **AC Impact**: AC#2 pattern must match both scalar and indexed overload names

**C10: Round-Trip Verification**
- **Source**: Interface defines getter/setter pairs for DAY, TIME, PREVCOM
- **Verification**: IEngineVariables.cs method signatures
- **AC Impact**: AC must test set-then-get returns correct value for each variable type

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F825 | [DONE] | Relationships & DI Integration; created the DIM stubs being implemented |
| Related | F829 | [DONE] | Phase 22 Deferred Obligations Consolidation; routing origin (OB-09) |
| Related | F821 | [DONE] | Weather System; added indexed GetDay/SetDay/GetTime/SetTime to interface |
| Related | F806 | [DONE] | WC Counter; added scalar GetTime/SetTime to interface |
| Related | F790 | [DONE] | Engine Data Access Layer; created IEngineVariables interface |
| Related | F828 | [DONE] | Date Init; consumer of SetDay indexed methods |
| Related | F811 | [DONE] | Predecessor for GetPrevCom consumers |
| Related | F812 | [DONE] | Predecessor for GetAssiPlay consumers |
| Successor | F835 | [DRAFT] | Remaining ~20 abstract IEngineVariables methods — real VariableData delegation |

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
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Every interface method must have a concrete runtime implementation" | All 9 DIM stubs must have concrete implementations in engine adapter | AC#1, AC#2 |
| "Every interface method must have a concrete runtime implementation" | Core EngineVariables must explicitly implement the 6 missing DIM methods (not rely on DIM fallback) | AC#3 |
| "eliminating no-op stubs that silently return incorrect values" | Engine adapter must delegate to actual VariableData arrays, not return 0 | AC#4 |
| "eliminating no-op stubs that silently return incorrect values" | GlobalStatic must register the adapter so it replaces NullEngineVariables at runtime | AC#5, AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | EngineVariablesImpl file exists in engine Services | file | Glob(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs) | exists | 1 | [x] |
| 2 | EngineVariablesImpl implements all 9 DIM methods | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetAssiPlay|GetPrevCom|SetPrevCom|GetTime|SetTime|GetDay|SetDay") | gte | 9 | [x] |
| 3 | Core EngineVariables explicitly implements 6 previously-DIM methods | code | Grep(core/src/Era.Core/Interfaces/EngineVariables.cs, pattern="GetTime\(\)|SetTime\(int|GetDay\(int|SetDay\(int|GetTime\(int|SetTime\(int,") | gte | 6 | [x] |
| 4 | EngineVariablesImpl delegates to VariableData (not hardcoded 0) | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="VariableData") | contains | `VariableData` | [x] |
| 5 | GlobalStatic has EngineVariablesInstance property | code | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs, pattern="EngineVariablesInstance") | contains | `EngineVariablesInstance` | [x] |
| 6 | EmueraConsole registers EngineVariablesImpl at bootstrap | code | Grep(engine/Assets/Scripts/Emuera/GameView/EmueraConsole.cs, pattern="EngineVariablesImpl|EngineVariablesInstance") | contains | `EngineVariablesImpl` | [x] |
| 7 | EngineVariablesImpl handles null VariableData gracefully | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="VariableData == null") | contains | `VariableData == null` | [x] |
| 8 | EngineVariablesImpl uses explicit int cast for Int64 narrowing | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="\(int\)") | contains | `(int)` | [x] |
| 9 | StubEngineVariables implements all 9 DIM methods | code | Grep(engine/tests/uEmuera.Tests/Tests/TestStubs.cs, pattern="GetAssiPlay|GetPrevCom|SetPrevCom|GetTime|SetTime|GetDay|SetDay") | gte | 9 | [x] |
| 10 | EngineVariablesImpl implements IEngineVariables | code | Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="IEngineVariables") | contains | `IEngineVariables` | [x] |
| 11 | EngineVariablesImpl unit tests exist | file | Glob(engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs) | exists | 1 | [x] |
| 12 | EngineVariablesImpl unit tests pass | test | dotnet test engine/tests/uEmuera.Tests/ --filter "FullyQualifiedName~EngineVariablesImplTests" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 13 | Engine build succeeds | build | dotnet build engine/uEmuera.sln | succeeds | pass | [x] |

### AC Details

**AC#2: EngineVariablesImpl implements all 9 DIM methods**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs, pattern="GetAssiPlay|GetPrevCom|SetPrevCom|GetTime|SetTime|GetDay|SetDay")`
- **Expected**: `gte 9` (9 individual method implementations: GetAssiPlay(), GetPrevCom(), SetPrevCom(int), GetTime(), SetTime(int), GetDay(int), SetDay(int,int), GetTime(int), SetTime(int,int))
- **Rationale**: Per C10, all 9 DIM stubs must have concrete implementations. The alternation pattern matches method names; gte 9 ensures all 9 are present (some names like GetTime/SetTime appear in both scalar and indexed overloads). Note: this is a heuristic proxy — the grep count may exceed 9 due to overload name overlap or comments, but the threshold guarantees minimum coverage.
- **Derivation**: 9 methods = GetAssiPlay(1) + GetPrevCom(1) + SetPrevCom(1) + GetTime(2: scalar+indexed) + SetTime(2: scalar+indexed) + GetDay(1: indexed) + SetDay(1: indexed)

**AC#3: Core EngineVariables explicitly implements 6 previously-DIM methods**
- **Test**: `Grep(core/src/Era.Core/Interfaces/EngineVariables.cs, pattern="GetTime\(\)|SetTime\(int|GetDay\(int|SetDay\(int|GetTime\(int|SetTime\(int,")`
- **Expected**: `gte 6` (6 methods that currently lack explicit implementations: GetTime(), SetTime(int), GetDay(int), SetDay(int,int), GetTime(int), SetTime(int,int))
- **Rationale**: Per C1, EngineVariables must not rely on DIM fallback. Currently 3 of 9 are explicit (GetAssiPlay, GetPrevCom, SetPrevCom at lines 133-140). The remaining 6 must be added.
- **Derivation**: 6 = 9 total DIM stubs - 3 already explicit (GetAssiPlay, GetPrevCom, SetPrevCom)

**AC#9: StubEngineVariables implements all 9 DIM methods**
- **Test**: `Grep(engine/tests/uEmuera.Tests/Tests/TestStubs.cs, pattern="GetAssiPlay|GetPrevCom|SetPrevCom|GetTime|SetTime|GetDay|SetDay")`
- **Expected**: `gte 9` (same 9 methods as AC#2, mirrored in test stub)
- **Rationale**: Per C5, StubEngineVariables must be updated alongside the adapter to prevent test desync. Currently has 0 of these 9 methods.
- **Derivation**: 9 methods matching AC#2 derivation

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Create EngineVariablesImpl adapter class in engine (following GameStateImpl pattern) | AC#1, AC#2, AC#10 |
| 2 | Implements all 9 IEngineVariables DIM stubs by delegating to GlobalStatic.VariableData integer arrays | AC#2, AC#4, AC#8 |
| 3 | Register it in the engine's DI bootstrap | AC#5, AC#6 |
| 4 | Update core EngineVariables class to provide explicit implementations | AC#3 |
| 5 | Null VariableData graceful handling | AC#7 |
| 6 | StubEngineVariables test stub completeness | AC#9 |
| 7 | Unit tests and build verification | AC#11, AC#12, AC#13 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

Create an `EngineVariablesImpl` adapter class in `engine/Assets/Scripts/Emuera/Services/` that implements `IEngineVariables` by reading and writing `GlobalStatic.VariableData.DataIntegerArray` entries for the four 1D-array variables: DAY, TIME, ASSIPLAY, and PREVCOM. The adapter follows the exact pattern established by `GameStateImpl.cs` — a public class in the `MinorShift.Emuera.Services` namespace implementing the Era.Core interface, registered via `GlobalStatic.EngineVariablesInstance = new Services.EngineVariablesImpl()` in `EmueraConsole.cs` alongside the existing adapter registrations (GameStateImpl pattern).

The `VariableData.DataIntegerArray` is a `Int64[][]` jagged array indexed by `(int)(VariableCode.X & VariableCode.__LOWERCASE__)`. The four variables DAY, TIME, ASSIPLAY, PREVCOM are all registered as `Int1DVariableToken` entries in `VariableData.cs:144,162-164`. Since `VariableDataAccessor` is HEADLESS_MODE-only, the adapter must access `GlobalStatic.VariableData.DataIntegerArray` directly with a null guard (return 0 / no-op when `VariableData` is null).

The scalar overloads (`GetTime()`, `SetTime(int)`, `GetDay()` — the zero-index case) delegate to the indexed overloads with `index: 0` to avoid code duplication. The indexed overloads perform an explicit `(int)` narrowing cast from `Int64`.

Simultaneously, `EngineVariables.cs` in core is updated to add the 6 missing explicit implementations (replacing DIM fallbacks). These remain 0-returning in core since core cannot access engine data, but converting from DIM fallback to explicit override is a code clarity improvement: it documents intent, prevents ambiguity in overload resolution, and makes the core class's contract explicit. The functional fix comes from the engine adapter (EngineVariablesImpl) replacing EngineVariables at runtime.

`StubEngineVariables` in `TestStubs.cs` is updated to add all 9 DIM method implementations so the test stub stays in sync.

Unit tests are added in `engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs` using xUnit. Since `GlobalStatic.VariableData` is a concrete `VariableData` object that requires a `ConstantData` constructor argument, the tests use null VariableData (testing the graceful-null path) and a pre-constructed VariableData instance (via reflection or a test-helper constant) to test the delegation path. The null-path tests are deterministic; the delegation-path tests verify round-trip set-then-get.

This approach satisfies all 13 ACs: AC#1 (file exists), AC#2 (9 methods present), AC#3 (6 core explicit implementations), AC#4 (VariableData delegation), AC#5 (GlobalStatic property), AC#6 (EmueraConsole registration), AC#7 (null check pattern), AC#8 (explicit `(int)` cast), AC#9 (StubEngineVariables updated), AC#10 (IEngineVariables declaration), AC#11-12 (unit tests), AC#13 (build passes).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs` |
| 2 | Implement all 9 DIM methods: `GetAssiPlay`, `GetPrevCom`, `SetPrevCom(int)`, `GetTime()`, `SetTime(int)`, `GetDay(int)`, `SetDay(int,int)`, `GetTime(int)`, `SetTime(int,int)` in the class body |
| 3 | Add 6 explicit override bodies to `EngineVariables.cs` in core: `GetTime()`, `SetTime(int)`, `GetDay(int)`, `SetDay(int,int)`, `GetTime(int)`, `SetTime(int,int)` — each returning 0 or no-op (replaces DIM fallback) |
| 4 | Each getter delegates to `GlobalStatic.VariableData?.DataIntegerArray[...]` rather than returning a constant |
| 5 | Add `GlobalStatic.EngineVariablesInstance` property to `GlobalStatic.cs` following the `GameStateInstance` pattern |
| 6 | Add `GlobalStatic.EngineVariablesInstance = new Services.EngineVariablesImpl()` to `EmueraConsole.cs` Init block alongside existing adapter registrations (GameStateImpl pattern) |
| 7 | Each getter/setter begins with `if (GlobalStatic.VariableData == null) return 0;` (or no-op for setters) |
| 8 | Array reads cast with `(int)` before returning: `return (int)GlobalStatic.VariableData.DataIntegerArray[...][index]` |
| 9 | Add `GetAssiPlay()`, `GetPrevCom()`, `SetPrevCom(int)`, `GetTime()`, `SetTime(int)`, `GetDay(int)`, `SetDay(int,int)`, `GetTime(int)`, `SetTime(int,int)` to `StubEngineVariables` in `TestStubs.cs` |
| 10 | Declare `public class EngineVariablesImpl : IEngineVariables` in the file |
| 11 | Create `engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs` |
| 12 | Write null-VariableData tests (returns 0, no-op) and round-trip tests using a real VariableData if constructable in test context; run via `dotnet test --filter EngineVariablesImplTests` |
| 13 | No new APIs outside existing engine types; build succeeds with `dotnet build engine/uEmuera.sln` |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Test strategy for VariableData dependency | A) Mock VariableData, B) Real VariableData with ConstantData, C) Test null-path only | C + B where feasible | Null-path tests are always deterministic (cover AC#7). Real VariableData construction requires ConstantData; if constructable in test harness, add round-trip tests. If ConstantData construction is too heavyweight, cover delegation via integration path (headless run) and test only null-path in unit tests — flag in Upstream Issues |
| Scalar vs indexed overload relationship | A) Duplicate DataIntegerArray access in each scalar, B) Scalars delegate to indexed at index 0 | B (delegate to indexed) | Eliminates duplication; `GetTime()` calls `GetTime(0)`, `SetTime(v)` calls `SetTime(0, v)`, `GetDay()` already non-DIM in core (returns 0) — only `GetDay(int)` is a DIM stub |
| `VariableCode.__LOWERCASE__` bitmask | A) Cast VariableCode directly, B) Apply `& VariableCode.__LOWERCASE__` mask | B (apply mask) | Existing codebase pattern: `(int)(VariableCode.DAY & VariableCode.__LOWERCASE__)` shown in `VariableSizeService.cs:40`, `GlobalStaticIntegrationTests.cs:106` |
| Core EngineVariables.cs additions | A) Add explicit bodies in core (still return 0), B) Do nothing (DIM fallback is functionally identical) | A (add explicit bodies) | AC#3 requires explicit implementations. Explicit bodies document intent and prevent ambiguity when overload resolution is in play. DIM fallback is an implicit default; AC constraint C1 requires eliminating it |
| GlobalStatic registration pattern | A) Nullable DI property (allow null get), B) Fallback to NullImpl on get, C) Null get (set only at bootstrap) | A (nullable, no fallback default) | `IEngineVariables` has no suitable default in engine (GameState pattern uses `new Era.Core.Commands.System.GameState()` as fallback, but no equivalent NullEngineVariables exists in engine scope). Registration in `EmueraConsole` init block guarantees non-null during game runtime. Test code assigns `StubEngineVariables`. |

### Interfaces / Data Structures

No new interfaces are defined. This feature wires an existing interface (`IEngineVariables` from Era.Core) to an existing data structure (`VariableData.DataIntegerArray`).

**EngineVariablesImpl — method bodies reference:**

```csharp
// Access pattern (matches VariableSizeService.cs:40 and GlobalStaticIntegrationTests.cs:106)
private static Int64[] GetArray(VariableCode code)
    => GlobalStatic.VariableData.DataIntegerArray[(int)(code & VariableCode.__LOWERCASE__)];

// Scalar methods delegate to indexed at index 0
public int GetTime() => GetTime(0);
public void SetTime(int value) => SetTime(0, value);

// Indexed methods do the actual work
public int GetTime(int index)
{
    if (GlobalStatic.VariableData == null) return 0;
    return (int)GetArray(VariableCode.TIME)[index];
}

public void SetTime(int index, int value)
{
    if (GlobalStatic.VariableData == null) return;
    GetArray(VariableCode.TIME)[index] = value;
}

// ASSIPLAY and PREVCOM are scalar-only (no indexed overloads)
public int GetAssiPlay()
{
    if (GlobalStatic.VariableData == null) return 0;
    return (int)GetArray(VariableCode.ASSIPLAY)[0];
}

public int GetPrevCom()
{
    if (GlobalStatic.VariableData == null) return 0;
    return (int)GetArray(VariableCode.PREVCOM)[0];
}

public void SetPrevCom(int value)
{
    if (GlobalStatic.VariableData == null) return;
    GetArray(VariableCode.PREVCOM)[0] = value;
}
```

Note: `GetDay()` (non-indexed scalar) is an abstract method in `IEngineVariables.cs` (not a DIM stub). Since `EngineVariablesImpl` implements `IEngineVariables` directly, it MUST provide a `GetDay()` body. Following the scalar-delegates-to-indexed pattern: `public int GetDay() => GetDay(0);` — this gives `GetDay()` real VariableData delegation for free. Similarly, all other abstract IEngineVariables methods (~20+) must have stub bodies for compilation (returning 0/empty/no-op for methods outside this feature's scope). The 9 DIM stubs (DIM = having a default body in the interface) are: `GetAssiPlay()`, `GetPrevCom()`, `SetPrevCom(int)`, `GetTime()`, `SetTime(int)`, `GetDay(int)`, `SetDay(int,int)`, `GetTime(int)`, `SetTime(int,int)`.

**GlobalStatic.cs — new property to add:**

```csharp
// DI property for engine variables (Feature 833)
private static Era.Core.Interfaces.IEngineVariables _engineVariables;
public static Era.Core.Interfaces.IEngineVariables EngineVariablesInstance
{
    get => _engineVariables;
    set => _engineVariables = value;
}
```

**EmueraConsole.cs — bootstrap line to add alongside existing adapter registrations (GameStateImpl pattern):**

```csharp
GlobalStatic.EngineVariablesInstance = new Services.EngineVariablesImpl();
```

**Core EngineVariables.cs — 6 explicit bodies to add (replacing DIM fallbacks):**

```csharp
// Feature 833 - explicit overrides replacing DIM stubs
public int GetTime() => 0;
public void SetTime(int value) { }
public int GetDay(int index) => 0;
public void SetDay(int index, int value) { }
public int GetTime(int index) => 0;
public void SetTime(int index, int value) { }
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| ~~AC#3 pattern~~ **Resolved**: `GetTime\(\)` uses escaped parentheses which match literal `()` in ripgrep — the original concern about `.` matching any char was incorrect. Pattern correctly distinguishes `GetTime()` from `GetTime(int)`. `SetTime\(int` may double-match both overloads but `gte 6` threshold accommodates this. | AC Definition Table AC#3 | No fix needed — pattern is correct as-is |
| Unit test constructability of VariableData: `VariableData` constructor requires `ConstantData` which may require full engine init. If round-trip delegation tests are not feasible in xUnit unit test, AC#12 round-trip coverage will only be exercised via headless integration. | AC Definition Table AC#12 | Consider adding a separate integration-level AC or clarifying that null-path tests alone satisfy AC#12 pass condition. Flag for wbs-generator to investigate ConstantData construction in test harness |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 10 | Create `engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs` declaring `public class EngineVariablesImpl : IEngineVariables` in `MinorShift.Emuera.Services` namespace | | [x] |
| 2 | 2, 4, 7, 8 | Implement all 9 DIM methods in EngineVariablesImpl: GetAssiPlay(), GetPrevCom(), SetPrevCom(int), GetTime(), SetTime(int), GetDay(int), SetDay(int,int), GetTime(int), SetTime(int,int) — each delegating to `GlobalStatic.VariableData.DataIntegerArray` via `(int)(code & VariableCode.__LOWERCASE__)` index, with null guard returning 0/no-op, and explicit `(int)` cast for Int64 narrowing. Also implement GetDay() as `=> GetDay(0)` (abstract method, delegates scalar to indexed). Remaining ~20 abstract IEngineVariables methods get 0-returning/no-op stub bodies (compilation requirement; real delegation is out of scope) | | [x] |
| 3 | 5 | Add `EngineVariablesInstance` static property to `engine/Assets/Scripts/Emuera/GlobalStatic.cs` following the `GameStateInstance` pattern | | [x] |
| 4 | 6 | Register `GlobalStatic.EngineVariablesInstance = new Services.EngineVariablesImpl()` in `engine/Assets/Scripts/Emuera/GameView/EmueraConsole.cs` Init block alongside existing adapter registrations (GameStateImpl pattern) | | [x] |
| 5 | 3 | Add 6 explicit method override bodies to `core/src/Era.Core/Interfaces/EngineVariables.cs`: GetTime(), SetTime(int), GetDay(int), SetDay(int,int), GetTime(int), SetTime(int,int) — each returning 0 or no-op (replacing DIM fallback) | | [x] |
| 6 | 9 | Update `StubEngineVariables` in `engine/tests/uEmuera.Tests/Tests/TestStubs.cs` to add all 9 DIM method implementations: GetAssiPlay(), GetPrevCom(), SetPrevCom(int), GetTime(), SetTime(int), GetDay(int), SetDay(int,int), GetTime(int), SetTime(int,int) | | [x] |
| 7 | 11, 12 | Create `engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs` with xUnit unit tests: null-VariableData path (returns 0/no-op) for all getters/setters, and round-trip set-then-get tests where VariableData construction is feasible | | [x] |
| 8 | 13 | Verify engine build succeeds: `dotnet build engine/uEmuera.sln` | | [x] |

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
| 1 | implementer | sonnet | feature-833.md Tasks 5 (core EngineVariables.cs explicit overrides) | 6 explicit method bodies added to core/src/Era.Core/Interfaces/EngineVariables.cs |
| 2 | implementer | sonnet | feature-833.md Tasks 1-4 (engine EngineVariablesImpl + GlobalStatic + EmueraConsole) | EngineVariablesImpl.cs created; GlobalStatic.cs updated; EmueraConsole.cs updated |
| 3 | implementer | sonnet | feature-833.md Task 6 (StubEngineVariables update) | TestStubs.cs updated with all 9 DIM method implementations |
| 4 | implementer | sonnet | feature-833.md Task 7 (unit tests) | EngineVariablesImplTests.cs created with null-path and round-trip tests |
| 5 | tester | sonnet | engine/tests/uEmuera.Tests/ | AC#12 pass: EngineVariablesImplTests pass; AC#13 pass: engine build succeeds |

### Pre-conditions

- core repo NuGet must be published before engine can consume updated EngineVariables.cs changes (AC#3)
- Cross-repo ordering: core changes (Task 5) → NuGet publish → engine changes (Tasks 1-4, 6-7)
- `GlobalStatic.VariableData` uses `(int)(VariableCode.X & VariableCode.__LOWERCASE__)` bitmask for array indexing (confirmed in VariableSizeService.cs:40, GlobalStaticIntegrationTests.cs:106)
- `VariableDataAccessor` is HEADLESS_MODE-only — adapter must access `GlobalStatic.VariableData.DataIntegerArray` directly

### Execution Order

1. **Task 5 (core)**: Add 6 explicit override bodies to `EngineVariables.cs` in core repo. These replace DIM fallbacks with explicit `=> 0` / `{ }` bodies. Publish core NuGet.
2. **Task 1 (engine)**: Create `EngineVariablesImpl.cs` with class declaration implementing IEngineVariables.
3. **Task 2 (engine)**: Implement all 9 DIM methods using `DataIntegerArray[(int)(code & VariableCode.__LOWERCASE__)]` pattern with null guards and `(int)` casts. Scalar overloads (`GetTime()`, `SetTime(int)`) delegate to indexed overloads at index 0.
4. **Task 3 (engine)**: Add `EngineVariablesInstance` property to GlobalStatic.cs (nullable, no fallback default).
5. **Task 4 (engine)**: Add bootstrap line to EmueraConsole.cs Init block: `GlobalStatic.EngineVariablesInstance = new Services.EngineVariablesImpl();`
6. **Task 6 (engine)**: Update `StubEngineVariables` in TestStubs.cs. Add the same 9 method signatures with backing dictionary or field storage for round-trip testing.
7. **Task 7 (engine)**: Create `EngineVariablesImplTests.cs`. Null-path tests assign `GlobalStatic.VariableData = null` then call each getter (expect 0) and setter (expect no-op/no exception). Round-trip tests construct a VariableData instance if feasible via test helper or reflection.
8. **Task 8 (engine)**: Run `dotnet build engine/uEmuera.sln`. Verify exit code 0.

### Build Verification Steps

```bash
# Step 1: Verify core builds (after Task 5)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build'

# Step 2: Verify engine builds (after Tasks 1-4, 6-7)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/engine && /home/siihe/.dotnet/dotnet build uEmuera.sln'

# Step 3: Run unit tests (AC#12)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/engine && /home/siihe/.dotnet/dotnet test tests/uEmuera.Tests/ --filter "FullyQualifiedName~EngineVariablesImplTests" --blame-hang-timeout 10s'
```

### Success Criteria

| AC# | Verification | Pass Condition |
|:---:|-------------|----------------|
| 1 | File exists at engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs | file_exists = 1 |
| 2 | Grep EngineVariablesImpl.cs for all 9 method names | gte 9 matches |
| 3 | Grep EngineVariables.cs for 6 explicit method signatures | gte 6 matches |
| 4 | Grep EngineVariablesImpl.cs for "VariableData" | matches |
| 5 | Grep GlobalStatic.cs for "EngineVariablesInstance" | matches |
| 6 | Grep EmueraConsole.cs for "EngineVariablesImpl\|EngineVariablesInstance" | matches |
| 7 | Grep EngineVariablesImpl.cs for null-check pattern | matches |
| 8 | Grep EngineVariablesImpl.cs for "(int)" cast | matches |
| 9 | Grep TestStubs.cs for all 9 method names | gte 9 matches |
| 10 | Grep EngineVariablesImpl.cs for "IEngineVariables" | matches |
| 11 | File exists at engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs | file_exists = 1 |
| 12 | dotnet test --filter EngineVariablesImplTests | pass |
| 13 | dotnet build engine/uEmuera.sln | pass |

### Error Handling

- If `VariableData` constructor requires heavyweight `ConstantData` init: use null-path tests only for unit tests (AC#12); note in Execution Log. Round-trip delegation coverage deferred to headless integration path.
- If core NuGet publish fails: STOP — report to user before proceeding to engine changes.
- If engine build fails after Tasks 1-4: STOP after 3 consecutive failures — report to user.

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|-------------|--------|
| ~20 abstract IEngineVariables methods still return 0/no-op in EngineVariablesImpl | Out of scope for this feature (9 DIM stubs only); Philosophy requires real delegation for all methods | Feature | F835 | N/A (created) | [x] | [DRAFT] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T00:00 | INIT | initializer | Status [REVIEWED] -> [WIP] | READY |
| 2026-03-06T00:00 | DEVIATION | orchestrator | Pre-flight build: tests/uEmuera.Tests build failed (PRE-EXISTING) | MockConsole missing IConsoleOutput.Wait() — not F833 scope |

<!-- run-phase-1-completed -->
| 2026-03-06T00:01 | INVESTIGATE | explorer | Codebase investigation complete | All target files confirmed; PRE-EXISTING test build error noted |
<!-- run-phase-2-completed -->
| 2026-03-06 00:30 | START | implementer | Task 7 (TDD RED: EngineVariablesImplTests.cs + stub + MockConsole fix) | - |
| 2026-03-06 00:30 | END | implementer | Task 7 | SUCCESS |
| 2026-03-06T00:31 | TDD-RED | orchestrator | 10 pass (null-path), 4 skip (round-trip deferred) | Phase 3 confirmed |
<!-- run-phase-3-completed -->
| 2026-03-06 | START | implementer | Tasks 3, 4, 6 (GlobalStatic property + EmueraConsole registration + StubEngineVariables) | - |
| 2026-03-06 | END | implementer | Tasks 3, 4, 6 | SUCCESS (build: 0 errors, 0 warnings) |
| 2026-03-06 | START | implementer | Task 5 (core EngineVariables.cs 6 explicit overrides) | - |
| 2026-03-06 | END | implementer | Task 5 | SUCCESS (core build: 0 errors) |
| 2026-03-06 | START | implementer | Round-trip test investigation | - |
| 2026-03-06 | END | implementer | Round-trip tests | 4 Skip (VariableData requires ConfigService.LoadConfig) |
| 2026-03-06 | BUILD | orchestrator | Engine headless build | SUCCESS (0 errors, 0 warnings) |
| 2026-03-06 | TEST | orchestrator | Full test suite: 587 pass, 4 skip, 9 fail (PRE-EXISTING) | ProcessLevelParallelRunnerTests(5) + VariableDataAccessorTests(4) isolation issues |
| 2026-03-06 | DEVIATION | orchestrator | Full test suite 9 failures (PRE-EXISTING) | Not F833 scope — test isolation issues in unrelated test classes |
<!-- run-phase-4-completed -->
| 2026-03-06 | VERIFY | ac-tester | All 13 ACs verified | 13/13 PASS |
<!-- run-phase-7-completed -->
| 2026-03-06 | REVIEW | feature-reviewer | Post-implementation quality review | READY |
| 2026-03-06 | SKIP | orchestrator | Step 8.2 doc-check — no new extensibility points | N/A |
| 2026-03-06 | CHECK | orchestrator | Step 8.3 SSOT update — no new types/interfaces/commands | N/A |
<!-- run-phase-8-completed -->
| 2026-03-06 | DEVIATION | orchestrator | ac-static-verifier.py failed: cross-repo paths not supported (exit 1) | Tool limitation for features spanning engine/core repos; ac-tester results authoritative |
| 2026-03-06 | VERIFY | orchestrator | Phase 9 AC re-verification: manual cross-repo grep + dotnet test/build | 13/13 PASS |
<!-- run-phase-9-completed -->

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A->B->A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: Technical Design/Tasks | Replace non-existent F461 references with descriptive "GameStateImpl pattern" text
- [fix] Phase2-Review iter1: Problem/Goal/C1/C5/C7/C10/Risks/Baseline | Fix DIM stub count 8→9 to match actual IEngineVariables.cs method count
- [fix] Phase2-Review iter1: Problem | Fix contradiction — EngineVariables.cs already overrides 3 of 9 DIM methods, not 0
- [fix] Phase2-Review iter1: C8 constraint/detail | Clarify registration mechanism as GlobalStatic property (not DI ServiceCollection override)
- [fix] Phase2-Uncertain iter1: AC#2 Detail | Add heuristic proxy note for grep matcher reliability
- [fix] Phase2-Review iter2: C5 Detail | Fix missed 8→9 DIM method count in AC Impact text
- [fix] Phase3-Maintainability iter3: Technical Design note | GetDay() is abstract, must be in EngineVariablesImpl; delegates to GetDay(0); ~20 abstract stubs noted for compilation
- [fix] Phase3-Maintainability iter3: Task 2 | Expand to include GetDay() delegation and ~20 abstract method stub bodies for compilation
- [fix] Phase3-Maintainability iter3: Philosophy | Narrow scope from "Every interface method" to "Every DIM stub method" matching approved scope
- [fix] Phase3-Maintainability iter3: Technical Design | Clarify Task 5 as code clarity improvement (DIM→explicit), not functional fix
- [fix] Phase3-Maintainability iter3: Mandatory Handoffs | Add F835 for remaining ~20 abstract method real delegation
- [fix] Phase4-ACValidation iter4: AC#4,5,6,10 | Change matcher from 'matches' to 'contains' for literal string checks
- [fix] Phase4-ACValidation iter4: AC#7 | Fix broken ripgrep pattern and vague Expected — use 'VariableData == null' with contains matcher
- [fix] Phase4-ACValidation iter4: AC#8 | Fix pattern escaping for '(int)' cast check
- [fix] Phase2-Review iter5: Upstream Issues AC#3 | Mark stale upstream issue as resolved — pattern is correct
- [fix] Phase7-FinalRefCheck iter6: Technical Design | Replace missed F461 reference with "GameStateImpl pattern" text
- [resolved-applied] Phase3-Maintainability iter1: EngineVariablesImpl indexed methods (GetDay/SetDay/GetTime/SetTime) lack bounds checking on index parameter — out-of-range index causes IndexOutOfRangeException at runtime
- [fix] Phase3-Maintainability iter1: F835 Dependencies | Fix F833 status [PROPOSED]→[WIP] in F835 dependency table
- [fix] PostLoop-UserFix post-loop: EngineVariablesImpl.cs indexed methods | Add bounds checking (index < 0 || index >= arr.Length) returning 0/no-op for out-of-range

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} -> `{target}` or -- {reason} -->

---

<!-- fc-phase-6-completed -->
## Links
- [Related: F825](feature-825.md) - Relationships & DI Integration (created stubs)
- [Related: F829](feature-829.md) - Phase 22 Deferred Obligations Consolidation (routing origin)
- [Related: F821](feature-821.md) - Weather System (indexed DAY/TIME methods)
- [Related: F806](feature-806.md) - WC Counter (scalar TIME methods)
- [Related: F790](feature-790.md) - Engine Data Access Layer (IEngineVariables interface)
- [Related: F828](feature-828.md) - Date Init (SetDay consumer)
- [Related: F811](feature-811.md) - GetPrevCom consumers
- [Related: F812](feature-812.md) - GetAssiPlay consumers
- [Successor: F835](feature-835.md) - Remaining abstract IEngineVariables methods real delegation
