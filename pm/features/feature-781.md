# Feature 781: Visitor Settings (体設定.ERB lines 1431-1976)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-16T00:00:00Z -->

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

## Type: erb

## Created: 2026-02-11

---

## Summary

Migrate visitor-specific body settings UI from 体設定.ERB (lines 1431-1976) to C#, preserving parallel structure to regular character settings with visitor-prefixed variables.

<!-- fc-phase-1-completed -->

---

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; F647 decomposed it into actionable sub-features.

### Problem (Current Issue)

The 3 visitor body settings functions (@体詳細設定訪問者, @体詳細整頓訪問者, @体詳細オプション設定訪問者) in 体設定.ERB (lines 1431-1976) cannot be migrated to C# because the C# interface layer lacks a generic accessor for user-defined global `#DIM SAVEDATA` variables. The 23 `訪問者_*` variables (DIM.ERH:14-38) are global SAVEDATA scalars, not character-scoped CFLAG arrays, so existing IVariableStore character methods cannot be used. IGameState.SetVariable (IGameState.cs:33) exists as a setter stub, but no corresponding GetVariable exists, leaving the visitor functions with no way to read their state from C#.

### Goal (What to Achieve)

Migrate the 3 visitor body settings functions to C# by: (1) creating a visitor variable access interface (IVisitorVariables or equivalent) that provides get/set for the 23 global SAVEDATA visitor variables, (2) implementing visitor settings business logic (menu range validation, reset dispatch) while selection-to-variable mapping, display formatting, input handling, and cancel flow (ERB RESTART) remain in ERB, (3) implementing deduplication and slot compaction logic, and (4) implementing mutual exclusion option validation. The C# implementation must produce equivalent behavior to the ERB original when called from MANSETTTING.ERB:294.

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

### Scope Reference

| File | Lines | Functions | Note |
|------|------:|-----------|------|
| Game/ERB/体設定.ERB | 1431-1976 | 3 | @体詳細設定訪問者, @体詳細整頓訪問者, @体詳細オプション設定訪問者 |

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why can't visitor functions be migrated? | They read and write 23 global SAVEDATA variables that have no C# accessor | DIM.ERH:14-38, 体設定.ERB:1437-1775 |
| 2 | Why do they use global SAVEDATA instead of CFLAG? | Visitor is a virtual character without a character ID; CFLAG requires CharacterId scope | DIM.ERH:14 (`#DIM SAVEDATA`, not `#DIM CHARADATA`) |
| 3 | Why can't IVariableStore be used? | IVariableStore only exposes character-scoped 2D arrays (CFLAG, ABL, TALENT, etc.) and a few 1D arrays (FLAG, TFLAG) | IVariableStore.cs:15-103 |
| 4 | Why isn't there a global variable interface? | IGameState.SetVariable exists but is setter-only; no GetVariable counterpart | IGameState.cs:33 (SetVariable stub) |
| 5 | Why (Root)? | No prior C# migration required read access to user-defined global SAVEDATA variables; the interface gap was never exercised | Zero grep results for IVisitorVariable or GetVariable in Era.Core |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Visitor settings remain unmigrated in ERB (体設定.ERB:1431-1976) | C# interface layer lacks get/set accessor for user-defined global SAVEDATA variables |
| Where | 体設定.ERB visitor functions | IVariableStore.cs (missing scope), IGameState.cs (missing getter) |
| Fix | Leave visitor functions in ERB | Create IVisitorVariables interface providing typed get/set for 23 visitor SAVEDATA variables |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Phase 20 Planning - decomposed this feature |
| F778 | [DRAFT] | Body Initialization (体設定.ERB lines 6-348) - sibling, no cross-calls |
| F779 | [DRAFT] | Body Settings UI (体設定.ERB lines 350-943) - sibling, parallel structure, no cross-calls |
| F780 | [DRAFT] | Genetics & Growth (体設定.ERB lines 944-1426) - sibling, no cross-calls |
| F789 | [DONE] | IStringVariables + I3DArrayVariables - Phase 20 extensions (interface pattern precedent) |
| F790 | [DONE] | IEngineVariables + ICsvNameResolver - engine data access layer (interface pattern precedent) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Variable Access | NEEDS_REVISION | No IVisitorVariables interface exists; IGameState.SetVariable is stub, no getter (IGameState.cs:33) |
| Display Formatting | NEEDS_REVISION | 汎用色SET (PRINT_STATE.ERB:1358) and 肌色SET (PRINT_STATE.ERB:968) have no C# equivalents; 12+ display format functions unmigrated |
| Core Logic | FEASIBLE | Dedup, compaction, and mutual exclusion logic is pure arithmetic; fully testable in isolation |
| Entry Point | FEASIBLE | Single caller at MANSETTTING.ERB:294; delegation pattern well-established |
| Naming Layer | FEASIBLE | VariableDefinitions.VisitorAppearance constants exist (VariableDefinitions.cs:49-83) |

**Verdict**: NEEDS_REVISION

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Interfaces | HIGH | New IVisitorVariables interface required for global SAVEDATA access |
| Era.Core/State/BodySettings.cs | HIGH | Visitor settings methods added (or new class) |
| Game/ERB/体設定.ERB | MEDIUM | Lines 1431-1976 replaced with C# delegation |
| Game/ERB/MANSETTTING.ERB | LOW | Caller at line 294 unchanged (ERB entry point preserved) |
| PRINT_STATE.ERB | LOW | Display format functions remain in ERB (out of scope for logic migration) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| 23 global SAVEDATA scalars, not character-scoped | DIM.ERH:14-38 | Cannot use IVariableStore character methods; new accessor required |
| Visitor UI omits menu selections 1, 15, 22 | 体設定.ERB:1488 (`RESULT != 15`) and absence of CASE 1 in selection rendering | Selection index mapping differs from F779 regular version |
| P size uses -2 offset | 体設定.ERB:1580,1713,1769 (`RESULT-2`) | Must preserve offset arithmetic in C# |
| RESTART loop for invalid input | 体設定.ERB:1778 | UI loop mechanism needed in C# (INPUT→validate→RESTART) |
| Display format functions in PRINT_STATE.ERB | PRINT_STATE.ERB:968,1358 | 汎用色SET/肌色SET must be called or stubbed; not in scope to migrate |
| Mutual exclusion: 5 range groups | 体設定.ERB:1900-1954 | Body options enforce one-per-range: 1-9, 10-29, 30-49, 50-54, 55-59 |
| P option range exclusion | 体設定.ERB:1962-1972 | P options 1-2 are mutually exclusive range (additional to simple duplicate check) |
| Derived value sync after compaction | 体設定.ERB:1878-1880 | 髪色=髪原色, 肌色=肌原色, 髪の長さ=髪の長さ指定*100 |
| Slot compaction order: highest-to-lowest | 体設定.ERB:1839-1877 | Body options compact from slot 4 down to slot 1 |
| Display-text-dependent input guard ("ーーー") | 体設定.ERB:1717 | Values with undefined display text rejected via RESTART; stays in ERB per Key Decision B (display-dependent logic) |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Global SAVEDATA interface gap blocks implementation | HIGH | HIGH | Create IVisitorVariables interface as first task; design for extensibility |
| PRINT_STATE display format functions have no C# equivalent | HIGH | MEDIUM | Stub display calls initially; delegate formatting to ERB via CALL until PRINT_STATE migrated |
| Premature coupling with F779 regular body settings | MEDIUM | MEDIUM | Keep implementations self-contained; shared patterns extracted only after both are [DONE] |
| Option validation ranges incorrectly ported | MEDIUM | HIGH | Exhaustive unit tests for all 5 range groups with boundary values |
| UI loop migration creates untestable code | MEDIUM | MEDIUM | Separate pure logic (dedup, validation, compaction) from UI (display, input) |
| Slot compaction order-sensitivity causes data loss | LOW | HIGH | Test compaction with all permutations of empty/filled slots |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Visitor function line count | Count lines 1431-1976 in 体設定.ERB | 546 lines | 3 functions total |
| Visitor variables | Grep `SAVEDATA 訪問者_` in DIM.ERH | 23 variables | All scalar globals |
| External callers | Grep `体詳細設定訪問者` in Game/ERB/ | 1 caller (MANSETTTING.ERB:294) | Single entry point |
| Existing C# constants | VisitorAppearance class in VariableDefinitions.cs | 23 constants | Naming layer ready |
| IVisitorVariables interface | Grep `IVisitorVariable` in Era.Core | 0 files | Does not exist yet |

**Baseline File**: `.tmp/baseline-781.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Visitor variables are global SAVEDATA, not CFLAG | DIM.ERH:14-38 | AC must verify new interface provides get/set for all 23 variables (not IVariableStore) |
| C2 | 23 distinct variables across 6 categories | DIM.ERH:14-38, VariableDefinitions.cs:49-83 | AC must verify all 23 variables accessible; count_equals matcher recommended |
| C3 | 5 range groups for body options mutual exclusion | 体設定.ERB:1900-1954 | AC must test each range boundary: 1-9, 10-29, 30-49, 50-54, 55-59 |
| C4 | Dedup logic is order-sensitive (option1 checked before option2) | 体設定.ERB:1789-1814 | AC must verify dedup respects slot priority order |
| C5 | Slot compaction shifts values downward (highest slot fills lowest empty) | 体設定.ERB:1826-1877 | AC must verify compaction produces correct slot assignments |
| C6 | 3 derived value syncs after compaction | 体設定.ERB:1878-1880 | AC must verify 髪色=髪原色, 肌色=肌原色, 髪の長さ=髪の長さ指定*100 |
| C7 | P size uses -2 offset | 体設定.ERB:1580,1769 | AC must verify P size = RESULT - 2 (not raw RESULT) |
| C8 | Color display uses SetColor+ResetColor pairs | 体設定.ERB:1437-1466 | AC must verify color method calls if display is in scope |
| C9 | [0]=reset, [999]=cancel conventions | 体設定.ERB:1629,1632,1636 | AC must verify reset clears to 0 and cancel exits without change |
| C10 | Visitor UI hides selection 1, excludes 15, caps at 21 | 体設定.ERB:1488 (RESULT != 15), CASE 1,2 at :1493, CASE 1 at :1723 | AC must verify: selection 1 accepted (hidden but handled, sets hair length); selection 15 rejected (IF excludes); selection 22 rejected (out of range, max 21) |
| C11 | P option range 1-2 mutual exclusion | 体設定.ERB:1962-1972 | AC must verify P options with values 1-2 enforce exclusivity beyond simple duplicate check |
| C12 | IConsoleOutput exists for Print/PrintLine/PrintForm | Interface Dependency Scan | AC can verify display output via IConsoleOutput methods |
| C13 | IStyleManager exists for SetColor/ResetColor | Interface Dependency Scan | AC can verify color calls via IStyleManager |
| C14 | IInputHandler exists for RequestNumericInput | Interface Dependency Scan | AC can verify input handling via IInputHandler |
| C15 | IEngineVariables exists for GetResult | Interface Dependency Scan | AC can verify RESULT access via IEngineVariables |
| C16 | IVisitorVariables DOES NOT EXIST | Interface Dependency Scan | CRITICAL GAP: new interface required as first deliverable |

### Constraint Details

**C1: Global SAVEDATA Scope**
- **Source**: DIM.ERH:14-38 declares 23 `#DIM SAVEDATA 訪問者_*` scalars
- **Verification**: Grep `#DIM SAVEDATA 訪問者_` in DIM.ERH returns 23 matches
- **AC Impact**: Must create and verify new interface (not reuse IVariableStore character methods)

**C2: Variable Completeness**
- **Source**: DIM.ERH:14-38 (23 variables), VariableDefinitions.cs:49-83 (23 naming constants)
- **Verification**: Count constants in VisitorAppearance class; compare with DIM.ERH declarations
- **AC Impact**: AC must count all 23 variables; consider count_equals matcher on interface methods

**C3: Mutual Exclusion Range Groups**
- **Source**: 体設定.ERB:1900-1954 (@体詳細オプション設定訪問者 body cases)
- **Verification**: Read ranges in CASE "体1" through "体4" -- five groups: 1-9, 10-29, 30-49, 50-54, 55-59
- **AC Impact**: Boundary value tests required for each range group (e.g., 9 and 10, 29 and 30, etc.)

**C4: Dedup Order Sensitivity**
- **Source**: 体設定.ERB:1789-1814 -- checks option1 vs option2 first, then cascading pairs
- **Verification**: If option1==option2, option2 is zeroed (not option1)
- **AC Impact**: Test with option1==option2 and verify option1 preserved, option2 cleared

**C5: Slot Compaction Direction**
- **Source**: 体設定.ERB:1839-1877 -- body option4 fills first available among 1,2,3; then option3 fills among 1,2; then option2 fills 1
- **Verification**: Set slot4 only, clear slots 1-3; verify slot4 value moves to slot1
- **AC Impact**: Test all permutations of empty/filled slot configurations

**C6: Derived Value Sync**
- **Source**: 体設定.ERB:1878-1880 (3 sync operations at end of 体詳細整頓訪問者)
- **Verification**: After compaction, 髪色 must equal 髪原色, 肌色 must equal 肌原色, 髪の長さ must equal 髪の長さ指定 * 100
- **AC Impact**: AC must verify all 3 derived values after running compaction

**C7: P Size Offset**
- **Source**: 体設定.ERB:1580 (display: `Ｐ大きさ(LOCAL-2)`), :1769 (set: `RESULT-2`)
- **Verification**: Input value 5 should store as 3 for P size
- **AC Impact**: AC must verify stored value = input value - 2

**C10: Hidden and Excluded Menu Selections**
- **Source**: 体設定.ERB:1488 (`RESULT >= 0 && RESULT <= 21 && RESULT != 15`) -- selection 15 explicitly excluded; selection 1 not displayed in UI menu (no `[1]` printed, menu jumps from `[0]` to `[2]`) but functionally handled by CASE 1,2 at line 1493 (sets LOCAL:2=10 for hair length choices) and CASE 1 at line 1723 (`訪問者_髪の長さ = RESULT*100`); selection 22 out of range (max 21)
- **Verification**: Read UI display at lines 1436-1485 -- no `[1]` label printed; read SELECTCASE at line 1490 -- CASE 1,2 handles selection 1; read CASE 1 at line 1723 -- sets hair length; IF at 1488 accepts RESULT=1 (does not exclude it)
- **AC Impact**: AC must verify: selection 1 is accepted and processed (sets hair length via CASE 1 at :1723); selection 15 is rejected (excluded by IF condition); selection 22 is rejected (out of range, max 21)

**C11: P Option Range Exclusion**
- **Source**: 体設定.ERB:1962-1972 -- P1 and P2 options with values 1-2 are mutually exclusive range
- **Verification**: If P option1 is 1 and P option2 is 2, validation returns 0 (reject)
- **AC Impact**: Test P options with both in range 1-2 simultaneously

**C16: IVisitorVariables Gap**
- **Source**: Grep `IVisitorVariable` returns 0 files in Era.Core
- **Verification**: Glob `Era.Core/Interfaces/IVisitor*.cs` returns no matches
- **AC Impact**: Interface creation is prerequisite; AC must verify interface exists with correct method signatures

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Related | F778 | [DONE] | Body Initialization (体設定.ERB lines 6-348) - sibling, no cross-calls |
| Related | F779 | [DONE] | Body Settings UI (体設定.ERB lines 350-943) - sibling, parallel structure, no cross-calls |
| Related | F780 | [PROPOSED] | Genetics & Growth (体設定.ERB lines 944-1426) - sibling, no cross-calls |
| Related | F789 | [DONE] | IStringVariables + I3DArrayVariables - interface pattern precedent |
| Related | F790 | [DONE] | IEngineVariables + ICsvNameResolver - interface pattern precedent |

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
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Pipeline Continuity - Each phase completion triggers next phase planning" | Business logic extraction must cover all 3 functions to unblock next phase | AC#1, AC#2, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#12, AC#13, AC#14, AC#17, AC#18, AC#21 |
| "SSOT: designs/full-csharp-architecture.md Phase 20 section defines the scope" | Implementation must follow Phase 20 interface patterns established by F789/F790 | AC#1, AC#2, AC#3, AC#16, AC#23, AC#24, AC#25 |
| "continuous development pipeline, clear phase boundaries, and documented transition points" | Zero technical debt in new code; no TODO/FIXME/HACK markers | AC#15 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IVisitorVariables.cs exists (Pos) | file | Glob(Era.Core/Interfaces/IVisitorVariables.cs) | exists | - | [x] |
| 2 | IVisitorVariables declares 23 getter methods | code | Grep(Era.Core/Interfaces/IVisitorVariables.cs) | count_equals | `int Get\w+\(\)` = 23 | [x] |
| 3 | IVisitorVariables declares 23 setter methods | code | Grep(Era.Core/Interfaces/IVisitorVariables.cs) | count_equals | `void Set\w+\(` = 23 | [x] |
| 4 | VisitorSettings C# implementation file exists (Pos) | file | Glob(Era.Core/State/VisitorSettings.cs) | exists | - | [x] |
| 5 | Dedup clears slot2 when slot1==slot2 (hair) (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 6 | Slot compaction moves slot4 to slot1 when slots 1-3 empty (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 7 | Derived value sync: hair color = hair original color (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 8 | Mutual exclusion rejects same range group in body options (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 9 | Mutual exclusion accepts different range groups (Neg) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 10 | P option range 1-2 mutual exclusion rejects both in range (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 11 | P size offset stores RESULT-2 (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 12 | Menu selection validation: accepts 1, rejects 15, rejects 22 (Pos/Neg) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 13 | Unit tests pass (all VisitorSettings tests) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 14 | C# build succeeds with zero warnings | build | dotnet build Era.Core | succeeds | - | [x] |
| 15 | Zero technical debt in new/modified files | code | Grep(Era.Core/) | not_matches | TODO\|FIXME\|HACK | [x] |
| 16 | VariableStore implements IVisitorVariables | code | Grep(Era.Core/Variables/VariableStore.cs) | matches | IVisitorVariables | [x] |
| 17 | Tidy() ValidateAndClear ordering: body 4→3→2 then P2 (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 18 | Reset clears option to 0 (Pos) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 19 | IVisitorVariables DI registration exists | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | IVisitorVariables | [x] |
| 20 | engine-dev SKILL.md updated with IVisitorVariables | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | IVisitorVariables | [x] |
| 21 | Simple duplicate validation for hair/eye/V option pairs (Pos/Neg) | test | dotnet test Era.Core.Tests --filter VisitorSettings | succeeds | - | [x] |
| 22 | VariableDefinitions.VisitorAppearance doc comment corrected to 23 | code | Grep(Era.Core/Common/VariableDefinitions.cs) | matches | 23 total | [x] |
| 23 | IVisitorSettings.cs exists (Pos) | file | Glob(Era.Core/Interfaces/IVisitorSettings.cs) | exists | - | [x] |
| 24 | VisitorSettings implements IVisitorSettings | code | Grep(Era.Core/State/VisitorSettings.cs) | matches | IVisitorSettings | [x] |
| 25 | IVisitorSettings DI registration exists | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | IVisitorSettings | [x] |

### AC Details

**AC#1: IVisitorVariables.cs exists (Pos)**
- **Test**: Glob pattern `Era.Core/Interfaces/IVisitorVariables.cs`
- **Expected**: File exists
- **Rationale**: C16 identifies IVisitorVariables as a CRITICAL GAP that does not exist. This AC verifies the interface file is created as the first deliverable, following the ISP pattern established by F789 (IStringVariables) and F790 (IEngineVariables). (C1, C16)

**AC#2: IVisitorVariables declares 23 getter methods**
- **Test**: Grep pattern=`int Get\w+\(\)` path=`Era.Core/Interfaces/IVisitorVariables.cs` | count
- **Expected**: 23 matches
- **Rationale**: C2 requires all 23 SAVEDATA visitor variables be accessible. DIM.ERH:14-38 declares exactly 23 `#DIM SAVEDATA 訪問者_*` variables. Each must have a typed getter method. The regex pattern `int Get\w+\(\)` matches method signatures in the interface file (no false positives expected in a pure interface). (C1, C2)

**AC#3: IVisitorVariables declares 23 setter methods**
- **Test**: Grep pattern=`void Set\w+\(` path=`Era.Core/Interfaces/IVisitorVariables.cs` | count
- **Expected**: 23 matches
- **Rationale**: Goal (1) requires get/set for all 23 variables. Every getter must have a corresponding setter per V2 multi-perspective checklist (Issue 65). Setters use void return (fire-and-forget convention from F789 IStringVariables pattern). The regex pattern `void Set\w+\(` matches method signatures in the interface file. (C1, C2)

**AC#4: VisitorSettings C# implementation file exists (Pos)**
- **Test**: Glob pattern `Era.Core/State/VisitorSettings.cs`
- **Expected**: File exists
- **Rationale**: Goal items (2), (3), (4) require C# implementation of visitor settings UI logic, dedup/compaction, and mutual exclusion. Following the existing BodySettings.cs pattern in Era.Core/State/, the visitor implementation belongs in VisitorSettings.cs. (C1)

**AC#5: Dedup clears slot2 when slot1==slot2 (hair) (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: when HairOption1==HairOption2 and HairOption1!=0, dedup sets HairOption2=0 while preserving HairOption1. Also verifies eye option dedup: when EyeOption1==EyeOption2 and EyeOption1!=0, dedup sets EyeOption2=0 while preserving EyeOption1 (ERB lines 1793-1796). Also verifies cascading dedup for body options (option1 vs option2, option1 vs option3, option1 vs option4, option2 vs option3, option2 vs option4, option3 vs option4).
- **Rationale**: C4 requires dedup to respect slot priority order. ERB source (lines 1789-1796, 1797-1814) performs dedup for hair, eye (瞳), and body options. Dedup must preserve the lower-numbered slot and clear the higher-numbered duplicate. (C4)

**AC#6: Slot compaction moves slot4 to slot1 when slots 1-3 empty (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: body options compact from slot4 downward (slot4 fills first empty among 1,2,3; slot3 fills among 1,2; slot2 fills slot1). Also verifies hair, eye (瞳), V, and P option pair compaction (4 distinct pair types). After compaction, no gaps remain (empty slots are at the end).
- **Rationale**: C5 specifies highest-to-lowest compaction order (ERB lines 1839-1877). A value in slot4 with slots 1-3 empty must move to slot1. (C5)

**AC#7: Derived value sync: hair color = hair original color (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying all 3 derived value syncs after compaction: (1) HairColor = HairOriginalColor, (2) SkinColor = SkinOriginalColor, (3) HairLength = HairLengthSpecified * 100.
- **Rationale**: C6 specifies 3 derived value syncs (ERB lines 1878-1880). These must execute at the end of the tidying function after all dedup and compaction operations. (C6)

**AC#8: Mutual exclusion rejects same range group in body options (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: for each of the 5 range groups (1-9, 10-29, 30-49, 50-54, 55-59), when ANY of the other 3 body slots has a value in the same range as the candidate, validation returns 0 (reject). Tests must verify OR-logic across all other slots from multiple slot perspectives: (a) slot=1, conflict in slot2 only → reject; (b) slot=1, conflict in slot3 only → reject; (c) slot=1, conflict in slot4 only → reject; (d) slot=2, conflict in slot1 → reject; (e) slot=3, conflict in slot4 → reject; (f) slot=4, conflict in slot3 → reject. Boundary values tested per group: 1 and 9 (group 1), 10 and 29 (group 2), 30 and 49 (group 3), 50 and 54 (group 4), 55 and 59 (group 5).
- **Rationale**: C3 requires boundary value testing for all 5 range groups. ERB source (lines 1900-1954) has 4 separate CASE blocks (体１/体２/体３/体４), each checking a different set of "other" slots. Tests must cover multiple slot perspectives to ensure each CASE path correctly identifies the other 3 slots. (C3)

**AC#9: Mutual exclusion accepts different range groups (Neg)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: when body option slots have values in different range groups (e.g., slot1=5 in range 1-9, candidate=15 in range 10-29), validation returns 1 (accept). Tests boundary crossings: 9 vs 10 (adjacent groups), 29 vs 30, 49 vs 50, 54 vs 55.
- **Rationale**: Negative test for C3. Ensures mutual exclusion does not falsely reject values in different range groups. Adjacent boundary values (9 and 10) are different groups and must be accepted simultaneously. (C3)

**AC#10: P option range 1-2 mutual exclusion rejects both in range (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: **Slot 2 (candidate for PenisOption2, existing PenisOption1)**: (1) when PenisOption1 is in range 1-2 and candidate for PenisOption2 is also in range 1-2, validation returns 0 (reject); (2) PenisOption1=1, candidate=3 returns 1 (accept, different and not both in range 1-2); (3) PenisOption1=3, candidate=1 returns 1 (accept, value 1 ok when other slot is outside range 1-2); (4) PenisOption1=3, candidate=3 returns 0 (reject, simple duplicate via fallthrough to RETURNF 0 at ERB line 1974); (5) PenisOption1=5, candidate=5 returns 0 (reject, duplicate). **Slot 1 (candidate for PenisOption1, existing PenisOption2)**: (6) PenisOption2=1, candidate=2 returns 0 (reject, both in range 1-2); (7) PenisOption2=3, candidate=1 returns 1 (accept, PenisOption2 outside range 1-2); (8) PenisOption2=4, candidate=4 returns 0 (reject, duplicate).
- **Rationale**: C11 specifies P options 1-2 have a special mutual exclusion range beyond simple duplicate check. ERB has separate CASE blocks: "Ｐ１" (lines 1961-1966, candidate for slot1 checked against existing PenisOption2) and "Ｐ２" (lines 1967-1972, candidate for slot2 checked against existing PenisOption1). Both slot paths must be tested to catch bugs in either CASE block. Combined logic: IF both in range [1,2] → reject; ELSEIF candidate != other → accept; ELSE fallthrough to RETURNF 0 (reject duplicate). (C11)

**AC#11: P size offset stores RESULT-2 (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: when raw input (RESULT) is 5 for P size, stored value is 3 (5-2). When raw input is 2, stored value is 0 (2-2).
- **Rationale**: C7 requires the -2 offset arithmetic from ERB lines 1580/1769. The display shows `Ｐ大きさ(LOCAL-2)` and assignment uses `RESULT-2`. (C7)

**AC#12: Menu selection validation: accepts 1, rejects 15, rejects 22 (Pos/Neg)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: (1) selection 1 is accepted (within valid range 0-21, not excluded) and maps to hair length category via CASE 1,2; (2) selection 15 is rejected (explicitly excluded by IF condition RESULT != 15); (3) selection 22 is rejected (out of range, max valid is 21); (4) selection 0 is accepted (valid); (5) selection 99 is rejected (out of range, returns false like any value >21).
- **Rationale**: C10 documents that selection 1 is hidden but functionally handled, selection 15 is explicitly excluded, and selection 22 is out of range. The IF condition at ERB line 1488 is `RESULT >= 0 && RESULT <= 21 && RESULT != 15`. (C10)

**AC#13: Unit tests pass (all VisitorSettings tests)**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter VisitorSettings'`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Umbrella AC ensuring all unit tests for the VisitorSettings module pass. Covers dedup (AC#5), compaction (AC#6), derived sync (AC#7), mutual exclusion (AC#8, AC#9, AC#10), P size offset (AC#11), and menu validation (AC#12). (C1-C11)

**AC#14: C# build succeeds with zero warnings**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`
- **Expected**: Build succeeds (exit code 0). TreatWarningsAsErrors=true ensures zero warnings.
- **Rationale**: Directory.Build.props enforces TreatWarningsAsErrors. New interface and implementation files must compile cleanly. (C16)

**AC#15: Zero technical debt in new/modified files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path=`Era.Core/` (directory-level scan covering all new/modified files)
- **Expected**: 0 matches in new/modified files (IVisitorVariables.cs, VisitorSettings.cs, VariableStore.cs)
- **Rationale**: Philosophy requires "documented transition points" with no untracked debt. Sub-Feature Requirements (line 48) mandate debt-free code. Directory-level grep ensures no new debt files are missed.

**AC#16: VariableStore implements IVisitorVariables**
- **Test**: Grep pattern=`IVisitorVariables` path=`Era.Core/Variables/VariableStore.cs`
- **Expected**: At least 1 match (class declaration includes IVisitorVariables in implements list). Additionally, VariableStore.cs doc comment updated from "7 interfaces" to "8 interfaces" with F781 added to the feature list.
- **Rationale**: Key Decisions table specifies "VariableStore implements IVisitorVariables" following F789 pattern where VariableStore implements IStringVariables. Without this AC, the interface could exist but never be wired to the implementation class. Philosophy claim "implementation must follow Phase 20 interface patterns established by F789/F790" and "documented transition points" require both the implementation binding and the doc comment update.

**AC#17: Tidy() ValidateAndClear ordering: body 4→3→2 then P2 (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying end-to-end Tidy() with order-sensitive scenario. Setup: body option1=5 (range 1-9), option2=15 (range 10-29), option3=7 (range 1-9), option4=3 (range 1-9). All values are distinct so Deduplicate() preserves them. After ValidateAndClearExclusiveOptions: option4=0 (cleared first, 3 conflicts with option1=5 in range 1-9), option3=0 (cleared second, 7 conflicts with option1=5 in range 1-9), option2=15 (retained, range 10-29), option1=5 (retained, first slot). Also verifies P option2 validation occurs after body clearing (ERB line 1823).
- **Rationale**: ERB lines 1816-1824 validate body slots in order 4→3→2 (not 1) then P slot 2. This order is semantically significant: clearing body option4 first changes the validation context for option3 (since option3's check at ERB CASE "体３" validates against option4 among others). Wrong order (e.g., 2→3→4) would produce different final state.

**AC#18: Reset clears option to 0 (Pos)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: when reset is triggered for a resettable option (e.g., HairOption1), the option value is set to 0. Tests cover at least one option per category (hair, eye, body, V, P) for reset.
- **Rationale**: C9 mandates "AC must verify reset clears to 0." ERB lines 1639-1675 implement per-selection reset (RESULT==0 sets the active option to 0). Cancel (RESULT==999) is purely ERB RESTART flow per Key Decision B — no C# method needed.

**AC#19: IVisitorVariables DI registration exists**
- **Test**: Grep pattern=`IVisitorVariables` path=`Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- **Expected**: At least 1 match (DI registration line for IVisitorVariables)
- **Rationale**: F789 pattern requires both class implementation (AC#16) AND DI container registration. Without DI registration, IVisitorVariables cannot be resolved at runtime. ServiceCollectionExtensions.cs must contain a registration line (e.g., `services.AddSingleton<IVisitorVariables>(sp => sp.GetRequiredService<VariableStore>())`).

**AC#20: engine-dev SKILL.md updated with IVisitorVariables**
- **Test**: Grep pattern=`IVisitorVariables` path=`.claude/skills/engine-dev/SKILL.md`
- **Expected**: At least 1 match (IVisitorVariables documented in Core Interfaces or Phase 20 section)
- **Rationale**: Per ssot-update-rules.md row 4, new `Era.Core/Interfaces/*.cs` (new interface) requires engine-dev SKILL.md update. IVisitorVariables must be discoverable by future agents/developers.

**AC#21: Simple duplicate validation for hair/eye/V option pairs (Pos/Neg)**
- **Test**: `dotnet test Era.Core.Tests --filter VisitorSettings`
- **Expected**: Test passes verifying: (1) ValidateSimpleDuplicate(5, 5) returns 0 (reject, duplicate); (2) ValidateSimpleDuplicate(5, 3) returns 1 (accept, different); (3) ValidateSimpleDuplicate(0, 5) returns 1 (accept, candidate 0 with non-zero other slot); (4) ValidateSimpleDuplicate(5, 0) returns 1 (accept, other slot empty); (5) ValidateSimpleDuplicate(0, 0) returns 0 (reject, matching ERB semantics — SIF 0 != 0 is false → RETURNF 0). Note: callers must handle RESULT=0 reset before calling validation (ERB lines 1637-1675 intercept reset before validation dispatch). Tests cover all 6 CASE paths from ERB: 髪1 (candidate for slot1, existing slot2), 髪2 (candidate for slot2, existing slot1), 目1, 目2, V1, V2.
- **Rationale**: ERB lines 1887-1898 (髪1/髪2/目1/目2) and 1955-1960 (V1/V2) implement simple duplicate checks for paired options. These 6 CASE branches are part of @体詳細オプション設定訪問者 which Goal item (4) requires migrating. A single `ValidateSimpleDuplicate` method handles all 6 cases since the logic is identical: `IF ARG:1 != other_slot_value THEN RETURNF 1 ELSE RETURNF 0`.

**AC#22: VariableDefinitions.VisitorAppearance doc comment corrected to 23**
- **Test**: Grep pattern=`23 total` path=`Era.Core/Common/VariableDefinitions.cs`
- **Expected**: At least 1 match (doc comment reads "23 total")
- **Rationale**: Source code VariableDefinitions.cs line 45 currently says "24 total" but DIM.ERH has exactly 23 variables and the class contains exactly 23 constants. AC#2 correctly expects 23 getters. The doc comment must be corrected to prevent confusion.

**AC#23: IVisitorSettings.cs exists (Pos)**
- **Test**: Glob pattern `Era.Core/Interfaces/IVisitorSettings.cs`
- **Expected**: File exists
- **Rationale**: All existing State classes (BodySettings, WeatherSettings, PregnancySettings) implement a corresponding interface for DI resolution and ERB delegation. VisitorSettings requires IVisitorSettings for runtime wiring and unit test mockability. (Phase 20 pattern compliance)

**AC#24: VisitorSettings implements IVisitorSettings**
- **Test**: Grep pattern=`IVisitorSettings` path=`Era.Core/State/VisitorSettings.cs`
- **Expected**: At least 1 match (class declaration includes IVisitorSettings in implements list)
- **Rationale**: Without implementation binding, the interface exists but VisitorSettings cannot be resolved via DI as IVisitorSettings. Follows BodySettings : IBodySettings pattern.

**AC#25: IVisitorSettings DI registration exists**
- **Test**: Grep pattern=`IVisitorSettings` path=`Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- **Expected**: At least 1 match (DI registration line for IVisitorSettings, e.g., `services.AddSingleton<IVisitorSettings, VisitorSettings>()`)
- **Rationale**: Without DI registration, IVisitorSettings cannot be resolved at runtime. ERB delegation layer depends on DI to obtain business logic instance. Follows existing IBodySettings DI registration pattern.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Creating a visitor variable access interface providing get/set for 23 global SAVEDATA visitor variables | AC#1, AC#2, AC#3, AC#16, AC#19, AC#20, AC#22, AC#23, AC#25 |
| 2 | Implementing visitor settings business logic (selection validation, reset) | AC#4, AC#12, AC#18, AC#24 |
| 3 | Implementing deduplication and slot compaction logic | AC#4, AC#5, AC#6, AC#7, AC#17 |
| 4 | Implementing mutual exclusion option validation | AC#4, AC#8, AC#9, AC#10, AC#17, AC#21 |
| 5 | C# implementation must produce equivalent behavior to ERB original | AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#12, AC#13, AC#17, AC#18, AC#21 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

This feature follows the established Phase 20 interface pattern (F789 IStringVariables, F790 IEngineVariables) by creating IVisitorVariables as an ISP-segregated interface for global SAVEDATA visitor variable access. The implementation consists of:

1. **IVisitorVariables Interface** (Era.Core/Interfaces/IVisitorVariables.cs): Defines 23 getter/setter pairs for all visitor appearance variables, following the naming convention established in VariableDefinitions.VisitorAppearance constants.

2. **VisitorSettings Business Logic** (Era.Core/State/VisitorSettings.cs): Pure C# implementation of the 3 ERB functions, separating concerns into:
   - **Deduplication**: Cascading duplicate removal preserving lower-numbered slots (髪/瞳/体 options only; V/P use selection-time validation, not tidy dedup)
   - **Mutual Exclusion Validation**: 5 range groups for body options (1-9, 10-29, 30-49, 50-54, 55-59) + P option range 1-2 exclusivity
   - **Slot Compaction**: Highest-to-lowest slot compaction to eliminate gaps (body option4→1, option3→1, option2→1)
   - **Derived Value Sync**: Post-compaction sync for 髪色=髪原色, 肌色=肌原色, 髪の長さ=髪の長さ指定*100

3. **UI Layer Delegation** (body setup entry point in ERB): The migrated C# provides pure business logic methods. Display formatting (汎用色SET, 肌色SET, 体オプション display) and input handling (INPUT loops, RESTART) remain in ERB until PRINT_STATE migration.

The approach satisfies all 25 ACs by:
- Creating the missing interface (AC#1) with complete get/set coverage (AC#2-3)
- Implementing testable business logic isolated from UI (AC#4-12, AC#17-18, AC#21)
- Wiring VariableStore implementation and DI registration (AC#16, AC#19)
- Providing IVisitorSettings business logic interface for DI/ERB delegation (AC#23, AC#24, AC#25)
- Following TDD with comprehensive unit tests (AC#13)
- Maintaining zero warnings, zero debt, and SSOT accuracy (AC#14-15, AC#20, AC#22)

**Key Design Decision**: VisitorSettings uses IVisitorVariables for data access rather than raw GlobalStatic calls, enabling unit testing with mock implementations and maintaining clean separation from the engine layer.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create Era.Core/Interfaces/IVisitorVariables.cs following ISP pattern from F789/F790 |
| 2 | Define 23 `int GetXxx()` methods in IVisitorVariables (HairLength, HairOriginalColor, HairColor, etc.) |
| 3 | Define 23 `void SetXxx(int value)` methods in IVisitorVariables (matching all getters per V2 Issue 65) |
| 4 | Create Era.Core/State/VisitorSettings.cs with public methods for dedup, compaction, validation |
| 5 | Unit test VisitorSettings.Deduplicate() verifying: HairOption1==HairOption2 → HairOption2=0 (slot1 preserved); body cascading pairs (1vs2, 1vs3, 1vs4, 2vs3, 2vs4, 3vs4) |
| 6 | Unit test VisitorSettings.Compact() verifying: body option4 value with empty slots 1-3 → option1 gets value, option4 cleared; hair/eye/P pairs; no gaps after compaction |
| 7 | Unit test VisitorSettings.SyncDerivedValues() verifying: GetHairColor()==GetHairOriginalColor(), GetSkinColor()==GetSkinOriginalColor(), GetHairLength()==GetHairLengthSpecified()*100 |
| 8 | Unit test VisitorSettings.ValidateBodyOption() verifying: candidate in range 1-9 with existing slot in 1-9 returns false (reject); test all 5 range boundaries |
| 9 | Unit test VisitorSettings.ValidateBodyOption() verifying: candidate=10, existing=9 returns true (accept, different groups); test all 4 adjacent boundaries (9/10, 29/30, 49/50, 54/55) |
| 10 | Unit test VisitorSettings.ValidatePenisOption() verifying: both in range 1-2 returns false (reject); option1=1, candidate=3 returns true (accept); option1=3, candidate=1 returns true |
| 11 | Unit test P size handling verifying: raw input 5 → stored value 3 (5-2); raw input 2 → stored value 0 (2-2) |
| 12 | Unit test menu selection validation verifying: selection 1 accepted (hidden but handled); selection 15 rejected (explicitly excluded); selection 22 rejected (out of range, max 21); selection 0 accepted; selection 99 triggers exit |
| 13 | TDD: Write all unit tests in Era.Core.Tests/State/VisitorSettingsTests.cs before implementation; verify all pass with `dotnet test --filter VisitorSettings` |
| 14 | Compile Era.Core with `dotnet build Era.Core`; TreatWarningsAsErrors=true enforces zero warnings |
| 15 | Verify no TODO/FIXME/HACK in IVisitorVariables.cs and VisitorSettings.cs via Grep |
| 16 | Grep `IVisitorVariables` in Era.Core/Variables/VariableStore.cs; verify VariableStore class implements the interface (F789 pattern) |
| 17 | Unit test Tidy() end-to-end with order-sensitive body option scenario verifying ValidateAndClearExclusiveOptions cascade (4→3→2, P2) |
| 18 | Unit test reset (option cleared to 0) for at least one option per category |
| 19 | Grep `IVisitorVariables` in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs; verify DI registration exists (F789 pattern) |
| 20 | Grep `IVisitorVariables` in .claude/skills/engine-dev/SKILL.md; verify SSOT documentation updated (ssot-update-rules.md compliance) |
| 21 | Unit test ValidateSimpleDuplicate() for all 6 paired option cases (髪1/髪2/目1/目2/V1/V2): duplicate → reject (0), different → accept (1), zero candidate → accept (1) |
| 22 | Fix VariableDefinitions.VisitorAppearance doc comment from "24 total" to "23 total" and verify via Grep |
| 23 | Create Era.Core/Interfaces/IVisitorSettings.cs with public method signatures (Tidy, ValidateMenuSelection, CalculatePenisSize, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, ResetOption) |
| 24 | VisitorSettings class declaration includes `: IVisitorSettings` in implements list |
| 25 | Grep `IVisitorSettings` in ServiceCollectionExtensions.cs; verify DI registration exists (e.g., `services.AddSingleton<IVisitorSettings, VisitorSettings>()`) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Interface location | (A) Extend IVariableStore, (B) New ISP interface IVisitorVariables | B | F789 established ISP pattern for variable access interfaces; visitor variables are global SAVEDATA (not character arrays), requiring separate interface |
| Data structure | (A) Dictionary<string, int>, (B) 23 individual properties, (C) Interface with 23 methods | C | Follows VariableDefinitions.VisitorAppearance naming layer; enables strong typing and compile-time verification |
| Validation return type | (A) Result<bool>, (B) bool, (C) int (0/1 per ERB) | C | Preserves ERB semantics (RETURNF 0/1); avoids Result overhead for simple validation; matches existing validation pattern in 体設定.ERB |
| UI migration scope | (A) Full UI+logic in C#, (B) Logic only, UI in ERB | B | Display format functions (汎用色SET, 肌色SET) unmigrated per C104; PRINT_STATE migration deferred to separate feature; this feature focuses on business logic equivalence |
| Compaction algorithm | (A) Single pass with swap, (B) Iterative highest-to-lowest | B | Matches ERB implementation (lines 1839-1877); highest slot fills first available among lower slots preserves original semantics |
| DI registration | (A) VariableStore implements IVisitorVariables, (B) Separate VisitorVariables service | A | Consistent with F789 pattern where VariableStore implements IStringVariables; avoids proliferation of service classes |
| Backing storage | (A) 23 private int fields, (B) int[] array with VisitorAppearance index constants | B | Consistent with existing `_flags`/`_tflags` pattern in VariableStore; VisitorAppearance constants provide index mapping; single `int[23]` array with getter/setter delegating to array access |
| Business logic interface | (A) No interface (direct class use), (B) IVisitorSettings interface | B | All existing State classes (BodySettings, WeatherSettings, PregnancySettings) implement interfaces for DI resolution; ERB delegation requires interface-based DI; unit test mockability |

### Interfaces / Data Structures

#### IVisitorVariables Interface

```csharp
// Era.Core/Interfaces/IVisitorVariables.cs
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Interface for accessing visitor (訪問者) appearance variables.
/// Provides typed access to 23 global SAVEDATA variables for visitor character customization.
/// Feature 781 - Visitor Settings Migration
/// </summary>
public interface IVisitorVariables
{
    // Hair (髪)
    int GetHairLength();
    void SetHairLength(int value);

    int GetHairLengthSpecified();
    void SetHairLengthSpecified(int value);

    int GetHairOriginalColor();
    void SetHairOriginalColor(int value);

    int GetHairColor();
    void SetHairColor(int value);

    int GetHairOption1();
    void SetHairOption1(int value);

    int GetHairOption2();
    void SetHairOption2(int value);

    // Eyes (目/瞳)
    int GetEyeExpression();
    void SetEyeExpression(int value);

    int GetEyeColorRight();
    void SetEyeColorRight(int value);

    int GetEyeColorLeft();
    void SetEyeColorLeft(int value);

    int GetEyeOption1();
    void SetEyeOption1(int value);

    int GetEyeOption2();
    void SetEyeOption2(int value);

    // Skin (肌)
    int GetSkinOriginalColor();
    void SetSkinOriginalColor(int value);

    int GetSkinColor();
    void SetSkinColor(int value);

    // Body (体)
    int GetBodyOption1();
    void SetBodyOption1(int value);

    int GetBodyOption2();
    void SetBodyOption2(int value);

    int GetBodyOption3();
    void SetBodyOption3(int value);

    int GetBodyOption4();
    void SetBodyOption4(int value);

    // Vagina (Ｖ)
    int GetVaginaPosition();
    void SetVaginaPosition(int value);

    int GetVaginaOption1();
    void SetVaginaOption1(int value);

    int GetVaginaOption2();
    void SetVaginaOption2(int value);

    // Penis (Ｐ)
    int GetPenisSize();
    void SetPenisSize(int value);

    int GetPenisOption1();
    void SetPenisOption1(int value);

    int GetPenisOption2();
    void SetPenisOption2(int value);
}
```

**Design Notes**:
- All 23 methods return/accept `int` (ERA variable semantics)
- Method names follow C# PascalCase convention derived from VariableDefinitions.VisitorAppearance constants
- No `Result<T>` wrapper needed — all variables have fixed bounds [0, N] with 0 as safe default
- VariableStore will implement this interface using a `private int[] _visitorVars = new int[23]` backing array, with VisitorAppearance constants as indices (consistent with existing `_flags`/`_tflags` pattern). Each getter/setter delegates to `_visitorVars[VisitorAppearance.XxxIndex]`

#### IVisitorSettings Interface

```csharp
// Era.Core/Interfaces/IVisitorSettings.cs
namespace Era.Core.Interfaces;

/// <summary>
/// Interface for visitor appearance settings business logic.
/// Provides deduplication, compaction, validation, and reset operations.
/// Feature 781 - Visitor Settings Migration
/// </summary>
public interface IVisitorSettings
{
    void Tidy();
    void Deduplicate();
    void ValidateAndClearExclusiveOptions();
    void Compact();
    void SyncDerivedValues();
    int ValidateBodyOption(int candidateValue, int slot);
    int ValidatePenisOption(int candidateValue, int slot);
    int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue);
    bool ValidateMenuSelection(int selection);
    int CalculatePenisSize(int rawInput);
    void ResetOption(int selection);
}
```

#### VisitorSettings Class

```csharp
// Era.Core/State/VisitorSettings.cs
namespace Era.Core.State;

/// <summary>
/// Business logic for visitor appearance settings management.
/// Handles deduplication, compaction, mutual exclusion validation, and derived value sync.
/// Feature 781 - Visitor Settings Migration
/// </summary>
public class VisitorSettings : IVisitorSettings
{
    private readonly IVisitorVariables _variables;

    public VisitorSettings(IVisitorVariables variables)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
    }

    /// <summary>Perform deduplication, mutual exclusion validation, compaction, and derived sync</summary>
    public void Tidy()
    {
        Deduplicate();
        ValidateAndClearExclusiveOptions();
        Compact();
        SyncDerivedValues();
    }

    /// <summary>Remove duplicate values, preserving lower-numbered slots</summary>
    public void Deduplicate() { /* cascading dedup per ERB:1789-1814 */ }

    /// <summary>Validate and clear mutually exclusive body/P options</summary>
    public void ValidateAndClearExclusiveOptions() { /* per ERB:1817-1824 */ }

    /// <summary>Compact slots to remove gaps (highest-to-lowest)</summary>
    public void Compact() { /* per ERB:1826-1877 */ }

    /// <summary>Sync derived values: 髪色=髪原色, 肌色=肌原色, 髪の長さ=髪の長さ指定*100</summary>
    public void SyncDerivedValues() { /* per ERB:1878-1880 */ }

    /// <summary>
    /// Range group definitions for body option mutual exclusion.
    /// Data-driven design: adding new ranges requires only a data change, not a code change (OCP).
    /// </summary>
    private static readonly (int Min, int Max)[] ExclusiveRanges =
        [(1, 9), (10, 29), (30, 49), (50, 54), (55, 59)];

    /// <summary>Validate body option mutual exclusion (5 range groups)</summary>
    /// <param name="candidateValue">Value to validate</param>
    /// <param name="slot">Target slot (1-4)</param>
    /// <returns>1 if valid (accept), 0 if invalid (reject) per ERB semantics</returns>
    public int ValidateBodyOption(int candidateValue, int slot) { /* per ERB:1900-1954, iterates ExclusiveRanges */ }

    /// <summary>Validate P option mutual exclusion (range 1-2)</summary>
    /// <param name="candidateValue">Value to validate</param>
    /// <param name="slot">Target slot (1 or 2)</param>
    /// <returns>1 if valid (accept), 0 if invalid (reject) per ERB semantics</returns>
    public int ValidatePenisOption(int candidateValue, int slot) { /* per ERB:1962-1972 */ }

    /// <summary>Validate simple duplicate for paired options (hair, eye, V)</summary>
    /// <param name="candidateValue">Value to validate</param>
    /// <param name="otherSlotValue">Value in the other slot of the same pair</param>
    /// <returns>1 if valid (accept, candidate != other), 0 if invalid (reject, duplicate) per ERB semantics</returns>
    public int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue) { /* per ERB:1887-1898, 1955-1960 */ }

    /// <summary>Validate menu selection range and exclusions</summary>
    /// <returns>True if selection is valid (0-21 except 15)</returns>
    public bool ValidateMenuSelection(int selection)
    {
        return selection >= 0 && selection <= 21 && selection != 15;
    }

    /// <summary>Calculate P size storage value (RESULT-2 offset per ERB:1580,1769)</summary>
    public int CalculatePenisSize(int rawInput)
    {
        return rawInput - 2;
    }

    /// <summary>Reset option to 0 by menu selection index (per ERB:1637-1675)</summary>
    /// <param name="selection">Menu selection index (0-21, maps to specific visitor variable)</param>
    /// <remarks>
    /// Maps selection index to the corresponding IVisitorVariables setter with value 0.
    /// ERB handles RESULT=0 as reset before validation dispatch (lines 1637-1675).
    /// Cancel (RESULT=999) is purely ERB RESTART flow — not handled here.
    /// </remarks>
    public void ResetOption(int selection) { /* per ERB:1639-1675 SELECTCASE dispatch to SetXxx(0) */ }
}
```

**Design Notes**:
- Constructor injection of IVisitorVariables enables unit testing with mocks
- `Tidy()` orchestrates the 4-step process matching ERB @体詳細整頓訪問者
- Validation methods return `int` (0/1) to preserve ERB semantics (RETURNF 0 = reject, RETURNF 1 = accept)
- Pure business logic — no UI dependencies (IConsoleOutput, IInputHandler, IStyleManager)

**ERB-to-C# Function Mapping** (transition documentation):

| ERB Function | C# Method(s) | Scope |
|-------------|--------------|-------|
| @体詳細設定訪問者 | ValidateMenuSelection, CalculatePenisSize, ResetOption | Menu range check, P size offset, reset dispatch; selection-to-variable mapping stays in ERB |
| @体詳細整頓訪問者 | Tidy() → Deduplicate, ValidateAndClearExclusiveOptions, Compact, SyncDerivedValues | Full migration of tidy logic |
| @体詳細オプション設定訪問者 | ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate | Full migration of option validation logic |

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| Display format functions unmigrated | Feasibility Assessment, Impact Analysis | Add note: "汎用色SET (PRINT_STATE.ERB:1358) and 肌色SET (PRINT_STATE.ERB:968) have no C# equivalents; display formatting deferred to PRINT_STATE migration feature" |
| IInputHandler path inconsistency | AC Design Constraints C14 | Correct interface path from `Era.Core/Interfaces/IInputHandler.cs` to `Era.Core/Input/IInputHandler.cs` (actual location per Grep verification) |

**Verification**: Both issues are documentation clarifications, not blocking gaps. C14 correctly references IInputHandler's existence; path correction improves accuracy. Display format functions are explicitly marked out-of-scope in C104.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1a | 1,2,3,16,23,24 | Create IVisitorVariables interface with 23 get/set method pairs + wire VariableStore implementation (int[23] backing array with VisitorAppearance index constants) + IVisitorSettings interface with method signatures | | [x] |
| 1b | 19,20,22,25 | Register IVisitorVariables in DI container, update engine-dev SKILL.md, fix VariableDefinitions doc comment (24→23) + IVisitorSettings DI registration | | [x] |
| 2 | 4,13 | Create VisitorSettings class with Tidy/Deduplicate/Compact/SyncDerivedValues/ValidateSimpleDuplicate/ValidateBodyOption/ValidatePenisOption/ValidateMenuSelection/CalculatePenisSize/ResetOption methods | | [x] |
| 3 | 5,6,7,17 | Write unit tests for deduplication, compaction, derived value sync, and Tidy ordering (等価性検証: ERB-derived expected values) | | [x] |
| 4 | 8,9,10,11,12,18,21 | Write unit tests for validation logic (mutual exclusion, simple duplicate, P size offset, menu selection, reset) (等価性検証: ERB-derived expected values) | | [x] |
| 5 | 13,14 | Verify all tests pass and build succeeds with zero warnings | | [x] |
| 6 | 15 | Verify zero TODO/FIXME/HACK in all new files (AC#15 verification) | | [x] |

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
| Interface Creation | implementer | Sonnet | Task 1a | IVisitorVariables.cs with 23 get/set pairs + VariableStore wiring (int[23] backing) + IVisitorSettings.cs interface |
| Registration & Docs | implementer | Sonnet | Task 1b | DI registration + engine-dev SKILL.md + VariableDefinitions doc fix + IVisitorSettings DI registration |
| Test Definition (RED) | implementer | Sonnet | Task 3, 4 | VisitorSettingsTests.cs (TDD RED — tests fail) |
| Implementation (GREEN) | implementer | Sonnet | Task 2 | VisitorSettings.cs with business logic (TDD GREEN — tests pass) |
| Verification | implementer | Sonnet | Task 5 | Build + test pass confirmation |
| Debt Cleanup | implementer | Sonnet | Task 6 | Zero technical debt confirmation |

**TDD Approach**: Task 1a creates the interface + VariableStore wiring. Task 1b handles registration and doc updates. Tasks 3-4 write tests first (RED state), then Task 2 implementation makes tests pass (GREEN state). Task 5 verifies all tests pass and build succeeds.

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Shared validation abstraction (body option dedup/compaction/mutual exclusion) | F781 and F779 implement identical algorithms for different variable scopes; extraction deferred until both are [DONE] | Feature (Option B) | F794 | - |

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
| 2026-02-16T01 | Phase1 | initializer | Initialize F781 | READY:781:erb |
| 2026-02-16T02 | Phase2 | explorer | Investigate codebase | Pattern analysis complete |
| 2026-02-16T03 | Phase3-4 | implementer | Task 1a: Interfaces + wiring | SUCCESS: IVisitorVariables, IVisitorSettings, VariableStore wired |
| 2026-02-16T04 | Phase3-4 | implementer | Task 1b: DI + SKILL.md | SUCCESS: DI registered, SKILL.md updated |
| 2026-02-16T05 | Phase3 | implementer | Tasks 3+4: TDD RED tests | 56 tests, 29 failing (RED) |
| 2026-02-16T06 | Phase4 | implementer | Task 2: Business logic | SUCCESS: 56/56 tests pass (GREEN) |
| 2026-02-16T07 | Phase5 | orchestrator | Refactoring assessment | SKIP: no refactoring needed |
| 2026-02-16T08 | Phase7 | ac-tester | AC verification | OK:25/25 all PASS |
| 2026-02-16T09 | DEVIATION | Bash | ac-static-verifier AC#15 | exit code 1: PRE-EXISTING TODO in GameInitialization.cs, OrgasmProcessor.cs (not F781 files) |
| 2026-02-16T10 | DEVIATION | feature-reviewer | doc-check NEEDS_REVISION | IVisitorSettings missing from engine-dev SKILL.md; VisitorSettings.cs missing from State listing |
| 2026-02-16T11 | Phase8 | implementer | Fix SKILL.md doc gaps | Added IVisitorSettings + VisitorSettings.cs to engine-dev SKILL.md |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: [INV-003] IVisitorVariables Interface (line 487-488) | GetEyeType/SetEyeType renamed to GetEyeExpression/SetEyeExpression to match VariableDefinitions.VisitorAppearance.EyeExpression SSOT
- [fix] Phase2-Review iter1: [INV-003] Baseline Measurement (line 156) | VisitorAppearance constant count corrected from 22 to 23
- [fix] Phase2-Review iter2: [FMT-004] fc-phase-4-completed marker (line 271) | Removed duplicate marker; kept correct one at line 393
- [fix] Phase2-Review iter2: [AC-001] AC#8 Expected (line 342) | Expanded to require OR-logic multi-slot testing (conflict in slot2/3/4 independently)
- [fix] Phase2-Review iter2: [AC-005] AC#16 added | VariableStore implements IVisitorVariables (Phase 20 pattern compliance)
- [resolved-applied] Phase2-Uncertain iter2: [AC-005] Missing hair/eye/V selection-time option validation methods and ACs. ERB @体詳細オプション設定訪問者 handles 12 cases but design only has ValidateBodyOption and ValidatePenisOption. Simple duplicate validation for hair/eye/V at selection time (髪１/髪２/目１/目２/Ｖ１/Ｖ２) has no C# method or AC coverage.
- [fix] Phase2-Uncertain iter3: [AC-001] AC#10 Expected (line 353) | Expanded to include P option duplicate rejection test cases (PenisOption1=3, candidate=3 → reject; PenisOption1=5, candidate=5 → reject) covering fallthrough to RETURNF 0
- [fix] Phase2-Review iter4: [INV-003] Approach section (line 416) | Corrected "15 ACs" to "18 ACs"
- [fix] Phase2-Review iter4: [SCP-002] Goal item (2) | Narrowed from "UI logic (display, selection, validation)" to "business logic (selection validation, reset, cancel)" to match Key Decision B (Logic only, UI in ERB)
- [fix] Phase2-Review iter4: [AC-005] AC#17 added | Tidy() ValidateAndClearExclusiveOptions ordering (body 4→3→2, P2) end-to-end test
- [fix] Phase2-Review iter5: [AC-005] AC#18 added | Reset (option cleared to 0) and cancel (values unchanged) per C9 constraint
- [fix] Phase2-Review iter5: [AC-001] AC#8 Expected | Expanded to require multi-slot-perspective testing (slot=2 conflict in slot1, slot=3 conflict in slot4, etc.)
- [fix] Phase2-Review iter6: [AC-001] AC#12 Expected item (5) | Changed "selection 99 triggers exit" to "selection 99 is rejected (out of range)" — exit is ERB UI-loop flow, not validation
- [resolved-applied] Phase2-Uncertain iter6: [AC-005] AC#18 reset/cancel design gap. Goal item (2) includes "reset handling, cancel handling" and AC#18 requires tests, but VisitorSettings design has no ResetOption/Cancel method. Reset is plausible as C# dispatch method (selection→SetXxx(0)), but cancel is purely ERB RESTART flow. Design needs reconciliation.
- [fix] Phase2-Review iter7: [INV-003] C2 Constraint Details (line 194) | Corrected "22 naming constants" to "23 naming constants"
- [fix] Phase2-Review iter7: [AC-005] AC#5 Expected (line 330) | Added eye (瞳) option dedup verification
- [fix] Phase2-Uncertain iter7: [AC-005] Philosophy Derivation row 1 | Changed to "Business logic extraction" and expanded AC coverage to include AC#4-13
- [fix] Phase2-Review iter7: [FMT-004] Review Notes | Added category codes to all entries per error-taxonomy.md
- [resolved-applied] Phase3-Maintainability iter8: [AC-005] Shared validation abstraction. Body option validation/dedup/compaction logic is identical between visitor (F781) and regular character (F779). No shared component designed — risks full duplication when F779 implemented.
- [fix] Phase3-Maintainability iter8: [FMT-004] Implementation Contract | Reordered to TDD sequence: Interface → Test Definition (RED) → Implementation (GREEN) → Verification → Debt Cleanup
- [fix] Phase3-Maintainability iter8: [AC-004] AC#2/AC#3 matchers | Changed from fragile `int Get`/`void Set` to precise regex `^ *int Get\w+\(\)`/`^ *void Set\w+\(`
- [fix] Phase2-Review iter9: [INV-003] Approach Deduplication scope (line 421) | Corrected from '髪/瞳/体/Ｖ/Ｐ' to '髪/瞳/体 only' — V/P not deduped in tidy
- [fix] Phase2-Review iter9: [INV-003] Philosophy Derivation row 4 removed | 'equivalent behavior' is Goal claim, not Philosophy — already covered by Goal Coverage row 5
- [fix] Phase2-Uncertain iter9: [FMT-004] Tasks 3/4 descriptions | Added 等価性検証 annotation per Sub-Feature Requirement 3
- [resolved-applied] Phase3-Maintainability iter1: [INV-003] VariableDefinitions.VisitorAppearance doc comment says '24 total' but actual constant count is 23. Source code fix needed (Era.Core/Common/VariableDefinitions.cs line 45).
- [fix] Phase3-Maintainability iter1: [EXT-001] ValidateBodyOption design | Added static readonly ExclusiveRanges array for OCP-compliant data-driven range validation
- [fix] Phase3-Maintainability iter1: [FMT-004] Task 6 description | Reframed from 'Remove' to 'Verify zero' — aligns with Zero Debt Upfront (debt should never be created)
- [fix] Phase3-Maintainability iter2: [AC-005] AC#19 added | IVisitorVariables DI registration in ServiceCollectionExtensions.cs (F789 pattern compliance)
- [fix] Phase2-Review iter3: [INV-003] Approach section (line 433) | Corrected "18 ACs" to "19 ACs" after AC#19 addition
- [fix] Phase2-Review iter4: [AC-001] AC#17 Expected | Changed test values from option3=5,option4=5 (dedup-eliminated) to option3=7,option4=3 (survive dedup, test ValidateAndClear ordering)
- [fix] Phase2-Review iter4: [AC-001] AC#6 Expected | Clarified "eye (V)" to "eye (瞳), V" — 4 distinct pair compaction types (hair, eye, V, P)
- [fix] Phase2-Review iter5: [AC-001] AC#10 Expected | Added bidirectional slot testing — slot 1 test cases (6-8) for CASE "Ｐ１" path (existing PenisOption2)
- [fix] Phase3-Maintainability iter6: [INV-003] AC#16 Expected | Added VariableStore.cs doc comment update verification (7→8 interfaces, F781 added)
- [fix] Phase3-Maintainability iter7: [AC-005] AC#20 added | engine-dev SKILL.md SSOT update for IVisitorVariables (ssot-update-rules.md compliance)
- [fix] Phase3-Maintainability iter7: [AC-004] AC#15 path expanded | Added VariableStore.cs to tech debt coverage (all modified files)
- [resolved-applied] Phase3-Maintainability iter7: [DES-001] VariableStore backing storage for 23 visitor scalars not designed. Options: (A) 23 private int fields, (B) int[] array with VisitorAppearance constants as indices (consistent with existing _flags/_tflags pattern). Implementer must choose without design guidance.
- [fix] Phase2-Review iter1: [AC-005] AC#21 added | ValidateSimpleDuplicate method + AC for hair/eye/V paired option duplicate validation (6 CASE branches from ERB)
- [fix] Phase2-Review iter1: [AC-005] AC#22 added | VariableDefinitions doc comment correction task (24→23) with Grep verification AC
- [fix] Phase2-Review iter2: [INV-003] Approach section | Corrected "20 ACs" to "22 ACs" and expanded bullet points to cover AC#16-22
- [fix] Phase2-Review iter2: [AC-001] Philosophy Derivation row 1 | Added AC#17, AC#18, AC#21 to business logic extraction coverage
- [fix] Phase2-Uncertain iter3: [CON-001] Technical Constraints | Added "ーーー" display-text guard constraint (ERB:1717, stays in ERB per Key Decision B)
- [fix] Phase2-Uncertain iter4: [DES-001] Mandatory Handoffs | Added shared abstraction deferral tracking (F779 destination, extraction after both [DONE])
- [fix] Phase2-Review iter1: [AC-001] Goal Coverage row 5 | Removed AC#22 (doc comment fix ≠ behavioral equivalence); moved AC#22 to Goal item 1 (count accuracy)
- [fix] Phase2-Review iter1: [AC-001] Philosophy Derivation row 2 | Added AC#2 to ISP pattern coverage (AC#1, AC#2, AC#3, AC#16)
- [fix] Phase2-Uncertain iter2: [FMT-004] Task 2 description | Added omitted methods ValidateBodyOption/ValidatePenisOption/ValidateMenuSelection/CalculatePenisSize
- [fix] Phase2-Review iter3: [AC-001] AC#21 Expected | Added ValidateSimpleDuplicate(0,0)=0 test case + caller-ordering design note
- [fix] Phase2-Review iter3: [SCP-002] Goal item (2) | Clarified "selection validation, reset handling" → "menu range validation, reset dispatch" + added "selection-to-variable mapping" to ERB-side
- [resolved-applied] Phase3-Maintainability iter4: [DES-001] Task 1 bundles 7 ACs across 5 files (interface, wiring, DI, SKILL.md, doc comment). Proposed split: (1a) interface+wiring (AC#1,2,3,16), (1b) DI+SKILL.md+doc fix (AC#19,20,22). Implementation Contract also needs update if split.
- [fix] Phase3-Maintainability iter4: [FMT-004] Technical Design | Added ERB-to-C# Function Mapping table documenting transition points per Philosophy
- [fix] PostLoop-UserFix post-loop: [AC-005] AC#18 reset design | Added ResetOption(int selection) method to VisitorSettings design; updated Task 2 + mapping table; cancel excluded (ERB RESTART)
- [fix] PostLoop-UserFix post-loop: [DES-001] VariableStore backing storage | Selected int[23] array with VisitorAppearance index constants; added Key Decision + implementation note
- [fix] PostLoop-UserFix post-loop: [DES-001] Task 1 split | Split Task 1 into 1a (interface+wiring AC#1,2,3,16) and 1b (DI+SKILL.md+doc AC#19,20,22); updated Implementation Contract
- [fix] Phase4-ACValidation iter1: [AC-004] AC#2/AC#3 regex anchors | Removed `^ *` anchor from count_equals patterns for ac-static-verifier compatibility
- [fix] Phase4-ACValidation iter1: [AC-004] AC#15 multi-path | Changed comma-separated paths to directory-level Grep(Era.Core/) scan
- [resolved-applied] Phase3-Maintainability iter1: [DES-001] VisitorSettings has no IVisitorSettings interface and no DI registration. Existing State classes (BodySettings, WeatherSettings, etc.) all implement an interface for DI/ERB delegation. Without IVisitorSettings, ERB cannot obtain VisitorSettings instance. Proposed fix: create IVisitorSettings interface + DI registration + AC coverage.
- [fix] PostLoop-UserFix post-loop: [DES-001] IVisitorSettings interface | Added IVisitorSettings.cs interface + AC#23/24/25 + DI registration + Task updates + Key Decision + Philosophy Derivation update
- [resolved-applied] Phase3-Maintainability iter1: [DES-001] Shared validation abstraction handoff targets F779 (Body Settings UI) as Option B, but F779's scope is UI migration (lines 350-943), not cross-cutting abstraction extraction. Shared abstraction may not naturally emerge from F779's /fc. → User chose: create dedicated Feature [DRAFT] (Option A).

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (decomposed this feature)
- [Related: F778](feature-778.md) - Body Initialization (sibling, no cross-calls)
- [Related: F779](feature-779.md) - Body Settings UI (sibling, parallel structure, no cross-calls)
- [Related: F780](feature-780.md) - Genetics & Growth (sibling, no cross-calls)
- [Related: F789](feature-789.md) - IStringVariables + I3DArrayVariables (interface pattern precedent)
- [Related: F790](feature-790.md) - IEngineVariables + ICsvNameResolver (interface pattern precedent)
- [Successor: F794](feature-794.md) - Shared Body Option Validation Abstraction (extraction after F781+F779 [DONE])
