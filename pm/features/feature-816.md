# Feature 816: Extract StubVariableStore Base Class in Era.Core.Tests

## Status: [DONE]
<!-- fl-reviewed: 2026-02-24T09:10:44Z -->

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

## Type: infra

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Test infrastructure must scale with interface evolution. As C# migration (Phase 20+) progressively extends IVariableStore with new variable accessors, Era.Core.Tests stubs must not require N-file updates per method addition. The SSOT for IVariableStore test stubs in Era.Core.Tests should be a single shared base class, following the pattern established by F815 for engine.Tests (StubVariableStore in TestStubs.cs).

### Problem (Current Issue)
Era.Core.Tests contains ~37 files that each independently implement IVariableStore stubs (44 methods: 43 explicit + 1 default SetExpLv), because no shared base class exists in the test project. When IVariableStore gains new methods (as F801 added GetEquip/SetEquip, F804 added GetCharacterString/GetExpLv/GetNoItem), all ~37 stub classes fail to compile with CS0535, requiring manual stub additions across every file. This creates an O(N*M) maintenance burden where N=37 stub files and M=methods added per evolution cycle. F815 solved the identical problem for engine.Tests (4 stubs) by extracting a shared `internal class StubVariableStore : IVariableStore` with `virtual` methods returning safe defaults in `engine.Tests/Tests/TestStubs.cs`. Era.Core.Tests has the same structural deficiency at ~9x larger scale, making each IVariableStore extension a ~37-file cascade instead of a 1-file update.

### Goal (What to Achieve)
Create a shared `internal class StubVariableStore : IVariableStore` base class in `Era.Core.Tests/TestStubs.cs` with all 44 IVariableStore methods as `virtual` with safe return defaults (0 for int, no-op for void, `Result<T>.Ok(default)` for Result types). Refactor all ~37 Era.Core.Tests stub classes to inherit from this base class, retaining only their custom method overrides. Future IVariableStore extensions will require updating only 1 file (TestStubs.cs) instead of ~37.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do ~37 test files fail to compile when IVariableStore gains new methods? | Each stub class directly implements `IVariableStore` with no inheritance hierarchy, so every new method requires an explicit implementation in every file. | `Era.Core.Tests/Counter/ActionSelectorTests.cs:325` (`private sealed class SelectorTestVariableStore : IVariableStore`) |
| 2 | Why does each stub class directly implement the interface? | No shared base class exists in Era.Core.Tests to provide default implementations. | Glob `Era.Core.Tests/**/TestStubs*.cs` returns no files. |
| 3 | Why does no shared base class exist? | Stubs were authored independently by different features (including F804 and earlier untracked features) with no upfront shared test infrastructure. | 37 classes across 37 files with different naming conventions (TestVariableStore, MockVariableStore, MinimalMockVariableStore, NullVariableStore, StubVariableStore). |
| 4 | Why was no shared infrastructure planned? | The project grew organically; the shared base class pattern was only established by F815 (2026-02-23) for engine.Tests and was never applied to Era.Core.Tests. | `engine.Tests/Tests/TestStubs.cs:93-150` (F815 solution). |
| 5 | Why (Root)? | The absence of a shared stub base class in Era.Core.Tests makes interface growth cost O(N*M) where N=~37 stub files and M=methods added per evolution cycle, instead of O(M) with a single update point. | F815 proved O(M) pattern works for engine.Tests; Era.Core.Tests lacks it. |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | CS0535 compile errors across ~37 files when IVariableStore gains new methods | No shared StubVariableStore base class in Era.Core.Tests to absorb interface evolution |
| Where | Scattered across 37 test files in Era.Core.Tests (Counter/, Commands/, Infrastructure/, Training/) | Architectural gap: missing test infrastructure layer between IVariableStore and individual stubs |
| Fix | Manually add new method stubs to each of ~37 files | Extract shared base class with virtual methods and safe defaults; refactor stubs to inherit |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F815 | [DONE] | Predecessor -- established StubVariableStore pattern in engine.Tests |
| F801 | [DONE] | Related -- triggered original CS0535 cascade by adding GetEquip/SetEquip |
| F804 | [DONE] | Related -- added GetCharacterString/GetExpLv/GetNoItem to IVariableStore |
| F809 | [DONE] | Related -- added SetExpLv default interface method; active Counter work |
| F803 | [PROPOSED] | Successor -- future IVariableStore extensions benefit from 1-file update point |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Pattern proven | FEASIBLE | F815 established identical pattern for engine.Tests (`engine.Tests/Tests/TestStubs.cs:93-150`) |
| Scale manageable | FEASIBLE | 37 files but each change is mechanical (change inheritance, remove boilerplate, keep overrides) |
| No production code changes | FEASIBLE | Only test infrastructure affected; no changes to Era.Core or engine |
| sealed keyword compatibility | FEASIBLE | C# allows sealed classes to inherit from non-sealed base; no code change needed for sealed modifier |
| NotImplementedException compatibility | FEASIBLE | Stubs using NIE can override base methods to throw if needed; F815 validated safe-default approach |
| Constructor parameter compatibility | FEASIBLE | Derived classes add own constructors; parameterless base constructor is compatible |
| Build validation | FEASIBLE | `dotnet test` catches any regression immediately |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core.Tests compilation | HIGH | Eliminates O(N*M) compile cascade for future IVariableStore extensions |
| Test behavior | LOW | All tests must continue passing; base class uses safe defaults matching majority pattern |
| Code volume | MEDIUM | ~2,200 lines of duplicated boilerplate collapse to ~60 lines in shared base class |
| Future development velocity | HIGH | IVariableStore extensions require 1-file update instead of ~37 |
| engine.Tests | LOW | No changes; already has its own StubVariableStore from F815 |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `TreatWarningsAsErrors=true` | `Directory.Build.props` | All code must compile warning-free; any unused override generates CS0114 warning |
| `SetExpLv` default interface method | `IVariableStore.cs:123` | Base class should provide explicit `virtual` override for consistency with F815 pattern |
| `internal` visibility required | F815 pattern, C# access rules | Base class must be `internal` so all test files in assembly can access it |
| Naming collision | `ConsoleOutputDelegationTests.cs:158` | Already has `private class StubVariableStore`; must handle name conflict (rename private class or qualify) |
| `ComEquivalenceTestBase.MockVariableStore` is `internal` | `ComEquivalenceTestBase.cs:219` | Already shared across namespace; must convert to inherit from base class |
| `Result.Fail` stubs must preserve behavior | `VariableStoreAdapterTests.cs`, `EffectContextTests.cs` | Stubs returning `Result.Fail()` for specific conditions must retain overrides |
| Non-zero default stubs | `ScomfCommandTests.cs` (GetDownbase returns 100) | Custom return values must be preserved as overrides |
| NotImplementedException stub files (13+ files, 277+ occurrences) | grep `NotImplementedException` in `Era.Core.Tests/` | Files confirmed using NIE stubs: `VariableStoreAdapterTests.cs`, `ScomfCommandTests.cs`, `ScomfStubTests.cs`, `BodySettingsGeneticsTests.cs`, `BodyDetailInitMigrationTests.cs`, `BodySettingsBusinessLogicTests.cs`, `BodySettingsGeneticsTask5bTests.cs`, `ScenarioPrerequisiteTests.cs`, `SpecialTrainingTests.cs`, `KojoEngineTests.cs`, `KojoEngineFacadeTests.cs`, and 2+ additional files; each NIE stub requires an explicit override decision by the implementer |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Silent behavior change from NotImplementedException to safe defaults | HIGH | MEDIUM | Affects 13+ files with 277+ NIE occurrences; this transition is near-certain to occur for every NIE-throwing stub; implementer must explicitly decide per-stub whether to (a) adopt safe default or (b) preserve NIE via override; full test suite verification required |
| Merge conflict with F809 (WIP) in Counter directory | MEDIUM | MEDIUM | Coordinate timing; F809 changes are additive, minimal overlap with inheritance refactoring |
| Missing override in refactored stub | MEDIUM | LOW | `dotnet test` catches immediately; mechanical nature reduces error likelihood |
| Result.Fail stubs silently converted to Ok(0) | LOW | HIGH | Explicitly verify VariableStoreAdapterTests and EffectContextTests retain custom overrides |
| Naming collision with existing StubVariableStore in ConsoleOutputDelegationTests | LOW | LOW | Rename private class or use fully qualified name |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Direct IVariableStore implementations | `grep -r ": IVariableStore" Era.Core.Tests/ --include="*.cs" -l \| wc -l` | ~37 | Files with classes directly implementing IVariableStore |
| NotImplementedException occurrences | `grep -r "NotImplementedException" Era.Core.Tests/ --include="*.cs" -c` | ~348 | Total NIE occurrences across all test files |
| Test suite pass rate | `dotnet test Era.Core.Tests/` | All pass | Must remain unchanged after refactoring |

**Baseline File**: `.tmp/baseline-816.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Base class must be `internal` with all methods `virtual` | F815 pattern (`engine.Tests/Tests/TestStubs.cs:93`) | AC must verify accessibility modifier and virtual keyword |
| C2 | All 43 explicit IVariableStore methods must have base implementations | `IVariableStore.cs:15-124` (44 total, minus 1 default SetExpLv that should still be explicit virtual) | AC must verify method count in base class |
| C3 | Refactored stubs must preserve custom behavior: (a) Result.Fail stubs — must retain override returning Result.Fail; (b) NotImplementedException stubs — implementer must decide per-stub: drop NIE (safe default) or preserve NIE via explicit override; decision must be documented | VariableStoreAdapterTests (Result.Fail), EffectContextTests (Result.Fail), ScomfCommandTests (non-zero defaults), 13+ files with 277+ NIE occurrences | AC must verify all existing tests still pass; AC must verify no silent NIE→Ok(default) substitution without documented decision |
| C4 | `sealed` modifier preserved on inheriting classes | 12 Counter test files use `private sealed class` | AC can verify sealed classes inherit correctly |
| C5 | No naming collision with existing StubVariableStore | `ConsoleOutputDelegationTests.cs:158` has `private class StubVariableStore` | AC must verify build succeeds without ambiguity |
| C6 | Zero direct IVariableStore implementations remain | Feature goal: all stubs inherit from base | AC should verify count of direct `: IVariableStore` in test stubs is 0 (only base class) |
| C7 | File location must be `Era.Core.Tests/TestStubs.cs` | F815 precedent (`engine.Tests/Tests/TestStubs.cs`) | AC must verify file exists at specified path |
| C8 | Net boilerplate reduction | 37 files x ~40 methods each = ~2,200 lines | AC could measure LOC reduction or verify override-only pattern |
| C9 | SetExpLv explicit virtual override in base class | F815 pattern, consistency | AC must verify SetExpLv is explicitly implemented despite being a default interface method |
| C10 | NIE stub handling decision must be explicit and documented | 13+ files with 277+ NotImplementedException occurrences; near-certain behavioral change | NIE stub handling decision must be documented: either blanket adoption of safe defaults per F815 precedent (covering all NIE-throwing stubs as a category, with per-category exceptions for behavioral stubs), or per-stub individual decisions. Key Decisions selected blanket Option B. AC must verify the decision is documented in base class and that stubs with genuine behavioral contracts retain explicit overrides (AC#10, AC#11, AC#13, AC#14). AC#9 (full test pass) guards against accidental behavioral regression. |

### Constraint Details

**C1: Internal Virtual Base Class**
- **Source**: F815 pattern in `engine.Tests/Tests/TestStubs.cs:93` (`internal class StubVariableStore : IVariableStore`)
- **Verification**: Grep for `internal class StubVariableStore` in `Era.Core.Tests/TestStubs.cs`
- **AC Impact**: AC must verify both `internal` visibility and `virtual` modifier on all methods

**C2: Complete Method Coverage**
- **Source**: `Era.Core/Interfaces/IVariableStore.cs:15-124` defines 44 methods (43 explicit + 1 default SetExpLv)
- **Verification**: Count methods in base class vs interface
- **AC Impact**: AC must verify all 44 methods are present in base class (including explicit SetExpLv)

**C3: Behavioral Preservation**
- **Source**: Investigation found stubs with custom behavior in two categories: (1) Result.Fail stubs — `VariableStoreAdapterTests.cs` returns `Result.Fail` for missing characters, `EffectContextTests.cs` returns `Result.Fail` for specific conditions, `ScomfCommandTests.cs` returns non-zero defaults; (2) NotImplementedException stubs — 13+ files with 277+ NIE occurrences where each stub's NIE is either a true behavior guard or legacy boilerplate
- **Verification**: Full test suite pass (`dotnet test Era.Core.Tests/`) AND explicit review of all NIE-throwing stub decisions
- **AC Impact**: AC must verify `dotnet test Era.Core.Tests/` passes with zero failures; AC must verify that any NIE→safe-default conversion is intentional (documented, not accidental)

**C4: Sealed Modifier Preservation**
- **Source**: 12 Counter test files use `private sealed class`; C# allows sealed classes to inherit from non-sealed base
- **Verification**: Grep for `sealed.*StubVariableStore` pattern in Counter test files
- **AC Impact**: AC should verify sealed classes successfully inherit (build passes)

**C5: Naming Collision Resolution**
- **Source**: `ConsoleOutputDelegationTests.cs:158` already has `private class StubVariableStore`
- **Verification**: Build succeeds without CS0104 ambiguity error
- **AC Impact**: AC must verify clean build; private nested class name may need renaming or qualification

**C6: Zero Direct Implementations**
- **Source**: Feature goal: single inheritance point
- **Verification**: Grep for `: IVariableStore` in Era.Core.Tests should find only the base class definition
- **AC Impact**: AC must verify count equals 1 (the base class itself) for direct IVariableStore implementations

**C7: File Placement**
- **Source**: F815 precedent places shared stubs in `TestStubs.cs` at project root
- **Verification**: File exists at `Era.Core.Tests/TestStubs.cs`
- **AC Impact**: AC must verify file exists at exact path

**C8: Boilerplate Reduction**
- **Source**: 37 stubs x ~40 boilerplate methods each
- **Verification**: Refactored stubs contain only override methods, not full interface implementations
- **AC Impact**: AC could verify that refactored stubs use `override` keyword and have fewer methods than pre-refactoring

**C9: Explicit SetExpLv**
- **Source**: F815 pattern (`engine.Tests/Tests/TestStubs.cs:148`) provides explicit `virtual` SetExpLv despite it being a default interface method
- **Verification**: Grep for `virtual void SetExpLv` in base class
- **AC Impact**: AC must verify SetExpLv is explicitly implemented in base class for consistency

**C10: NIE Stub Handling Decision**
- **Source**: 13+ files across `Era.Core.Tests/` contain 277+ `NotImplementedException` occurrences in IVariableStore stub methods (confirmed files: `VariableStoreAdapterTests.cs`, `ScomfCommandTests.cs`, `ScomfStubTests.cs`, `BodySettingsGeneticsTests.cs`, `BodyDetailInitMigrationTests.cs`, `BodySettingsBusinessLogicTests.cs`, `BodySettingsGeneticsTask5bTests.cs`, `ScenarioPrerequisiteTests.cs`, `SpecialTrainingTests.cs`, `KojoEngineTests.cs`, `KojoEngineFacadeTests.cs`, and others). This is a near-certain behavioral change when the base class provides safe defaults.
- **Verification**: Blanket decision (Option B: adopt safe defaults for all NIE stubs per F815 precedent) is acceptable when documented in base class comment block. Per-category exceptions for behavioral stubs (Result.Fail, non-zero defaults, dictionary state) must retain explicit overrides. Full test suite pass (AC#9) guards against accidental behavioral regression from NIE→safe-default conversion.
- **AC Impact**: AC#7 verifies the blanket decision is documented in TestStubs.cs. ACs #10, #11, #13, #14 verify that stubs with genuine behavioral contracts retain explicit overrides. AC#9 verifies full test pass as behavioral regression guard. Together these ensure no NIE→Ok(default) substitution occurs without documentation and behavioral regression is caught.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F815 | [DONE] | Established StubVariableStore base class pattern for engine.Tests; must be [DONE] before applying same pattern to Era.Core.Tests |
| Related | F801 | [DONE] | Triggered original CS0535 cascade by adding GetEquip/SetEquip to IVariableStore |
| Related | F804 | [DONE] | Added GetCharacterString/GetExpLv/GetNoItem to IVariableStore |
| Related | F809 | [DONE] | Added SetExpLv default interface method; active Counter work may create merge conflicts |
| Successor | F803 | [PROPOSED] | Future IVariableStore extensions benefit from 1-file update point |

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
| "Test infrastructure **must** scale with interface evolution" | Base class absorbs new methods so stubs auto-compile | AC#1, AC#2, AC#3, AC#4 |
| "stubs **must not** require N-file updates per method addition" | All stubs inherit from single base class; no direct IVariableStore implementations remain (including gitignored files) | AC#5, AC#6, AC#15a, AC#15b |
| "The SSOT for IVariableStore test stubs should be a **single** shared base class" | Exactly one base class at canonical path; all stubs derive from it | AC#1, AC#5 |
| "following the pattern established by F815" | Base class mirrors F815 structure: internal, virtual, safe defaults, explicit SetExpLv | AC#2, AC#3, AC#4, AC#17 |
| "stubs with genuine behavioral contracts retain explicit overrides" (C3 constraint) | Result.Fail, non-zero, dictionary-state stubs preserve overrides including getter/setter pairs | AC#10, AC#11, AC#13, AC#14, AC#18 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TestStubs.cs exists at canonical path | file | Glob(Era.Core.Tests/TestStubs.cs) | exists | - | [x] |
| 2 | Base class is internal and implements IVariableStore | code | Grep(Era.Core.Tests/TestStubs.cs) | matches | `internal class StubVariableStore : IVariableStore` | [x] |
| 3 | All 44 methods are public virtual in base class | code | Grep(Era.Core.Tests/TestStubs.cs) | count_equals | `public virtual` = 44 | [x] |
| 4 | SetExpLv is explicitly implemented as virtual | code | Grep(Era.Core.Tests/TestStubs.cs) | matches | `public virtual void SetExpLv` | [x] |
| 5 | Only base class directly implements IVariableStore | code | Grep(Era.Core.Tests/) | count_equals | `class \w+ : IVariableStore` = 1 | [x] |
| 6 | Refactored stubs inherit from StubVariableStore | code | Grep(Era.Core.Tests/) | gte | `class \w+ : StubVariableStore` >= 36 | [x] |
| 7a | NIE blanket decision documented (NIE + safe default on same line) | code | Grep(Era.Core.Tests/TestStubs.cs) | matches | `NIE.*safe default\|NotImplementedException.*safe default` | [x] |
| 7b | NIE exception categories documented (behavioral contract types) | code | Grep(Era.Core.Tests/TestStubs.cs) | matches | `Result.Fail\|behavioral\|non-zero\|in-memory` | [x] |
| 8 | Era.Core.Tests build succeeds | build | dotnet build Era.Core.Tests/ | succeeds | - | [x] |
| 9 | All Era.Core.Tests tests pass | test | dotnet test Era.Core.Tests/ | succeeds | - | [x] |
| 10 | VariableStoreAdapterTests preserves ALL three Result.Fail getter overrides (GetMaxBase, GetJuel, GetCharacterFlag) | code | Grep(Era.Core.Tests/Infrastructure/VariableStoreAdapterTests.cs) | gte | `override.*Get(MaxBase\|Juel\|CharacterFlag)` >= 3 | [x] |
| 18 | VariableStoreAdapterTests preserves ALL three setter overrides (SetMaxBase, SetJuel, SetCharacterFlag) | code | Grep(Era.Core.Tests/Infrastructure/VariableStoreAdapterTests.cs) | gte | `override.*Set(MaxBase\|Juel\|CharacterFlag)` >= 3 | [x] |
| 11 | EffectContextTests preserves Result.Fail override for GetDownbase | code | Grep(Era.Core.Tests/Effects/EffectContextTests.cs) | matches | `override.*GetDownbase[\s\S]*?Fail` | [x] |
| 12 | ConsoleOutputDelegationTests naming collision resolved | code | Grep(Era.Core.Tests/Shop/ConsoleOutputDelegationTests.cs) | not_matches | `private class StubVariableStore : IVariableStore` | [x] |
| 13 | ScomfCommandTests preserves non-zero GetDownbase override via StubVariableStore inheritance | code | Grep(Era.Core.Tests/Commands/Special/ScomfCommandTests.cs) | matches | `override.*GetDownbase` | [x] |
| 14 | ActionSelectorTests preserves ALL dictionary-based overrides (GetFlag, GetCharacterFlag, GetTCVar) | code | Grep(Era.Core.Tests/Counter/ActionSelectorTests.cs) | gte | `override.*(GetFlag\|GetCharacterFlag\|GetTCVar)` >= 3 | [x] |
| 15a1 | F629DebugTests inherits from StubVariableStore | code | Grep(Era.Core.Tests/Debug/F629DebugTests.cs) | matches | `: StubVariableStore` | [x] |
| 15a2 | F629YamlDebugTests inherits from StubVariableStore | code | Grep(Era.Core.Tests/Debug/F629YamlDebugTests.cs) | matches | `: StubVariableStore` | [x] |
| 15b1 | F629DebugTests has no direct IVariableStore implementation | code | Grep(Era.Core.Tests/Debug/F629DebugTests.cs) | not_matches | `: IVariableStore` | [x] |
| 15b2 | F629YamlDebugTests has no direct IVariableStore implementation | code | Grep(Era.Core.Tests/Debug/F629YamlDebugTests.cs) | not_matches | `: IVariableStore` | [x] |
| 16 | Refactored stubs with overrides contain no non-override method bodies (spot-check) | code | Grep(Era.Core.Tests/Counter/WcActionSelectorTests.cs) | not_matches | `public (?!override)(int\|void\|Result).*(Get\|Set)` | [x] |
| 17 | Base class safe defaults use correct return values (spot-check) | code | Grep(Era.Core.Tests/TestStubs.cs) | matches | `Result<string>.Ok\(string.Empty\)` | [x] |

### AC Details

**AC#1: TestStubs.cs exists at canonical path**
- **Test**: `Glob(Era.Core.Tests/TestStubs.cs)`
- **Expected**: File exists at `Era.Core.Tests/TestStubs.cs`
- **Rationale**: C7 constraint. F815 pattern places shared stubs in `TestStubs.cs` at project root. Single canonical location is required for SSOT.

**AC#2: Base class is internal and implements IVariableStore**
- **Test**: `Grep(Era.Core.Tests/TestStubs.cs)` for pattern `internal class StubVariableStore : IVariableStore`
- **Expected**: Pattern matches in TestStubs.cs
- **Rationale**: C1 constraint. `internal` visibility required so all test files in assembly can access it. Direct IVariableStore implementation required to absorb interface evolution.

**AC#3: All 44 methods are public virtual in base class**
- **Test**: `Grep(Era.Core.Tests/TestStubs.cs)` counting `public virtual` occurrences
- **Expected**: Exactly 44 matches (43 explicit interface methods + 1 explicit SetExpLv)
- **Rationale**: C2 constraint. Every method must be `virtual` so derived classes can override. Count of 44 matches F815 pattern in engine.Tests.

**AC#4: SetExpLv is explicitly implemented as virtual**
- **Test**: `Grep(Era.Core.Tests/TestStubs.cs)` for pattern `public virtual void SetExpLv`
- **Expected**: Pattern matches
- **Rationale**: C9 constraint. SetExpLv is a default interface method but must be explicitly implemented as `virtual` in the base class for consistency with F815 pattern (engine.Tests/Tests/TestStubs.cs:148).

**AC#5: Only base class directly implements IVariableStore**
- **Test**: `Grep(Era.Core.Tests/)` counting `class \w+ : IVariableStore` pattern across all .cs files
- **Expected**: Exactly 1 match (the base class in TestStubs.cs). Current baseline: 36 matches (Grep tool excludes 2 gitignored Debug/ files; total with Debug/ = 38). Note: Grep cannot verify gitignored files; implementer must explicitly refactor those 2 files during T2.
- **Rationale**: C6 constraint. Zero direct IVariableStore implementations should remain in stub classes. Only the base class should directly implement the interface, ensuring future interface extensions require only 1-file update.
- **Gitignored files**: 2 Debug/ files are excluded from Grep verification. Implementer MUST explicitly verify these files inherit from `StubVariableStore` (not direct `IVariableStore`) during T2. Verification method: `Read()` each Debug/ stub file and confirm `: StubVariableStore` inheritance. If Debug/ files cannot be located, STOP and report to user.
- **Assumption**: Exactly 2 gitignored stub files exist in `Era.Core.Tests/Debug/` (`F629DebugTests.cs`, `F629YamlDebugTests.cs`). T2 pre-condition: implementer must verify no additional gitignored IVariableStore stubs exist by running `Glob(Era.Core.Tests/Debug/**/*.cs)` and checking for any additional `: IVariableStore` declarations. If additional files are found, STOP and update AC#15 to include them.

**AC#6: Refactored stubs inherit from StubVariableStore**
- **Test**: `Grep(Era.Core.Tests/)` counting `class \w+ : StubVariableStore` pattern across all .cs files
- **Expected**: >= 36 matches (at minimum 36 existing stub classes all refactored to inherit from StubVariableStore; may exceed 36 if successor features add new stubs). Current baseline: 0 matches. Note: 2 Debug/ files are gitignored and excluded from Grep tool count; these files are compiled by `dotnet build` (default SDK glob includes all `**/*.cs`) but Grep cannot verify their inheritance. Implementer must explicitly refactor these 2 files during T2 execution; AC#15a/AC#15b verify Debug/ files separately. Total with Debug/ files = 38 stubs (36 Grep-visible + 2 gitignored).
- **Rationale**: Uses `count_gte` instead of `count_equals` so the AC remains valid if successor features (e.g., F803) add new StubVariableStore-inheriting test stubs. Combined with AC#5, guarantees complete migration.

**AC#7a/7b: NIE handling decision documented with blanket decision and exception categories**
- **Test**: `Grep(Era.Core.Tests/TestStubs.cs)` for TWO patterns: (1) `NIE.*safe default|NotImplementedException.*safe default` AND (2) `Result.Fail|behavioral|non-zero|in-memory`
- **Expected**: Both patterns match in TestStubs.cs. Pattern 1 requires NIE/NotImplementedException AND safe default to co-occur on the same line, ensuring the blanket decision is documented (not just a loose mention of "safe default"). Pattern 2 verifies exception categories are named.
- **Rationale**: C10 constraint. 13+ files with 277+ NIE occurrences undergo behavioral change. Pattern 1 uses `NIE.*safe default` (not OR) to require both the problem (NIE) and the decision (safe default) to appear together, preventing a minimal "safe default" comment from satisfying the AC without documenting the NIE handling rationale.

**AC#8: Era.Core.Tests build succeeds**
- **Test**: `dotnet build Era.Core.Tests/`
- **Expected**: Build succeeds with zero errors
- **Rationale**: C5 constraint (naming collision) and C4 constraint (sealed modifier) verification. Build success confirms no CS0104 ambiguity, no CS0535 missing implementations, and sealed classes correctly inherit from non-sealed base.

**AC#9: All Era.Core.Tests tests pass**
- **Test**: `dotnet test Era.Core.Tests/`
- **Expected**: All tests pass with zero failures
- **Rationale**: C3 constraint (behavioral preservation). Full test suite must pass to confirm no silent behavior change from NotImplementedException to safe defaults. This is the primary guard against accidental regression.

**AC#10: VariableStoreAdapterTests preserves ALL three Result.Fail overrides (GetMaxBase, GetJuel, GetCharacterFlag)**
- **Test**: `Grep(Era.Core.Tests/Infrastructure/VariableStoreAdapterTests.cs)` counting `override.*Get(MaxBase|Juel|CharacterFlag)` matches
- **Expected**: Count >= 3 (all three behavioral override methods must be present: GetMaxBase, GetJuel, GetCharacterFlag). Each method returns Result.Fail for missing character validation.
- **Rationale**: C3 constraint. VariableStoreAdapterTests.MockVariableStore uses Result.Fail returns for GetMaxBase, GetJuel, and GetCharacterFlag. Using `count_gte` with 3 ensures ALL three methods are preserved as overrides, not just any one. Previous OR matcher was insufficient — it passed if only one of three existed.

**AC#11: EffectContextTests preserves Result.Fail override for GetDownbase**
- **Test**: `Grep(Era.Core.Tests/Effects/EffectContextTests.cs, multiline=true)` for `override.*GetDownbase[\s\S]*?Fail`
- **Expected**: Pattern matches (GetDownbase override with Fail return, confirming the conditional failure simulation is preserved)
- **Rationale**: C3 constraint. EffectContextTests.TestableVariableStore uses GetDownbase with conditional Result.Fail via `_downbaseFailures` HashSet. Method-specific matcher ensures the override is on GetDownbase specifically. Multiline handles block-body formatting.

**AC#18: VariableStoreAdapterTests preserves ALL three setter overrides (SetMaxBase, SetJuel, SetCharacterFlag)**
- **Test**: `Grep(Era.Core.Tests/Infrastructure/VariableStoreAdapterTests.cs)` counting `override.*Set(MaxBase|Juel|CharacterFlag)` matches
- **Expected**: Count >= 3 (all three setter override methods must be present: SetMaxBase, SetJuel, SetCharacterFlag). Each setter writes to the ConcurrentDictionary store.
- **Rationale**: C3 constraint. VariableStoreAdapterTests.MockVariableStore uses ConcurrentDictionary-backed Set overrides for SetMaxBase, SetJuel, and SetCharacterFlag. AC#10 only verifies getter overrides; setter overrides must also be preserved to maintain the full store pattern. Without setters, the dictionary-backed store's write-side is silently lost.

**AC#12: ConsoleOutputDelegationTests naming collision resolved**
- **Test**: `Grep(Era.Core.Tests/Shop/ConsoleOutputDelegationTests.cs)` for `private class StubVariableStore : IVariableStore`
- **Expected**: Pattern NOT found (private class renamed or converted to inherit from base)
- **Rationale**: C5 constraint. ConsoleOutputDelegationTests.cs:158 currently has `private class StubVariableStore : IVariableStore` which would conflict with the new base class name. Must be renamed or refactored to inherit.

**AC#13: ScomfCommandTests preserves non-zero GetDownbase override via StubVariableStore inheritance**
- **Test**: `Grep(Era.Core.Tests/Commands/Special/ScomfCommandTests.cs)` for `override.*GetDownbase`
- **Expected**: Pattern matches (override method returning non-zero value for GetDownbase)
- **Rationale**: C3 constraint. ScomfCommandTests.MinimalMockVariableStore returns `Result<int>.Ok(100)` for GetDownbase (non-zero default). After refactoring to inherit from StubVariableStore, this must be preserved as an explicit `override`, not silently replaced with the base class `Result<int>.Ok(0)` default.

**AC#14: ActionSelectorTests preserves ALL dictionary-based overrides (GetFlag, GetCharacterFlag, GetTCVar)**
- **Test**: `Grep(Era.Core.Tests/Counter/ActionSelectorTests.cs)` counting `override.*(GetFlag|GetCharacterFlag|GetTCVar)` matches
- **Expected**: Count >= 3 (all three dictionary-backed override methods must be present: GetFlag, GetCharacterFlag, GetTCVar). Each uses in-memory dictionary lookups.
- **Rationale**: C3 constraint. ActionSelectorTests.SelectorTestVariableStore uses in-memory dictionaries for GetFlag, GetCharacterFlag, and GetTCVar. Using `count_gte` with 3 ensures ALL three methods are preserved as overrides. Previous `matches` on GetFlag alone was insufficient — GetCharacterFlag and GetTCVar overrides could be silently dropped without detection.

**AC#15a: Gitignored Debug/ stubs inherit from StubVariableStore (positive)**
- **Test**: `Grep(Era.Core.Tests/Debug/F629DebugTests.cs)` for `: StubVariableStore` AND `Grep(Era.Core.Tests/Debug/F629YamlDebugTests.cs)` for `: StubVariableStore`. Grep with explicit file path bypasses .gitignore.
- **Expected**: Pattern matches in both files (positive confirmation of correct inheritance).
- **Rationale**: C6 constraint. These 2 Debug/ files are gitignored and excluded from directory-level Grep (AC#5/AC#6). Explicit file path Grep enables verification. Split from compound AC#15 to use standard `matches` matcher.

**AC#15b: Gitignored Debug/ stubs no longer directly implement IVariableStore (negative)**
- **Test**: `Grep(Era.Core.Tests/Debug/F629DebugTests.cs)` for `: IVariableStore` AND `Grep(Era.Core.Tests/Debug/F629YamlDebugTests.cs)` for `: IVariableStore`. Grep with explicit file path bypasses .gitignore.
- **Expected**: Pattern NOT found in either file (negative confirmation: no direct IVariableStore implementation remains).
- **Rationale**: C6 constraint. Prevents partially-refactored files where both `: IVariableStore` (old) and `: StubVariableStore` (new) classes coexist. Split from compound AC#15 to use standard `not_matches` matcher.

**AC#16: Refactored stubs with overrides contain no non-override method bodies (spot-check)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcActionSelectorTests.cs)` for pattern `public (?!override)(int|void|Result).*(Get|Set)` (negative lookahead: matches `public int/void/Result...Get/Set` lines where `override` does NOT follow `public`)
- **Expected**: Pattern NOT found. All remaining method declarations in WcSelectorTestVariableStore should use `public override ...` (from base class) or be absent entirely (removed boilerplate). Lines with `public override int GetFlag(...)` will NOT match due to the `(?!override)` lookahead.
- **Rationale**: C8 constraint and Goal#5 (retain only custom overrides). WcActionSelectorTests has dictionary-backed overrides (like ActionSelectorTests) so the refactored stub will retain some `override` methods while removing boilerplate. The `(?!override)` lookahead must appear immediately after `public ` to correctly detect non-override method declarations in C# (where `override` precedes the return type).

**AC#17: Base class safe defaults use correct return values (spot-check)**
- **Test**: `Grep(Era.Core.Tests/TestStubs.cs)` for pattern `Result<string>.Ok\(string.Empty\)`
- **Expected**: Pattern matches (verifying GetCharacterString returns `Result<string>.Ok(string.Empty)`, the most distinctive safe default value)
- **Rationale**: AC#3 verifies method count (44 `public virtual`) but not return value correctness. This spot-check verifies the `Result<string>` category safe default, which is the most likely to be incorrectly implemented (e.g., `null` instead of `string.Empty`). Combined with AC#9 (full test pass), provides coverage for safe default value correctness across all return type categories.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Create shared `internal class StubVariableStore : IVariableStore` base class | AC#1, AC#2 |
| 2 | Base class in `Era.Core.Tests/TestStubs.cs` | AC#1 |
| 3 | All 44 IVariableStore methods as `virtual` with safe return defaults; NIE→safe-default decision documented | AC#3, AC#4, AC#7a, AC#7b, AC#17 |
| 4 | Refactor all ~37 Era.Core.Tests stub classes to inherit from base class; resolve naming collisions | AC#5, AC#6, AC#12, AC#15a, AC#15b |
| 5 | Retain only custom method overrides in refactored stubs | AC#10, AC#11, AC#13, AC#14, AC#16, AC#18 |
| 6 | Future IVariableStore extensions require updating only 1 file | AC#5, AC#15a, AC#15b (structural guarantee: if all stubs inherit from StubVariableStore (AC#5+AC#6+AC#15a+AC#15b) and only the base class implements IVariableStore, then by C# inheritance semantics, new interface methods only require updating TestStubs.cs — the O(M) property is a logical consequence of the verified hierarchy) |
| 7 | All tests continue passing after refactoring | AC#8, AC#9 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Create `Era.Core.Tests/TestStubs.cs` containing an `internal class StubVariableStore : IVariableStore` base class with all 44 IVariableStore methods as `public virtual`, using safe return defaults identical to the F815 pattern in `engine.Tests/Tests/TestStubs.cs`. Then mechanically refactor all 36 direct IVariableStore stub classes (currently confirmed by Grep across non-gitignored test files) to inherit from this base class, retaining only their custom method overrides.

The approach is a direct port of the F815 pattern at ~9x scale:
- Base class: `Era.Core.Tests/TestStubs.cs`, namespace `Era.Core.Tests`, `internal class StubVariableStore : IVariableStore`
- All 44 methods declared `public virtual`: 40 getter/setter pairs (int returns → 0, void returns → no-op, `Result<int>` returns → `Result<int>.Ok(0)`) + `GetCharacterString` (→ `Result<string>.Ok(string.Empty)`) + `GetExpLv` (→ `Result<int>.Ok(0)`) + `GetNoItem` (→ 0) + explicit `SetExpLv` (→ no-op, despite being a default interface method)
- NIE (NotImplementedException) decision: All NIE-throwing stubs adopt safe defaults per F815 precedent. NIE behavior in these stubs is legacy boilerplate, not a behavior guard. This decision is explicitly documented with a comment block in `TestStubs.cs`. Stubs that genuinely need non-default behavior (e.g., `VariableStoreAdapterTests.MockVariableStore` Result.Fail returns, `EffectContextTests.TestableVariableStore` GetDownbase with failure simulation, `ActionSelectorTests.SelectorTestVariableStore` with in-memory dictionaries) retain their full custom implementations as overrides.
- Naming collision resolution for `ConsoleOutputDelegationTests.cs`: The existing `private class StubVariableStore : IVariableStore` at line 158 is renamed to `private class DefaultStubVariableStore : StubVariableStore`, inheriting from the base. Since it implements only safe defaults, no method overrides are needed.
- The `ComEquivalenceTestBase.MockVariableStore` (currently `internal` at file scope, not nested) converts to `internal class MockVariableStore : StubVariableStore` — no method overrides needed since all methods are already safe defaults.

This approach satisfies all 16 ACs (AC#1-18, with sub-numbered splits: AC#7→7a/7b, AC#15→15a1/15a2/15b1/15b2): file creation (AC#1), class declaration (AC#2), virtual method count (AC#3), SetExpLv explicit (AC#4), zero direct implementations remaining (AC#5), 36 inheriting classes (AC#6), NIE blanket decision (AC#7a) and exception categories (AC#7b), build succeeds (AC#8), tests pass (AC#9), Result.Fail getter overrides preserved (AC#10), EffectContextTests GetDownbase preserved (AC#11), naming collision resolved (AC#12), ScomfCommandTests non-zero override preserved (AC#13), ActionSelectorTests dictionary-based overrides preserved (AC#14), gitignored Debug/ stubs verified with positive+negative checks (AC#15a/15b), boilerplate removal spot-check (AC#16), safe default return value spot-check (AC#17), VariableStoreAdapterTests setter overrides preserved (AC#18).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core.Tests/TestStubs.cs` at that exact path |
| 2 | Declare `internal class StubVariableStore : IVariableStore` as the class header in TestStubs.cs |
| 3 | Declare all 44 methods with `public virtual` modifier — count verified by `Grep("public virtual", Era.Core.Tests/TestStubs.cs)` == 44 |
| 4 | Include explicit `public virtual void SetExpLv(int index, int value) { }` in base class body |
| 5 | Refactor all 36 non-base stub classes so none has `: IVariableStore` — only the base class keeps that declaration |
| 6 | Change each of the 36 existing stubs from `: IVariableStore` to `: StubVariableStore` (ConsoleOutputDelegationTests' renamed class counts as 1 of the 36; ComEquivalenceTestBase.MockVariableStore counts as 1 of the 36) |
| 7a,7b | Add a comment block in TestStubs.cs header documenting NIE→safe-default decision per F815 precedent |
| 8 | Run `dotnet build Era.Core.Tests/` — all CS0535, CS0104, CS0114 issues resolved by base class and rename |
| 9 | Run `dotnet test Era.Core.Tests/` — behavioral stubs that needed non-default behavior retain their overrides |
| 10 | `VariableStoreAdapterTests.MockVariableStore` retains `Result<int>.Fail(...)` returns in GetMaxBase, GetJuel, GetCharacterFlag as explicit overrides of base methods |
| 11 | `EffectContextTests.TestableVariableStore` retains `Result<int>.Fail("Error")` return in GetDownbase override |
| 12 | Rename `ConsoleOutputDelegationTests.StubVariableStore` to `DefaultStubVariableStore` inheriting from base — pattern `private class StubVariableStore : IVariableStore` no longer present in that file |
| 13 | `ScomfCommandTests.MinimalMockVariableStore` retains `override` for `GetDownbase` returning non-zero `Result<int>.Ok(100)` |
| 14 | `ActionSelectorTests.SelectorTestVariableStore` retains `override` for `GetFlag` (and other dictionary-backed methods) preserving in-memory lookup behavior |
| 15a | Verify `: StubVariableStore` matches in both `Debug/F629DebugTests.cs` and `Debug/F629YamlDebugTests.cs` |
| 15b | Verify `: IVariableStore` NOT found in both Debug/ files — confirms no partial refactoring remnants |
| 16 | Verify `WcActionSelectorTests.WcSelectorTestVariableStore` has no non-override method declarations — confirms boilerplate removal with override retention |
| 17 | Implement `GetCharacterString` with `Result<string>.Ok(string.Empty)` safe default — spot-checks return value correctness for `Result<string>` category |
| 18 | `VariableStoreAdapterTests.MockVariableStore` retains `override` for `SetMaxBase`, `SetJuel`, `SetCharacterFlag` (ConcurrentDictionary-backed write-side pattern) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| NIE stub handling | A: Preserve all NIE via explicit override; B: Drop NIE, adopt safe defaults (F815 precedent); C: Per-stub decision | B: Drop NIE, adopt safe defaults | NIE in these stubs is legacy boilerplate (CS0535 workaround), not intentional behavior guards. F815 validated that safe defaults are sufficient for engine.Tests. Full test suite (AC#9) catches any regression from this decision. Explicit documentation satisfies AC#7 and C10. |
| ConsoleOutputDelegationTests naming collision | A: Rename existing private class; B: Use fully-qualified base class name; C: Delete existing class and convert to inherit | A: Rename to `DefaultStubVariableStore` and inherit from base | Rename is the least-invasive change. The existing class already returns only safe defaults so no method body changes needed, only the class declaration line changes. |
| ComEquivalenceTestBase.MockVariableStore | A: Keep `internal class MockVariableStore : IVariableStore`; B: Convert to `: StubVariableStore` | B: Convert to `: StubVariableStore` | All methods return safe defaults — zero overrides needed. Converts to zero-body class definition, dramatically reducing file size. |
| Stubs with in-memory dictionary state (ActionSelectorTests, WcActionSelectorTests, etc.) | A: Keep full IVariableStore body; B: Inherit from base, keep only non-default methods as overrides | B: Inherit, keep overrides | These stubs have custom logic for specific methods (dictionary lookups, configured returns). They inherit the base and only override the methods they customize. Safe default methods (those returning Ok(0)/0) are removed from the stub body. |
| Stubs with Result.Fail returns | A: Convert to safe defaults; B: Preserve Result.Fail as override | B: Preserve override | VariableStoreAdapterTests and EffectContextTests use Result.Fail to simulate missing-character conditions. These are genuine behavior guards that tests assert against. Converting to safe defaults would change test semantics. |
| File namespace | `Era.Core.Tests` (matches TestHelpers.cs, BaseTestClass.cs) | `Era.Core.Tests` | Consistent with existing root-level test files in same project directory. |

### Interfaces / Data Structures

The new `Era.Core.Tests/TestStubs.cs` file structure mirrors `engine.Tests/Tests/TestStubs.cs`:

```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Tests
{
    // NIE DECISION: NotImplementedException stubs → safe defaults per F815 precedent (F816).
    // All 36 IVariableStore stub classes previously used NIE as CS0535 boilerplate.
    // Safe default = 0 for int, no-op for void, Result<T>.Ok(default) for Result types.
    // Stubs with genuine behavioral contracts (Result.Fail, in-memory state, non-zero returns)
    // retain explicit overrides in their derived classes.
    internal class StubVariableStore : IVariableStore
    {
        // 1D array getters — return int
        public virtual int GetFlag(FlagIndex index) => 0;
        public virtual int GetTFlag(FlagIndex index) => 0;

        // 1D array setters — no-op
        public virtual void SetFlag(FlagIndex index, int value) { }
        public virtual void SetTFlag(FlagIndex index, int value) { }

        // 1D borderline
        public virtual Result<int> GetPalamLv(int index) => Result<int>.Ok(0);
        public virtual void SetPalamLv(int index, int value) { }

        // 2D character-scoped getters — return Result<int>.Ok(0)
        public virtual Result<int> GetCharacterFlag(CharacterId character, CharacterFlagIndex flag) => Result<int>.Ok(0);
        public virtual Result<int> GetAbility(CharacterId character, AbilityIndex ability) => Result<int>.Ok(0);
        public virtual Result<int> GetTalent(CharacterId character, TalentIndex talent) => Result<int>.Ok(0);
        public virtual Result<int> GetPalam(CharacterId character, PalamIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetExp(CharacterId character, ExpIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetBase(CharacterId character, BaseIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetTCVar(CharacterId character, TCVarIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetSource(CharacterId character, SourceIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetMark(CharacterId character, MarkIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetNowEx(CharacterId character, NowExIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetMaxBase(CharacterId character, MaxBaseIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetCup(CharacterId character, CupIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetJuel(CharacterId character, int index) => Result<int>.Ok(0);
        public virtual Result<int> GetGotJuel(CharacterId character, int index) => Result<int>.Ok(0);
        public virtual Result<int> GetStain(CharacterId character, StainIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetDownbase(CharacterId character, DownbaseIndex index) => Result<int>.Ok(0);
        public virtual Result<int> GetEquip(CharacterId character, int index) => Result<int>.Ok(0);

        // 2D character-scoped setters — no-op
        public virtual void SetCharacterFlag(CharacterId character, CharacterFlagIndex flag, int value) { }
        public virtual void SetAbility(CharacterId character, AbilityIndex ability, int value) { }
        public virtual void SetTalent(CharacterId character, TalentIndex talent, int value) { }
        public virtual void SetPalam(CharacterId character, PalamIndex index, int value) { }
        public virtual void SetExp(CharacterId character, ExpIndex index, int value) { }
        public virtual void SetBase(CharacterId character, BaseIndex index, int value) { }
        public virtual void SetTCVar(CharacterId character, TCVarIndex index, int value) { }
        public virtual void SetSource(CharacterId character, SourceIndex index, int value) { }
        public virtual void SetMark(CharacterId character, MarkIndex index, int value) { }
        public virtual void SetNowEx(CharacterId character, NowExIndex index, int value) { }
        public virtual void SetMaxBase(CharacterId character, MaxBaseIndex index, int value) { }
        public virtual void SetCup(CharacterId character, CupIndex index, int value) { }
        public virtual void SetJuel(CharacterId character, int index, int value) { }
        public virtual void SetGotJuel(CharacterId character, int index, int value) { }
        public virtual void SetStain(CharacterId character, StainIndex index, int value) { }
        public virtual void SetDownbase(CharacterId character, DownbaseIndex index, int value) { }
        public virtual void SetEquip(CharacterId character, int index, int value) { }

        // Feature 804 / 809 additions
        public virtual Result<string> GetCharacterString(CharacterId character, CstrIndex index) => Result<string>.Ok(string.Empty);
        public virtual Result<int> GetExpLv(int level) => Result<int>.Ok(0);
        public virtual int GetNoItem() => 0;
        public virtual void SetExpLv(int index, int value) { }
    }
}
```

**Refactoring pattern for stubs with safe-defaults-only (e.g., ComEquivalenceTestBase.MockVariableStore)**:
```csharp
// Before:
internal class MockVariableStore : IVariableStore
{
    public int GetFlag(FlagIndex index) => 0;
    // ... 43 more methods ...
}

// After:
internal class MockVariableStore : StubVariableStore { }
```

**Refactoring pattern for stubs with behavioral state (e.g., ActionSelectorTests.SelectorTestVariableStore)**:
```csharp
// Before: full IVariableStore with 44 methods
private sealed class SelectorTestVariableStore : IVariableStore
{
    private readonly Dictionary<...> _cflag = new();
    // ... fields and 44 method implementations ...
    public int GetFlag(FlagIndex i) => _flag.TryGetValue(i.Value, out var v) ? v : 0;
    public int GetAbility(...) => Result<int>.Ok(0); // safe default
    // ... etc.
}

// After: inherit base, override only custom-logic methods
private sealed class SelectorTestVariableStore : StubVariableStore
{
    private readonly Dictionary<...> _cflag = new();
    // ... fields ...
    public override int GetFlag(FlagIndex i) => _flag.TryGetValue(i.Value, out var v) ? v : 0;
    // only methods with non-default logic get override keyword
    // safe-default methods removed entirely
}
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#5 count_equals Expected is 1, but Grep `class \w+ : IVariableStore` currently returns 36 matches across 36 files (not 37 as Background states) | AC#5, Background/Problem | Background mentions "~37 files" based on earlier investigation. Current Grep across non-gitignored test files confirms 36. AC#5 Expected=1 is correct. Background text estimate is not an AC; no action required. |
| AC#6 count_equals Expected is 36: all 36 existing stub classes are refactored to `: StubVariableStore`. The base class (StubVariableStore) is NEW (created by Task 1), so it is not counted among the 36 existing stubs. ComEquivalenceTestBase.MockVariableStore is `internal` (not private/nested) — Grep pattern `class \w+ : StubVariableStore` still matches it. | AC#6 | No issue — pattern matches both nested and file-scope classes. Expected=36 is correct. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,3,4,7a,7b,17 | Create `Era.Core.Tests/TestStubs.cs` with `internal class StubVariableStore : IVariableStore`, 44 `public virtual` methods (safe defaults), explicit `SetExpLv`, and NIE decision comment block | | [x] |
| 2 | 13,14,15a1,15a2,15b1,15b2,16 | Refactor 33 Grep-visible stub classes + 2 gitignored Debug/ stubs (`Debug/F629DebugTests.cs`, `Debug/F629YamlDebugTests.cs`) to inherit `StubVariableStore` (remove boilerplate, keep only method overrides for non-default behavior; includes stubs with behavioral overrides: ScomfCommandTests.MinimalMockVariableStore with GetDownbase override, ActionSelectorTests.SelectorTestVariableStore with dictionary-based overrides; excludes VariableStoreAdapterTests, EffectContextTests, ConsoleOutputDelegationTests handled in T3). Debug/ files must be verified via Read() not Grep. | | [x] |
| 3 | 5,6,10,11,12,18 | Refactor behavioral stubs: (a) `VariableStoreAdapterTests.MockVariableStore` — preserve full ConcurrentDictionary-backed store pattern (Get/Set overrides for MaxBase, Juel, CharacterFlag + _store field + Key() helpers); (b) `EffectContextTests.TestableVariableStore` — preserve full dictionary-backed testing infrastructure (all dictionaries, Setup/Was/GetSet helpers, getter/setter overrides for Source, Exp, Downbase, Palam + conditional GetDownbase Fail); (c) `ConsoleOutputDelegationTests.StubVariableStore` — rename to `DefaultStubVariableStore` and inherit from base | | [x] |
| 4 | 8,9 | Run `dotnet build Era.Core.Tests/` and `dotnet test Era.Core.Tests/` — verify zero build errors and zero test failures | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

**Example**:
```markdown
| 1 | 1 | Add API endpoint | | [ ] |        ← KNOWN: Expected response format is specified
| 2 | 2 | Calculate aggregates | [I] | [ ] | ← UNCERTAIN: Actual totals unknown until implemented
| 3 | 3 | Format report | | [ ] |           ← KNOWN: Uses Task 2's output (determined after Task 2)
```

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-816.md Technical Design (Interfaces/Data Structures section) + `Era.Core/Interfaces/IVariableStore.cs` + `engine.Tests/Tests/TestStubs.cs` (F815 reference) | `Era.Core.Tests/TestStubs.cs` with complete StubVariableStore base class |
| 2 | implementer | sonnet | feature-816.md Technical Design (refactoring patterns) + `Era.Core.Tests/` stub classes (33 Grep-visible + 2 gitignored Debug/ files) + `Era.Core.Tests/TestStubs.cs` from Phase 1 | 35 refactored stub files inheriting StubVariableStore (33 Grep-visible + 2 gitignored Debug/) |
| 3 | implementer | sonnet | feature-816.md Technical Design (behavioral stubs section) + `Era.Core.Tests/Infrastructure/VariableStoreAdapterTests.cs` + `Era.Core.Tests/Effects/EffectContextTests.cs` + `Era.Core.Tests/Shop/ConsoleOutputDelegationTests.cs` | 3 refactored behavioral stub files with preserved overrides |
| 4 | tester | sonnet | `Era.Core.Tests/` (all files post-refactor) | `dotnet build Era.Core.Tests/` PASS + `dotnet test Era.Core.Tests/` PASS |

### Pre-conditions

- F815 is [DONE]: `engine.Tests/Tests/TestStubs.cs` exists with F815 StubVariableStore pattern
- `Era.Core/Interfaces/IVariableStore.cs` lists all 44 methods to implement in base class
- F809 Counter work: coordinate timing to minimize merge conflicts in `Era.Core.Tests/Counter/`

### Execution Order

1. **Phase 1** (T1): Create `Era.Core.Tests/TestStubs.cs` first. All refactoring in Phases 2-3 depends on this file existing.
2. **Phase 2** (T2): Refactor 33 Grep-visible stubs + 2 gitignored Debug/ stubs. These are mechanical substitutions (`: IVariableStore` → `: StubVariableStore`, remove boilerplate, preserve behavioral overrides for stubs like ScomfCommandTests and ActionSelectorTests).
3. **Phase 3** (T3): Refactor 3 behavioral stubs requiring infrastructure judgment (multi-field state, ConcurrentDictionary pattern):
   - `VariableStoreAdapterTests.MockVariableStore`: Retain full ConcurrentDictionary-backed store pattern — `_store` field, `Key()` helpers, Get/Set overrides for MaxBase, Juel, CharacterFlag (getters return `Result.Fail` on miss, setters write to dictionary)
   - `EffectContextTests.TestableVariableStore`: Retain full dictionary-backed testing infrastructure — all dictionaries (`_sourceValues`, `_setSourceValues`, `_downbaseValues`, `_setDownbaseValues`, `_expValues`, `_setExpValues`, `_palamValues`), Setup/Was/GetSet helper methods, getter/setter overrides for Source, Exp, Downbase, Palam, and conditional `GetDownbase` Fail via `_downbaseFailures` HashSet
   - `ConsoleOutputDelegationTests.StubVariableStore`: Rename to `DefaultStubVariableStore`, inherit base, no overrides needed
4. **Phase 4** (T4): Run build + test to validate. No code changes in this phase.

### Build Verification Steps

```bash
# Phase 1 check (after creating TestStubs.cs)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core.Tests/'

# Phase 4 final check
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'
```

### Success Criteria

- Phase 1: `Era.Core.Tests/TestStubs.cs` exists, Grep `public virtual` count == 44, `internal class StubVariableStore : IVariableStore` matches
- Phase 2: Grep `class \w+ : StubVariableStore` count == 33 (36 Grep-visible total minus 3 behavioral stubs = 33 after Phase 2; Phase 3 adds 3 more) AND 2 gitignored Debug/ stubs verified via Read() per AC#15
- Phase 3: Grep `class \w+ : StubVariableStore` count == 36, Grep `class \w+ : IVariableStore` count == 1, `Result<int>.Fail` present in VariableStoreAdapterTests + EffectContextTests
- Phase 4: `dotnet build` exits 0, `dotnet test` exits 0 with 0 failures

### Error Handling

| Error | Action |
|-------|--------|
| CS0535 on any file after refactor | Missing method in base class — add to `TestStubs.cs` and re-verify AC#3 count |
| CS0104 ambiguity for StubVariableStore | ConsoleOutputDelegationTests rename not applied — apply Phase 3 rename immediately |
| Test failure after refactor | Behavioral stub lost override — check derived class has override for failing method, STOP if unclear |
| Method count != 44 | `IVariableStore.cs` may have changed — re-count interface methods and update base class |

### Rollback Plan

If issues arise after the feature is merged:
1. Run `git revert HEAD` (or the specific commit hash)
2. Report to user with details of which tests failed and which files were reverted
3. Create follow-up feature-{ID}.md to diagnose and re-attempt the refactoring

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| N/A | - | - | - | - |
<!-- No handoffs: all scope is contained within this feature -->

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
| 2026-02-24 10:15 | START | implementer | Task 1: Create TestStubs.cs | - |
| 2026-02-24 10:17 | END | implementer | Task 1: Create TestStubs.cs | SUCCESS |
| 2026-02-24 10:20 | START | implementer | Task 2: Refactor 35 stubs (3 batches) | - |
| 2026-02-24 11:05 | END | implementer | Task 2: Refactor 35 stubs | SUCCESS |
| 2026-02-24 11:10 | START | implementer | Task 3: Behavioral stubs (3 files) | - |
| 2026-02-24 11:15 | END | implementer | Task 3: Behavioral stubs | SUCCESS |
| 2026-02-24 11:16 | FIX | orchestrator | FQN→short form (3 files) | SUCCESS |
| 2026-02-24 11:17 | START | tester | Task 4: Build + Test | - |
| 2026-02-24 11:18 | END | tester | Task 4: Build 0err/0warn, Test 2562/2562 pass | SUCCESS |
| 2026-02-24 11:20 | DEVIATION | ac-static-verifier | code AC verification | exit 1: AC#13 wrong path (Training/ → Commands/Special/), AC#16 false positive (file-scoped Grep hits non-IVariableStore stubs) |
| 2026-02-24 11:21 | END | ac-tester | AC verification | 21/22 PASS, AC#16 FAIL (DEFINITION: Grep scope issue, implementation correct) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: AC#6 Expected | AC#6 count_equals Expected was 35 but should be 36 (36 existing stubs all refactored, base class is new not part of baseline)
- [fix] Phase2-Review iter2: AC#10,AC#11 Matcher | Changed single-line regex to multiline pattern `override.*Result<int>[\s\S]*?Fail` to handle block-body C# formatting
- [fix] Phase2-Review iter3: AC#13 Addition | Added AC#13 for ScomfCommandTests non-zero GetDownbase override preservation (C3 coverage gap for Goal#5)
- [fix] Phase2-Review iter4: AC#5,AC#6 Details | Corrected gitignored Debug/ files verification notes — AC#8/AC#9 don't verify inheritance vs direct implementation; implementer must explicitly refactor during T2
- [fix] Phase2-Review iter5: AC#14 Addition | Added AC#14 for ActionSelectorTests dictionary-based override preservation (Goal#5 coverage gap for dictionary-state stub category)
- [fix] Phase2-Review iter6: T2 Description | Removed contradictory 'safe-default-only' label from T2 (T2 includes stubs with behavioral overrides like ScomfCommandTests and ActionSelectorTests)
- [fix] Phase1-RefCheck iter1: 5 Whys Level 3 | Removed non-existent feature references (F393, F399, F400, F469, F563) from 5 Whys evidence; replaced with general description
- [info] Phase1-DriftChecked: F809 (Related)
- [fix] Phase2-Review iter1: Approach text | Updated 'all 12 ACs' to 'all 14 ACs' with AC#13/AC#14 enumeration
- [fix] Phase2-Review iter1: Mandatory Handoffs | Added placeholder comment for intentionally empty table
- [fix] Phase2-Review iter1: AC#5 Details | Added mandatory gitignored Debug/ file verification step for implementer
- [fix] Phase2-Review iter2: C10 Constraint + Details | Clarified C10 to accept blanket Option B decision (per Key Decisions), updated verification and AC Impact to reference AC#7+AC#9+AC#10/11/13/14 as combined coverage
- [fix] Phase2-Review iter3: T2 Description | Added 2 gitignored Debug/ files (F629DebugTests.cs, F629YamlDebugTests.cs) explicitly to T2 scope
- [fix] Phase2-Review iter3: AC#15 Addition | Added AC#15 Read-based verification for gitignored Debug/ files inheritance (closes C6 Grep gap)
- [fix] Phase2-Review iter3: AC#16 Addition | Added AC#16 spot-check for boilerplate removal in ComEquivalenceTestBase (closes C8/Goal#5 gap)
- [fix] Phase3-Maintainability iter4: T2 AC# | Added AC#16 to T2 AC# column (orphan AC fix)
- [fix] Phase3-Maintainability iter4: Impl Contract Phase 2 | Updated output from '33 refactored' to '35 refactored (33 Grep-visible + 2 gitignored Debug/)'
- [fix] Phase2-Review iter5: Goal#6 Coverage | Added structural guarantee logical proof (C# inheritance semantics → O(M) property by construction)
- [fix] Phase2-Review iter5: AC#15 Matcher | Changed from not_matches (negative) to matches (positive) ': StubVariableStore' for stronger verification
- [fix] Phase2-Review iter6: AC#10 + Impl Contract | Corrected Result.Fail method list from {GetMaxBase, GetJuel, GetGotJuel} to {GetMaxBase, GetJuel, GetCharacterFlag} per actual VariableStoreAdapterTests source code
- [fix] Phase2-Review iter7: AC#16 Target | Changed spot-check from ComEquivalenceTestBase (empty-body, tautological) to WcActionSelectorTests (has overrides, meaningful check)
- [fix] Phase2-Review iter8: Success Criteria Phase 2 | Added explicit Debug/ file verification note (AC#15) to Phase 2 success criteria
- [fix] Phase2-Review iter9: AC#16 Matcher | Fixed broken regex: moved (?!override) lookahead to position after `public ` (before return type) to correctly detect non-override C# method declarations
- [fix] Phase3-Maintainability iter10: T3 + Impl Contract Phase 3 | Expanded behavioral stub descriptions: VariableStoreAdapterTests (full ConcurrentDictionary pattern, not just Result.Fail) and EffectContextTests (full dictionary infrastructure, not just GetDownbase Fail)
- [fix] Phase4-ACValidation iter10: AC#15 Method | Changed from Read() to Grep(explicit path) — Grep with explicit file path bypasses .gitignore, enabling standard ac-static-verifier verification
- [fix] Phase2-Review iter1: Mandatory Handoffs Table | Added `| N/A | - | - | - | - |` placeholder row for valid markdown table structure
- [fix] Phase2-Uncertain iter1: AC#17 Addition | Added AC#17 safe default return value spot-check (Result<string>.Ok(string.Empty)) to cover AC#3 count-only gap
- [fix] Phase2-Uncertain iter1: AC#7 Matcher | Strengthened AC#7 to require two patterns: blanket decision + behavioral exception categories
- [fix] Phase2-Uncertain iter1: AC#10 Matcher | Changed from generic multiline Result.Fail to method-specific `override.*GetMaxBase|override.*GetJuel|override.*GetCharacterFlag`
- [fix] Phase2-Uncertain iter1: AC#11 Matcher | Changed to method-specific `override.*GetDownbase[\s\S]*?Fail` for GetDownbase-specific verification
- [fix] Phase2-Review iter2: AC#10 Matcher | Changed from OR-matches (at-least-one) to count_gte >= 3 requiring ALL three behavioral overrides (GetMaxBase, GetJuel, GetCharacterFlag)
- [fix] Phase2-Review iter3: AC#15 Matcher | Added negative `: IVariableStore` not_matches check alongside positive `: StubVariableStore` match to prevent partial-refactor gap
- [fix] Phase2-Review iter3: Execution Order Phase 2 | Changed 'safe-default stubs' to 'Grep-visible stubs + gitignored Debug/ stubs' with behavioral override preservation note
- [fix] Phase2-Review iter4: AC#18 Addition | Added AC#18 for VariableStoreAdapterTests setter overrides (SetMaxBase, SetJuel, SetCharacterFlag) — AC#10 only covered getters
- [fix] Phase2-Review iter4: AC#5 Gitignored Assumption | Added explicit assumption (exactly 2 gitignored files) with T2 pre-condition to verify no additional gitignored stubs exist
- [fix] Phase2-Review iter5: AC#7 Pattern 1 | Tightened from OR (NIE|F815|safe default) to co-occurrence (NIE.*safe default) requiring both problem and decision on same line
- [fix] Phase2-Review iter6: AC#5 Task Assignment | Moved AC#5 from T2 to T3 (AC#5 count=1 only achievable after T3 completes all behavioral stubs)
- [fix] Phase2-Review iter6: AC#6 Matcher | Changed from count_equals 36 to count_gte 36 (resilient to successor features adding new stubs)
- [fix] Phase2-Review iter6: AC#15 Split | Split compound AC#15 (matches+not_matches) into AC#15a (matches) and AC#15b (not_matches) per standard matcher list
- [fix] Phase2-Review iter7: AC#7 Code Sample | Restructured NIE comment to place NotImplementedException and safe defaults on same line (single-line Grep match)
- [fix] Phase2-Review iter7: AC#6 Task Assignment | Moved AC#6 from T2 to T3 (count_gte >= 36 only achievable after T3 completes)
- [fix] Phase2-Review iter8: AC#14 Matcher | Changed from matches override.*GetFlag to count_gte >= 3 for override.*(GetFlag|GetCharacterFlag|GetTCVar) — all three dictionary-backed overrides
- [fix] Phase2-Review iter9: Philosophy Derivation | Added AC#17 to 'F815 pattern' row and new C3 behavioral preservation row with AC#10/11/13/14/18 — closed traceability gap
- [fix] Phase2-Review iter9: Execution Order T3 Label | Changed 'behavioral stubs requiring judgment' to 'behavioral stubs requiring infrastructure judgment (multi-field state, ConcurrentDictionary pattern)' to distinguish from T2's simple overrides
- [fix] Phase4-ACValidation iter10: AC count text | Fixed 'All 18 ACs' → 'All 16 ACs' to match ac_ops count (sub-numbered ACs count as single base)
- [fix] Phase4-ACValidation iter10: Matcher names | Changed count_gte → gte for AC#6, AC#10, AC#14, AC#18 (valid matcher per SSOT)
- [fix] Phase4-ACValidation iter10: AC#7 Split | Split AC#7 into AC#7a (blanket decision) and AC#7b (exception categories) — single matches matcher per AC
- [fix] Phase4-ACValidation iter10: AC#11 Method | Removed ', multiline' from Grep method ([\s\S] handles cross-line matching)
- [fix] Phase4-ACValidation iter10: AC#15a/15b Split | Split multi-Grep AC#15a into 15a1/15a2 and AC#15b into 15b1/15b2 — single Grep per AC row
- [fix] Phase2-Review iter1: T2 AC# Column | Updated T2 AC# from '13,14,15a,15b,16' to '13,14,15a1,15a2,15b1,15b2,16' to match AC Definition Table sub-numbering

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F815](feature-815.md) - Established StubVariableStore base class pattern in engine.Tests
- [Related: F801](feature-801.md) - Added GetEquip/SetEquip to IVariableStore, triggering the original deviation discovery
- [Related: F804](feature-804.md) - Added GetCharacterString/GetExpLv/GetNoItem to IVariableStore
- [Related: F809](feature-809.md) - Added SetExpLv default interface method
- [Successor: F803](feature-803.md) - Future IVariableStore extensions benefit from this feature
