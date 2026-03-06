# Feature 750: YAML TALENT Condition Migration

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
ERB==YAML equivalence testing requires both implementations to execute the same branch under the same conditions. Currently, YAML files have multiple branches but all conditions are empty ({}), causing branch selection mismatch.

### Problem (Current Issue)
F706 batch testing discovered 601/650 failures due to YAML branch selection mismatch:
- ERB uses IF TALENT:恋人...ELSEIF TALENT:恋慕...ELSEIF TALENT:思慕...ELSE branching
- YAML files have all TALENT branches but with empty conditions ({})
- YamlRunner selects first matching branch (恋人 branch)
- ErbRunner with empty state (representative test) selects ELSE branch
- Result: Different content comparison = MISMATCH

### Goal (What to Achieve)
1. Migrate YAML files to include proper TALENT conditions matching ERB branching logic
2. Enable YamlRunner to select correct branch based on test state
3. Achieve 650/650 PASS in KojoComparer batch verification (F706 AC7)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Successor | F706 | [BLOCKED] | KojoComparer batch infrastructure (blocked BY this feature) |
| Related | F725 | [DONE] | YamlRunner K{N} format and branches parser support |
| Related | F746 | [DONE] | YAML subset matching implementation |

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why do 601/650 equivalence tests fail with YAML branch selection mismatch?**
   Because KojoBranchesParser selects the LAST branch with empty condition (`condition: {}`), while ERB with empty state executes the ELSE branch. When YAML files have all branches with `condition: {}`, KojoBranchesParser selects the last one regardless of actual ERB branching logic.

2. **Why do YAML files have empty conditions when ERB uses TALENT conditions?**
   Because ErbToYaml (DatalistConverter.cs) calls `ParseCondition()` which uses TalentConditionParser to parse TALENT conditions. When the parser cannot find the talent name in Talent.csv (line 218-224), it returns empty `{}` as a graceful fallback.

3. **Why does TalentConditionParser fail to find TALENT names like "恋人" and "思慕"?**
   Because Talent.csv only defines TALENT index 3 as "恋慕" (line 7: `3,恋慕,;愛情に似た感情を抱いている状態`). The ERB uses undefined constants "恋人" and "思慕" which are aliases, not actual Talent.csv entries. TalentCsvLoader.GetTalentIndex("恋人") returns null.

4. **Why are "恋人" and "思慕" used in ERB but not defined in Talent.csv?**
   These are likely defined as ERB constants or aliases elsewhere (e.g., in DIM.ERH or constants section), not as actual Talent.csv rows. ErbToYaml's TalentCsvLoader only reads Talent.csv and doesn't resolve ERB-level constants.

5. **Why wasn't this discovered during ErbToYaml conversion?**
   ErbToYaml outputs warnings to stderr (`Console.Error.WriteLine($"Warning: Talent '{talentRef.Name}' not found in Talent.csv")`) but continues processing with empty conditions. These warnings were not treated as errors, and the YAML files were generated with `condition: {}` silently.

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 601/650 test failures with MISMATCH | YAML files have empty conditions, YamlRunner selects last empty-condition branch while ERB selects based on TALENT state |
| YamlRunner selects wrong branch | KojoBranchesParser.Parse() uses `LastOrDefault(b => IsEmptyCondition(b.Condition))` - all branches have empty conditions so last one is selected |
| YAML files have `condition: {}` for TALENT branches | TalentCsvLoader.GetTalentIndex() returns null for "恋人", "思慕" (not in Talent.csv) → ParseCondition returns empty {} |
| ERB constants 恋人/思慕 not resolved | These are ERB-level aliases/constants, not Talent.csv entries. Only "恋慕" (index 3) is defined in CSV |

### Conclusion

**The root cause is DUAL: Missing Talent.csv entries AND ErbToYaml silent fallback behavior.**

1. **Primary Issue - Missing Talent.csv Entries**: ERB uses TALENT constants "恋人" and "思慕" which are NOT defined in Talent.csv. Only "恋慕" (index 3) exists. These are likely ERB-level constants defined elsewhere (e.g., `@恋人 = 0`, `@恋慕 = 3`, `@思慕 = 2`).

2. **Secondary Issue - Silent Fallback**: ErbToYaml's DatalistConverter.ParseCondition() returns empty `{}` when talent lookup fails, only logging a warning to stderr. This silent fallback masked the underlying data gap during conversion.

3. **Tertiary Issue - KojoBranchesParser Selection Logic**: Even with conditions, the current implementation assumes empty-condition branches should be selected (for ELSE fallback), but when ALL branches have empty conditions, it incorrectly selects the last one.

**Investigation Findings from YAML Files**:
- Total `condition: {}` occurrences: ~1316 across 397 files
- Total `TALENT:` conditions that WORKED: 67 files (美鈴 character with index 3 "恋慕")
- The working conditions show format: `TALENT: 3: ne: 0` (talent index 3, not equal to 0)

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F706 | [BLOCKED] | Successor | KojoComparer Full Equivalence Verification - blocked until YAML conditions are correct |
| F725 | [DONE] | Created infrastructure | KojoBranchesParser for branches format, YamlRunner K{N} format support |
| F746 | [DONE] | Related fix | Subset matching for PRINTDATA/DATALIST, CALLNAME normalization |
| F349 | [DONE] | Created tool | ErbToYaml DatalistConverter with TalentConditionParser |
| F361 | [DONE] | Added validation | Schema validation for dialogue YAML |
| F634 | [DONE] | Batch conversion | ErbToYaml batch processing with FileConverter |

### Pattern Analysis

This is a **silent failure pattern** combined with **data dependency gap**:

1. **Silent failure**: ErbToYaml logs warnings but continues with empty conditions, masking underlying issues
2. **Data gap**: Talent.csv lacks entries for ERB constants that are defined elsewhere
3. **Cascading effect**: Empty conditions in YAML cause branch selection mismatch in YamlRunner

Similar patterns may exist for other ERB constants not defined in CSV files. Future conversion tools should treat such gaps as errors, not warnings.

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Either add missing Talent.csv entries OR implement ERB constant resolution in ErbToYaml OR migrate YAML files programmatically |
| Scope is realistic | PARTIAL | 601 files need condition migration. Batch update is feasible if TALENT mapping is known. Manual review may be needed for verification. |
| No blocking constraints | PARTIAL | Requires understanding ERB constant definitions (恋人, 思慕, etc.). May need ERB parser enhancement or manual mapping table. |

**Verdict**: FEASIBLE with investigation

The implementation requires:
1. **Identify TALENT constant mappings**: Find where 恋人, 思慕, etc. are defined in ERB (likely DIM.ERH or constants)
2. **Create migration tool**: Script to update YAML files with correct conditions based on ERB-TALENT mapping
3. **Update KojoBranchesParser**: Implement proper condition evaluation (not just empty-condition selection)

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Game/YAML/Kojo/**/*.yaml | Update | ~601 files need TALENT conditions migrated from empty {} to proper format |
| tools/YamlTalentMigrator/ | Create | New project for standalone YAML migration script with Program.cs entry point |
| tools/ErbToYaml/TalentCsvLoader.cs | Optional Update | Could add support for ERB constant aliases |
| tools/KojoComparer/KojoBranchesParser.cs | Update | Implement condition evaluation based on test state |
| Game/CSV/Talent.csv | Possibly Update | May need to add 恋人, 思慕 entries if they should be formal talents |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| YAML schema must be preserved | dialogue-schema.json | MEDIUM - Condition format { TALENT: { idx: { op: value } } } must be valid |
| Backward compatibility | Existing YAML files | HIGH - Migration must not break working files |
| ERB constant resolution | ErbToYaml design | MEDIUM - Tool uses Talent.csv only, not ERB constants |
| KojoBranchesParser state injection | F706/F746 | MEDIUM - Need state context to evaluate conditions |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| ERB constants 恋人/思慕 have different definitions per character | LOW | HIGH | Analyze ERB files to verify consistent TALENT mapping |
| Migration tool incorrectly maps TALENT conditions | MEDIUM | HIGH | Validate with sample equivalence tests before batch migration |
| KojoBranchesParser condition evaluation complexity | MEDIUM | MEDIUM | Start with simple TALENT conditions, expand incrementally |
| Some YAML files have valid conditions, migration overwrites | LOW | MEDIUM | Skip files with existing non-empty conditions |
| ERB uses compound conditions (TALENT:x && TALENT:y) | MEDIUM | MEDIUM | Parse compound conditions from ERB and convert to YAML format |

---

## Links
- [feature-706.md](feature-706.md) - KojoComparer Full Equivalence Verification (blocked by this)
- [feature-725.md](feature-725.md) - YamlRunner branches support
- [feature-746.md](feature-746.md) - Subset matching implementation
- [feature-748.md](feature-748.md) - Intro line extraction to ErbToYaml
- [feature-349.md](feature-349.md) - ErbToYaml DatalistConverter (created the conversion tool)
- [feature-634.md](feature-634.md) - ErbToYaml batch conversion
- [feature-751.md](feature-751.md) - TALENT semantic mapping validation (handoff)
- [feature-752.md](feature-752.md) - Compound condition support (handoff)
- [feature-753.md](feature-753.md) - Migration script parameterization (handoff)

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "both implementations to execute the same branch" | YamlRunner must select branch based on test state conditions | AC#4, AC#5 |
| "ERB==YAML equivalence testing requires...same conditions" | YAML files must have proper TALENT conditions matching ERB | AC#1, AC#2, AC#3 |
| "achieve 650/650 PASS" | All equivalence tests pass after migration | AC#6, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 0 | TALENT name resolution mechanism investigated | file | Grep(Game/agents/f750-investigation-report.md) | contains | "TALENT:恋人" | [x] |
| 1 | TALENT constant investigation documented | file | Grep(Game/agents/f750-investigation-report.md) | contains | "resolution mechanism" | [x] |
| 2 | YAML files have non-empty TALENT conditions | file | Grep(Game/YAML/Kojo/) | contains | "TALENT:" | [x] |
| 3 | Only ELSE branches have empty conditions | exit_code | Bash(python -c "...non-ELSE empty check...") | equals | 0 | [x] |
| 4 | KojoBranchesParser evaluates conditions | file | Grep(tools/KojoComparer/KojoBranchesParser.cs) | contains | "EvaluateCondition" | [x] |
| 4a | Parse method accepts state parameter | file | Grep(tools/KojoComparer/KojoBranchesParser.cs) | contains | "state = null" | [x] |
| 4b | YamlRunner passes state to parser | file | Grep(tools/KojoComparer/YamlRunner.cs) | matches | "Parse.*state" | [x] |
| 5 | Branch selection uses test state | test | dotnet test tools/KojoComparer.Tests/ --filter KojoBranchesParser | succeeds | - | [x] |
| 6 | Batch verification improvement | exit_code | Bash(dotnet run --project tools/KojoComparer -- --all) | succeeds | - | [B] |
| 7 | TALENT-aware branch selection implemented | output | Bash(dotnet run --project tools/KojoComparer -- --all) | contains | "650/650" | [B] |
| 8 | No regression in existing tests | test | dotnet test Era.Core.Tests/ | succeeds | - | [x] |
| 9 | F751 DRAFT file exists | file | Game/agents/feature-751.md | exists | - | [x] |
| 10 | F752 DRAFT file exists | file | Game/agents/feature-752.md | exists | - | [x] |
| 11 | F753 DRAFT file exists | file | Game/agents/feature-753.md | exists | - | [x] |

**Note**: 11 ACs (8 implementation + 3 DRAFT verification) is within typical infra range (8-15).

### AC Details

**AC#1: TALENT constant resolution investigation documented**
- Verifies that investigation findings are documented (known: 恋慕=3, unknown: 恋人/思慕 resolution mechanism)
- Documents whether mapping is confirmed or requires engine investigation
- Method: Grep feature file for investigation results with evidence

**AC#2: YAML files have non-empty TALENT conditions**
- Verifies YAML files have proper TALENT conditions in format `TALENT: N:` (index-based)
- Pattern matches actual YAML condition format with numeric talent index
- Method: Grep YAML directory for condition pattern

**AC#3: Only ELSE branches have empty conditions**
- Baseline: ~1316 `condition: {}` occurrences before migration (all branches)
- Target: Zero empty conditions in non-ELSE branches (ELSE branches may remain empty)
- Result: 576 empty conditions remain, ALL are ELSE branches (last branch per file)
- Verification: Python script checks that only the last branch in each file has empty condition
- Command: Validate that non-ELSE branches have 0 empty conditions

**AC#4: KojoBranchesParser evaluates conditions**
- KojoBranchesParser must have condition evaluation logic
- Current behavior: selects last empty-condition branch
- Required: evaluate TALENT conditions against test state
- Method: Grep for EvaluateCondition method in parser

**AC#4a: Parse method accepts state parameter**
- Verifies Parse method signature includes state parameter for branch selection
- Method: Grep for state parameter in method signature

**AC#5: Branch selection uses test state**
- Unit tests verify branch selection logic works correctly
- Tests cover: TALENT=0 (恋人) → branch 0, TALENT=3 (恋慕) → branch 1, etc.
- Method: dotnet test on KojoComparer.Tests

**AC#6: Batch verification improvement**
- Batch verification runs without error after migration
- Command: `dotnet run --project tools/KojoComparer -- --batch`
- Method: Exit code 0 indicates successful execution

**AC#7: TALENT-aware branch selection implemented**
- Verifies branch selection works with TALENT conditions (not just empty state)
- Split validation: 7a for baseline (empty state), 7b for TALENT states (pending investigation)
- Method: Batch output shows consistent branch selection behavior
- **Contingency**: If T0 investigation reveals TALENT:恋人/思慕 cannot be resolved, AC#7 Expected will be revised to reflect achievable count based on investigation results

**AC#8: No regression in existing tests**
- Era.Core.Tests must continue to pass after changes
- Ensures YAML parsing changes don't break existing functionality
- Method: dotnet test on Era.Core.Tests

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Four-phase migration with ERB analysis, batch scripting, parser enhancement, and verification.**

This feature addresses a data dependency gap where ERB uses TALENT constants (恋人, 思慕, 恋慕) not defined in Talent.csv but as ERB-level constants. The YAML files currently have empty conditions `{}` because ErbToYaml's TalentConditionParser could not resolve these constants.

**Key Insight from Investigation**:
- Only "恋慕" (index 3) exists in Talent.csv
- "恋人" and "思慕" are used in ERB but not defined in Talent.csv
- DIM.ERH contains `写真詳細_恋人 = 4` but no direct TALENT constant definitions
- ERB branching pattern: `IF TALENT:恋人 / ELSEIF TALENT:恋慕 / ELSEIF TALENT:思慕 / ELSE`
- This suggests a semantic relationship: 恋人 (lover) > 恋慕 (romantic love) > 思慕 (admiration)

**Analysis Conclusion**: These are likely implicit TALENT aliases with a progression hierarchy. Based on ERB patterns and semantic meaning:
- 恋人 = TALENT:3 with higher threshold (e.g., TALENT:3 >= 2)
- 恋慕 = TALENT:3 (standard check, TALENT:3 != 0)
- 思慕 = TALENT:3 with lower threshold (e.g., TALENT:3 == 1)

**However**, this approach assumes a mapping that may not match actual ERB semantics. A safer approach is to:
1. Document the observed ERB branching patterns
2. Migrate YAML conditions to match ERB branch order semantics
3. Update KojoBranchesParser to select based on TALENT state (not just empty conditions)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 0 | Investigate Emuera TALENT name resolution mechanism for undefined names (恋人, 思慕) via engine analysis or empirical testing |
| 1 | Document TALENT constant investigation findings in Technical Design section (恋人/思慕/恋慕 analysis) |
| 2 | Create C# migration script that updates YAML files with TALENT conditions based on branch index position |
| 3 | Script execution reduces `condition: {}` count from ~1316 to <50 (only genuine ELSE branches remain) |
| 4 | Add `EvaluateCondition()` method to KojoBranchesParser that checks TALENT state against conditions |
| 5 | Create unit tests in KojoBranchesParserTests.cs verifying branch selection with different TALENT states |
| 6 | Run `dotnet run --project tools/KojoComparer -- --batch` after migration and parser update |
| 7 | Batch output contains "650/650" PASS count (verified via stdout parsing) |
| 8 | Run `dotnet test Era.Core.Tests/` to ensure YAML parsing changes don't break existing functionality |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| TALENT constant resolution | A: Add to Talent.csv<br>B: Extend TalentCsvLoader<br>C: Infer from ERB semantics<br>D: Match ERB branch order | D | Adding to Talent.csv would require understanding actual ERB semantics which are implicit. Option D defers semantic mapping to test state setup, focusing on structural correctness. |
| Migration approach | A: Re-run ErbToYaml with fixed parser<br>B: Create standalone migration script<br>C: Manual YAML edits | B | Re-running ErbToYaml risks overwriting other fixes (F748 intro lines, F746 CALLNAME). Standalone script is surgical and auditable. |
| Condition format | A: Name-based (TALENT:恋人)<br>B: Index-based (TALENT:3) with operators | B | YAML schema already uses numeric indices. Parser can evaluate `{ TALENT: { 3: { ne: 0 } } }` format. |
| KojoBranchesParser logic | A: Select first matching condition<br>B: Select last empty-condition<br>C: Evaluate conditions with state | C | Current "last empty" is wrong. Need actual condition evaluation to match ERB IF/ELSEIF/ELSE semantics. |
| Test state injection | A: Hard-code state in parser<br>B: Add state parameter to Parse()<br>C: Use TestCase.State from batch | C | TestCase already has State dictionary. Pass it to parser for evaluation. |

### TALENT Constant Mapping Documentation

**Investigation Findings**:

Based on ERB analysis (KOJO_K10_会話親密.ERB lines 36-163):

```erb
IF TALENT:恋人
    ; Branch 0: Lover relationship (highest intimacy)
ELSEIF TALENT:恋慕
    ; Branch 1: Romantic love (Talent.csv index 3)
ELSEIF TALENT:思慕
    ; Branch 2: Admiration (lower intimacy)
ELSE
    ; Branch 3: No romantic relationship
```

**Engine Resolution Investigation Required**:

Current Evidence:
- **恋慕** = TALENT:3 (confirmed in Talent.csv line 7)
- **恋人**, **思慕** = NOT defined in Talent.csv (indices 0-2 have different names)
- COMF352.ERB line 24 shows `TALENT:TARGET:恋人 = 1` - proving ERB uses these as variables
- Game runs without errors despite undefined TALENT constants

**Research Gap**: How does Emuera engine resolve TALENT:恋人 and TALENT:思慕 when not defined in Talent.csv?

**Possible Mechanisms**:
1. Dynamic TALENT slot allocation for undefined names
2. eraTW compatibility layer with predefined mappings
3. Name-based matching to existing TALENT indices

**Interim Migration Strategy** (Pending Investigation):
Use branch position-based mapping for YAML migration. Requires engine investigation to confirm:
- Branch 0 (恋人) → Requires: investigation of TALENT:恋人 resolution mechanism
- Branch 1 (恋慕) → TALENT:3 ne 0 (confirmed working in existing YAMLs)
- Branch 2 (思慕) → Requires: investigation of TALENT:思慕 resolution mechanism
- Branch 3 (ELSE) → State: empty or all TALENT:0

### Migration Script Design

**Script: tools/YamlTalentMigrator/Program.cs**

```csharp
// Phase 1: Analyze ERB files to extract TALENT branching patterns
Dictionary<string, List<TalentBranch>> ExtractTalentBranches(string erbDir)
{
    // For each KOJO_*.ERB file:
    // - Parse IF TALENT:X / ELSEIF TALENT:Y / ELSE blocks
    // - Extract talent names and branch order
    // - Return mapping: FunctionName → List<TalentBranch>
}

// Phase 2: Map ERB branches to YAML files
Dictionary<string, YamlBranchMapping> MapErbToYaml(
    Dictionary<string, List<TalentBranch>> erbBranches,
    string yamlDir)
{
    // For each YAML file with branches:
    // - Match to ERB function by character + situation
    // - Align YAML branch index to ERB branch conditions
    // - Return mapping: YamlFile → List<(branchIndex, talentCondition)>
}

// Phase 3: Update YAML files with conditions
void UpdateYamlConditions(Dictionary<string, YamlBranchMapping> mappings)
{
    // For each YAML file:
    // - Deserialize with YamlDotNet
    // - Update branches[i].condition with mapped TALENT condition
    // - Serialize back with preserved formatting
}
```

**Condition Format**:
```yaml
condition:
  TALENT:
    3:  # Talent index
      ne: 0  # Operator: ne/eq/gt/gte/lt/lte
```

**Edge Cases**:
- YAML files with existing non-empty conditions → Skip (preserve existing)
- ERB files with compound conditions (TALENT:X && TALENT:Y) → Warn and skip (requires manual review)
- YAML files with mismatched branch count → Warn and skip

### KojoBranchesParser Enhancement

**Current Behavior (WRONG)**:
```csharp
// Line 42: Select LAST branch with empty condition
var selectedBranch = fileData.Branches.LastOrDefault(b => IsEmptyCondition(b.Condition));
```

**New Behavior (CORRECT)**:
```csharp
public DialogueResult Parse(string yamlContent, Dictionary<string, int>? state = null)
{
    var fileData = _deserializer.Deserialize<BranchesFileData>(yamlContent);

    // Select FIRST branch where condition evaluates to TRUE
    // If no conditions match, select LAST branch with empty condition (ELSE fallback)
    var selectedBranch = fileData.Branches
        .FirstOrDefault(b => EvaluateCondition(b.Condition, state))
        ?? fileData.Branches.LastOrDefault(b => IsEmptyCondition(b.Condition));

    // ... rest unchanged
}

private bool EvaluateCondition(Dictionary<string, object>? condition, Dictionary<string, int>? state)
{
    if (condition == null || condition.Count == 0)
        return false; // Empty condition is only for ELSE fallback

    if (state == null)
        return false; // No state → only match empty conditions

    // Example condition: { "TALENT": { "3": { "ne": 0 } } }
    if (condition.TryGetValue("TALENT", out var talentObj) && talentObj is Dictionary<string, object> talentDict)
    {
        foreach (var (indexStr, opObj) in talentDict)
        {
            if (!int.TryParse(indexStr, out var talentIndex))
                continue;

            var stateKey = $"TALENT:{talentIndex}";
            var stateValue = state.GetValueOrDefault(stateKey, 0);

            if (opObj is Dictionary<string, object> opDict)
            {
                foreach (var (op, expectedObj) in opDict)
                {
                    if (!int.TryParse(expectedObj?.ToString(), out var expected))
                        continue;

                    var result = op switch
                    {
                        "eq" => stateValue == expected,
                        "ne" => stateValue != expected,
                        "gt" => stateValue > expected,
                        "gte" => stateValue >= expected,
                        "lt" => stateValue < expected,
                        "lte" => stateValue <= expected,
                        _ => false
                    };

                    if (!result)
                        return false; // All conditions must match (AND logic)
                }
            }
        }

        return true; // All TALENT conditions matched
    }

    return false;
}
```

**State Format in TestCase**:
```csharp
var state = new Dictionary<string, int>
{
    { "TALENT:3", 1 }  // TALENT index 3 = 1 (恋慕 state)
};
```

### Integration with F706 Batch Infrastructure

**TestCase Structure** (already exists in F706):
```csharp
public class TestCase
{
    public int CharacterId { get; set; }
    public int ComId { get; set; }
    public string FunctionName { get; set; }
    public Dictionary<string, int> State { get; set; }  // ← Already exists!
}
```

**Current F706 State Setup** (representative test):
```csharp
State = new Dictionary<string, int>() // Empty state for representative test
```

**Post-Migration State Setup** (for TALENT-aware verification):
```csharp
// Option A: Default to ELSE branch (empty state)
State = new Dictionary<string, int>()

// Option B: Add TALENT states for specific branch testing (future enhancement)
State = new Dictionary<string, int> { { "TALENT:3", 1 } }
```

**Key Point**: F706's representative test uses empty state, which should select ELSE branch. After migration:
- YAML Branch 0-2: Have TALENT conditions → `EvaluateCondition()` returns false with empty state
- YAML Branch 3 (ELSE): Has `condition: {}` → `IsEmptyCondition()` returns true → Selected

This matches ERB behavior where empty TALENT state executes ELSE branch.

### Migration Script Revised Approach

**Handling Partially-Migrated Files**:

1. **Detect Current State**: Check each branch's condition
   - Empty: `condition: {}` or `condition: ` (null)
   - Non-empty: Any condition present (e.g., existing TALENT:3 conditions in 美鈴 files)

2. **Selective Migration Logic**:
   - Skip files where ALL branches have non-empty conditions
   - For partially-migrated files: only fill remaining empty conditions
   - Preserve existing conditions (e.g., TALENT:3 ne 0 in 美鈴 Branch 1)

3. **Position-Based Assignment** (For empty conditions only):
   - Branch 0: Pending TALENT:恋人 investigation
   - Branch 1: `{ TALENT: { 3: { ne: 0 } } }` (if empty - 恋慕 confirmed)
   - Branch 2: Pending TALENT:思慕 investigation
   - Branch 3+: `{}` (ELSE fallback)

**Special Handling**: 美鈴 character files already have TALENT:3 conditions in some branches, requiring selective migration.

**Validation**: After migration, run KojoComparer on sample files to verify branch selection matches ERB behavior.

**Dry-Run Mode**: Migration script supports `--dry-run` flag that logs proposed changes without writing files. Implementation: `void UpdateYamlConditions(..., bool dryRun = false)` - when dryRun is true, output changes to stdout/log instead of writing to files.

### Verification Strategy

**Phase 1: Sample Verification** (AC#6)
- Select 5-10 test cases with TALENT branches
- Run KojoComparer with empty state
- Verify ERB and YAML both select ELSE branch
- Verify content matches

**Phase 2: Full Batch** (AC#7)
- Run `dotnet run --project tools/KojoComparer -- --batch`
- Parse output for "650/650 PASS" metric
- All test cases must MATCH

**Phase 3: Regression** (AC#8)
- Run `dotnet test Era.Core.Tests/`
- Ensure YAML parsing changes don't break DialogueRenderer, YamlKojoLoader, etc.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0,1 | Investigate Emuera TALENT name resolution mechanism for undefined names (恋人, 思慕) and create Game/agents/f750-investigation-report.md | [x] |
| 1 | 1 | Document TALENT constant investigation findings with evidence in feature file | [x] |
| 2 | 2,3 | Create YAML migration script | [x] |
| 3 | 4 | Implement EvaluateCondition method in KojoBranchesParser with state-based logic | [x] |
| 4 | 5 | Create unit tests for KojoBranchesParser branch selection with TALENT conditions | [x] |
| 5 | 6,7 | Run batch verification and verify TALENT-aware branch selection | [B] |
| 6 | 8 | Run Era.Core.Tests to ensure no regression | [x] |
| 7 | 9 | Create F751 DRAFT for TALENT semantic mapping validation | [x] |
| 8 | 10 | Create F752 DRAFT for compound condition support | [x] |
| 9 | 11 | Create F753 DRAFT for migration script parameterization | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. DO NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 0 | tech-investigator | opus | T0,T1 | ERB analysis, Emuera behavior | TALENT resolution mechanism documented |
| 1 | implementer | sonnet | T2 | Investigation results, migration script spec | Migration tool (blocked until Phase 0 completes) |
| 2 | implementer | sonnet | T3,T4 | KojoBranchesParser enhancement spec | Parser with condition evaluation + tests |
| 3 | ac-tester | haiku | T5,T6 | Batch verification commands, regression tests | Test results + no regression |

**Constraints** (from Technical Design):
1. YAML schema must be preserved - condition format `{ TALENT: { idx: { op: value } } }` must be valid per dialogue-schema.json
2. Migration must skip files with existing non-empty conditions to preserve working YAML files
3. KojoBranchesParser must maintain backward compatibility - ELSE fallback with empty conditions must still work
4. Migration script must be auditable - log all changes for review

**Pre-conditions**:
- F706 KojoComparer batch infrastructure exists with TestCase.State support
- YAML files in Game/YAML/Kojo/ have branches format with condition fields
- KojoBranchesParser.cs exists and currently uses LastOrDefault selection
- Era.Core.Tests passing baseline established

**Success Criteria**:
1. All 8 ACs pass verification
2. Empty condition count reduced from ~1316 to <50 (only genuine ELSE branches)
3. Batch verification shows 650/650 PASS (no MISMATCH failures)
4. Era.Core.Tests continues to pass (no regression)
5. TALENT constant mapping documented in feature file for future reference

**Rollback Plan**:

If issues arise after deployment:
1. Revert migration script execution by restoring YAML files from git (before script run)
2. Revert KojoBranchesParser changes with `git revert <commit>`
3. Notify user of rollback with error details
4. Create follow-up feature for investigation with additional analysis of TALENT semantic mapping

**Migration Script Execution Steps** (Phase 1, Task 2):

1. **Baseline Count**: Run `grep -r "condition: {}" Game/YAML/Kojo/ | wc -l` → ~1316 expected
2. **Create Migration Script**: `tools/YamlTalentMigrator/Program.cs` with:
   - YAML file discovery (Game/YAML/Kojo/**/*.yaml with branches format)
   - Condition analysis (count empty vs. non-empty conditions per file)
   - Branch-position-based TALENT assignment (PENDING investigation):
     - Branch 0 → PENDING: Requires 恋人 resolution mechanism investigation
     - Branch 1 → `{ TALENT: { 3: { ne: 0 } } }` (恋慕 - confirmed in Talent.csv)
     - Branch 2 → PENDING: Requires 思慕 resolution mechanism investigation
     - Branch 3+ → `{}` (ELSE fallback - keep empty)
   - NOTE: Migration script creation blocked until TALENT name resolution investigation completes
   - Skip logic: If file has ANY non-empty condition, skip entire file (preserve existing)
   - Logging: Output migrated file count and skipped file count to stdout
3. **Dry Run**: Execute with `--dry-run` flag, verify log output shows reasonable file counts
4. **Execute Migration**: Run without --dry-run, update YAML files in place
   - **Gate**: T0 investigation must be complete. If TALENT:恋人/思慕 cannot be resolved, migration script must be revised or scope adjusted
5. **Verify Count**: Re-run `grep -r "condition: {}" Game/YAML/Kojo/ | wc -l` → expect <50

**KojoBranchesParser Enhancement Steps** (Phase 2, Task 3):

1. **Add State Parameter**: Update `Parse(string yamlContent, Dictionary<string, int>? state = null)` signature
2. **Update YamlRunner Integration**: Modify `YamlRunner.RenderBranchesFormat()` to extract state from context dictionary and pass to `parser.Parse(yamlContent, state)`. State extraction converts context nested format `{"TALENT": {"3": value}}` to parser flat format `{"TALENT:3": value}`
3. **Implement EvaluateCondition**: Method signature `private bool EvaluateCondition(Dictionary<string, object>? condition, Dictionary<string, int>? state)`
   - Return false for null/empty conditions
   - Return false if state is null
   - Parse TALENT conditions: `{ "TALENT": { "3": { "ne": 0 } } }` format
   - Evaluate operators: eq, ne, gt, gte, lt, lte
   - Return true if ALL conditions match (AND logic)
3. **Update Selection Logic**: `FirstOrDefault(b => EvaluateCondition(b.Condition, state)) ?? LastOrDefault(b => IsEmptyCondition(b.Condition))`
4. **Build Verification**: Run `dotnet build tools/KojoComparer/` → must succeed

**Unit Test Creation Steps** (Phase 2, Task 4):

1. **Create Test File**: `tools/KojoComparer.Tests/KojoBranchesParserConditionTests.cs`
2. **Test Cases**:
   - `Parse_WithTalent3Equals1_SelectsBranch1`: State `{ "TALENT:3": 1 }` → selects branch with `TALENT: 3: { ne: 0 }`
   - `Parse_WithEmptyState_SelectsElseBranch`: State `{}` → selects last branch with `condition: {}`
   - `Parse_WithTalent0Equals1_SelectsBranch0`: State `{ "TALENT:0": 1 }` → selects branch with `TALENT: 0: { ne: 0 }`
   - `Parse_WithNoMatchingCondition_SelectsElse`: State `{ "TALENT:5": 1 }` → selects ELSE branch (no match)
3. **Run Tests**: `dotnet test tools/KojoComparer.Tests/KojoBranchesParserConditionTests.cs` → all pass

**Batch Verification Steps** (Phase 3, Task 5):

1. **Sample Verification**: Run `dotnet run --project tools/KojoComparer -- --file Game/YAML/Kojo/10_魔理沙/K10_会話親密_0.yaml` → verify MATCH
2. **Full Batch**: Run `dotnet run --project tools/KojoComparer -- --batch 2>&1 | tee kojo_results.txt`
3. **Parse Output**: `grep "650/650" kojo_results.txt` → verify presence
4. **Check for MISMATCH**: `grep "MISMATCH" kojo_results.txt | wc -l` → expect 0

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| TALENT semantic mapping validation | Circular dependency between F750 and F706 | Feature | F751 | Task 7 |
| Compound condition support (TALENT:X && TALENT:Y) | May be needed if discovered during investigation | Feature | F752 | Task 8 |
| Migration script parameterization | Enhancement for maintainability | Feature | F753 | Task 9 |

**Note**: F751/F752/F753 are [DRAFT] files. After F750 reaches [DONE], these require `/fc {ID}` → `/fl {ID}` → `/run {ID}` workflow completion.

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-05 | Phase 0 | T0 complete - TALENT mappings confirmed: 恋人=16, 恋慕=3, 思慕=17 via Talent.yaml |
| 2026-02-05 | Phase 0 | T1 complete - Investigation documented in f750-investigation-report.md |
| 2026-02-05 | Phase 1 | T2 complete - Migration script created and executed: 409 files, 1488 branches updated |
| 2026-02-05 | Phase 1 | DEVIATION: AC#3 target (<50) was unrealistic. 576 empty conditions remain but ALL are ELSE branches (last branch per file). Non-ELSE branches: 0 empty. Migration correct. |
| 2026-02-05 | Phase 2 | T3 complete - EvaluateCondition() added to KojoBranchesParser with state parameter |
| 2026-02-05 | Phase 2 | T4 complete - 8 unit tests pass for branch selection logic |
| 2026-02-05 | Phase 3 | T6 complete - Era.Core.Tests: 1599/1599 pass (no regression) |
| 2026-02-05 | Phase 3 | DEVIATION: T5 batch verification (AC#6, AC#7) blocked - ERB execution timeout in KojoComparer batch mode (PRE-EXISTING F706 issue) |
| 2026-02-05 | Post-Review | DEVIATION: NEEDS_REVISION - AC#3 Expected inconsistent with actual outcome. Fixed: Updated AC#3 to verify "only ELSE branches have empty conditions" (semantic correctness) instead of raw count threshold |
| 2026-02-05 | Phase 9 | User decision: Option B - Waive [B] ACs. AC#6,7 use F706 batch which is Successor of F750 (not circular - design issue). Core implementation complete. |

## 残課題

| Issue | Type | Destination | Note |
|-------|------|-------------|------|
| AC#6 batch verification | Waived [B] | F706 | F706 batch機能でF750の移行結果を検証 |
| AC#7 650/650 PASS | Waived [B] | F706 | F706 unblock後に自動検証可能 |

## Review Notes
- [resolved-applied] Phase1 iter1: TALENT constant mapping documentation contains factual errors. ERB files use TALENT:恋人 and TALENT:思慕 as direct TALENT references (1221 occurrences across 43 files), but these identifiers DO NOT exist in Talent.csv (which only defines index 3=恋慕). The spec claims 恋人=TALENT:0 and 思慕=TALENT:2 but provides no evidence - Talent.csv has no entries for indices 0 or 2 named 恋人 or 思慕. DIM.ERH only has 写真詳細_恋人=4 which is unrelated. This is NOT a matter of aliases - these are undefined TALENT constants that the ERB engine resolves somehow.
- [resolved-applied] Phase2 iter1: Migration script assigns Branch 0 → TALENT:0, Branch 1 → TALENT:3, Branch 2 → TALENT:2 based on position. But examining KOJO_K1_会話親密.ERB (lines 29-156), ERB has: IF TALENT:恋人 (branch 0), ELSEIF TALENT:恋慕 (branch 1), ELSEIF TALENT:思慕 (branch 2), ELSE (branch 3). The YAML K1_会話親密_0.yaml has 4 branches where branch 1 already has 'TALENT: 3: ne: 0' (恋慕). This shows 美鈴 character YAMLs already have PARTIAL conditions - not all empty. Migration script logic to skip files with ANY non-empty condition would skip these partially-migrated files.
- [resolved-applied] Phase2 iter1: AC#3 expects empty condition count reduced to <50. Grep results show 'condition: {}' and 'condition:' (empty/null) patterns exist in YAML files. The AC only counts 'condition: {}' pattern. YAML files also use 'condition: ' (trailing whitespace/null) which is semantically equivalent to empty condition but won't be counted.
- [resolved-applied] Phase6 iter1: Feature references F751, F752, F753 in Deferred Items do not exist as files. → Created DRAFT files for F751, F752, F753 and Tasks 7-9 already exist in Tasks table.
- [resolved-user] Phase1 iter3: AC#7 expects '650/650' PASS but Technical Design has 'Interim Migration Strategy (Pending Investigation)' for Branch 0 (恋人) and Branch 2 (思慕). → User decision: Maintain current state with Contingency note. Expected will be revised after T0 investigation if needed.
- [resolved-applied] Phase1 iter4: AC#3 Method pattern vs AC Details inconsistency - AC table uses broader regex, AC Details shows simpler command. → Aligned AC#3 Method to use simpler literal match from AC Details.
- [resolved-applied] Phase1 iter4: Philosophy Derivation maps YamlRunner to AC#4/AC#5 but these verify KojoBranchesParser. → Added AC#4b for YamlRunner passes state to parser.
