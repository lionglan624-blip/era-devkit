# Feature 794: Shared Body Option Validation Abstraction

## Status: [DONE]
<!-- initializer: 2026-02-21T07:29:00Z -->

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

## Review Context

### Origin

| Field | Value |
|-------|-------|
| Parent Feature | F781 (Visitor Settings) |
| Discovery Point | FL POST-LOOP Step 6.3 |
| Timestamp | 2026-02-17 |

### Identified Gap

F781 (Visitor Settings) and F779 (Body Settings UI) implement identical body option algorithms. Four methods (ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize) and one constant (ExclusiveRanges) are duplicated across both classes with different variable access scopes.

### Review Evidence

| Gap Source | Derived Task | Comparison Result | DEFER Reason |
|-----------|-------------|-------------------|--------------|
| F781 Mandatory Handoff (feature-781.md:818) | Extract shared algorithms into BodyOptionValidator | Algorithms identical; variable access differs | Deferred until both F779 and F781 [DONE] |
| F779 Key Decision Option C (feature-779.md:575) | Pure-function extraction with slot values as params | Pre-designed extraction path | N/A (extraction path pre-planned) |

### Files Involved

| File | Role |
|------|------|
| Era.Core/State/BodySettings.cs | Consumer: character-scoped algorithms (IVariableStore) |
| Era.Core/State/VisitorSettings.cs | Consumer: visitor-scoped algorithms (IVisitorVariables) |
| Era.Core/State/BodyOptionValidator.cs | Target: shared pure-function implementations (new) |

### Parent Review Observations

- Deduplication (cascading duplicate removal), slot compaction, and derived value sync remain in respective classes — tightly interleaved with variable reads/writes (C10, C11)
- Only the variable access differs between consumers; algorithms operate on resolved int values (F779 Option C insight)

<!-- fc-phase-1-completed -->

---

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; F647 decomposed it into actionable sub-features.

### Problem (Current Issue)

BodySettings (F779) and VisitorSettings (F781) contain four algorithmically identical validation/calculation methods (ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize) plus a shared ExclusiveRanges constant, along with structurally similar but variable-access-interleaved Tidy orchestration (deduplication, mutual exclusion validation, compaction). Both were independently migrated from the same ERB source (`体設定.ERB` @体詳細整頓 at line 743 and @体詳細整頓訪問者 at line 1786) with different variable access scopes (character-scoped `IVariableStore.GetCharacterFlag` at `BodySettings.cs:506-510` vs global `IVisitorVariables.GetBodyOption1()` at `VisitorSettings.cs:54`). The extraction was deliberately deferred until both F779 and F781 reached [DONE] (`feature-781.md:818` Mandatory Handoffs), since no common abstraction for "reading/writing option slot values" existed when either was implemented. F779 Key Decision Option C (`feature-779.md:575`) pre-designed the extraction approach: pure functions accepting slot values as parameters, decoupling algorithms from variable access.

### Goal (What to Achieve)

Extract the four duplicated validation/calculation algorithms (ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize) and the shared ExclusiveRanges constant into a shared utility class (BodyOptionValidator) with pure-function signatures that accept slot values as parameters per F779 Option C. Both BodySettings and VisitorSettings delegate to the shared implementation, preserving their existing public interfaces (IBodySettings, IVisitorSettings) and behavioral differences (SyncDerivedValues visitor-only, Tidy orchestration granularity including deduplication and compaction which remain in respective classes). All 104 existing tests pass without regression.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do six identical algorithms exist in two classes? | BodySettings and VisitorSettings independently migrated the same ERB algorithms from `体設定.ERB` for different data sources | `BodySettings.cs:512-713`, `VisitorSettings.cs:15-297` |
| 2 | Why were they migrated independently? | F779 (body/character-scoped CFLAG) and F781 (visitor/global SAVEDATA) were designed as independent features with different variable access models | `IVariableStore.cs:28` vs `IVisitorVariables.cs:19` |
| 3 | Why was no shared abstraction created during implementation? | Variable access differs fundamentally: `GetCharacterFlag(CharacterId, CharacterFlagIndex)` returning `Result<int>` vs `GetBodyOption1()` returning `int` -- making extraction non-trivial | `BodySettings.cs:506-510` vs `VisitorSettings.cs:54` |
| 4 | Why was extraction not attempted at that time? | Extraction was deliberately deferred until both F779 and F781 reached [DONE] to avoid blocking downstream features (F780, F782) | `feature-781.md:818` (Mandatory Handoffs) |
| 5 | Why (Root)? | Pipeline Continuity philosophy prioritizes completing individual features to unblock dependencies over cross-cutting refactoring during active development; F779 pre-designed the extraction path (Option C: pure functions with slot values as params) for F794 | `feature-779.md:575` (Key Decision Option C) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Six identical algorithms duplicated across BodySettings.cs and VisitorSettings.cs | Independent migration from shared ERB source with incompatible variable access interfaces; no common pure-function abstraction layer |
| Where | `BodySettings.cs:512-713` and `VisitorSettings.cs:15-297` | Architecture gap: two variable access models (IVariableStore character-scoped vs IVisitorVariables global-scoped) with no shared algorithm layer |
| Fix | Copy-paste code and maintain both | Extract pure-function validation into shared BodyOptionValidator class per F779 Option C; both classes delegate to shared implementation |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F779 | [DONE] | Predecessor -- Body Settings UI; source of character-scoped algorithms and Option C design decision |
| F781 | [DONE] | Predecessor -- Visitor Settings; source of visitor-scoped algorithms and Mandatory Handoff to F794 |
| F780 | [PROPOSED] | Related -- Genetics & Growth calls @体詳細整頓 via ERB, not C#; unaffected by F794 |
| F782 | [DRAFT] | Successor -- Post-Phase Review depends on F794 |
| F796 | [PROPOSED] | Related -- BodyDetailInit migration also modifies BodySettings.cs; coordinate execution order |
| F789 | [DONE] | Related -- IStringVariables interface pattern (precedent for accessor abstraction) |
| F790 | [DONE] | Related -- IEngineVariables interface pattern (precedent for accessor abstraction) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Algorithm equivalence | FEASIBLE | All 6 algorithms identical across both implementations |
| Pure-function extraction | FEASIBLE | ValidateSimpleDuplicate and CalculatePenisSize already stateless; others extractable via Option C |
| Behavioral variant handling | FEASIBLE | SyncDerivedValues well-documented as visitor-only; Tidy orchestration stays in respective classes |
| Interface backward compatibility | FEASIBLE | Preserve existing IBodySettings and IVisitorSettings contracts; internal delegation only |
| Test coverage | FEASIBLE | 104 existing tests (48 in BodySettingsBusinessLogicTests, 56 in VisitorSettingsTests) provide regression safety |
| Design pre-made | FEASIBLE | F779 Key Decisions specify Option C pure-function extraction path |
| Dependencies satisfied | FEASIBLE | Both F779 and F781 are [DONE] |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/State | HIGH | BodySettings.cs and VisitorSettings.cs refactored to delegate shared algorithms |
| Era.Core/State (new) | HIGH | New BodyOptionValidator.cs created with shared pure-function implementations |
| Era.Core/Interfaces | LOW | IBodySettings and IVisitorSettings signatures preserved; no contract changes |
| Era.Core.Tests | MEDIUM | Existing tests may need minor adaptation; new tests for shared validator |
| DI registration | LOW | BodyOptionValidator is static utility (no DI registration needed for pure functions) |
| F782 Post-Phase Review | MEDIUM | Unblocked by F794 completion |
| F796 BodyDetailInit | LOW | Shares BodySettings.cs modification target; coordinate execution order |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| SyncDerivedValues NOT in shared code | `feature-779.md:675` | Shared Tidy/orchestrator must exclude SyncDerivedValues; visitor class retains its own sync call |
| Tidy public API divergence | `feature-779.md:676` | BodySettings has single Tidy(); VisitorSettings exposes Deduplicate/Validate/Compact separately; extract leaf algorithms only |
| ValidateBodyOption Option C pure-function signature | `feature-779.md:575,677` | Shared validation functions accept slot values as parameters, not interface references |
| IBodySettings requires characterId | `IBodySettings.cs:18-23` | Shared functions must not impose characterId; callers bind characterId before delegation |
| GetTightnessBaseValue is BodySettings-only | `BodySettings.cs:715-726` | Not extractable; stays in BodySettings |
| ResetOption/ValidateMenuSelection VisitorSettings-only | `VisitorSettings.cs:285-344` | Not extractable; stays in VisitorSettings |
| TreatWarningsAsErrors | `Directory.Build.props` | Zero warnings required in all new and modified code |
| F796 parallel modification | `feature-796.md` | Both F794 and F796 modify BodySettings.cs; execution order coordination needed |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Breaking IBodySettings/IVisitorSettings API | LOW | HIGH | Preserve existing signatures; internal delegation only |
| Dedup/Compact complex abstraction | MEDIUM | MEDIUM | Follow Option C pure-function approach; extract leaf algorithms, not orchestrators |
| SyncDerivedValues false symmetry | LOW | HIGH | Document asymmetry explicitly; AC verifies shared Tidy excludes sync |
| Over-engineering shared abstraction | MEDIUM | MEDIUM | Static utility with pure functions keeps complexity minimal while remaining open to additional consumers. No accessor interface needed because algorithms operate on resolved int values, decoupled from variable access by design |
| F796 merge conflict on BodySettings.cs | MEDIUM | LOW | Execute F794 before F796; coordinate file modifications |
| F780 cross-call regression | LOW | MEDIUM | F780 calls ERB @体詳細整頓, not C# Tidy; Tidy signature preserved |
| Test regression | LOW | HIGH | 104 existing tests as comprehensive regression net |
| Scope creep into F796 territory | MEDIUM | MEDIUM | Strict scope boundary: BodyDetailInit migration is F796 scope |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| BodySettings duplicate lines | Grep ExclusiveRanges in Era.Core/State/ | 2 definitions | Target: 1 definition after extraction |
| Existing test count | dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings" | 104 tests (48+56) | All must pass post-refactor |
| ValidateBodyOption definitions | Grep "ValidateBodyOption" in Era.Core/State/ | 2 implementations | Target: 1 shared + 2 delegating |

**Baseline File**: `.tmp/baseline-794.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | ExclusiveRanges must have single definition | `BodySettings.cs:512-513`, `VisitorSettings.cs:15-16` | AC verify grep count_equals 1 in shared location |
| C2 | Pure-function signatures (no interface dependency) | `feature-779.md:575` Option C | AC verify shared methods accept slot values as params, not IVariableStore/IVisitorVariables |
| C3 | SyncDerivedValues NOT in shared code | `feature-779.md:675` | AC verify shared Tidy/pipeline excludes SyncDerivedValues |
| C4 | Both classes delegate to shared code | Duplication evidence across all 3 investigations | AC verify BodySettings and VisitorSettings call shared validator methods |
| C5 | Existing interfaces preserved | `IBodySettings.cs`, `IVisitorSettings.cs`, DI registrations | AC verify IBodySettings and IVisitorSettings signatures unchanged |
| C6 | All existing tests pass | `BodySettingsBusinessLogicTests.cs` (27 methods / 48 cases), `VisitorSettingsTests.cs` (29 methods / 56 cases) = 104 total | AC verify zero test regression |
| C7 | ValidateBodyOption returns int 0/1 | `feature-779.md:571` ERB RETURNF convention | AC verify return type is int |
| C8 | Zero technical debt in new code | CLAUDE.md Zero Debt Upfront | AC verify no TODO/FIXME/HACK in new and modified files |
| C9 | Domain-specific methods stay in their classes | `BodySettings.cs:715-726` (GetTightnessBaseValue), `VisitorSettings.cs:285-344` (ResetOption, ValidateMenuSelection) | AC verify these methods NOT extracted |
| C10 | Tidy orchestration stays in respective classes | `feature-779.md:676` | AC verify Tidy method remains in each class, delegates to shared leaf algorithms |
| C11 | Deduplication and compaction NOT extracted to shared code | Key Decision 5 (Option A rationale) | AC verify Deduplicate/Compact remain in VisitorSettings; dedup logic in BodySettings.Tidy stays inline |

### Constraint Details

**C1: ExclusiveRanges Single Definition**
- **Source**: Both BodySettings.cs:512-513 and VisitorSettings.cs:15-16 define identical `(int Min, int Max)[] ExclusiveRanges = [(1, 9), (10, 29), (30, 49), (50, 54), (55, 59)]`
- **Verification**: Grep for ExclusiveRanges in Era.Core/State/ should return exactly 1 definition after extraction
- **AC Impact**: Use count_equals matcher to verify single definition location

**C2: Pure-Function Signatures**
- **Source**: F779 Key Decision Option C (`feature-779.md:575`) specifies "pass all slot values as parameters" for F794 extraction
- **Verification**: Shared validation functions must not reference IVariableStore, IVisitorVariables, or any state interface
- **AC Impact**: Grep shared file for absence of interface references; verify parameter lists contain slot values

**C3: SyncDerivedValues Exclusion**
- **Source**: F779 Upstream Issue (`feature-779.md:675`) documents body version does NOT sync derived values
- **Verification**: Shared validator class must not contain SyncDerivedValues method or derived value sync logic
- **AC Impact**: Grep shared file for absence of SyncDerivedValues

**C4: Delegation Verification**
- **Source**: All 3 investigations confirm both classes must delegate to shared implementation
- **Verification**: Grep both BodySettings.cs and VisitorSettings.cs for calls to shared validator methods
- **AC Impact**: Verify both classes contain call references to BodyOptionValidator

**C5: Interface Backward Compatibility**
- **Source**: IBodySettings.cs (7 methods), IVisitorSettings.cs (11 methods), ServiceCollectionExtensions.cs:149,152
- **Verification**: Interface method counts and signatures must remain identical
- **AC Impact**: count_equals for interface method count; preserve exact signatures

**C6: Test Regression Zero**
- **Source**: 104 existing tests across both test files
- **Verification**: dotnet test with filter for both test classes must pass all
- **AC Impact**: dotnet test succeeds matcher

**C7: Int Return Type Convention**
- **Source**: F779 Key Decision (`feature-779.md:571`) selected int (0/1) over bool to match ERB RETURNF convention
- **Verification**: Shared ValidateBodyOption/ValidatePenisOption/ValidateSimpleDuplicate return int
- **AC Impact**: Grep for return type in shared class

**C8: Zero Technical Debt**
- **Source**: CLAUDE.md Zero Debt Upfront principle
- **Verification**: Grep new and modified files for TODO|FIXME|HACK
- **AC Impact**: not_matches for debt markers

**C9: Domain-Specific Method Preservation**
- **Source**: GetTightnessBaseValue exists only in BodySettings; ResetOption/ValidateMenuSelection exist only in VisitorSettings
- **Verification**: These methods must remain in their respective classes, not extracted
- **AC Impact**: Grep to verify presence in original locations

**C10: Tidy Orchestration Preservation**
- **Source**: F779 Upstream Issue (`feature-779.md:676`) documents divergent Tidy APIs
- **Verification**: BodySettings.Tidy(int characterId) and VisitorSettings.Tidy() remain as orchestrators
- **AC Impact**: Verify Tidy methods exist in both classes, calling shared leaf algorithms

**C11: Deduplication/Compaction NOT Extracted**
- **Source**: Key Decision 5 (Tidy leaf algorithms extraction scope) — Option A selected: "Extract only the 4 validation/calculation methods"
- **Rationale**: Deduplication and compaction in BodySettings.Tidy and VisitorSettings.Deduplicate/Compact are tightly interleaved with variable reads/writes; extracting them as pure functions would require passing large arrays of slot values, gaining no maintainability benefit for only 2 consumers
- **Verification**: VisitorSettings retains `public void Deduplicate()` and `public void Compact()`; BodySettings.Tidy retains inline deduplication/compaction logic
- **AC Impact**: AC#25 verifies Deduplicate/Compact remain in VisitorSettings

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F781 | [DONE] | Visitor Settings - source of visitor-scoped algorithms; Mandatory Handoff to F794 (`feature-781.md:818`) |
| Predecessor | F779 | [DONE] | Body Settings UI - source of character-scoped algorithms; Option C design decision (`feature-779.md:575`) |
| Successor | F782 | [DRAFT] | Post-Phase Review - depends on F794 (`index-features.md:64`) |
| Related | F780 | [PROPOSED] | Genetics & Growth - calls @体詳細整頓 via ERB, not C#; unaffected |
| Related | F796 | [DONE] | BodyDetailInit migration - also modifies BodySettings.cs; coordinate execution order |

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
| "Pipeline Continuity - Each phase completion triggers next phase planning" | F779/F781 completion triggers F794 extraction as pre-planned by F779 Option C; shared abstraction must exist | AC#1, AC#2 |
| F779 Key Decision Option C (pure-function extraction mandate) + DRY principle | Both consumers must delegate to shared pure-function code with resolved int params, eliminating duplication | AC#3, AC#4, AC#5, AC#9, AC#15, AC#16, AC#17, AC#18, AC#27, AC#29, AC#31, AC#32, AC#33, AC#35 |
| "documented transition points" | Existing interfaces and test suites preserved as documented contracts; new public API requires isolated test coverage | AC#6, AC#7, AC#8, AC#21, AC#22, AC#23, AC#24, AC#26, AC#30, AC#34 |
| SyncDerivedValues asymmetry + Tidy orchestration divergence (F779 Upstream Issues) | Shared code must not include visitor-only behavior; orchestrators stay in respective classes | AC#10, AC#11, AC#12, AC#13, AC#14, AC#28 |
| DRY exemption for dedup/compact (Key Decision 5, C11) | Dedup/compact tightly interleaved with variable reads/writes; exempt from extraction by design | AC#25 |
| Zero Debt Upfront (CLAUDE.md) | No TODO/FIXME/HACK in new/modified code; build must succeed with zero warnings | AC#19, AC#20 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | BodyOptionValidator.cs exists | file | Glob(Era.Core/State/BodyOptionValidator.cs) | exists | - | [x] |
| 2 | ExclusiveRanges single definition in BodyOptionValidator | code | Grep(Era.Core/State/BodyOptionValidator.cs) | matches | `ExclusiveRanges` | [x] |
| 3 | ExclusiveRanges removed from BodySettings and VisitorSettings | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs) | not_matches | `ExclusiveRanges\s*=` | [x] |
| 4 | BodySettings delegates to BodyOptionValidator | code | Grep(Era.Core/State/BodySettings.cs) | matches | `BodyOptionValidator\.` | [x] |
| 5 | VisitorSettings delegates to BodyOptionValidator | code | Grep(Era.Core/State/VisitorSettings.cs) | matches | `BodyOptionValidator\.` | [x] |
| 6 | IBodySettings interface preserved (7 methods) | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | count_equals | `^\s+(void|int)\s+\w+\(` = 7 | [x] |
| 7 | IVisitorSettings interface preserved (11 methods) | code | Grep(Era.Core/Interfaces/IVisitorSettings.cs) | count_equals | `^\s+(void|int|bool)\s+\w+\(` = 11 | [x] |
| 8 | All existing tests pass | test | dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings" | succeeds | - | [x] |
| 9 | Pure-function signatures (no interface dependency) | code | Grep(Era.Core/State/BodyOptionValidator.cs) | not_matches | `IVariableStore|IVisitorVariables` | [x] |
| 10 | SyncDerivedValues not in shared code | code | Grep(Era.Core/State/BodyOptionValidator.cs) | not_matches | `SyncDerivedValues` | [x] |
| 11 | Tidy method preserved in BodySettings | code | Grep(Era.Core/State/BodySettings.cs) | matches | `public void Tidy\(int characterId\)` | [x] |
| 12 | Tidy method preserved in VisitorSettings | code | Grep(Era.Core/State/VisitorSettings.cs) | matches | `public void Tidy\(\)` | [x] |
| 13 | GetTightnessBaseValue remains in BodySettings | code | Grep(Era.Core/State/BodySettings.cs) | matches | `public int GetTightnessBaseValue\(int selectionIndex\)` | [x] |
| 14 | ResetOption and ValidateMenuSelection remain in VisitorSettings | code | Grep(Era.Core/State/VisitorSettings.cs) | count_equals | `public (void ResetOption|bool ValidateMenuSelection)` = 2 | [x] |
| 15 | Shared ValidateBodyOption returns int | code | Grep(Era.Core/State/BodyOptionValidator.cs) | matches | `static.*int ValidateBodyOption\(` | [x] |
| 16 | Shared ValidatePenisOption returns int | code | Grep(Era.Core/State/BodyOptionValidator.cs) | matches | `static.*int ValidatePenisOption\(` | [x] |
| 17 | Shared ValidateSimpleDuplicate returns int | code | Grep(Era.Core/State/BodyOptionValidator.cs) | matches | `static.*int ValidateSimpleDuplicate\(` | [x] |
| 18 | Shared CalculatePenisSize returns int | code | Grep(Era.Core/State/BodyOptionValidator.cs) | matches | `static.*int CalculatePenisSize\(` | [x] |
| 19 | Zero technical debt in new and modified files | code | Grep(Era.Core/State/BodyOptionValidator.cs,Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs) | not_matches | `TODO|FIXME|HACK` | [x] |
| 20 | Build succeeds with zero warnings | build | dotnet build Era.Core | succeeds | - | [x] |
| 21 | BodyOptionValidatorTests.cs exists | file | Glob(Era.Core.Tests/State/BodyOptionValidatorTests.cs) | exists | - | [x] |
| 22 | BodyOptionValidator unit tests pass | test | dotnet test Era.Core.Tests --filter "BodyOptionValidator" | succeeds | - | [x] |
| 23 | IBodySettings shared method signatures preserved | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | count_equals | `int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` = 4 | [x] |
| 24 | IVisitorSettings shared method signatures preserved | code | Grep(Era.Core/Interfaces/IVisitorSettings.cs) | count_equals | `int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` = 4 | [x] |
| 25 | Deduplicate and Compact remain in VisitorSettings | code | Grep(Era.Core/State/VisitorSettings.cs) | count_equals | `public void (Deduplicate|Compact)\(\)` = 2 | [x] |
| 26 | Existing test method count preserved (no test deletion) | code | Grep(Era.Core.Tests/State/BodySettingsBusinessLogicTests.cs,Era.Core.Tests/State/VisitorSettingsTests.cs) | count_equals | `\[(Fact|Theory)\]` = 56 | [x] |
| 27 | ExclusiveRanges has internal visibility | code | Grep(Era.Core/State/BodyOptionValidator.cs) | matches | `internal.*ExclusiveRanges` | [x] |
| 28 | SyncDerivedValues still called in VisitorSettings | code | Grep(Era.Core/State/VisitorSettings.cs) | matches | `SyncDerivedValues\(\)` | [x] |
| 29 | Algorithm bodies removed from consumer classes | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs) | not_matches | `rawInput - 2` | [x] |
| 30 | Test case count preserved (104 cases) | test | dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings" --verbosity normal | succeeds | 104 passed in output | [x] |
| 31 | No Era.Core.Interfaces import in shared code | code | Grep(Era.Core/State/BodyOptionValidator.cs) | not_matches | `using Era.Core.Interfaces` | [x] |
| 32 | ValidateBodyOption algorithm body removed from consumer classes | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs) | not_matches | `foreach.*ExclusiveRanges` | [x] |
| 33 | ValidatePenisOption algorithm body removed from consumer classes | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs) | not_matches | `candidateInRange && otherInRange` | [x] |
| 34 | BodyOptionValidator test method count | code | Grep(Era.Core.Tests/State/BodyOptionValidatorTests.cs) | gte | `\[(Fact|Theory)\]` >= 4 | [x] |
| 35 | ValidateSimpleDuplicate algorithm body removed from consumer classes | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs) | not_matches | `candidateValue != otherSlotValue \? 1 : 0` | [x] |

### AC Details

**AC#1: BodyOptionValidator.cs exists**
- **Test**: Glob pattern="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: File exists
- **Rationale**: The shared utility class must exist as a new file in Era.Core/State/ alongside BodySettings.cs and VisitorSettings.cs. (C4)

**AC#2: ExclusiveRanges single definition in BodyOptionValidator**
- **Test**: Grep pattern=`ExclusiveRanges` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern found (the single canonical definition lives here)
- **Rationale**: C1 requires ExclusiveRanges to have exactly one definition. This AC verifies the shared location contains it. Combined with AC#3, ensures single definition.

**AC#3: ExclusiveRanges removed from BodySettings and VisitorSettings**
- **Test**: Grep pattern=`ExclusiveRanges\s*=` path="Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern NOT found (definitions removed from both consumers)
- **Rationale**: C1 requires single definition. AC#2 confirms it exists in shared code; this AC confirms the duplicate definitions are removed from both consumer classes.

**AC#4: BodySettings delegates to BodyOptionValidator**
- **Test**: Grep pattern=`BodyOptionValidator\.` path="Era.Core/State/BodySettings.cs"
- **Expected**: Pattern found (at least one call to shared validator)
- **Rationale**: C4 requires both classes delegate to shared implementation. This verifies BodySettings calls shared methods.

**AC#5: VisitorSettings delegates to BodyOptionValidator**
- **Test**: Grep pattern=`BodyOptionValidator\.` path="Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern found (at least one call to shared validator)
- **Rationale**: C4 requires both classes delegate to shared implementation. This verifies VisitorSettings calls shared methods.

**AC#6: IBodySettings interface preserved (7 methods)**
- **Test**: Grep pattern=`^\s+(void|int)\s+\w+\(` path="Era.Core/Interfaces/IBodySettings.cs" | count
- **Expected**: 7 (BodyDetailInit, Tidy, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize, GetTightnessBaseValue)
- **Rationale**: C5 requires IBodySettings signatures unchanged. Method count verification ensures no accidental additions or removals.

**AC#7: IVisitorSettings interface preserved (11 methods)**
- **Test**: Grep pattern=`^\s+(void|int|bool)\s+\w+\(` path="Era.Core/Interfaces/IVisitorSettings.cs" | count
- **Expected**: 11 (Tidy, Deduplicate, ValidateAndClearExclusiveOptions, Compact, SyncDerivedValues, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, ValidateMenuSelection, CalculatePenisSize, ResetOption)
- **Rationale**: C5 requires IVisitorSettings signatures unchanged. Method count verification ensures no accidental additions or removals.

**AC#8: All existing tests pass**
- **Test**: `dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings"`
- **Expected**: All tests pass (104 test cases from 56 test methods: 27 methods in BodySettingsBusinessLogicTests generating 48 cases via Theory, 29 methods in VisitorSettingsTests generating 56 cases via Theory)
- **Rationale**: C6 requires zero test regression. This is the primary regression safety net. AC#26 guards against test deletion (56 test methods); AC#8 guards against test failure.

**AC#9: Pure-function signatures (no interface dependency)**
- **Test**: Grep pattern=`IVariableStore|IVisitorVariables` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern NOT found
- **Rationale**: C2 requires shared methods accept slot values as parameters per F779 Option C, not interface references. The shared class must be decoupled from variable access.

**AC#10: SyncDerivedValues not in shared code**
- **Test**: Grep pattern=`SyncDerivedValues` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern NOT found
- **Rationale**: C3 requires SyncDerivedValues to remain visitor-only. Shared code must not contain derived value sync logic.

**AC#11: Tidy method preserved in BodySettings**
- **Test**: Grep pattern=`public void Tidy\(int characterId\)` path="Era.Core/State/BodySettings.cs"
- **Expected**: Pattern found
- **Rationale**: C10 requires Tidy orchestration to stay in respective classes. BodySettings.Tidy(int) remains as the orchestrator delegating to shared leaf algorithms.

**AC#12: Tidy method preserved in VisitorSettings**
- **Test**: Grep pattern=`public void Tidy\(\)` path="Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern found
- **Rationale**: C10 requires Tidy orchestration to stay in respective classes. VisitorSettings.Tidy() remains as the orchestrator (calling Deduplicate, ValidateAndClearExclusiveOptions, Compact, SyncDerivedValues).

**AC#13: GetTightnessBaseValue remains in BodySettings**
- **Test**: Grep pattern=`public int GetTightnessBaseValue\(int selectionIndex\)` path="Era.Core/State/BodySettings.cs"
- **Expected**: Pattern found
- **Rationale**: C9 requires domain-specific methods to stay in their classes. GetTightnessBaseValue is BodySettings-only logic.

**AC#14: ResetOption and ValidateMenuSelection remain in VisitorSettings**
- **Test**: Grep pattern=`public (void ResetOption|bool ValidateMenuSelection)` path="Era.Core/State/VisitorSettings.cs" | count
- **Expected**: 2 matches (one for each method)
- **Rationale**: C9 requires domain-specific methods to stay in their classes. ResetOption and ValidateMenuSelection are VisitorSettings-only logic.

**AC#15: Shared ValidateBodyOption returns int**
- **Test**: Grep pattern=`static.*int ValidateBodyOption\(` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern found
- **Rationale**: C7 requires int return type (0/1) per ERB RETURNF convention. C2 requires static pure function. This verifies the shared method has the correct signature.

**AC#16: Shared ValidatePenisOption returns int**
- **Test**: Grep pattern=`static.*int ValidatePenisOption\(` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern found
- **Rationale**: C7 requires int return type. Validates the shared penis option validation has correct static int signature.

**AC#17: Shared ValidateSimpleDuplicate returns int**
- **Test**: Grep pattern=`static.*int ValidateSimpleDuplicate\(` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern found
- **Rationale**: C7 requires int return type. Validates the shared simple duplicate check has correct static int signature.

**AC#18: Shared CalculatePenisSize returns int**
- **Test**: Grep pattern=`static.*int CalculatePenisSize\(` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern found
- **Rationale**: C7 requires int return type. Validates the shared penis size calculation has correct static int signature.

**AC#19: Zero technical debt in new and modified files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path="Era.Core/State/BodyOptionValidator.cs,Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern NOT found in any of the three files
- **Rationale**: C8 requires zero technical debt per CLAUDE.md Zero Debt Upfront principle.

**AC#20: Build succeeds with zero warnings**
- **Test**: `dotnet build Era.Core`
- **Expected**: Build succeeds (TreatWarningsAsErrors in Directory.Build.props ensures zero warnings)
- **Rationale**: All new and modified code must compile cleanly. TreatWarningsAsErrors policy ensures no warnings slip through.

**AC#21: BodyOptionValidatorTests.cs exists**
- **Test**: Glob pattern="Era.Core.Tests/State/BodyOptionValidatorTests.cs"
- **Expected**: File exists
- **Rationale**: New public API (BodyOptionValidator) requires dedicated unit tests per TDD principle. Task 2 creates this file.

**AC#22: BodyOptionValidator unit tests pass**
- **Test**: `dotnet test Era.Core.Tests --filter "BodyOptionValidator"`
- **Expected**: All tests pass
- **Rationale**: The new BodyOptionValidator tests verify the shared pure-function implementations independently of BodySettings/VisitorSettings delegation wrappers.

**AC#23: IBodySettings shared method signatures preserved**
- **Test**: Grep pattern=`int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` path="Era.Core/Interfaces/IBodySettings.cs" | count
- **Expected**: 4 (one each for ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize)
- **Rationale**: C5 requires IBodySettings signatures unchanged. Combined with AC#6 (total count = 7), this verifies the four shared method signatures specifically exist on the interface. Stronger than count-only verification.

**AC#24: IVisitorSettings shared method signatures preserved**
- **Test**: Grep pattern=`int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` path="Era.Core/Interfaces/IVisitorSettings.cs" | count
- **Expected**: 4 (one each for ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize)
- **Rationale**: C5 requires IVisitorSettings signatures unchanged. Combined with AC#7 (total count = 11), this verifies the four shared method signatures specifically exist on the interface. Stronger than count-only verification.

**AC#25: Deduplicate and Compact remain in VisitorSettings**
- **Test**: Grep pattern=`public void (Deduplicate|Compact)\(\)` path="Era.Core/State/VisitorSettings.cs" | count
- **Expected**: 2 (one Deduplicate, one Compact)
- **Rationale**: C11 requires deduplication and compaction to remain in their respective classes, not extracted to shared code. VisitorSettings.Deduplicate() and VisitorSettings.Compact() must remain as class methods.

**AC#26: Existing test method count preserved (no test deletion)**
- **Test**: Grep pattern=`\[(Fact|Theory)\]` path="Era.Core.Tests/State/BodySettingsBusinessLogicTests.cs,Era.Core.Tests/State/VisitorSettingsTests.cs" | count
- **Expected**: 56 (27 in BodySettingsBusinessLogicTests + 29 in VisitorSettingsTests)
- **Rationale**: AC#8 verifies all tests pass but uses `succeeds` matcher which cannot detect test deletion. This count-based AC guards against accidental test deletion during refactoring. Baseline: 27 + 29 = 56 test methods generating 104 test cases.

**AC#27: ExclusiveRanges has internal visibility**
- **Test**: Grep pattern=`internal.*ExclusiveRanges` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern found
- **Rationale**: Key Decision 3 selects `internal` visibility to avoid leaking implementation detail beyond Era.Core assembly. Only BodySettings and VisitorSettings (same assembly) need access. AC#2 verifies presence; AC#27 verifies correct visibility.

**AC#28: SyncDerivedValues still called in VisitorSettings**
- **Test**: Grep pattern=`SyncDerivedValues\(\)` path="Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern found (SyncDerivedValues call exists in VisitorSettings.Tidy)
- **Rationale**: C3 requires SyncDerivedValues to remain visitor-only. AC#10 verifies absence from shared code; AC#28 positively verifies the call is still present in VisitorSettings (defense-in-depth alongside IVisitorSettings interface enforcement via AC#20).

**AC#29: Algorithm bodies removed from consumer classes**
- **Test**: Grep pattern=`rawInput - 2` path="Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern NOT found (CalculatePenisSize body replaced with delegation)
- **Rationale**: AC#4/AC#5 verify delegation calls exist; AC#29 verifies the original algorithm body is removed. Uses `rawInput - 2` as a representative pattern — the CalculatePenisSize method body should only exist in BodyOptionValidator.cs after extraction, not in either consumer class.

**AC#30: Test case count preserved (104 cases)**
- **Test**: `dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings" --verbosity normal`
- **Expected**: Output contains '104 passed' — verified by manual inspection of test output during /run Phase 9 AC verification
- **Rationale**: AC#8 uses `succeeds` matcher which cannot detect InlineData row deletion. AC#26 counts test methods (56) but not test cases. This AC verifies the full 104 test case count using the `succeeds` matcher (test runner exit code), while the 104 count is verified by output inspection (not a matcher), which is the pragmatic approach given that count_equals is a Grep-based matcher not applicable to test runner output.

**AC#31: No Era.Core.Interfaces import in shared code**
- **Test**: Grep pattern=`using Era.Core.Interfaces` path="Era.Core/State/BodyOptionValidator.cs"
- **Expected**: Pattern NOT found
- **Rationale**: Defense-in-depth for C2 pure-function guarantee. AC#9 checks specific interface type names; AC#31 checks the namespace import itself. Together they ensure BodyOptionValidator has no path to interface dependencies.

**AC#32: ValidateBodyOption algorithm body removed from consumer classes**
- **Test**: Grep pattern=`foreach.*ExclusiveRanges` path="Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern NOT found (the ExclusiveRanges iteration loop body should only exist in BodyOptionValidator.cs after extraction)
- **Rationale**: AC#29 verifies CalculatePenisSize body removal. AC#32 extends body-removal verification to ValidateBodyOption, which contains the most complex algorithm (range group mutual exclusion loop). Together with AC#3 (ExclusiveRanges definition removed), this ensures the complete ValidateBodyOption algorithm is extracted.

**AC#33: ValidatePenisOption algorithm body removed from consumer classes**
- **Test**: Grep pattern=`candidateInRange && otherInRange` path="Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern NOT found (the dual-range check should only exist in BodyOptionValidator.cs after extraction)
- **Rationale**: AC#29 covers CalculatePenisSize, AC#32 covers ValidateBodyOption. AC#33 extends body-removal verification to ValidatePenisOption's unique dual-range mutual exclusion check.

**AC#34: BodyOptionValidator test method count**
- **Test**: Grep pattern=`\[(Fact|Theory)\]` path="Era.Core.Tests/State/BodyOptionValidatorTests.cs" | count
- **Expected**: >= 4 (at least one test method per shared method: ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize)
- **Rationale**: AC#22 uses `succeeds` matcher which cannot detect trivially empty test files. AC#26 guards existing test method counts; AC#34 applies the same protection to the new BodyOptionValidatorTests.cs. Mirrors the deletion-risk mitigation pattern.

**AC#35: ValidateSimpleDuplicate algorithm body removed from consumer classes**
- **Test**: Grep pattern=`candidateValue != otherSlotValue \? 1 : 0` path="Era.Core/State/BodySettings.cs,Era.Core/State/VisitorSettings.cs"
- **Expected**: Pattern NOT found (the ternary expression should only exist in BodyOptionValidator.cs after extraction)
- **Rationale**: AC#29 covers CalculatePenisSize, AC#32 covers ValidateBodyOption, AC#33 covers ValidatePenisOption. AC#35 completes body-removal verification for all four extracted methods. The pattern `candidateValue != otherSlotValue ? 1 : 0` is unique to ValidateSimpleDuplicate within the two consumer files.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extract four duplicated validation/calculation algorithms + ExclusiveRanges constant into shared BodyOptionValidator class | AC#1, AC#2, AC#3, AC#15, AC#16, AC#17, AC#18, AC#21, AC#22, AC#34 |
| 2 | Pure-function signatures per F779 Option C (slot values as params) | AC#9, AC#15, AC#16, AC#17, AC#18, AC#31 |
| 3 | Both BodySettings and VisitorSettings delegate to shared implementation | AC#4, AC#5, AC#29, AC#32, AC#33, AC#35 |
| 4 | Preserve IBodySettings (7 methods) and IVisitorSettings (11 methods) contracts | AC#6, AC#7, AC#23, AC#24 |
| 5 | SyncDerivedValues visitor-only (not in shared code) | AC#10, AC#28 |
| 6 | Tidy orchestration granularity preserved in respective classes (including dedup/compact) | AC#11, AC#12, AC#25 |
| 7 | ExclusiveRanges single definition | AC#2, AC#3 |
| 8 | Domain-specific methods stay in their classes | AC#13, AC#14 |
| 9 | All 104 existing tests pass without regression | AC#8, AC#26, AC#30 |
| 10 | Zero technical debt | AC#19, AC#20 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Create `Era.Core/State/BodyOptionValidator.cs` as a `public static class` containing five pure-function members: the `ExclusiveRanges` constant (shared between both classes), and four methods — `ValidateBodyOption`, `ValidatePenisOption`, `ValidateSimpleDuplicate`, and `CalculatePenisSize`. All methods accept slot values as parameters only (no `IVariableStore` or `IVisitorVariables` references), implementing F779 Key Decision Option C.

Both `BodySettings` and `VisitorSettings` are then refactored to delegate to `BodyOptionValidator` instead of their own implementations, removing their local `ExclusiveRanges` definitions and replacing the four method bodies with single-line delegation calls. The `Tidy` orchestrators remain intact in each respective class; only the leaf algorithms are extracted. `SyncDerivedValues` is not touched and stays exclusively in `VisitorSettings`.

The key design insight is that the two classes currently differ in how they read/write slot values (BodySettings reads via `GetCFlag(characterId, CharacterFlagIndex.BodyOption1)`, VisitorSettings reads via `_variables.GetBodyOption1()`), but the algorithmic logic operates only on the resolved integer values. By extracting the algorithms to accept resolved `int` values, both callers can bind their own variable access before delegating.

**Parameter signatures for BodyOptionValidator (pure functions per F779 Option C)**:

```csharp
// Era.Core/State/BodyOptionValidator.cs
namespace Era.Core.State;

public static class BodyOptionValidator
{
    // Single definition of ExclusiveRanges (satisfies C1, AC#2, AC#3)
    internal static readonly (int Min, int Max)[] ExclusiveRanges =
        [(1, 9), (10, 29), (30, 49), (50, 54), (55, 59)];

    // slot1..slot4 are the resolved int values from the caller's variable store
    public static int ValidateBodyOption(int candidateValue, int slot,
        int slot1Value, int slot2Value, int slot3Value, int slot4Value)

    // otherSlotValue is the resolved int of the other P slot
    public static int ValidatePenisOption(int candidateValue, int otherSlotValue)

    // Both inputs are resolved ints
    public static int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue)

    // Raw input only
    public static int CalculatePenisSize(int rawInput)
}
```

**ValidateBodyOption signature design note**: The BodySettings version currently takes `(int characterId, int candidateValue, int slot)` and reads the other slots via `GetCFlag`. The VisitorSettings version takes `(int candidateValue, int slot)` and reads other slots via `_variables.GetBodyOptionN()`. The pure-function version takes pre-resolved values for all four slots and uses the `slot` parameter only to exclude the candidate's own slot from the "other" list — identical logic to both current implementations but without the variable access dependency.

**ValidatePenisOption signature design note**: BodySettings takes `(int characterId, int candidateValue, int slot)` and derives `otherSlotValue` internally. VisitorSettings takes `(int candidateValue, int slot)` and reads `otherSlotValue` internally. The pure function takes `(int candidateValue, int otherSlotValue)` — the caller resolves which P slot is "other" before delegating. This eliminates the slot parameter entirely since both implementations only ever check `slot == 1` to pick the other slot value.

**Delegation pattern in BodySettings.Tidy (Phase 2 mutual exclusion excerpt)**:
```csharp
// After refactor - delegate to shared validator with resolved slot values
if (BodyOptionValidator.ValidateBodyOption(
        GetCFlag(characterId, CharacterFlagIndex.BodyOption4), 4,
        GetCFlag(characterId, CharacterFlagIndex.BodyOption1),
        GetCFlag(characterId, CharacterFlagIndex.BodyOption2),
        GetCFlag(characterId, CharacterFlagIndex.BodyOption3),
        GetCFlag(characterId, CharacterFlagIndex.BodyOption4)) == 0)
{
    SetCFlag(characterId, CharacterFlagIndex.BodyOption4, 0);
}
```

**Delegation pattern in VisitorSettings.ValidateAndClearExclusiveOptions (excerpt)**:
```csharp
if (BodyOptionValidator.ValidateBodyOption(
        _variables.GetBodyOption4(), 4,
        _variables.GetBodyOption1(),
        _variables.GetBodyOption2(),
        _variables.GetBodyOption3(),
        _variables.GetBodyOption4()) == 0)
{
    _variables.SetBodyOption4(0);
}
```

This approach satisfies all 35 ACs. See AC Coverage table below for per-AC satisfaction mapping.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/State/BodyOptionValidator.cs` as a new file |
| 2 | Place `ExclusiveRanges` constant in `BodyOptionValidator.cs`; Grep confirms presence |
| 3 | Remove `ExclusiveRanges` field from `BodySettings.cs` and `VisitorSettings.cs` |
| 4 | Replace `ValidateBodyOption`, `ValidatePenisOption`, `ValidateSimpleDuplicate`, `CalculatePenisSize` body in `BodySettings.cs` with `BodyOptionValidator.X(...)` delegation calls |
| 5 | Replace `ValidateBodyOption`, `ValidatePenisOption`, `ValidateSimpleDuplicate`, `CalculatePenisSize` body in `VisitorSettings.cs` with `BodyOptionValidator.X(...)` delegation calls |
| 6 | Touch `IBodySettings.cs` only to verify — no structural changes; 7 methods remain |
| 7 | Touch `IVisitorSettings.cs` only to verify — no structural changes; 11 methods remain |
| 8 | Run `dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings"` after refactor; all 104 tests pass because delegation produces identical results |
| 9 | `BodyOptionValidator.cs` contains no `using Era.Core.Interfaces;` and no references to `IVariableStore` or `IVisitorVariables` |
| 10 | `BodyOptionValidator.cs` contains no `SyncDerivedValues` method or call |
| 11 | `BodySettings.cs` retains `public void Tidy(int characterId)` as orchestrator — only internal delegation changes |
| 12 | `VisitorSettings.cs` retains `public void Tidy()` as orchestrator — only internal delegation changes |
| 13 | `GetTightnessBaseValue` is not extracted; remains in `BodySettings.cs` with its `switch` implementation |
| 14 | `ResetOption` and `ValidateMenuSelection` are not extracted; remain in `VisitorSettings.cs` |
| 15 | `BodyOptionValidator.ValidateBodyOption` declared as `public static int ValidateBodyOption(...)` |
| 16 | `BodyOptionValidator.ValidatePenisOption` declared as `public static int ValidatePenisOption(...)` |
| 17 | `BodyOptionValidator.ValidateSimpleDuplicate` declared as `public static int ValidateSimpleDuplicate(...)` |
| 18 | `BodyOptionValidator.CalculatePenisSize` declared as `public static int CalculatePenisSize(...)` |
| 19 | No `TODO`, `FIXME`, or `HACK` comments written in `BodyOptionValidator.cs`, `BodySettings.cs`, or `VisitorSettings.cs` |
| 20 | `dotnet build Era.Core` succeeds; no warnings because no new APIs introduced, no nullable violations, no unused variables |
| 21 | Create `Era.Core.Tests/State/BodyOptionValidatorTests.cs` with unit tests covering all four shared methods with representative inputs |
| 22 | Run `dotnet test Era.Core.Tests --filter "BodyOptionValidator"` after implementation; all new validator unit tests pass |
| 23 | Grep `IBodySettings.cs` for `int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` and confirm count = 4 |
| 24 | Grep `IVisitorSettings.cs` for `int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` and confirm count = 4 |
| 25 | Grep `VisitorSettings.cs` for `public void (Deduplicate|Compact)\(\)` and confirm count = 2 — these orchestration methods remain in VisitorSettings per C11 |
| 26 | Grep `BodySettingsBusinessLogicTests.cs` and `VisitorSettingsTests.cs` for `\[(Fact|Theory)\]` and confirm count = 56 — verifies no test deletion during refactor |
| 27 | Grep `BodyOptionValidator.cs` for `internal.*ExclusiveRanges` — verifies Key Decision 3 (internal visibility) is enforced |
| 28 | Grep `VisitorSettings.cs` for `SyncDerivedValues\(\)` — verifies SyncDerivedValues call still present after refactoring (C3 defense-in-depth) |
| 29 | Grep `BodySettings.cs` and `VisitorSettings.cs` for `rawInput - 2` and confirm NOT found — verifies algorithm bodies fully replaced with delegation calls |
| 30 | Run `dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings"` and parse output for 104 passed tests |
| 31 | Grep `BodyOptionValidator.cs` for `using Era.Core.Interfaces` and confirm NOT found — defense-in-depth for pure-function guarantee |
| 32 | Grep `BodySettings.cs` and `VisitorSettings.cs` for `foreach.*ExclusiveRanges` and confirm NOT found — verifies ValidateBodyOption algorithm body fully replaced with delegation |
| 33 | Grep `BodySettings.cs` and `VisitorSettings.cs` for `candidateInRange && otherInRange` and confirm NOT found — verifies ValidatePenisOption algorithm body fully replaced with delegation |
| 34 | Grep `BodyOptionValidatorTests.cs` for `\[(Fact|Theory)\]` and confirm count >= 4 — ensures minimum test method coverage for new shared class |
| 35 | Grep `BodySettings.cs` and `VisitorSettings.cs` for `candidateValue != otherSlotValue \? 1 : 0` and confirm NOT found — verifies ValidateSimpleDuplicate algorithm body fully replaced with delegation |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| ValidateBodyOption pure-function parameter shape | A: Pass `IVariableStore`/`IVisitorVariables`, B: Pass `int[]` array, C: Pass 4 individual slot int values | C: 4 individual slot ints | Matches F779 Key Decision Option C; no interface coupling; no heap allocation; matches existing pattern of passing individual values |
| ValidatePenisOption parameter shape | A: Keep `slot` param + pass 2 ints, B: Compute otherSlotValue at call site, pass only 2 ints | B: `(int candidateValue, int otherSlotValue)` | Callers already know which slot is "other" before calling; `slot` param is only used to pick `otherSlotValue`; removing it simplifies shared signature |
| ExclusiveRanges visibility | A: `public`, B: `internal` | B: `internal` | Only `BodySettings` and `VisitorSettings` (same assembly) need it; `internal` avoids leaking implementation detail beyond Era.Core |
| Class structure | A: Instance class with constructor, B: Static class with static methods | B: Static class | Pure functions with no state; static class prevents accidental instantiation and is the natural fit for stateless utility methods |
| Tidy leaf algorithms extraction scope | A: Extract only the 4 validation methods, B: Also extract deduplication and compaction as static helpers | A: Extract only the 4 validation/calculation methods | Deduplication and compaction in BodySettings.Tidy and VisitorSettings.Deduplicate/Compact are tightly interleaved with variable reads/writes; extracting them as pure functions would require passing large arrays of slot values with no reduction in coupling. Constraints C10 and feature-779.md:676 explicitly require Tidy orchestration to stay in respective classes |
| New tests for BodyOptionValidator | A: Add dedicated unit tests for BodyOptionValidator, B: Rely on existing 104 tests via delegation | A: Add dedicated unit tests | BodyOptionValidator is a new public API; direct unit tests provide isolated regression safety independent of the delegation wrappers; existing 104 tests serve as integration regression via delegation |

### Interfaces / Data Structures

`BodyOptionValidator` is a pure static utility class — no interface, no DI registration required.

**Full class signature**:

```csharp
// Era.Core/State/BodyOptionValidator.cs
namespace Era.Core.State;

/// <summary>
/// Shared pure-function validation algorithms for body option settings.
/// Extracted from BodySettings and VisitorSettings per F779 Option C.
/// Both BodySettings and VisitorSettings delegate to this class; callers
/// resolve slot values from their variable stores before delegation.
/// </summary>
public static class BodyOptionValidator
{
    /// <summary>
    /// Exclusive range groups for body option mutual exclusion.
    /// Single definition replacing duplicates in BodySettings.cs and VisitorSettings.cs.
    /// ERB source: 体設定.ERB lines 1900-1954 (range group definitions).
    /// </summary>
    internal static readonly (int Min, int Max)[] ExclusiveRanges =
        [(1, 9), (10, 29), (30, 49), (50, 54), (55, 59)];

    /// <summary>
    /// Validates a body option candidate value against mutual exclusion range groups.
    /// Returns 1 (accept) or 0 (reject, conflict found).
    /// ERB: @体詳細整頓 lines 772-780, @体詳細整頓訪問者 lines 1816-1824.
    /// </summary>
    /// <param name="candidateValue">The option value being validated.</param>
    /// <param name="slot">Which slot (1-4) the candidate occupies; excluded from "other" comparison.</param>
    /// <param name="slot1Value">Resolved value of body option slot 1.</param>
    /// <param name="slot2Value">Resolved value of body option slot 2.</param>
    /// <param name="slot3Value">Resolved value of body option slot 3.</param>
    /// <param name="slot4Value">Resolved value of body option slot 4.</param>
    public static int ValidateBodyOption(int candidateValue, int slot,
        int slot1Value, int slot2Value, int slot3Value, int slot4Value)

    /// <summary>
    /// Validates a penis option candidate against range 1-2 mutual exclusion and simple duplication.
    /// Returns 1 (accept) or 0 (reject).
    /// ERB: @体詳細整頓 lines 1962-1972 (P option validation).
    /// </summary>
    /// <param name="candidateValue">The P option value being validated.</param>
    /// <param name="otherSlotValue">The value of the other P slot (resolved by caller).</param>
    public static int ValidatePenisOption(int candidateValue, int otherSlotValue)

    /// <summary>
    /// Validates that candidate value does not duplicate another slot value.
    /// Returns 1 (accept) or 0 (reject, duplicate found).
    /// ERB: @体詳細整頓訪問者 lines 1887-1898, 1955-1960.
    /// </summary>
    public static int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue)

    /// <summary>
    /// Calculates penis size from raw input (subtracts 2).
    /// ERB: lines 1580, 1769.
    /// </summary>
    public static int CalculatePenisSize(int rawInput)
}
```

**BodySettings.ValidatePenisOption delegation**:
The current `BodySettings.ValidatePenisOption(int characterId, int candidateValue, int slot)` resolves the `otherSlotValue` then delegates:
```csharp
public int ValidatePenisOption(int characterId, int candidateValue, int slot)
{
    var otherSlotValue = slot == 1
        ? GetCFlag(characterId, CharacterFlagIndex.POption2)
        : GetCFlag(characterId, CharacterFlagIndex.POption1);
    return BodyOptionValidator.ValidatePenisOption(candidateValue, otherSlotValue);
}
```

**VisitorSettings.ValidatePenisOption delegation**:
```csharp
public int ValidatePenisOption(int candidateValue, int slot)
{
    var otherSlotValue = slot == 1 ? _variables.GetPenisOption2() : _variables.GetPenisOption1();
    return BodyOptionValidator.ValidatePenisOption(candidateValue, otherSlotValue);
}
```

**Delegation correctness coverage note**: The `otherSlotValue` resolution in both classes' `ValidatePenisOption` wrappers is covered by existing tests: `ValidatePenisOption_Slot1_*` and `ValidatePenisOption_Slot2_*` test both cross-slot directions (slot==1 reads POption2, slot==2 reads POption1). These end-to-end tests through the delegation wrappers verify correct `otherSlotValue` binding as part of AC#8 regression safety.

**Interface Dependency Verification** (mandatory per agent process step 8):

- `IBodySettings.cs`: All 7 methods confirmed present (BodyDetailInit, Tidy, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize, GetTightnessBaseValue). No changes needed.
- `IVisitorSettings.cs`: All 11 methods confirmed present (Tidy, Deduplicate, ValidateAndClearExclusiveOptions, Compact, SyncDerivedValues, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, ValidateMenuSelection, CalculatePenisSize, ResetOption). No changes needed.
- `BodyOptionValidator.cs` references no interfaces — pure static class. No interface dependency verification required.

### Upstream Issues

<!-- No upstream issues found during Technical Design. ACs are well-formed and constraints are complete. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,9,10,15,16,17,18,27,31 | Create `Era.Core/State/BodyOptionValidator.cs` as public static class with `ExclusiveRanges` constant (internal) and four static int methods: `ValidateBodyOption`, `ValidatePenisOption`, `ValidateSimpleDuplicate`, `CalculatePenisSize` — all accepting resolved int slot values, no interface dependencies, no SyncDerivedValues | | [x] |
| 2 | 21,22,34 | Write at least 4 separate [Fact]/[Theory] test methods for `BodyOptionValidator` in `Era.Core.Tests/` — one per shared method (ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize) with representative inputs (runs GREEN against Task 1 implementation; serves as isolated regression net independent of delegation wrappers; satisfies AC#34 gte >= 4) | | [x] |
| 3 | 3,4,6,8,11,13,19,29,32,33,35 | Refactor `BodySettings.cs`: remove local `ExclusiveRanges` field; replace `ValidateBodyOption`, `ValidatePenisOption`, `ValidateSimpleDuplicate`, `CalculatePenisSize` method bodies with single-line `BodyOptionValidator.*` delegation calls; confirm `Tidy(int characterId)` and `GetTightnessBaseValue` are untouched; ensure no TODO/FIXME/HACK introduced | | [x] |
| 4 | 3,5,7,8,12,14,19,25,28,29,32,33,35 | Refactor `VisitorSettings.cs`: remove local `ExclusiveRanges` field; replace `ValidateBodyOption`, `ValidatePenisOption`, `ValidateSimpleDuplicate`, `CalculatePenisSize` method bodies with single-line `BodyOptionValidator.*` delegation calls; confirm `Tidy()`, `Deduplicate()`, `Compact()`, `SyncDerivedValues()`, `ResetOption`, and `ValidateMenuSelection` are untouched; ensure no TODO/FIXME/HACK introduced | | [x] |
| 5 | 6,7,23,24 | Verify `IBodySettings.cs` (7 methods) and `IVisitorSettings.cs` (11 methods) are unchanged — read both interface files and confirm method counts (AC#6, AC#7) and shared method signatures (AC#23, AC#24) match expected values; no edits required | | [x] |
| 6 | 8,26,30 | Run `dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings"` via WSL and confirm all 104 existing tests pass (regression safety net); verify test method count = 56 (AC#26) and test case count = 104 (AC#30) | | [x] |
| 7 | 20 | Run `dotnet build Era.Core` via WSL and confirm build succeeds with zero warnings | | [x] |

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
| 1 | implementer | sonnet | feature-794.md Tasks 1-7, Technical Design, AC Design Constraints | BodyOptionValidator.cs (new), BodySettings.cs (refactored), VisitorSettings.cs (refactored), new unit tests |

### Pre-conditions

- F779 [DONE] and F781 [DONE] (both predecessors satisfied — confirmed in Dependencies table)
- `Era.Core/State/BodySettings.cs` and `Era.Core/State/VisitorSettings.cs` exist (confirmed by Glob)
- `Era.Core/Interfaces/IBodySettings.cs` has 7 method declarations (pre-existing; do not modify)
- `Era.Core/Interfaces/IVisitorSettings.cs` has 11 method declarations (pre-existing; do not modify)

### Execution Order

1. **Task 1 — Create BodyOptionValidator.cs** (TDD RED prerequisite)
   - File: `Era.Core/State/BodyOptionValidator.cs`
   - Namespace: `Era.Core.State;`
   - No `using` directives required (pure int arithmetic only)
   - Class: `public static class BodyOptionValidator`
   - Members (exact signatures per Technical Design):
     ```csharp
     internal static readonly (int Min, int Max)[] ExclusiveRanges =
         [(1, 9), (10, 29), (30, 49), (50, 54), (55, 59)];

     public static int ValidateBodyOption(int candidateValue, int slot,
         int slot1Value, int slot2Value, int slot3Value, int slot4Value)

     public static int ValidatePenisOption(int candidateValue, int otherSlotValue)

     public static int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue)

     public static int CalculatePenisSize(int rawInput)
     ```
   - Copy algorithm bodies verbatim from the existing `BodySettings.cs` implementations (they are identical between both classes). Adapt variable references to use the parameter names.
   - Do NOT include `SyncDerivedValues` or any reference to `IVariableStore`/`IVisitorVariables`.

2. **Task 2 — Write BodyOptionValidator unit tests** (runs GREEN against Task 1 implementation)
   - Create test file: `Era.Core.Tests/State/BodyOptionValidatorTests.cs` (or equivalent per project test naming convention)
   - Tests run GREEN immediately against Task 1's full implementation. This is an extraction/refactoring feature — the 104 existing tests (AC#8) serve as the primary TDD regression net; these new tests provide isolated coverage of the shared pure functions independent of delegation wrappers.
   - Write at least 4 separate [Fact]/[Theory] test methods (one per shared method) to satisfy AC#34 (gte >= 4).
   - Cover: `ValidateBodyOption` (accept/reject, in-range conflict, out-of-range), `ValidatePenisOption` (duplicate, non-duplicate), `ValidateSimpleDuplicate` (duplicate, non-duplicate), `CalculatePenisSize` (rawInput - 2 arithmetic).

3. **Task 3 — Refactor BodySettings.cs**
   - Remove the `ExclusiveRanges` field definition (lines ~512-513 per baseline; verify with Grep before editing).
   - For each of the four methods — `ValidateBodyOption`, `ValidatePenisOption`, `ValidateSimpleDuplicate`, `CalculatePenisSize` — replace the method body with a single-line delegation call per the Technical Design delegation patterns:
     - `ValidateBodyOption(int characterId, int candidateValue, int slot)`: resolve all 4 slot values via `GetCFlag`, then call `BodyOptionValidator.ValidateBodyOption(candidateValue, slot, slot1, slot2, slot3, slot4)`.
     - `ValidatePenisOption(int characterId, int candidateValue, int slot)`: resolve `otherSlotValue` via `GetCFlag`, then call `BodyOptionValidator.ValidatePenisOption(candidateValue, otherSlotValue)`.
     - `ValidateSimpleDuplicate`: delegate directly per identical signature pattern.
     - `CalculatePenisSize`: delegate directly.
   - Do NOT modify `Tidy(int characterId)` orchestrator body, `GetTightnessBaseValue`, or any other method.
   - Add `using Era.Core.State;` if not already present (same namespace — likely not needed).

4. **Task 4 — Refactor VisitorSettings.cs**
   - Remove the `ExclusiveRanges` field definition (lines ~15-16 per baseline; verify with Grep before editing).
   - For each of the four methods, replace the method body with delegation call per Technical Design delegation patterns:
     - `ValidateBodyOption(int candidateValue, int slot)`: resolve all 4 slot values via `_variables.GetBodyOption1()` etc., then call `BodyOptionValidator.ValidateBodyOption(candidateValue, slot, s1, s2, s3, s4)`.
     - `ValidatePenisOption(int candidateValue, int slot)`: resolve `otherSlotValue` from `_variables`, then call `BodyOptionValidator.ValidatePenisOption(candidateValue, otherSlotValue)`.
     - `ValidateSimpleDuplicate` and `CalculatePenisSize`: delegate directly.
   - Do NOT modify `Tidy()`, `Deduplicate()`, `ValidateAndClearExclusiveOptions()`, `Compact()`, `SyncDerivedValues()`, `ResetOption()`, or `ValidateMenuSelection()`.

5. **Task 5 — Verify interfaces unchanged** (read-only verification)
   - Grep `IBodySettings.cs` for `^\s+(void|int)\s+\w+\(` and confirm count = 7 (AC#6).
   - Grep `IVisitorSettings.cs` for `^\s+(void|int|bool)\s+\w+\(` and confirm count = 11 (AC#7).
   - Grep `IBodySettings.cs` for `int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` and confirm count = 4 (AC#23).
   - Grep `IVisitorSettings.cs` for `int (ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize)\(` and confirm count = 4 (AC#24).
   - No edits to either interface file.

6. **Task 6 — Run regression tests**
   - Command (via WSL): `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests --filter "BodySettings|VisitorSettings"'`
   - Expected: 104 tests pass (48 BodySettingsBusinessLogicTests + 56 VisitorSettingsTests).
   - If any test fails: STOP → report to user.

7. **Task 7 — Run build verification**
   - Command (via WSL): `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core'`
   - Expected: Build succeeds with zero warnings (TreatWarningsAsErrors enforced).
   - If build fails or warnings appear: STOP → report to user.

### Success Criteria

All 35 ACs pass:
- AC#1: `Era.Core/State/BodyOptionValidator.cs` exists (Glob)
- AC#2: `ExclusiveRanges` found in `BodyOptionValidator.cs` (Grep matches)
- AC#3: `ExclusiveRanges\s*=` not found in `BodySettings.cs` or `VisitorSettings.cs` (Grep not_matches)
- AC#4: `BodyOptionValidator\.` found in `BodySettings.cs` (Grep matches)
- AC#5: `BodyOptionValidator\.` found in `VisitorSettings.cs` (Grep matches)
- AC#6: 7 method declarations in `IBodySettings.cs` (Grep count_equals)
- AC#7: 11 method declarations in `IVisitorSettings.cs` (Grep count_equals)
- AC#8: All 104 existing tests pass (dotnet test)
- AC#9: `IVariableStore|IVisitorVariables` not found in `BodyOptionValidator.cs` (Grep not_matches)
- AC#10: `SyncDerivedValues` not found in `BodyOptionValidator.cs` (Grep not_matches)
- AC#11: `public void Tidy\(int characterId\)` found in `BodySettings.cs` (Grep matches)
- AC#12: `public void Tidy\(\)` found in `VisitorSettings.cs` (Grep matches)
- AC#13: `public int GetTightnessBaseValue\(int selectionIndex\)` found in `BodySettings.cs` (Grep matches)
- AC#14: `public (void ResetOption|bool ValidateMenuSelection)` count = 2 in `VisitorSettings.cs` (Grep count_equals)
- AC#15: `static.*int ValidateBodyOption\(` found in `BodyOptionValidator.cs` (Grep matches)
- AC#16: `static.*int ValidatePenisOption\(` found in `BodyOptionValidator.cs` (Grep matches)
- AC#17: `static.*int ValidateSimpleDuplicate\(` found in `BodyOptionValidator.cs` (Grep matches)
- AC#18: `static.*int CalculatePenisSize\(` found in `BodyOptionValidator.cs` (Grep matches)
- AC#19: `TODO|FIXME|HACK` not found in `BodyOptionValidator.cs`, `BodySettings.cs`, or `VisitorSettings.cs` (Grep not_matches)
- AC#20: `dotnet build Era.Core` succeeds (build)
- AC#21: `Era.Core.Tests/State/BodyOptionValidatorTests.cs` exists (Glob)
- AC#22: All BodyOptionValidator unit tests pass (dotnet test --filter "BodyOptionValidator")
- AC#23: 4 shared method signatures in `IBodySettings.cs` (Grep count_equals)
- AC#24: 4 shared method signatures in `IVisitorSettings.cs` (Grep count_equals)
- AC#25: 2 Deduplicate/Compact methods in `VisitorSettings.cs` (Grep count_equals)
- AC#26: 56 test methods in existing test files (Grep count_equals)
- AC#27: `internal.*ExclusiveRanges` in `BodyOptionValidator.cs` (Grep matches)
- AC#28: `SyncDerivedValues\(\)` in `VisitorSettings.cs` (Grep matches)
- AC#29: `rawInput - 2` not in `BodySettings.cs` or `VisitorSettings.cs` (Grep not_matches)
- AC#30: 104 test cases pass (dotnet test count)
- AC#31: `using Era.Core.Interfaces` not in `BodyOptionValidator.cs` (Grep not_matches)
- AC#32: `foreach.*ExclusiveRanges` not in `BodySettings.cs` or `VisitorSettings.cs` (Grep not_matches)
- AC#33: `candidateInRange && otherInRange` not in `BodySettings.cs` or `VisitorSettings.cs` (Grep not_matches)
- AC#34: >= 4 test methods in `BodyOptionValidatorTests.cs` (Grep gte)
- AC#35: `candidateValue != otherSlotValue ? 1 : 0` not in `BodySettings.cs` or `VisitorSettings.cs` (Grep not_matches)

### Error Handling

| Scenario | Action |
|----------|--------|
| Build warning appears after refactor | STOP — investigate root cause (unused variable, nullable, etc.); fix before proceeding |
| Test count drops below 104 | STOP — report which test(s) fail and error message to user |
| ExclusiveRanges still matches in consumer files after refactor | Verify edit was applied; check for additional definitions not captured by baseline |
| BodyOptionValidator.cs contains interface reference | Revert and re-extract algorithm body using only parameter values |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

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

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| ac-static-verifier regex anchor and gte parser failures | PRE-EXISTING tool limitation causing false FAIL on AC#6,#7,#34 | New Feature | F798 | Phase 9 DEVIATION handoff |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-21 07:29 | START | implementer | Task 2 (Phase 3 TDD RED) | - |
| 2026-02-21 07:30 | END | implementer | Task 2 (Phase 3 TDD RED) | RED confirmed: 9 CS0103 errors, BodyOptionValidator not found |
| 2026-02-21 07:35 | START | implementer | Tasks 1, 3, 4 | - |
| 2026-02-21 07:36 | END | implementer | Tasks 1, 3, 4 | SUCCESS: build GREEN, 104 regression tests pass, 33 BodyOptionValidator tests pass |
| 2026-02-21 08:00 | START | ac-tester | Phase 7 AC Verification | - |
| 2026-02-21 08:03 | END | ac-tester | Phase 7 AC Verification | 35/35 ACs PASS |
| 2026-02-21 08:10 | DEVIATION | Bash | ac-static-verifier --ac-type code | exit code 1: 26/29 pass; AC#6,#7 regex anchor mismatch, AC#34 gte format parse error — all verified PASS by ac-tester |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: Goal | Goal overclaimed "six algorithms" but only 4 methods + ExclusiveRanges constant are extracted; updated Goal and Goal Coverage table
- [fix] Phase2-Review iter1: Tasks table Task 2 | Removed AC#8 from Task 2 AC# mapping; AC#8 is regression test (Task 6), not new test creation (Task 2)
- [fix] Phase2-Review iter1: Philosophy Derivation | Updated derivation table to include DRY principle and SyncDerivedValues asymmetry as actual design drivers for structural ACs
- [fix] Phase2-Review iter2: Task 2 description + Implementation Contract Step 2 | Removed false TDD RED claim; extraction feature's tests run GREEN immediately against Task 1 implementation
- [fix] Phase2-Review iter2: AC Definition Table + AC Details | Added AC#23 and AC#24 for signature-level verification of shared methods on IBodySettings/IVisitorSettings; updated Task 5, Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase2-Review iter2: AC Design Constraints + AC#25 | Added C11 (dedup/compact exemption) and AC#25 verifying Deduplicate/Compact remain in VisitorSettings; updated Philosophy Derivation, Goal Coverage, Task 4, AC Coverage, Success Criteria
- [fix] Phase3-Maintainability iter3: Risks table + Key Decision 4 + Key Decision 5 | Removed YAGNI-forbidden language from 3 locations; replaced with Zero Debt Upfront-compliant rationales
- [fix] Phase2-Review iter4: AC Definition Table + AC Details | Added AC#26 for test method count preservation (56 test methods); updated Task 6, Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase2-Review iter5: AC#8 Details | Clarified test cases (104) vs test methods (56) distinction; added delegation correctness coverage note for ValidatePenisOption
- [fix] Phase2-Review iter6: Review Context | Updated to distinguish extracted (4 methods + ExclusiveRanges) vs remaining (dedup/compact/sync) algorithms
- [fix] Phase2-Review iter6: AC#27 | Added ExclusiveRanges internal visibility verification; updated Task 1, AC Coverage, Success Criteria
- [fix] Phase2-Review iter7: C6 | Corrected BodySettingsBusinessLogicTests count from (25) to (27 methods / 48 cases)
- [fix] Phase2-Uncertain iter7: AC#28 | Added SyncDerivedValues call-site verification in VisitorSettings; updated Goal Coverage, Task 4, AC Coverage, Success Criteria
- [fix] Phase2-Review iter8: Philosophy Derivation | Separated DRY from Pipeline Continuity; sourced from F779 Key Decision Option C
- [fix] Phase2-Review iter8: AC#29 | Added algorithm body removal verification (rawInput - 2 not_matches); updated Tasks 3/4, Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase3-Maintainability iter9: Philosophy Derivation | Added AC#23,24,26,28,29 to coverage columns in Philosophy Derivation table
- [fix] Phase3-Maintainability iter9: Problem section | Corrected "six" to "four methods + ExclusiveRanges + structurally similar Tidy"
- [fix] Phase2-Review iter1: Review Context | Restructured from freeform prose to template-required sub-sections (Origin, Identified Gap, Review Evidence, Files Involved, Parent Review Observations)
- [fix] Phase2-Uncertain iter1: AC Details | Reordered AC#23-29 entries to sequential numeric order after AC#22
- [fix] Phase2-Review iter1: AC Definition Table + AC Details | Added AC#30 for 104 test case count verification; updated Task 6, Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase2-Uncertain iter1: AC Definition Table + AC Details | Added AC#31 for using Era.Core.Interfaces import check; updated Task 1, Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase2-Review iter1: AC Definition Table + AC Details | Added AC#32 (ValidateBodyOption body removal) and AC#33 (ValidatePenisOption body removal); updated Tasks 3/4, Goal Coverage, AC Coverage, Success Criteria, Philosophy Derivation
- [fix] Phase2-Review iter2: AC#30 | Changed matcher from count_equals to succeeds; count_equals is Grep-based, not applicable to test runner output
- [fix] Phase2-Review iter2: AC Definition Table + AC Details | Added AC#34 for BodyOptionValidator test method count (>= 4); updated Task 2, Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase2-Review iter3: Philosophy Derivation | Added AC#21, AC#22, AC#30, AC#34 to "documented transition points" row; closes traceability gap for test-related ACs
- [fix] Phase2-Review iter4: AC Definition Table + AC Details | Added AC#35 for ValidateSimpleDuplicate body removal; updated Tasks 3/4, Goal Coverage, AC Coverage, Success Criteria, Philosophy Derivation
- [fix] Phase2-Review iter5: Goal Coverage | Removed AC#12 from Goal item 5 (SyncDerivedValues); AC#12 verifies Tidy() preservation which belongs in Goal item 6
- [resolved-applied] Phase2-Uncertain iter6: Task-to-AC alignment | Tasks 3/4 modify consumer classes but don't reference AC#8 (regression tests) or AC#6/AC#7 (interface preservation). Validator uncertain: template does not clearly define whether Task AC# column means "ACs this Task satisfies" vs "all ACs related to this Task". Fix: Add AC#8 to Tasks 3/4, AC#6 to Task 3, AC#7 to Task 4.
- [fix] Phase3-Maintainability iter6: Philosophy Derivation | Added AC#27 to Option C+DRY row; added Zero Debt Upfront row for AC#19, AC#20; closes orphan AC traceability gap
- [fix] Phase2-Review iter7: Technical Design | Updated stale "22 ACs" count to "35 ACs"; removed incomplete inline enumeration, referencing AC Coverage table instead
- [fix] Phase4-ACValidation iter8: AC#34 | Changed matcher from count_equals to gte; count_equals requires exact number, gte is correct for >= threshold
- [fix] Phase2-Review iter9: Task 2 + Implementation Contract Step 2 | Added explicit "at least 4 separate test methods" requirement to align with AC#34 gte >= 4
- [fix] PostLoop-UserFix post-loop: Tasks 3/4 AC# | Added AC#6,AC#8 to Task 3; AC#7,AC#8 to Task 4 per user decision

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F781](feature-781.md) - Visitor Settings (source of shared algorithms)
- [Predecessor: F779](feature-779.md) - Body Settings UI (parallel implementation)
- [Successor: F782](feature-782.md) - Post-Phase Review (depends on F794)
- [Related: F780](feature-780.md) - Genetics & Growth (ERB-only dependency)
- [Related: F789](feature-789.md) - IStringVariables interface pattern (precedent)
- [Related: F790](feature-790.md) - IEngineVariables interface pattern (precedent)
- [Related: F796](feature-796.md) - BodyDetailInit migration (shared file modification)
