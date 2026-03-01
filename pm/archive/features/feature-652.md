# Feature 652: KojoComparer Test YAML Migration

## Status: [DONE]

## Type: infra

## Created: 2026-01-28

---

## Summary

Migrate test YAML files to new format AND extend YamlDialogueLoader to support Priority/Condition deserialization required by the new format.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion quality validation - KojoComparer tests must pass for full equivalence verification.

### Problem (Current Issue)

F651 updated YamlRunner.cs to use YamlDialogueLoader (public class) instead of LegacyYamlDialogueLoader (internal class). However, test YAML files in tools/KojoComparer.Tests still use the old format (branches: structure) which is only compatible with LegacyYamlDialogueLoader.

This causes 4 test failures in KojoComparer.Tests.

### Goal (What to Achieve)

1. Extend YamlDialogueLoader to support Priority/Condition deserialization
2. Migrate test YAML files to new format compatible with YamlDialogueLoader
3. All KojoComparer.Tests pass

---

## Links

- [feature-651.md](feature-651.md) - KojoComparer KojoEngine API Update (predecessor, origin of this issue)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (related)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 4 unit tests in PilotEquivalenceTests fail when running `dotnet test tools/KojoComparer.Tests`
2. Why: YamlRunner.Render() throws `InvalidOperationException: Failed to get dialogue: YamlDialogueLoader: File not found: ...\1_美鈴\COM_0.yaml`
3. Why: KojoEngine.GetPath() constructs path as `{basePath}/1_美鈴/COM_0.yaml` using CharacterFolderMap, but the actual test file is a flat file at `tools/ErbToYaml.Tests/TestOutput/meirin_com0.yaml` (no subdirectory structure)
4. Why: F651 changed YamlRunner to use the new KojoEngine DI API, which delegates file loading to KojoEngine.GetPath() instead of loading the file directly by the provided path
5. Why: The test YAML file uses old conventions (flat naming `meirin_com0.yaml`, old `branches:` schema) that are incompatible with KojoEngine's expectations (directory structure `N_CharacterName/COM_NNN.yaml`, new `entries:` schema with `id`/`content` fields)

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 4 test failures with "File not found" error | Test YAML data has TWO incompatibilities with the new KojoEngine pipeline: (1) file path/directory structure mismatch, and (2) YAML schema mismatch (`branches:`+`lines:`+`condition:` vs `entries:`+`id:`+`content:`+`condition:`+`priority:`) |

### Conclusion

The problem described in Background is partially correct but understates the scope. The root cause is not just a YAML format mismatch -- it is a dual incompatibility:

1. **Path structure**: Test file is flat (`meirin_com0.yaml` in `TestOutput/`) but KojoEngine.GetPath() expects directory structure (`TestOutput/1_美鈴/COM_0.yaml`). This is the IMMEDIATE cause of the "File not found" error.

2. **YAML schema**: Even if the path issue were fixed, the test YAML uses `branches:` with `lines:` arrays and nested `condition:` (old ErbToYaml output format), while YamlDialogueLoader expects `entries:` with `id:` and `content:` string fields (new DialogueFile schema). The `branches:` format would silently fail to deserialize.

Note: The LegacyYamlDialogueLoader (in Era.Core.Tests/Helpers/) handles the `entries:`+`lines:`+`condition:` format (a hybrid), NOT the `branches:` format. So even that loader would not work with the current test files. The `branches:` format is an even older format from ErbToYaml output.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F651 | [BLOCKED] | Origin/Blocked by this | F651 is BLOCKED until F652 resolves AC#12 |
| F553 | [DONE] | API change source | KojoEngine Facade Refactoring - changed constructor API |
| F549 | [DONE] | Defines new format | YamlDialogueLoader with `entries:`+`id:`+`content:` schema |
| F644 | [DRAFT] | Downstream consumer | Equivalence Testing Framework depends on working KojoComparer |
| F636-F643 | Various | Blocked downstream | Kojo conversion features depend on KojoComparer for equivalence verification |

### Pattern Analysis

This is a cascading format migration gap. F553 refactored KojoEngine API, F549 defined new YamlDialogueLoader with `entries:` schema, and F651 updated KojoComparer to use the new API. However, none of these features migrated the test YAML data from the oldest `branches:` format to the new `entries:` format. The Era.Core.Tests had a similar issue but was addressed by creating the LegacyYamlDialogueLoader helper. The KojoComparer.Tests test data was never updated.

Additionally, the production YAML files in `Game/YAML/Kojo/` ALSO use the `branches:` format, meaning this format migration gap extends beyond just test data.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Both incompatibilities can be fixed: restructure test files into directory format AND convert YAML schema from `branches:` to `entries:` |
| Scope is realistic | YES | Only 2 YAML files need migration (`meirin_com0.yaml`, `meirin_com0_render.yaml`), plus updating test code to reference new paths |
| No blocking constraints | YES | No external dependencies block this work |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F553 | [DONE] | KojoEngine API defines the target interface |
| Predecessor | F549 | [DONE] | YamlDialogueLoader defines the target YAML schema |
| Successor | F651 | [BLOCKED] | Will unblock F651 AC#12 |
| Related | F644 | [DRAFT] | Equivalence Testing Framework - downstream |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | Runtime | Low | Already in use, no version change needed |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer.Tests/PilotEquivalenceTests.cs | HIGH | 4 unit tests directly reference `meirin_com0.yaml` path and expect branch-specific dialogue content |
| tools/KojoComparer.Tests/YamlRunnerTests.cs | LOW | 2 integration tests (Skip'd) reference same file path |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Dialogue/Loading/YamlDialogueLoader.cs | Extend | Add DialogueConditionData class, Priority/Condition properties to DialogueEntryData, ConvertCondition() method |
| tools/ErbToYaml.Tests/TestOutput/meirin_com0.yaml | Rewrite | Convert from `branches:` to `entries:` schema with proper `id`/`content`/`priority`/`condition` fields |
| tools/ErbToYaml.Tests/TestOutput/meirin_com0_render.yaml | Rewrite | Same format migration |
| tools/ErbToYaml.Tests/TestOutput/ (directory structure) | Create | Add `1_美鈴/COM_0.yaml` subdirectory to match KojoEngine.GetPath() expectation |
| tools/KojoComparer.Tests/PilotEquivalenceTests.cs | Update | Update YamlFilePath to point to new directory-structured file path |
| tools/KojoComparer.Tests/YamlRunnerTests.cs | Update | Update yamlFilePath references to new path (Skip'd tests, low priority) |
| tools/KojoComparer/YamlRunner.cs | Potential Update | ParseYamlPath test format regex may need updating for new path convention |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| YamlDialogueLoader only reads `entries:` with `id:`/`content:` fields | Era.Core/Dialogue/Loading/YamlDialogueLoader.cs | HIGH - test YAML MUST use this exact schema or YamlDialogueLoader returns empty entries |
| KojoEngine.GetPath() requires `{basePath}/{N_CharacterName}/COM_{NNN}.yaml` directory structure | Era.Core/KojoEngine.cs line 91-95 | HIGH - test data must be reorganized into this directory structure |
| YamlDialogueLoader uses CamelCaseNamingConvention | YamlDialogueLoader.cs line 29-31 | MEDIUM - YAML keys must be camelCase (`entries`, `id`, `content`) |
| PriorityDialogueSelector requires non-null Condition or fallback entry | PriorityDialogueSelector.cs | MEDIUM - entries without conditions need Priority=0 as fallback |
| DialogueEntry.Condition uses DialogueCondition record (Type/TalentType/Threshold) | DialogueFile.cs line 45 | MEDIUM - conditions must be converted from old `TALENT: {16: {ne: 0}}` to new DialogueCondition format |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| YamlDialogueLoader cannot parse condition fields from YAML | Medium | High | Verify YamlDialogueLoader's DialogueEntryData class -- currently it only has `Id` and `Content`, no `Condition` or `Priority`. May need to extend YamlDialogueLoader or use LegacyYamlDialogueLoader approach |
| Test assertions reference specific dialogue content that changes during format migration | Low | Medium | Preserve exact dialogue text content, only change structural format |
| Production YAML files also use `branches:` format -- fixing tests doesn't fix production | Low | Low | Out of scope for F652; production format migration is a separate concern for conversion features (F636-F643) |
| ErbToYaml.Tests may have their own tests depending on `meirin_com0.yaml` format | Medium | Medium | Check ErbToYaml.Tests for references to these files before modifying |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "KojoComparer tests must pass" | All 4 unit tests in PilotEquivalenceTests pass | AC#1 |
| "must pass for full equivalence verification" | YamlRunner.Render() produces correct dialogue content for all TALENT states | AC#2, AC#3 |
| "Migrate test YAML files to new format" | Test YAML uses `entries:` schema with `id`/`content` fields | AC#4 |
| "compatible with YamlDialogueLoader" | YamlDialogueLoader successfully loads migrated file | AC#5 |
| "All KojoComparer.Tests pass" | Full test suite passes (`dotnet test tools/KojoComparer.Tests`) | AC#6 |
| (implicit) Directory structure matches KojoEngine.GetPath() | Test file at `{basePath}/1_美鈴/COM_0.yaml` path | AC#7 |
| (implicit) No regression in ErbToYaml.Tests | ErbToYaml.Tests that reference meirin_com0.yaml still pass or are updated | AC#8 |
| (implicit) Test code references updated | PilotEquivalenceTests.cs points to new file path | AC#9 |
| (implicit) No technical debt markers | No TODO/FIXME/HACK in modified files | AC#10, AC#11 |
| "YamlDialogueLoader must support full DialogueEntry schema (Priority, Condition) for production YAML rendering" | YamlDialogueLoader unit tests pass after extension | AC#12 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PilotEquivalenceTests 4 unit tests pass | test | dotnet test tools/KojoComparer.Tests --filter "Category=Unit&FullyQualifiedName~PilotEquivalence" | succeeds | - | [x] |
| 2 | Lover branch renders correct content | code | Grep(tools/KojoComparer.Tests/PilotEquivalenceTests.cs) | contains | "恋人に触れられる幸せ" | [x] |
| 3 | All 4 branch assertions preserved | code | Grep(tools/KojoComparer.Tests/PilotEquivalenceTests.cs) | contains | "何するんですか" | [x] |
| 4 | YAML uses entries schema | file | Grep(tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml) | contains | "entries:" | [x] |
| 5 | YAML has id and content fields | file | Grep(tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml) | contains | "- id:" | [x] |
| 6 | Full KojoComparer.Tests pass | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 7 | Directory structure matches KojoEngine | file | Glob(tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml) | exists | - | [x] |
| 8 | ErbToYaml.Tests pass | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 9 | Test code references new path | code | Grep(tools/KojoComparer.Tests/PilotEquivalenceTests.cs) | contains | "1_美鈴" | [x] |
| 10 | No technical debt in PilotEquivalenceTests.cs | code | Grep(tools/KojoComparer.Tests/PilotEquivalenceTests.cs, pattern="TODO|FIXME|HACK") | count_equals | 0 | [x] |
| 11 | No technical debt in YamlDialogueLoader.cs | code | Grep(Era.Core/Dialogue/Loading/YamlDialogueLoader.cs, pattern="TODO|FIXME|HACK") | count_equals | 0 | [x] |
| 12 | YamlDialogueLoader unit tests pass | test | dotnet test Era.Core.Tests --filter "FullyQualifiedName~YamlDialogueLoader" | succeeds | - | [x] |

**Note**: 12 ACs is within the infra type range (8-15).

### AC Details

**AC#1: PilotEquivalenceTests 4 unit tests pass**
- Verifies the 4 unit tests (PilotYamlUnit_Lover, PilotYamlUnit_Yearning, PilotYamlUnit_DifferentTalentStates, PilotYamlUnit_AllBranchConditions) all pass
- These tests call `_yamlRunner.Render(YamlFilePath, yamlContext)` and assert specific dialogue content
- This is the primary goal of F652: the "File not found" error must be resolved
- Note: The 4 integration tests (Skip'd) are not in scope - they require headless mode

**AC#2: Lover branch renders correct content**
- Ensures test assertions still check for "恋人に触れられる幸せ" (lover branch characteristic phrase)
- Validates that dialogue content is preserved through format migration, not just structural changes
- The test assertions must remain identical - only the YAML format and file path change

**AC#3: All 4 branch assertions preserved**
- Ensures all 4 branch-specific assertions remain in the test code
- "何するんですか" is from the fallback (no TALENT) branch - the last branch
- Combined with AC#2, this verifies both ends of the branch list are preserved

**AC#4: YAML uses entries schema**
- The migrated YAML file must use `entries:` top-level key (not `branches:`)
- This is the YamlDialogueLoader's expected format per DialogueFileData class
- Search scope: `tools/KojoComparer.Tests/` or the new subdirectory under TestOutput

**AC#5: YAML has id and content fields**
- Each entry in the `entries:` list must have `id:` and `content:` fields
- These map to DialogueEntryData.Id and DialogueEntryData.Content
- Note: YamlDialogueLoader currently does NOT deserialize Condition/Priority fields. The implementation must work within this limitation (e.g., multiple entries with different content, using content-based selection or extending the loader)

**AC#6: Full KojoComparer.Tests pass**
- Runs all tests in KojoComparer.Tests project including OutputNormalizerTests, DiffEngineTests, ErbRunnerTests, YamlRunnerTests, BatchProcessorTests
- Ensures no regression from the migration changes
- Skip'd tests should remain Skip'd (not broken)

**AC#7: Directory structure matches KojoEngine**
- KojoEngine.GetPath() constructs: `Path.Combine(_kojoBasePath, "1_美鈴", "COM_0.yaml")`
- YamlRunner.ParseYamlPath for test format sets basePath = file's directory
- Therefore the actual file must exist at `{basePath}/1_美鈴/COM_0.yaml`
- This means creating subdirectory `1_美鈴/` under the test output directory

**AC#8: ErbToYaml.Tests pass**
- SchemaValidationTests.cs references `meirin_com0.yaml` for schema validation
- If the original file is moved/replaced, this test may break
- Options: (a) keep original file for ErbToYaml.Tests, (b) create separate copy for KojoComparer, (c) update ErbToYaml.Tests references
- Must verify no regression

**AC#9: Test code references new path**
- PilotEquivalenceTests.cs must update YamlFilePath to point to the new directory-structured location
- The path should include "1_美鈴" as part of the directory structure
- YamlRunnerTests.cs (Skip'd) should also be updated for consistency

**AC#10: No technical debt**
- No TODO, FIXME, or HACK markers in modified test files
- Ensures clean implementation without deferred workarounds

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The migration involves two parallel transformations:

1. **YAML Schema Conversion**: Convert from old `branches:` format to new `entries:` format
2. **Directory Restructuring**: Move flat test files into `{N_CharacterName}/COM_NNN.yaml` hierarchy

**Key Insight**: The investigation revealed that YamlDialogueLoader (lines 18-19) only deserializes `Id` and `Content` fields, NOT `Condition` or `Priority`. However, DialogueEntry (DialogueFile.cs lines 25-46) DOES have `Priority` and `Condition` fields with defaults. This means:

- YamlDialogueLoader creates entries with `Priority = 0` (default) and `Condition = null` (default)
- PriorityDialogueSelector (lines 28-31) filters by condition first, then orders by priority
- Without explicit conditions in YAML, ALL entries have `Condition = null` and pass the filter
- The selector will select the first entry (all have Priority=0)

**Solution Strategy**: Since YamlDialogueLoader cannot deserialize conditions, we must extend it to support condition/priority fields OR use a workaround. Given the constraints analysis identified this limitation, the approach is:

**Option A (Recommended)**: Extend YamlDialogueLoader to deserialize Condition and Priority fields
- Modify DialogueEntryData (YamlDialogueLoader.cs line 16-20) to include `Priority` and `Condition` properties
- Update deserialization mapping (line 50) to include these fields
- Convert old condition format (`TALENT: {16: {ne: 0}}`) to new DialogueCondition record format

**Option B (Fallback)**: Create multiple entries with same Priority and rely on content-based selection
- This won't work because PriorityDialogueSelector uses conditions, not content

**Option C (Legacy)**: Use LegacyYamlDialogueLoader (Era.Core.Tests/Helpers/)
- This is a test helper class and should not be used for production-like KojoComparer tests

**Selected**: Option A - extend YamlDialogueLoader. This is the proper fix that resolves the root cause rather than working around it.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | After YAML migration and YamlDialogueLoader extension, run `dotnet test tools/KojoComparer.Tests --filter "Category=Unit&FullyQualifiedName~PilotEquivalence"` to verify 4 unit tests pass |
| 2 | Preserve exact dialogue content during format migration - "恋人に触れられる幸せ" text remains in migrated YAML and test assertions unchanged |
| 3 | Preserve all 4 branch assertions in PilotEquivalenceTests.cs including "何するんですか" (fallback branch) |
| 4 | Migrated YAML uses `entries:` top-level key instead of `branches:` |
| 5 | Each entry has `id:`, `content:`, `priority:` (optional), and `condition:` (optional) fields in camelCase |
| 6 | Run full test suite `dotnet test tools/KojoComparer.Tests` after all changes |
| 7 | Create directory structure `tools/ErbToYaml.Tests/TestOutput/1_美鈴/` and place `COM_0.yaml` inside |
| 8 | SchemaValidationTests.cs references meirin_com0.yaml - update path OR keep copy in TestOutput root for backward compatibility |
| 9 | Update PilotEquivalenceTests.cs line 24: `YamlFilePath = Path.Combine(RepoRoot, "tools", "ErbToYaml.Tests", "TestOutput", "1_美鈴", "COM_0.yaml")` |
| 10 | Code review modified files for TODO/FIXME/HACK markers |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| How to handle condition deserialization | A) Extend YamlDialogueLoader, B) Content-based selection, C) LegacyYamlDialogueLoader | A | Root cause fix. DialogueEntry supports these fields; YamlDialogueLoader just needs to deserialize them |
| Condition format conversion | A) Manual YAML editing, B) Automated converter script, C) Copy from existing format | A | Only 2 test files, manual is faster and more verifiable |
| Directory structure | A) Create new hierarchy, B) Update KojoEngine.GetPath(), C) Keep flat with path override | A | Matches production structure, tests realistic scenarios |
| File placement | A) Move original file, B) Create copy in new location, C) Keep both | C | Keep original in TestOutput/ for SchemaValidationTests, create copy in 1_美鈴/ for PilotEquivalenceTests |
| YAML schema version | A) Keep old branches: for SchemaValidationTests, B) Migrate both files, C) Update schema validator | B | Both files should use the new format; update SchemaValidationTests to reference new location |

### Interfaces / Data Structures

**YamlDialogueLoader Extension**:

```csharp
// Era.Core/Dialogue/Loading/YamlDialogueLoader.cs
// Extend DialogueEntryData to include Priority and Condition (lines 16-20)

internal class DialogueEntryData
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;  // NEW
    public DialogueConditionData? Condition { get; set; }  // NEW
}

// NEW: Helper class for YAML deserialization of DialogueCondition
internal class DialogueConditionData
{
    public string Type { get; set; } = string.Empty;
    public string? TalentType { get; set; }
    public string? AblType { get; set; }
    public int? Threshold { get; set; }
    public string? Operand { get; set; }
    public List<DialogueConditionData>? Operands { get; set; }
    public DialogueConditionData? SingleOperand { get; set; }
}

// Update mapping (line 49-51) to convert DialogueConditionData to DialogueCondition
var entries = fileData.Entries
    .Select(e => new DialogueEntry {
        Id = e.Id,
        Content = e.Content,
        Priority = e.Priority,  // NEW
        Condition = e.Condition == null ? null : ConvertCondition(e.Condition)  // NEW
    })
    .ToList();

// NEW: Recursive conversion method
private static DialogueCondition? ConvertCondition(DialogueConditionData data)
{
    return new DialogueCondition(
        Type: data.Type,
        TalentType: data.TalentType,
        AblType: data.AblType,
        Threshold: data.Threshold,
        Operand: data.Operand,
        Operands: data.Operands?.Select(ConvertCondition).ToList(),
        SingleOperand: data.SingleOperand == null ? null : ConvertCondition(data.SingleOperand)
    );
}
```

**New YAML Format Example** (from old meirin_com0.yaml branches[0]):

```yaml
entries:
- id: "lover"
  content: |
    DATAFORM 「んっ……そこ、気持ちいい……」
    %CALLNAME:人物_美鈴%は%CALLNAME:MASTER%に身を預け、されるがままになっている。
    「……%CALLNAME:MASTER%の手、あったかいね」
    %CALLNAME:人物_美鈴%は%CALLNAME:MASTER%の愛撫を心地よさそうに受け入れている。
    その顔には、恋人に触れられる幸せが浮かんでいた。
    ...
  priority: 3
  condition:
    type: "Talent"
    talentType: "16"
    threshold: 1

- id: "yearning"
  content: |
    DATAFORM 「ひゃっ……！　ちょ、ちょっと%CALLNAME:MASTER%……」
    ...
  priority: 2
  condition:
    type: "Talent"
    talentType: "3"
    threshold: 1

- id: "admiration"
  content: |
    DATAFORM 「え、えっと……%CALLNAME:MASTER%？」
    ...
  priority: 1
  condition:
    type: "Talent"
    talentType: "17"
    threshold: 1

- id: "fallback"
  content: |
    DATAFORM 「ちょっ……何するんですか！？」
    ...
  priority: 0
  # No condition = always matches (fallback)
```

**Condition Mapping** (old → new):

| Old Format | New Format |
|------------|------------|
| `condition: { TALENT: { 16: { ne: 0 } } }` | `condition: { type: "Talent", talentType: "16", threshold: 1 }` |
| `condition: { TALENT: { 3: { ne: 0 } } }` | `condition: { type: "Talent", talentType: "3", threshold: 1 }` |
| (no condition) | (no condition field) |

**Note**: The old format uses `ne: 0` (not equal to 0), which means "has this TALENT". The new TalentSpecification (Dialogue/Specifications/TalentSpecification.cs) checks `GetTalent() >= threshold`. So `ne: 0` maps to `threshold: 1` (TALENT value must be >= 1).

**Priority Assignment Strategy**:
- Highest priority (3): TALENT:16 (恋人 - most intimate)
- Medium priority (2): TALENT:3 (恋慕 - yearning)
- Low priority (1): TALENT:17 (思慕 - admiration)
- Fallback (0): No condition (default response)

This mirrors the original ERB SELECTCASE priority where earlier branches take precedence.

### Implementation Plan

**Phase 1: Extend YamlDialogueLoader**
1. Add DialogueConditionData class to YamlDialogueLoader.cs
2. Add Priority and Condition properties to DialogueEntryData
3. Add ConvertCondition() helper method
4. Update Select() mapping to include Priority and Condition

**Phase 2: Create Migrated YAML Files**
1. Create directory: `tools/ErbToYaml.Tests/TestOutput/1_美鈴/`
2. Convert meirin_com0.yaml to new format:
   - Replace `branches:` with `entries:`
   - For each branch, create entry with `id:`, `content:`, `priority:`, `condition:`
   - Convert condition format from nested dict to DialogueCondition format
3. Save as `tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml`
4. Repeat for meirin_com0_render.yaml if used by tests (verify usage first)

**Phase 3: Update Test Code**
1. Update PilotEquivalenceTests.cs line 24 to point to new path
2. Update YamlRunnerTests.cs references (Skip'd tests)
3. Check SchemaValidationTests.cs - if it references meirin_com0.yaml, either:
   - Keep copy in TestOutput/ root (prefer this for minimal change)
   - Update path in SchemaValidationTests.cs

**Phase 4: Verification**
1. Run `dotnet test tools/KojoComparer.Tests --filter "Category=Unit"` - verify 4 tests pass
2. Run `dotnet test tools/ErbToYaml.Tests` - verify no regression
3. Run full `dotnet test tools/KojoComparer.Tests` - verify all pass (Skip'd remain Skip'd)
4. Code review for TODO/FIXME/HACK

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 4,5,12 | Extend YamlDialogueLoader to deserialize Priority and Condition fields | [x] |
| 2 | 4,5,7 | Create directory structure and migrate meirin_com0.yaml to new format | [x] |
| 3 | 9 | Update test file paths (PilotEquivalenceTests.cs and YamlRunnerTests.cs) to new location | [x] |
| 4 | 8 | Verify SchemaValidationTests.cs and update if needed | [x] |
| 5 | 1,2,3 | Run PilotEquivalenceTests unit tests and verify dialogue content | [x] |
| 6 | 6 | Run full KojoComparer.Tests suite | [x] |
| 7 | 8 | Run ErbToYaml.Tests suite to verify no regression | [x] |
| 8 | 10,11 | Code review for technical debt markers | [x] |
| 9 | 12 | Create unit tests for YamlDialogueLoader Priority/Condition deserialization | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | YamlDialogueLoader.cs, DialogueFile.cs | Extended DialogueEntryData with Priority/Condition, ConvertCondition() method |
| 2 | implementer | sonnet | T2 | meirin_com0.yaml (old format), Technical Design conversion rules | tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml (new format) |
| 3 | implementer | sonnet | T3, T4 | PilotEquivalenceTests.cs, SchemaValidationTests.cs | Updated test file paths |
| 4 | ac-tester | haiku | T5, T6, T7 | Test commands from ACs | Test results |
| 5 | implementer | sonnet | T8 | Modified files | Code review report |

**Constraints** (from Technical Design):

1. YamlDialogueLoader deserialization must use camelCase naming convention (existing)
2. Condition conversion: old `{ TALENT: { N: { ne: 0 } } }` → new `{ type: "Talent", talentType: "N", threshold: 1 }`
3. Priority assignment: TALENT:16=3, TALENT:3=2, TALENT:17=1, no condition=0 (fallback)
4. Directory structure: `{basePath}/1_美鈴/COM_0.yaml` to match KojoEngine.GetPath()
5. SchemaValidationTests.cs may reference original file - keep copy in TestOutput/ root if needed

**Pre-conditions**:

- F553 (KojoEngine API) is [DONE]
- F549 (YamlDialogueLoader) is [DONE]
- F651 (KojoComparer update) is [BLOCKED] awaiting this fix

**Success Criteria**:

- All 4 PilotEquivalenceTests unit tests pass
- Full KojoComparer.Tests suite passes (Skip'd tests remain Skip'd)
- ErbToYaml.Tests passes (no regression)
- No TODO/FIXME/HACK markers in modified files
- F651 AC#12 can be unblocked after completion

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation
4. Mark F651 as [BLOCKED] again if rollback occurs

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| Production YAML format migration | F636-F643 | Game/YAML/Kojo/ files use branches: format; separate concern for F636-F643 conversion features |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-28 11:43 | START | implementer - Task 1 |
| 2026-01-28 11:43 | END | implementer - Task 1 - SUCCESS |
| 2026-01-28 11:49 | START | implementer - Task 2 |
| 2026-01-28 11:49 | END | implementer - Task 2 - SUCCESS |
| 2026-01-28 11:48 | START | implementer - Task 3, 4 |
| 2026-01-28 11:48 | END | implementer - Task 3, 4 - SUCCESS |
| 2026-01-28 11:53 | START | implementer - Task 9 |
| 2026-01-28 11:53 | END | implementer - Task 9 - SUCCESS |
| 2026-01-28 | DEVIATION | ac-tester | dotnet test tools/KojoComparer.Tests | AC#1 FAIL: exit code 1 - DialogueFileData missing Character/Situation properties |
| 2026-01-28 | DEVIATION | ac-tester | Grep | AC#5 FAIL: Expected "  id:" but YAML uses "- id:" (definition error) |
| 2026-01-28 | FIX | debugger | Remove character/situation from COM_0.yaml | AC#1,6 now PASS |
| 2026-01-28 | FIX | manual | AC#5 Expected updated to "- id:", verified PASS | AC#5 now PASS |
| 2026-01-28 | END | ac-tester | All 12 ACs PASS | 4 PilotEquivalence unit, 12/12+8skip KojoComparer, 60/60 ErbToYaml, 13/13 YamlDialogueLoader |


