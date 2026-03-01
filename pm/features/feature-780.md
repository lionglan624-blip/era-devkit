# Feature 780: Genetics & Growth (体設定.ERB lines 944-1426)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-21T06:46:13Z -->

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

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; F647 decomposed it into actionable sub-features. Behavioral Correctness: migrated C# functions verified per branching path via seeded deterministic tests covering all branching paths (mutation, twin copy, visitor override, slot distribution, skin color, eye option, P size, growth stage, hair change).

### Problem (Current Issue)

Four genetics and growth functions (497 ERB lines across body_settings.ERB:932-1426) cannot be migrated to C# because the existing BodySettings class (Era.Core/State/BodySettings.cs) lacks the required dependency injection points: IRandomProvider for RAND-based genetics logic, IVisitorVariables for visitor trait override access, IEngineVariables for NO: character registration lookups, and a callback mechanism for the external multi-birth father determination function (multi_birth_father_check in 妊娠処理変更パッチ.ERB:310-358) which uses CSTR/SPLIT operations not available in IVariableStore. Additionally, CharacterFlagIndex.cs is missing required CFLAG indices (Father=73, GrowthStage=75, ChildPSizeStore), and FlagIndex.cs lacks a MiscSettings(6441) constant needed for GETBIT(FLAG:雑多設定, 3) visitor body settings check. Because these DI interfaces and type constants are absent, the genetics system remains in ERB with untestable random logic and bidirectional coupling to PREGNACY_S.ERB.

### Goal (What to Achieve)

Migrate 4 functions (@body_change_daily, @body_settings_genetics, @body_settings_child_p_growth, @body_settings_child_hair_change) from body_settings.ERB to BodySettings.cs by: (1) extending IBodySettings with 4 new method signatures, (2) injecting IRandomProvider, IVisitorVariables, and IEngineVariables into BodySettings, (3) adding missing CharacterFlagIndex and FlagIndex constants, (4) implementing a Func<int,int> callback delegate for the external multi-birth father determination call, (5) replicating the skin color validation logic (valid set {0,1,2,3,10,11,13,14}), and (6) maintaining ERB wrapper functions for external callers (EVENTTURNEND.ERB, PREGNACY_S.ERB, 追加パッチverup.ERB).

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are genetics/growth functions not in C#? | They remain in ERB because F647 deferred them to F780 after F778 (initialization) and F779 (UI) were completed | feature-647.md line 54 |
| 2 | Why could they not be included in F778/F779? | These functions have distinct dependency profiles: randomness (RAND), visitor variables (SAVEDATA globals), external callback (multi-birth father determination), and FLAG bitfield access -- none of which were needed by initialization or UI logic | body_settings.ERB:998, :1034, :1082-1085 |
| 3 | Why does the genetics function need an external callback? | @body_settings_genetics calls multi_birth_father_check(mother) which uses CSTR (character string) variable index 13 and SPLIT operations to parse sperm-tracking data -- CSTR is not exposed through IVariableStore | 妊娠処理変更パッチ.ERB:310-358, IVariableStore.cs |
| 4 | Why are CFLAG indices missing? | CharacterFlagIndex.cs only defines indices used by F778/F779 (body appearance 500-518, V/P 402-408); genetics-specific indices (Father=73, GrowthStage=75, ChildPSizeStore) were not needed by prior features | CharacterFlagIndex.cs:29-59 |
| 5 | Why (Root)? | The existing BodySettings class was designed for initialization (F778) and UI validation (F779) use cases, with only IVariableStore DI. Genetics/growth functions require IRandomProvider, IVisitorVariables, IEngineVariables, and a callback delegate -- a fundamentally different dependency profile that necessitates extending the class DI surface | BodySettings.cs:29-34, IBodySettings.cs:10-24 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 4 ERB functions (497 lines) remain unmigrated with untestable random logic | BodySettings.cs lacks IRandomProvider, IVisitorVariables, IEngineVariables DI and CharacterFlagIndex is missing genetics-specific constants |
| Where | body_settings.ERB lines 932-1426 | BodySettings.cs constructor (line 29), CharacterFlagIndex.cs, FlagIndex.cs |
| Fix | Leave in ERB, test manually | Extend BodySettings DI with 3 new interfaces + callback delegate, add missing type constants, implement 4 methods |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Parent planning feature (Phase 20 decomposition) |
| F778 | [DONE] | Sibling: Body Initialization, established BodySettings.cs class |
| F779 | [DONE] | Sibling: Body Settings UI, added Tidy() which F780 calls at line 1336 |
| F781 | [DONE] | Related: Visitor Settings, created IVisitorVariables interface |
| F794 | [DONE] | Related: Shared Body Option Validation Abstraction |
| F796 | [DONE] | Related: BodyDetailInit IVariableStore Migration |
| F782 | [DRAFT] | Successor: Post-Phase Review |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Existing class extensible | FEASIBLE | BodySettings.cs has IVariableStore DI, GetCFlag/SetCFlag helpers |
| IRandomProvider available | FEASIBLE | Era.Core/Random/IRandomProvider.cs with Next(long max), Next(long min, long max) |
| IVisitorVariables available | FEASIBLE | Era.Core/Interfaces/IVisitorVariables.cs with 23 getter/setter pairs (F781) |
| IEngineVariables.GetCharacterNo available | FEASIBLE | Era.Core/Interfaces/IEngineVariables.cs:58 |
| FLAG access for GETBIT | FEASIBLE | IVariableStore.GetFlag(FlagIndex) exists; bit extraction is standard C# |
| Tidy() already migrated | FEASIBLE | BodySettings.Tidy() exists from F779 |
| External call bridgeable | FEASIBLE | Func<int,int> callback delegate pattern established in codebase |
| Skin color validation replicable | FEASIBLE | Valid set {0,1,2,3,10,11,13,14} identifiable from PRINT_STATE.ERB:934-962 |
| Missing CFLAG indices addable | FEASIBLE | CharacterFlagIndex.cs uses simple readonly record struct pattern |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| BodySettings.cs | HIGH | Constructor expansion with 3 new interface dependencies + callback delegate |
| IBodySettings.cs | HIGH | 4 new method signatures added to public interface |
| CharacterFlagIndex.cs | MEDIUM | 3+ new CFLAG index constants (Father, GrowthStage, ChildPSizeStore) |
| FlagIndex.cs | LOW | 1 new constant (MiscSettings=6441) |
| EVENTTURNEND.ERB | LOW | Caller of body_change_daily -- ERB wrapper preserved |
| PREGNACY_S.ERB | LOW | Bidirectional caller -- ERB wrappers preserved |
| 追加パッチverup.ERB | LOW | Caller of genetics + child_hair_change -- ERB wrappers preserved |
| Test suite | MEDIUM | New equivalence tests with seeded IRandomProvider |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| multi_birth_father_check uses CSTR+SPLIT | 妊娠処理変更パッチ.ERB:310-358 | Must remain as ERB callback; inject as Func<int,int> delegate |
| GETBIT requires bit manipulation on FLAG | body_settings.ERB:1034 | Need (GetFlag(6441) >> 3) & 1 for visitor body settings check |
| NO: needed for gender check | body_settings.ERB:1329, :1371 | IEngineVariables.GetCharacterNo() required for NO:child==148 |
| Skin color validation via @skin_color() | body_settings.ERB:1084 | Must replicate valid skin color set {0,1,2,3,10,11,13,14} in C# |
| Body option random distribution varies per slot | body_settings.ERB:1098-1219 | RAND modulo escalates: 100/120/140/160 per slot intentionally |
| Eye option 5 is non-inheritable | body_settings.ERB:1322-1325 | Must be cleared to 0 after genetics copy |
| Single constructor in BodySettings | BodySettings.cs:31 | F796 removed parameterless ctor; F780 extends the single IVariableStore constructor to accept all 5 dependencies |
| TreatWarningsAsErrors | Directory.Build.props | All new code must be warning-free |
| Asymmetric caller pattern | 追加パッチverup.ERB:117-122 | verup_patch calls genetics + child_hair_change but NOT child_p_growth |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| multi_birth_father_check callback interface design mismatch | MEDIUM | MEDIUM | Use simple Func<int,int> delegate (mother_id -> father_id) |
| Random distribution equivalence between ERA RAND and IRandomProvider | MEDIUM | HIGH | Verify IRandomProvider.Next semantics match ERA RAND; use seeded provider in tests |
| BodySettings constructor parameter explosion (now 4+ dependencies) | MEDIUM | MEDIUM | Consider separate GeneticsService class if constructor exceeds 5 params |
| Skin color validation drift from ERB source | LOW | MEDIUM | Extract valid set as constant; verify against PRINT_STATE.ERB |
| ERB callers break after migration | LOW | HIGH | Maintain ERB wrapper functions that delegate to C# |
| Growth stage constants need C# equivalents | LOW | LOW | 成長度_幼児=2, 成長度_成長期=3, 成長度_成人=4 must be defined as C# constants |
| Body option random tables complexity (16 trait slots with varying modulo) | LOW | MEDIUM | Table-driven implementation with clear slot-to-modulo mapping |
| F794 execution order: if F794 runs before F780, validation methods may be moved to BodyOptionValidator | MEDIUM | LOW | Run F780 before F794, or verify AC#4 pattern matches delegation stubs after F794 |
| Commented-out code at lines 1014-1018 | LOW | LOW | Implement active behavior only (LOCAL=8 for non-mutation). Add code comment referencing body_settings.ERB:1014-1018 for traceability. Do NOT preserve dead code in C# (zero-debt per C8/AC#15) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| BodySettings method count | grep -c "public.*void\|public.*int" Era.Core/State/BodySettings.cs | 7 | Current methods from F778+F779 |
| IBodySettings method count | grep -c "void\|int" Era.Core/Interfaces/IBodySettings.cs | 7 | Will become 11 after adding 4 new methods |
| CharacterFlagIndex constant count | grep -c "public static readonly" Era.Core/Types/CharacterFlagIndex.cs | 18 | Will increase by 3+ |
| Unit test count | dotnet test --filter "BodySettings" --list-tests | TBD at implementation | New equivalence tests added |

**Baseline File**: `.tmp/baseline-780.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Random-dependent logic cannot use exact value assertions | RAND usage in genetics/growth | Use seeded IRandomProvider for deterministic test results |
| C2 | multi_birth_father_check is external ERB function | body_settings.ERB:998 | Verify callback invocation with mock; do not test internal logic |
| C3 | Tidy() call must be verified after trait assignment | body_settings.ERB:1336 | Verify Tidy() is invoked at end of genetics method |
| C4 | Twin copy must copy ALL 24 CFLAG traits from sibling (ARG:2), not mother | body_settings.ERB:959-993 | Verify all CFLAG fields copied from sibling (ARG:2) to child; count = Father(1) + 15 body traits + 2 eye colors + 4 derived + 2 hair options = 24 |
| C5 | P size stored then reset to -2 for non-twin | body_settings.ERB:978-979, :1317-1319 | Verify ChildPSizeStore = PSize, then PSize = -2 |
| C6 | Eye option 5 is non-inheritable for BOTH 瞳オプション１ AND 瞳オプション２ | body_settings.ERB:1322-1325 | Verify BOTH eye options independently cleared to 0 when their value is 5 |
| C7 | Hair length category differs by child gender | body_settings.ERB:1329-1332 | Verify NO:child==148 -> category 2, else -> category 4 |
| C8 | Phase 20 sub-feature requires zero tech debt | architecture.md:4629-4637 | Include not_contains matcher for TODO/FIXME/HACK |
| C9 | Visitor override requires GETBIT bit 3 set; FlagIndex.MiscSettings=6441 | body_settings.ERB:1034, FLAG.yaml:507 | Test both paths: bit 3 set (use visitor traits) and unset (skip) |
| C10 | Body option random has increasing modulo per slot | body_settings.ERB:1098+ | Verify asymmetric RAND distribution preserved (100/120/140/160) |
| C11 | 1/15 mutation chance branching | body_settings.ERB:1011-1019 | Test both mutation and non-mutation paths with seeded random |
| C12 | verup_patch calls genetics + child_hair_change but NOT child_p_growth | 追加パッチverup.ERB:117-122 | Methods must be independently callable |
| C13 | IBodySettings needs 4 new methods | IBodySettings.cs (Interface Dependency Scan) | Verify interface extension with new method signatures |
| C14 | Skin color validation rejects invalid values | body_settings.ERB:1082-1086 | Valid set {0,1,2,3,10,11,13,14} only; loop has 20-iteration cap |
| C15 | Growth stage constants must be defined in C# | body_settings.ERB:1347-1362 (uses 成長度_幼児, 成長度_成長期, 成長度_成人) | Verify constants 幼児=2, 成長期=3, 成人=4 exist in C# type definition |
| C16 | BodyChangeDaily increments HairLength by 10 and clamps at category threshold | body_settings.ERB:932-939 | Test HairLength increment and clamp: HairLength += 10, if HairLength >= HairLengthCategory * 100 then reset to (HairLengthCategory - 1) * 100 |
| C17 | Slot distribution loop has 20-iteration cap (separate from skin color cap) | body_settings.ERB:1021-1029 | Verify WHILE 20 cap on slot distribution loop (SLOTM = RAND:16 + 1 selection) |

### Constraint Details

**C1: Seeded Random for Deterministic Tests**
- **Source**: All 3 investigations identify RAND usage in genetics (RAND:100/120/140/160), growth (RAND:4), and hair change (RAND)
- **Verification**: IRandomProvider.cs defines Next(long max) and Next(long min, long max) with [0, max) semantics
- **AC Impact**: All random-dependent ACs must use seeded IRandomProvider; expected values derived from seed + algorithm

**C2: External Callback for multi_birth_father_check**
- **Source**: body_settings.ERB:998 calls multi_birth_father_check(mother); function at 妊娠処理変更パッチ.ERB:310-358 uses CSTR/SPLIT
- **Verification**: Confirm CSTR is not in IVariableStore interface
- **AC Impact**: Mock Func<int,int> callback; verify invocation count and argument, not internal logic

**C3: Tidy() Integration**
- **Source**: body_settings.ERB:1336 calls body_detail_tidy(child) which maps to BodySettings.Tidy() from F779
- **Verification**: Tidy() method exists in IBodySettings.cs:18
- **AC Impact**: Verify Tidy() called after genetics trait assignment completes

**C4: Complete Twin Copy**
- **Source**: body_settings.ERB:959-993 copies 24 CFLAG fields from sibling (ARG:2) to child, not from mother
- **Verification**: Count CFLAG copy operations in ERB source (Father + 15 body traits + 2 eye colors + 4 derived + 2 hair options = 24 total)
- **AC Impact**: Verify all 24 fields match sibling's values after twin copy

**C5: P Size Store and Reset**
- **Source**: body_settings.ERB:978-979 stores PSize to ChildPSizeStore, then sets PSize to -2
- **Verification**: CFLAG index for ChildPSizeStore = 1193 (confirmed from CFLAG.yaml:821)
- **AC Impact**: Verify store operation and -2 reset in sequence

**C6: Non-Inheritable Eye Option 5**
- **Source**: body_settings.ERB:1322-1325 checks BOTH 瞳オプション１ and 瞳オプション２ independently; each cleared to 0 if value == 5
- **Verification**: Read ERB source for exact condition on both eye options
- **AC Impact**: Test with eye option 1 = 5 (cleared) + eye option 1 != 5 (preserved) AND eye option 2 = 5 (cleared) + eye option 2 != 5 (preserved)

**C7: Gender-Dependent Hair Length Category**
- **Source**: body_settings.ERB:1329-1332 uses NO:child==148 (son character slot)
- **Verification**: IEngineVariables.GetCharacterNo() available at IEngineVariables.cs:58
- **AC Impact**: Test both male (NO==148, category 2) and female (NO!=148, category 4)

**C9: Visitor Override GETBIT Check**
- **Source**: body_settings.ERB:1034 checks GETBIT(FLAG:雑多設定, 3) before applying visitor traits
- **Verification**: FLAG:6441 = 雑多設定 confirmed from FLAG.yaml:507 (index 35 = 参加人数, an unrelated flag; legacy readme cited 35 which is obsolete)
- **AC Impact**: Test with bit 3 set (visitor traits override genetics) and unset (normal genetics); FlagIndex.MiscSettings must equal 6441

**C10: Asymmetric Body Option Random**
- **Source**: body_settings.ERB:1098-1219 uses RAND:100 for slot 1, RAND:120 for slot 2, RAND:140 for slot 3, RAND:160 for slot 4
- **Verification**: Read ERB source for exact modulo values per slot
- **AC Impact**: Verify each slot uses correct modulo; seeded random must produce different distributions

**C14: Skin Color Validation**
- **Source**: PRINT_STATE.ERB:934-962 defines @skin_color() with valid values {0,1,2,3,10,11,13,14}
- **Verification**: Skin color != "---" check at body_settings.ERB:1084 (string returned by skin_color function)
- **AC Impact**: Verify valid skin colors accepted, invalid rejected; 20-iteration cap on retry loop

**C15: Growth Stage Constants**
- **Source**: body_settings.ERB:1347-1362 uses symbolic names 成長度_幼児, 成長度_成長期, 成長度_成人 for CFLAG:成長度 comparisons
- **Verification**: ERB symbolic names resolve to integer values; C# must define equivalent constants (幼児=2, 成長期=3, 成人=4)
- **AC Impact**: Verify C# GrowthStage constant type (or enum) defines Infant=2, GrowthPeriod=3, Adult=4; used by child_p_growth method branching logic

**C16: BodyChangeDaily HairLength Increment and Clamp**
- **Source**: body_settings.ERB:932-939 — increments CFLAG:髪の長さ by 10, then clamps: if HairLength >= HairLengthCategory * 100, reset to (HairLengthCategory - 1) * 100
- **Verification**: Read ERB lines 932-939 for exact arithmetic
- **AC Impact**: Test both paths: (a) normal increment (below threshold), (b) clamp at threshold

**C8: Zero Technical Debt**
- **Source**: architecture.md:4629-4637 — Phase 20 sub-feature zero-debt requirement
- **Verification**: Grep for TODO/FIXME/HACK in all modified/created files
- **AC Impact**: AC#15 verifies zero matches across all target files including test files

**C11: 1/15 Mutation Chance Branching**
- **Source**: body_settings.ERB:1011-1019 — `!RAND:15` → mutation path vs non-mutation
- **Verification**: ERB line 1011 checks `IF !RAND:15` for 1/15 mutation probability
- **AC Impact**: AC#33 verifies `Next(15)` appears in tests; AC#14 confirms both paths pass

**C12: Independent Callable Methods**
- **Source**: 追加パッチverup.ERB:117-122 — calls genetics + child_hair_change but NOT child_p_growth
- **Verification**: verup_patch calling pattern confirms methods must be independently callable
- **AC Impact**: AC#3 verifies both ChildPGrowth and ChildHairChange have separate method signatures on IBodySettings

**C13: IBodySettings Needs 4 New Methods**
- **Source**: IBodySettings.cs Interface Dependency Scan — currently 7 methods from F778+F779
- **Verification**: Current IBodySettings.cs method count is 7; will become 11 after F780
- **AC Impact**: AC#1, AC#2, AC#3 verify the 4 new method signatures; AC#4 verifies the 7 existing methods preserved

**C17: Slot Distribution WHILE 20 Cap**
- **Source**: body_settings.ERB:1021-1029 — `WHILE 20` loop for SLOTM = RAND:16 + 1 selection
- **Verification**: ERB line 1021 contains `WHILE 20` limiting slot distribution iterations
- **AC Impact**: AC#28 verifies 20-iteration cap is coded near slot logic in BodySettings.cs

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Predecessor | F779 | [DONE] | Body Settings UI -- Tidy() method needed. Evidence: body_settings.ERB:1336 CALL body_detail_tidy(child) |
| Related | F778 | [DONE] | Body Initialization (established BodySettings.cs class) |
| Related | F781 | [DONE] | Visitor Settings (provides IVisitorVariables interface) |
| Related | F794 | [DONE] | Shared Body Option Validation Abstraction |
| Related | F796 | [DONE] | BodyDetailInit IVariableStore Migration |
| Successor | F782 | [DRAFT] | Post-Phase Review |

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
| "ensures continuous development pipeline" | All 4 ERB functions migrated to C# with method signatures on IBodySettings and behavioral correctness verified | AC#1, AC#2, AC#3, AC#14, AC#18, AC#19, AC#27, AC#33, AC#35 |
| "clear phase boundaries" | ERB wrapper functions preserved for external callers (EVENTTURNEND, PREGNACY_S, verup_patch) | AC#4, AC#17 |
| "documented transition points" | ERB wrapper functions preserved for external callers, maintaining clear transition boundary between migrated C# and remaining ERB | AC#17 |
| "Pipeline Continuity - Each phase completion triggers next phase planning" | All 4 ERB functions migrated so F782 (Post-Phase Review) can proceed without ERB dependency blockers | AC#1, AC#2, AC#3 |
| "Behavioral Correctness: migrated C# functions verified per branching path" | All 4 migrated methods verified by seeded-random deterministic per-path tests covering mutation paths, twin copy completeness, visitor override, slot distribution, skin color validation, eye option clearing, P size store/reset, growth stage branching, and hair change logic | AC#16, AC#18, AC#19, AC#21, AC#22, AC#23, AC#24, AC#25, AC#26, AC#27, AC#28, AC#29, AC#30, AC#31, AC#32, AC#33, AC#34, AC#36, AC#37, AC#38, AC#43, AC#44, AC#45, AC#46, AC#48, AC#49, AC#50 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IBodySettings declares BodyChangeDaily | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | matches | `void BodyChangeDaily\(int` | [x] |
| 2 | IBodySettings declares BodySettingsGenetics | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | matches | `void BodySettingsGenetics\(int` | [x] |
| 3 | IBodySettings declares ChildPGrowth and ChildHairChange | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | count_equals | `void (BodySettingsChildPGrowth|BodySettingsChildHairChange)\(int` = 2 | [x] |
| 4 | IBodySettings existing methods preserved | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | count_equals | `(BodyDetailInit|Tidy|ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize|GetTightnessBaseValue)\(` = 7 | [x] |
| 5 | CharacterFlagIndex defines Father, GrowthStage, ChildPSizeStore | code | Grep(Era.Core/Types/CharacterFlagIndex.cs) | count_equals | `public static readonly CharacterFlagIndex (Father|GrowthStage|ChildPSizeStore)` = 3 | [x] |
| 6 | FlagIndex defines MiscSettings constant with value 6441 | code | Grep(Era.Core/Types/FlagIndex.cs) | matches | `MiscSettings = new\(6441\)` | [x] |
| 7 | BodySettings has IRandomProvider as private readonly field | code | Grep(Era.Core/State/BodySettings.cs) | matches | `private readonly IRandomProvider` | [x] |
| 8 | BodySettings has IVisitorVariables as private readonly field | code | Grep(Era.Core/State/BodySettings.cs) | matches | `private readonly IVisitorVariables` | [x] |
| 9 | BodySettings has IEngineVariables as private readonly field | code | Grep(Era.Core/State/BodySettings.cs) | matches | `private readonly IEngineVariables` | [x] |
| 10 | BodySettings stores callback delegate as private readonly field | code | Grep(Era.Core/State/BodySettings.cs) | matches | `private readonly Func<int,\s*int>` | [x] |
| 11 | Skin color validation with valid set {0,1,2,3,10,11,13,14} | code | Grep(Era.Core/State/BodySettings.cs) | matches | `ValidSkinColors.*0.*1.*2.*3.*10.*11.*13.*14` | [x] |
| 12 | Growth stage constants defined (None=0, Nursing=1, Infant=2, GrowthPeriod=3, Adult=4) | code | Grep(Era.Core/Types/GrowthStage.cs) | count_equals | `None\s*=\s*0|Nursing\s*=\s*1|Infant\s*=\s*2|GrowthPeriod\s*=\s*3|Adult\s*=\s*4` = 5 | [x] |
| 13 | Build succeeds with zero warnings | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 14 | Unit tests pass | test | dotnet test Era.Core.Tests/ --filter BodySettings | succeeds | - | [x] |
| 15 | Zero technical debt in new code | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/Interfaces/IBodySettings.cs,Era.Core/Types/CharacterFlagIndex.cs,Era.Core/Types/FlagIndex.cs,Era.Core/Types/GrowthStage.cs,Era.Core/Types/CharacterConstants.cs,Era.Core.Tests/) | not_matches | `TODO|FIXME|HACK` | [x] |
| 16 | Multi-birth callback invocation verified in tests (count + argument) | code | Grep(Era.Core.Tests/) | matches | `(_multiBirthFatherCheck|multiBirthFatherCheck|MultiBirthFather).*(Verify|Times|Received|invoc).*(mother|Mother)` | [x] |
| 17 | ERB caller files preserve CALL statements for migrated functions | code | Grep(Game/ERB/EVENTTURNEND.ERB,Game/ERB/PREGNACY_S.ERB,Game/ERB/追加パッチverup.ERB) | count_equals | `CALL 体(変化_１日経過|設定_遺伝|設定_子供Ｐ成長|設定_子供髪変更)` = 7 | [x] |
| 18 | BodyChangeDaily test covers increment and clamp behavior (both paths) | code | Grep(Era.Core.Tests/) | count_equals | `(BodyChangeDaily|body_change_daily|DailyBodyChange).*(HairLength|Clamp|Increment|clamp|increment)` = 2 | [x] |
| 19 | Slot distribution test covers per-slot asymmetric modulo values per C10 | code | Grep(Era.Core.Tests/**/BodySettings*Tests.cs) | count_equals | `Next\((100|120|140|160)\)` = 4 | [x] |
| 20 | VisitorCharacterIndex constant defined with correct value 998 | code | Grep(Era.Core/Types/CharacterConstants.cs) | matches | `VisitorCharacterIndex\s*=\s*998` | [x] |
| 21 | Tidy() invocation verified in genetics test | code | Grep(Era.Core.Tests/) | matches | `(Received|Verify|Times)\(.*\)\.(Tidy|tidy)` | [x] |
| 22 | Twin copy test verifies all 24 CFLAG fields from sibling | code | Grep(Era.Core.Tests/) | matches | `(Assert|Equal|Should).*(twin|Twin|sibling|Sibling|CFLAG).*24` | [x] |
| 23 | Both eye options independently cleared when value is 5 | code | Grep(Era.Core.Tests/) | count_equals | `(EyeOption|eyeOption|瞳オプション).*(5|cleared|Clear)` = 2 | [x] |
| 24 | IVisitorVariables accessor methods invoked in visitor override test | code | Grep(Era.Core.Tests/) | matches | `(_visitor|visitor|IVisitorVariables).*(Received|Verify|Times|verify)` | [x] |
| 25 | Skin color retry loop has 20-iteration cap | code | Grep(Era.Core/State/BodySettings.cs) | matches | `(while|for|While|For).*20.*ValidSkinColors|(ValidSkinColors|skinColor|SkinColor).*20` | [x] |
| 26 | Callback not invoked for non-ELSE paths (single birth + identical twin) | code | Grep(Era.Core.Tests/) | count_equals | `(_multiBirthFatherCheck|MultiBirthFather).*(DidNotReceive|Received\(0\)|Times\(0\)|Never)` = 2 | [x] |
| 27 | ChildHairChange dual-path test coverage (RAND:4 gate + hair option update) | code | Grep(Era.Core.Tests/) | count_equals | `(ChildHairChange|child_hair_change|HairChange).*(200|Option|option|髪オプション|collision|Next\(4\))` = 2 | [x] |
| 28 | Slot distribution loop has 20-iteration cap | code | Grep(Era.Core/State/BodySettings.cs) | matches | `(while|for|While|For).*20.*(slot|Slot|SLOT)|(slot|Slot|SLOT).*20` | [x] |
| 29 | Slot modulo values referenced in implementation code | code | Grep(Era.Core/State/BodySettings.cs) | matches | `(Next|modulo|Modulo|SlotModul).*(100|120|140|160)` | [x] |
| 30 | P size store and -2 reset test assertions exist (both paths) | code | Grep(Era.Core.Tests/) | count_equals | `(Assert|Equal|Should|SetCFlag|Received).*(ChildPSizeStore|PSize).*-2|(ChildPSizeStore|PSize).*(Assert|Equal|Should|SetCFlag|Received).*-2` = 2 | [x] |
| 31 | IEngineVariables.GetCharacterNo invoked in BOTH genetics and hair change tests | code | Grep(Era.Core.Tests/) | count_equals | `(_engine|engine|IEngineVariables).*(Received|Verify|Times|verify).*GetCharacterNo` = 2 | [x] |
| 32 | Slot-to-modulo mapping defined in correct order (100,120,140,160) | code | Grep(Era.Core/State/BodySettings.cs) | matches | `100.*120.*140.*160` | [x] |
| 33 | Mutation path test verifies RAND:15 check exists | code | Grep(Era.Core.Tests/**/BodySettings*Tests.cs) | matches | `Next\(15\)` | [x] |
| 34 | CharacterConstants.VisitorCharacterIndex referenced in BodySettings implementation | code | Grep(Era.Core/State/BodySettings.cs) | matches | `CharacterConstants\.VisitorCharacterIndex` | [x] |
| 35 | ERB wrapper function definitions preserved in body_settings.ERB | code | Grep(Game/ERB/体設定.ERB) | count_equals | `@体変化_１日経過|@体設定_遺伝|@体設定_子供Ｐ成長|@体設定_子供髪変更` = 4 | [x] |
| 36 | ChildPGrowth growth stage branching test coverage | code | Grep(Era.Core.Tests/) | matches | `(ChildPGrowth|child_p_growth).*(Infant|GrowthPeriod|Adult)` | [x] |
| 37 | Visitor override negative path: IVisitorVariables NOT called when bit 3 unset | code | Grep(Era.Core.Tests/) | matches | `(_visitor|IVisitorVariables).*(DidNotReceive|Received\(0\)|Times\(0\)|Never)` | [x] |
| 38 | Callback RESULT < 0 path: father ID unchanged when callback returns negative | code | Grep(Era.Core.Tests/) | matches | `(_multiBirthFatherCheck|MultiBirthFather).*(Returns\(-1\)|minus1|-1.*father|NoChange)` | [x] |
| 39 | FlagIndex.MiscSettings referenced in BodySettings implementation (not hardcoded 6441) | code | Grep(Era.Core/State/BodySettings.cs) | matches | `FlagIndex\.MiscSettings` | [x] |
| 40 | DI registration uses explicit BodySettings construction (not auto-resolved) | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `new BodySettings\(` | [x] |
| 41 | IRandomProvider actually used in BodySettings implementation (not just stored) | code | Grep(Era.Core/State/BodySettings.cs) | matches | `_random\.Next` | [x] |
| 42 | DI registration references actual multi-birth father check bridge (not no-op lambda) | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `多生児父親判定|MultiBirthFather|multiBirthFather` | [x] |
| 43 | SkinColor and HairColor derivation values verified in genetics test | code | Grep(Era.Core.Tests/) | matches | `(SkinColor|HairColor).*(Assert|Equal|Should)|(Assert|Equal|Should).*(SkinColor|HairColor)` | [x] |
| 44 | Callback positive result stores returned fatherId to child CFLAG:Father | code | Grep(Era.Core.Tests/) | matches | `Received.*SetCFlag.*Father` | [x] |
| 45 | Visitor override test configures specific return values for IVisitorVariables properties | code | Grep(Era.Core.Tests/) | matches | `_visitor\.\w+.*Returns\(\d+\)` | [x] |
| 46 | Body option trait CFLAG assignments verified in slot distribution test | code | Grep(Era.Core.Tests/) | matches | `(BodyOption|bodyOption).*(Received|Assert|Equal|SetCFlag)|(Received|Assert|Equal|SetCFlag).*(BodyOption|bodyOption)` | [x] |
| 47 | IEngineVariables actually used in BodySettings implementation (not just stored) | code | Grep(Era.Core/State/BodySettings.cs) | matches | `_engine\.GetCharacterNo` | [x] |
| 48 | IVisitorVariables actually used in BodySettings implementation (not just stored) | code | Grep(Era.Core/State/BodySettings.cs) | matches | `_visitor\.` | [x] |
| 49 | ChildPGrowth PSize modification verified in all 3 growth stage paths | code | Grep(Era.Core.Tests/) | count_equals | `(ChildPGrowth|child_p_growth).*(SetCFlag|Assert|Equal).*(PSize|pSize)|(PSize|pSize).*(SetCFlag|Assert|Equal).*(ChildPGrowth|child_p_growth)` = 3 | [x] |
| 50 | Twin copy test references at least 5 distinct CFLAG field names with twin/sibling context | code | Grep(Era.Core.Tests/) | gte | `(Father|SkinColor|EyeOption|BodyOption|HairLength).*(twin|Twin|sibling|Sibling)|(twin|Twin|sibling|Sibling).*(Father|SkinColor|EyeOption|BodyOption|HairLength)` >= 5 | [x] |

### AC Details

**AC#1: IBodySettings declares BodyChangeDaily**
- **Test**: Grep pattern=`void BodyChangeDaily\(int` path=Era.Core/Interfaces/IBodySettings.cs
- **Expected**: `Pattern found (method signature for daily body change function)`
- **Rationale**: Goal item (1) requires 4 new method signatures on IBodySettings. This verifies the @body_change_daily migration entry point. Constraint C13 requires 4 new methods.

**AC#2: IBodySettings declares BodySettingsGenetics**
- **Test**: Grep pattern=`void BodySettingsGenetics\(int` path=Era.Core/Interfaces/IBodySettings.cs
- **Expected**: `Pattern found (method signature for genetics function)`
- **Rationale**: Goal item (1) requires the genetics function signature. This is the most complex of the 4 functions (body_settings.ERB:944-1339). Constraint C13.

**AC#3: IBodySettings declares ChildPGrowth and ChildHairChange**
- **Test**: Grep pattern=`void (BodySettingsChildPGrowth|BodySettingsChildHairChange)\(int` path=Era.Core/Interfaces/IBodySettings.cs | count
- **Expected**: `2 matches (both child-related method signatures present)`
- **Rationale**: Goal item (1) requires all 4 methods. Constraint C12 requires these methods be independently callable (verup_patch calls genetics + child_hair_change but NOT child_p_growth).

**AC#4: IBodySettings existing methods preserved**
- **Test**: Grep pattern=`(BodyDetailInit|Tidy|ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize|GetTightnessBaseValue)\(` path=Era.Core/Interfaces/IBodySettings.cs | count
- **Expected**: `7 matches (all pre-existing methods from F778+F779 remain intact, matching both void and int return types)`
- **Rationale**: Interface extension must not break backward compatibility (ENGINE.md Issue 63). The existing 7 methods must be preserved.

**AC#5: CharacterFlagIndex defines Father, GrowthStage, ChildPSizeStore**
- **Test**: Grep pattern=`public static readonly CharacterFlagIndex (Father|GrowthStage|ChildPSizeStore)` path=Era.Core/Types/CharacterFlagIndex.cs | count
- **Expected**: `3 matches`
- **Rationale**: Goal item (3) requires missing CFLAG indices. Father=73 for father determination (C2), GrowthStage=75 for growth branching (C15), ChildPSizeStore for P size storage (C5).

**AC#6: FlagIndex defines MiscSettings constant with value 6441**
- **Test**: Grep pattern=`MiscSettings = new\(6441\)` path=Era.Core/Types/FlagIndex.cs
- **Expected**: `Pattern found`
- **Rationale**: Goal item (3) requires FlagIndex.MiscSettings=6441 for the GETBIT(FLAG:6441, 3) visitor body settings check. Constraint C9.

**AC#7: BodySettings has IRandomProvider as private readonly field**
- **Test**: Grep pattern=`private readonly IRandomProvider` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (private readonly field confirming constructor injection, not static usage)`
- **Rationale**: Goal item (2) requires IRandomProvider injection for RAND-based genetics logic. Constraint C1 requires seeded random for deterministic tests. Matcher strengthened from bare `IRandomProvider` to `private readonly IRandomProvider` to verify constructor-injected DI pattern.

**AC#8: BodySettings has IVisitorVariables as private readonly field**
- **Test**: Grep pattern=`private readonly IVisitorVariables` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (private readonly field confirming constructor injection)`
- **Rationale**: Goal item (2) requires IVisitorVariables injection for visitor trait override access. Constraint C9. Matcher strengthened to verify DI pattern.

**AC#9: BodySettings has IEngineVariables as private readonly field**
- **Test**: Grep pattern=`private readonly IEngineVariables` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (private readonly field confirming constructor injection)`
- **Rationale**: Goal item (2) requires IEngineVariables injection for GetCharacterNo() calls in child gender check. Constraint C7. Matcher strengthened to verify DI pattern.

**AC#10: BodySettings stores callback delegate as private readonly field**
- **Test**: `Grep pattern=private readonly Func<int,\s*int> path=Era.Core/State/BodySettings.cs`
- **Expected**: `Pattern found (private readonly Func<int,int> field confirming constructor-injected DI for mother_id -> father_id callback)`
- **Rationale**: Goal item (4) requires Func<int,int> callback delegate for the external multi_birth_father_check function. Constraint C2 specifies this must be a mock-verifiable callback, not internal logic. Matcher strengthened from bare `Func<int,\s*int>` to `private readonly Func<int,\s*int>` to verify constructor-injected DI pattern, consistent with AC#7/AC#8/AC#9 strengthening (restart4-iter8). Previous matcher could match the constructor parameter declaration alone.

**AC#11: Skin color validation with valid set {0,1,2,3,10,11,13,14}**
- **Test**: Grep pattern=`ValidSkinColors.*0.*1.*2.*3.*10.*11.*13.*14` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (HashSet initializer containing all 8 valid skin color values in order)`
- **Rationale**: Goal item (5) requires replicating the skin color validation from body_settings.ERB:1082-1086. Constraint C14 specifies valid set {0,1,2,3,10,11,13,14} with 20-iteration cap on retry loop. Previous matcher only checked for identifier name `ValidSkinColors` without verifying the actual values; strengthened to require all 8 values appear on the same line as the identifier, preventing an implementation with wrong values from passing.

**AC#12: Growth stage constants defined**
- **Test**: Grep pattern=`None\s*=\s*0|Nursing\s*=\s*1|Infant\s*=\s*2|GrowthPeriod\s*=\s*3|Adult\s*=\s*4` path=Era.Core/Types/GrowthStage.cs | count
- **Expected**: `5 matches (all 5 growth stage constants: None=0, Nursing=1, Infant=2, GrowthPeriod=3, Adult=4)`
- **Rationale**: Constraint C15 requires C# equivalents for ERB growth stages. F780 uses Infant/GrowthPeriod/Adult directly, but None=0 (成長度_なし) and Nursing=1 (成長度_乳児) are used by 10+ other ERB files (MOVEMENT, PREGNACY_S, RESTROOM, etc.) that will migrate in future phases. Defining all 5 upfront follows Zero Debt Upfront principle.

**AC#13: Build succeeds with zero warnings**
- **Test**: dotnet build Era.Core/ (via WSL)
- **Expected**: `Build succeeds (exit code 0)`
- **Rationale**: TreatWarningsAsErrors in Directory.Build.props (Technical Constraint). All new code must compile cleanly.

**AC#14: Unit tests pass**
- **Test**: dotnet test Era.Core.Tests/ --filter BodySettings (via WSL)
- **Expected**: `All tests pass (exit code 0)`
- **Rationale**: TDD RED->GREEN cycle. Equivalence tests with seeded IRandomProvider verify behavioral parity with legacy ERB functions. Covers constraints C1 (seeded random), C3 (Tidy invocation), C4 (twin copy all 24 CFLAG fields from sibling), C5 (P size store/reset), C6 (both eye options 瞳OP1 and 瞳OP2 independently cleared when value is 5), C7 (gender hair), C9 (visitor override), C10 (asymmetric modulo), C11 (mutation paths), C14 (skin color validation). Note: Current unit test approach verifies behavioral correctness per-path with mocked dependencies. Full aggregate output equivalence (identical CFLAG state given identical inputs) is verified by the combination of all 43 ACs covering every branching path and edge case.

**AC#15: Zero technical debt in new code**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths=Era.Core/State/BodySettings.cs, Era.Core/Interfaces/IBodySettings.cs, Era.Core/Types/CharacterFlagIndex.cs, Era.Core/Types/FlagIndex.cs, Era.Core/Types/GrowthStage.cs, Era.Core/Types/CharacterConstants.cs, Era.Core.Tests/ (includes test files)
- **Expected**: `0 matches`
- **Rationale**: Constraint C8 (Phase 20 sub-feature requires zero tech debt per architecture.md:4629-4637). Scope includes test files created by Task#5 to prevent TODO stubs in test code. Constraint referenced: C8.

**AC#16: Multi-birth callback invocation verified in tests (count + argument) for RAND:3 == 0 path**
- **Test**: Grep pattern=`(_multiBirthFatherCheck|multiBirthFatherCheck|MultiBirthFather).*(Verify|Times|Received|invoc).*(mother|Mother)` path=Era.Core.Tests/
- **Expected**: `Pattern found (test verifies mock callback invocation count AND motherId argument under multi-birth non-identical twin path where RAND:3 == 0)`
- **Rationale**: Constraint C2 explicitly requires "Verify callback invocation with mock; do not test internal logic." AC#10 only verifies the Func<int,int> field exists in BodySettings.cs — a static declaration check. AC#16 ensures the callback is actually invoked with the correct argument (motherId, not childId or other values) and correct invocation count when multiBirthFlag >= 1 and RAND:3 == 0 (non-identical twin path, ELSE branch of ERB line 955 `IF ARG:1 && RAND:3`). The matcher requires both verification keywords and motherId reference on the same line, preventing a test that verifies invocation but with the wrong argument.

**AC#17: ERB caller files preserve CALL statements for migrated functions**
- **Test**: Grep pattern=`CALL 体(変化_１日経過|設定_遺伝|設定_子供Ｐ成長|設定_子供髪変更)` paths=Game/ERB/EVENTTURNEND.ERB, Game/ERB/PREGNACY_S.ERB, Game/ERB/追加パッチverup.ERB | count
- **Expected**: `7 matches (EVENTTURNEND:1 体変化_１日経過, PREGNACY_S:3 体設定_遺伝+体設定_子供髪変更+体設定_子供Ｐ成長, 追加パッチverup:3 体設定_遺伝×2+体設定_子供髪変更)`
- **Rationale**: Goal item 6 requires "Maintain ERB wrapper functions for external callers." Technical Design states "ERB wrapper functions...are preserved as-is; no ERB changes are required." AC#17 guards against accidental deletion of ERB CALL statements during implementation.

**AC#18: BodyChangeDaily test covers increment and clamp behavior (both paths)**
- **Test**: Grep pattern=`(BodyChangeDaily|body_change_daily|DailyBodyChange).*(HairLength|Clamp|Increment|clamp|increment)` path=Era.Core.Tests/ | count
- **Expected**: `2 matches (one for normal increment path below threshold, one for clamp path at/above threshold)`
- **Rationale**: Constraint C16 requires testing BodyChangeDaily behavior (HairLength += 10, clamp at HairLengthCategory * 100). Goal item 1 requires migrating @body_change_daily but AC#1 only verifies the method signature. AC#18 ensures behavioral test coverage exists for BOTH the increment and clamp paths. Without count_equals=2, a single test covering only the increment path (below threshold) would pass the matcher. The two-path pattern is consistent with AC#26 (count_equals=2 for callback negative guard), AC#30 (count_equals=2 for P size store/reset), and AC#23 (count_equals=2 for both eye options).
- **Implementation Guidance**: The 2 matches should come from separate test methods (e.g., `BodyChangeDaily_BelowThreshold_IncrementsHairLength` and `BodyChangeDaily_AtThreshold_ClampsHairLength`). Note: grep count_equals cannot enforce "separate methods" — this is guidance for implementers.

**AC#19: Slot distribution test covers per-slot asymmetric modulo values per C10**
- **Test**: Grep pattern=`Next\((100|120|140|160)\)` path=Era.Core.Tests/**/BodySettings*Tests.cs | count
- **Expected**: `4 matches (each of the 4 asymmetric modulo values 100, 120, 140, 160 appears as a distinct Next(N) call in tests)`
- **Rationale**: Constraint C10 requires verifying the asymmetric RAND distribution (100/120/140/160 per slot) is preserved. Previous matcher `(100|120|140|160).*(slot|Slot|option|Option|RAND|Next|modulo)` only verified co-occurrence and could not distinguish per-slot correctness. Strengthened to require 4 distinct `Next(N)` calls — one for each modulo value — ensuring all 4 boundary values are explicitly tested. The 16-slot body option distribution algorithm (body_settings.ERB:1098-1219) uses increasing modulo values for slots 6-9. Grep scope narrowed to BodySettings*Tests.cs to prevent false positives from `Next(100)` in unrelated test files (e.g., EngineVariablesDelegationTests.cs).

**AC#20: VisitorCharacterIndex constant defined with correct value 998**
- **Test**: Grep pattern=`VisitorCharacterIndex\s*=\s*998` path=Era.Core/Types/CharacterConstants.cs
- **Expected**: `Pattern found (public const defining visitor character index as 998 in shared type)`
- **Rationale**: The visitor override path in BodySettingsGenetics uses character index 998 (人物_訪問者 from DIM.ERH:71) to identify the visitor character for trait source lookup. If this constant is wrong, the visitor override silently applies to the wrong character. AC#8 verifies IVisitorVariables is injected but not the character index used for visitor identification. AC#20 ensures the correct value is hard-coded as a named constant.

**AC#21: Tidy() invocation verified in genetics test**
- **Test**: Grep pattern=`(Received|Verify|Times)\(.*\)\.(Tidy|tidy)` path=Era.Core.Tests/
- **Expected**: `Pattern found (NSubstitute-style mock verification: _mock.Received(N).Tidy(id))`
- **Rationale**: Constraint C3 requires "Verify Tidy() is invoked at end of genetics method." AC#14 only checks aggregate test pass/fail. AC#21 ensures a mock verification assertion exists for Tidy() invocation. Previous matcher required `genetics` keyword on the same assertion line, which NSubstitute syntax does not produce (the test method name is on a separate line). Simplified to match `Received(N).Tidy(...)` pattern which is the idiomatic NSubstitute mock verification call.

**AC#22: Twin copy test verifies all 24 CFLAG fields from sibling**
- **Test**: Grep pattern=`(Assert|Equal|Should).*(twin|Twin|sibling|Sibling|CFLAG).*24` path=Era.Core.Tests/
- **Expected**: `Pattern found (assertion line referencing twin/sibling/CFLAG copy with explicit 24-count reference)`
- **Rationale**: Constraint C4 requires "Verify all 24 fields match sibling's values after twin copy." Matcher requires the literal `24` to appear on the assertion line alongside twin/sibling/CFLAG keywords, ensuring the test explicitly references the full 24-field count. Previous matcher allowed `Father|GrowthStage` alternatives which could match a partial implementation testing only 2 of 24 fields.
- **Design Constraint**: Test MUST include `24` in an assertion message or inline comment on the same line as the Assert/Equal/Should call (e.g., `Assert.Equal(24, copiedFieldCount)` or `// Verify all 24 CFLAG fields copied from sibling`). This ensures the grep matcher can verify field-count completeness.

**AC#23: Both eye options independently cleared when value is 5**
- **Test**: Grep pattern=`(EyeOption|eyeOption|瞳オプション).*(5|cleared|Clear)` path=Era.Core.Tests/ | count
- **Expected**: `2 matches (one test assertion per eye option: 瞳オプション１ and 瞳オプション２)`
- **Rationale**: Constraint C6 requires "Verify BOTH eye options independently cleared to 0 when their value is 5." AC#14 cannot distinguish whether the test checks both eye options independently or only one. AC#23 requires 2 distinct matches, ensuring both 瞳OP1 and 瞳OP2 have separate test coverage.

**AC#24: IVisitorVariables accessor methods invoked in visitor override test**
- **Test**: Grep pattern=`(_visitor|visitor|IVisitorVariables).*(Received|Verify|Times|verify)` path=Era.Core.Tests/
- **Expected**: `Pattern found (test verifies IVisitorVariables mock methods are actually called during visitor override path)`
- **Rationale**: AC#8 only checks IVisitorVariables field exists in BodySettings.cs (structural). AC#20 checks the VisitorCharacterIndex constant value. Neither verifies that the visitor trait VALUES are actually sourced from IVisitorVariables interface methods during the visitor override path. AC#24 ensures behavioral wiring — that the mock IVisitorVariables receives method calls — preventing a bypass where traits could be read from IVariableStore globals instead.

**AC#25: Skin color retry loop has 20-iteration cap**
- **Test**: Grep pattern=`(while|for|While|For).*20.*ValidSkinColors|(ValidSkinColors|skinColor|SkinColor).*20` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (loop construct with 20-iteration cap near skin color validation logic)`
- **Rationale**: Constraint C14 specifies "20-iteration cap on retry loop" for skin color validation. AC#11 only verifies the valid set constant exists. An infinite loop implementation would pass AC#11 and AC#14 if all test scenarios use valid skin colors. AC#25 verifies the 20-iteration cap is actually coded, matching ERB's `WHILE 20` construct.

**AC#26: Callback not invoked for non-ELSE paths (single birth + identical twin)**
- **Test**: Grep pattern=`(_multiBirthFatherCheck|MultiBirthFather).*(DidNotReceive|Received\(0\)|Times\(0\)|Never)` path=Era.Core.Tests/ | count
- **Expected**: `2 matches (two distinct negative guard tests)`
- **Rationale**: ERB lines 954-1003 show: (1) IF ARG:1 && RAND:3 → identical twin path, callback NOT called, (2) ELSE with multiBirthFlag==0 → single birth, callback NOT called. Only the ELSE branch with multiBirthFlag>=1 AND RAND:3==0 (non-identical twin) calls the callback. AC#16 covers the positive path. AC#26 requires 2 negative guard tests: one for multiBirthFlag==0 (single birth) and one for multiBirthFlag>=1 with RAND:3!=0 (identical twin). An implementation that calls the callback unconditionally when multiBirthFlag>=1 would pass AC#16 but fail AC#26.

**AC#27: ChildHairChange dual-path test coverage (RAND:4 gate + hair option update)**
- **Test**: `Grep pattern=(ChildHairChange|child_hair_change|HairChange).*(200|Option|option|髪オプション|collision|Next\(4\)) path=Era.Core.Tests/ | count`
- **Expected**: `2 matches (one for RAND:4==0 positive path with hair option update, one for RAND:4!=0 negative path where hair change is skipped)`
- **Rationale**: ERB lines 1367-1425 contain a RAND:4 gate (1/4 chance, ERB:1369) creating 2 distinct paths: (a) RAND:4==0 — hair change proceeds with HairLength>=200 threshold, option collision avoidance, dual RAND:5, and clear-when-<200; (b) RAND:4!=0 — hair change skipped entirely. Previous matcher was `matches` (single occurrence), inconsistent with the dual-path AC pattern (AC#18 count_equals=2, AC#26 count_equals=2, AC#30 count_equals=2). Changed to count_equals=2 to enforce both positive and negative RAND:4 gate path coverage.
- **Implementation Guidance**: The 2 matches should come from separate test methods (e.g., `ChildHairChange_RandGateTriggered_UpdatesHairOption` and `ChildHairChange_RandGateNotTriggered_NoChange`). Note: grep count_equals cannot enforce "separate methods" — this is guidance for implementers.

**AC#28: Slot distribution loop has 20-iteration cap**
- **Test**: Grep pattern=`(while|for|While|For).*20.*(slot|Slot|SLOT)|(slot|Slot|SLOT).*20` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (loop construct with 20-iteration cap near slot distribution logic)`
- **Rationale**: ERB lines 1021-1029 contain a WHILE 20 loop for the slot distribution algorithm (randomly distributing 8 body option slots across 16 positions). This is DISTINCT from the skin color retry WHILE 20 (AC#25). An unbounded loop implementation passes all current ACs if seeded random completes within attempts. AC#28 ensures the 20-iteration cap is coded, matching ERB's WHILE 20 construct.

**AC#29: Slot modulo values referenced in implementation code**
- **Test**: Grep pattern=`(Next|modulo|Modulo|SlotModul).*(100|120|140|160)` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (at least one modulo value referenced in implementation near Next() call or as named constant/array)`
- **Rationale**: ERB body_settings.ERB:1098-1219 uses RAND:100/120/140/160 for slots 6-9. AC#29 verifies implementation code references modulo values in a recognizable pattern. Completeness and ordering of all 4 values are enforced by AC#32 (`100.*120.*140.*160` on a single line). AC#29 was changed from count_equals=4 to matches to resolve conflict with AC#32: a single-line array definition `{ 100, 120, 140, 160 }` produces 1 grep match (not 4), satisfying AC#32 but failing count_equals=4.

**AC#30: P size store and -2 reset test assertions exist (both paths)**
- **Test**: Grep pattern=`(Assert|Equal|Should|SetCFlag|Received).*(ChildPSizeStore|PSize).*-2|(ChildPSizeStore|PSize).*(Assert|Equal|Should|SetCFlag|Received).*-2` path=Era.Core.Tests/ | count
- **Expected**: `2 matches (one for twin path PSize=-2 at ERB:979, one for non-twin path PSize=-2 at ERB:1319; each must co-occur with assertion/mock keyword)`
- **Rationale**: C5 requires "ChildPSizeStore = PSize, then PSize = -2." The PSize=-2 reset occurs in BOTH the twin path (ERB line 979, inside IF ARG:1 && RAND:3 block) and the non-twin path (ERB line 1319). AC#22 covers twin copy field count but not the -2 reset specifically. AC#30 requires 2 matches to ensure both the twin-path and non-twin-path PSize=-2 resets are independently tested.
- **Implementation Guidance**: The 2 matches should ideally come from separate test methods (one for twin-path, one for non-twin-path). Test methods should be named to distinguish the twin/non-twin paths (e.g., `TwinPath_StoresPSizeAndResets` and `NonTwinPath_StoresPSizeAndResets`). Note: grep count_equals cannot enforce "separate methods" — this is guidance for implementers, not a machine-verifiable constraint.

**AC#31: IEngineVariables.GetCharacterNo invoked in BOTH genetics and hair change tests**
- **Test**: `Grep pattern=(_engine|engine|IEngineVariables).*(Received|Verify|Times|verify).*GetCharacterNo path=Era.Core.Tests/ | count`
- **Expected**: `2 matches (one for BodySettingsGenetics gender-based hair length category at ERB:1329, one for BodySettingsChildHairChange gender-based randomization at ERB:1371)`
- **Rationale**: AC#9 checks IEngineVariables field exists structurally. ERB uses `NO:子==148` for gender determination in TWO methods: BodySettingsGenetics (ERB:1329, hair length category assignment) and BodySettingsChildHairChange (ERB:1371, 1/4 chance hair randomization). Previous matcher was `matches` (single occurrence), which could be satisfied solely by a ChildHairChange test, leaving BodySettingsGenetics gender branching uncovered. Changed to count_equals=2 to verify GetCharacterNo mock verification exists in BOTH method test contexts.

**AC#32: Slot-to-modulo mapping defined in correct order (100,120,140,160)**
- **Test**: Grep pattern=`100.*120.*140.*160` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (all 4 modulo values appearing in ascending order on a single line or array definition)`
- **Rationale**: AC#19 verifies 4 modulo values appear in tests (count_equals=4) and AC#29 verifies they appear in implementation code, but neither enforces the correct ordering (slot 6→100, slot 7→120, slot 8→140, slot 9→160). An implementation with (100,100,100,160) could pass both. AC#32 requires all 4 values to appear in the correct sequential order on a single line, structurally enforcing the slot-to-modulo mapping. This is achievable via array initialization or sequential constant definitions.

**AC#33: Mutation path test verifies RAND:15 check exists**
- **Test**: Grep pattern=`Next\(15\)` path=Era.Core.Tests/**/BodySettings*Tests.cs
- **Expected**: `Pattern found (test invokes _random.Next(15) for the 1/15 mutation chance check)`
- **Rationale**: Constraint C11 requires testing both mutation (RAND:15==0) and non-mutation paths. AC#14 lists C11 coverage in its rationale but is an aggregate pass/fail check. AC#33 ensures a structural grep verifies the mutation check is present in tests, analogous to AC#18 (BodyChangeDaily), AC#19 (slot modulo), AC#27 (ChildHairChange). Grep scope narrowed to BodySettings*Tests.cs to prevent false positives from unrelated test files calling Next(15), consistent with AC#19's scope restriction.

**AC#34: CharacterConstants.VisitorCharacterIndex referenced in BodySettings implementation**
- **Test**: Grep pattern=`CharacterConstants\.VisitorCharacterIndex` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (named constant used, not hardcoded magic number 998)`
- **Rationale**: AC#20 verifies the constant is defined with correct value 998. AC#34 closes the gap between "constant defined" and "constant used" — preventing an implementation that hardcodes 998 directly while the named constant goes unused.

**AC#35: ERB wrapper function definitions preserved in body_settings.ERB**
- **Test**: Grep pattern=`@体変化_１日経過|@体設定_遺伝|@体設定_子供Ｐ成長|@体設定_子供髪変更` path=Game/ERB/体設定.ERB | count
- **Expected**: `4 matches (all 4 wrapper function definitions exist)`
- **Rationale**: AC#17 verifies external callers (EVENTTURNEND, PREGNACY_S, verup_patch) preserve CALL statements to the migrated functions. But AC#17 does not verify body_settings.ERB itself still contains the 4 wrapper function definitions that these external callers invoke. AC#35 guards against accidental deletion of the function definitions in 体設定.ERB. Technical Design states "ERB wrapper functions in body_settings.ERB...are preserved as-is."

**AC#36: ChildPGrowth growth stage branching test coverage**
- **Test**: Grep pattern=`(ChildPGrowth|child_p_growth).*(Infant|GrowthPeriod|Adult)` path=Era.Core.Tests/
- **Expected**: `Pattern found (test verifies growth stage branching logic for ChildPGrowth)`
- **Rationale**: body_settings.ERB:1342-1365 implements 3-way branching on GrowthStage (Infant: PSize--, GrowthPeriod: PSize++, Adult: PSize=ChildPSizeStore). None of AC#18-AC#33 verifies ChildPGrowth branching. AC#14 is aggregate pass/fail. AC#36 ensures a structural grep verifies the growth stage branching test exists, analogous to AC#18 (BodyChangeDaily), AC#27 (ChildHairChange), AC#33 (mutation path).

**AC#37: Visitor override negative path: IVisitorVariables NOT called when bit 3 unset**
- **Test**: Grep pattern=`(_visitor|IVisitorVariables).*(DidNotReceive|Received\(0\)|Times\(0\)|Never)` path=Era.Core.Tests/
- **Expected**: `Pattern found (test asserts IVisitorVariables is NOT called when GETBIT(FLAG:MiscSettings, 3) is unset)`
- **Rationale**: C9 requires "Test both paths: bit 3 set (use visitor traits) and unset (skip)." AC#24 covers the positive path (IVisitorVariables IS called when bit 3 set). AC#37 covers the negative path (IVisitorVariables NOT called when bit 3 unset). Without AC#37, an implementation that calls IVisitorVariables unconditionally (ignoring the GETBIT check) passes all existing ACs. This mirrors AC#26's pattern (callback negative guard for 2 non-ELSE paths).

**AC#38: Callback RESULT < 0 path — father ID unchanged when callback returns negative**
- **Test**: Grep pattern=`(_multiBirthFatherCheck|MultiBirthFather).*(Returns\(-1\)|minus1|-1.*father|NoChange)` path=Era.Core.Tests/
- **Expected**: `Pattern found (test sets up callback returning -1 and verifies father ID is not updated)`
- **Rationale**: ERB body_settings.ERB:998-1003 shows `RESULT = multi_birth_father_check(母)` followed by `IF RESULT >= 0` guard before assigning father. The RESULT < 0 path means "no father change." AC#16 covers positive invocation (callback called with motherId), AC#26 covers non-invocation (callback not called in single-birth/identical-twin). Neither covers the case where callback IS called but returns -1, meaning father should remain unchanged. An implementation that ignores the RESULT < 0 guard and always assigns the return value would pass AC#16 and AC#26.

**AC#39: FlagIndex.MiscSettings referenced in BodySettings implementation**
- **Test**: Grep pattern=`FlagIndex\.MiscSettings` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (named constant used for GETBIT check, not hardcoded 6441)`
- **Rationale**: AC#6 verifies MiscSettings=6441 is defined in FlagIndex.cs. AC#39 closes the "defined vs used" gap — preventing an implementation that hardcodes `(GetFlag(6441) >> 3) & 1` while the named constant goes unused. Follows the AC#20/AC#34 pattern (AC#20=VisitorCharacterIndex defined, AC#34=VisitorCharacterIndex used).

**AC#40: DI registration uses explicit BodySettings construction**
- **Test**: Grep pattern=`new BodySettings\(` path=Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- **Expected**: `Pattern found (explicit construction with all 5 parameters, not auto-resolved AddSingleton<IBodySettings, BodySettings>())`
- **Rationale**: The current DI registration `services.AddSingleton<IBodySettings, BodySettings>()` relies on auto-resolution. After F780 adds Func<int,int> callback parameter (not DI-registered), auto-resolution will fail at runtime. AC#40 verifies the registration is changed to explicit construction, ensuring all 5 constructor parameters including the callback delegate are provided.

**AC#41: IRandomProvider actually used in BodySettings implementation**
- **Test**: Grep pattern=`_random\.Next` path=Era.Core/State/BodySettings.cs
- **Expected**: `Pattern found (_random.Next() called in implementation code for RAND-based logic)`
- **Rationale**: AC#7 verifies IRandomProvider is stored as a `private readonly` field (structural DI). But an implementation could store `_random` and internally use `new Random()` for actual RAND operations. AC#14 provides a behavioral safety net (seeded mock tests fail non-deterministically if `_random` is bypassed), but non-deterministic failure is unreliable. AC#41 provides deterministic structural verification that `_random.Next` is called, consistent with the "defined→used" AC pair pattern: AC#20/AC#34 (VisitorCharacterIndex defined→used), AC#6/AC#39 (MiscSettings defined→used), AC#7/AC#41 (IRandomProvider stored→used).

**AC#42: DI registration references actual multi-birth father check bridge**
- **Test**: Grep pattern=`多生児父親判定|MultiBirthFather|multiBirthFather` path=Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- **Expected**: `Pattern found (DI factory references the actual ERB bridge function for multi-birth father determination, not a no-op lambda)`
- **Rationale**: AC#10 verifies Func<int,int> field exists, AC#16/AC#26/AC#38 verify behavioral paths with mocks. But a no-op lambda `_ => -1` satisfies all four ACs. AC#42 closes the "wiring" gap by verifying the DI registration code references the actual ERB bridge function name. C2 says "do not test internal logic" — AC#42 does not test internal logic; it verifies structural wiring in the DI container. Follows the "defined→used" AC pair pattern: AC#10 (callback field defined) / AC#42 (callback wired to real function).

**AC#43: SkinColor and HairColor derivation values verified in genetics test**
- **Test**: Grep pattern=`(SkinColor|HairColor).*(Assert|Equal|Should)|(Assert|Equal|Should).*(SkinColor|HairColor)` path=Era.Core.Tests/
- **Expected**: `Pattern found (test asserts the derived SkinColor and/or HairColor CFLAG values after genetics execution)`
- **Rationale**: AC#11 verifies the valid skin color set constant exists, AC#25 verifies the retry loop cap, AC#19/AC#29/AC#32 verify slot distribution modulo values. None verify the actual derivation RESULT is stored to the character's CFLAG. A correct validation loop that discards its result (missing SetCFlag assignment) would pass all existing ACs. AC#43 ensures the test checks the final SkinColor/HairColor CFLAG values after genetics, catching missing assignment bugs.

**AC#44: Callback positive result stores returned fatherId to child CFLAG:Father**
- **Test**: `Grep pattern=Received.*SetCFlag.*Father path=Era.Core.Tests/`
- **Expected**: `Pattern found (NSubstitute mock verification of SetCFlag with CharacterFlagIndex.Father in callback positive path test)`
- **Rationale**: AC#16 verifies callback invocation with motherId argument. AC#38 verifies father ID unchanged when callback returns -1. Neither verifies the positive assignment path: callback returns >= 0 → CFLAG:Father set to returned value (ERB:999 `IF RESULT >= 0` → ERB:1000 `CFLAG:子:父親 = RESULT`). Task#5b describes this requirement but no AC verified it. An implementation that calls the callback but ignores the return value (always keeping original fatherId) would pass AC#16 and AC#38. AC#44 closes this gap by verifying the test contains mock verification of SetCFlag with Father.
- **Design Constraint**: The test MUST use distinct values for original fatherId (method parameter) and callback return value (e.g., original fatherId=1, callback Returns(99)). The `Received().SetCFlag(child, Father, 99)` assertion must reference the callback-returned value, not the original parameter. This disambiguates whether the implementation uses the callback result vs. the original parameter. Note: grep matcher `Received.*SetCFlag.*Father` cannot enforce the specific value — this Design Constraint ensures implementers structure the test for meaningful verification (analogous to AC#22's '24' Design Constraint).

**AC#45: Visitor override test configures specific return values for IVisitorVariables properties**
- **Test**: `Grep pattern=_visitor\.\w+.*Returns\(\d+\) path=Era.Core.Tests/`
- **Expected**: `Pattern found (NSubstitute mock setup: _visitor.PropertyName.Returns(N) configuring specific numeric return values for visitor trait properties)`
- **Rationale**: AC#24 verifies IVisitorVariables mock methods are called during visitor override path (behavioral wiring). AC#43 verifies SkinColor/HairColor derivation values are asserted. But neither verifies that visitor trait VALUES from IVisitorVariables are actually stored to child CFLAGs. An implementation that calls IVisitorVariables getters but discards returned values (applying genetics-derived values instead) would pass both AC#24 and AC#43. AC#45 verifies the test configures specific numeric return values for visitor properties, which is the foundation for value-based verification — combined with AC#24 (methods called) and AC#43 (derivation asserted), this triangulation ensures visitor values flow through to child CFLAGs.
- **Design Constraint**: The visitor override test MUST configure `_visitor.PropertyName.Returns(N)` with values that are DISTINCT from any genetics-derived default (e.g., `_visitor.SkinColor.Returns(14)` where genetics would produce a different value). The subsequent CFLAG assertion must verify the child's SkinColor equals the visitor-configured value (14), not a genetics-derived value. This ensures the grep pattern verifies meaningful value flow, not just mock setup (analogous to AC#44's Design Constraint for callback return values).

**AC#46: Body option trait CFLAG assignments verified in slot distribution test**
- **Test**: `Grep pattern=(BodyOption|bodyOption).*(Received|Assert|Equal|SetCFlag)|(Received|Assert|Equal|SetCFlag).*(BodyOption|bodyOption) path=Era.Core.Tests/`
- **Expected**: `Pattern found (test verifies that body option CFLAG values are assigned via SetCFlag after slot distribution algorithm execution)`
- **Rationale**: AC#19/AC#29/AC#32 verify the slot distribution modulo values (100/120/140/160) appear in tests/implementation. AC#43 covers SkinColor/HairColor derivation result assertion. But neither verifies that the 16-slot body option distribution actually stores results to body option CFLAGs (BodyOption1-4 at indices 515-518). An implementation that executes the distribution loop without calling SetCFlag for body options passes all existing ACs. AC#46 verifies the test contains assertions about BodyOption CFLAG values, closing the output-storage gap for the slot distribution algorithm.

**AC#47: IEngineVariables actually used in BodySettings implementation (not just stored)**
- **Test**: `Grep pattern=_engine\.GetCharacterNo path=Era.Core/State/BodySettings.cs`
- **Expected**: `Pattern found (_engine.GetCharacterNo() called in implementation code for gender determination)`
- **Rationale**: AC#9 verifies IEngineVariables is stored as `private readonly` field (structural DI). AC#31 verifies GetCharacterNo is mock-verified in tests. But neither verifies that `_engine.GetCharacterNo` is actually called in BodySettings.cs implementation code. An implementation could store `_engine` but hardcode `== 148` comparison instead. AC#47 closes the defined→used gap for IEngineVariables, consistent with the established pattern: AC#7/AC#41 (IRandomProvider stored→used), AC#20/AC#34 (VisitorCharacterIndex defined→used), AC#6/AC#39 (MiscSettings defined→used).

**AC#48: IVisitorVariables actually used in BodySettings implementation (not just stored)**
- **Test**: `Grep pattern=_visitor\. path=Era.Core/State/BodySettings.cs`
- **Expected**: `Pattern found (_visitor property accessed in implementation code for visitor trait override logic)`
- **Rationale**: AC#8 verifies IVisitorVariables is stored as a field. AC#24 verifies mock verification in tests. But neither verifies _visitor is actually called in the implementation. An implementation could store _visitor but use IVariableStore globals for visitor traits instead. AC#48 closes the defined→used gap consistent with AC#41 (_random.Next) and AC#47 (_engine.GetCharacterNo) pairs.

**AC#49: ChildPGrowth PSize modification verified in all 3 growth stage paths**
- **Test**: `Grep pattern=(ChildPGrowth|child_p_growth).*(SetCFlag|Assert|Equal).*(PSize|pSize)|(PSize|pSize).*(SetCFlag|Assert|Equal).*(ChildPGrowth|child_p_growth) path=Era.Core.Tests/ | count`
- **Expected**: `3 matches (one per growth stage path: Infant PSize--, GrowthPeriod PSize++, Adult PSize=ChildPSizeStore)`
- **Rationale**: AC#36 verifies growth stage branching test exists (Infant/GrowthPeriod/Adult labels) but does not verify the PSize result is stored via SetCFlag. AC#30 covers the -2 reset path but not the Infant (PSize--), GrowthPeriod (PSize++), Adult (PSize=ChildPSizeStore) modifications. count_equals=3 ensures all 3 branches have independent PSize assertions, consistent with AC#18 (count_equals=2) and AC#27 (count_equals=2) multi-path enforcement pattern.

**AC#50: Twin copy test references at least 5 distinct CFLAG field names with twin/sibling context**
- **Test**: `Grep pattern=(Father|SkinColor|EyeOption|BodyOption|HairLength).*(twin|Twin|sibling|Sibling)|(twin|Twin|sibling|Sibling).*(Father|SkinColor|EyeOption|BodyOption|HairLength) path=Era.Core.Tests/ | count`
- **Expected**: `>= 5 matches (at least 5 lines referencing distinct CFLAG field names with twin/sibling context keywords)`
- **Rationale**: AC#22 verifies the literal '24' appears in twin copy test but can be gamed by hardcoded count (Assert.Equal(24, 24)). AC#50 supplements AC#22 by requiring at least 5 of the 24 CFLAG field names (Father, SkinColor, EyeOption, BodyOption, HairLength) to co-occur with twin/sibling context keywords. Both OR alternatives require twin/sibling keywords, preventing false positives from non-twin tests that use the same CFLAG names with SetCFlag/GetCFlag. Combined with AC#22's Design Constraint, this ensures practical twin copy completeness.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extend IBodySettings with 4 new method signatures | AC#1, AC#2, AC#3 |
| 2 | Inject IRandomProvider, IVisitorVariables, IEngineVariables into BodySettings | AC#7, AC#8, AC#9, AC#20, AC#24, AC#31, AC#37, AC#41, AC#45, AC#47, AC#48 |
| 3 | Add missing CharacterFlagIndex and FlagIndex constants | AC#5, AC#6 |
| 4 | Implement Func<int,int> callback delegate for multi-birth father determination | AC#10, AC#16, AC#26, AC#38, AC#42, AC#44 |
| 5 | Replicate skin color validation logic (valid set {0,1,2,3,10,11,13,14}) | AC#11, AC#25, AC#43 |
| 6 | Maintain ERB wrapper functions for external callers | AC#17, AC#35 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Extend BodySettings.cs with three additional constructor-injected dependencies (IRandomProvider, IVisitorVariables, IEngineVariables) and one Func<int,int> callback field, then implement the four new methods directly on the class. This approach reuses the existing GetCFlag/SetCFlag helper pattern established in F779 (Tidy), avoids a separate GeneticsService class (constructor parameter count is 5 after addition — within the 5-param threshold identified in Risks), and preserves the dual-constructor backward-compatibility pattern until F796 resolves it.

The four methods map as follows:
- `BodyChangeDaily(int characterId)` — increments HairLength by 10, clamps at category threshold (body_settings.ERB:932-939)
- `BodySettingsGenetics(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId)` — twin copy path (ERB:955-993) or independent genetics path (ERB:994-1338) including 16-slot distribution, visitor override, skin color loop, eye option cleanup, gender-based hair length category, SkinColor/HairColor derivation, and Tidy() call. Note: `siblingId` is only used when `multiBirthFlag > 0` (twin copy path at ERB:955-993); when `multiBirthFlag == 0`, `siblingId` is not referenced (ELSE branch at ERB:994+). Callers may pass any value (e.g., 0) for `siblingId` in non-twin single-birth calls.
- `BodySettingsChildPGrowth(int childId)` — three-way branching on GrowthStage constant (Infant/GrowthPeriod/Adult) updating PSize toward ChildPSizeStore (ERB:1342-1365)
- `BodySettingsChildHairChange(int childId)` — 1/4 chance hair length category randomization by gender + hair option update when hair is long enough (ERB:1367-1426)

All dependencies are non-nullable constructor parameters (following F796's established pattern). No RequireXxx() guards are needed since every BodySettings instance has all dependencies available.

New CharacterFlagIndex constants (Father=73, GrowthStage=75, ChildPSizeStore=1193) are added to CharacterFlagIndex.cs following the existing `public static readonly CharacterFlagIndex X = new(N)` pattern. FlagIndex.MiscSettings=6441 is added to FlagIndex.cs as the first named constant (file currently has none). A `GrowthStage` static class or enum with Infant=2, GrowthPeriod=3, Adult=4 is defined in Era.Core/Types/GrowthStage.cs (new file) to satisfy C15 and AC#12.

The ValidSkinColors constant is defined as a `private static readonly HashSet<int>` within BodySettings.cs with values {0,1,2,3,10,11,13,14}, matching the PRINT_STATE.ERB:934-962 valid skin color set. The skin color retry loop uses a 20-iteration cap matching ERB WHILE 20.

Visitor override uses the visitor constants integer value 998 (人物_訪問者 from DIM.ERH:71) stored as a public constant in `CharacterConstants.VisitorCharacterIndex = 998`, and checks `(IVariableStore.GetFlag(FlagIndex.MiscSettings) >> 3) & 1` for bit 3 GETBIT semantics.

The Func<int,int> callback for multi-birth father determination (母→父) is stored as `_multiBirthFatherCheck` and invoked only when multiBirthFlag >= 1 and RAND:3 == 0 (non-identical twin path, i.e., ELSE branch of ERB line 955 `IF ARG:1 && RAND:3`). The callback follows the exact pattern of 妊娠処理変更パッチ.ERB:310-358: takes motherId, returns fatherId (-1 meaning "no change").

ERB wrapper functions in body_settings.ERB, EVENTTURNEND.ERB, PREGNACY_S.ERB, and 追加パッチverup.ERB are preserved as-is; no ERB changes are required by this feature.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `void BodyChangeDaily(int characterId)` to IBodySettings.cs |
| 2 | Add `void BodySettingsGenetics(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId)` to IBodySettings.cs |
| 3 | Add `void BodySettingsChildPGrowth(int childId)` and `void BodySettingsChildHairChange(int childId)` to IBodySettings.cs (2 new method signatures) |
| 4 | No deletions to existing IBodySettings methods; extend interface only (compiler enforces via BodySettings : IBodySettings) |
| 5 | Add `public static readonly CharacterFlagIndex Father = new(73)`, `GrowthStage = new(75)`, `ChildPSizeStore = new(1193)` to CharacterFlagIndex.cs |
| 6 | Add `public static readonly FlagIndex MiscSettings = new(6441)` to FlagIndex.cs |
| 7 | Add `private readonly IRandomProvider _random` field in the single 5-parameter constructor |
| 8 | Add `private readonly IVisitorVariables _visitor` field in the single 5-parameter constructor |
| 9 | Add `private readonly IEngineVariables _engine` field in the single 5-parameter constructor |
| 10 | Add `private readonly Func<int, int> _multiBirthFatherCheck` field in the single 5-parameter constructor |
| 11 | Define `private static readonly HashSet<int> ValidSkinColors = new() { 0, 1, 2, 3, 10, 11, 13, 14 }` in BodySettings.cs; skin color loop checks `ValidSkinColors.Contains(candidate)` |
| 12 | Define `Era.Core/Types/GrowthStage.cs` with all 5 growth stages: `None = 0, Nursing = 1, Infant = 2, GrowthPeriod = 3, Adult = 4` |
| 13 | Build passes after all additions; TreatWarningsAsErrors enforced by Directory.Build.props |
| 14 | Unit tests in Era.Core.Tests covering: twin copy (all 24 CFLAG fields from sibling ARG:2 per C4), non-twin genetics (seeded random, both mutation/non-mutation paths, slot distribution algorithm), visitor override on/off (bit 3 set/unset), skin color validation (valid set), both eye options (瞳OP1/OP2) cleared when value is 5, gender hair category (male/female), Tidy() invoked, P size store/reset, callback RESULT >= 0 guard, growth stage branching, hair change 1/4 chance |
| 15 | No TODO/FIXME/HACK in the four modified/created files |
| 16 | Test includes mock callback invocation verification (Verify/Times assertion on _multiBirthFatherCheck) for multi-birth genetics path (multiBirthFlag >= 1, non-identical twin RAND path) |
| 17 | No ERB changes: EVENTTURNEND.ERB, PREGNACY_S.ERB, 追加パッチverup.ERB preserve all 7 CALL statements to migrated functions |
| 18 | Test covers BodyChangeDaily increment (HairLength += 10) and clamp (reset to (HairLengthCategory - 1) * 100 when HairLength >= HairLengthCategory * 100) behavior per C16 |
| 19 | Test references asymmetric modulo values (100/120/140/160) for body option slot distribution per C10 |
| 20 | Define `public const int VisitorCharacterIndex = 998` in Era.Core/Types/CharacterConstants.cs (人物_訪問者 from DIM.ERH:71); AC verifies constant value is 998 |
| 21 | Test verifies Tidy() is called after genetics trait assignment via mock verification (Verify/Times/Received assertion on Tidy in genetics test context) |
| 22 | Twin copy test explicitly references 24-field completeness (assert all CFLAG fields copied from sibling, not just subset) |
| 23 | Two separate test assertions for eye option 5 clearing: one for 瞳OP1, one for 瞳OP2 (independently verified) |
| 24 | Test verifies IVisitorVariables mock methods are called during visitor override path (Received/Verify/Times assertion on _visitor) |
| 25 | Implement skin color retry loop with explicit 20-iteration cap (matching ERB WHILE 20 construct); AC verifies cap is coded near ValidSkinColors |
| 26 | Test verifies callback DidNotReceive for 2 non-ELSE paths: (1) multiBirthFlag==0 single birth, (2) multiBirthFlag>=1 AND RAND:3!=0 identical twin |
| 27 | Test covers ChildHairChange hair option update logic (HairLength>=200 threshold, option collision avoidance, clear-when-<200) |
| 28 | Implement slot distribution loop with explicit 20-iteration cap (matching ERB WHILE 20 at lines 1021-1029); AC verifies cap near slot logic |
| 29 | Implementation code references all 4 modulo values (100, 120, 140, 160) via Next() calls or named constants |
| 30 | Test verifies P size store (ChildPSizeStore = PSize) and reset (PSize = -2) sequence for non-twin genetics |
| 31 | Test verifies IEngineVariables mock methods (GetCharacterNo) are called during gender hair category determination |
| 32 | Define slot modulo values in order as an array or sequential constants: `{ 100, 120, 140, 160 }` matching slot 6→100, slot 7→120, slot 8→140, slot 9→160 |
| 33 | Test includes `_random.Next(15)` call for the 1/15 mutation chance check per C11 |
| 34 | Implementation uses `CharacterConstants.VisitorCharacterIndex` named constant (not hardcoded 998) in visitor override path |
| 35 | No ERB changes: body_settings.ERB preserves all 4 wrapper function definitions (@体変化_１日経過, @体設定_遺伝, @体設定_子供Ｐ成長, @体設定_子供髪変更) |
| 36 | Test verifies ChildPGrowth 3-way GrowthStage branching (Infant/GrowthPeriod/Adult → distinct PSize behavior) |
| 37 | Test verifies IVisitorVariables NOT called when GETBIT(FLAG:MiscSettings, 3) is unset (visitor override negative path per C9) |
| 38 | Test sets up mock callback returning -1 and asserts father ID is NOT updated (RESULT < 0 guard from ERB:999 `IF RESULT >= 0`) |
| 39 | Implementation uses `FlagIndex.MiscSettings` named constant (not hardcoded 6441) in GETBIT visitor override check |
| 40 | Change DI registration from `services.AddSingleton<IBodySettings, BodySettings>()` to explicit factory providing all 5 constructor parameters including Func<int,int> callback |
| 41 | Implementation calls `_random.Next(...)` in genetics/growth methods (not `new Random()` or static source) |
| 42 | DI factory lambda references the actual ERB `多生児父親判定` bridge method (not `_ => -1` no-op) |
| 43 | Test asserts SkinColor/HairColor CFLAG values after BodySettingsGenetics execution (derivation result, not just validation mechanics) |
| 44 | Test verifies SetCFlag with CharacterFlagIndex.Father is called via mock verification in callback positive path (RESULT >= 0 → father assigned) |
| 45 | Test configures specific numeric return values for IVisitorVariables properties (visitor trait value-based testing) |
| 46 | Body option trait CFLAG assignments verified in slot distribution test (BodyOption1-4 output storage) |
| 47 | IEngineVariables._engine.GetCharacterNo() called in BodySettings implementation (defined→used pattern) |
| 48 | IVisitorVariables defined→used verification. Verifies _visitor. property access in BodySettings.cs, closing IVisitorVariables storage-without-usage gap (AC#8 stores, AC#48 uses) |
| 49 | Test verifies PSize CFLAG modification in ChildPGrowth growth stage test (Infant/GrowthPeriod/Adult paths). Closes ChildPGrowth output-storage gap (AC#36 structure, AC#49 storage); mirrors AC#44 (Father SetCFlag) and AC#46 (BodyOption SetCFlag) patterns |
| 50 | Twin copy test references at least 5 distinct CFLAG field names. Supplements AC#22 by requiring 5+ specific CFLAG field names in twin copy test (prevents hardcoded '24' gaming) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where to put GrowthStage constants | A: Enum in CharacterFlagIndex.cs, B: GrowthStage enum in GrowthStage.cs, C: Constants in BodySettings.cs | B (GrowthStage enum in GrowthStage.cs) | Enum provides type safety (prevents accidental int mixing), exhaustive switch checking, and better IntelliSense. File named GrowthStage.cs per C# convention. Separate type file is discoverable from multiple callers (PREGNACY_S.ERB callers will also migrate later) |
| Constructor parameter count (4 new deps) | A: Replace existing constructor with 5-param, B: Separate GeneticsService class, C: Builder/options object | A (single 5-param constructor) | 5 params is within the stated threshold in Risks; all params are interfaces (mockable); replaces existing 1-param constructor per F796's consolidation direction; GeneticsService split deferred to F782 |
| VisitorCharacterIndex (998) definition | A: Named constant in BodySettings.cs, B: CharacterConstants.cs (new type), C: Inline magic number | B (public const in CharacterConstants.cs) | 人物_訪問者 = 998 is a game-wide constant from DIM.ERH:71 used by multiple ERB files. Per Zero Debt Upfront, define in a shared type (Era.Core/Types/CharacterConstants.cs) so future migrations reference the same constant without duplication |
| Func<int,int> callback signature | A: Func<int,int> (motherId -> fatherId), B: Func<int,int,int> (childId, motherId -> fatherId), C: Delegate type | A (Func<int,int>) | Matches the ERB calling convention: `CALL 多生児父親判定(母)` takes only the mother ID, returns RESULT; minimal surface area for AC#10 mock verification |
| Skin color valid set representation | A: HashSet<int> named ValidSkinColors, B: int[] with Contains, C: Inline CASE-style checks | A (HashSet<int>) | O(1) lookup; AC#11 matcher requires `ValidSkinColors` identifier; HashSet semantics match set membership check |
| Bit extraction for GETBIT(FLAG:雑多設定, 3) | A: (GetFlag(MiscSettings) >> 3) & 1, B: Separate helper BitGet(flag, bit), C: Use existing BitGet if available | A (inline expression) | Single usage within BodySettingsGenetics; helper indirection not warranted; pattern is the established C# equivalent of ERA's GETBIT |
| SRP deferral to F782 | A: Extract GeneticsService upfront in F780, B: Defer to F782 Post-Phase Review, C: Tolerate 6-responsibility class permanently | B (Defer to F782) | The 4 new methods share GetCFlag/SetCFlag helper infrastructure with existing init/validation methods, making extraction require either duplication or a shared base class. The ERB source groups all functions under 体設定.ERB, implying original cohesion. F782 Post-Phase Review has concrete GeneticsService extraction obligation in its Review Context. Deferral is a deliberate investment decision — F782 will have full context of all Phase 20 methods before designing the split. |
| Constructor injection vs parameter object | A: Direct 5-param constructor, B: BodySettingsOptions parameter object, C: Builder pattern | A (Direct 5-param constructor) | All 5 dependencies are distinct interfaces/delegates with no shared grouping. Parameter object adds indirection without benefit for 5 params. F782 will likely extract genetics methods into separate GeneticsService class, reducing BodySettings params back to 1-2. Future constructor churn is mitigated by F782's planned extraction. |
| Single constructor (5-param replacing 1-param) | A: Single constructor with all 5 deps required, B: 2-constructor pattern (1-param + 5-param), C: Optional parameters | A (single constructor) | F796 [DONE] removed parameterless ctor and established non-null IVariableStore pattern. Extending to single 5-param constructor follows same direction. All callers must provide all dependencies. No RequireXxx() guards needed. |

### Interfaces / Data Structures

**Extended IBodySettings.cs** — 4 new method signatures added after existing 7:

```csharp
void BodyChangeDaily(int characterId);
void BodySettingsGenetics(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId);
void BodySettingsChildPGrowth(int childId);
void BodySettingsChildHairChange(int childId);
```

**Replaced BodySettings.cs constructor** — single 5-parameter constructor (replaces old 1-parameter, parameterless already removed by F796):

```csharp
public BodySettings(
    IVariableStore variables,
    IRandomProvider random,
    IVisitorVariables visitor,
    IEngineVariables engine,
    Func<int, int> multiBirthFatherCheck)
{
    _variables = variables ?? throw new ArgumentNullException(nameof(variables));
    _random    = random    ?? throw new ArgumentNullException(nameof(random));
    _visitor   = visitor   ?? throw new ArgumentNullException(nameof(visitor));
    _engine    = engine    ?? throw new ArgumentNullException(nameof(engine));
    _multiBirthFatherCheck = multiBirthFatherCheck
        ?? throw new ArgumentNullException(nameof(multiBirthFatherCheck));
}
```

**New CharacterFlagIndex constants** (CharacterFlagIndex.cs):

```csharp
// Genetics / growth parameters (F780)
public static readonly CharacterFlagIndex Father          = new(73);   // 父親
public static readonly CharacterFlagIndex GrowthStage     = new(75);   // 成長度
public static readonly CharacterFlagIndex ChildPSizeStore = new(1193); // 子供Ｐ大きさ保管
```

**New FlagIndex constant** (FlagIndex.cs):

```csharp
public static readonly FlagIndex MiscSettings = new(6441); // 雑多設定 (GETBIT bit 3 = visitor body settings)
```

**New GrowthStage.cs** (Era.Core/Types/):

```csharp
namespace Era.Core.Types;

/// <summary>
/// Growth stage enum for child characters.
/// Maps to ERB symbolic names (DIM.ERH:547-552):
/// 成長度_なし=0, 成長度_乳児=1, 成長度_幼児=2, 成長度_成長期=3, 成長度_成人=4.
/// </summary>
public enum GrowthStage
{
    None = 0,         // 成長度_なし
    Nursing = 1,      // 成長度_乳児
    Infant = 2,       // 成長度_幼児
    GrowthPeriod = 3, // 成長度_成長期
    Adult = 4,        // 成長度_成人
}
```

**ValidSkinColors** (BodySettings.cs, private static readonly):

```csharp
// Valid skin color indices from PRINT_STATE.ERB:934-962 @skin_color()
// Values producing "ーーー" (invalid) are excluded.
private static readonly HashSet<int> ValidSkinColors = new() { 0, 1, 2, 3, 10, 11, 13, 14 };
```

**Visitor character constant** (Era.Core/Types/CharacterConstants.cs, new file):

```csharp
namespace Era.Core.Types;

public static class CharacterConstants
{
    public const int VisitorCharacterIndex = 998; // 人物_訪問者 (DIM.ERH:71)
}
```

**IRandomProvider usage note**: ERA's `RAND:N` maps to `_random.Next(N)` for `[0,N)`, and `RAND(min, max)` maps to `_random.Next(min, max)` for `[min,max)`. Both overloads are confirmed on IRandomProvider. The `!RAND:15` mutation check (`RAND:15 == 0`) maps to `_random.Next(15) == 0`.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#12 grep pattern `(Infant\|GrowthPeriod\|Adult)\s*=\s*(2\|3\|4)` matches `public const int Infant = 2` but also would match any stray string in test files; pattern is safe given GrowthStageConstants.cs is the only definition location | AC Definition Table (AC#12) | No change required; note for ac-tester: grep path `Era.Core/` excludes test files |
| DIM.ERH:547-552 defines 5 growth stages (None=0, Nursing=1, Infant=2, GrowthPeriod=3, Adult=4); C15 in AC Design Constraints states 幼児=2 / 成長期=3 / 成人=4 but omits 乳児=1 and なし=0 — these are not used by F780 methods but are present in ERB | AC Design Constraints (C15) | Superseded: AC#12 requires count_equals=5 (all 5 stages) and Task#3 specifies all 5 per Zero Debt Upfront. GrowthStage.cs defines None=0, Nursing=1, Infant=2, GrowthPeriod=3, Adult=4. |

<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,3,4 | Extend IBodySettings.cs with 4 new method signatures (BodyChangeDaily, BodySettingsGenetics, BodySettingsChildPGrowth, BodySettingsChildHairChange), preserving all 7 existing method signatures | | [x] |
| 2 | 5,6 | Add CharacterFlagIndex constants (Father=73, GrowthStage=75, ChildPSizeStore=1193) to CharacterFlagIndex.cs and FlagIndex.MiscSettings=6441 to FlagIndex.cs | | [x] |
| 3 | 12 | Create Era.Core/Types/GrowthStage.cs with public enum GrowthStage defining None=0, Nursing=1, Infant=2, GrowthPeriod=3, Adult=4 (all 5 ERB growth stages per Zero Debt Upfront) | | [x] |
| 3b | 20 | Create Era.Core/Types/CharacterConstants.cs with public static class CharacterConstants defining VisitorCharacterIndex=998 (人物_訪問者 from DIM.ERH:71) | | [x] |
| 4 | 7,8,9,10,11 | Extend BodySettings.cs: add 5-parameter constructor (IVariableStore, IRandomProvider, IVisitorVariables, IEngineVariables, Func<int,int>), add private fields for all new dependencies, define ValidSkinColors HashSet<int> | | [x] |
| 4b | 13,14 | Update all existing BodySettings test instantiations in BodyDetailInitMigrationTests.cs and BodySettingsBusinessLogicTests.cs to use the new 5-parameter constructor with mock IRandomProvider, IVisitorVariables, IEngineVariables, and Func<int,int> | | [x] |
| 4c | 40,42 | Update DI registration in ServiceCollectionExtensions.cs: change auto-resolved AddSingleton<IBodySettings, BodySettings>() to explicit factory registration providing all 5 constructor parameters including Func<int,int> callback delegate. The callback lambda in the factory must bridge to the engine's ERB function dispatch (calling @多生児父親判定 with motherId), satisfying AC#42's bridge reference check. | | [x] |
| 5a | 14,15,18,27,31,36,49 | Write RED unit tests for simpler methods: BodyChangeDaily (HairLength increment below threshold, clamp at HairLengthCategory * 100 per C16), ChildPGrowth (growth stage branching Infant/GrowthPeriod/Adult constants with PSize modification verification), ChildHairChange (1/4 chance hair randomization by gender + hair option update with HairLength>=200 threshold). Zero TODO/FIXME/HACK in test code (AC#15 scope includes Era.Core.Tests/). | | [x] |
| 5b | 14,15,16,19,21,22,23,24,26,30,31,33,37,38,43,44,45,46,50 | Write RED unit tests for BodySettingsGenetics: twin copy (all 24 CFLAG fields from sibling ARG:2), non-twin genetics (seeded random, mutation path RAND:15==0 and non-mutation, slot distribution via WHILE loop with seeded RAND:16), visitor override path (bit 3 set/unset for GETBIT(FLAG:6441,3)), skin color validation (valid set accepted, invalid rejected with 20-iteration cap), both eye options (瞳OP1 and 瞳OP2) independently cleared to 0 when value is 5, gender-based hair category (male NO==148 → category 2, female → category 4), Tidy() invoked after genetics, P size store and -2 reset in TWO separate test methods: (1) twin-path (ERB:979) and (2) non-twin-path (ERB:1319), mock callback invocation verification (both RESULT >= 0 father assignment and RESULT < 0 no-assignment paths; when RESULT >= 0, verify child CFLAG traits derive from callback-returned fatherId, not original fatherId parameter), mock callback NOT called for 2 non-ELSE paths: (1) multiBirthFlag == 0 (single birth), (2) multiBirthFlag >= 1 AND RAND:3 != 0 (identical twin path). Zero TODO/FIXME/HACK in test code (AC#15 scope includes Era.Core.Tests/). | | [x] |
| 6a | 14,15,18,27,36,47,49 | Implement BodyChangeDaily (HairLength increment+clamp), BodySettingsChildPGrowth (three-way GrowthStage branching for PSize toward ChildPSizeStore with SetCFlag storage), BodySettingsChildHairChange (1/4 chance hair randomization by gender + hair option update). Zero TODO/FIXME/HACK. | | [x] |
| 6b | 14,15,25,28,29,30,32,34,39,41,43,44,46,47,48,50 | Implement BodySettingsGenetics (twin copy path copying all 24 CFLAG fields from sibling ARG:2 + independent genetics path including 16-slot body option distribution with slot modulo values {100,120,140,160} in order, visitor override using CharacterConstants.VisitorCharacterIndex (not literal 998), skin color retry loop with 20-iteration cap, both eye options independently cleared when value is 5, gender hair category, SkinColor/HairColor derivation, Tidy() call, Func<int,int> callback for multi-birth). Slot distribution loop has 20-iteration cap. Zero TODO/FIXME/HACK. | | [x] |
| 7 | 13 | Build Era.Core/ via WSL dotnet build and verify zero warnings (TreatWarningsAsErrors) | | [x] |
| 8 | 14,17,35 | Run unit tests via WSL dotnet test Era.Core.Tests/ --filter BodySettings and verify all pass (GREEN). Verify AC#17 (ERB caller CALL preservation) and AC#35 (ERB wrapper definition preservation) via static grep — no ERB changes made | | [x] |

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
| 1 | implementer | sonnet | Feature file (Tasks 1-3: interface/type changes), feature-template.md, Skill(engine-dev), Skill(csharp-14) | IBodySettings.cs extended; CharacterFlagIndex.cs + FlagIndex.cs updated; GrowthStage.cs created |
| 2 | implementer | sonnet | Feature file (Task 4: BodySettings constructor + fields), existing BodySettings.cs, IRandomProvider.cs, IVisitorVariables.cs, IEngineVariables.cs | BodySettings.cs with new constructor, fields, ValidSkinColors, VisitorCharacterIndex |
| 3 | implementer | sonnet | Feature file (Tasks 5a+5b: RED tests), body_settings.ERB:932-1426, 妊娠処理変更パッチ.ERB:310-358, existing test file patterns | Era.Core.Tests BodySettingsGeneticsTests.cs (FAILING RED) |
| 4 | implementer | sonnet | Feature file (Task 6: GREEN implementation), RED tests from Phase 3, body_settings.ERB source | BodySettings.cs 4 methods implemented, all RED tests turn GREEN |
| 5 | implementer | sonnet | Feature file (Tasks 7-8: build + test run) | Build success log, test pass log; AC status updated |

### Pre-conditions

- F779 [DONE]: BodySettings.Tidy() exists in IBodySettings.cs and BodySettings.cs
- F781 [DONE]: IVisitorVariables.cs exists at Era.Core/Interfaces/IVisitorVariables.cs
- Era.Core/Interfaces/IBodySettings.cs exists with 7 existing method signatures
- Era.Core/State/BodySettings.cs exists with IVariableStore constructor
- Era.Core/Random/IRandomProvider.cs exists with Next(long max) and Next(long min, long max)
- Era.Core/Interfaces/IEngineVariables.cs exists with GetCharacterNo() at line 58
- F794 [DONE] — preserved all 7 IBodySettings methods (AC#4 count_equals=7 remains valid; resolved Phase1-CodebaseDrift iter1)

### Execution Order

1. Task 1 first (interface must exist before implementation compiles)
2. Tasks 2 and 3 can run in parallel (independent type files)
3. Task 4 after Task 1 (constructor references interfaces)
3b. Tasks 4b and 4c after Task 4 (existing tests and DI need updated constructor calls)
4. Tasks 5a+5b after Tasks 1-4 (tests reference all new types and constructor)
5. Task 6 after Task 5 (TDD RED→GREEN; implement to make RED tests pass)
6. Tasks 7 and 8 after Task 6 (build and test verification)

### Build Verification Steps

```bash
# Build (must succeed with zero warnings)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# Test (must all pass)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter BodySettings'
```

### Success Criteria

- IBodySettings.cs has exactly 11 method signatures (7 existing + 4 new)
- CharacterFlagIndex.cs has Father=73, GrowthStage=75, ChildPSizeStore=1193
- FlagIndex.cs has MiscSettings=6441
- Era.Core/Types/GrowthStage.cs exists with Infant=2, GrowthPeriod=3, Adult=4
- BodySettings.cs has 5-parameter constructor + all required private fields
- All unit tests for BodySettings PASS
- dotnet build Era.Core/ exits 0 with zero warnings
- No TODO/FIXME/HACK in the 4 modified/created files

### Error Handling

- If constructor parameter count exceeds 5 after changes: STOP → Report to user (consider GeneticsService split per Risks table)
- If IRandomProvider.Next semantics differ from ERA RAND: STOP → Report to user
- If BodySettings : IBodySettings compilation fails after interface extension: fix interface parameter types to match ERB signatures
- If any existing test (non-BodySettings filter) breaks after changes: STOP → Report to user

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task to this feature's Tasks table. Handoff Issue becomes the new feature's Problem. -->
<!-- Option B (existing Feature): Add to target feature's Review Notes as [pending]. -->
<!-- Option C (Phase reference): Only if Phase Status is NOT WIP/Done AND Phase != this feature's Phase. -->
<!-- Validation (FL PHASE-7): Every Mandatory Handoff row must have: (1) non-empty Destination column, (2) non-TBD Destination ID, (3) verifiable artifact (feature file exists or Phase doc section exists). -->
<!-- DRAFT Creation Checklist: If Option A chosen, new feature MUST have: Status [DRAFT], Philosophy (copied from parent), Problem (from Handoff Issue), Goal (actionable), Dependencies (this feature as Predecessor). -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| BodySettings responsibility extraction (6 responsibilities → GeneticsService) | F780 grows BodySettings to 11 methods spanning 6 concerns (init, validation, daily change, genetics, P growth, hair change); SRP violation | Feature | F782 | N/A (F782 Post-Phase Review already exists) |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-21 | START | initializer | Phase 1 Initialize | READY |
| 2026-02-21 | END | explorer | Phase 2 Investigation | SUCCESS |
| 2026-02-21 | START | implementer | Task 4b+4c | Fix test constructors + DI |
| 2026-02-21 | END | implementer | Task 4b+4c | SUCCESS (48/48 pass) |
| 2026-02-21 | START | implementer | Task 5a RED tests | 8 tests created |
| 2026-02-21 | END | implementer | Task 5a | SUCCESS (RED confirmed) |
| 2026-02-21 | START | implementer | Task 5b RED tests | 18 tests created |
| 2026-02-21 | END | implementer | Task 5b | SUCCESS (RED confirmed) |
| 2026-02-21 | START | implementer | Task 6a+6b GREEN | 4 methods implemented |
| 2026-02-21 | END | implementer | Task 6a+6b | SUCCESS (74/74 GREEN) |
| 2026-02-21 | END | ac-tester | Phase 7 AC verification | 40/50 PASS, 8 count_equals overmatch, 1 PRE-EXISTING |
| 2026-02-21 | DEVIATION | ac-static-verifier | AC#15 | PRE-EXISTING TODO in TrainingIntegrationTests.cs |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Impact Analysis/Technical Constraints/AC Design Constraints/Technical Design | verup_patch.ERB → 追加パッチverup.ERB (actual filename correction)
- [fix] Phase2-Review iter1: Tasks section Task Tags | Prose list replaced with 4-column table format per template
- [fix] Phase2-Review iter1: AC Definition Table/AC Details/Goal Coverage/AC Coverage/Tasks | Added AC#16 for multi-birth callback invocation verification (C2 gap)
- [resolved-applied] Phase2-Uncertain iter1: [AC-005] AC#14 bundles 10+ behavioral constraints (C3-C14) into single test-pass AC. No individual behavioral ACs for twin copy completeness, Tidy() invocation, skin color 20-iteration cap, etc. Consider adding dedicated ACs per high-risk constraint or requiring explicit test method names in AC#14.
- [fix] Phase2-Review iter2: AC Definition Table/Goal Coverage | Added AC#17 for ERB caller CALL statement preservation (Goal item 6 gap)
- [fix] Phase2-Review iter2: AC#15 | Extended Grep scope to include Era.Core.Tests/ for zero-debt coverage of test files (C8)
- [fix] Phase2-Review iter3: AC Design Constraints/AC Definition Table/AC Details/Goal Coverage/AC Coverage/Tasks | Added C16 constraint + AC#18 for BodyChangeDaily behavioral test coverage (increment+clamp)
- [fix] Phase2-Review iter4: AC#12 | Matcher strengthened: anchored name-to-value pairings (Infant=2, GrowthPeriod=3, Adult=4), changed to count_equals=3 in GrowthStageConstants.cs
- [fix] Phase2-Review iter4: AC#16 | Matcher strengthened: added motherId argument verification per C2 requirement
- [fix] Phase2-Review iter5: AC#11 | Matcher strengthened: added value verification for all 8 valid skin colors {0,1,2,3,10,11,13,14}
- [fix] Phase2-Review iter6: C4/AC#14/Task#5/Task#6 | Twin copy source corrected: sibling (ARG:2) not mother, count corrected 22→24
- [fix] Phase2-Review iter6: C6/AC#14/Task#5/Task#6 | Eye option clearing: specified BOTH 瞳OP1 AND 瞳OP2 independently
- [fix] Phase2-Review iter6: Technical Design/AC#16 | Callback condition corrected: RAND:3 == 0 (ELSE branch), not != 0
- [fix] Phase2-Review iter7: AC Coverage AC#14 | Corrected "22 fields" → "24 CFLAG fields from sibling", added slot distribution and callback RESULT guard
- [fix] Phase2-Review iter7: Task#5 | Added slot distribution (WHILE loop seeded RAND:16) and callback RESULT >= 0/< 0 paths to test coverage
- [fix] Phase2-Review iter8: AC Definition Table/AC Details/AC Coverage/Task#5 | Added AC#19 for slot distribution modulo test (C10: 100/120/140/160)
- [fix] Phase2-Review iter9: Task#5 | Added callback negative guard: NOT called when multiBirthFlag == 0
- [resolved-applied] Phase2-Review iter9: [AC-005] IVisitorVariables accessor method invocation not verified by any AC. AC#8 checks field exists, AC#14 tests visitor on/off, but no AC verifies visitor trait VALUES come from IVisitorVariables interface methods vs IVariableStore globals.
- [resolved-applied] Phase2-Review iter9: [AC-005] AC#19 slot-to-modulo mapping is not slot-specific. Pattern matches co-occurrence but cannot verify correct modulo per slot (100→OP1, 120→OP2, 140→OP3, 160→OP4).
- [fix] Phase2-Review iter10: Tasks/AC Definition Table/AC Details/AC Coverage/Goal Coverage | Added AC#20 for VisitorCharacterIndex=998 constant verification; added AC#17 to Task#8
- [fix] Phase3-Maintainability iter10: Risks table | Commented-out code mitigation changed from "migrate as-is" to "implement active behavior only" per zero-debt mandate
- [fix] Phase3-Maintainability iter10: AC#12/Task#3/Technical Design/AC Coverage | GrowthStageConstants extended from 3→5 values (added None=0, Nursing=1) per Zero Debt Upfront
- [fix] Phase3-Maintainability iter10: Key Decisions | Added 3-constructor pattern decision documenting temporary nature and F796 consolidation plan
- [resolved-applied] Phase3-Maintainability iter10: [DES-003] BodySettings class has 6 responsibilities (init, validation, daily change, genetics, P growth, hair change) with 11 methods. Consider extracting genetics/growth into GeneticsService after F780 completion. Track as Mandatory Handoff or Key Decision.
- [fix] Phase7-FinalRefCheck iter10: Background/Technical Constraints/Technical Design/Implementation Contract | pregnancy_patch.ERB → 妊娠処理変更パッチ.ERB (actual filename correction, same pattern as verup_patch fix)
- [fix] Phase2-Review iter1: Technical Design/Tasks divider | Added missing --- divider between Technical Design and Tasks sections (template compliance)
- [fix] Phase2-Review iter1: Scope Reference table | Standardized column alignment (right-align → left-align)
- [fix] Phase2-Review iter2: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#21 (Tidy invocation), AC#22 (twin copy 24 fields), AC#23 (both eye options) — resolves [pending] Phase2-Uncertain iter1 AC-005
- [fix] Phase2-Review iter2: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#24 (IVisitorVariables accessor invocation) — resolves [pending] Phase2-Review iter9 AC-005
- [fix] Phase2-Review iter3: AC#19 | Matcher strengthened: co-occurrence pattern → count_equals=4 on `Next\((100|120|140|160)\)` for per-slot modulo verification — resolves [pending] Phase2-Review iter9 AC-005
- [fix] Phase2-Review iter4: AC#21 | Matcher fixed: removed genetics keyword requirement from assertion line, simplified to `(Received|Verify|Times)\(.*\)\.(Tidy|tidy)` matching NSubstitute idiom
- [fix] Phase2-Review iter4: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#25 for skin color 20-iteration cap verification (C14 gap)
- [fix] Phase2-Review iter4: AC#22 | Matcher strengthened: added assertion keyword requirement (Assert|Equal|Should) to prevent comment-only match
- [fix] Phase2-Review iter5: Philosophy Derivation | 'clear phase boundaries' row AC Coverage: AC#4 → AC#4, AC#17 (AC#17 covers ERB wrapper preservation)
- [fix] Phase2-Review iter5: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#26 for callback negative guard (multiBirthFlag==0 → DidNotReceive)
- [fix] Phase2-Review iter6: AC#26 | Expanded from single to dual negative guard: count_equals=2 covering (1) single birth multiBirthFlag==0, (2) identical twin multiBirthFlag>=1 AND RAND:3!=0
- [fix] Phase2-Review iter7: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#27 for ChildHairChange hair option update test coverage
- [fix] Phase2-Review iter7: AC Design Constraints/AC Definition Table/AC Details/AC Coverage/Tasks | Added C17 + AC#28 for slot distribution WHILE 20 cap (distinct from skin color C14/AC#25)
- [fix] Phase3-Maintainability iter8: Mandatory Handoffs | Added 3-constructor pattern consolidation → F796 (Leak Prevention compliance)
- [fix] Phase3-Maintainability iter8: Risks | Added F794 execution order risk (validation method relocation affects AC#4)
- [fix] Phase2-Review iter9: Implementation Contract Pre-conditions | Added F794 coordination constraint (AC#4 count_equals=7 depends on F794 not running first)
- [fix] Phase2-Review iter10: Mandatory Handoffs | Added F794 AC#4 count dependency tracking (Forward-Only Mode)
- [resolved-applied] Phase2-Review iter10: [AC-005] AC#19 slot-to-modulo ordering not verifiable by grep pattern. count_equals=4 confirms 4 modulo values tested but cannot verify correct slot assignment (loop-detected: iter3/iter8 addressed count, not ordering)
- [fix] PostLoop-UserFix: Mandatory Handoffs | Added GeneticsService extraction → F782 (DES-003 user decision: Handoff to Post-Phase Review)
- [fix] PostLoop-UserFix: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#29 for slot modulo order verification in BodySettings.cs (AC-005 user decision: implementation code matcher)
- [fix] Phase2-Review restart-iter1: AC#29 | Matcher relaxed: single-line `100.*120.*140.*160` → count_equals=4 on `(Next|modulo|Modulo|SlotModul).*(100|120|140|160)` to support both array and inline implementations
- [fix] Phase2-Review restart-iter1: F782 Review Context | Added GeneticsService extraction obligation from F780 (Mandatory Handoff Option B)
- [fix] Phase2-Review restart-iter1: F782 Dependencies | F780 status [DRAFT] → [PROPOSED] (stale status correction)
- [fix] Phase2-Review restart-iter2: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#30 (C5 P size store/reset) and AC#31 (IEngineVariables invocation) — behavioral gap closure analogous to AC#21-24 pattern
- [fix] Phase7-FinalRefCheck restart-iter3: Links | Added F540, F567, F100 (referenced in Dependencies but missing from Links)
- [fix] Phase2-Review restart-iter4: AC#30 | Expanded to count_equals=2 covering both twin-path (ERB:979) and non-twin-path (ERB:1319) PSize=-2 reset
- [resolved-applied] Phase1-RefCheck iter1: Links section references F540 (feature-540.md) but file does not exist — was incorrectly added from HTML comment template example
- [resolved-applied] Phase1-RefCheck iter1: Links section references F567 (feature-567.md) but file does not exist — was incorrectly added from HTML comment template example
- [resolved-applied] Phase1-RefCheck iter1: Links section references F100 (feature-100.md) but file does not exist — was incorrectly added from HTML comment template example
- [fix] Phase2-Review restart2-iter1: AC#22 | Matcher strengthened: removed Father|GrowthStage alternatives, now requires literal '24' for C4 full-count enforcement
- [fix] Phase2-Review restart2-iter1: AC Definition Table/AC Details/AC Coverage | Added AC#32 for slot-to-modulo ordering verification (100.*120.*140.*160 single-line pattern)
- [fix] Phase2-Review restart2-iter1: Philosophy Derivation | 'documented transition points' remapped from AC#14,AC#15 to AC#17 (ERB wrapper preservation)
- [fix] Phase2-Review restart2-iter1: Philosophy Derivation | 'continuous development pipeline' AC coverage expanded to include AC#14
- [fix] Phase3-Maintainability restart2-iter2: AC#32 added to Task#6 AC# column
- [resolved-applied] Phase3-Maintainability restart2-iter2: 3-constructor pattern (parameterless + 1-param + 5-param) — F796 scope does not explicitly include constructor consolidation (loop: same issue as Phase3 iter10) → resolved by F796 [DONE] removing parameterless ctor
- [resolved-applied] Phase3-Maintainability restart2-iter2: BodySettings SRP 6 responsibilities → 11 methods — handoff to F782 exists but F782 Problem is generic review, not concrete extraction plan (loop: same issue as DES-003) → F782 Review Context now contains concrete GeneticsService extraction obligation (lines 41-48). /fc will generate Problem/Goal from this.
- [fix] Phase3-Maintainability restart2-iter2: Task#6 | Split into Task#6a (simpler methods: BodyChangeDaily, ChildPGrowth, ChildHairChange) and Task#6b (BodySettingsGenetics complex method)
- [fix] Phase3-Maintainability restart2-iter2: Key Decisions/Technical Design/AC#20/Task#4 | VisitorCharacterIndex moved from private const in BodySettings.cs to public const in Era.Core/Types/CharacterConstants.cs (Zero Debt Upfront: game-wide constant)
- [fix] Phase3-Maintainability restart2-iter2: Key Decisions/Technical Design/AC#12/AC#15/Task#3 | GrowthStageConstants static class → GrowthStage enum in GrowthStage.cs (type safety, exhaustive switch checking)
- [fix] Phase2-Review restart2-iter3: Philosophy Derivation row 4 | Remapped from 'type constants added' → 'All 4 ERB functions migrated so F782 can proceed'; AC#5,AC#6 → AC#1,AC#2,AC#3
- [fix] Phase2-Review restart2-iter3: AC Coverage AC#20 | Corrected 'private const in BodySettings.cs' → 'public const in CharacterConstants.cs' (SSOT consistency with Key Decisions)
- [fix] Phase2-Review restart2-iter3: Goal Coverage item 1 | Removed AC#18 (behavioral test, not interface extension); Goal item 1 = AC#1, AC#2, AC#3 only
- [fix] Phase2-Review restart2-iter4: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#33 for mutation path test (C11 structural coverage, Next(15) check)
- [fix] Phase2-Review restart2-iter4: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#34 for CharacterConstants.VisitorCharacterIndex usage verification in BodySettings.cs
- [resolved-applied] Phase2-Review restart2-iter5: AC#22 matcher cannot fully verify 24-field twin copy completeness via grep — matches pattern relies on literal '24' which may not appear in actual assertion lines. → User chose A: added Design Constraint to AC#22 requiring '24' in assertion line.
- [fix] PostLoop-UserFix: AC#22 AC Details | Added Design Constraint: test must include '24' in assertion message/comment for grep matcher verification
- [resolved-applied] Phase1-CodebaseDrift iter1: [DES-002] F796 [DONE] removed parameterless constructor and RequireVariables() guard. → User chose A: single 5-param constructor. Applied to Technical Constraints/Design/Key Decisions/Interfaces/AC Coverage.
- [fix] PostLoop-UserFix: Technical Constraints/Technical Design/Key Decisions/Interfaces/AC Coverage | 3-constructor pattern → single 5-param constructor (user decision: F796 pattern consolidation)
- [resolved-applied] Phase1-CodebaseDrift iter1: [DES-002] Mandatory Handoff row 1 '3-constructor pattern consolidation → F796' is obsolete. → User chose A: deleted row.
- [fix] PostLoop-UserFix: Mandatory Handoffs | Deleted obsolete 3-constructor pattern → F796 row (F796 [DONE], single constructor decided)
- [resolved-applied] Phase1-CodebaseDrift iter1: [DEP-001] Mandatory Handoff row 2 'AC#4 depends on F794 not relocating methods' — F794 [DONE] preserved all 7 IBodySettings methods. Coordination constraint satisfied.
- [resolved-applied] Phase1-CodebaseDrift iter1: [DEP-001] Implementation Contract Pre-condition 'F794 must NOT be [DONE] before F780 Task 1' — F794 [DONE] with no IBodySettings impact. Pre-condition stale but harmless.
- [resolved-applied] Phase1-CodebaseDrift iter1: Review Notes [pending] line 886 '3-constructor pattern — F796 scope' — resolved by F796 completion (F796 removed parameterless ctor).
- [info] Phase1-DriftChecked: F794 (Related)
- [info] Phase1-DriftChecked: F796 (Related)
- [fix] Phase2-Review restart3-iter1: Implementation Contract Phase 1 output | GrowthStageConstants.cs → GrowthStage.cs (SSOT consistency with Task#3/AC#12/Technical Design)
- [fix] Phase2-Review restart3-iter1: Implementation Contract Pre-conditions | F794 stale pre-condition updated to reflect [DONE] status
- [fix] Phase2-Review restart4-iter1: Sections | Removed non-template sections (## Created, ## Summary, ## Scope Reference) per template compliance
- [fix] Phase2-Review restart4-iter1: Baseline Measurement | BodySettings method count TBD → 7 (actual measurement)
- [fix] Phase2-Review restart4-iter2: Philosophy Derivation | 'continuous development pipeline' AC coverage expanded to include behavioral ACs (AC#18, AC#19, AC#27, AC#33)
- [fix] Phase2-Review restart4-iter2: AC#15 Details | Added Era.Core/Types/CharacterConstants.cs to grep path list (SSOT consistency with AC Definition Table)
- [fix] Phase2-Review restart4-iter2: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#35 for ERB wrapper function definitions preservation in body_settings.ERB
- [fix] Phase2-Review restart4-iter3: AC#30 Details | Added Design Constraint requiring 2 matches from separate test methods (twin/non-twin paths)
- [fix] Phase2-Review restart4-iter3: Philosophy | Added Behavioral Correctness principle supplementing Pipeline Continuity
- [fix] Phase2-Review restart4-iter4: Philosophy Derivation | Added Behavioral Correctness row mapping to AC#18-AC#33
- [fix] Phase2-Review restart4-iter4: Task#6b | Added explicit CharacterConstants.VisitorCharacterIndex usage instruction (AC#34 alignment)
- [fix] Phase2-Review restart4-iter5: Task#6a AC# | Removed AC#25 (skin color belongs to Task#6b BodySettingsGenetics)
- [fix] Phase2-Review restart4-iter5: Task#6b AC# | Added AC#25 (skin color retry cap)
- [fix] Phase2-Review restart4-iter5: AC Definition Table/AC Details/AC Coverage/Tasks/Philosophy | Added AC#36 for ChildPGrowth growth stage branching test coverage
- [fix] Phase2-Review restart4-iter5: AC#27 | Extended matcher to include Next(4) for RAND:4 gate coverage
- [fix] Phase2-Review restart4-iter6: AC#31 | Matcher strengthened to require GetCharacterNo in verification chain (prevent false positive with other IEngineVariables methods)
- [fix] Phase2-Review restart4-iter7: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks/Philosophy | Added AC#37 for visitor override negative path (C9: IVisitorVariables NOT called when bit 3 unset)
- [fix] Phase2-Review restart4-iter7: Upstream Issues | Corrected GrowthStage entry to align with AC#12/Task#3 (5 stages per Zero Debt Upfront)
- [fix] Phase2-Review restart4-iter8: AC#30 | Design Constraint downgraded to Implementation Guidance (grep count_equals cannot enforce separate test methods)
- [fix] Phase2-Review restart4-iter8: AC#7,AC#8,AC#9 | Matchers strengthened from bare interface name to `private readonly` field pattern (verify constructor-injected DI)
- [fix] Phase2-Review restart4-iter9: AC#29 | Changed from count_equals=4 to matches (resolves conflict with AC#32 single-line array requirement)
- [fix] Phase2-Review restart4-iter10: Philosophy Derivation | Added AC#34 to Behavioral Correctness row (visitor character constant usage affects behavioral correctness)
- [fix] Phase4-ACValidation iter10: AC#30 | Fixed regex escaping: `\\-2` → `-2` (was matching literal backslash, not numeric -2)
- [fix] Phase2-Review restart3-iter1: Mandatory Handoffs | Deleted stale F794 coordination row (F794 [DONE], no IBodySettings impact)
- [fix] Phase2-Review iter1: Background section | Removed duplicate --- divider (template compliance)
- [fix] Phase2-Review iter1: Constraint Details | Added missing C8, C11, C12, C13, C17 detail blocks
- [fix] Phase2-Uncertain iter1: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#38 for callback RESULT < 0 path coverage (C2 gap)
- [fix] Phase2-Review iter1: Goal Coverage Verification | Removed AC#4 from Goal#6 (interface preservation ≠ ERB wrapper preservation)
- [fix] Phase2-Review iter1: Philosophy text | Updated 'AC#18-AC#33' → 'AC#18-AC#37' (Behavioral Correctness range expansion)
- [fix] Phase2-Review iter2: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#39 for FlagIndex.MiscSettings usage verification (AC#6/AC#34 pattern gap)
- [fix] Phase2-Review iter2: Task#6b AC# | Added AC#30 (P size store/reset implementation ownership)
- [fix] Phase2-Review iter2: Philosophy text | Updated 'AC#18-AC#37' → 'AC#18, AC#19, AC#21-AC#34, AC#36-AC#38' (exclude non-behavioral AC#20, AC#35)
- [fix] Phase3-Maintainability iter3: Tasks/AC Definition Table/AC Details/AC Coverage/Implementation Contract | Added Task#4b (existing test constructor updates), Task#4c (DI registration), AC#40 (explicit BodySettings construction)
- [fix] Phase2-Review iter4: AC#18 | Changed from matches to count_equals=2 for two-path coverage (increment+clamp), consistent with AC#26/AC#30/AC#23 pattern
- [resolved-applied] Phase2-Uncertain iter5: [AC-005] No AC verifies Func<int,int> callback DI wiring delegates to actual ERB @多生児父親判定 wrapper. C2 says "do not test internal logic" but no-op lambda passes all ACs. Scope decision: unit vs integration.
- [fix] Phase2-Uncertain iter1: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#41 for IRandomProvider usage verification (_random.Next in BodySettings.cs, defined→used AC pair pattern)
- [fix] Phase2-Review iter2: Goal Coverage item 4 | Added AC#38 to Func<int,int> callback Goal coverage (RESULT<0 guard path)
- [fix] Phase2-Review iter2: Philosophy Derivation | Added AC#38 to Behavioral Correctness row (SSOT consistency with Philosophy text)
- [fix] Phase2-Review iter5: AC#19 | Narrowed grep scope from Era.Core.Tests/ to Era.Core.Tests/**/BodySettings*Tests.cs (prevent false positive from EngineVariablesDelegationTests.cs Next(100))
- [fix] Phase2-Review iter6: Task#5 | PSize store/reset description expanded to explicitly require TWO separate test methods for twin-path (ERB:979) and non-twin-path (ERB:1319)
- [fix] Phase2-Review iter3: AC#33 | Narrowed grep scope from Era.Core.Tests/ to Era.Core.Tests/**/BodySettings*Tests.cs (consistent with AC#19 path restriction)
- [resolved-applied] Phase2-Uncertain iter3: [SSOT-001] Philosophy text references AC numbers directly (AC#18, AC#19, AC#21-AC#34, AC#36-AC#38). Conceptual concern: Philosophy should state invariants, not cite verification artifacts. However, no SSOT rule explicitly forbids this and the text was intentionally added through FL fixes.
- [resolved-skipped] Phase2-Review iter4: [DES-003] CharacterFlagIndex.GrowthStage field name same as GrowthStage enum type in Era.Core.Types namespace. Validator confirms no CS0101 error (field vs type), but reviewer flagged naming ambiguity concern. Consider renaming to GrowthStageIndex.
- [fix] PostLoop-UserFix: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#42 for callback DI wiring verification (多生児父親判定 bridge reference in ServiceCollectionExtensions.cs)
- [fix] PostLoop-UserFix: Philosophy text | Removed AC number references from Behavioral Correctness clause (SSOT direction: Philosophy → AC, not AC → Philosophy)
- [fix] Phase2-Review restart-iter1: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#43 for SkinColor/HairColor derivation result verification (slot distribution result gap)
- [fix] Phase2-Review restart-iter1: Task#5 | Added zero TODO/FIXME/HACK constraint and AC#15 to Task#5 AC# column (ordering clarity)
- [fix] Phase2-Review restart-iter2: AC#43 | Fixed escaped pipes in AC Definition Table matcher (\\| → | for ripgrep alternation)
- [fix] Phase2-Review restart-iter2: Philosophy Derivation/Goal Coverage | Added AC#43 to Behavioral Correctness row and Goal#5 (traceability)
- [fix] Phase2-Review restart-iter3: Task#6a AC# | Added AC#18,AC#27,AC#36 (behavioral ACs whose GREEN state depends on Task#6a implementation)
- [fix] Phase2-Review restart-iter4: Task#6b AC# | Added AC#43 (SkinColor/HairColor derivation, AC#30 precedent)
- [fix] Phase2-Uncertain restart-iter4: Task#5 | Added callback father ID propagation guidance (verify child traits from callback-returned fatherId, not original)
- [resolved-skipped] Phase3-Maintainability iter5: [DES-003] Mandatory Handoff indirection F780→F782→new feature for GeneticsService extraction. User decision: 2-level indirection accepted; F782 Post-Phase Review handles responsibility separation holistically.
- [fix] Phase3-Maintainability iter5: Key Decisions | Added SRP deferral rationale (deliberate: shared helpers, ERB cohesion, F782 planned extraction)
- [fix] Phase3-Maintainability iter5: Task#4c | Added bridge method clarification (callback lambda → engine ERB dispatch)
- [fix] Phase3-Maintainability iter5: AC#14 Details | Added equivalence testing note (per-path coverage via 43 ACs)
- [fix] Phase3-Maintainability iter5: Key Decisions | Added constructor injection vs parameter object rationale
- [fix] Phase3-Maintainability iter5: Tasks | Split Task#5 → Task#5a (simpler methods) + Task#5b (BodySettingsGenetics) per Task#6a/6b precedent
- [fix] Phase2-Review iter1: Technical Design | Added siblingId usage documentation: ignored when multiBirthFlag==0 (ELSE branch ERB:994+), callers may pass any value
- [fix] Phase2-Review iter1: AC Details | Added backtick formatting to Expected fields across all 43 AC Detail blocks (template compliance)
- [resolved-applied] Phase2-Review iter2: [AC-005] Philosophy narrowed from 'identical outputs' to 'verified per branching path' to align with per-path AC approach. User decision applied.
- [fix] Phase2-Review iter2: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#44 for callback positive result CFLAG:Father assignment verification (Task#5b gap)
- [resolved-applied] Phase2-Review iter2: [AC-005] AC#22 twin copy '24' literal gaming resolved by adding AC#50 (5+ CFLAG field names required in twin copy test). User decision applied.
- [fix] Phase2-Review iter3: AC#31 | Changed matches → count_equals=2 (verify GetCharacterNo in BOTH BodySettingsGenetics and ChildHairChange test contexts); added AC#31 to Task#5a
- [fix] Phase2-Review iter4: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#45 for visitor trait value derivation verification (IVisitorVariables Returns setup)
- [fix] Phase2-Review iter5: AC#44 Details | Added Implementation Guidance: test must use distinct values for original fatherId vs callback return
- [fix] Phase2-Review iter5: AC Definition Table/AC Details/AC Coverage/Tasks | Added AC#46 for body option trait CFLAG storage verification (slot distribution output gap)
- [fix] Phase2-Review iter6: AC#10 | Matcher strengthened: Func<int,\s*int> → private readonly Func<int,\s*int> (DI pattern consistency with AC#7/8/9)
- [fix] Phase2-Review iter6: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Tasks | Added AC#47 for IEngineVariables defined→used gap (_engine.GetCharacterNo in BodySettings.cs)
- [fix] Phase2-Review iter6: AC#44 Details | Upgraded Implementation Guidance → Design Constraint (value distinction analogous to AC#22)
- [fix] Phase2-Review iter7: AC#27 | Changed matches → count_equals=2 (RAND:4 gate dual-path: positive + negative, consistent with AC#18/26/30 pattern)
- [fix] Phase2-Review iter7: AC#45 Details | Added Design Constraint for visitor value distinction (analogous to AC#44)
- [fix] Phase2-Uncertain iter1: AC#46 AC Definition Table | Removed backslash escaping from pipe characters in matcher (consistency with AC Details and AC#43/AC#45)
- [fix] Phase2-Review iter2: Philosophy Derivation Behavioral Correctness | Added AC#44, AC#45, AC#46 to coverage row (output-level verification ACs missing from derivation)
- [fix] Phase2-Review iter3: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Philosophy Derivation/Task#6b | Added AC#48 for IVisitorVariables defined→used gap (_visitor. in BodySettings.cs, consistent with AC#41/AC#47 pattern)
- [fix] Phase2-Review iter4: Philosophy Derivation Behavioral Correctness | Added AC#16 (callback invocation verification, multi-birth path behavioral check)
- [fix] Phase2-Review iter4: Task#6b AC# | Added AC#44, AC#46 (SetCFlag verification ACs, following AC#43 precedent in Task#6b)
- [fix] Phase2-Review iter5: AC Definition Table/AC Details/AC Coverage/Goal Coverage/Philosophy Derivation/Task#5a/Task#6a | Added AC#49 for ChildPGrowth PSize output-storage gap (consistent with AC#44/AC#46 pattern)
- [fix] Phase2-Review iter6: Goal Coverage item 1 | Removed AC#49 (behavioral AC misassigned to interface extension goal)
- [fix] Phase2-Review iter6: AC#30 AC Definition Table/AC Details | Strengthened matcher to require assertion/mock keywords (consistent with AC#44/AC#46 pattern, prevents comment-only false positives)
- [fix] PostLoop-UserFix: Philosophy/Philosophy Derivation | Narrowed 'identical outputs to ERB sources' to 'verified per branching path' (user decision: per-path AC approach sufficient)
- [fix] PostLoop-UserFix: AC Definition Table/AC Details/AC Coverage/Philosophy Derivation/Task#5b/Task#6b | Added AC#50 for twin copy CFLAG field name verification (supplements AC#22, prevents '24' literal gaming)
- [fix] Phase2-Review restart2-iter1: AC#50 AC Definition Table/AC Details | Narrowed matcher to require twin/sibling context keywords (prevents false positives from non-twin tests)
- [fix] Phase2-Review restart2-iter2: AC#49 AC Definition Table/AC Details | Changed matches → count_equals=3 (3-way growth stage paths, consistent with AC#18/AC#27 multi-path pattern)
- [fix] Phase2-Review iter1: AC Coverage table AC#48/49/50 | Removed extra pipe characters creating 3-column rows in 2-column table (template compliance)
- [fix] Phase2-Uncertain iter1: Philosophy Derivation | Added AC#35 to 'ensures continuous development pipeline' row (AC#35 covers ERB wrapper definitions, pipeline continuity gap)
- [fix] Phase4-ACValidation iter2: AC#50 AC Definition Table | Changed invalid matcher count_gte → gte (valid matcher per Skill(testing), works with Grep types per F792)
- [fix] Phase2-Review iter3: Philosophy Derivation Behavioral Correctness | Added AC#49 (ChildPGrowth PSize 3-way branching, growth stage path coverage gap)
- [fix] Phase2-Review iter4: AC#50 AC Definition Table/AC Details | Removed SetCFlag/GetCFlag from first OR alternative, both alternatives now require twin/sibling context (prevents false positives from non-twin tests)

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (parent)
- [Related: F778](feature-778.md) - Body Initialization (sibling, established BodySettings.cs)
- [Predecessor: F779](feature-779.md) - Body Settings UI (Tidy() method needed)
- [Related: F781](feature-781.md) - Visitor Settings (IVisitorVariables interface)
- [Related: F794](feature-794.md) - Shared Body Option Validation Abstraction
- [Related: F796](feature-796.md) - BodyDetailInit IVariableStore Migration
- [Successor: F782](feature-782.md) - Post-Phase Review
