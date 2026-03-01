# Feature 644: Equivalence Testing Framework

## Status: [DONE]

**Initialized**: 2026-01-30 (initializer)

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

## Created: 2026-01-27

---

## Summary

Implement batch verification mode for KojoComparer to verify ERB=YAML equivalence for all 117 converted files.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

Need systematic verification that converted YAML produces identical output to original ERB. KojoComparer needs `--all` batch mode for full coverage testing.

### Goal (What to Achieve)

1. Add `--all` mode to KojoComparer for batch verification
2. Report 117/117 MATCH or detailed diff for failures
3. Integrate with CI for regression prevention
4. Integration test coverage deferred from F554

**Prerequisite**: F675 (YAML Format Unification) resolves the `branches:` vs `entries:` format mismatch, enabling YamlRunner to render production YAML files.

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F675 | YAML Format Unification | [DONE] |
| Predecessor | F636-F643 | All conversion features | [DONE] |
| Successor | F645 | Kojo Quality Validator | [PROPOSED] |

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-651.md](feature-651.md) - KojoComparer KojoEngine API Update (transitive predecessor)
- [feature-652.md](feature-652.md) - KojoComparer Test YAML Migration
- [feature-675.md](feature-675.md) - YAML Format Unification (Predecessor)
- [feature-554.md](feature-554.md) - Integration test coverage deferred from F554
- [feature-646.md](feature-646.md) - F646 Post-Phase Review scope for CI integration

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Need to verify 117 ERB files produce identical output to their converted YAML files
2. Why: Manual verification is impractical at scale - need automated batch comparison
3. Why: KojoComparer currently only supports single-file comparison (--erb --yaml --function --talent)
4. Why: No discovery mechanism exists to map ERB files to their corresponding YAML files
5. Why: Phase 19 conversion created YAML files with different naming/structure than KojoComparer expects

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| Cannot verify 117 conversions automatically | KojoComparer lacks --all batch mode and ERB-to-YAML discovery mechanism |
| YAML format incompatibility | Production YAML uses `branches:` format while YamlDialogueLoader expects `entries:` format |

### Conclusion

The primary issue is the lack of a batch verification mode in KojoComparer. However, a critical blocking issue exists: **Production YAML files (443 files in Game/YAML/Kojo/) use the `branches:` format**, which is incompatible with the current YamlDialogueLoader (which expects `entries:` format with `id`/`content` fields).

This format mismatch means:
1. YamlRunner.cs cannot render production YAML files (uses YamlDialogueLoader internally)
2. The 443 YAML files in `Game/YAML/Kojo/` ALL use the incompatible `branches:` schema
3. F652 migrated only test YAML files, leaving production format unchanged

**Root Cause**: Phase 19 conversion creates YAML in a legacy `branches:` format that is incompatible with the KojoEngine pipeline that F651/F652 established.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F651 | [DONE] | Prerequisite | KojoComparer KojoEngine API Update - established new DI pattern |
| F652 | [DONE] | Prerequisite | Test YAML migration - extended YamlDialogueLoader for entries format |
| F636-F643 | Various | Predecessor | Character conversion features - create YAML files to be verified |
| F645 | [DRAFT] | Successor | Kojo Quality Validator - depends on F644 |
| F555 | [DONE] | Planning | Phase 19 Planning - defined the 117 file scope |

### Pattern Analysis

This is a format migration gap that spans the Phase 19 conversion pipeline:
- F553/F549/F551/F552 defined the new KojoEngine API with `entries:` format
- F651/F652 updated KojoComparer to use the new API
- F636-F643 conversion features create YAML in `branches:` format (legacy ErbToYaml output)
- No feature exists to bridge the format gap for production YAML

The conversion features (F636-F643) should produce YAML in the new `entries:` format, OR a format adapter is needed.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Batch mode can be added; F675 resolves format issue |
| Scope is realistic | YES | Batch mode only; Format migration handled by F675 |
| No blocking constraints | YES | F675 as Predecessor resolves format mismatch |

**Verdict**: FEASIBLE (with F675 as Predecessor)

**Resolution**:
- F675 (YAML Format Unification) created as Predecessor
- F675 will unify all YAML to `entries:` format
- F644 scope is now limited to `--all` batch mode implementation


### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| uEmuera.Headless | Runtime | Medium | Required for ERB execution via ErbRunner |
| Era.Core.KojoEngine | Runtime | Low | YAML rendering pipeline |
| YamlDotNet | Runtime | Low | YAML parsing |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| CI Pipeline (future) | HIGH | Will use --all mode for regression testing |
| F645 Kojo Quality Validator | MEDIUM | Depends on F644 for verified equivalence |
| F646 Post-Phase Review | LOW | Uses F644 results to confirm Phase 19 success |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/KojoComparer/Program.cs | Update | Add --all CLI mode parsing |
| tools/KojoComparer/BatchProcessor.cs | Update | Extend for full batch processing with discovery |
| tools/KojoComparer/YamlRunner.cs | Update | Support `branches:` format OR require format adapter |
| tools/KojoComparer/FileDiscovery.cs | Create | ERB-to-YAML file mapping logic |
| tools/kojo-mapper/com_file_map.json | Read | May provide ERB-to-YAML mapping data |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Production YAML uses `branches:` format | Game/YAML/Kojo/ | HIGH - YamlDialogueLoader cannot parse |
| YamlDialogueLoader expects `entries:` format | Era.Core/Dialogue/Loading/YamlDialogueLoader.cs | HIGH - blocks direct comparison |
| 443 YAML files vs 117 ERB files | File count mismatch | MEDIUM - one ERB produces multiple YAML (by branch) |
| ErbRunner requires headless mode | tools/KojoComparer/ErbRunner.cs | MEDIUM - needs `engine/uEmuera.Headless.csproj` |
| State injection for condition testing | KojoComparer design | MEDIUM - must test all TALENT/ABL states |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Format mismatch blocks all comparisons | HIGH | HIGH | Resolve format issue before implementing --all mode |
| ERB-to-YAML mapping is non-trivial | MEDIUM | MEDIUM | Use kojo-mapper data (com_file_map.json) for mapping |
| Some ERB functions may not have YAML equivalents | LOW | MEDIUM | Report unmapped files as warnings |
| Headless mode execution is slow | MEDIUM | LOW | Parallelize ERB execution where possible |
| State permutations explode test count | MEDIUM | MEDIUM | Test representative states, not all permutations |

## Architecture Investigation

### KojoComparer Current Structure

```
tools/KojoComparer/
├── Program.cs          # CLI entry point (single-file mode only)
├── BatchProcessor.cs   # Directory-based batch processing (exists but limited)
├── ErbRunner.cs        # ERB execution via headless mode
├── YamlRunner.cs       # YAML rendering via KojoEngine (F651 updated)
├── OutputNormalizer.cs # Output normalization for comparison
└── DiffEngine.cs       # Line-by-line diff generation
```

### Current CLI Interface

```
dotnet run -- --erb <path> --function <name> --yaml <path> --talent <state>
```

Missing: `--all` mode for batch verification

### File Count Analysis

| Directory | ERB Files | YAML Files | Note |
|-----------|-----------|------------|------|
| Game/ERB/口上/ | 117 | - | Original ERB files |
| Game/YAML/Kojo/ | - | 443 | Converted YAML (multiple per ERB) |

The 117→443 expansion is because each ERB file contains multiple COM functions, and each function produces a separate YAML file.

### Format Mismatch Detail

**Production YAML (branches: format)**:
```yaml
character: 美鈴
situation: K1_愛撫
branches:
- lines:
  - 「んっ……そこ、気持ちいい……」
  - ...
  condition: {}
- lines:
  - ...
  condition: { ... }
```

**Test YAML (entries: format - what YamlDialogueLoader expects)**:
```yaml
entries:
- id: "lover"
  content: |
    「んっ……そこ、気持ちいい……」
    ...
  priority: 3
  condition:
    type: "Talent"
    talentType: "16"
    threshold: 1
```

### Resolution Options

**Option A**: Extend YamlRunner to support `branches:` format
- Add LegacyBranchesLoader alongside YamlDialogueLoader
- Detect format by checking for `branches:` vs `entries:` key
- Pros: Works with existing production YAML
- Cons: Maintains two format paths

**Option B**: Convert production YAML to `entries:` format
- Batch convert 443 files during F636-F643 or as separate feature
- Pros: Single format going forward
- Cons: Large migration scope

**Option C**: Update F636-F643 to produce `entries:` format
- Modify conversion pipeline output
- Pros: Fixes at source
- Cons: May require significant rework of conversion features

---

<!-- fc-phase-3-completed -->
<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Implement `--all` batch verification mode by:
1. Creating **FileDiscovery.cs** to map 117 ERB files to their 443 YAML counterparts using `com_file_map.json` metadata
2. Extending **Program.cs** to parse `--all` flag and delegate to BatchProcessor
3. Updating **BatchProcessor.cs** to use FileDiscovery for automated file discovery instead of requiring manual directory paths
4. Generating summary report with "N/N MATCH" format and appropriate exit codes for CI integration

**Key insight**: The `com_file_map.json` already contains the ERB-to-COM range mapping needed for discovery. Each ERB file (e.g., `KOJO_K1_愛撫.ERB`) maps to a set of COM IDs (e.g., 0-6), which correspond to YAML files (e.g., `COM_000.yaml` - `COM_006.yaml`) in the character directory structure.

**Discovery Strategy**:
- Scan `Game/ERB/口上/` for ERB files (117 files across 11 character directories + U_汎用)
- Use `com_file_map.json` to determine which COM ranges each ERB file implements
- For each COM range, locate corresponding YAML files in `Game/YAML/Kojo/{N_CharacterName}/COM_{NNN}.yaml`
- Generate test cases for each ERB function → YAML file pair

**State Testing Strategy** (deferred to F645):
- This feature focuses on infrastructure (`--all` mode and discovery)
- State permutation testing (TALENT combinations) is scope of F645 (Kojo Quality Validator)
- BatchProcessor will use minimal representative states (e.g., TALENT:16=1 for lover state)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `tools/KojoComparer/FileDiscovery.cs` with `DiscoverTestCases()` method |
| 2 | Add `--all` flag parsing in `Program.cs` ParseArguments() logic |
| 3 | Add conditional branch in Program.Main() to invoke BatchProcessor when `--all` is detected |
| 4 | FileDiscovery.DiscoverTestCases() scans `Game/ERB/口上/` directory recursively |
| 5 | BatchReport class already has `TotalTests` property (line 26 in BatchProcessor.cs) |
| 6 | BatchReport already has `PassedTests` and `FailedTests` properties (lines 27-28) |
| 7 | Program.cs prints summary using `Console.WriteLine($"{report.PassedTests}/{report.TotalTests} PASS")` format |
| 8 | Program.Main() returns `report.FailedTests == 0 ? 0 : 1` exit code |
| 9 | Create `tools/KojoComparer.Tests/FileDiscoveryTests.cs` with unit tests |
| 10 | Verify with `dotnet test tools/KojoComparer.Tests/` command |
| 11 | Verify with `dotnet build tools/KojoComparer/` command |
| 12 | Grep for TODO/FIXME/HACK and verify count equals 0 |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| File mapping strategy | A) Directory scanning + filename matching<br>B) Use com_file_map.json metadata<br>C) Hardcode mappings | **B) Use com_file_map.json** | SSOT principle - com_file_map.json is already maintained and contains authoritative ERB-to-COM mappings |
| Discovery granularity | A) ERB file level<br>B) Function level<br>C) COM ID level | **C) COM ID level** | Most granular - each COM ID maps to one YAML file, enabling precise test reporting |
| State testing scope | A) Test all TALENT permutations<br>B) Representative states only<br>C) Defer to F645 | **C) Defer to F645** | F644 focuses on infrastructure; F645 will implement comprehensive state testing |
| BatchProcessor API | A) Keep existing directory-based API<br>B) Replace with FileDiscovery-based API<br>C) Add overload for both | **C) Add overload** | Preserve existing tests while enabling new --all mode |
| Exit code logic | A) Exit 0 always<br>B) Exit 1 on any failure<br>C) Exit code equals failure count | **B) Exit 1 on any failure** | Standard CI convention - binary success/failure |

### Interfaces / Data Structures

#### FileDiscovery.cs

```csharp
public class FileDiscovery
{
    private readonly string _erbBasePath;
    private readonly string _yamlBasePath;
    private readonly string _mapFilePath;

    public FileDiscovery(string erbBasePath, string yamlBasePath, string mapFilePath)
    {
        _erbBasePath = erbBasePath;
        _yamlBasePath = yamlBasePath;
        _mapFilePath = mapFilePath;
    }

    /// <summary>
    /// Discovers all ERB-YAML test cases by scanning ERB directory and mapping to YAML files.
    /// </summary>
    /// <returns>List of test cases with ERB file, function name, YAML file, and test state</returns>
    public List<TestCase> DiscoverTestCases()
    {
        var testCases = new List<TestCase>();

        // 1. Load com_file_map.json to get COM→ERB mappings
        var comMap = LoadComFileMap(_mapFilePath);

        // 2. Scan Game/ERB/口上/ for ERB files
        var erbFiles = Directory.GetFiles(_erbBasePath, "*.ERB", SearchOption.AllDirectories);

        // 3. For each ERB file, determine COM ranges it implements
        foreach (var erbFile in erbFiles)
        {
            var characterId = ExtractCharacterId(erbFile);
            var erbFileName = Path.GetFileNameWithoutExtension(erbFile);

            // 4. Find COM ranges that map to this ERB file
            var comRanges = FindComRangesForErb(comMap, erbFileName, characterId);

            // 5. For each COM ID, create test case with corresponding YAML file
            foreach (var comId in comRanges)
            {
                var yamlFile = FindYamlFile(characterId, comId);
                if (yamlFile != null)
                {
                    var functionName = GenerateFunctionName(characterId, comId);
                    var state = GetRepresentativeState(comId); // Minimal state for basic verification

                    testCases.Add(new TestCase
                    {
                        ErbFile = erbFile,
                        FunctionName = functionName,
                        YamlFile = yamlFile,
                        State = state,
                        ComId = comId,
                        CharacterId = characterId
                    });
                }
            }
        }

        return testCases;
    }

    private string? FindYamlFile(string characterId, int comId)
    {
        // Construct expected YAML path: Game/YAML/Kojo/{N_CharacterName}/COM_{NNN}.yaml
        var characterDir = Directory.GetDirectories(_yamlBasePath, $"{characterId}_*").FirstOrDefault();
        if (characterDir == null) return null;

        var yamlPath = Path.Combine(characterDir, $"COM_{comId:D3}.yaml");
        return File.Exists(yamlPath) ? yamlPath : null;
    }

    private string ExtractCharacterId(string erbFilePath)
    {
        // Extract character ID from path: .../1_美鈴/KOJO_K1_愛撫.ERB -> "1"
        var directory = Path.GetFileName(Path.GetDirectoryName(erbFilePath));
        var match = Regex.Match(directory, @"^(\d+|U)_");
        return match.Success ? match.Groups[1].Value : throw new ArgumentException($"Cannot extract character ID from {erbFilePath}");
    }

    private string GenerateFunctionName(string characterId, int comId)
    {
        // Generate ERB function name: @KOJO_MESSAGE_COM_K1_0_1
        return $"@KOJO_MESSAGE_COM_K{characterId}_{comId / 100}_{comId % 100}";
    }
}

public class TestCase
{
    public string ErbFile { get; set; } = "";
    public string FunctionName { get; set; } = "";
    public string YamlFile { get; set; } = "";
    public Dictionary<string, int> State { get; set; } = new();
    public int ComId { get; set; }
    public string CharacterId { get; set; } = "";
}
```

#### Updated BatchProcessor API

```csharp
public class BatchProcessor
{
    // Existing directory-based API (preserved for existing tests)
    public async Task<BatchReport> ProcessAsync(
        string erbDirectory,
        string yamlDirectory,
        string functionName,
        List<Dictionary<string, int>> states)
    { /* existing implementation */ }

    // New overload for --all mode with FileDiscovery
    public async Task<BatchReport> ProcessAllAsync(List<TestCase> testCases)
    {
        var report = new BatchReport();

        foreach (var testCase in testCases)
        {
            report.TotalTests++;

            try
            {
                // Execute ERB
                var erbOutput = await _erbRunner.ExecuteAsync(
                    testCase.ErbFile,
                    testCase.FunctionName,
                    testCase.State);
                var normalizedErb = _normalizer.Normalize(erbOutput);

                // Render YAML
                var context = ConvertStateToContext(testCase.State);
                var yamlOutput = _yamlRunner.Render(testCase.YamlFile, context);
                var normalizedYaml = _normalizer.Normalize(yamlOutput);

                // Compare
                var comparison = _diffEngine.Compare(normalizedErb, normalizedYaml);

                if (comparison.IsMatch)
                {
                    report.PassedTests++;
                }
                else
                {
                    report.FailedTests++;
                    report.Failures.Add($"FAIL: COM_{testCase.ComId:D3} ({testCase.CharacterId})");
                    report.Failures.AddRange(comparison.Differences);
                }
            }
            catch (Exception ex)
            {
                report.FailedTests++;
                report.Failures.Add($"ERROR: COM_{testCase.ComId:D3} - {ex.Message}");
            }
        }

        return report;
    }
}
```

#### Updated Program.cs CLI Logic

```csharp
static async Task<int> Main(string[] args)
{
    var arguments = ParseArguments(args);

    // Check for --all mode
    if (arguments.ContainsKey("all"))
    {
        return await RunBatchModeAsync();
    }

    // Existing single-file mode
    if (!arguments.ContainsKey("erb") || /* ... existing validation ... */)
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Batch mode:  dotnet run -- --all");
        Console.WriteLine("  Single mode: dotnet run -- --erb <path> --function <name> --yaml <path> --talent <state>");
        return 1;
    }

    // ... existing single-file implementation ...
}

static async Task<int> RunBatchModeAsync()
{
    Console.WriteLine("Running batch verification (--all mode)...\n");

    // Initialize paths
    var erbBasePath = Path.GetFullPath("Game/ERB/口上");
    var yamlBasePath = Path.GetFullPath("Game/YAML/Kojo");
    var mapFilePath = Path.GetFullPath("tools/kojo-mapper/com_file_map.json");

    // Discover test cases
    var discovery = new FileDiscovery(erbBasePath, yamlBasePath, mapFilePath);
    var testCases = discovery.DiscoverTestCases();

    Console.WriteLine($"Discovered {testCases.Count} test cases\n");

    // Initialize components
    var gamePath = Path.GetFullPath("Game");
    var headlessProjectPath = Path.GetFullPath("engine/uEmuera.Headless.csproj");

    var erbRunner = new ErbRunner(gamePath, headlessProjectPath);
    var yamlRunner = new YamlRunner();
    var normalizer = new OutputNormalizer();
    var diffEngine = new DiffEngine();
    var batchProcessor = new BatchProcessor(erbRunner, yamlRunner, normalizer, diffEngine);

    // Process all test cases
    var report = await batchProcessor.ProcessAllAsync(testCases);

    // Print summary
    Console.WriteLine("\n=== SUMMARY ===");
    Console.WriteLine($"{report.PassedTests}/{report.TotalTests} PASS");

    if (report.FailedTests > 0)
    {
        Console.WriteLine($"\n{report.FailedTests} FAILURES:");
        foreach (var failure in report.Failures)
        {
            Console.WriteLine(failure);
        }
    }

    return report.FailedTests == 0 ? 0 : 1;
}
```

### Implementation Notes

1. **com_file_map.json structure**: The JSON contains `ranges` array with `{start, end, file}` mappings and optional `character_overrides` for character-specific ERB file assignments.

2. **Character ID extraction**: Character directories follow `{N_CharacterName}` pattern (e.g., `1_美鈴`, `U_汎用`). Extract the prefix before underscore.

3. **Function naming convention**: ERB functions follow `@KOJO_MESSAGE_COM_K{CharId}_{Major}_{Minor}` where ComId = Major*100 + Minor (e.g., COM_311 → `@KOJO_MESSAGE_COM_K1_3_11`).

4. **Representative states**: Use minimal states for basic verification (e.g., TALENT:16=1 for lover state). Comprehensive state permutation testing is deferred to F645.

5. **Error handling**: Gracefully handle missing YAML files, unmapped COM ranges, and execution failures. Report each as a failure without crashing the batch run.

6. **Performance consideration**: 443 test cases may take significant time in serial execution. Parallel execution optimization is out of scope for F644 but can be added in future features.

---

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "verify **117** ERB kojo files" | All 117 ERB files must be discoverable and verified | AC#1, AC#5, AC#6 |
| "Automated migration" | Batch mode must run without manual intervention | AC#2, AC#3 |
| "Add `--all` mode" | CLI must parse --all flag and trigger batch processing | AC#2, AC#3 |
| "Report 117/117 MATCH or detailed diff" | Clear summary report with pass/fail counts | AC#5, AC#6, AC#7 |
| "Integrate with CI" | Exit code 0 on success, non-zero on failure | AC#8 |
| "ERB-to-YAML discovery" | FileDiscovery.cs maps 117 ERB to 443 YAML files | AC#4 |
| "Integration test coverage (F554)" | Unit tests for new batch functionality | AC#9, AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | FileDiscovery.cs created | file | Glob | exists | tools/KojoComparer/FileDiscovery.cs | [x] |
| 2 | --all flag parsed in Program.cs | code | Grep(tools/KojoComparer/Program.cs) | contains | "--all" | [x] |
| 3 | --all triggers BatchProcessor | code | Grep(tools/KojoComparer/Program.cs) | contains | "RunBatchModeAsync" | [x] |
| 4 | FileDiscovery discovers ERB files | code | Grep(tools/KojoComparer/FileDiscovery.cs) | contains | "口上" | [x] |
| 5 | BatchReport includes TotalTests count | code | Grep(tools/KojoComparer/BatchProcessor.cs) | contains | "TotalTests" | [x] |
| 6 | BatchReport includes PassedTests/FailedTests | code | Grep(tools/KojoComparer/BatchProcessor.cs) | matches | "PassedTests.*FailedTests|FailedTests.*PassedTests" | [x] |
| 7 | Summary report format includes N/N pattern | code | Grep(tools/KojoComparer/) | matches | "\\d+/\\d+.*PASS|PASS.*\\d+/\\d+" | [x] |
| 8 | Exit code reflects batch result | code | Grep(tools/KojoComparer/Program.cs) | matches | "return.*FailedTests.*0|FailedTests.*==.*0.*return" | [x] |
| 9 | Unit tests exist for FileDiscovery | file | Glob | exists | tools/KojoComparer.Tests/FileDiscoveryTests.cs | [x] |
| 10 | All KojoComparer tests pass | test | dotnet test tools/KojoComparer.Tests/ | succeeds | - | [x] |
| 11 | Build succeeds | build | dotnet build tools/KojoComparer/ | succeeds | - | [x] |
| 12 | No technical debt markers | code | Grep(tools/KojoComparer/) | count_equals | "TODO|FIXME|HACK" | 0 | [x] |

**Note**: 12 ACs within typical infra feature range (8-15).

### AC Details

**AC#1: FileDiscovery.cs created**
- New file to encapsulate ERB-to-YAML file mapping logic
- Separates discovery concern from BatchProcessor
- Must be created (not just updated) per Impact Analysis

**AC#2: --all flag parsed in Program.cs**
- CLI entry point must recognize `--all` argument
- Pattern: `dotnet run -- --all` triggers batch mode
- Single-file mode (--erb --yaml) remains unchanged

**AC#3: --all triggers BatchProcessor**
- When --all is detected, Program.cs must invoke BatchProcessor
- Integration between CLI parsing and batch execution
- Must not require additional manual arguments

**AC#4: FileDiscovery discovers ERB files**
- Must scan `Game/ERB/口上/` directory structure
- Maps 117 ERB files to their corresponding YAML files in `Game/YAML/Kojo/`
- Uses com_file_map.json or similar mapping strategy

**AC#5: BatchReport includes TotalTests count**
- BatchReport class already has TotalTests property (verified in current code)
- Ensures batch processing tracks total verification attempts
- Must increment for each ERB-YAML comparison

**AC#6: BatchReport includes PassedTests/FailedTests**
- BatchReport already has PassedTests/FailedTests properties
- Ensures pass/fail tracking for summary report
- Critical for CI integration (exit code determination)

**AC#7: Summary report format includes N/N pattern**
- Output must show "117/117 MATCH" or "115/117 MATCH (2 FAILED)"
- Human-readable summary for CI logs
- Format: Passed count / Total count

**AC#8: Exit code reflects batch result**
- Exit code 0: All tests passed (FailedTests == 0)
- Exit code 1: Any test failed (FailedTests > 0)
- Critical for CI pipeline integration

**AC#9: Unit tests exist for FileDiscovery**
- New FileDiscovery.cs requires corresponding unit tests
- Tests ERB discovery, YAML mapping, edge cases
- Follows existing KojoComparer.Tests structure

**AC#10: All KojoComparer tests pass**
- Regression prevention: existing tests must not break
- New tests must pass
- Gate for feature completion

**AC#11: Build succeeds**
- KojoComparer project must compile without errors
- Verifies code integrity
- Standard infra gate

**AC#12: No technical debt markers**
- No TODO, FIXME, or HACK comments in implementation
- Ensures clean, complete implementation
- Count must equal 0

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,4 | Create FileDiscovery.cs with ERB-YAML mapping logic | [x] |
| 2 | 2,3 | Add --all flag parsing and BatchProcessor dispatch in Program.cs | [x] |
| 3 | 5,6,7,8 | Extend BatchProcessor with ProcessAllAsync and summary report | [x] |
| 4 | 9 | Create FileDiscoveryTests.cs with unit tests | [x] |
| 5 | 10,11 | Run all tests and verify build succeeds | [x] |
| 6 | 12 | Verify no technical debt markers exist | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | FileDiscovery design from Technical Design | FileDiscovery.cs with DiscoverTestCases() |
| 2 | implementer | sonnet | T2 | CLI design from Technical Design | Updated Program.cs with --all mode |
| 3 | implementer | sonnet | T3 | BatchProcessor design from Technical Design | Updated BatchProcessor.cs with ProcessAllAsync |
| 4 | implementer | sonnet | T4 | FileDiscovery interface from T1 | FileDiscoveryTests.cs |
| 5 | ac-tester | haiku | T5 | Test commands from AC#10-11 | Test execution results |
| 6 | ac-tester | haiku | T6 | Grep command from AC#12 | Technical debt verification |

**Constraints** (from Technical Design):
1. Use `com_file_map.json` as SSOT for ERB-to-COM mappings
2. Preserve existing BatchProcessor directory-based API for backward compatibility
3. ERB files in `Game/ERB/口上/` must follow `{N_CharacterName}` directory pattern
4. YAML files in `Game/YAML/Kojo/{N_CharacterName}/COM_{NNN}.yaml` format
5. FileDiscovery must handle both numeric character IDs (1-11) and U_汎用 (U prefix)

**Pre-conditions**:
- F675 (YAML Format Unification) completed - YamlRunner can parse production YAML
- F636-F643 conversion features completed - YAML files exist in Game/YAML/Kojo/
- tools/kojo-mapper/com_file_map.json exists and is up-to-date
- uEmuera.Headless.csproj available for ERB execution
- Era.Core.KojoEngine available for YAML rendering

**Success Criteria**:
- `dotnet run --project tools/KojoComparer/ -- --all` discovers all ERB-YAML test cases
- Summary report shows "N/N PASS" format with total test count
- Exit code 0 on all tests passing, exit code 1 on any failure
- All KojoComparer tests pass (including new FileDiscoveryTests)
- No technical debt markers (TODO/FIXME/HACK) in implementation

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation
4. Document specific failure mode (discovery logic, state handling, performance, etc.)

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| State permutation testing (TALENT combinations) | F645 | F644 focuses on infrastructure; comprehensive state testing is F645 scope |
| Parallel execution optimization | F646 | 443 test cases may be slow in serial execution; Phase 19 Post-Phase Review scope |
| CI pipeline integration | F646 | Once --all mode is stable, integrate with .github/workflows/; Phase 19 Post-Phase Review scope |
| ac-static-verifier Permission denied on directory | New Feature | Tool bug: tries to open directory as file when path contains directory |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-29 19:09 | Task 1 | Created FileDiscovery.cs with ERB-YAML mapping logic using com_file_map.json. Build succeeded. |
| 2026-01-29 19:13 | Task 2 | Added --all flag parsing to ParseArguments() (boolean flag support). Added RunBatchModeAsync() dispatch logic in Main(). Updated usage message. Build succeeded. Note: RunBatchModeAsync() contains placeholder pending Task 3 (ProcessAllAsync implementation). |
| 2026-01-29 19:15 | Task 3 | Extended BatchProcessor.cs with ProcessAllAsync() overload. New method processes List<TestCase> from FileDiscovery. Preserves existing ProcessAsync() for backward compatibility. Build succeeded. |
| 2026-01-29 | DEVIATION | Bash | ac-static-verifier code | exit code 1 - Permission denied on tools/KojoComparer directory |
| 2026-01-29 | Phase 4 | Fixed Program.cs RunBatchModeAsync() to call ProcessAllAsync(). Task 2/3 integration complete. Build succeeded. |
| 2026-01-29 | Task 4 | Created FileDiscoveryTests.cs with 6 unit tests. All tests pass. |
| 2026-01-29 | Task 5 | Build: 0 errors, 0 warnings. Tests: 18 passed, 8 skipped, 0 failed. |
| 2026-01-29 | Task 6 | No TODO/FIXME/HACK found in tools/KojoComparer/. |
| 2026-01-29 | Phase 6 | All 12 ACs verified [x]. |
| 2026-01-29 | DEVIATION | feature-reviewer | NEEDS_REVISION | Dependencies table F675 status stale ([PROPOSED] vs [DONE]) |
| 2026-01-29 | Phase 7 | Post-review: OK. Doc-check: OK. SSOT update: N/A. |
