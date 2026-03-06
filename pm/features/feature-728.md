# Feature 728: Character Config Model Extension

## Status: [DONE]

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

## Background

### Philosophy (Mid-term Vision)
SSOT (Single Source of Truth) - Complete the CSV→YAML character data migration by extending CharacterConfig to support all fields present in archive CSV data, ensuring YAML files serve as the complete character initialization data source with full data fidelity. F727 created character YAML files but the CharacterConfig model (from F589) only supports core fields (CharacterId, Name, CallName, BaseStats, Abilities, Talents, Flags). Archive CSV files contain 2 additional fields (経験 Experience, 相性 Relation) that were lost during conversion.

### Problem (Current Issue)
CharacterConfig model does not support all CSV fields. Character YAML files created by F727 only contain core fields. Additional fields from archive CSV are not preserved in YAML format.

### Goal (What to Achieve)
1. Extend CharacterConfig model with Experience and Relation properties
2. Update 14 character YAML files with 経験/相性 data from archive CSV
3. Update KojoTestRunner ApplyCharacterConfig for new fields
4. Update CharacterLoaderTests for new field deserialization

## Links
- [feature-727.md](feature-727.md) - Parent feature (Character YAML Data and KojoTestRunner Migration)
- [feature-589.md](feature-589.md) - Original CharacterConfig model creation
- [feature-729.md](feature-729.md) - Game Runtime YAML Character Loading
- [feature-731.md](feature-731.md) - Character Data Structure Encapsulation
- [feature-706.md](feature-706.md) - Verification (KojoComparer Full Equivalence Verification)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why are character YAML files missing fields from archive CSV?**
   Because F727 converted archive CSV data to YAML using the CharacterConfig model, which only has 7 fields (CharacterId, Name, CallName, BaseStats, Abilities, Talents, Flags). Fields not in the model were silently dropped.

2. **Why does CharacterConfig only have 7 fields?**
   Because F589 designed CharacterConfig to match the fields present in Chara0.csv (the player character), which does not contain 経験 (Experience) or 相性 (Relation) entries. F589 did not audit all 18 archive CSV files to discover additional fields.

3. **Why didn't F589 include all possible CSV fields?**
   Because F589's scope was "CharacterConfig model and YamlCharacterLoader infrastructure" - it created the model from the CSV structure reference table which listed only the 7 core fields (番号, 名前, 呼び名, 基礎, 能力, 素質, フラグ). The engine's CharacterTemplate supports 13 fields total, but F589 only modeled the fields visible in the sample CSV.

4. **Why is the engine CharacterTemplate's full field set not documented in F589?**
   Because F589 focused on the YAML loader pattern (following F583 precedent) rather than on achieving full data fidelity with the engine's CharacterTemplate. The CSV structure analysis used a simplified mapping table.

5. **Why is full data fidelity important?**
   Because F729 (Game Runtime YAML Character Loading) will use CharacterConfig to initialize characters at game startup, replacing the CSV-based CharacterTemplate loading. If CharacterConfig lacks fields that CharacterTemplate has, characters loaded via YAML will have incomplete initialization compared to the original CSV path, causing runtime behavior differences.

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| Character YAML files are missing 経験 (Experience) and 相性 (Relation) data | CharacterConfig model was designed from incomplete CSV field survey (only Chara0.csv, which lacks these fields) |
| Background claims 8+ missing fields | Incorrect field count in F728 Background - only 2 additional fields (経験, 相性) exist in archive CSV data |

### Conclusion

The root cause is **incomplete field survey during F589 model design**. CharacterConfig was modeled from the core 7 CSV fields without cross-referencing the engine's CharacterTemplate class, which supports 13 fields. However, the actual data loss is much smaller than the Background claims:

**CRITICAL CORRECTION**: The Background states "8+ additional fields (経験, 相性, 刻印, あだ名, 主人の呼び方, 装着物, 宝珠, CSTR)" but archive CSV analysis shows only **2 additional fields** exist in the actual data:

1. **経験 (Experience)** - Present in 13 of 18 CSV files. Format: `経験,{index},{value}` (same Dictionary<int,int> pattern)
2. **相性 (Relation/Affinity)** - Present in 14 of 18 CSV files. Format: `相性,{characterId},{value}` (character relationship mapping)

The other 6 claimed fields (刻印 Mark, あだ名 Nickname, 主人の呼び方 Mastername, 装着物 Equip, 宝珠 Juel, CSTR) do **NOT appear** in any of the 18 archive CSV files. While the engine's CSV parser and CharacterTemplate support these fields, the game's actual character data does not use them.

Additionally, フラグ entries have semicolon-delimited comments (e.g., `フラグ,7,101;服装`) but these are metadata comments, not data values. The CSV parser extracts only the numeric value before the semicolon, and the current YAML correctly preserves `7: 101`. No data is lost.

**The Background section needs revision**: Change "8+ additional fields" to "2 additional fields (経験 Experience, 相性 Relation)".

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F589 | [DONE] | Created CharacterConfig model | Established 7-field model; did not include 経験/相性 |
| F727 | [DONE] | Created YAML data files | Converted 18 archive CSVs to YAML using F589 model; 経験/相性 data lost during conversion |
| F729 | [PROPOSED] | Downstream consumer | Game runtime YAML character loading will use CharacterConfig; needs complete data for correct initialization |
| F731 | [PROPOSED] | Downstream consumer | Character data encapsulation helper; will need to apply 経験/相性 fields via VariableCode.EXP and VariableCode.RELATION |
| F706 | [PROPOSED] | Verification | KojoComparer Full Equivalence Verification may detect 経験/相性 discrepancies |

### Pattern Analysis

This is a **data fidelity gap** pattern. F589 created the infrastructure, F727 created the data, but neither feature performed a complete field audit against the engine's CharacterTemplate. The gap is small (2 fields) and follows the common pattern of designing from a sample (Chara0.csv) rather than the complete dataset.

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Adding 2 Dictionary<int,int> properties to CharacterConfig is trivial; same pattern as existing BaseStats/Abilities/Talents/Flags |
| Scope is realistic | YES | ~50 lines model change + 18 YAML file updates + test updates; well within ~300 line limit |
| No blocking constraints | YES | No architectural constraints; YamlDotNet handles new Dictionary properties automatically |

**Verdict**: FEASIBLE

**Justification**:

1. **Minimal model change**: Adding `Experience` and `Relation` properties follows the exact same `Dictionary<int, int>` pattern as the existing 4 collection properties (BaseStats, Abilities, Talents, Flags).

2. **YamlDotNet compatibility**: The existing YamlCharacterLoader uses `DeserializerBuilder().Build()` which automatically maps YAML dictionary keys to C# Dictionary properties. No loader code changes needed - only the model needs new properties.

3. **YAML file updates are mechanical**: Each archive CSV file's 経験/相性 entries follow the same index-value format. Conversion is straightforward copy of data into YAML dictionary format.

4. **Backward compatible**: Existing YAML files without Experience/Relation fields will deserialize with empty dictionaries (default initialization). No breaking changes.

5. **Engine integration ready**: VariableCode.EXP and VariableCode.RELATION already exist in the engine. KojoTestRunner's ApplyCharacterConfig (or F731's helper) just needs additional foreach loops matching the existing pattern.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F589 | [DONE] | Created CharacterConfig model and YamlCharacterLoader |
| Predecessor | F727 | [DONE] | Created 18 YAML data files and KojoTestRunner integration |
| Related | F729 | [PROPOSED] | Game runtime YAML loading; will benefit from complete CharacterConfig |
| Related | F731 | [PROPOSED] | Character data encapsulation; ApplyCharacterConfig will need to handle new fields |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | NuGet package | LOW | Already used; new Dictionary properties auto-mapped |
| VariableCode.EXP | Engine enum | LOW | Stable; already used in CharacterData initialization |
| VariableCode.RELATION | Engine enum | LOW | Stable; already used in CharacterData initialization |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| Era.Core/Data/YamlCharacterLoader.cs | NONE | No changes needed; YamlDotNet's DeserializerBuilder automatically maps YAML dictionary keys to C# Dictionary properties by name |
| Era.Core.Tests/Data/CharacterLoaderTests.cs | LOW | Tests should verify new properties deserialize correctly |
| engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs | MEDIUM | ApplyCharacterConfig needs new foreach loops for Experience/Relation |
| Game/data/characters/*.yaml (18 files) | HIGH | All files need 経験/相性 data added from archive CSV |

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Data/Models/CharacterConfig.cs | Update | Add Experience and Relation Dictionary<int,int> properties |
| Era.Core.Tests/Data/CharacterLoaderTests.cs | Update | Add test for YAML with Experience/Relation fields |
| Game/data/characters/chara1.yaml | Update | Add Experience (5 entries) and Relation (1 entry) |
| Game/data/characters/chara2.yaml | Update | Add Experience (5 entries) and Relation (1 entry) |
| Game/data/characters/chara3.yaml | Update | Add Experience (8 entries) and Relation (2 entries) |
| Game/data/characters/chara4.yaml | Update | Add Experience (7 entries) and Relation (2 entries) |
| Game/data/characters/chara5.yaml | Update | Add Experience (1 entry) and Relation (2 entries) |
| Game/data/characters/chara6.yaml | Update | Add Experience (4 entries) and Relation (2 entries) |
| Game/data/characters/chara7.yaml | Update | Add Experience (3 entries) and Relation (2 entries) |
| Game/data/characters/chara9.yaml | Update | Add Experience (2 entries) and Relation (1 entry) |
| Game/data/characters/chara10.yaml | Update | Add Experience (1 entry) and Relation (3 entries) |
| Game/data/characters/chara13.yaml | Update | Add Experience (1 entry) and Relation (3 entries) |
| Game/data/characters/chara28.yaml | Update | Add Experience (6 entries) and Relation (1 entry) |
| Game/data/characters/chara29.yaml | Update | Add Experience (6 entries) and Relation (1 entry) |
| Game/data/characters/chara8.yaml | Update | Add Relation (1 entry) only (no Experience) |
| Game/data/characters/chara11.yaml | Update | Add Relation (2 entries) only (no Experience) |
| engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs | Update | Add Experience/Relation foreach loops to ApplyCharacterConfig |

**No changes needed** for: chara0.yaml, chara12.yaml, chara148.yaml, chara149.yaml (no 経験/相性 data in archive CSV)

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| CharacterConfig uses `init` properties with `Dictionary<int, int>` | F589 design pattern | LOW - New properties follow same pattern |
| YamlDotNet property naming must match YAML keys | Serialization convention | LOW - Use PascalCase property names matching YAML keys |
| KojoTestRunner.ApplyCharacterConfig is in engine project | Separate git repo | LOW - Standard engine modification |
| Relation uses characterId as key (not sequential index) | CSV format: `相性,{charId},{value}` | LOW - Dictionary<int,int> handles this naturally |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| YAML property names don't match expected format | LOW | MEDIUM | Follow existing convention (PascalCase: Experience, Relation); verify with unit test |
| ApplyCharacterConfig changes break existing kojo tests | LOW | HIGH | Run F181 regression tests after KojoTestRunner changes |
| F731 helper design conflicts with field additions | LOW | LOW | F728 and F731 are independent; F731 can add support for new fields incrementally |
| Archive CSV encoding issues during data extraction | LOW | LOW | Archive files are UTF-8 with BOM; Read tool handles correctly (verified during investigation) |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "support all CSV fields" (Goal 1) | CharacterConfig must have Experience property | AC#1, AC#2 |
| "support all CSV fields" (Goal 1) | CharacterConfig must have Relation property | AC#1, AC#3 |
| "complete data from archive CSV" (Goal 2) | 14 YAML files must contain Experience and/or Relation data | AC#4, AC#5 |
| "complete data from archive CSV" (Goal 2) | 4 YAML files without data remain unchanged | AC#6 |
| "Update YamlCharacterLoader tests" (Goal 3) | Tests verify new properties deserialize correctly | AC#8, AC#9 |
| "lost during conversion" (Philosophy) | KojoTestRunner must apply Experience/Relation fields | AC#7 |
| "correct initialization compared to original CSV path" (Root Cause) | Build and tests pass after all changes | AC#10, AC#11 |
| Zero Debt Upfront (Design Principle) | No tech debt markers in modified files | AC#12 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CharacterConfig has 9 Dictionary/string/int properties | code | Grep(Era.Core/Data/Models/CharacterConfig.cs, pattern="public .+ \\{ get; init; \\}") | count_equals | 9 | [x] |
| 2 | Experience property is Dictionary<int, int> with init accessor | code | Grep(Era.Core/Data/Models/CharacterConfig.cs) | matches | `Dictionary<int, int> Experience` | [x] |
| 3 | Relation property is Dictionary<int, int> with init accessor | code | Grep(Era.Core/Data/Models/CharacterConfig.cs) | matches | `Dictionary<int, int> Relation` | [x] |
| 4 | YAML files with Experience data contain Experience key | code | Grep(Game/data/characters/chara1.yaml,Game/data/characters/chara2.yaml,Game/data/characters/chara3.yaml,Game/data/characters/chara4.yaml,Game/data/characters/chara5.yaml,Game/data/characters/chara6.yaml,Game/data/characters/chara7.yaml,Game/data/characters/chara9.yaml,Game/data/characters/chara10.yaml,Game/data/characters/chara13.yaml,Game/data/characters/chara28.yaml,Game/data/characters/chara29.yaml) | count_equals | 12 | [x] |
| 5 | YAML files with Relation data contain Relation key | code | Grep(Game/data/characters/chara1.yaml,Game/data/characters/chara2.yaml,Game/data/characters/chara3.yaml,Game/data/characters/chara4.yaml,Game/data/characters/chara5.yaml,Game/data/characters/chara6.yaml,Game/data/characters/chara7.yaml,Game/data/characters/chara8.yaml,Game/data/characters/chara9.yaml,Game/data/characters/chara10.yaml,Game/data/characters/chara11.yaml,Game/data/characters/chara13.yaml,Game/data/characters/chara28.yaml,Game/data/characters/chara29.yaml) | count_equals | 14 | [x] |
| 6 | Unchanged YAML files have no Experience or Relation | code | Grep(Game/data/characters/chara0.yaml,Game/data/characters/chara12.yaml,Game/data/characters/chara148.yaml,Game/data/characters/chara149.yaml) | not_contains | "Experience:" | [x] |
| 6b | Unchanged YAML files have no Relation key | code | Grep(Game/data/characters/chara0.yaml,Game/data/characters/chara12.yaml,Game/data/characters/chara148.yaml,Game/data/characters/chara149.yaml) | not_contains | "Relation:" | [x] |
| 7 | KojoTestRunner applies Experience and Relation in ApplyCharacterConfig | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs) | matches | `foreach.*config\\.Experience` | [x] |
| 7b | KojoTestRunner applies Relation in ApplyCharacterConfig | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs) | matches | `foreach.*config\\.Relation` | [x] |
| 8 | Unit test verifies Experience deserialization | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~CharacterLoaderTests | succeeds | - | [x] |
| 9 | Unit test for missing Experience/Relation defaults to empty | code | Grep(Era.Core.Tests/Data/CharacterLoaderTests.cs) | contains | "Experience" | [x] |
| 10 | Era.Core project builds successfully | build | dotnet build Era.Core | succeeds | - | [x] |
| 11 | All Era.Core.Tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 12 | No tech debt in modified files | code | Grep(Era.Core/Data/Models/CharacterConfig.cs,engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs,Era.Core.Tests/Data/CharacterLoaderTests.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |

### AC Details

**AC#1: CharacterConfig has 9 properties total**
- Test: Grep pattern=`public .+ { get; init; }` path=Era.Core/Data/Models/CharacterConfig.cs | count
- Expected: 9 matches (3 scalar: CharacterId, Name, CallName + 4 existing Dictionary: BaseStats, Abilities, Talents, Flags + 2 new Dictionary: Experience, Relation)
- Verifies: All properties declared with `init` accessor pattern

**AC#2: Experience property is Dictionary<int, int> with init accessor**
- Test: Grep pattern=`Dictionary<int, int> Experience` path=Era.Core/Data/Models/CharacterConfig.cs
- Expected: Matches line like `public Dictionary<int, int> Experience { get; init; } = new();`
- Verifies: Same pattern as existing BaseStats/Abilities/Talents/Flags properties
- Edge case: Property must use `init` accessor (not `set`) for immutability consistency

**AC#3: Relation property is Dictionary<int, int> with init accessor**
- Test: Grep pattern=`Dictionary<int, int> Relation` path=Era.Core/Data/Models/CharacterConfig.cs
- Expected: Matches line like `public Dictionary<int, int> Relation { get; init; } = new();`
- Verifies: Same pattern as existing properties; uses `Relation` (not `Affinity` or `Compatibility`)

**AC#4: YAML files with Experience data contain Experience key**
- Test: Grep pattern=`^Experience:` across 12 files that have Experience data in archive CSV
- Expected: 12 matches (one per file: chara1,2,3,4,5,6,7,9,10,13,28,29)
- Note: chara8 and chara11 have Relation but no Experience. chara0,12,148,149 have neither.

**AC#5: YAML files with Relation data contain Relation key**
- Test: Grep pattern=`^Relation:` across 14 files that have Relation data in archive CSV
- Expected: 14 matches (one per file: chara1,2,3,4,5,6,7,8,9,10,11,13,28,29)
- Note: All 14 files listed in Impact Analysis that need Relation data

**AC#6: Unchanged YAML files have no Experience or Relation**
- Test: Grep pattern=`Experience:` path=Game/data/characters/chara0.yaml,chara12.yaml,chara148.yaml,chara149.yaml
- Expected: 0 matches (not_contains)
- Verifies: Files without archive CSV data are not modified with empty sections
- Also verify: Grep pattern=`Relation:` same files → 0 matches (not_contains)

**AC#7: KojoTestRunner applies Experience and Relation in ApplyCharacterConfig**
- Test: Grep pattern=`config\.Experience` path=engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs
- Expected: At least 1 match showing foreach loop over config.Experience
- Also verify: Grep pattern=`config\.Relation` → at least 1 match
- Pattern should follow existing structure: foreach with VariableCode.EXP and VariableCode.RELATION respectively
- Edge case: Bounds checking must match existing pattern (null check + key < array.Length)
- Additional verification: Grep pattern=`expArray != null && kvp.Key < expArray.Length` → at least 1 match
- Additional verification: Grep pattern=`relationArray != null && kvp.Key < relationArray.Length` → at least 1 match

**AC#8: Unit test verifies Experience deserialization**
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~CharacterLoaderTests
- Expected: All tests pass including new test(s) for Experience/Relation deserialization
- New test should create YAML with Experience and Relation sections and verify they deserialize to correct Dictionary<int,int> values

**AC#9: Unit test for missing Experience/Relation defaults to empty**
- Test: Grep pattern=`Experience` path=Era.Core.Tests/Data/CharacterLoaderTests.cs
- Expected: Contains reference to Experience property in test code
- Verifies: Existing Load_ValidYamlFile test (which uses YAML without Experience/Relation) still passes, proving backward compatibility with empty default dictionaries
- Negative test: Existing test YAML has no Experience/Relation → config.Experience should be empty dictionary

**AC#10: Era.Core project builds successfully**
- Test: dotnet build Era.Core
- Expected: Build succeeds with zero errors (TreatWarningsAsErrors=true)
- Verifies: New properties don't introduce build warnings or errors

**AC#11: All Era.Core.Tests pass**
- Test: dotnet test Era.Core.Tests
- Expected: All tests pass (both existing and new)
- Verifies: No regression from model changes

**AC#12: No tech debt in modified files**
- Test: Grep pattern=`TODO|FIXME|HACK` across CharacterConfig.cs, KojoTestRunner.cs, CharacterLoaderTests.cs
- Expected: 0 matches
- Covers all directories where this feature modifies files: Era.Core/Data/Models/, engine/Assets/Scripts/Emuera/Headless/, Era.Core.Tests/Data/

---

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Add Experience and Relation properties to CharacterConfig model | [x] |
| 2 | 4,5,6,6b | Update 14 character YAML files with Experience/Relation data from archive CSV | [x] |
| 3 | 7,7b | Add Experience/Relation foreach loops to KojoTestRunner.ApplyCharacterConfig | [x] |
| 4 | 8,9 | Add unit test for Experience/Relation deserialization | [x] |
| 5 | 10,11 | Build Era.Core and run all tests | [x] |
| 6 | 12 | Verify no technical debt markers in modified files | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | CharacterConfig pattern from Technical Design | Model extension with 2 new properties |
| 2 | implementer | sonnet | T2 | Archive CSV data + YAML format from Technical Design | 14 updated YAML files |
| 3 | implementer | sonnet | T3 | KojoTestRunner pattern from Technical Design | Runtime integration with foreach loops |
| 4 | implementer | sonnet | T4 | Unit test pattern from Technical Design | New test method + helper |
| 5 | ac-tester | haiku | T5 | Build/test commands from AC#10-11 | Build and test verification |
| 6 | ac-tester | haiku | T6 | Grep pattern from AC#12 | Tech debt verification |

**Constraints**:
1. CharacterConfig properties must use `init` accessor and `= new()` default initialization (matches existing pattern)
2. YAML property names must be PascalCase: `Experience`, `Relation` (YamlDotNet convention)
3. KojoTestRunner foreach loops must include null check and bounds checking (matches existing pattern for BaseStats/Abilities/Talents/Flags)
4. Only update 14 YAML files that have archive CSV data; do not modify chara0.yaml, chara12.yaml, chara148.yaml, chara149.yaml
5. No loader changes needed - YamlDotNet auto-deserializes new Dictionary properties

**Pre-conditions**:
- F589 CharacterConfig model exists with 7 properties (CharacterId, Name, CallName, BaseStats, Abilities, Talents, Flags) - current state before extension
- F727 created 18 character YAML files in Game/data/characters/
- Archive CSV files are available for data extraction reference (via Read tool on Game/data/characters/*.yaml and comparison with original CSV patterns)
- KojoTestRunner.ApplyCharacterConfig exists with foreach loops for BaseStats/Abilities/Talents/Flags (around line 1050-1067)

**Success Criteria**:
- All 12 ACs pass
- CharacterConfig has 9 properties total (7 existing + 2 new) - final state after extension
- 14 YAML files updated, 4 files unchanged
- KojoTestRunner has 6 foreach loops total (4 existing + 2 new)
- Unit tests include new test for Experience/Relation deserialization
- Zero build warnings or errors
- Zero technical debt markers in modified files

**Rollback Plan**:

If issues arise after implementation:
1. Revert commit with `git revert`
2. Notify user of rollback and specific failure (model deserialization error, test failure, KojoTestRunner runtime error)
3. Create follow-up feature for fix with investigation of root cause (e.g., YAML format mismatch, VariableCode array bounds issue)

**Data Extraction Reference**:

Archive CSV data for Experience/Relation is documented in Root Cause Analysis and Impact Analysis sections. Implementer should reference the Impact Analysis table for exact entry counts per character file.

**Pattern Examples**:

Model Extension:
```csharp
/// <summary>Experience collection (経験): index → value</summary>
public Dictionary<int, int> Experience { get; init; } = new();

/// <summary>Character relation/affinity collection (相性): characterId → value</summary>
public Dictionary<int, int> Relation { get; init; } = new();
```

KojoTestRunner Integration:
```csharp
// Set Experience (経験)
foreach (var kvp in config.Experience)
{
    var expArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.EXP)];
    if (expArray != null && kvp.Key < expArray.Length)
        expArray[kvp.Key] = kvp.Value;
}

// Set Relation (相性)
foreach (var kvp in config.Relation)
{
    var relationArray = chara.DataIntegerArray[(int)(VariableCode.__LOWERCASE__ & VariableCode.RELATION)];
    if (relationArray != null && kvp.Key < relationArray.Length)
        relationArray[kvp.Key] = kvp.Value;
}
```

## Deferred Items

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-01 | AC Verification | All 12 ACs verified and PASS: AC#1-3 (CharacterConfig properties), AC#4-6 (YAML data), AC#7-7b (KojoTestRunner integration), AC#8-9 (Unit tests), AC#10-11 (Build/Tests), AC#12 (Tech debt). Model extension complete: 9 properties found (3 scalar + 6 Dictionary). YAML updates: 12 files with Experience, 14 files with Relation. KojoTestRunner: foreach loops for Experience and Relation at lines 1069-1077. CharacterLoaderTests: new test for deserialization added. Build: 0 warnings, 0 errors. Tests: 1596 pass, 0 fail. Tech debt: 0 TODO/FIXME/HACK found. |

---

## Technical Design

### Approach

**Pattern Replication Strategy**: Extend CharacterConfig with 2 new Dictionary<int, int> properties following the exact same pattern as existing collection properties (BaseStats, Abilities, Talents, Flags). This ensures design consistency and leverages YamlDotNet's automatic deserialization.

This approach satisfies all ACs through:
1. **Model Extension (AC#1-3)**: Add `Experience` and `Relation` properties with `init` accessors
2. **Data Migration (AC#4-6)**: Update 14 YAML files with archive CSV data, leave 4 files unchanged
3. **Runtime Integration (AC#7)**: Extend KojoTestRunner.ApplyCharacterConfig with foreach loops for new properties
4. **Test Coverage (AC#8-9)**: Add unit test for new property deserialization and verify backward compatibility
5. **Build Validation (AC#10-12)**: Ensure zero warnings/errors and no tech debt

**Key Insight**: No loader changes needed. YamlDotNet's `DeserializerBuilder().Build()` automatically maps YAML dictionary sections to C# Dictionary properties based on property name matching.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `public Dictionary<int, int> Experience { get; init; } = new();` and `public Dictionary<int, int> Relation { get; init; } = new();` to CharacterConfig.cs after Flags property |
| 2 | Experience property declaration uses exact pattern: `Dictionary<int, int>` type with `init` accessor and `new()` default initialization |
| 3 | Relation property declaration uses exact pattern: `Dictionary<int, int>` type with `init` accessor and `new()` default initialization |
| 4 | Add `Experience:` section to 12 YAML files (chara1,2,3,4,5,6,7,9,10,13,28,29) with index-value pairs from archive CSV 経験 entries |
| 5 | Add `Relation:` section to 14 YAML files (chara1,2,3,4,5,6,7,8,9,10,11,13,28,29) with characterId-value pairs from archive CSV 相性 entries |
| 6 | Do not modify chara0.yaml, chara12.yaml, chara148.yaml, chara149.yaml (no 経験/相性 data in archive CSV) |
| 7 | Add two foreach loops in KojoTestRunner.ApplyCharacterConfig after Flags loop: one for `config.Experience` → `VariableCode.EXP` array, one for `config.Relation` → `VariableCode.RELATION` array |
| 8 | Add `Load_YamlWithExperienceAndRelation_DeserializesCorrectly()` test to CharacterLoaderTests.cs with test YAML containing both new sections |
| 9 | Verify existing `Load_ValidYamlFile_ReturnsSuccess()` test still passes (backward compatibility - YAML without Experience/Relation defaults to empty dictionaries) |
| 10 | Run `dotnet build Era.Core` after model changes; TreatWarningsAsErrors=true ensures zero warnings |
| 11 | Run `dotnet test Era.Core.Tests` after test additions; all tests must pass |
| 12 | Grep for `TODO\|FIXME\|HACK` in modified files returns zero matches |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Property Type | `Dictionary<int, int>` vs `List<KeyValuePair>` vs custom class | `Dictionary<int, int>` | Matches existing pattern for BaseStats/Abilities/Talents/Flags; YamlDotNet auto-maps YAML dictionaries to C# Dictionary type |
| Property Naming | `Experience`/`Relation` vs `Exp`/`Compatibility` vs `経験`/`相性` | `Experience`/`Relation` | English property names match existing convention (BaseStats, Abilities); full word "Experience" is clearer than abbreviation; "Relation" is concise (vs "Relationship" or "Affinity") |
| Default Initialization | `= new()` vs nullable `Dictionary<int, int>?` | `= new()` | Matches existing pattern; prevents null reference exceptions; empty dictionary is semantically correct for "no data" |
| Loader Changes | Update YamlCharacterLoader vs no changes | No changes | YamlDotNet's deserializer automatically maps YAML keys to property names; adding properties is sufficient |
| YAML File Updates | Batch script vs manual edit | Manual edit (14 files) | Only 14 files need updates; manual ensures data accuracy from archive CSV; low risk of automation errors |
| Test Strategy | New test vs extend existing test | Add new test `Load_YamlWithExperienceAndRelation_DeserializesCorrectly()` | Existing test verifies backward compatibility (YAML without new fields); new test explicitly verifies new property deserialization |
| KojoTestRunner Location | Add to existing method vs new method | Add to existing `ApplyCharacterConfig` method | Follows F727 pattern; all CharacterConfig field application is in one method; maintains cohesion |

### Interfaces / Data Structures

**CharacterConfig Model Extension** (Era.Core/Data/Models/CharacterConfig.cs):

```csharp
/// <summary>Strongly typed configuration for character initialization data</summary>
public class CharacterConfig
{
    /// <summary>Character unique identifier (番号)</summary>
    public int CharacterId { get; init; }

    /// <summary>Character display name (名前)</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Character nickname/callname (呼び名)</summary>
    public string CallName { get; init; } = string.Empty;

    /// <summary>Base stats collection (基礎): index → value</summary>
    public Dictionary<int, int> BaseStats { get; init; } = new();

    /// <summary>Abilities collection (能力): index → value</summary>
    public Dictionary<int, int> Abilities { get; init; } = new();

    /// <summary>Talents collection (素質): index → value</summary>
    public Dictionary<int, int> Talents { get; init; } = new();

    /// <summary>Flags collection (フラグ): index → value</summary>
    public Dictionary<int, int> Flags { get; init; } = new();

    // NEW PROPERTIES
    /// <summary>Experience collection (経験): index → value</summary>
    public Dictionary<int, int> Experience { get; init; } = new();

    /// <summary>Character relation/affinity collection (相性): characterId → value</summary>
    public Dictionary<int, int> Relation { get; init; } = new();
}
```

**YAML Data Format** (example from chara1.yaml):

```yaml
CharacterId: 1
Name: 紅美鈴
CallName: 美鈴
BaseStats:
  0: 2500
  1: 2300
  # ... existing data ...
Flags:
  7: 101
  310: 300
  # ... existing data ...
# NEW SECTIONS
Experience:
  1: 30
  10: 5
  22: 20
  51: 50
  53: 1
Relation:
  4: 200
```

**KojoTestRunner.ApplyCharacterConfig Extension** (engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs):

Add after the Flags foreach loop (currently around line 1067):

```csharp
// Set Experience (経験)
foreach (var kvp in config.Experience)
{
    var expArray = chara.DataIntegerArray[(int)VariableCode.EXP];
    if (expArray != null && kvp.Key < expArray.Length)
        expArray[kvp.Key] = kvp.Value;
}

// Set Relation (相性)
foreach (var kvp in config.Relation)
{
    var relationArray = chara.DataIntegerArray[(int)VariableCode.RELATION];
    if (relationArray != null && kvp.Key < relationArray.Length)
        relationArray[kvp.Key] = kvp.Value;
}
```

**Edge Cases Handled**:
- **Empty dictionaries**: Properties default to `new()` so YAML files without Experience/Relation sections deserialize successfully (backward compatibility)
- **Null array protection**: foreach loops check `!= null` before accessing array (matches existing pattern)
- **Bounds checking**: `kvp.Key < array.Length` prevents out-of-bounds access (matches existing pattern)
- **YAML key format**: Dictionary<int, int> handles both sequential indices (Experience: index → value) and non-sequential keys (Relation: characterId → value)

**Unit Test Addition** (Era.Core.Tests/Data/CharacterLoaderTests.cs):

```csharp
[Fact]
public void Load_YamlWithExperienceAndRelation_DeserializesCorrectly()
{
    // Arrange
    var loader = Services.GetRequiredService<ICharacterLoader>();
    var testYaml = CreateYamlWithExperienceAndRelation();

    try
    {
        // Act
        var result = loader.Load(testYaml);

        // Assert
        var config = ResultAssert.AssertSuccess(result);
        Assert.Equal(1, config.CharacterId);
        Assert.Equal("紅美鈴", config.Name);

        // Verify Experience deserialization
        Assert.Equal(5, config.Experience.Count);
        Assert.Equal(30, config.Experience[1]);
        Assert.Equal(5, config.Experience[10]);
        Assert.Equal(20, config.Experience[22]);
        Assert.Equal(50, config.Experience[51]);
        Assert.Equal(1, config.Experience[53]);

        // Verify Relation deserialization
        Assert.Single(config.Relation);
        Assert.Equal(200, config.Relation[4]);
    }
    finally
    {
        if (System.IO.File.Exists(testYaml))
        {
            System.IO.File.Delete(testYaml);
        }
    }
}

private string CreateYamlWithExperienceAndRelation()
{
    var testYamlPath = GetTestDataPath("character_exp_relation_test.yaml");
    var yamlContent = @"
CharacterId: 1
Name: 紅美鈴
CallName: 美鈴
Experience:
  1: 30
  10: 5
  22: 20
  51: 50
  53: 1
Relation:
  4: 200
";
    System.IO.File.WriteAllText(testYamlPath, yamlContent);
    return testYamlPath;
}
```

**Backward Compatibility Verification**: The existing `Load_ValidYamlFile_ReturnsSuccess()` test uses YAML without Experience/Relation. After model changes, this test will still pass because:
1. YamlDotNet's deserializer ignores missing properties (doesn't fail on absent YAML keys)
2. Properties default to `new()` so `config.Experience` and `config.Relation` are empty dictionaries (not null)
3. This proves backward compatibility - old YAML files work with new model

### Implementation Sequence

1. **Model Extension**: Add 2 properties to CharacterConfig.cs (2 lines)
2. **Test Addition**: Add new unit test + helper method to CharacterLoaderTests.cs (~50 lines)
3. **Verify Backward Compatibility**: Run existing tests to confirm old YAML files still load
4. **YAML Data Migration**: Update 14 character YAML files with Experience/Relation sections from archive CSV (~140 lines total, ~10 lines per file average)
5. **KojoTestRunner Extension**: Add 2 foreach loops to ApplyCharacterConfig (~16 lines)
6. **Build Verification**: `dotnet build Era.Core` + `dotnet test Era.Core.Tests`
7. **Tech Debt Check**: Grep for TODO/FIXME/HACK in modified files

**Total LOC Estimate**: ~210 lines (well within 300 line guideline for engine type features)
