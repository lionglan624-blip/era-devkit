# Feature 778: Body Initialization Verification (体設定.ERB lines 6-348)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-17T00:00:00Z -->

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

## Created: 2026-02-11

---

## Summary

Verify existing F370/F377 body initialization migration (BodySettings.cs, IBodySettings, DI registration), fix F370 CFLAG index bug (body options 513-516 → 515-518), and close per-character equivalence test gaps for all 14 characters (chars 2-13 untested, char 0/1 shallow). Phase 20 debt-zero verification for BodySettings.cs scope.

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
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
| Game/ERB/体設定.ERB | 6-348 | 15 | @体詳細初期設定 dispatcher + @体詳細初期設定0 through @体詳細初期設定13 |

### C# Implementation (Pre-existing from F370/F377)

| File | Lines | Role | Note |
|------|------:|------|------|
| Era.Core/State/BodySettings.cs | 491 | Implementation | All 14 chars + dispatcher, created F370 |
| Era.Core/Interfaces/IBodySettings.cs | 17 | Interface | BodyDetailInit signature, created F377 |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | - | DI | IBodySettings registered, F377 |
| Era.Core/Common/GameInitialization.cs | 313-321 | Consumer | Wrapper with dummy lambdas (Phase 22 scope) |
| engine.Tests/Tests/StateSettingsTests.cs | 26-155 | Tests | 5 tests: char 0 F/M, char 1 (shallow), OOR, null |

---

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity with Equivalence Assurance - Each phase completion triggers next phase planning, and each sub-feature must include its own equivalence tests to verify migration correctness at the point of implementation (not deferred to a later batch test feature). SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; F647 decomposed it into actionable sub-features. Phase 20 Sub-Feature Requirements (architecture.md:4629-4637) mandate debt cleanup and equivalence testing per sub-feature.

### Problem (Current Issue)

F647 (Phase 20 Planning) created F778 as a "migrate body initialization to C#" task, but the core C# migration was already completed by F370 (Phase 3, BodySettings.cs with all 14 character initializations) and F377 (Phase 4, IBodySettings interface + DI registration). This happened because F647 decomposed 体設定.ERB by line ranges without cross-referencing Phase 3/4 deliverables. The existing implementation has two concrete gaps: (1) only 5 of 14+ needed per-character equivalence tests exist (chars 2-13 are completely untested, char 1 test is shallow -- checks key existence but not values), and (2) Phase 20 requires debt-zero verification which has not been performed for this scope.

### Goal (What to Achieve)

1. Fix F370 CFLAG index bug: body option indices in BodySettings.cs (513-516 → 515-518) to match CFLAG.CSV mapping
2. Verify the existing BodySettings.cs implementation covers all 14 characters with correct CFLAG 500-512, 515-518 assignments matching 体設定.ERB source — via value-level tests for all characters (char 0 female/male strengthened + chars 1-13 new/strengthened)
3. Expand per-character equivalence tests to cover all 14 characters with value-level assertions: strengthen char 0 female/male and char 1 tests, add new tests for chars 2-13
4. Verify zero technical debt (TODO/FIXME/HACK) in BodySettings.cs scope

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does F778 propose migrating already-migrated code? | F647 created F778 without checking if migration was already done | feature-647.md:31 lists F778-F781 as body settings decomposition |
| 2 | Why didn't F647 check Phase 3/4 deliverables? | F647 decomposed 体設定.ERB by line ranges, focusing on ERB structure not C# codebase | feature-647.md:54 decomposes by functional areas within the ERB file |
| 3 | Why was line-range decomposition used without cross-referencing? | Phase 20 planning (research type) analyzed ERB files but not existing C# implementations | F647 Type: research, focused on ERB source analysis |
| 4 | Why were existing C# implementations not discovered? | No systematic check for pre-existing migrations exists in the /fc planning workflow | F370 [DONE] created BodySettings.cs:1-491; F377 [DONE] added DI/interface |
| 5 | Why (Root)? | Phase planning workflow lacks a "prior migration check" step that cross-references new sub-features against completed Phase 3/4 deliverables | archive/feature-370.md:17 shows F370 scope = 体設定.ERB @体詳細初期設定 lines 6-347 (identical to F778) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F778 proposes migrating body initialization that already exists in C# | Phase 20 planning decomposed ERB files without checking whether earlier phases already migrated the same functions |
| Where | feature-778.md Summary/Goal (says "migrate") | F647 planning process (no cross-reference step against F370/F377) |
| Fix | Change F778 Summary from "migrate" to "verify" | Reframe F778 to verification + test gap closure; add cross-reference lesson for future planning features |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F370 | [DONE] | Created BodySettings.cs implementation (Phase 3 TRYCALL migration) |
| F377 | [DONE] | Added IBodySettings interface + DI registration (Phase 4 architecture refactoring) |
| F647 | [DONE] | Parent: Phase 20 Planning that created this feature |
| F779 | [PROPOSED] | Sibling: Body Settings UI (体設定.ERB lines 350-943), no cross-calls |
| F780 | [DRAFT] | Sibling: Genetics & Growth (体設定.ERB lines 944-1426), no cross-calls |
| F781 | [DONE] | Sibling: Visitor Settings (体設定.ERB lines 1431-1976), no cross-calls |
| F782 | [DRAFT] | Successor: Post-Phase Review Phase 20 |
| F791 | [DONE] | IEntryPointRegistry exists but not wired for body init (informational) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Implementation exists | FEASIBLE | BodySettings.cs:1-491 fully implements all 14 chars + dispatcher |
| Interface exists | FEASIBLE | IBodySettings.cs:9-16 defines BodyDetailInit signature |
| DI registration exists | FEASIBLE | ServiceCollectionExtensions.cs:148 registers IBodySettings |
| Test infrastructure exists | FEASIBLE | StateSettingsTests.cs has 5 tests with established pattern |
| Test gap closure | FEASIBLE | Requires adding ~12 test methods following existing char 0/1 pattern |
| Runtime integration | NOT_FEASIBLE | Dummy lambdas in GameInitialization.cs:315-320 require Phase 22 (out of scope) |
| Debt verification | FEASIBLE | Grep for TODO/FIXME/HACK in BodySettings.cs shows 0 matches |

**Verdict**: NEEDS_REVISION

Feature must be reframed from "migrate" to "verify + test gap closure". Core migration is complete. Runtime integration (replacing dummy lambdas) is blocked by Phase 22 and out of scope. Reframed scope is fully FEASIBLE.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| BodySettings.cs | LOW | No code changes needed; implementation is complete |
| StateSettingsTests.cs | HIGH | ~12 new test methods for chars 2-13, strengthen char 1 test |
| feature-778.md | HIGH | Scope reframing from migration to verification |
| ERB files | NONE | ERB stays active until Phase 30 |
| GameInitialization.cs | NONE | Dummy lambdas are Phase 22 scope, not modified |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| TreatWarningsAsErrors | Directory.Build.props | All new test code must compile warning-free |
| ERB deletion blocked | Phase 30 strategy | Cannot remove 体設定.ERB; ERB and C# coexist |
| Runtime integration blocked | Phase 22 (State Systems) | GameInitialization.cs dummy lambdas cannot be replaced yet |
| Char 0 gender branch | 体設定.ERB:36-72, BodySettings.cs | Character 0 uses HAS_VAGINA (TALENT:0) to branch female/male defaults |
| HairColor == HairBaseColor | 体設定.ERB chars 1-13 | Pattern: CFLAG:505 (髪色) = CFLAG:504 (髪原色) for all non-char-0 characters |
| CHARA_SET range | DIM.ERH:56-68 | CHARA_SET iterates chars 1-10 and 1-12 (excludes char 0 and char 13 in some paths) |
| Test project location | engine.Tests/ not Era.Core.Tests/ | BodySettings tests are in engine.Tests/Tests/StateSettingsTests.cs |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Test values don't match ERB source | LOW | HIGH | Each test method derives Expected values directly from 体設定.ERB source lines |
| New tests cause build warnings | LOW | MEDIUM | Follow existing StateSettingsTests.cs patterns exactly |
| Scope creep into runtime integration | MEDIUM | HIGH | Explicitly exclude GameInitialization.cs dummy lambda replacement from scope |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| BodySettings test count | Grep "\\[Fact\\]" engine.Tests/Tests/StateSettingsTests.cs (BodySettings region) | 5 | Chars 0 F/M, 1, OOR, null |
| BodySettings.cs debt | Grep "TODO\|FIXME\|HACK" Era.Core/State/BodySettings.cs | 0 | Zero debt confirmed |
| IBodySettings methods | Grep "void\|int\|string" Era.Core/Interfaces/IBodySettings.cs | 1 | Single method: BodyDetailInit |

**Baseline File**: `.tmp/baseline-778.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | All 14 characters (0-13) must have per-character equivalence tests verifying CFLAG 500-512, 515-518 values | Phase 20 equivalence requirement (architecture.md:4633) | Need 14 character test methods (2 for char 0 due to gender branch) |
| C2 | Character 0 has gender-dependent branching via HAS_VAGINA (TALENT:0) | 体設定.ERB:36-72, BodySettings.cs | Char 0 needs both female and male test cases (already exist) |
| C3 | HairColor (505) always equals HairBaseColor (504) for chars 1-13 | 体設定.ERB pattern across all non-char-0 functions | Tests must verify CFLAG:505 == CFLAG:504 for each character |
| C4 | Zero technical debt required in BodySettings.cs scope | Phase 20 Sub-Feature Requirements (architecture.md:4635) | Debt-zero AC with TODO/FIXME/HACK pattern |
| C5 | Tests are in engine.Tests/ not Era.Core.Tests/ | StateSettingsTests.cs location | AC test commands must target engine.Tests/ project |
| C6 | Runtime integration (dummy lambda replacement) is Phase 22 scope | GameInitialization.cs:315 TODO comment | Must NOT create ACs that require replacing dummy lambdas |
| C7 | Out-of-range character handling is silent no-op | BodySettings.cs dispatcher, 体設定.ERB:6-34 | Existing OOR test already covers this (verify preservation) |
| C8 | DI registration and interface already exist | ServiceCollectionExtensions.cs:148, IBodySettings.cs | Verification ACs only (not creation ACs) |

### Constraint Details

**C1: Per-Character Equivalence Testing**
- **Source**: Phase 20 Sub-Feature Requirements mandate equivalence testing per sub-feature; investigation found chars 2-13 completely untested, char 1 shallow
- **Verification**: Grep StateSettingsTests.cs for test methods matching each character ID
- **AC Impact**: Need individual test ACs for each character (or parameterized tests covering all), verifying CFLAG 500-512, 515-518 values match 体設定.ERB source

**C2: Character 0 Gender Branch**
- **Source**: 体設定.ERB:36-72 has IF HAS_VAGINA branch; BodySettings.cs implements via getTalent(charId, 0)
- **Verification**: Existing tests BodyDetailInit_Character0_Female and _Male already cover this
- **AC Impact**: Existing tests sufficient; AC should verify they still pass (not create new)

**C3: HairColor == HairBaseColor Pattern**
- **Source**: In 体設定.ERB, every character 1-13 sets CFLAG:505 = CFLAG:504 (hairColor = hairBaseColor)
- **Verification**: For each character test, assert cflag[(charId, 505)] == cflag[(charId, 504)]
- **AC Impact**: Each per-character test should include this assertion

**C4: Debt-Zero Verification**
- **Source**: Phase 20 requirement; currently 0 matches in BodySettings.cs
- **Verification**: Grep "TODO|FIXME|HACK" Era.Core/State/BodySettings.cs
- **AC Impact**: Single code-type AC with not_matches matcher

**C5: Test Project Location**
- **Source**: Investigation found tests in engine.Tests/Tests/StateSettingsTests.cs, not Era.Core.Tests/
- **Verification**: Glob engine.Tests/Tests/StateSettingsTests.cs
- **AC Impact**: All test execution ACs must use `dotnet test engine.Tests/` not `dotnet test Era.Core.Tests/`

**C6: Phase 22 Scope Exclusion**
- **Source**: GameInitialization.cs:315 has explicit TODO comment "Replace with GlobalStatic accessors -> Phase 22"
- **Verification**: GameInitialization.cs:315-320 still has dummy lambdas
- **AC Impact**: Must NOT include ACs that require modifying GameInitialization.cs

**C7: Out-of-Range Handling**
- **Source**: BodySettings.cs dispatcher returns silently for unknown character IDs
- **Verification**: Existing test BodyDetailInit_OutOfRange_NoOp covers this
- **AC Impact**: Verify existing test passes (not create new)

**C8: Existing Infrastructure Verification**
- **Source**: F370 created BodySettings.cs, F377 added IBodySettings + DI registration
- **Verification**: Grep for class/interface/DI patterns in respective files
- **AC Impact**: Use code-type ACs to verify existing infrastructure, not create new files

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F370 | [DONE] | Created BodySettings.cs implementation (Phase 3 body & state migration) |
| Predecessor | F377 | [DONE] | Added IBodySettings interface + DI registration (Phase 4 architecture) |
| Predecessor | F647 | [DONE] | Phase 20 Planning (parent, decomposed this feature) |
| Successor | F782 | [DRAFT] | Post-Phase Review Phase 20 |
| Related | F779 | [DONE] | Sibling: Body Settings UI (体設定.ERB lines 350-943) |
| Related | F780 | [PROPOSED] | Sibling: Genetics & Growth (体設定.ERB lines 944-1426) |
| Related | F781 | [DONE] | Sibling: Visitor Settings (体設定.ERB lines 1431-1976) |
| Related | F791 | [DONE] | IEntryPointRegistry (not wired for body init, informational) |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
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
| "each sub-feature must include its own equivalence tests" | All 14 characters must have per-character equivalence tests with value-level assertions | AC#1-AC#13, AC#20, AC#21 |
| "verify migration correctness at the point of implementation" | Test CFLAG 500-512, 515-518 values against 体設定.ERB source for every character | AC#1-AC#13, AC#19, AC#20, AC#21 |
| "debt cleanup...per sub-feature" | Zero TODO/FIXME/HACK in BodySettings.cs scope | AC#14 |
| "Pipeline Continuity" | Existing infrastructure (interface, DI, implementation) verified intact | AC#15, AC#16, AC#17 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Char 1 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character1 | succeeds | - | [x] |
| 2 | Char 2 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character2 | succeeds | - | [x] |
| 3 | Char 3 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character3 | succeeds | - | [x] |
| 4 | Char 4 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character4 | succeeds | - | [x] |
| 5 | Char 5 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character5 | succeeds | - | [x] |
| 6 | Char 6 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character6 | succeeds | - | [x] |
| 7 | Char 7 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character7 | succeeds | - | [x] |
| 8 | Char 8 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character8 | succeeds | - | [x] |
| 9 | Char 9 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character9 | succeeds | - | [x] |
| 10 | Char 10 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character10 | succeeds | - | [x] |
| 11 | Char 11 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character11 | succeeds | - | [x] |
| 12 | Char 12 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character12 | succeeds | - | [x] |
| 13 | Char 13 equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character13 | succeeds | - | [x] |
| 14 | Zero technical debt in BodySettings.cs (Pos) | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 15 | IBodySettings interface exists with BodyDetailInit signature | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | matches | void BodyDetailInit | [x] |
| 16 | DI registration for IBodySettings exists | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | contains | AddSingleton<IBodySettings, BodySettings> | [x] |
| 17 | BodyDetailInit character test method count is 15 (Pos) | code | Grep(engine.Tests/Tests/StateSettingsTests.cs) | count_equals | `public void BodyDetailInit_Character\d+` = 15 | [x] |
| 18 | HairColor == HairBaseColor verified for all characters (Pos) | code | Grep(engine.Tests/Tests/StateSettingsTests.cs) | count_equals | `cflag\[\(charId, 504\)\].*cflag\[\(charId, 505\)\]` = 15 | [x] |
| 19 | SkinColor == SkinBaseColor verified for all characters (Pos) | code | Grep(engine.Tests/Tests/StateSettingsTests.cs) | count_equals | `cflag\[\(charId, 511\)\].*cflag\[\(charId, 512\)\]` = 15 | [x] |
| 20 | Char 0 female equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character0_Female | succeeds | - | [x] |
| 21 | Char 0 male equivalence test verifies all 17 CFLAG values (Pos) | test | dotnet test engine.Tests/ --filter BodyDetailInit_Character0_Male | succeeds | - | [x] |
| 22 | Existing out-of-range test still passes (Neg) | test | dotnet test engine.Tests/ --filter BodyDetailInit_OutOfRange | succeeds | - | [x] |
| 23 | Existing null accessor test still passes (Neg) | test | dotnet test engine.Tests/ --filter BodyDetailInit_NullAccessor | succeeds | - | [x] |
| 24 | All BodySettings tests pass together | test | dotnet test engine.Tests/ --filter FullyQualifiedName~StateSettingsTests.BodyDetailInit | succeeds | - | [x] |
| 25 | BodySettings.cs body option indices use 515-518 (not 513-516) (Pos) | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | setCflag\(characterId, 51[3-4], | [x] |
| 26 | BodySettings.cs doc comments reference corrected indices 515-518 (Pos) | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `51[34].*体オプション` | [x] |
| 27 | BodySettings.cs range comments updated from 500-516 to 500-512, 515-518 (Pos) | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `500-516` | [x] |
| 28 | BodySettings.cs doc comments for BodyOption3/4 use corrected indices 517-518 (Pos) | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `515.*体オプション３\|516.*体オプション４` | [x] |

### AC Details

**AC#1: Char 1 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character1`
- **Expected**: Test passes. Replaces existing shallow char 1 test (ContainsKey only) with value-level assertions for all CFLAG 500-512, 515-518 matching 体設定.ERB lines 76-95: HairLength=500, HairLengthCategory=5, HairOption1=4, HairBaseColor=0, HairColor=0, EyeColorRight=16, EyeColorLeft=16, SkinBaseColor=2, plus HairColor==HairBaseColor check.
- **Rationale**: Char 1 existing test only checks key existence, not values. Phase 20 equivalence requirement demands value-level verification. (C1, C3)

**AC#2: Char 2 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character2`
- **Expected**: Test passes. Verifies 体設定.ERB lines 97-116: HairLength=600, HairLengthCategory=6, HairBaseColor=2, EyeColorRight=61, SkinBaseColor=1, BodyOption1=14, BodyOption2=50, BodyOption3=58, plus HairColor==HairBaseColor.
- **Rationale**: Char 2 (小悪魔) completely untested. Has non-zero BodyOption1/2/3 values making it important for equivalence. (C1, C3)

**AC#3: Char 3 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character3`
- **Expected**: Test passes. Verifies 体設定.ERB lines 119-137: HairLength=400, HairBaseColor=10, EyeColorRight=10, EyeExpression=4, SkinBaseColor=0. (C1, C3)
- **Rationale**: Char 3 (パチュリー) has unique EyeExpression=4 and SkinBaseColor=0 values.

**AC#4: Char 4 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character4`
- **Expected**: Test passes. Verifies 体設定.ERB lines 140-158: HairLength=300, HairOption1=4, HairBaseColor=90, EyeColorRight=12, SkinBaseColor=1. (C1, C3)
- **Rationale**: Char 4 (咲夜) completely untested.

**AC#5: Char 5 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character5`
- **Expected**: Test passes. Verifies 体設定.ERB lines 161-179: HairLength=300, HairBaseColor=11, EyeColorRight=1, EyeExpression=2, BodyOption1=2, BodyOption2=12. (C1, C3)
- **Rationale**: Char 5 (レミリア) has vampire-specific BodyOption values.

**AC#6: Char 6 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character6`
- **Expected**: Test passes. Verifies 体設定.ERB lines 182-200: HairLength=300, HairOption1=2, HairBaseColor=89, EyeColorRight=1, BodyOption1=2, BodyOption2=13. (C1, C3)
- **Rationale**: Char 6 (フラン) has vampire-specific BodyOption values distinct from char 5.

**AC#7: Char 7 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character7`
- **Expected**: Test passes. Verifies 体設定.ERB lines 203-221: HairLength=300, HairBaseColor=2, EyeColorRight=61, SkinBaseColor=1, BodyOption1=14, BodyOption2=50, BodyOption3=58. (C1, C3)
- **Rationale**: Char 7 (子悪魔) shares body options with char 2, testing pattern consistency.

**AC#8: Char 8 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character8`
- **Expected**: Test passes. Verifies 体設定.ERB lines 224-242: HairLength=300, HairBaseColor=20, EyeColorRight=18, SkinBaseColor=1, BodyOption1=11. (C1, C3)
- **Rationale**: Char 8 (チルノ) has unique HairBaseColor=20 and BodyOption1=11.

**AC#9: Char 9 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character9`
- **Expected**: Test passes. Verifies 体設定.ERB lines 245-263: HairLength=700, HairLengthCategory=7, HairOption1=1, HairBaseColor=28, EyeColorRight=37, BodyOption1=10. (C1, C3)
- **Rationale**: Char 9 (大妖精) has the longest hair (700) and unique color values.

**AC#10: Char 10 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character10`
- **Expected**: Test passes. Verifies 体設定.ERB lines 266-284: HairLength=500, HairLengthCategory=6, HairBaseColor=89, EyeColorRight=89, SkinBaseColor=1. (C1, C3)
- **Rationale**: Char 10 (魔理沙) has unique EyeColor matching HairColor (both 89).

**AC#11: Char 11 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character11`
- **Expected**: Test passes. Verifies 体設定.ERB lines 287-305: HairLength=500, HairLengthCategory=6, HairBaseColor=69, EyeColorRight=65, SkinBaseColor=1. (C1, C3)
- **Rationale**: Char 11 (霊夢) completely untested.

**AC#12: Char 12 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character12`
- **Expected**: Test passes. Verifies 体設定.ERB lines 308-326: HairLength=300, HairBaseColor=89, EyeColorRight=57, SkinBaseColor=1, BodyOption1=1. (C1, C3)
- **Rationale**: Char 12 (ルーミア) has unique BodyOption1=1.

**AC#13: Char 13 equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character13`
- **Expected**: Test passes. Verifies 体設定.ERB lines 329-347: HairLength=300, HairBaseColor=89, EyeColorRight=17, SkinBaseColor=1. (C1, C3)
- **Rationale**: Char 13 (アリス) is the last character, completing full coverage.

**AC#14: Zero technical debt in BodySettings.cs (Pos)**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches
- **Rationale**: Phase 20 Sub-Feature Requirements mandate debt-zero verification. Baseline confirms 0 matches; AC verifies no regression. (C4)

**AC#15: IBodySettings interface exists with BodyDetailInit signature**
- **Test**: Grep pattern=`void BodyDetailInit` path=`Era.Core/Interfaces/IBodySettings.cs`
- **Expected**: Pattern found
- **Rationale**: Verifies pre-existing interface (F377) is intact. This is a verification AC, not creation AC. (C8)

**AC#16: DI registration for IBodySettings exists**
- **Test**: Grep pattern=`AddSingleton<IBodySettings, BodySettings>` path=`Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- **Expected**: Pattern found
- **Rationale**: Verifies pre-existing DI registration (F377) is intact. This is a verification AC, not creation AC. (C8)

**AC#17: BodyDetailInit character test method count is 15**
- **Test**: Grep pattern=`public void BodyDetailInit_Character\d+` path=`engine.Tests/Tests/StateSettingsTests.cs` | count
- **Expected**: 15 matches (char 0 F/M methods = 2, chars 1-13 methods = 13 = 15 total character test methods)
- **Rationale**: Ensures all 14 characters have dedicated test methods by matching test method declarations precisely. Char 0 has 2 (female/male). Chars 1-13 each have 1. (C1, C5)

**AC#18: HairColor == HairBaseColor verified for all characters**
- **Test**: Grep pattern=`cflag\[\(charId, 504\)\].*cflag\[\(charId, 505\)\]` path=`engine.Tests/Tests/StateSettingsTests.cs` | count
- **Expected**: 15 matches (one `Assert.Equal(cflag[(charId, 504)], cflag[(charId, 505)])` invariant assertion per character: 2 for char 0 F/M + 13 for chars 1-13 = 15 total)
- **Rationale**: Validates that the HairColor==HairBaseColor invariant from 体設定.ERB is explicitly tested for every character. Pattern matches only the invariant assertion (both 504 and 505 on same line), not individual value assertions.
- **Assertion Order Constraint**: Implementer MUST write `Assert.Equal(cflag[(charId, 504)], cflag[(charId, 505)])` (504 as first arg, 505 as second) to match the grep pattern. See Implementation Contract test pattern for normative assertion order.

**AC#19: SkinColor == SkinBaseColor verified for all characters (Pos)**
- **Test**: Grep pattern=`cflag\[\(charId, 511\)\].*cflag\[\(charId, 512\)\]` path=`engine.Tests/Tests/StateSettingsTests.cs` | count
- **Expected**: 15 matches (one `Assert.Equal(cflag[(charId, 511)], cflag[(charId, 512)])` invariant assertion per character: 2 for char 0 F/M + 13 for chars 1-13 = 15 total)
- **Rationale**: Validates that the SkinColor==SkinBaseColor invariant from 体設定.ERB is explicitly tested for every character, symmetric with AC#18's HairColor check. Pattern matches only the invariant assertion.
- **Assertion Order Constraint**: Implementer MUST write `Assert.Equal(cflag[(charId, 511)], cflag[(charId, 512)])` (511 as first arg, 512 as second) to match the grep pattern. See Implementation Contract test pattern for normative assertion order.

**AC#20: Char 0 female equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character0_Female`
- **Expected**: Test passes. Strengthens existing shallow char 0 female test with full value-level assertions for all CFLAG 500-512, 515-518 matching 体設定.ERB lines 38-55: HairLength=400, HairLengthCategory=4, HairOption1=0, HairOption2=0, HairBaseColor=88, HairColor=88, EyeColorRight=88, EyeColorLeft=88, EyeExpression=0, EyeOption1=0, EyeOption2=0, SkinBaseColor=2, SkinColor=2, BodyOption1=0, BodyOption2=0, BodyOption3=0, BodyOption4=0. Plus HairColor==HairBaseColor and SkinColor==SkinBaseColor invariant checks.
- **Rationale**: Philosophy demands full equivalence for all 14 characters. Existing test only verified 7/17 values. (C1, C2)

**AC#21: Char 0 male equivalence test verifies all 17 CFLAG values (Pos)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_Character0_Male`
- **Expected**: Test passes. Strengthens existing shallow char 0 male test with full value-level assertions for all CFLAG 500-512, 515-518 matching 体設定.ERB lines 57-73: HairLength=100, HairLengthCategory=2, HairOption1=0, HairOption2=0, HairBaseColor=88, HairColor=88, EyeColorRight=88, EyeColorLeft=88, EyeExpression=0, EyeOption1=0, EyeOption2=0, SkinBaseColor=2, SkinColor=2, BodyOption1=0, BodyOption2=0, BodyOption3=0, BodyOption4=0. Plus HairColor==HairBaseColor and SkinColor==SkinBaseColor invariant checks.
- **Rationale**: Philosophy demands full equivalence for all 14 characters. Existing test only verified 2/17 values. (C1, C2)

**AC#22: Existing out-of-range test still passes (Neg)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_OutOfRange`
- **Expected**: Test passes (pre-existing F370 test preserved)
- **Rationale**: Verifies out-of-range character handling (silent no-op) is preserved. (C7)

**AC#23: Existing null accessor test still passes (Neg)**
- **Test**: `dotnet test engine.Tests/ --filter BodyDetailInit_NullAccessor`
- **Expected**: Test passes (pre-existing F370 test preserved)
- **Rationale**: Verifies null parameter validation is preserved.

**AC#24: All BodySettings tests pass together**
- **Test**: `dotnet test engine.Tests/ --filter FullyQualifiedName~StateSettingsTests.BodyDetailInit`
- **Expected**: All BodySettings tests (existing + new) pass in a single run
- **Rationale**: Integration check ensuring no test interactions or shared state issues when all tests run together. (C5)

**AC#25: BodySettings.cs body option indices use 515-518 (not 513-516) (Pos)**
- **Test**: Grep pattern=`setCflag\(characterId, 51[3-4],` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (all body option setCflag calls use indices 515-518, not 513-516; absence of 513/514 confirms the +2 index shift was applied)
- **Rationale**: F370 mapped BodyOption1-4 to CFLAG indices 513-516, but CFLAG.CSV maps 体オプション1-4 to 515-518 (indices 513-514 are undefined gaps). This AC verifies the index correction by checking that no setCflag calls use the old indices 513 or 514.

**AC#26: BodySettings.cs doc comments reference corrected indices 515-518 (Pos)**
- **Test**: Grep pattern=`51[34].*体オプション` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (all doc comments referencing body option CFLAG indices 513 or 514 with 体オプション updated from 513-516 to 515-518)
- **Rationale**: Task 20 fixes the setCflag indices but BodySettings.cs also has doc comments at multiple locations (class summary line 21, BodyParams property comments lines 47-50, method summary line 413, apply comment line 469) that reference old indices 513-516. Pattern `51[34].*体オプション` catches both 513 and 514 references, ensuring property comments for BodyOption1 (513→515) and BodyOption2 (514→516) are both updated. These must be updated to prevent stale documentation, which constitutes technical debt under the Philosophy's "debt cleanup per sub-feature" requirement.

**AC#27: BodySettings.cs range comments updated from 500-516 to 500-512, 515-518 (Pos)**
- **Test**: Grep pattern=`500-516` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (all range references in doc comments updated from "500-516" to "500-512, 515-518")
- **Rationale**: AC#26 catches `513.*体オプション` patterns but BodySettings.cs also has 3 range comments at lines 30, 413, 469 that reference "500-516" without mentioning "体オプション". These escape AC#26's pattern. AC#27 ensures all range references are corrected, completing debt-zero verification for stale documentation.

**AC#28: BodySettings.cs doc comments for BodyOption3/4 use corrected indices 517-518 (Pos)**
- **Test**: Grep pattern=`515.*体オプション３|516.*体オプション４` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (doc comments for BodyOption3 use index 517, BodyOption4 use index 518)
- **Rationale**: AC#26 catches 513/514 doc comment references but misses the shift from 515→517 (BodyOption3) and 516→518 (BodyOption4). After the index fix, old `515=体オプション３` and `516=体オプション４` references become stale. AC#28 verifies these are updated to 517/518, completing full doc comment coverage for the 4-index shift.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Fix F370 CFLAG index bug: body option indices 513-516 → 515-518 | AC#25, AC#26, AC#27, AC#28 |
| 2 | Verify BodySettings.cs covers all 14 characters with correct CFLAG 500-512, 515-518 assignments | AC#1-AC#13, AC#17, AC#20, AC#21 |
| 3 | Expand per-character equivalence tests: strengthen char 0 F/M and char 1, add chars 2-13 | AC#1-AC#13, AC#17, AC#18, AC#19, AC#20, AC#21 |
| 4 | Verify zero technical debt in BodySettings.cs scope | AC#14 |

---

## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

This feature requires **one production code fix** (BodySettings.cs CFLAG index correction) plus test expansion in `engine.Tests/Tests/StateSettingsTests.cs`. The approach is:

1. **Fix F370 CFLAG index bug** - Correct BodySettings.cs body option indices from 513-516 to 515-518 to match CFLAG.CSV mapping (indices 513-514 are undefined gaps)
2. **Strengthen existing char 0 female/male tests** - Replace shallow assertions (7/17 and 2/17 values) with full value-level assertions for all 17 CFLAG values (500-512, 515-518) matching 体設定.ERB source
3. **Strengthen existing char 1 test** - Replace shallow ContainsKey assertions with full value-level assertions for all 17 CFLAG values (500-512, 515-518) matching 体設定.ERB source
4. **Add 12 new test methods** - Create equivalence tests for chars 2-13 using the same pattern (Dictionary-based mock accessors, value-level assertions)
5. **Verify existing infrastructure** - Use code-type ACs to confirm IBodySettings interface, DI registration, zero technical debt, and correct CFLAG indices
6. **Validate test completeness** - Use count-based code ACs to ensure all 14 characters have test coverage and HairColor==HairBaseColor invariant is verified

All new test methods follow the established pattern from `BodyDetailInit_Character0_Female_SetsFemaleDefaults()` and `BodyDetailInit_Character0_Male_SetsMaleDefaults()`:
- Arrange: Create Dictionary<(int, int), int> for cflag/talent, define mock lambdas
- Act: Call `_bodySettings.BodyDetailInit(charId, getCflag, setCflag, getTalent)`
- Assert: Verify all 17 CFLAG values (500-512, 515-518) match ERB source, plus HairColor==HairBaseColor for chars 1-13

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1-13 | Add/strengthen 13 test methods (char 1 strengthen + chars 2-13 new), each verifying all 17 CFLAG values against 体設定.ERB source lines. Tests execute via `dotnet test engine.Tests/ --filter BodyDetailInit_Character{N}`. See Tasks 1-13 |
| 14 | Grep "TODO\|FIXME\|HACK" in Era.Core/State/BodySettings.cs - expect 0 matches (debt-zero verification) |
| 15 | Grep "void BodyDetailInit" in Era.Core/Interfaces/IBodySettings.cs - verify interface exists |
| 16 | Grep "AddSingleton<IBodySettings, BodySettings>" in ServiceCollectionExtensions.cs - verify DI registration exists |
| 17 | Grep "public void BodyDetailInit_Character\d+" in StateSettingsTests.cs - count test method declarations, expect 15 (char 0 F/M + chars 1-13) |
| 18 | Grep "cflag\[\(charId, 504\)\].*cflag\[\(charId, 505\)\]" in StateSettingsTests.cs - count HairColor==HairBaseColor invariant assertions, expect 15 (char 0 F/M + chars 1-13) |
| 19 | Grep "cflag\[\(charId, 511\)\].*cflag\[\(charId, 512\)\]" in StateSettingsTests.cs - count SkinColor==SkinBaseColor invariant assertions, expect 15 (char 0 F/M + chars 1-13) |
| 20-21 | Strengthen char 0 female/male tests with all 17 CFLAG value assertions against 体設定.ERB lines 38-73. See Tasks 18-19 |
| 22-23 | Execute existing edge-case tests via `dotnet test engine.Tests/ --filter {test_name}` - verify they still pass (regression prevention) |
| 24 | Execute all BodySettings tests together via `dotnet test engine.Tests/ --filter FullyQualifiedName~StateSettingsTests.BodyDetailInit` - integration check |
| 25 | Grep BodySettings.cs for old setCflag indices 513-514 - expect 0 matches after fix. See Task 20 |
| 26 | Grep BodySettings.cs for old doc comment indices (51[34].*体オプション) - expect 0 matches after fix. See Task 20 |
| 27 | Grep BodySettings.cs for stale range "500-516" - expect 0 matches after fix. See Task 20 |
| 28 | Grep BodySettings.cs for stale BodyOption3/4 doc comment indices (515.*体オプション３, 516.*体オプション４) - expect 0 matches after fix. See Task 20 |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Test method structure | (A) Parameterized test with InlineData, (B) Individual test methods per character | B | Existing tests use individual methods. Consistency with codebase patterns. Each character has unique CFLAG values making individual methods clearer. |
| Char 1 test approach | (A) Delete and recreate, (B) Replace assertions in existing test | B | Preserve test method name/structure. Only assertions change from shallow (ContainsKey) to deep (Assert.Equal for all 17 values). |
| CFLAG value source | (A) Copy from BodySettings.cs C# data, (B) Derive from 体設定.ERB source | B | ERB is SSOT. Tests verify C# implementation matches ERB source. Each test comment references ERB line numbers for traceability. |
| HairColor==HairBaseColor verification | (A) Assert in each character test, (B) Separate parameterized test | A | Inline assertion (`Assert.Equal(cflag[(charId, 504)], cflag[(charId, 505)])`) in each char 1-13 test is clearer. AC#18 count-based grep confirms all 13 tests include this check. |
| Test project location | (A) Move to Era.Core.Tests, (B) Keep in engine.Tests | B | Existing StateSettingsTests.cs location. No migration needed. Matches constraint C5. |
| GameInitialization.cs | (A) Replace dummy lambdas, (B) No changes | B | Phase 22 scope per constraint C6. This feature is verification only, not runtime integration. |

### Interfaces / Data Structures

No new interfaces or data structures required. Uses existing:
- `IBodySettings` interface (Era.Core/Interfaces/IBodySettings.cs) - pre-existing from F377
- `BodySettings` class (Era.Core/State/BodySettings.cs) - pre-existing from F370
- `Dictionary<(int, int), int>` for mock CFLAG/TALENT accessors - established pattern in StateSettingsTests.cs
- `Func<int, int, int>` getCflag/getTalent, `Action<int, int, int>` setCflag - standard accessor signatures

### Upstream Issues

None identified. ACs comprehensively cover verification requirements, and constraints are complete.

---

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Strengthen char 1 equivalence test with all 17 CFLAG value assertions | | [x] |
| 2 | 2 | Add char 2 equivalence test with all 17 CFLAG value assertions | | [x] |
| 3 | 3 | Add char 3 equivalence test with all 17 CFLAG value assertions | | [x] |
| 4 | 4 | Add char 4 equivalence test with all 17 CFLAG value assertions | | [x] |
| 5 | 5 | Add char 5 equivalence test with all 17 CFLAG value assertions | | [x] |
| 6 | 6 | Add char 6 equivalence test with all 17 CFLAG value assertions | | [x] |
| 7 | 7 | Add char 7 equivalence test with all 17 CFLAG value assertions | | [x] |
| 8 | 8 | Add char 8 equivalence test with all 17 CFLAG value assertions | | [x] |
| 9 | 9 | Add char 9 equivalence test with all 17 CFLAG value assertions | | [x] |
| 10 | 10 | Add char 10 equivalence test with all 17 CFLAG value assertions | | [x] |
| 11 | 11 | Add char 11 equivalence test with all 17 CFLAG value assertions | | [x] |
| 12 | 12 | Add char 12 equivalence test with all 17 CFLAG value assertions | | [x] |
| 13 | 13 | Add char 13 equivalence test with all 17 CFLAG value assertions | | [x] |
| 14 | 14,15,16 | Verify existing infrastructure (debt-zero, interface, DI registration) | | [x] |
| 15 | 17,18,19 | Verify test completeness (test count, HairColor==HairBaseColor invariant, SkinColor==SkinBaseColor invariant) | | [x] |
| 16 | 22,23 | Verify existing edge-case tests still pass (regression prevention) | | [x] |
| 17 | 24 | Execute all BodySettings tests together (integration check) | | [x] |
| 18 | 20 | Strengthen char 0 female equivalence test with all 17 CFLAG value assertions | | [x] |
| 19 | 21 | Strengthen char 0 male equivalence test with all 17 CFLAG value assertions | | [x] |
| 20 | 25,26,27,28 | Fix BodySettings.cs body option CFLAG indices (513→515, 514→516, 515→517, 516→518) and update all doc comments referencing old indices (including range comments "500-516" → "500-512, 515-518" at lines 30, 413, 469, and BodyOption3/4 index comments 515→517, 516→518 at lines 21, 49, 50) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

All 20 tasks are untagged (KNOWN) because all Expected values are deterministic: test methods verify hardcoded CFLAG values derived from 体設定.ERB source, and static verification patterns are fixed.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 0 | implementer | sonnet | BodySettings.cs, CFLAG.CSV | Fix body option indices 513→515, 514→516, 515→517, 516→518 and update all doc comments referencing old indices (Task 20) |
| 1 | implementer | sonnet | StateSettingsTests.cs, 体設定.ERB source lines | Strengthened char 0 F/M + char 1 tests + 12 new char 2-13 tests (Tasks 1-13, 18-19) |
| 2 | implementer | sonnet | Era.Core files | Static verification results (Tasks 14-15) |
| 3 | ac-tester | sonnet | All BodySettings tests | Test execution results (Tasks 16-17) |

### Test Pattern

All character tests follow the established pattern from `BodyDetailInit_Character0_Female_SetsFemaleDefaults()` and `BodyDetailInit_Character0_Male_SetsMaleDefaults()`:

```csharp
[Fact]
public void BodyDetailInit_Character{N}_SetsExpectedValues()
{
    // Arrange
    const int charId = {N};
    var cflag = new Dictionary<(int, int), int>();
    var talent = new Dictionary<(int, int), int>();

    Func<int, int, int> getCflag = (id, idx) => cflag.TryGetValue((id, idx), out var val) ? val : 0;
    Action<int, int, int> setCflag = (id, idx, val) => cflag[(id, idx)] = val;
    Func<int, int, int> getTalent = (id, idx) => talent.TryGetValue((id, idx), out var val) ? val : 0;

    // Act
    _bodySettings.BodyDetailInit(charId, getCflag, setCflag, getTalent);

    // Assert - all 17 CFLAG values (500-512, 515-518)
    Assert.Equal({expected_500}, cflag[(charId, 500)]); // HairLength
    Assert.Equal({expected_501}, cflag[(charId, 501)]); // HairLengthCategory
    // ... (500-512 indices, then 515-518 for body options)

    // HairColor == HairBaseColor invariant (all characters)
    Assert.Equal(cflag[(charId, 504)], cflag[(charId, 505)]); // HairBaseColor == HairColor

    // SkinColor == SkinBaseColor invariant (all characters)
    Assert.Equal(cflag[(charId, 511)], cflag[(charId, 512)]); // SkinBaseColor == SkinColor
}
```

### Expected Values by Character

| Char | ERB Lines | Key CFLAGs | Notes |
|:----:|-----------|------------|-------|
| 0F | 38-55 | 500=400, 501=4, 504=88, 505=88, 506=88, 507=88, 508=0, 511=2, 512=2 | Player female (HAS_VAGINA branch) |
| 0M | 57-73 | 500=100, 501=2, 504=88, 505=88, 506=88, 507=88, 508=0, 511=2, 512=2 | Player male (ELSE branch) |
| 1 | 76-95 | 500=500, 501=5, 502=4, 504=0, 505=0, 506=16, 507=16, 508=0, 511=2 | Meiling (hair length 500, category 5, color black) |
| 2 | 97-116 | 500=600, 501=6, 504=2, 506=61, 507=61, 508=0, 511=1, 515=14, 516=50, 517=58 | Koakuma (body options set) |
| 3 | 119-137 | 500=400, 501=4, 504=10, 506=10, 507=10, 508=4, 511=0 | Patchouli (eye expression 4) |
| 4 | 140-158 | 500=300, 501=3, 502=4, 504=90, 506=12, 507=12, 508=0, 511=1 | Sakuya |
| 5 | 161-179 | 500=300, 501=3, 504=11, 506=1, 507=1, 508=2, 511=0, 515=2, 516=12 | Remilia (vampire options) |
| 6 | 182-200 | 500=300, 501=3, 502=2, 504=89, 506=1, 507=1, 508=0, 511=0, 515=2, 516=13 | Flandre (vampire options) |
| 7 | 203-221 | 500=300, 501=3, 504=2, 506=61, 507=61, 508=0, 511=1, 515=14, 516=50, 517=58 | Koakuma child (same body options as char 2) |
| 8 | 224-242 | 500=300, 501=3, 504=20, 506=18, 507=18, 508=0, 511=1, 515=11 | Cirno (unique hair color 20) |
| 9 | 245-263 | 500=700, 501=7, 502=1, 504=28, 506=37, 507=37, 508=0, 511=1, 515=10 | Daiyousei (longest hair, unique colors) |
| 10 | 266-284 | 500=500, 501=6, 504=89, 506=89, 507=89, 508=0, 511=1 | Marisa (eye color matches hair) |
| 11 | 287-305 | 500=500, 501=6, 504=69, 506=65, 507=65, 508=0, 511=1 | Reimu |
| 12 | 308-326 | 500=300, 501=3, 504=89, 506=57, 507=57, 508=0, 511=1, 515=1 | Rumia (body option 1) |
| 13 | 329-347 | 500=300, 501=3, 504=89, 506=17, 507=17, 508=0, 511=1 | Alice |

**Source**: Derive exact values from 体設定.ERB source lines listed above. Each test method must include a comment referencing the ERB line range (e.g., `// Source: 体設定.ERB:76-95`).

### Task 1: Strengthen Char 1 Test

Replace existing shallow test (ContainsKey only) with full value-level assertions:

**Before** (existing StateSettingsTests.cs:92):
```csharp
[Fact]
public void BodyDetailInit_Character1_SetsSpecificValues()
{
    // ... (ContainsKey assertions only)
}
```

**After**:
```csharp
[Fact]
public void BodyDetailInit_Character1_SetsSpecificValues()
{
    // Source: 体設定.ERB:76-95
    // Arrange
    const int charId = 1;
    var cflag = new Dictionary<(int, int), int>();
    var talent = new Dictionary<(int, int), int>();

    Func<int, int, int> getCflag = (id, idx) => cflag.TryGetValue((id, idx), out var val) ? val : 0;
    Action<int, int, int> setCflag = (id, idx, val) => cflag[(id, idx)] = val;
    Func<int, int, int> getTalent = (id, idx) => talent.TryGetValue((id, idx), out var val) ? val : 0;

    // Act
    _bodySettings.BodyDetailInit(charId, getCflag, setCflag, getTalent);

    // Assert - all 17 CFLAG values (500-512, 515-518)
    Assert.Equal(500, cflag[(charId, 500)]); // CFLAG:1:500 髪の長さ
    Assert.Equal(5, cflag[(charId, 501)]);   // CFLAG:1:501 髪の長さカテゴリ
    Assert.Equal(4, cflag[(charId, 502)]);   // CFLAG:1:502 髪型オプション1
    Assert.Equal(0, cflag[(charId, 503)]);   // CFLAG:1:503 髪型オプション2
    Assert.Equal(0, cflag[(charId, 504)]);   // CFLAG:1:504 髪原色
    Assert.Equal(0, cflag[(charId, 505)]);   // CFLAG:1:505 髪色
    Assert.Equal(16, cflag[(charId, 506)]);  // CFLAG:1:506 瞳色(右)
    Assert.Equal(16, cflag[(charId, 507)]);  // CFLAG:1:507 瞳色(左)
    Assert.Equal(0, cflag[(charId, 508)]);   // CFLAG:1:508 目つき
    Assert.Equal(0, cflag[(charId, 509)]);   // CFLAG:1:509 瞳オプション１
    Assert.Equal(0, cflag[(charId, 510)]);   // CFLAG:1:510 瞳オプション２
    Assert.Equal(2, cflag[(charId, 511)]);   // CFLAG:1:511 肌原色
    Assert.Equal(2, cflag[(charId, 512)]);   // CFLAG:1:512 肌色 (= 肌原色)
    Assert.Equal(0, cflag[(charId, 515)]);   // CFLAG:1:515 体型オプション1
    Assert.Equal(0, cflag[(charId, 516)]);   // CFLAG:1:516 体型オプション2
    Assert.Equal(0, cflag[(charId, 517)]);   // CFLAG:1:517 体型オプション3
    Assert.Equal(0, cflag[(charId, 518)]);   // CFLAG:1:518 体型オプション4

    // HairColor == HairBaseColor invariant
    Assert.Equal(cflag[(charId, 504)], cflag[(charId, 505)]);

    // SkinColor == SkinBaseColor invariant
    Assert.Equal(cflag[(charId, 511)], cflag[(charId, 512)]);
}
```

### Tasks 2-13: Add New Character Tests

For each character 2-13, create a new test method following the pattern above. Derive all 17 CFLAG values from the ERB source lines listed in the Expected Values table.

### Build Verification

After each character test addition:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build engine.Tests/ < /dev/null'
```

Expected: Build succeeds with no warnings (TreatWarningsAsErrors=true).

### Success Criteria

- All 24 ACs pass
- All BodySettings tests execute successfully (char 0 F/M, chars 1-13, OOR, null = 17 total test methods)
- Zero technical debt in BodySettings.cs scope
- HairColor==HairBaseColor invariant verified for all chars 1-13
- SkinColor==SkinBaseColor invariant verified for all chars 1-13

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

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
| 2026-02-17 | DEVIATION | ac-static-verifier | AC#28 code grep | FAIL: pattern `515.*体オプション３\|516.*体オプション４` false-positive match on line 21 (all 4 body options on single line, `.*` bridges across comma-separated values) |
| 2026-02-17 | FIX | orchestrator | AC#28 false-positive | Split BodySettings.cs doc comment line 21 into two lines to prevent `.*` bridging across comma-separated body option values |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: Expected Values by Character table | Systematic CFLAG index misassignment (508/509/511 swapped for SkinBaseColor/EyeExpression)
- [fix] Phase2-Review iter1: Task 1 code template (lines 626-630) | CFLAG index/comment misassignment matching Expected Values table error
- [fix] Phase2-Review iter1: AC#17 title (line 301) | Title "at least 18" contradicted Expected count_equals=15
- [fix] Phase2-Review iter1: AC#17 Details header | Title mismatch with Expected value
- [fix] Phase2-Review iter1: Goal Coverage Verification | Goal 4 removed (already addressed during FC, no genuine AC coverage)
- [fix] Phase2-Review iter2: AC#14 Expected column | Escaped pipe characters in TODO|FIXME|HACK to preserve table structure
- [fix] Phase2-Review iter2: AC Definition Table + Details + Coverage | Added AC#19 SkinColor==SkinBaseColor invariant (symmetric with AC#18), renumbered AC#19-23→AC#20-24
- [fix] Phase2-Uncertain iter2: Task 1 Before/After code blocks | Method name SetsBasicValues→SetsSpecificValues to match actual test at StateSettingsTests.cs:92
- [fix] Phase2-Review iter3: AC#17/18/19 Expected column | Added backticks to count_equals pattern per ENGINE.md Issue 65 format
- [fix] Phase2-Review iter3: Goal 1 wording | Clarified asymmetric verification scope (existing tests for char 0, new value-level tests for chars 1-13)
- [fix] Phase2-Review iter3: Goal Coverage Goal 2 | Added AC#19 (SkinColor invariant) to covering ACs
- [fix] Phase2-Review iter4: Test Pattern + Task 1 code | Changed from numeric literals to charId variable for AC#18/19 grep pattern consistency
- [fix] Phase2-Review iter4: Test Pattern | Added SkinColor==SkinBaseColor invariant assertion for AC#19 grep match
- [fix] Phase2-Review iter4: AC#17 Expected column | Tightened grep pattern to `public void BodyDetailInit_Character\d+` for method-only matching
- [fix] Phase2-Review iter5: Between Technical Design and Tasks | Added missing fc-phase-4-completed marker
- [fix] Phase2-Review iter5: AC#18 pattern + Details | Changed from cflag[(charId, 505)] to cflag[(charId, 504)].*cflag[(charId, 505)] to match only invariant assertions (avoids double-match)
- [fix] Phase2-Review iter5: AC#19 pattern + Details | Changed from cflag[(charId, 512)] to cflag[(charId, 511)].*cflag[(charId, 512)] to match only invariant assertions (avoids double-match)
- [fix] Phase2-Review iter5: AC Coverage section | Updated AC#17/18/19 descriptions to match new patterns
- [resolved-applied] Phase2-Pending iter1: [AC-002] Char 0 tests (AC#20/AC#21) only verify 7/17 CFLAG values (female: 500,501,505,506,507,508,511; male: 500,501). Philosophy claims "each sub-feature must include its own equivalence tests" for all 14 chars. Options: (A) Add Tasks/ACs to strengthen char 0 female/male tests to full 17-value assertions, or (B) Narrow Goal 1 wording to explicitly accept char 0 existing coverage as sufficient.
- [fix] Phase3-Maintainability iter1: AC#18/AC#19 Details | Added assertion order constraint notes to bind implementer to grep-matchable argument order
- [fix] PostLoop-UserFix post-loop: AC#20/AC#21 + Goals + Tasks + Implementation Contract | User chose Option A: Strengthened char 0 tests to full 17-value assertions. Added Tasks 18-19, updated AC#20/21 descriptions/details, updated Goal Coverage, Philosophy Derivation, Expected Values table, AC Coverage, and Technical Design Approach.
- [fix] Phase2-Review iter3: AC#18/AC#19 count_equals + Details + AC Coverage + Test Pattern | Updated from 13→15 to include char 0 F/M invariant assertions per AC#20/AC#21 scope expansion
- [resolved-applied] Phase2-Review iter4: [AC-003] Pre-existing F370 CFLAG index bug: BodySettings.cs maps body options (体オプション1-4) to indices 513-516, but CFLAG.CSV maps them to 515-518 (with 513-514 as undefined gaps). F778's Philosophy demands "verify migration correctness" but the migration has a 2-index offset for body options. Options: (A) Create new feature to fix F370 bug, F778 tests verify current (buggy) C# behavior as characterization tests + add Mandatory Handoff, or (B) Fix indices in F778 scope (expand from verification-only to include code fix).
- [fix] Phase3-Maintainability iter2: Success Criteria line 682 | Changed '18 total test methods' to '17' (2+13+1+1=17, not 18)
- [fix] PostLoop-UserFix post-loop: Goals + Summary + AC#25 + Task 20 + Expected Values + Implementation Contract + Test Pattern | User chose Option A (scope expansion): Added Goal 1 (fix indices), AC#25 (verify correct indices), Task 20 (fix BodySettings.cs), updated Expected Values body option indices 513-516→515-518, added Implementation Contract Phase 0
- [info] Phase1-DriftChecked: F781 (Related)
- [fix] Phase2-Review iter1: AC#25 pattern + Details + AC Coverage | Changed grep pattern from `BodyOption[1234]\s*=\s*51[3-6]` to `setCflag\(characterId, 51[3-4],` to match actual code structure (old pattern was vacuously passing)
- [fix] Phase2-Review iter2: Task 20 + AC#26 + Goal Coverage + AC Coverage + Impl Contract | Added AC#26 (doc comment index verification) and expanded Task 20 scope to include doc comment updates for stale 513-516 references
- [fix] Phase2-Review iter1: C1, C1 Details, Philosophy Derivation, AC#1/AC#20/AC#21 Details, Technical Design | Replaced stale 'CFLAG 500-516' with 'CFLAG 500-512, 515-518' at 7 locations (indices 513-514 undefined, 517-518 missing)
- [fix] Phase3-Maintainability iter1: AC#27 + Task 20 + Goal Coverage + AC Coverage | Added AC#27 (range comment "500-516" not_matches) to catch stale doc comments at BodySettings.cs:30,413,469 that escape AC#26's pattern
- [fix] Phase3-Maintainability iter2: AC#26 pattern + Details + AC Coverage | Broadened pattern from `513.*体オプション` to `51[34].*体オプション` to catch both 513 and 514 doc comment references (line 48 property comment escaped original pattern)
- [fix] Phase3-Maintainability iter3: AC#28 + Task 20 + Goal Coverage + AC Coverage | Added AC#28 (BodyOption3/4 doc comment index shift 515→517/516→518 verification) to catch stale assignments that escape AC#26's 51[34] pattern

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F370](archive/feature-370.md) - Body & State Systems Migration (created BodySettings.cs)
- [Predecessor: F377](archive/feature-377.md) - Phase 4 Architecture Refactoring (added IBodySettings + DI)
- [Predecessor: F647](feature-647.md) - Phase 20 Planning (parent)
- [Successor: F782](feature-782.md) - Post-Phase Review Phase 20
- [Related: F779](feature-779.md) - Body Settings UI (sibling)
- [Related: F780](feature-780.md) - Genetics & Growth (sibling)
- [Related: F781](feature-781.md) - Visitor Settings (sibling)
- [Related: F791](feature-791.md) - IEntryPointRegistry (informational)
