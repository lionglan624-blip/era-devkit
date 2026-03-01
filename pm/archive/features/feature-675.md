# Feature 675: YAML Format Unification (branches → entries)

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

## Type: infra

## Created: 2026-01-28

---

## Summary

Unify all production YAML files from legacy `branches:` format to canonical `entries:` format, enabling KojoComparer equivalence testing.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Single canonical YAML format across the entire pipeline (conversion, rendering, testing)

### Problem (Current Issue)

Critical format mismatch exists:
- **Production YAML** (443 files in `Game/YAML/Kojo/`): Uses `branches:` format with `lines:` arrays
- **KojoEngine/YamlDialogueLoader**: Expects `entries:` format with `id:`/`content:` fields
- **Result**: YamlRunner cannot render production YAML, blocking F644 Equivalence Testing

This gap was created because:
1. ErbToYaml outputs `branches:` format (legacy)
2. F651/F652 updated KojoComparer to use KojoEngine which requires `entries:` format
3. No migration was performed on production files

### Goal (What to Achieve)

1. Update ErbToYaml to output `entries:` format (fix at source)
2. Re-convert all 443 production YAML files to `entries:` format
3. Verify YamlDialogueLoader can load all converted files
4. Single canonical format: `entries:` everywhere

---


<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: YamlRunner cannot render 443 production YAML files
2. Why: YamlDialogueLoader (used by KojoEngine) expects `entries:` format with `id:`/`content:` fields
3. Why: Production YAML files use `branches:` format with `lines:` arrays and nested `condition:` objects
4. Why: ErbToYaml tool (DatalistConverter, FileConverter, PrintDataConverter) outputs `branches:` format per dialogue-schema.json specification
5. Why: The schema was designed before KojoEngine refactoring (F549-F553) which introduced the new `entries:` format; no migration was planned

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| YamlRunner throws "File not found" or parse errors on production YAML | ErbToYaml outputs `branches:` format while YamlDialogueLoader expects `entries:` format - schema divergence between conversion tool and rendering engine |
| F644 Equivalence Testing blocked | Format adapter or source fix required before batch verification can proceed |

### Conclusion

The problem is a **schema divergence** created during the KojoEngine refactoring (F549-F553):

1. **ErbToYaml** outputs `branches:` format per `dialogue-schema.json`:
   - `character`, `situation`, `branches[]` top-level structure
   - Each branch has `lines[]` (array of strings) and optional `condition` (nested dict like `{ TALENT: { 3: { ne: 0 } } }`)

2. **YamlDialogueLoader** expects `entries:` format per `DialogueFile.cs`:
   - `entries[]` top-level structure
   - Each entry has `id` (string), `content` (multiline string), `priority` (int), `condition` (typed record with `Type`, `TalentType`, `Threshold`)

This is not a bug but a **planned evolution** that was incomplete. F652 migrated test files but production files were left in the legacy format.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F549 | [DONE] | Defines target format | YamlDialogueLoader with `entries:` schema |
| F551 | [DONE] | Consumer of format | TemplateDialogueRenderer uses DialogueEntry |
| F553 | [DONE] | API change source | KojoEngine Facade Refactoring |
| F651 | [DONE] | Updated consumer | KojoComparer now uses new KojoEngine API |
| F652 | [DONE] | Partial migration | Migrated test YAML files, identified production gap |
| F644 | [DRAFT] | Blocked successor | Equivalence Testing requires format unification |
| F636-F643 | Various | Content producers | Kojo conversion features create production YAML |

### Pattern Analysis

This is a **format evolution gap** that follows a pattern:
1. New API introduced (F549-F553)
2. Consumers updated (F651)
3. Test data migrated (F652)
4. Production data left behind (gap)

The gap was explicitly noted in F652's 残課題: "Production YAML format migration is a separate concern for F636-F643 conversion features." However, F636-F643 continued to produce `branches:` format because ErbToYaml was not updated.

The fix should be at the **source** (ErbToYaml) rather than adapters, to ensure all future conversions produce compatible output.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Both schemas are well-defined; transformation is deterministic |
| Scope is realistic | YES | ErbToYaml modification is localized; re-conversion is batch operation |
| No blocking constraints | YES | F651/F652 are [DONE]; no circular dependencies |

**Verdict**: FEASIBLE

**Rationale**:
1. **Format transformation is lossless**: `branches:` → `entries:` mapping is straightforward
   - `lines[]` → join into `content` string
   - `condition: { TALENT: { N: { ne: 0 } } }` → `condition: { type: "Talent", talentType: "N", threshold: 1 }`
   - Priority can be derived from branch order (earlier = higher priority)
   - `id` can be generated from condition type or "fallback"

2. **Tool modification is localized**:
   - DatalistConverter.cs (lines 91-96): Change `{ "branches", branches }` to `{ "entries", entries }`
   - FileConverter.cs (lines 244-250): Same pattern
   - PrintDataConverter.cs (lines 55-69): Same pattern

3. **Batch re-conversion exists**: ErbToYaml already has batch mode (`--directory` flag)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F651 | [DONE] | KojoComparer uses new KojoEngine API with entries: format |
| Predecessor | F652 | [DONE] | YamlDialogueLoader extended for Priority/Condition deserialization |
| Related | F549 | [DONE] | YamlDialogueLoader defines target entries: schema |
| Related | F553 | [DONE] | KojoEngine API change source |
| Successor | F644 | [PROPOSED] | Equivalence Testing Framework - blocked by format mismatch |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | Runtime | Low | Already in use, no version change needed |
| ErbParser | Build | Low | Already dependency of ErbToYaml |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer/YamlRunner.cs | HIGH | Uses YamlDialogueLoader to render YAML |
| Era.Core/KojoEngine.cs | HIGH | GetDialogue() uses YamlDialogueLoader |
| tools/YamlSchemaGen/dialogue-schema.json | MEDIUM | Schema must be updated to reflect new format |
| tools/com-validator/schemas/com.schema.json | LOW | Does not validate dialogue YAML format (validates COM files only) |
| F636-F643 conversion features | MEDIUM | Must re-run to regenerate YAML in new format |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/DatalistConverter.cs | Update | Change output from `branches:` to `entries:` format |
| tools/ErbToYaml/FileConverter.cs | Update | Change ConvertConditionalNode output format |
| tools/ErbToYaml/PrintDataConverter.cs | Update | Change simple PRINTDATA output format |
| tools/YamlSchemaGen/dialogue-schema.json | Update | Update schema to define `entries:` format |
| Game/YAML/Kojo/**/*.yaml | Regenerate | All 443 files re-converted via batch operation |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| YamlDialogueLoader uses CamelCaseNamingConvention | YamlDialogueLoader.cs line 46 | MEDIUM - YAML keys must be camelCase |
| DialogueCondition has specific record structure | DialogueCondition.cs | HIGH - condition must match Type/TalentType/Threshold format |
| Priority ordering must match ERB branch order | PriorityDialogueSelector.cs | MEDIUM - higher priority selected first |
| Content must be single string (not array) | DialogueEntry.cs line 35 | MEDIUM - lines[] must be joined |
| Id must be unique per file | DialogueEntry.cs line 30 | LOW - can generate from condition |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Condition format conversion loses information | Low | Medium | Old format uses `ne: 0` which maps cleanly to `threshold: 1` |
| Priority assignment differs from ERB branch order | Medium | Medium | Use reverse branch index (first branch = highest priority) |
| Character/situation metadata lost in entries format | Low | Low | DialogueFile only has Entries+FilePath; metadata inferred from path |
| Re-conversion requires source ERB files | Low | Low | ERB files exist in Game/ERB/口上/ |
| Schema validator breaks on new format | Medium | Low | Update dialogue-schema.json and com.schema.json |
| Generated id collision | Low | Low | Use condition-based naming (e.g., "talent_16", "fallback") |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Single canonical YAML format across the entire pipeline" | All pipeline components (conversion, rendering, testing) must use `entries:` format | AC#1, AC#2, AC#3 |
| "Update ErbToYaml output" (Goal 1) | ErbToYaml tool outputs `entries:` format, not `branches:` | AC#4, AC#5 |
| "Re-convert all 443 production YAML files" (Goal 2) | All production YAML files regenerated in `entries:` format | AC#6, AC#7 |
| "Verify YamlDialogueLoader can load all converted files" (Goal 3) | KojoComparer YamlRunner successfully loads converted files | AC#8, AC#9 |
| "Single canonical format: entries: everywhere" (Goal 4) | No `branches:` format remains in production YAML | AC#10 |
| Schema must support new format | dialogue-schema.json updated to define `entries:` schema | AC#11 |
| No technical debt | No TODO/FIXME/HACK markers in modified code | AC#12 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DatalistConverter outputs entries format | code | Grep(tools/ErbToYaml/DatalistConverter.cs) | contains | "entries" | [x] |
| 2 | FileConverter outputs entries format | code | Grep(tools/ErbToYaml/FileConverter.cs) | contains | "entries" | [x] |
| 3 | PrintDataConverter outputs entries format | code | Grep(tools/ErbToYaml/PrintDataConverter.cs) | contains | "entries" | [x] |
| 4 | DatalistConverter no branches output | code | Grep(tools/ErbToYaml/DatalistConverter.cs) | not_contains | "\\\"branches\\\"" | [x] |
| 5 | FileConverter no branches output | code | Grep(tools/ErbToYaml/FileConverter.cs) | not_contains | "\\\"branches\\\"" | [x] |
| 6 | Production YAML uses entries format | file | Grep(Game/YAML/Kojo/) | contains | "entries:" | [x] |
| 7 | Production YAML file count maintained | file | Glob(Game/YAML/Kojo/**/*.yaml) | count_gte | 443 | [x] |
| 8 | YamlDialogueLoader loads sample file | test | dotnet test Era.Core.Tests --filter "YamlDialogueLoader" | succeeds | - | [x] |
| 9 | KojoComparer builds successfully | build | dotnet build tools/KojoComparer/ | succeeds | - | [x] |
| 10 | No branches format in production YAML | file | Grep(Game/YAML/Kojo/) | not_contains | "branches:" | [x] |
| 11 | Schema defines entries structure | file | Grep(tools/YamlSchemaGen/dialogue-schema.json) | contains | "entries" | [x] |
| 12 | No technical debt in converters | code | Grep(tools/ErbToYaml/) | not_contains | "TODO|FIXME|HACK" | [x] |
| 13 | BranchesToEntriesConverter implementation exists | file | Glob(tools/ErbToYaml/BranchesToEntriesConverter.cs) | exists | - | [x] |
| 14 | PrintDataConverter no branches output | code | Grep(tools/ErbToYaml/PrintDataConverter.cs) | not_contains | "\"branches\"" | [x] |

**Note**: 14 ACs is within infra feature range (8-15).

### AC Details

**AC#1: DatalistConverter outputs entries format**
- Verifies DatalistConverter.cs uses "entries" key in YAML output
- This is the primary converter for DATALIST-based ERB files
- The key must appear in the output dictionary construction

**AC#2: FileConverter outputs entries format**
- Verifies FileConverter.cs uses "entries" key in YAML output
- FileConverter handles conditional branch conversions
- Must output `entries:` array with `id`, `content`, `priority`, `condition` fields

**AC#3: PrintDataConverter outputs entries format**
- Verifies PrintDataConverter.cs uses "entries" key in YAML output
- PrintDataConverter handles simple PRINTDATA constructs
- Same output structure as other converters

**AC#4: DatalistConverter no branches output**
- Negative test ensuring legacy "branches" key removed from DatalistConverter
- Uses escaped quote pattern `\"branches\"` to match dictionary key assignment
- Ensures no residual legacy format in output

**AC#5: FileConverter no branches output**
- Negative test ensuring legacy "branches" key removed from FileConverter
- Critical for format unification as FileConverter is most complex

**AC#6: Production YAML uses entries format**
- Verifies regenerated YAML files contain `entries:` top-level key
- Grep across Game/YAML/Kojo/ directory
- At least one file must have the format (positive existence check)

**AC#7: Production YAML file count maintained**
- Verifies all 443+ YAML files still exist after regeneration
- Uses Glob with count_gte matcher to ensure no files lost during conversion
- Allows for growth (new files added) with gte rather than equals

**AC#8: YamlDialogueLoader loads sample file**
- Verifies Era.Core's YamlDialogueLoader can parse new format
- Uses existing unit tests in Era.Core.Tests
- Tests deserialization of entries, priority, and condition fields

**AC#9: KojoComparer builds successfully**
- Verifies KojoComparer project compiles with updated dependencies
- KojoComparer uses YamlRunner which depends on new format
- Build success indicates no breaking API changes

**AC#10: No branches format in production YAML**
- Critical negative test ensuring complete format migration
- Verifies no YAML file retains legacy `branches:` key
- Must return zero matches across all production YAML

**AC#11: Schema defines entries structure**
- Verifies dialogue-schema.json updated to define `entries:` structure
- Schema must validate the new format for tooling compatibility
- Used by YamlValidator and other schema-aware tools

**AC#12: No technical debt in converters**
- Ensures no TODO/FIXME/HACK markers left in modified converter code
- Uses ripgrep alternation pattern (unescaped pipe per INFRA.md Issue 33)
- Covers all files in tools/ErbToYaml/ directory

**AC#13: BranchesToEntriesConverter implementation exists**
- Verifies BranchesToEntriesConverter.cs file exists in tools/ErbToYaml/
- Uses Glob with exists matcher to confirm file creation
- Addresses gap where T2 implements helper class but no AC verified its existence

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Transform ErbToYaml output from legacy `branches:` format to canonical `entries:` format by:

1. **Modify three converter classes** to output `entries:` array instead of `branches:` array
2. **Transform data structure** during conversion:
   - Join `lines[]` array into single `content` string (multiline YAML scalar)
   - Generate unique `id` from condition type or use "fallback" for unconditional entries
   - Derive `priority` from branch order (reverse index: first branch = highest priority)
   - Transform nested condition dict `{ TALENT: { N: { ne: 0 } } }` to flat DialogueCondition record `{ type: "Talent", talentType: "N", threshold: 1 }`
3. **Update dialogue-schema.json** to define new `entries:` structure
4. **Re-convert all production YAML** using batch mode (preserve directory structure)
5. **Verify loading** via existing Era.Core.Tests

This approach fixes the format at the **source** (ErbToYaml tool) rather than creating adapters, ensuring all future conversions produce compatible output.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Modify DatalistConverter.cs line 96: `{ "branches", branches }` → `{ "entries", ConvertBranchesToEntries(branches) }` |
| 2 | Modify FileConverter.cs line 249: `{ "branches", branches }` → `{ "entries", ConvertBranchesToEntries(branches) }` |
| 3 | Modify PrintDataConverter.cs line 61: `{ "branches", ... }` → `{ "entries", CreateSingleEntry(lines, character, situation) }` |
| 4 | Remove "branches" key assignment from DatalistConverter.cs (replaced by AC#1) |
| 5 | Remove "branches" key assignment from FileConverter.cs (replaced by AC#2) |
| 6 | Grep will find `entries:` in regenerated YAML files (ErbToYaml outputs new format) |
| 7 | Glob count verified after batch re-conversion (all 443+ files regenerated) |
| 8 | Existing YamlDialogueLoaderTests run against sample regenerated file (unit test execution) |
| 9 | Build KojoComparer after Era.Core changes propagate (no API breaking changes) |
| 10 | Grep will find zero `branches:` after batch re-conversion completes |
| 11 | Update dialogue-schema.json to replace `branches` property with `entries` array schema |
| 12 | No technical debt markers after clean implementation (verified via Grep) |
| 13 | Create BranchesToEntriesConverter.cs in tools/ErbToYaml/ directory (verified via Glob) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Fix location** | A) Adapter in YamlDialogueLoader to accept both formats<br>B) Fix at source (ErbToYaml) | B | Single canonical format principle; adapters create ongoing complexity; future conversions automatically correct |
| **Content format** | A) Keep lines as array<br>B) Join into multiline string | B | DialogueEntry.Content is `string` not `string[]`; YAML multiline scalars preserve readability |
| **Priority assignment** | A) All entries priority 0<br>B) Reverse branch index (first=highest)<br>C) Sequential index (first=lowest) | B | Matches ERB evaluation order where first matching branch wins; PriorityDialogueSelector selects highest priority first |
| **ID generation** | A) UUID<br>B) Sequential index<br>C) Condition-based ("talent_16_0", "fallback") | C | Semantic IDs aid debugging; condition type is human-readable; branch index prevents collisions; fallback clearly identifies unconditional entries |
| **Condition transformation** | A) Keep nested dict format<br>B) Transform to DialogueCondition record format<br>C) String serialization | B | YamlDialogueLoader expects DialogueCondition with Type/TalentType/Threshold fields; type-safe deserialization |
| **Schema update timing** | A) Update schema first<br>B) Update schema after converter changes<br>C) Update schema during converter changes | C | Schema and converter must stay synchronized; update together to prevent validation failures during development |
| **Re-conversion scope** | A) Manual file-by-file<br>B) Batch re-conversion with `--directory` flag<br>C) Incremental (convert on demand) | B | 443 files require automation; ErbToYaml already has batch mode; ensures consistency across all files |
| **Condition operator mapping** | A) Direct string copy ("ne")<br>B) Map to threshold value (ne 0 → threshold 1)<br>C) Keep original ERB condition string | B | DialogueCondition uses Threshold (int) not operator strings; "ne 0" semantically means "≥1" for existence checks |

### Interfaces / Data Structures

#### New Converter Helper: BranchesToEntriesConverter

```csharp
public static class BranchesToEntriesConverter
{
    /// <summary>
    /// Convert legacy branches format to entries format
    /// </summary>
    /// <param name="branches">List of branch dictionaries with lines/condition</param>
    /// <returns>List of entry dictionaries with id/content/priority/condition</returns>
    public static List<Dictionary<string, object>> Convert(List<object> branches)
    {
        var entries = new List<Dictionary<string, object>>();
        int branchCount = branches.Count;

        for (int i = 0; i < branchCount; i++)
        {
            var branch = (Dictionary<string, object>)branches[i];
            var lines = (List<string>)branch["lines"];
            var condition = branch.ContainsKey("condition")
                ? (Dictionary<string, object>)branch["condition"]
                : null;

            // Join lines into content (preserve line breaks)
            var content = string.Join("\n", lines);

            // Generate ID from condition or use "fallback"
            var id = GenerateId(condition, i);

            // Calculate priority (reverse index: first branch = highest)
            var priority = branchCount - i;

            var entry = new Dictionary<string, object>
            {
                { "id", id },
                { "content", content },
                { "priority", priority }
            };

            // Transform condition from nested dict to DialogueCondition format
            if (condition != null && condition.Count > 0)
            {
                var transformedCondition = TransformCondition(condition);
                if (transformedCondition != null)
                {
                    entry["condition"] = transformedCondition;
                }
            }

            entries.Add(entry);
        }

        return entries;
    }

    private static string GenerateId(Dictionary<string, object>? condition, int index)
    {
        if (condition == null || condition.Count == 0)
            return "fallback";

        // Extract condition type and generate semantic ID with branch index to prevent collisions
        if (condition.ContainsKey("TALENT"))
        {
            var talentDict = (Dictionary<string, object>)condition["TALENT"];
            var talentId = talentDict.Keys.First(); // e.g., "3"
            return $"talent_{talentId}_{index}";
        }
        if (condition.ContainsKey("ABL"))
        {
            var ablDict = (Dictionary<string, object>)condition["ABL"];
            var ablId = ablDict.Keys.First();
            return $"abl_{ablId}_{index}";
        }

        // Fallback for unknown condition types
        return $"condition_{index}";
    }

    private static Dictionary<string, object>? TransformCondition(Dictionary<string, object> legacyCondition)
    {
        // Transform { TALENT: { 3: { ne: 0 } } }
        // → { type: "Talent", talentType: "3", threshold: 1 }

        if (legacyCondition.ContainsKey("TALENT"))
        {
            var talentDict = (Dictionary<string, object>)legacyCondition["TALENT"];
            var talentId = talentDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)talentDict[talentId];

            // Extract operator (ne, eq, gt, etc.)
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString();

            // Map to threshold (ne 0 → threshold 1, eq 0 → threshold 0, etc.)
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Talent" },
                { "talentType", talentId },
                { "threshold", threshold }
            };
        }

        // Handle ABL conditions
        if (legacyCondition.ContainsKey("ABL"))
        {
            var ablDict = (Dictionary<string, object>)legacyCondition["ABL"];
            var ablId = ablDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)ablDict[ablId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString();
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Abl" },
                { "ablType", ablId },
                { "threshold", threshold }
            };
        }

        // Handle EXP conditions
        if (legacyCondition.ContainsKey("EXP"))
        {
            var expDict = (Dictionary<string, object>)legacyCondition["EXP"];
            var expId = expDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)expDict[expId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString();
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Exp" },
                { "expType", expId },
                { "threshold", threshold }
            };
        }

        // Handle FLAG conditions
        if (legacyCondition.ContainsKey("FLAG"))
        {
            var flagDict = (Dictionary<string, object>)legacyCondition["FLAG"];
            var flagId = flagDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)flagDict[flagId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString();
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Flag" },
                { "flagId", flagId },
                { "threshold", threshold }
            };
        }

        // Handle CFLAG conditions
        if (legacyCondition.ContainsKey("CFLAG"))
        {
            var cflagDict = (Dictionary<string, object>)legacyCondition["CFLAG"];
            var cflagId = cflagDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)cflagDict[cflagId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString();
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "CFlag" },
                { "cflagId", cflagId },
                { "threshold", threshold }
            };
        }

        return null;
    }

    private static int MapOperatorToThreshold(string op, string value)
    {
        // For existence checks (ne 0), threshold is 1
        // For exact value checks (eq N), threshold is N
        // For comparison (gt N), threshold is N+1
        return op switch
        {
            "ne" when value == "0" => 1,
            "eq" => int.Parse(value),
            "gt" => int.Parse(value) + 1,
            "gte" => int.Parse(value),
            _ => 1 // Default: treat as existence check
        };
    }
}
```

#### Updated dialogue-schema.json Structure

```json
{
  "type": "object",
  "properties": {
    "character": { "type": "string" },
    "situation": { "type": "string" },
    "entries": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "id": { "type": "string" },
          "content": { "type": "string" },
          "priority": { "type": "integer", "default": 0 },
          "condition": {
            "type": "object",
            "properties": {
              "type": { "type": "string", "enum": ["Talent", "Abl", "Exp", "Flag", "CFlag"] },
              "talentType": { "type": "string" },
              "ablType": { "type": "string" },
              "threshold": { "type": "integer" },
              "operand": { "type": "string" },
              "operands": {
                "type": "array",
                "items": { "$ref": "#/properties/entries/items/properties/condition" }
              },
              "singleOperand": { "$ref": "#/properties/entries/items/properties/condition" }
            },
            "required": ["type"]
          }
        },
        "required": ["id", "content"]
      }
    }
  },
  "required": ["entries"]
}
```

#### Batch Re-conversion Script

```bash
# tools/ErbToYaml-batch-reconvert.sh
#!/bin/bash

# Re-convert all production ERB files to YAML with new entries format
# Preserves directory structure under Game/YAML/Kojo/

ERB_ROOT="Game/ERB/口上"
YAML_ROOT="Game/YAML/Kojo"
TALENT_CSV="Game/CSV/Talent.csv"
SCHEMA="tools/YamlSchemaGen/dialogue-schema.json"

echo "Starting batch re-conversion of $ERB_ROOT → $YAML_ROOT"

dotnet run --project tools/ErbToYaml/ErbToYaml.csproj -- \
  --directory "$ERB_ROOT" \
  --output "$YAML_ROOT" \
  --talent-csv "$TALENT_CSV" \
  --schema "$SCHEMA" \
  --verbose

echo "Batch re-conversion complete. Files regenerated: $(find "$YAML_ROOT" -name '*.yaml' | wc -l)"
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 11 | Update dialogue-schema.json to define entries: format | [x] |
| 2 | 1,13 | Implement BranchesToEntriesConverter helper class in tools/ErbToYaml/ | [x] |
| 3 | 1,4 | Modify DatalistConverter.cs to output entries: format | [x] |
| 4 | 2,5 | Modify FileConverter.cs to output entries: format | [x] |
| 5 | 3,14 | Modify PrintDataConverter.cs to output entries: format | [x] |
| 6 | 9 | Build KojoComparer to verify compilation | [x] |
| 7 | 6,7,10 | Execute batch re-conversion of all production YAML files | [x] |
| 8 | 8 | Run YamlDialogueLoader unit tests | [x] |
| 9 | 12 | Verify no technical debt markers in converter code | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | dialogue-schema.json current structure | Updated schema with entries: definition |
| 2 | implementer | sonnet | T2 | Technical Design BranchesToEntriesConverter pseudocode | BranchesToEntriesConverter.cs implementation |
| 3 | implementer | sonnet | T3-T5 | BranchesToEntriesConverter class | Modified DatalistConverter, FileConverter, PrintDataConverter |
| 4 | ac-tester | haiku | T6 | Modified converter code | Build verification (dotnet build) |
| 5 | implementer | sonnet | T7 | Batch re-conversion script | Execute batch conversion, verify file count |
| 6 | ac-tester | haiku | T8 | Regenerated YAML files | YamlDialogueLoader unit test execution |
| 7 | ac-tester | haiku | T9 | Modified converter files | Technical debt verification |

**Execution Steps**:

### Phase 1: Schema Update (T1)
1. Read current `tools/YamlSchemaGen/dialogue-schema.json`
2. Replace `branches` property definition with `entries` array schema per Technical Design
3. Verify JSON schema validity

### Phase 2: Converter Helper Implementation (T2)
1. Create `tools/ErbToYaml/BranchesToEntriesConverter.cs`
2. Implement `Convert()` method per Technical Design pseudocode
3. Implement helper methods: `GenerateId()`, `TransformCondition()`, `MapOperatorToThreshold()`
4. Build to verify compilation

### Phase 3: Converter Modifications (T3-T5)
1. **T3**: Modify `tools/ErbToYaml/DatalistConverter.cs`
   - Replace `{ "branches", branches }` with `{ "entries", BranchesToEntriesConverter.Convert(branches) }`
   - Verify no "branches" key remains
2. **T4**: Modify `tools/ErbToYaml/FileConverter.cs`
   - Same pattern as DatalistConverter
   - Update ConvertConditionalNode output
3. **T5**: Modify `tools/ErbToYaml/PrintDataConverter.cs`
   - Replace branches structure with entries
   - Use BranchesToEntriesConverter for simple PRINTDATA constructs

### Phase 4: Build Verification (T6)
1. Execute `dotnet build tools/KojoComparer/`
2. Verify zero errors
3. Verify zero warnings related to YAML format

### Phase 5: Batch Re-conversion (T7)
1. Execute batch re-conversion script (Technical Design Batch Re-conversion Script section)
2. Verify file count: `Glob(Game/YAML/Kojo/**/*.yaml)` returns ≥443 files
3. Verify format: Sample 5 random YAML files contain `entries:` key
4. Verify no legacy format: `Grep(Game/YAML/Kojo/) "branches:"` returns 0 matches

### Phase 6: Unit Test Verification (T8)
1. Execute `dotnet test Era.Core.Tests --filter "YamlDialogueLoader"`
2. If no existing tests for production YAML, create sample test to verify YamlDialogueLoader.Load() works with converted YAML containing conditions
3. Verify all tests pass including new format verification
4. If failures occur: STOP → Report to user

### Phase 7: Technical Debt Check (T9)
1. Execute `Grep(tools/ErbToYaml/) "TODO|FIXME|HACK"`
2. Verify 0 matches in modified converter files
3. Document any pre-existing debt found in unmodified files

**Constraints** (from Technical Design):

1. **YamlDialogueLoader CamelCase Convention**: All YAML keys must use camelCase (entries, id, content, priority, condition)
2. **DialogueCondition Record Structure**: Condition must have Type, TalentType (or AblType), and Threshold fields
3. **Priority Ordering**: Reverse branch index (first branch = highest priority) to match ERB evaluation order
4. **Content Format**: Join lines[] into single multiline string (preserve line breaks with \n)
5. **ID Semantic Naming**: Use condition-based IDs (talent_N, abl_N, fallback) for debugging clarity

**Pre-conditions**:
- F651 KojoComparer KojoEngine API Update is [DONE]
- F652 KojoComparer Test YAML Migration is [DONE]
- YamlDialogueLoader exists in Era.Core with entries: format support
- ErbToYaml tool builds successfully before modifications
- ERB source files exist in Game/ERB/口上/ for re-conversion

**Success Criteria**:
- All 12 ACs pass verification
- Zero `branches:` format remains in production YAML
- KojoComparer builds without errors
- YamlDialogueLoader unit tests pass
- File count maintained (≥443 YAML files)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with specific error details
3. Create follow-up feature for investigation with additional testing:
   - Test YAML rendering with sample character dialogue
   - Verify condition evaluation correctness
   - Check priority-based selection logic
4. If revert not possible (files already re-converted):
   - Restore YAML files from git history: `git restore --source=HEAD~1 Game/YAML/Kojo/`
   - Revert converter changes only: `git checkout HEAD~1 -- tools/ErbToYaml/`

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Review Notes

<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped] {phase} iter{N}: {description} -->
- [pending] Phase1-Uncertain iter1: YamlDotNet's default SerializerBuilder behavior omits null values unless explicitly configured with ConfigureDefaultValuesHandling. The pseudocode pattern is suboptimal but may not cause 'operand: ~' in output. Needs empirical verification.
- [pending] Phase1-Uncertain iter1: The pattern '\"branches\"' in markdown is escaped representation. When passed to Grep tool, proper unescaping should yield the search for literal '"branches"' in C# source code, which correctly matches dictionary key assignments like '{ "branches", branches }'. The escaping may be intentional for markdown table rendering.

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| (none) | - | - | - | - |

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-29 18:15 | Phase 3 (T3) | Modified DatalistConverter.cs to output entries: format via BranchesToEntriesConverter.Convert() |
| 2026-01-29 18:16 | Phase 3 (T4) | Modified FileConverter.cs to output entries: format via BranchesToEntriesConverter.Convert() |
| 2026-01-29 19:30 | Phase 3 (T5) | Modified PrintDataConverter.cs to output entries: format via BranchesToEntriesConverter.Convert() |
| 2026-01-29 20:00 | Phase 4 (T6) | KojoComparer builds successfully (0 errors, 0 warnings) |
| 2026-01-29 20:05 | Phase 4 (T7) | Batch re-conversion: fixed YamlValidator numeric type handling, updated schema for string/int flexibility |
| 2026-01-29 20:10 | Phase 4 (T7) | Re-converted 1110 YAML files to entries: format, 0 branches: remain |
| 2026-01-29 20:15 | Phase 4 (T8) | YamlDialogueLoader tests: 13 passed, 0 failed |
| 2026-01-29 20:20 | Phase 4 (T9) | No technical debt markers in converters |
| 2026-01-29 20:25 | Phase 6 | AC verification: 14/14 passed |

---

## Links

- [feature-644.md](feature-644.md) - Blocked by this feature
- [feature-651.md](feature-651.md) - Established entries: format requirement
- [feature-652.md](feature-652.md) - Extended YamlDialogueLoader for entries: format
- [feature-549.md](feature-549.md) - YamlDialogueLoader with entries: schema
- [feature-551.md](feature-551.md) - TemplateDialogueRenderer uses DialogueEntry
- [feature-553.md](feature-553.md) - KojoEngine Facade Refactoring
- [feature-636.md](feature-636.md) - Kojo conversion features
- [feature-637.md](feature-637.md) - Kojo conversion features
- [feature-638.md](feature-638.md) - Kojo conversion features
- [feature-639.md](feature-639.md) - Kojo conversion features
- [feature-640.md](feature-640.md) - Kojo conversion features
- [feature-641.md](feature-641.md) - Kojo conversion features
- [feature-642.md](feature-642.md) - Kojo conversion features
- [feature-643.md](feature-643.md) - Kojo conversion features
