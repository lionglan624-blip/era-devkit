# Feature 753: Migration Script Parameterization

## Status: [DONE]
<!-- completed: 2026-02-05T20:42:00Z -->

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Migration scripts should be configurable rather than hard-coded, enabling reuse across different game variants or future TALENT structure changes.

### Problem (Current Issue)
F750's YamlTalentMigrator hard-codes TALENT indices (16, 3, 17 for Branch 0-2) directly in the migration logic. Changes to TALENT mappings require code changes.

### Goal (What to Achieve)
Parameterize TALENT index mappings via configuration file (e.g., talent-mapping.json), making migration script reusable and maintainable.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F750 | [DONE] | Migration script implementation |

---

## Scope

Deferred from F750 残課題. Enhancement for maintainability after F750 completes.

---

## Links
- [feature-750.md](feature-750.md) - Migration script implementation (predecessor)
- [feature-751.md](feature-751.md) - TALENT semantic mapping validation (parallel enhancement)
- [feature-752.md](feature-752.md) - Compound condition support (parallel enhancement)
- [feature-711.md](feature-711.md) - YAML supplement loading (source of TALENT definitions)

---

<!-- fc-phase-1-completed -->

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why does YamlTalentMigrator hard-code TALENT indices?**
   Because F750 was implemented as a one-time migration to unblock F706, not as a reusable tool. The priority was speed of implementation over flexibility.

2. **Why was a one-time approach chosen over a configurable approach?**
   Because the TALENT mapping investigation (T0) revealed specific indices (恋人=16, 恋慕=3, 思慕=17) that were stable and documented in Talent.yaml. Hard-coding these known values was faster than designing a configuration system.

3. **Why wasn't a configuration system designed for future reuse?**
   Because F750 scope was narrowly defined to address the immediate F706 blocker (601/650 failures). Configurability was explicitly deferred as a "残課題" to avoid scope creep.

4. **Why does this matter for maintainability?**
   Because TALENT definitions are not immutable. Game/data/Talent.yaml can be extended with new indices, and the ERB branching patterns may vary across character files. Future migrations would require code changes rather than configuration changes.

5. **Why is code change problematic for migrations?**
   Because code changes require: (a) developer intervention, (b) recompilation, (c) testing. Configuration files enable non-developer adjustments, version control of mappings independent of code, and easier adaptation to game variants (e.g., eraTW fork compatibility).

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| YamlTalentMigrator uses hard-coded indices (16, 3, 17) | Design decision prioritized F706 unblocking over configurability |
| Changing TALENT mappings requires code changes | No external configuration mechanism exists |
| Script is not reusable for different game variants | One-time tool design without abstraction layer |

### Conclusion

The root cause is a **conscious scope limitation** in F750, not a technical oversight. The hard-coded indices were the correct choice for the immediate goal (unblocking F706), and configurability was explicitly tracked for future implementation. This feature (F753) fulfills that deferred requirement.

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F750 | [DONE] | Predecessor | Created the hard-coded migrator; deferred configurability to F753 |
| F751 | [DRAFT] | Parallel enhancement | TALENT semantic mapping validation - shares TALENT index knowledge |
| F752 | [DRAFT] | Parallel enhancement | Compound condition support - may need condition format in config |
| F711 | [DONE] | Related | Implemented Talent.yaml loading - source of authoritative TALENT definitions |

### Pattern Analysis

This is a **deliberate scope deferral pattern**, not a recurring technical debt issue:

1. F750 explicitly documented configurability as out-of-scope (残課題)
2. F753 was created as the designated destination for this deferred work
3. The pattern is healthy: solve immediate problem first, enhance later

No cycle-breaking needed - this is working as designed.

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Standard configuration pattern (JSON/YAML file → Dictionary mapping) |
| Scope is realistic | YES | Single file change (Program.cs) + new config file + tests |
| No blocking constraints | YES | F750 is [DONE], no runtime constraints |

**Verdict**: FEASIBLE

The implementation is straightforward:
1. Create `talent-mapping.json` (or `.yaml`) with branch-to-TALENT index mapping
2. Replace hard-coded `BranchConditions` dictionary with file-loaded configuration
3. Add CLI option to specify config file path (optional, default to embedded)
4. Update tests to verify configuration loading

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/YamlTalentMigrator/Program.cs | Update | Replace hard-coded BranchConditions with config-loaded dictionary |
| tools/YamlTalentMigrator/talent-mapping.json | Create | External configuration file with TALENT index mappings |
| tools/YamlTalentMigrator/README.md | Update | Document configuration file format and usage |
| tools/YamlTalentMigrator.Tests/ | Create | Unit test directory for configuration loading tests |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| YAML condition format preservation | dialogue-schema.json | LOW - Config only affects which indices; format unchanged |
| Backward compatibility | Existing migrated YAML files | NONE - This is a tool change, not YAML format change |
| .NET 8.0+ runtime | YamlTalentMigrator.csproj | LOW - Already using .NET 8.0 |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Config file schema validation missing | LOW | MEDIUM | Add JSON schema or runtime validation |
| Breaking existing --dry-run workflow | LOW | LOW | Maintain backward compatibility with default config |
| Config file not found at runtime | LOW | MEDIUM | Embed default config as fallback; clear error messages |
| Over-engineering for minimal benefit | MEDIUM | LOW | Keep config simple: just branch→TALENT mapping, no complex logic |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "configurable rather than hard-coded" | External configuration file must exist | AC#1 |
| "configurable rather than hard-coded" | Code must load mappings from config file | AC#2 |
| "reuse across different game variants" | Config file format must be documented | AC#3 |
| "reuse across different game variants" | Custom config path must be specifiable | AC#4 |
| "future TALENT structure changes" | Default config must match current hard-coded values | AC#5 |
| "reusable and maintainable" | Tests verify config-driven behavior | AC#6 |
| "reusable and maintainable" | Build must succeed after changes | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Config file exists | file | Glob(tools/YamlTalentMigrator/talent-mapping.json) | exists | - | [x] |
| 2 | Config loading implemented | code | Grep(tools/YamlTalentMigrator/Program.cs) | contains | "talent-mapping.json" | [x] |
| 3 | README documents config format | file | Grep(tools/YamlTalentMigrator/README.md) | contains | "talent-mapping.json" | [x] |
| 4 | Custom config path CLI option | code | Grep(tools/YamlTalentMigrator/Program.cs) | contains | "--config" | [x] |
| 5a | Default config contains恋人 TALENT | file | Grep(tools/YamlTalentMigrator/talent-mapping.json) | contains | "16" | [x] |
| 5b | Default config contains恋慕 TALENT | file | Grep(tools/YamlTalentMigrator/talent-mapping.json) | contains | "3" | [x] |
| 5c | Default config contains思慕 TALENT | file | Grep(tools/YamlTalentMigrator/talent-mapping.json) | contains | "17" | [x] |
| 6 | Unit test verifies config loading | test | dotnet test tools/YamlTalentMigrator.Tests | succeeds | - | [x] |
| 8 | Config error handling test | test | dotnet test tools/YamlTalentMigrator.Tests --filter ConfigNotFoundTest | succeeds | - | [x] |
| 7 | Build succeeds | build | dotnet build tools/YamlTalentMigrator | succeeds | - | [x] |

### AC Details

**AC#1: Config file exists**
- Verifies the external configuration file `talent-mapping.json` is created in the tool directory
- This is the fundamental artifact that enables configurability
- JSON format chosen for .NET ecosystem compatibility (System.Text.Json)

**AC#2: Config loading implemented**
- Verifies Program.cs references the config file for loading mappings
- The hard-coded `BranchConditions` dictionary should be replaced with file-loaded data
- Pattern check ensures the code path exists, not just dead code

**AC#3: README documents config format**
- Verifies documentation exists for the config file format
- Users need to understand how to customize mappings for their game variant
- Essential for "reuse across different game variants" claim

**AC#4: Custom config path CLI option**
- Verifies `--config` argument is implemented for specifying alternate config files
- Allows users to maintain multiple mapping configurations
- Pattern: `--config path/to/custom-mapping.json`

**AC#5a-5c: Default config contains TALENT indices**
- Verifies the default config preserves F750's proven mappings (16=恋人, 3=恋慕, 17=思慕)
- Split into three separate contains checks for reliability
- Maintains backward compatibility with existing migrated files

**AC#8: Config error handling test**
- Verifies custom config path not found scenario produces appropriate error
- Tests the edge case handling documented in Technical Design
- Ensures ConfigNotFoundTest exists and passes

**AC#6: Unit test verifies config loading**
- Verifies test coverage for the new configuration system
- Tests should verify:
  - Config file parsing
  - Missing config fallback (if implemented)
  - Invalid config error handling

**AC#7: Build succeeds**
- Verifies the tool compiles after changes
- Ensures no syntax errors or missing dependencies introduced

---

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 5a, 5b, 5c | Create talent-mapping.json with default mappings (16, 3, 17) | | [x] |
| 2 | 2, 4 | Implement LoadMappingConfig() method and --config CLI parsing in Program.cs | | [x] |
| 3 | 3 | Update README.md with configuration format documentation | | [x] |
| 4 | 6, 8 | Create unit tests for config loading (default, custom, fallback, error handling) | | [x] |
| 5 | 7 | Verify build succeeds with no errors | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

Refer to the Technical Design section for detailed implementation guidance:

1. **Task 1-2**: Follow "Configuration Loading Flow" and "Code Changes" subsections
2. **Task 3**: Document JSON schema from "Configuration File Schema" subsection
3. **Task 4**: Test cases listed in AC#6 coverage (AC Coverage table)
4. **Task 5**: Standard build verification

**Critical constraints**:
- Default config MUST match F750's proven mappings (16=恋人, 3=恋慕, 17=思慕)
- Config file structure MUST mirror existing BranchConditions dictionary shape
- LoadMappingConfig() MUST implement fallback chain: custom → default → embedded

---

## Technical Design

### Approach

The implementation will externalize the hard-coded `BranchConditions` dictionary (lines 9-38 in Program.cs) into a JSON configuration file. The design follows these principles:

1. **Default-first**: Embed default mappings as fallback when config file is missing
2. **CLI override**: Support `--config <path>` for custom mappings
3. **Simple schema**: JSON structure mirrors the existing dictionary structure
4. **Backward compatible**: Default config preserves F750's proven indices (16, 3, 17)

**Configuration Loading Flow**:
```
1. Parse CLI args for --config option
2. If --config specified → Load from custom path
3. Else → Try loading from default path (talent-mapping.json in tool directory)
4. If file not found → Use embedded default (maintain F750 mappings)
5. Validate config structure (has keys 0, 1, 2)
6. Replace BranchConditions with loaded config
```

**Why JSON over YAML**: Program.cs already uses `System.Text.Json` ecosystem (.NET 8 built-in), avoiding additional dependency on YamlDotNet for config loading. YamlDotNet is only used for processing target kojo files.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `tools/YamlTalentMigrator/talent-mapping.json` with structure: `{"0": {"TALENT": {"16": {"ne": 0}}}, "1": {...}, "2": {...}}` |
| 2 | Add method `LoadMappingConfig(string? configPath)` to Program.cs that loads JSON and returns `Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>>`. Call it before file processing loop |
| 3 | Update `tools/YamlTalentMigrator/README.md` with section "Configuration" documenting JSON schema, default values, and `--config` usage |
| 4 | Parse `--config` CLI argument (similar to existing `--path` pattern at lines 45-53), pass to `LoadMappingConfig()` |
| 5 | Set config file content to: `{"0": {"TALENT": {"16": {"ne": 0}}}, "1": {"TALENT": {"3": {"ne": 0}}}, "2": {"TALENT": {"17": {"ne": 0}}}}` (extracted from current BranchConditions dictionary) |
| 6 | Create `tools/YamlTalentMigrator.Tests/ConfigLoadingTests.cs` with tests: `LoadDefaultConfig_Success()`, `LoadCustomConfig_Success()`, `MissingConfig_UsesFallback()`, `ConfigNotFoundTest()` |
| 7 | Ensure no syntax errors, no new dependencies beyond System.Text.Json (already available in .NET 8) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Config file format** | A) JSON, B) YAML, C) XML | **A) JSON** | System.Text.Json is .NET 8 built-in (no new dependency). YamlDotNet already exists for kojo processing but separating concerns keeps config loading simple. JSON is more portable for tooling |
| **Config location strategy** | A) Mandatory config file (error if missing), B) Default embedded fallback | **B) Default embedded fallback** | Maintains backward compatibility. Users who don't need customization can run tool without config file. Migration path: existing users see no change, new users can customize |
| **CLI option naming** | A) `--config`, B) `--mapping`, C) `--talent-config` | **A) `--config`** | Concise and follows common convention (e.g., `eslint --config`). Clear intent without being verbose. Consistent with `--path` and `--dry-run` existing options |
| **Config schema validation** | A) JSON Schema validation, B) Runtime type checking, C) No validation (fail late) | **B) Runtime type checking** | JSON Schema adds dependency. Runtime checks with clear error messages (e.g., "Missing branch 0 in config") provide immediate feedback without over-engineering. Dictionary key validation on load is sufficient |
| **Fallback strategy** | A) Hard error on missing config, B) Use embedded default (F750 values), C) Prompt user | **B) Use embedded default** | Satisfies "Default config matches original values" AC#5. Non-interactive tool should not prompt. Hard error breaks existing workflows. Embedded default ensures zero-config operation |

### Interfaces / Data Structures

**Configuration File Schema** (`talent-mapping.json`):
```json
{
  "0": {
    "TALENT": {
      "16": { "ne": 0 }
    }
  },
  "1": {
    "TALENT": {
      "3": { "ne": 0 }
    }
  },
  "2": {
    "TALENT": {
      "17": { "ne": 0 }
    }
  }
}
```

**Notes**:
- Top-level keys are branch indices (0-2)
- Second level is condition type (`"TALENT"` for TALENT-based conditions)
- Third level is TALENT index (e.g., 16=恋人, 3=恋慕, 17=思慕)
- Fourth level is operator mapping (e.g., `{"ne": 0}` means "not equal to 0")

**Code Changes**:

1. **New method** `LoadMappingConfig()`:
```csharp
private static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> LoadMappingConfig(string? configPath)
{
    // Try custom path first
    if (configPath != null && File.Exists(configPath))
    {
        string json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>>(json);
        if (config == null || !config.ContainsKey("0") || !config.ContainsKey("1") || !config.ContainsKey("2"))
        {
            Console.Error.WriteLine($"Error: Invalid config format in {configPath}");
            Environment.Exit(1);
        }
        return config;
    }

    // Try default location
    string defaultPath = Path.Combine(AppContext.BaseDirectory, "talent-mapping.json");
    if (File.Exists(defaultPath))
    {
        string json = File.ReadAllText(defaultPath);
        var config = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>>(json);
        if (config == null || !config.ContainsKey("0") || !config.ContainsKey("1") || !config.ContainsKey("2"))
        {
            Console.Error.WriteLine($"Error: Invalid config format in {defaultPath}");
            Environment.Exit(1);
        }
        return config;
    }

    // Fallback to embedded default (F750 mappings)
    return new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>
    {
        { "0", new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                { "TALENT", new Dictionary<string, Dictionary<string, int>>
                    {
                        { "16", new Dictionary<string, int> { { "ne", 0 } } }
                    }
                }
            }
        },
        { "1", new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                { "TALENT", new Dictionary<string, Dictionary<string, int>>
                    {
                        { "3", new Dictionary<string, int> { { "ne", 0 } } }
                    }
                }
            }
        },
        { "2", new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                { "TALENT", new Dictionary<string, Dictionary<string, int>>
                    {
                        { "17", new Dictionary<string, int> { { "ne", 0 } } }
                    }
                }
            }
        }
    };
}
```

2. **Update `Main()` signature**: Change `BranchConditions` from `static readonly` field to local variable loaded from config:
```csharp
// Parse --config argument
string? configPath = null;
for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--config")
    {
        configPath = args[i + 1];
        break;
    }
}

// Load mapping configuration
var branchConditions = LoadMappingConfig(configPath);
```

3. **Update `ProcessFile()` signature**: Pass `branchConditions` as parameter instead of referencing static field:
```csharp
private static async Task<(bool Modified, int BranchesUpdated)> ProcessFile(
    string filePath,
    bool dryRun,
    Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> branchConditions)
{
    // ... existing logic ...
    // Replace all references to BranchConditions with branchConditions parameter
    string branchKey = i.ToString();
    if (i < 3 && branchConditions.ContainsKey(branchKey))
    {
        branch["condition"] = branchConditions[branchKey];
    }
}
```

**Edge Case Handling**:
- **Empty config file**: Runtime validation checks for keys 0, 1, 2; exits with clear error if missing
- **Malformed JSON**: `JsonSerializer.Deserialize` throws `JsonException`; add try-catch with error message
- **Config file not found**: Falls back to embedded default (F750 mappings)
- **Custom config path does not exist**: Error message and exit (explicit user intent should not fallback silently)

---

## 残課題 (Deferred Items)

None - this feature is self-contained.

---

## Execution Log

| Date | Phase | Action | Result |
|------|-------|--------|--------|
| (pending) | - | Feature created via /fc 753 | [PROPOSED] |
| 2026-02-05 20:29 | Task 1 | Created talent-mapping.json with default mappings (16, 3, 17) | SUCCESS |
| 2026-02-05 20:36 | Task 3 | Updated README.md with configuration format documentation | SUCCESS |
| 2026-02-05 20:36 | Task 4 | Created unit tests for config loading (8 tests, all passing) | SUCCESS |