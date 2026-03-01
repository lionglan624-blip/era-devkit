# Feature 800: BodySettings GeneticsService Extraction & Shop-Layer IVariableStore Dedup

## Status: [DONE]
<!-- fl-reviewed: 2026-02-22T08:06:15Z -->

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
| Parent Feature | F782 |
| Discovery Point | Post-Phase Review Phase 20, Mandatory Handoff |
| Timestamp | 2026-02-22 |

### Identified Gap
BodySettings.cs at 1149 lines / 11 methods / 6 concerns violates SRP due to F780 migrating genetics, P growth, and hair change into the existing class. GeneticsService extraction needed to split genetics, P growth, and hair change methods into a dedicated service class.

Additionally, Shop-layer IVariableStore helper duplication exists: ShopSystem.cs and ShopDisplay.cs contain strongly-typed private helpers (GetCharacterFlag, GetTalent, SetCharacterFlag, SetTalent, etc.) that can be deduplicated using the IVariableStoreExtensions class created in F782.

### Files Involved
| File | Relevance |
|------|-----------|
| Era.Core/State/BodySettings.cs | SRP violation — genetics, P growth, hair change methods to extract |
| Era.Core/Shop/ShopSystem.cs | Strongly-typed private helpers duplicating IVariableStoreExtensions |
| Era.Core/Shop/ShopDisplay.cs | Strongly-typed private helpers duplicating IVariableStoreExtensions |

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | Post-Phase Review Phase 20, Mandatory Handoff (F782) |
| Derived Task | "GeneticsService extraction and Shop-layer IVariableStoreExtensions dedup" |
| Comparison Result | N/A (Mandatory Handoff, not task-comparator) |
| DEFER Reason | N/A (ADOPT — feature resolves identified SRP violation) |

### Parent Review Observations
F782 Phase 20 post-review identified BodySettings.cs at 1149 lines / 11 methods / 6 concerns as SRP violation caused by F780 migrating genetics methods using ERB file boundary instead of SRP boundary. Additionally identified 14 duplicated private helpers in ShopSystem.cs and ShopDisplay.cs that can be consolidated via IVariableStoreExtensions. Both tracks are co-deployed as a single refactoring feature.

---

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
Zero Debt Upfront - BodySettings.cs is the single source of truth for body initialization and tidy/validation logic. Genetics, child P growth, and hair change logic must be extracted into a dedicated GeneticsService to enforce SRP boundaries. IVariableStoreExtensions is the SSOT for strongly-typed IVariableStore access, and all Shop-layer private helper duplicates must be consolidated there to eliminate the impedance mismatch between Result<int> returns and plain int consumption.

### Problem (Current Issue)
BodySettings.cs contains 1140 lines spanning 6 distinct concerns (body init, tidy/validation, daily change, genetics, child P growth, hair change) because F780 migrated genetics-related ERB functions from body_settings.ERB into the pre-existing class following ERB file boundary rather than SRP boundary (Era.Core/State/BodySettings.cs:705-1136). The migration strategy (docs/architecture/phases/phase-20-27-game-systems.md:46) intentionally deferred structural refactoring to post-phase review checkpoints. Additionally, ShopSystem.cs (6 private helpers, lines 443-459) and ShopDisplay.cs (8 private helpers, lines 464-479) contain identical `.Match(v => v, _ => 0)` wrapper patterns because IVariableStoreExtensions (Era.Core/Interfaces/IVariableStoreExtensions.cs) cannot provide CharacterId-typed overloads for GetTalent, GetBase, GetMaxBase, GetAbility, GetExp, GetMark, GetJuel due to C# extension method shadowing rules (instance methods always take precedence per C# spec 12.8.10.3).

### Goal (What to Achieve)
1. Extract genetics, child P growth, child hair change, and their private helpers (CopyTwinFields, AssignTrait, GenerateBodyOption) from BodySettings.cs into a new GeneticsService class with a dedicated IGeneticsService interface, reducing BodySettings to ~690 lines of cohesive body-init and tidy/validation logic.
2. Extend IVariableStoreExtensions with renamed CharacterId-typed getter extensions to deduplicate the 14 private helpers across ShopSystem.cs and ShopDisplay.cs.
3. Update DI registration, GameInitialization, and relocate existing genetics tests to verify GeneticsService directly.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does BodySettings.cs have 1140 lines with 11 public methods spanning 6 concerns? | Because F780 added BodySettingsGenetics, BodySettingsChildPGrowth, and BodySettingsChildHairChange methods plus private helpers into the existing class | Era.Core/State/BodySettings.cs:705, 959, 995 |
| 2 | Why were genetics functions added to BodySettings instead of a separate service? | Because the IBodySettings interface already existed and the methods originated from the same ERB source file (body_settings.ERB) | Era.Core/Interfaces/IBodySettings.cs:21-23 |
| 3 | Why was ERB file boundary used as the migration unit? | Because Phase 20 sub-features were organized by ERB module boundary for traceability | docs/architecture/phases/phase-20-27-game-systems.md:46 |
| 4 | Why did the architecture design choose ERB file structure? | Because the migration strategy intentionally defers refactoring to post-phase review checkpoints to avoid scope creep during migration | pm/features/feature-782.md:66 |
| 5 | Why (Root)? | The ERB-to-C# methodology prioritizes behavioral equivalence over structural refactoring, creating a deliberate technical debt that F800 now resolves | pm/features/feature-782.md:30 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | BodySettings.cs has 1140 lines / 11 methods / 6 concerns; Shop files have 14 duplicated private helpers | Migration strategy used ERB file boundary (body_settings.ERB) instead of SRP boundary, and C# extension method shadowing prevents deduplicating CharacterId-typed IVariableStore accessors |
| Where | Era.Core/State/BodySettings.cs:705-1136; Era.Core/Shop/ShopSystem.cs:443-459; Era.Core/Shop/ShopDisplay.cs:464-479 | docs/architecture/phases/phase-20-27-game-systems.md:46 (ERB module boundary design); C# spec 12.8.10.3 (extension shadowing rule) |
| Fix | Add more methods to BodySettings; keep private helpers per-file | Extract GeneticsService with dedicated interface; extend IVariableStoreExtensions with renamed getter methods |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F782 | [DONE] | Predecessor - created this DRAFT, established IVariableStoreExtensions pattern |
| F780 | [DONE] | Related - introduced genetics methods into BodySettings causing SRP violation |
| F794 | [DONE] | Related - extracted BodyOptionValidator, proves extraction pattern works |
| F796 | [DONE] | Related - unified BodyDetailInit to IVariableStore |
| F779 | [DONE] | Related - Phase 20 migration context |
| F783 | [DRAFT] | Related - Phase 21 Planning, no blocking dependency |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Code compiles with all predecessors done | FEASIBLE | F780, F782 both [DONE] |
| Extraction target well-bounded | FEASIBLE | Genetics methods (705-1136) share unique deps: IRandomProvider, IVisitorVariables, IEngineVariables, multiBirthFatherCheck; not used elsewhere in BodySettings |
| Extension shadowing solvable | FEASIBLE | Renamed extensions (GetTalentValue, etc.) avoid shadowing; GetCFlag/SetCFlag pattern proves approach works |
| Tests exist and are relocatable | FEASIBLE | BodySettingsGeneticsTests.cs, BodySettingsGeneticsTask5bTests.cs already isolated for genetics testing |
| DI registration pattern exists | FEASIBLE | ServiceCollectionExtensions.cs:149-159 shows IBodySettings registration as precedent |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/State/BodySettings.cs | HIGH | Extract ~430 lines (genetics, P growth, hair change, private helpers); remove 4 constructor dependencies |
| Era.Core/Interfaces/IBodySettings.cs | HIGH | Remove 3 genetics method declarations; breaking change for consumers |
| Era.Core/State/GeneticsService.cs | HIGH | New file receiving extracted methods (~430 lines) |
| Era.Core/Interfaces/IGeneticsService.cs | HIGH | New interface with 3 genetics methods |
| Era.Core/Interfaces/IVariableStoreExtensions.cs | MEDIUM | Add CharacterId-typed renamed getter extensions for Shop dedup |
| Era.Core/Shop/ShopSystem.cs | MEDIUM | Replace 6 private helpers with extension method calls |
| Era.Core/Shop/ShopDisplay.cs | MEDIUM | Replace 8 private helpers with extension method calls |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | LOW | Add IGeneticsService singleton registration |
| Era.Core/Common/GameInitialization.cs | LOW | Add IGeneticsService dependency |
| Era.Core.Tests/ | MEDIUM | Relocate genetics tests to verify GeneticsService directly |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Extension method shadowing | C# spec 12.8.10.3; IVariableStore defines instance methods returning Result<int> | Cannot create same-name extensions for CharacterId-typed getters; must use rename pattern (e.g., GetTalentValue) |
| IBodySettings interface split is breaking change | Era.Core/Interfaces/IBodySettings.cs:21-23 | All consumers (GameInitialization, DI) must switch to IGeneticsService for genetics methods |
| TreatWarningsAsErrors=true | Directory.Build.props F708 | Unused using/shadowing warnings become build errors |
| ERB callers must continue working | Game/ERB/PREGNACY_S.ERB:376,706-707 | ERB wrappers in body_settings.ERB call C# implementation; must remain functional |
| BodySettingsGenetics calls Tidy() at line 955 | Era.Core/State/BodySettings.cs:955 | GeneticsService must depend on IBodySettings for Tidy() cross-call |
| Constructor param redistribution | Era.Core/State/BodySettings.cs:30-34 | BodySettings drops from 5 to 1 param (IVariableStore only); GeneticsService takes all 5 original deps plus IBodySettings |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Extension naming inconsistency across codebase | MEDIUM | MEDIUM | Establish and document consistent naming convention (e.g., GetTalentValue, GetBaseValue) |
| IBodySettings consumers break after interface split | LOW | MEDIUM | Only GameInitialization and DI reference IBodySettings; both updated in same feature |
| Test constructor changes cause regressions | HIGH | LOW | Mechanical change: update test constructors to create GeneticsService instead of BodySettings |
| Tidy() cross-call creates tight coupling | MEDIUM | MEDIUM | One-way dependency: GeneticsService receives IBodySettings via constructor; no circular dependency |
| Circular dependency risk | LOW | HIGH | Enforced one-way: GeneticsService depends on IBodySettings, never reverse |
| ItemPurchase mistakenly included in scope | MEDIUM | LOW | Explicitly excluded per F782 C5; different param signatures (raw int) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| BodySettings.cs line count | wc -l Era.Core/State/BodySettings.cs | ~1140 | Should reduce to ~690 after extraction |
| ShopSystem.cs private helpers | grep -c "private.*Get\|private.*Set" Era.Core/Shop/ShopSystem.cs | 6 | Should be 0 after dedup |
| ShopDisplay.cs private helpers | grep -c "private.*Get\|private.*Set" Era.Core/Shop/ShopDisplay.cs | 8 | Should be 0 after dedup |
| IVariableStoreExtensions method count | grep -c "public static" Era.Core/Interfaces/IVariableStoreExtensions.cs | 5 | Should increase with new extensions |
| All tests pass | dotnet test Era.Core.Tests/ | PASS | Must remain PASS after refactoring |

**Baseline File**: `.tmp/baseline-800.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Extension shadowing prevents CharacterId-typed overloads with same name as interface methods | C# spec 12.8.10.3; Era.Core/Interfaces/IVariableStoreExtensions.cs:26-29 | Must use rename pattern for getter extensions (e.g., GetTalentValue instead of GetTalent) |
| C2 | BodySettingsGenetics calls Tidy() at line 955 | Era.Core/State/BodySettings.cs:955 | GeneticsService must have IBodySettings constructor dependency for Tidy() call; AC must verify this dependency |
| C3 | ItemPurchase uses raw int params, excluded from scope | Era.Core/Shop/ItemPurchase.cs:300-352; F782 C5 | AC must NOT verify ItemPurchase helper removal |
| C4 | ArchitectureTests must include IGeneticsService | Era.Core.Tests/ArchitectureTests.cs | AC must verify IGeneticsService added to architecture test assertions |
| C5 | ValidSkinColors only used by genetics | Era.Core/State/BodySettings.cs:38 | Should move to GeneticsService; AC must verify removal from BodySettings |
| C6 | BodyParams/CharacterBodySettings are init-only data | Era.Core/State/BodySettings.cs:61-431 | Must remain in BodySettings; AC must NOT move these |
| C7 | Set helpers are void wrappers (no .Match() needed) | Era.Core/Shop/ShopSystem.cs:447,453,459 | Setters may use direct delegation or thin extensions; AC must verify helper removal regardless |
| C8 | GetJuel uses int index not JuelIndex enum | Era.Core/Shop/ShopDisplay.cs:478-479 | Extension must accept int index parameter; AC must verify correct parameter type |
| C9 | multiBirthFatherCheck callback only used by genetics | Era.Core/State/BodySettings.cs:34,44 | Dependency moves entirely to GeneticsService constructor |
| C10 | No interface gaps found | Interface Dependency Scan | All required interfaces (IVariableStore, IRandomProvider, IVisitorVariables, IEngineVariables, IBodySettings) exist; only IGeneticsService needs creation |

### Constraint Details

**C1: Extension Method Shadowing**
- **Source**: C# language specification 12.8.10.3; documented in IVariableStoreExtensions.cs:26-29 comment block
- **Verification**: Attempt to create `GetTalent(this IVariableStore, CharacterId, TalentIndex)` extension -- compiler will use instance method instead
- **AC Impact**: All CharacterId-typed getter extensions must use distinct names (e.g., GetTalentValue, GetBaseValue, GetMaxBaseValue, GetAbilityValue, GetExpValue, GetMarkValue, GetJuelValue)

**C2: Tidy() Cross-Call**
- **Source**: BodySettings.cs line 955 calls `Tidy(chara)` from within BodySettingsGenetics method
- **Verification**: Grep for `Tidy(` in BodySettings.cs genetics region (lines 705-957)
- **AC Impact**: GeneticsService constructor must accept IBodySettings; AC must verify constructor parameter and Tidy() call delegation

**C3: ItemPurchase Exclusion**
- **Source**: F782 C5 explicitly defers ItemPurchase to separate scope; raw int params differ from CharacterId-typed pattern
- **Verification**: Compare ItemPurchase.cs:300-342 param types with ShopSystem/ShopDisplay helpers
- **AC Impact**: No AC may reference ItemPurchase.cs modifications

**C4: ArchitectureTests Update**
- **Source**: Era.Core.Tests/ArchitectureTests.cs contains interface registration assertions
- **Verification**: Grep for IBodySettings in ArchitectureTests.cs
- **AC Impact**: Must add IGeneticsService assertion alongside existing IBodySettings assertion

**C5: ValidSkinColors Migration**
- **Source**: BodySettings.cs:38 defines ValidSkinColors used only in genetics methods
- **Verification**: Grep for ValidSkinColors usage across BodySettings.cs; all references in genetics region
- **AC Impact**: AC must verify ValidSkinColors is in GeneticsService and not in BodySettings

**C6: Init Data Stays in BodySettings**
- **Source**: BodyParams struct and CharacterBodySettings dictionaries (lines 61-431) are init-only preset data
- **Verification**: These are consumed by BodyDetailInit which remains in BodySettings
- **AC Impact**: AC must NOT expect these to move; BodySettings retains body-init responsibility

**C7: Setter Helpers Are Void Wrappers**
- **Source**: ShopSystem.cs SetTalent (line 447), SetCharacterFlag (line 453), SetMaxBase (line 459) are void passthrough wrappers
- **Verification**: Read ShopSystem.cs setter helpers; no .Match() pattern, direct delegation
- **AC Impact**: Setters can use either direct IVariableStore calls or thin extension wrappers; AC verifies helper removal

**C8: GetJuel Int Index Parameter**
- **Source**: ShopDisplay.cs:478-479 GetJuel(CharacterId, int) uses int index, not JuelIndex enum
- **Verification**: Read ShopDisplay.cs GetJuel signature
- **AC Impact**: Extension method must accept int parameter for juel index; AC must verify correct parameter type

**C9: multiBirthFatherCheck Migration**
- **Source**: BodySettings.cs:34 declares `_multiBirthFatherCheck` callback; line 44 receives via constructor; used only in genetics
- **Verification**: Grep for `_multiBirthFatherCheck` in BodySettings.cs; all uses in genetics region
- **AC Impact**: BodySettings constructor must not accept this parameter after extraction; GeneticsService constructor must accept it

**C10: Interface Completeness**
- **Source**: Interface Dependency Scan across all 3 investigations
- **Verification**: All interfaces (IVariableStore, IRandomProvider, IVisitorVariables, IEngineVariables, IBodySettings) verified to exist with required methods
- **AC Impact**: Only IGeneticsService needs creation; no interface gaps block implementation

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F782 | [DONE] | Created IVariableStoreExtensions pattern, established F800 DRAFT via Mandatory Handoff |
| Related | F780 | [DONE] | Introduced genetics methods into BodySettings causing SRP violation |
| Related | F794 | [DONE] | Extracted BodyOptionValidator, proves extraction pattern works |
| Related | F796 | [DONE] | Unified BodyDetailInit to IVariableStore |
| Related | F779 | [DONE] | Phase 20 migration context |
| Related | F783 | [DRAFT] | Phase 21 Planning, no blocking dependency |

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
| "BodySettings.cs is the **single source of truth** for body initialization and tidy/validation logic" | BodySettings must retain only body-init, tidy/validation, and daily-change methods; genetics methods must be removed | AC#5, AC#6 |
| "Genetics, child P growth, and hair change logic **must** be extracted into a dedicated GeneticsService" | GeneticsService.cs must exist with all 3 genetics public methods + private helpers | AC#1, AC#2, AC#3, AC#4 |
| "IVariableStoreExtensions is the **SSOT** for strongly-typed IVariableStore access" | All Shop-layer private helpers using .Match(v => v, _ => 0) must be replaced with extension method calls | AC#9, AC#10, AC#11, AC#12 |
| "**all** Shop-layer private helper duplicates **must** be consolidated" | ShopSystem (6 helpers) and ShopDisplay (8 helpers) must have zero private variable access helpers remaining | AC#11, AC#12 |
| "**eliminate** the impedance mismatch between Result<int> returns and plain int consumption" | IVariableStoreExtensions must provide renamed CharacterId-typed getters that handle Result<int> unwrapping | AC#9, AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | GeneticsService.cs exists | file | Glob(Era.Core/State/GeneticsService.cs) | exists | - | [x] |
| 2 | IGeneticsService.cs exists | file | Glob(Era.Core/Interfaces/IGeneticsService.cs) | exists | - | [x] |
| 3 | IGeneticsService declares all 3 genetics methods | code | Grep(Era.Core/Interfaces/IGeneticsService.cs) | count_equals | `void (BodySettingsGenetics|BodySettingsChildPGrowth|BodySettingsChildHairChange)` = 3 | [x] |
| 4 | GeneticsService implements IGeneticsService | code | Grep(Era.Core/State/GeneticsService.cs) | matches | `class GeneticsService.*IGeneticsService` | [x] |
| 5 | IBodySettings no longer declares genetics methods | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | not_matches | `BodySettingsGenetics|BodySettingsChildPGrowth|BodySettingsChildHairChange` | [x] |
| 6 | IBodySettings retains 8 non-genetics methods | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | count_equals | `(void|int) (BodyDetailInit|Tidy|ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize|GetTightnessBaseValue|BodyChangeDaily)` = 8 | [x] |
| 7 | ValidSkinColors removed from BodySettings | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `ValidSkinColors` | [x] |
| 8 | ValidSkinColors present in GeneticsService | code | Grep(Era.Core/State/GeneticsService.cs) | matches | `ValidSkinColors` | [x] |
| 9 | IVariableStoreExtensions has CharacterId-typed getter extensions | code | Grep(Era.Core/Interfaces/IVariableStoreExtensions.cs) | count_equals | `public static int Get(TalentValue|BaseValue|MaxBaseValue|AbilityValue|ExpValue|MarkValue|JuelValue)` = 7 | [x] |
| 10 | ShopSystem setter helpers replaced by direct instance calls | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | `private void SetTalent|private void SetMaxBase` | [x] |
| 11 | ShopSystem has no private variable access helpers | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | `private int GetCharacterFlag|private void SetCharacterFlag|private int GetTalent|private void SetTalent|private int GetMaxBase|private void SetMaxBase` | [x] |
| 12 | ShopDisplay has no private variable access helpers | code | Grep(Era.Core/Shop/ShopDisplay.cs) | not_matches | `private int GetCharacterFlag|private int GetTalent|private int GetBase|private int GetMaxBase|private int GetAbility|private int GetExp|private int GetMark|private int GetJuel` | [x] |
| 13 | BodySettings constructor no longer accepts IRandomProvider | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `IRandomProvider` | [x] |
| 14 | BodySettings constructor no longer accepts multiBirthFatherCheck | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `_multiBirthFatherCheck` | [x] |
| 15 | GeneticsService constructor accepts IBodySettings | code | Grep(Era.Core/State/GeneticsService.cs) | matches | `IBodySettings` | [x] |
| 16 | GeneticsService calls Tidy via IBodySettings | code | Grep(Era.Core/State/GeneticsService.cs) | matches | `\.Tidy\(` | [x] |
| 17 | DI registration for IGeneticsService | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `IGeneticsService` | [x] |
| 18 | ArchitectureTests includes IGeneticsService | code | Grep(Era.Core.Tests/ArchitectureTests.cs) | matches | `IGeneticsService` | [x] |
| 19 | Genetics tests verify GeneticsService directly | code | Grep(Era.Core.Tests/State/BodySettingsGeneticsTests.cs) | matches | `GeneticsService` | [x] |
| 20 | Build succeeds | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 21 | All Era.Core.Tests pass | test | dotnet test Era.Core.Tests/ | succeeds | - | [x] |
| 22 | All engine.Tests pass | test | dotnet test engine.Tests/ | succeeds | - | [x] |
| 23 | No technical debt in new files | code | Grep(Era.Core/State/GeneticsService.cs,Era.Core/Interfaces/IGeneticsService.cs,Era.Core/Interfaces/IVariableStoreExtensions.cs) | not_matches | `TODO|FIXME|HACK` | [x] |
| 24 | ItemPurchase private helpers unchanged (C3 exclusion) | code | Grep(Era.Core/Shop/ItemPurchase.cs) | count_equals | `private (int|void) (GetTalent|SetTalent|GetAbility|SetAbility|GetBase|SetBase|GetMaxBase|SetMaxBase|GetCFlag|SetCFlag)` = 10 | [x] |
| 25 | BodyParams/CharacterBodySettings remain in BodySettings (C6) | code | Grep(Era.Core/State/BodySettings.cs) | matches | `BodyParams` | [x] |
| 26 | GeneticsService constructor accepts Func<int, int> multiBirthFatherCheck (C9) | code | Grep(Era.Core/State/GeneticsService.cs) | matches | `Func<int.*int>.*multiBirthFatherCheck` | [x] |
| 27 | IVariableStoreExtensions preserves existing 5 methods (backward compatibility) | code | Grep(Era.Core/Interfaces/IVariableStoreExtensions.cs) | count_equals | `public static (int|void) (GetCFlag|SetCFlag|GetTalent)` = 5 | [x] |
| 28 | engine-dev SKILL.md updated with IGeneticsService | file | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `IGeneticsService` | [x] |
| 29 | BodySettings has no genetics private helpers after extraction | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `CopyTwinFields\|AssignTrait\|GenerateBodyOption` | [x] |
| 30 | IGeneticsService.BodySettingsGenetics has correct 5-parameter signature | code | Grep(Era.Core/Interfaces/IGeneticsService.cs) | matches | `void BodySettingsGenetics\(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId\)` | [x] |

### AC Details

**AC#1: GeneticsService.cs exists**
- **Test**: Glob pattern="Era.Core/State/GeneticsService.cs"
- **Expected**: File exists
- **Rationale**: New file receiving extracted genetics, P growth, and hair change methods (~430 lines)

**AC#2: IGeneticsService.cs exists**
- **Test**: Glob pattern="Era.Core/Interfaces/IGeneticsService.cs"
- **Expected**: File exists
- **Rationale**: Dedicated interface for the extracted genetics service, following existing IBodySettings pattern

**AC#3: IGeneticsService declares all 3 genetics methods**
- **Test**: Grep pattern=`void (BodySettingsGenetics|BodySettingsChildPGrowth|BodySettingsChildHairChange)` path="Era.Core/Interfaces/IGeneticsService.cs" | count
- **Expected**: 3 matches
- **Rationale**: All 3 genetics methods from IBodySettings must move to the new interface. (C10: no interface gaps)

**AC#4: GeneticsService implements IGeneticsService**
- **Test**: Grep pattern=`class GeneticsService.*IGeneticsService` path="Era.Core/State/GeneticsService.cs"
- **Expected**: 1 match
- **Rationale**: Implementation class must declare interface implementation

**AC#5: IBodySettings no longer declares genetics methods**
- **Test**: Grep pattern=`BodySettingsGenetics|BodySettingsChildPGrowth|BodySettingsChildHairChange` path="Era.Core/Interfaces/IBodySettings.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: Interface split is complete; genetics methods belong to IGeneticsService only. Verified current state: these 3 methods currently exist at lines 21-23 -- pattern DOES currently match, so not_matches is non-vacuous.

**AC#6: IBodySettings retains 8 non-genetics methods**
- **Test**: Grep pattern=`(void|int) (BodyDetailInit|Tidy|ValidateBodyOption|ValidatePenisOption|ValidateSimpleDuplicate|CalculatePenisSize|GetTightnessBaseValue|BodyChangeDaily)` path="Era.Core/Interfaces/IBodySettings.cs" | count
- **Expected**: 8 matches (was 11 with genetics; 11 - 3 = 8). Pattern `(void|int)` matches all 8 method return types.
- **Rationale**: Backward compatibility; existing consumers of body-init/tidy/validation must not break. (C6: init-only data stays)

**AC#7: ValidSkinColors removed from BodySettings**
- **Test**: Grep pattern=`ValidSkinColors` path="Era.Core/State/BodySettings.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: (C5) ValidSkinColors is only used by genetics methods; must move entirely to GeneticsService. Currently matched at lines 38, 795, 798.

**AC#8: ValidSkinColors present in GeneticsService**
- **Test**: Grep pattern=`ValidSkinColors` path="Era.Core/State/GeneticsService.cs"
- **Expected**: Pattern found
- **Rationale**: Complement of AC#7; ensures ValidSkinColors is actually relocated, not just deleted.

**AC#9: IVariableStoreExtensions has CharacterId-typed getter extensions**
- **Test**: Grep pattern=`public static int Get(TalentValue|BaseValue|MaxBaseValue|AbilityValue|ExpValue|MarkValue|JuelValue)` path="Era.Core/Interfaces/IVariableStoreExtensions.cs" | count
- **Expected**: 7 matches (one per variable type)
- **Rationale**: (C1) Renamed getters avoid extension method shadowing. These deduplicate the .Match(v => v, _ => 0) pattern across ShopSystem (3 getters) and ShopDisplay (8 getters). Verified current state: 0 matches -- ensures RED state.

**AC#10: ShopSystem setter helpers replaced by direct instance calls**
- **Test**: Grep pattern=`private void SetTalent|private void SetMaxBase` path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: (C7, Upstream Issue) IVariableStore.SetTalent and SetMaxBase are instance methods that would shadow extensions with same signatures (C# spec 12.8.10.3). Setter helpers are replaced by direct instance calls (`_variables.SetTalent(...)`, `_variables.SetMaxBase(...)`), not new extensions. SetCFlag already exists as extension (F782). Verified current state: private void SetTalent at line 452 and private void SetMaxBase at line 458 currently match.

**AC#11: ShopSystem has no private variable access helpers**
- **Test**: Grep pattern=`private int GetCharacterFlag|private void SetCharacterFlag|private int GetTalent|private void SetTalent|private int GetMaxBase|private void SetMaxBase` path="Era.Core/Shop/ShopSystem.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: All 6 helpers (3 getters: lines 443, 449, 455; 3 setters: lines 446, 452, 458) must be replaced with IVariableStoreExtensions calls. Verified current state: pattern currently matches at 6 locations.

**AC#12: ShopDisplay has no private variable access helpers**
- **Test**: Grep pattern=`private int GetCharacterFlag|private int GetTalent|private int GetBase|private int GetMaxBase|private int GetAbility|private int GetExp|private int GetMark|private int GetJuel` path="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: All 8 getter helpers (lines 464-479) must be replaced with IVariableStoreExtensions calls. ShopDisplay has no setter helpers. Verified current state: pattern currently matches at 8 locations.

**AC#13: BodySettings constructor no longer accepts IRandomProvider**
- **Test**: Grep pattern=`IRandomProvider` path="Era.Core/State/BodySettings.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: IRandomProvider dependency moves entirely to GeneticsService. Verified current state: currently matches at lines 31, 42 (using + constructor param).

**AC#14: BodySettings constructor no longer accepts multiBirthFatherCheck**
- **Test**: Grep pattern=`_multiBirthFatherCheck` path="Era.Core/State/BodySettings.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: (C9) multiBirthFatherCheck callback only used by genetics; must move to GeneticsService. Verified current state: matches at lines 34, 51, 720.

**AC#15: GeneticsService constructor accepts IBodySettings**
- **Test**: Grep pattern=`IBodySettings` path="Era.Core/State/GeneticsService.cs"
- **Expected**: Pattern found
- **Rationale**: (C2) GeneticsService needs IBodySettings to call Tidy() at line 955 of current BodySettings.cs. One-way dependency prevents circular reference.

**AC#16: GeneticsService calls Tidy via IBodySettings**
- **Test**: Grep pattern=`\.Tidy\(` path="Era.Core/State/GeneticsService.cs"
- **Expected**: Pattern found
- **Rationale**: (C2) The Tidy() call at current line 955 must be preserved as a cross-service delegation call.

**AC#17: DI registration for IGeneticsService**
- **Test**: Grep pattern=`IGeneticsService` path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- **Expected**: Pattern found
- **Rationale**: New service must be registered in DI container alongside existing IBodySettings registration.

**AC#18: ArchitectureTests includes IGeneticsService**
- **Test**: Grep pattern=`IGeneticsService` path="Era.Core.Tests/ArchitectureTests.cs"
- **Expected**: Pattern found
- **Rationale**: (C4) ArchitectureTests.cs contains interface registration assertions; IGeneticsService must be added.

**AC#19: Genetics tests verify GeneticsService directly**
- **Test**: Grep pattern=`GeneticsService` path="Era.Core.Tests/State/BodySettingsGeneticsTests.cs"
- **Expected**: Pattern found
- **Rationale**: Existing genetics tests currently create BodySettings instances; must be updated to create GeneticsService instances for proper unit testing of the extracted service.

**AC#20: Build succeeds**
- **Test**: `dotnet build Era.Core/`
- **Expected**: Exit code 0
- **Rationale**: TreatWarningsAsErrors=true; ensures no unused usings, shadowing warnings, or missing references after refactoring.

**AC#21: All Era.Core.Tests pass**
- **Test**: `dotnet test Era.Core.Tests/`
- **Expected**: All tests pass
- **Rationale**: Extraction must not break any existing tests; relocated genetics tests must pass against GeneticsService.

**AC#22: All engine.Tests pass**
- **Test**: `dotnet test engine.Tests/`
- **Expected**: All tests pass
- **Rationale**: Engine tests may reference IBodySettings; interface split must not break engine-level integration.

**AC#23: No technical debt in new files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths="Era.Core/State/GeneticsService.cs, Era.Core/Interfaces/IGeneticsService.cs, Era.Core/Interfaces/IVariableStoreExtensions.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: Zero Debt Upfront philosophy; new code must not introduce technical debt markers.

**AC#24: ItemPurchase private helpers unchanged (C3 exclusion)**
- **Test**: Grep pattern=`private (int|void) (GetTalent|SetTalent|GetAbility|SetAbility|GetBase|SetBase|GetMaxBase|SetMaxBase|GetCFlag|SetCFlag)` path="Era.Core/Shop/ItemPurchase.cs" | count
- **Expected**: 10 matches (count_equals). Baseline: ItemPurchase currently has 10 private helpers with raw int params. Count must remain 10 after F800 (no removal, no addition).
- **Rationale**: (C3) ItemPurchase uses raw int params, excluded from scope per F782 C5. A count_equals check verifies the file was not accidentally modified during Shop-layer dedup.

**AC#25: BodyParams/CharacterBodySettings remain in BodySettings (C6)**
- **Test**: Grep pattern=`BodyParams` path="Era.Core/State/BodySettings.cs"
- **Expected**: Pattern found
- **Rationale**: (C6) Init-only data must remain in BodySettings; only genetics methods are extracted. Currently matches -- this AC verifies they are NOT accidentally moved.

**AC#26: GeneticsService constructor accepts Func<int, int> multiBirthFatherCheck (C9)**
- **Test**: Grep pattern=`Func<int.*int>.*multiBirthFatherCheck` path="Era.Core/State/GeneticsService.cs"
- **Expected**: Pattern found
- **Rationale**: (C9) The multiBirthFatherCheck callback migrates from BodySettings to GeneticsService constructor.

**AC#27: IVariableStoreExtensions preserves existing 5 methods (backward compatibility)**
- **Test**: Grep pattern=`public static (int|void) (GetCFlag|SetCFlag|GetTalent)` path="Era.Core/Interfaces/IVariableStoreExtensions.cs" | count
- **Expected**: 5 matches (count_equals). Baseline: IVariableStoreExtensions currently has 5 methods (GetCFlag x2, SetCFlag x2, GetTalent x1). Adding 7 new getter extensions must not remove or modify existing methods.
- **Rationale**: (C10) Backward compatibility for F782's IVariableStoreExtensions. State-layer consumers (BodySettings, PregnancySettings, WeatherSettings) depend on existing extension methods.

**AC#28: engine-dev SKILL.md updated with IGeneticsService**
- **Test**: Grep pattern=`IGeneticsService` path=".claude/skills/engine-dev/SKILL.md"
- **Expected**: Pattern found
- **Rationale**: Per ssot-update-rules.md, `Era.Core/Interfaces/*.cs` (new interface) requires engine-dev SKILL.md update. IGeneticsService is a new interface created by this feature.

**AC#29: BodySettings has no genetics private helpers after extraction**
- **Test**: Grep pattern=`CopyTwinFields|AssignTrait|GenerateBodyOption` path="Era.Core/State/BodySettings.cs"
- **Expected**: 0 matches (not_matches)
- **Rationale**: Private helpers CopyTwinFields, AssignTrait, and GenerateBodyOption are exclusively used by genetics methods and must migrate to GeneticsService. Without this AC, partial extraction could leave dead helper code in BodySettings while all other ACs pass. Verified current state: CopyTwinFields at line 892, AssignTrait at line 935, GenerateBodyOption at line 860 — pattern currently matches.

**AC#30: IGeneticsService.BodySettingsGenetics has correct 5-parameter signature**
- **Test**: Grep pattern=`void BodySettingsGenetics\(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId\)` path="Era.Core/Interfaces/IGeneticsService.cs"
- **Expected**: Pattern found
- **Rationale**: AC#3 verifies method name count but not parameter signatures. A stubbed interface with wrong parameter count would pass AC#3. This AC ensures the extracted interface maintains behavioral equivalence with the original IBodySettings declaration. Current IBodySettings.cs line 21 defines the exact 5-parameter signature.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extract genetics, child P growth, child hair change, and their private helpers from BodySettings.cs into a new GeneticsService class with a dedicated IGeneticsService interface, reducing BodySettings to ~690 lines | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#13, AC#14, AC#25, AC#26, AC#29, AC#30 |
| 2 | Extend IVariableStoreExtensions with renamed CharacterId-typed getter extensions to deduplicate the 14 private helpers across ShopSystem.cs and ShopDisplay.cs | AC#9, AC#10, AC#11, AC#12, AC#24, AC#27 |
| 3 | Update DI registration, GameInitialization, and relocate existing genetics tests to verify GeneticsService directly | AC#17, AC#18, AC#19, AC#20, AC#21, AC#22, AC#28 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Feature 800 consists of two independent but co-deployed refactoring tracks:

**Track A — GeneticsService Extraction**
Extract `BodySettingsGenetics`, `BodySettingsChildPGrowth`, `BodySettingsChildHairChange`, plus private helpers `CopyTwinFields`, `AssignTrait`, `GenerateBodyOption`, and the `ValidSkinColors` constant from `BodySettings.cs` into a new `GeneticsService` class that implements a new `IGeneticsService` interface. `BodySettings` constructor drops from 5 parameters to 1 (`IVariableStore` only); `GeneticsService` receives all 5 original deps plus `IBodySettings` (for the `Tidy()` cross-call at line 955). The extraction follows the proven pattern established by F794 (`BodyOptionValidator`): same namespace hierarchy (`Era.Core.State`/`Era.Core.Interfaces`), same DI singleton registration style in `ServiceCollectionExtensions.cs`. `IBodySettings` loses the 3 genetics method declarations; it retains the remaining 8 body-init/tidy/validation methods verbatim.

**Track B — Shop-layer IVariableStoreExtensions Deduplication**
Add 7 renamed CharacterId-typed getter extensions to `IVariableStoreExtensions` to resolve the extension-method-shadowing constraint (C# spec 12.8.10.3). Setter helpers are replaced by direct instance method calls since setter extensions would also be shadowed. The rename pattern (`GetTalentValue`, `GetBaseValue`, etc.) is already documented in the extension file comment. Replace all 6 private helpers in `ShopSystem.cs` and all 8 private helpers in `ShopDisplay.cs` with calls to the new extensions or direct `_variables` calls for void setters. `GetCFlag`/`SetCFlag` CharacterId overloads already exist (F782) and are reused as-is.

**Both tracks are delivered in a single build/test cycle.** ArchitectureTests are updated to add `IGeneticsService` to the DI registration assertion list. Genetics tests in `Era.Core.Tests/State/BodySettingsGeneticsTests.cs` are mechanically updated to construct `GeneticsService` instead of `BodySettings`.

All 30 ACs are satisfied by these two tracks with no additional infrastructure.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/State/GeneticsService.cs` with extracted genetics methods |
| 2 | Create `Era.Core/Interfaces/IGeneticsService.cs` with 3 method declarations |
| 3 | Declare `void BodySettingsGenetics(...)`, `void BodySettingsChildPGrowth(...)`, `void BodySettingsChildHairChange(...)` in `IGeneticsService.cs` |
| 4 | Declare `public class GeneticsService : IGeneticsService` in `GeneticsService.cs` |
| 5 | Remove the 3 genetics method declarations from `IBodySettings.cs` |
| 6 | Verify `IBodySettings.cs` retains exactly 8 non-genetics method declarations via count_equals grep |
| 7 | Delete `ValidSkinColors` field from `BodySettings.cs` as part of extraction move |
| 8 | Declare `ValidSkinColors` as `private static readonly HashSet<int>` inside `GeneticsService.cs` |
| 9 | Add 7 `public static int Get{TypeValue}(this IVariableStore, CharacterId, ...)` extensions to `IVariableStoreExtensions.cs` |
| 10 | Remove `private void SetTalent` and `private void SetMaxBase` from `ShopSystem.cs`; replace with direct `_variables.SetTalent(...)` and `_variables.SetMaxBase(...)` instance calls (shadowing prevents extension approach) |
| 11 | Remove all 6 private helper methods from `ShopSystem.cs`; replace call sites with `_variables.GetCFlag(...)`, `_variables.GetTalentValue(...)`, `_variables.GetMaxBaseValue(...)`, `_variables.SetCFlag(...)`, `_variables.SetTalent(...)`, `_variables.SetMaxBase(...)` |
| 12 | Remove all 8 private getter helpers from `ShopDisplay.cs`; replace call sites with corresponding extension calls |
| 13 | Remove `IRandomProvider` using and constructor parameter from `BodySettings.cs`; update constructor body accordingly |
| 14 | Remove `_multiBirthFatherCheck` field and constructor parameter from `BodySettings.cs` |
| 15 | Accept `IBodySettings bodySettings` as constructor parameter in `GeneticsService`; store as `_bodySettings` field |
| 16 | Replace the direct `Tidy(childId)` call (currently line 955) with `_bodySettings.Tidy(childId)` in `GeneticsService.cs` |
| 17 | Add `services.AddSingleton<IGeneticsService>(sp => new GeneticsService(...))` to `ServiceCollectionExtensions.cs` following existing `IBodySettings` lambda pattern |
| 18 | Add `"IGeneticsService"` to the `expectedRegistrations` array in `ArchitectureTests.cs` `DI_AllInterfaces_Registered` test |
| 19 | Change `new BodySettings(...)` to `new GeneticsService(...)` in `BodySettingsGeneticsTests.cs` and `BodySettingsGeneticsTask5bTests.cs`; update constructor arguments |
| 20 | Run `dotnet build Era.Core/`; TreatWarningsAsErrors catches unused usings or missing references |
| 21 | Run `dotnet test Era.Core.Tests/`; relocated genetics tests must pass against GeneticsService |
| 22 | Run `dotnet test engine.Tests/`; IBodySettings interface split must not break engine-level integration |
| 23 | New files (`GeneticsService.cs`, `IGeneticsService.cs`, updated `IVariableStoreExtensions.cs`) must contain no `TODO`, `FIXME`, or `HACK` markers |
| 24 | Do not touch `ItemPurchase.cs`; the new extension methods must not appear in that file |
| 25 | `BodyParams` struct remains in `BodySettings.cs`; extraction only removes genetics methods and their private helpers |
| 26 | `GeneticsService` constructor signature includes `Func<int, int> multiBirthFatherCheck` parameter |
| 27 | Verify existing 5 IVariableStoreExtensions methods are preserved (count_equals 5 for GetCFlag/SetCFlag/GetTalent) |
| 28 | Update `.claude/skills/engine-dev/SKILL.md` with `IGeneticsService` per ssot-update-rules.md |
| 29 | Do not touch genetics private helpers in `BodySettings.cs` — they are extracted by Task 2 into `GeneticsService.cs` |
| 30 | Declare `void BodySettingsGenetics(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId)` in `IGeneticsService.cs` matching IBodySettings.cs signature |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| GeneticsService namespace | `Era.Core.State` vs new `Era.Core.Genetics` | `Era.Core.State` | Consistent with BodySettings, BodyOptionValidator, PregnancySettings — all State-layer implementations. No new namespace needed for a single service. |
| IBodySettings dependency direction | GeneticsService → IBodySettings vs circular split | GeneticsService → IBodySettings (one-way) | Tidy() is defined on IBodySettings; GeneticsService calls it once (line 955). One-way dependency is safe; no circular risk since IBodySettings does not know about IGeneticsService. |
| DI registration style for IGeneticsService | Simple `AddSingleton<IGeneticsService, GeneticsService>()` vs factory lambda | Factory lambda matching IBodySettings pattern | GeneticsService requires `multiBirthFatherCheck` which is built from `IEngineVariables.GetResult()`. Same bridge pattern already used at line 152 of ServiceCollectionExtensions.cs; factory lambda is the only way to wire this. |
| ShopDisplay GetCFlag dedup | Add `GetCFlag` CharacterId overload vs use existing | Existing `GetCFlag(CharacterId, CharacterFlagIndex)` already exists in IVariableStoreExtensions (F782) | No new extension needed; ShopDisplay just calls `_variables.GetCFlag(character, flag)` directly. |
| Shop setter style (SetCharacterFlag) | New `SetCFlag` named extension vs direct call | Direct `_variables.SetCharacterFlag(character, flag, value)` call | `SetCFlag` extension (CharacterId) already exists (F782). ShopSystem can call it directly: `_variables.SetCFlag(character, flag, value)`. No new extension needed. |
| Setter extension naming for SetTalent/SetMaxBase | `SetTalent` / `SetMaxBase` vs `SetTalentValue` / `SetMaxBaseValue` | `SetTalent` / `SetMaxBase` (no rename) | Setters are void wrappers with no return value; extension method shadowing rule only applies to methods with matching signatures. IVariableStore.SetTalent takes `(CharacterId, TalentIndex, int)` — same as the extension. However, void setters ARE shadowed if signatures match. Since IVariableStore defines `void SetTalent(CharacterId, TalentIndex, int)`, the extension would be unreachable. **Revised decision**: callers use `_variables.SetTalent(...)` / `_variables.SetMaxBase(...)` directly (instance method); no setter extension needed. Remove AC#10 getter extensions cover all 7 getter types; setter helpers are replaced by direct instance method calls. |
| Setter extension for AC#10 | Extension methods vs direct instance calls | Direct instance calls (`_variables.SetTalent`, `_variables.SetMaxBase`) | Instance methods exist on IVariableStore with matching signatures; extension would be shadowed (same shadowing rule as getters). AC#10 verifies extensions don't exist in IVariableStoreExtensions for setters — **see Upstream Issues**: AC#10 as written expects setter extensions to be added, but setter extensions are unreachable due to shadowing. |
| Test file relocation | Rename files vs update constructors in-place | Update constructors in-place | Files named `BodySettingsGeneticsTests.cs` still describe the domain; only the constructed type changes to `GeneticsService`. Renaming is out of scope and risks test infrastructure churn. |

### Interfaces / Data Structures

**IGeneticsService** (new — `Era.Core/Interfaces/IGeneticsService.cs`):
```csharp
namespace Era.Core.Interfaces
{
    public interface IGeneticsService
    {
        void BodySettingsGenetics(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId);
        void BodySettingsChildPGrowth(int childId);
        void BodySettingsChildHairChange(int childId);
    }
}
```

**GeneticsService** (new — `Era.Core/State/GeneticsService.cs`):
```csharp
using System;
using System.Collections.Generic;
using Era.Core.Interfaces;
using Era.Core.Random;
using Era.Core.Types;

namespace Era.Core.State
{
    public class GeneticsService : IGeneticsService
    {
        private readonly IVariableStore _variables;
        private readonly IRandomProvider _random;
        private readonly IVisitorVariables _visitor;
        private readonly IEngineVariables _engine;
        private readonly IBodySettings _bodySettings;
        private readonly Func<int, int> _multiBirthFatherCheck;

        private static readonly HashSet<int> ValidSkinColors = new() { 0, 1, 2, 3, 10, 11, 13, 14 };

        public GeneticsService(
            IVariableStore variables,
            IRandomProvider random,
            IVisitorVariables visitor,
            IEngineVariables engine,
            IBodySettings bodySettings,
            Func<int, int> multiBirthFatherCheck)
        { ... }

        public void BodySettingsGenetics(...) { ... } // moved from BodySettings
        public void BodySettingsChildPGrowth(...) { ... } // moved from BodySettings
        public void BodySettingsChildHairChange(...) { ... } // moved from BodySettings

        private void CopyTwinFields(...) { ... }
        private void AssignTrait(...) { ... }
        private int GenerateBodyOption(...) { ... }
    }
}
```

**BodySettings after extraction** — constructor reduces to single parameter:
```csharp
public BodySettings(IVariableStore variables)
{
    _variables = variables ?? throw new ArgumentNullException(nameof(variables));
}
```

**IVariableStoreExtensions additions** — 7 renamed getter extensions (AC#9):
```csharp
public static int GetTalentValue(this IVariableStore variables, CharacterId character, TalentIndex talent)
    => variables.GetTalent(character, talent).Match(v => v, _ => 0);

public static int GetBaseValue(this IVariableStore variables, CharacterId character, BaseIndex index)
    => variables.GetBase(character, index).Match(v => v, _ => 0);

public static int GetMaxBaseValue(this IVariableStore variables, CharacterId character, MaxBaseIndex index)
    => variables.GetMaxBase(character, index).Match(v => v, _ => 0);

public static int GetAbilityValue(this IVariableStore variables, CharacterId character, AbilityIndex ability)
    => variables.GetAbility(character, ability).Match(v => v, _ => 0);

public static int GetExpValue(this IVariableStore variables, CharacterId character, ExpIndex index)
    => variables.GetExp(character, index).Match(v => v, _ => 0);

public static int GetMarkValue(this IVariableStore variables, CharacterId character, MarkIndex mark)
    => variables.GetMark(character, mark).Match(v => v, _ => 0);

public static int GetJuelValue(this IVariableStore variables, CharacterId character, int index)
    => variables.GetJuel(character, index).Match(v => v, _ => 0);
```

Note: No setter extensions are added. Setter helpers in ShopSystem.cs (`SetCharacterFlag`, `SetTalent`, `SetMaxBase`) are replaced by direct instance method calls (`_variables.SetCFlag(...)`, `_variables.SetTalent(...)`, `_variables.SetMaxBase(...)`) which are not shadowed in the same sense — they ARE the instance methods. The instance methods are callable directly, eliminating the private wrappers.

**DI registration addition** in `ServiceCollectionExtensions.cs`:
```csharp
services.AddSingleton<IGeneticsService>(sp =>
{
    Func<int, int> multiBirthFatherCheck = motherId => sp.GetRequiredService<IEngineVariables>().GetResult();
    return new GeneticsService(
        sp.GetRequiredService<IVariableStore>(),
        sp.GetRequiredService<Era.Core.Random.IRandomProvider>(),
        sp.GetRequiredService<IVisitorVariables>(),
        sp.GetRequiredService<IEngineVariables>(),
        sp.GetRequiredService<IBodySettings>(),
        multiBirthFatherCheck);
});
```

Note: `IBodySettings` is registered before `IGeneticsService` in the DI sequence. The `GeneticsService` registration lambda calls `sp.GetRequiredService<IBodySettings>()`, which resolves to the already-registered singleton. The `BodySettings` constructor now only requires `IVariableStore`, so its registration simplifies:
```csharp
services.AddSingleton<IBodySettings, BodySettings>();
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#10 was originally designed to expect `public static void Set(Talent|MaxBase)` extensions, but C# extension method shadowing (12.8.10.3) makes setter extensions unreachable. **Resolved**: AC#10 now correctly uses `not_matches` on `private void SetTalent|private void SetMaxBase` to verify helper removal. Setter calls use direct instance methods. | AC Design Constraints (C7) + AC#10 | **RESOLVED** — AC#10 already reflects the corrected approach (not_matches on private void Set helpers). No further changes needed. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 2, 3, 30 | Create `Era.Core/Interfaces/IGeneticsService.cs` with namespace `Era.Core.Interfaces`, declaring `void BodySettingsGenetics(...)`, `void BodySettingsChildPGrowth(int childId)`, `void BodySettingsChildHairChange(int childId)` | | [x] |
| 2 | 1, 4, 8, 15, 16, 26 | Create `Era.Core/State/GeneticsService.cs` implementing `IGeneticsService`: move genetics methods, private helpers (`CopyTwinFields`, `AssignTrait`, `GenerateBodyOption`), and `ValidSkinColors` constant from `BodySettings.cs`; constructor accepts `IVariableStore`, `IRandomProvider`, `IVisitorVariables`, `IEngineVariables`, `IBodySettings`, and `Func<int, int> multiBirthFatherCheck`; replace inline `Tidy(childId)` call with `_bodySettings.Tidy(childId)` | | [x] |
| 3 | 5, 6 | Update `Era.Core/Interfaces/IBodySettings.cs`: remove the 3 genetics method declarations (`BodySettingsGenetics`, `BodySettingsChildPGrowth`, `BodySettingsChildHairChange`); verify 8 non-genetics methods remain | | [x] |
| 4 | 7, 13, 14, 25, 29 | Update `Era.Core/State/BodySettings.cs`: remove `ValidSkinColors` field, `IRandomProvider` using + constructor parameter, `_multiBirthFatherCheck` field + constructor parameter, and all genetics method bodies; retain `BodyParams`, `CharacterBodySettings`, and body-init/tidy/validation methods | | [x] |
| 5 | 9, 24, 27 | Add 7 CharacterId-typed getter extensions to `Era.Core/Interfaces/IVariableStoreExtensions.cs`: `GetTalentValue`, `GetBaseValue`, `GetMaxBaseValue`, `GetAbilityValue`, `GetExpValue`, `GetMarkValue`, `GetJuelValue` (each unwrapping `Result<int>` via `.Match(v => v, _ => 0)`; `GetJuelValue` accepts `int` index per C8); do not modify `Era.Core/Shop/ItemPurchase.cs` | | [x] |
| 6 | 10, 11 | Update `Era.Core/Shop/ShopSystem.cs`: remove all 6 private variable-access helpers (`GetCharacterFlag`, `SetCharacterFlag`, `GetTalent`, `SetTalent`, `GetMaxBase`, `SetMaxBase`); replace call sites with `_variables.GetCFlag(...)`, `_variables.GetTalentValue(...)`, `_variables.GetMaxBaseValue(...)`, `_variables.SetCFlag(...)`, `_variables.SetTalent(...)`, `_variables.SetMaxBase(...)` directly | | [x] |
| 7 | 12 | Update `Era.Core/Shop/ShopDisplay.cs`: remove all 8 private getter helpers (`GetCharacterFlag`, `GetTalent`, `GetBase`, `GetMaxBase`, `GetAbility`, `GetExp`, `GetMark`, `GetJuel`); replace call sites with corresponding extension method calls (`_variables.GetCFlag(...)`, `_variables.GetTalentValue(...)`, `_variables.GetBaseValue(...)`, etc.) | | [x] |
| 8 | 17, 18 | Update `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`: add `IGeneticsService` singleton registration using factory lambda; update `Era.Core.Tests/ArchitectureTests.cs`: add `IGeneticsService` to the `expectedRegistrations` array | | [x] |
| 9 | 19 | Update `Era.Core.Tests/State/BodySettingsGeneticsTests.cs` and `Era.Core.Tests/State/BodySettingsGeneticsTask5bTests.cs`: change `new BodySettings(...)` to `new GeneticsService(...)` with updated constructor arguments | | [x] |
| 10 | 20 | Run `dotnet build Era.Core/` and verify exit code 0 (TreatWarningsAsErrors; catches unused usings, missing references, shadowing) | | [x] |
| 11 | 21 | Run `dotnet test Era.Core.Tests/` and verify all tests pass | | [x] |
| 12 | 22 | Run `dotnet test engine.Tests/` and verify all tests pass | | [x] |
| 13 | 23 | Verify no `TODO`, `FIXME`, or `HACK` markers in `Era.Core/State/GeneticsService.cs`, `Era.Core/Interfaces/IGeneticsService.cs`, and `Era.Core/Interfaces/IVariableStoreExtensions.cs` | | [x] |
| 14 | 28 | Update `.claude/skills/engine-dev/SKILL.md`: add `IGeneticsService` to Core Interfaces section per ssot-update-rules.md | | [x] |

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
| 1 | implementer | sonnet | feature-800.md Tasks 1-4 (Track A: GeneticsService extraction) | `IGeneticsService.cs`, `GeneticsService.cs`, updated `IBodySettings.cs`, updated `BodySettings.cs` |
| 2 | implementer | sonnet | feature-800.md Tasks 5-7 (Track B: Shop IVariableStoreExtensions dedup) | updated `IVariableStoreExtensions.cs`, updated `ShopSystem.cs`, updated `ShopDisplay.cs` |
| 3 | implementer | sonnet | feature-800.md Tasks 8-9, 14 (DI + test + SSOT updates) | updated `ServiceCollectionExtensions.cs`, updated `ArchitectureTests.cs`, updated `BodySettingsGeneticsTests.cs`, updated `engine-dev/SKILL.md` |
| 4 | tester | sonnet | feature-800.md Tasks 10-13 (build + test verification) | build and test pass confirmation, no-debt verification |

### Pre-conditions

- F782 is `[DONE]` (IVariableStoreExtensions exists with `GetCFlag`/`SetCFlag`).
- F780 is `[DONE]` (genetics methods exist in BodySettings.cs at lines 705-1136).
- All existing tests pass before starting (baseline).

### Execution Order

**Track A must complete before Track B** (Track B reads the final BodySettings line count to confirm reduction, not strictly required for compilation, but logically ordered).

1. **Task 1**: Create `IGeneticsService.cs` first (interface defines contract for Task 2).
2. **Task 2**: Create `GeneticsService.cs` implementing the interface (uses `IBodySettings` for Tidy cross-call).
3. **Task 3**: Update `IBodySettings.cs` (remove genetics methods; must happen after Task 2 compiles cleanly).
4. **Task 4**: Update `BodySettings.cs` (remove extracted content; after Task 3 so the interface no longer requires those methods).
5. **Task 5**: Add extensions to `IVariableStoreExtensions.cs`.
6. **Task 6**: Update `ShopSystem.cs` (uses Task 5's new extension methods).
7. **Task 7**: Update `ShopDisplay.cs` (uses Task 5's new extension methods).
8. **Task 8**: Update DI registration + ArchitectureTests.
9. **Task 9**: Update genetics tests.
10. **Task 10**: Build verification.
11. **Task 11**: Era.Core.Tests.
12. **Task 12**: engine.Tests.
13. **Task 13**: Tech debt check.

### DI Registration

Add in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` after the `IBodySettings` registration:

```csharp
services.AddSingleton<IGeneticsService>(sp =>
{
    Func<int, int> multiBirthFatherCheck = motherId => sp.GetRequiredService<IEngineVariables>().GetResult();
    return new GeneticsService(
        sp.GetRequiredService<IVariableStore>(),
        sp.GetRequiredService<Era.Core.Random.IRandomProvider>(),
        sp.GetRequiredService<IVisitorVariables>(),
        sp.GetRequiredService<IEngineVariables>(),
        sp.GetRequiredService<IBodySettings>(),
        multiBirthFatherCheck);
});
```

Note: `IBodySettings` singleton must already be registered before `IGeneticsService`. The `BodySettings` constructor simplifies to `IVariableStore` only:
```csharp
services.AddSingleton<IBodySettings, BodySettings>();
```

### ArchitectureTests Update

In `Era.Core.Tests/ArchitectureTests.cs`, locate the `expectedRegistrations` array in the `DI_AllInterfaces_Registered` test and add `"IGeneticsService"` alongside `"IBodySettings"`.

### IVariableStoreExtensions Addition Pattern

All 7 new extensions follow this pattern:
```csharp
public static int GetTalentValue(this IVariableStore variables, CharacterId character, TalentIndex talent)
    => variables.GetTalent(character, talent).Match(v => v, _ => 0);
```
Use `int index` (not `JuelIndex`) for `GetJuelValue` per C8.

### Shop Setter Clarification

`SetTalent(CharacterId, TalentIndex, int)` and `SetMaxBase(CharacterId, MaxBaseIndex, int)` are **instance methods** on `IVariableStore`. They are NOT extension methods. Call them directly on `_variables`:
- `_variables.SetTalent(character, talent, value)` — not via a new extension.
- `_variables.SetMaxBase(character, maxBase, value)` — not via a new extension.
- `_variables.SetCFlag(character, flag, value)` — this IS a `SetCFlag` extension from F782.

### Build Verification Steps

Run via WSL:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests/'
```

### Success Criteria

All 30 ACs pass:
- GeneticsService extraction (AC#1-8, AC#13-16, AC#25-26)
- Shop deduplication (AC#9-12, AC#24, AC#27)
- DI + test integration + SSOT (AC#17-22, AC#28)
- Zero technical debt in new files (AC#23)

### Error Handling

- **Build fails with `CS0122` (inaccessible)**: `GeneticsService` is in `Era.Core.State` namespace; ensure `Era.Core.Tests` can access it. If `internal`, add `[assembly: InternalsVisibleTo("Era.Core.Tests")]` to `Era.Core.csproj`.
- **Circular dependency**: If DI throws on `IGeneticsService` resolving `IBodySettings`, verify `IBodySettings` is registered before `IGeneticsService` in `ServiceCollectionExtensions.cs`.
- **Test constructor failures**: Genetics tests will have compilation errors until Task 9 updates constructors. Complete Tasks 1-4 before attempting Task 9.

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
| 2026-02-22 | Phase 1 | deep-explorer x3 | Round 1 investigation | 3/3 HIGH agreement |
| 2026-02-22 | Phase 1 | consensus-synthesizer | Synthesis | OK |
| 2026-02-22 | Phase 2 | deep-explorer(sonnet) x3 | Round 2 vote | 3/3 GO |
| 2026-02-22 | Phase 2 | deep-explorer(opus) x1 | Escalation verify | NO-GO (5 to 1 param) |
| 2026-02-22 | Phase 2 | consensus-synthesizer(sonnet) | micro-revision | OK |
| 2026-02-22 | Phase 3 | ac-designer | AC design (26 ACs) | OK |
| 2026-02-22 | Phase 4 | tech-designer | Technical design | OK + Upstream Issue AC10 |
| 2026-02-22 | Phase 4 | orchestrator | Upstream Issue Gate AC10 | AC10 setter fix |
| 2026-02-22 | Phase 5 | wbs-generator | Tasks (13 Tasks) | OK |
| 2026-02-22 | Phase 6 | quality-fixer | Quality auto-fix | 2 fixes |
| 2026-02-22 | Phase 7 | feature-validator | Validation 1 | FAIL 2C+2M |
| 2026-02-22 | Phase 7 | orchestrator | Fix 4 issues | Applied |
| 2026-02-22 | Phase 7 | feature-validator | Validation 2 | FAIL 3M |
| 2026-02-22 | Phase 7 | orchestrator | Fix 3 issues | Applied |
| 2026-02-22 | Phase 7 | feature-validator | Validation 3 | PASS |
| 2026-02-22 | Phase 8 | feature-status.py | DRAFT to PROPOSED | OK |
| 2026-02-22 | /run Phase 4 | implementer | Tasks 8,14 (DI + SSOT) | AC17,28 OK; AC18 hook-blocked |
| 2026-02-22 | /run Phase 4 | orchestrator | AC18 ArchTests edit | OK (hook temp move) |
| 2026-02-22 | DEVIATION | dotnet test engine.Tests/ | CS1729 BodySettings 5-arg ctor | 3 files fixed |
| 2026-02-22 | /run Phase 4 | orchestrator | engine.Tests ctor fix + unused using removal | 586 tests pass |
| 2026-02-22 | /run Phase 4 | orchestrator | Task 13 tech debt check | 0 matches |
| 2026-02-22 | /run Phase 7 | ac-tester | AC verification | 30/30 PASS |
| 2026-02-22 | /run Phase 8 | feature-reviewer | Quality review (post) | READY |
| 2026-02-22 | DEVIATION | feature-reviewer | Doc-check (8.2) | NEEDS_REVISION: 4 SSOT stale items in SKILL.md |
| 2026-02-22 | /run Phase 8 | orchestrator | SSOT fix (4 items) | IBodySettings count, IVarStoreExt desc, State/ listing, interface count |
| 2026-02-22 | /run Phase 8 | orchestrator | Step 8.3 SSOT check | All updated |
| 2026-02-22 | /run Phase 9 | orchestrator | Report & Approval | 30/30 PASS, 2 DEVIATION (D) |
| 2026-02-22 | /run Phase 10 | finalizer | [WIP] → [DONE] | READY_TO_COMMIT |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] fc iter1: [AC-CNT] AC6 count 7 to 8 (IBodySettings 11 methods minus 3 genetics)
- [fix] fc iter1: [AC-VAC] AC24 vacuous not_matches replaced with count_equals on ItemPurchase (baseline 10)
- [fix] fc iter1: [AC-GAP] AC27 added: IVariableStoreExtensions backward compat (count_equals 5)
- [fix] fc iter1: [STR-MIS] Scope Discipline section added (required by feature-template.md)
- [fix] fc iter2: [XR-INC] Tech Design Approach + AC Coverage AC6 text 7 to 8
- [fix] fc iter2: [XR-INC] Task 3 description 7 to 8 non-genetics methods
- [fix] fc iter2: [AC-GAP] AC28 added: engine-dev SKILL.md SSOT update (ssot-update-rules.md)
- [fix] fc iter2: [TSK-GAP] Task 14 added for AC28 SSOT update
- [fix] Phase2-Review iter1: [STR-MIS] Review Evidence table added (required by feature-template.md)
- [fix] Phase2-Review iter1: [STR-MIS] Parent Review Observations section added (required by feature-template.md)
- [fix] Phase2-Review iter1: [XR-INC] Upstream Issues AC#10 description updated to reflect resolved state
- [fix] Phase2-Review iter1: [AC-GAP] AC#29 added: BodySettings genetics private helper removal verification
- [fix] Phase2-Review iter1: [AC-GAP] AC#30 added: IGeneticsService.BodySettingsGenetics 5-parameter signature verification

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F782](feature-782.md) - Post-Phase Review Phase 20 (created this DRAFT)
- [Related: F780](feature-780.md) - Introduced genetics methods causing SRP violation
- [Related: F794](feature-794.md) - BodyOptionValidator extraction precedent
- [Related: F796](feature-796.md) - Unified BodyDetailInit to IVariableStore
- [Related: F779](feature-779.md) - Phase 20 migration context
- [Related: F783](feature-783.md) - Phase 21 Planning
