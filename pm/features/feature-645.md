# Feature 645: Kojo Quality Validator

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

## Created: 2026-01-27

---

## Summary

Implement KojoQualityValidator tool to enforce quality rules (4 branches x 4 variations x 4 lines) with incremental validation modes.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

Need mechanical enforcement of kojo quality rules. Currently no automated way to verify minimum content requirements.

### Goal (What to Achieve)

1. Implement KojoQualityValidator tool
2. Add `--diff HEAD~1` mode for incremental validation
3. Add `--files` mode for specific file validation
4. CI integration with git diff pipeline
5. Configurable quality rules (--min-entries, --min-lines)

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F644 | Equivalence Testing Framework | [DONE] |
| Successor | F646 | Post-Phase Review Phase 19 | [DRAFT] |

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Predecessor)
- [feature-675.md](feature-675.md) - YAML Format Unification (Transitive Predecessor)
- [feature-646.md](feature-646.md) - Post-Phase Review Phase 19 (Successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Line 4287-4291, 4332-4397

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Quality rules for kojo (4 branches x 4 variations x 4 lines) need to be mechanically enforced
2. Why: Manual quality review of 443+ YAML files is error-prone and inconsistent
3. Why: kojo_mapper.py provides quality metrics but cannot validate YAML files (it reads ERB)
4. Why: YamlValidator only validates schema compliance, not content quality rules
5. Why: No tool exists to bridge YAML parsing with quality rule validation using configurable thresholds

### Symptom vs Root Cause

| Symptom (Current) | Root Cause (Technical) |
|----------------|----------------------|
| Manual quality review is inconsistent | No automated quality validation tool for YAML dialogue files |
| Quality audit shows 0/405 Phase 8d PASS | Quality metrics exist in kojo_mapper.py but not applicable to YAML |
| Cannot validate quality on CI pipeline | Missing tool that combines YAML parsing with quality rule enforcement |

### Conclusion

The root cause is the lack of a dedicated YAML quality validation tool. Current tooling gap:
- **kojo_mapper.py**: Analyzes ERB files, produces quality metrics (branch_type, dialogue_text_lines, kojo_block_count), but cannot read YAML
- **YamlValidator**: Schema validation only, no content quality rules
- **KojoComparer**: Equivalence testing (ERB == YAML output), not quality validation

F645 fills this gap by creating a C# tool that parses YAML dialogue files and validates against configurable quality rules (branches, variations, lines per branch). This enables CI integration through `--diff` and `--files` modes for incremental validation.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F644 | [DONE] | Predecessor | Equivalence Testing Framework - verifies YAML output matches ERB |
| F675 | [PROPOSED] | Transitive Predecessor | YAML Format Unification - standardizes YAML structure F645 will validate |
| F636-F643 | Various | Predecessors | Conversion features produce YAML files F645 validates |
| F646 | [DRAFT] | Successor | Post-Phase Review Phase 19 - uses F645 results for quality gate |
| F555 | [DONE] | Planning | Phase 19 Planning - defined quality validator requirement (Task 6) |

### Pattern Analysis

F645 follows the established Phase 19 tooling pattern:
1. **Conversion tools** (F633-F635, ErbToYaml) - convert ERB to YAML
2. **Equivalence tools** (F644, KojoComparer) - verify conversion correctness
3. **Quality tools** (F645, KojoQualityValidator) - enforce content quality

This is a Phase 19 completion gate tool, not a recurring issue. The pattern is one-time tooling investment for migration quality assurance.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | YAML parsing + rule validation is straightforward C# implementation |
| Scope is realistic | YES | ~300 lines estimated, similar to existing tools |
| No blocking constraints | YES | F644 not strictly required (F645 can validate YAML independently) |

**Verdict**: FEASIBLE

**Notes**:
- F644 dependency is logical (validate equivalence before quality) but not technical
- YamlDotNet library already used in codebase for YAML parsing
- Quality rules are well-defined in full-csharp-architecture.md (4x4x4 minimum)

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | Runtime | Low | Already used in Era.Core and tools |
| .NET 10.0 | Runtime | Low | Standard toolchain |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| CI Pipeline (future) | HIGH | `--diff HEAD~1` for PR validation |
| F646 Post-Phase Review | MEDIUM | Quality gate for Phase 19 completion |
| Manual quality review | LOW | `--files` for targeted validation |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/KojoQualityValidator/KojoQualityValidator.csproj | Create | New project file |
| tools/KojoQualityValidator/Program.cs | Create | CLI entry point with --diff, --files, --min-* flags |
| tools/KojoQualityValidator/QualityValidator.cs | Create | Core validation logic |
| tools/KojoQualityValidator/YamlDialogueParser.cs | Create | YAML parsing adapted from Era.Core |
| tools/KojoQualityValidator.Tests/*.cs | Create | Unit tests |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| YAML format must be unified | F675 | MEDIUM - Validator must handle either `branches:` or `entries:` format |
| Quality rules are configurable | Design requirement | LOW - CLI flags for min thresholds |
| Git diff integration requires git | CI environment | LOW - Standard CI capability |
| 443+ files in production | Scale | MEDIUM - Need efficient parsing |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| YAML format mismatch (branches vs entries) | MEDIUM | HIGH | Wait for F675 or support both formats |
| Quality rules too strict for existing content | HIGH | MEDIUM | Configurable thresholds via --min-* flags |
| Parsing performance on large file sets | LOW | LOW | Parallelize file processing |
| Git diff mode complexity | LOW | MEDIUM | Use standard `git diff --name-only` approach |

## Architecture Investigation

### Existing Quality Analysis (kojo_mapper.py)

The Python tool already defines quality metrics:
```python
# QualityRule equivalent in kojo_mapper.py
PHASE_REQUIREMENTS = {
    "C2": {"branch_type": "TALENT_4", "patterns": 1},  # 4-branch
    "C3": {"branch_type": "TALENT_4", "patterns": 4},  # 4-branch x 4-variation
    "C6": {"branch_type": "FAV_9", "patterns": 9},     # 9-level FAV branching
}
```

Metrics tracked per function:
- `branch_type`: TALENT_4, TALENT_3, ABL_3, NONE, etc.
- `dialogue_text_lines`: PRINTFORM + DATAFORM lines
- `kojo_block_count`: Number of IF/ELSEIF/ELSE blocks with dialogue
- `lines_per_branch`: dialogue_text_lines / kojo_block_count

### Proposed KojoQualityValidator Structure

```
tools/KojoQualityValidator/
├── KojoQualityValidator.csproj
├── Program.cs                    # CLI entry (--diff, --files, --min-*)
├── QualityValidator.cs           # Core validation logic
├── YamlDialogueParser.cs         # YAML parsing
└── ValidationResult.cs           # Result model
```

### CLI Design

```bash
# Diff mode: validate files changed since commit
dotnet run --project tools/KojoQualityValidator -- --diff HEAD~1

# Files mode: validate specific files
dotnet run --project tools/KojoQualityValidator -- --files "Game/YAML/Kojo/1_美鈴/*.yaml"

# Custom rules
dotnet run --project tools/KojoQualityValidator -- \
  --files "Game/YAML/Kojo/*.yaml" \
  --min-branches 4 \
  --min-variations 4 \
  --min-lines 4

# CI integration
git diff --name-only origin/main -- '*.yaml' | xargs dotnet run --project tools/KojoQualityValidator
```

### Output Format (from full-csharp-architecture.md)

```
Validating 10 files (changed since HEAD~1)...

OK tea/meiling.yaml     4 branches x 4+ variations x 4+ lines
OK tea/sakuya.yaml      4 branches x 4+ variations x 4+ lines
NG tea/flandre.yaml     Branch[2]: Variations 3 < 4
NG tea/patchouli.yaml   Branch[1].Variation[0]: Lines 3 < 4

Result: 8/10 PASS, 2/10 FAIL
```

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "mechanical enforcement" | Tool must be automated, exit code indicates result | AC#1, AC#12, AC#13 |
| "automated way to verify minimum content requirements" | Tool parses YAML and validates against thresholds | AC#2, AC#3, AC#4, AC#5 |
| "4 branches x 4 lines minimum" | Default quality rules enforce MinBranches=4, MinLinesPerBranch=4 | AC#4, AC#5 |
| "--diff HEAD~1 mode" | Incremental validation via git diff | AC#6, AC#7 |
| "--files mode" | Specific file validation | AC#8, AC#9 |
| "Configurable quality rules" | CLI flags override defaults | AC#10, AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Project file exists | file | Glob(tools/KojoQualityValidator/KojoQualityValidator.csproj) | exists | - | [x] |
| 2 | Program.cs exists | file | Glob(tools/KojoQualityValidator/Program.cs) | exists | - | [x] |
| 3 | QualityValidator.cs exists | file | Glob(tools/KojoQualityValidator/QualityValidator.cs) | exists | - | [x] |
| 4 | Entry count validation logic | code | Grep(tools/KojoQualityValidator/QualityValidator.cs) | matches | "MinEntries" | [x] |
| 5 | Lines per entry validation logic | code | Grep(tools/KojoQualityValidator/QualityValidator.cs) | matches | "MinLinesPerEntry" | [x] |
| 6 | --diff flag defined | code | Grep(tools/KojoQualityValidator/Program.cs) | contains | "--diff" | [x] |
| 7 | --diff mode uses git diff | code | Grep(tools/KojoQualityValidator/Program.cs) | matches | "git diff.*--name-only" | [x] |
| 8 | --files flag defined | code | Grep(tools/KojoQualityValidator/Program.cs) | contains | "--files" | [x] |
| 9 | --files mode accepts patterns | code | Grep(tools/KojoQualityValidator/Program.cs) | matches | "GetFiles\\|Directory.EnumerateFiles\\|Glob" | [x] |
| 10 | --min-entries flag defined | code | Grep(tools/KojoQualityValidator/Program.cs) | contains | "--min-entries" | [x] |
| 11 | --min-lines flag defined | code | Grep(tools/KojoQualityValidator/Program.cs) | contains | "--min-lines" | [x] |
| 12 | Build succeeds | build | dotnet build tools/KojoQualityValidator | succeeds | - | [x] |
| 13 | Unit tests pass | test | dotnet test tools/KojoQualityValidator.Tests | succeeds | - | [x] |
| 14 | Test: PASS file returns exit 0 | exit_code | dotnet run --project tools/KojoQualityValidator -- --files "TestData/quality-pass.yaml" | succeeds | 0 | [x] |
| 15 | Test: FAIL file returns exit 1 | exit_code | dotnet run --project tools/KojoQualityValidator -- --files "TestData/quality-fail.yaml" | fails | 1 | [x] |
| 16 | Zero technical debt | code | Grep(tools/KojoQualityValidator/) | not_matches | "TODO\\|FIXME\\|HACK" | [x] |

### AC Details

**AC#1: Project file exists**
- Test: Glob pattern `tools/KojoQualityValidator/KojoQualityValidator.csproj`
- Rationale: Standard .NET project structure required for `dotnet run/build`

**AC#2: Program.cs exists**
- Test: Glob pattern `tools/KojoQualityValidator/Program.cs`
- Rationale: CLI entry point with argument parsing

**AC#3: QualityValidator.cs exists**
- Test: Glob pattern `tools/KojoQualityValidator/QualityValidator.cs`
- Rationale: Core validation logic separated from CLI parsing

**AC#4: Branch count validation logic**
- Test: Grep pattern=`MinBranches` path=`tools/KojoQualityValidator/QualityValidator.cs`
- Rationale: Quality rule for branch count (default: 4)
- Validates YAML structure has required number of branches (TALENT_4 pattern)

**AC#5: Lines per branch validation logic**
- Test: Grep pattern=`MinLinesPerBranch` path=`tools/KojoQualityValidator/QualityValidator.cs`
- Rationale: Quality rule for minimum dialogue lines per branch (default: 4)
- Each branch must have sufficient dialogue content (4+ lines)

**AC#6: --diff flag defined**
- Test: Grep pattern=`--diff` path=`tools/KojoQualityValidator/Program.cs`
- Rationale: Incremental validation mode for CI integration

**AC#7: --diff mode uses git diff**
- Test: Grep pattern=`git diff.*--name-only` path=`tools/KojoQualityValidator/Program.cs`
- Rationale: Must shell out to git to get changed files since specified commit
- Typical usage: `--diff HEAD~1` validates files changed in last commit

**AC#8: --files flag defined**
- Test: Grep pattern=`--files` path=`tools/KojoQualityValidator/Program.cs`
- Rationale: Direct file/pattern validation mode

**AC#9: --files mode accepts patterns**
- Test: Grep pattern=`GetFiles|Directory.EnumerateFiles|Glob` path=`tools/KojoQualityValidator/Program.cs`
- Rationale: Must support glob patterns for flexible file selection
- Example: `--files "Game/YAML/Kojo/**/*.yaml"`

**AC#10: --min-branches flag defined**
- Test: Grep pattern=`--min-branches` path=`tools/KojoQualityValidator/Program.cs`
- Rationale: Override default branch threshold (4)

**AC#11: --min-lines flag defined**
- Test: Grep pattern=`--min-lines` path=`tools/KojoQualityValidator/Program.cs`
- Rationale: Override default lines per branch threshold (4)

**AC#12: Build succeeds**
- Test: `dotnet build tools/KojoQualityValidator`
- Rationale: Tool must compile without errors

**AC#13: Unit tests pass**
- Test: `dotnet test tools/KojoQualityValidator.Tests`
- Rationale: Core validation logic verified through unit tests
- Tests cover: YAML parsing, branch/line counting, threshold comparison

**AC#14: Test: PASS file returns exit 0 (Positive)**
- Test: `dotnet run --project tools/KojoQualityValidator -- --files "tests/fixtures/quality-pass.yaml"`
- Expected: Exit code 0
- Rationale: CI integration requires exit code 0 for passing validation
- Fixture file meets 4x4 minimum (4 branches, 4 lines per branch)

**AC#15: Test: FAIL file returns exit 1 (Negative)**
- Test: `dotnet run --project tools/KojoQualityValidator -- --files "tests/fixtures/quality-fail.yaml"`
- Expected: Exit code 1
- Rationale: CI integration requires non-zero exit for failing validation
- Fixture file intentionally fails quality threshold

**AC#16: Zero technical debt**
- Test: Grep pattern=`TODO|FIXME|HACK` path=`tools/KojoQualityValidator/`
- Expected: 0 matches
- Rationale: Production-ready code with no deferred work

---

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Create project structure and core files (KojoQualityValidator.csproj, Program.cs, QualityValidator.cs) | [x] |
| 2 | 4,5 | Implement QualityRule record and validation logic (MinBranches, MinLinesPerBranch) | [x] |
| 3 | 6,7 | Implement --diff mode with git diff integration | [x] |
| 4 | 8,9 | Implement --files mode with pattern matching | [x] |
| 5 | 10,11 | Implement CLI argument parsing with --min-* flags | [x] |
| 6 | 12,13 | Create unit test project with xUnit tests for validation logic | [x] |
| 7 | 14,15 | Create integration tests with fixture files (quality-pass.yaml, quality-fail.yaml) | [x] |
| 8 | 16 | Final review and technical debt elimination | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T5 | Technical Design specs | KojoQualityValidator tool with CLI |
| 2 | implementer | sonnet | T6-T7 | Test requirements from ACs | Unit and integration tests |
| 3 | ac-tester | haiku | T8 | AC table | Verification results |

**Constraints** (from Technical Design):

1. **Standalone tool**: NO Era.Core dependency - this is migration-specific tooling
2. **YAML format**: Validates `entries` format (unified kojo YAML per F675)
3. **Exit codes**: Binary success (0) / failure (1) for CI integration
4. **Quality rules**: Default MinEntries=4, MinLinesPerEntry=4
5. **Git requirement**: Git must be in PATH for --diff mode

**Quality Rule Clarification**:

Original architecture doc used "4 branches x 4 variations x 4 lines" but this is actually:
- **MinBranches**: At least 4 branches (e.g., 4 TALENT levels)
- **MinLinesPerBranch**: At least 4 dialogue lines per branch

"Variations" in the context means "branches" (each branch is a variation of the dialogue). The corrected model is:

```csharp
public record QualityRule(
    int MinBranches = 4,      // e.g., 4 TALENT branches
    int MinLinesPerBranch = 4 // e.g., 4 dialogue lines per branch
);
```

**Project Structure**:

```
tools/KojoQualityValidator/
├── KojoQualityValidator.csproj          # Project file (net10.0, Exe)
├── Program.cs                            # CLI entry point, argument parsing, main loop
├── QualityValidator.cs                   # Core validation logic
├── Models/
│   ├── KojoFile.cs                       # YAML deserialization model
│   └── ValidationResult.cs               # Output model
└── README.md                             # Usage documentation

tools/KojoQualityValidator.Tests/
├── KojoQualityValidator.Tests.csproj     # Test project (xunit)
├── QualityValidatorTests.cs              # Unit tests for validation logic
├── fixtures/
│   ├── quality-pass.yaml                 # Test file meeting 4x4x4
│   └── quality-fail.yaml                 # Test file failing requirements
└── IntegrationTests.cs                   # End-to-end CLI tests
```

**Dependencies**:

```xml
<!-- KojoQualityValidator.csproj -->
<ItemGroup>
  <PackageReference Include="YamlDotNet" Version="16.2.0" />
  <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
</ItemGroup>
```

**CLI Argument Structure**:

```bash
# Usage patterns
dotnet run --project tools/KojoQualityValidator -- --diff HEAD~1
dotnet run --project tools/KojoQualityValidator -- --files "Game/YAML/Kojo/**/*.yaml"
dotnet run --project tools/KojoQualityValidator -- --files "Game/YAML/Kojo/1_美鈴/*.yaml" --min-branches 3
```

**Argument definitions** (using System.CommandLine):
- `--diff <commit>`: Validate files changed since commit (mutually exclusive with `--files`)
- `--files <pattern>`: File pattern to validate (mutually exclusive with `--diff`)
- `--min-branches <int>`: Override default branch threshold (default: 4)
- `--min-lines <int>`: Override default lines threshold (default: 4)

**Data Models**:

```csharp
// Models/KojoFile.cs
public class KojoFile
{
    public string Character { get; set; } = string.Empty;
    public string Situation { get; set; } = string.Empty;
    public List<KojoBranch> Branches { get; set; } = new();
}

public class KojoBranch
{
    public List<string> Lines { get; set; } = new();
    public Dictionary<string, object>? Condition { get; set; }
}

// Models/ValidationResult.cs
public class ValidationResult
{
    public string FilePath { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public int BranchCount { get; init; }
    public int MinLines { get; init; }
    public int MaxLines { get; init; }
}

// QualityValidator.cs
public record QualityRule(
    int MinBranches = 4,
    int MinLinesPerBranch = 4
);
```

**Git Diff Integration**:

```csharp
// Program.cs - GetChangedFiles method
private static List<string> GetChangedFiles(string commit)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "git",
        Arguments = $"diff --name-only {commit} -- *.yaml",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(startInfo);
    if (process == null)
        throw new InvalidOperationException("Failed to start git process");

    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    if (process.ExitCode != 0)
        throw new InvalidOperationException($"git diff failed with exit code {process.ExitCode}");

    return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Where(f => f.EndsWith(".yaml"))
        .Select(f => Path.GetFullPath(f))
        .ToList();
}
```

**Output Format**:

```
Validating 10 files...

✓ K1_愛撫_0.yaml               4 branches × 4+ lines
✓ K1_会話親密_0.yaml            4 branches × 4+ lines
✗ K1_日常_0.yaml               Branch count 3 < 4
✗ K10_会話親密_0.yaml           Branch[2]: Lines 3 < 4

Result: 8/10 PASS, 2/10 FAIL
```

**Test Fixtures**:

**quality-pass.yaml** (4 branches × 4 lines each):
```yaml
character: 美鈴
situation: TEST
branches:
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
```

**quality-fail.yaml** (fails MinLinesPerBranch):
```yaml
character: 美鈴
situation: TEST
branches:
- lines: ["Line 1", "Line 2", "Line 3"]  # Only 3 lines (fails MinLines=4)
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
```

**Pre-conditions**:

1. Git must be installed and in PATH (for --diff mode)
2. .NET 10.0 SDK installed
3. YamlDotNet and System.CommandLine packages available via NuGet

**Success Criteria**:

1. All 16 ACs pass verification
2. Tool runs without errors for both --diff and --files modes
3. Exit code 0 for passing files, exit code 1 for failing files
4. Unit tests cover all validation logic (branch count, line count, edge cases)
5. Integration tests verify CLI behavior with fixture files
6. No technical debt markers (TODO/FIXME/HACK)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-30 07:46 | START | implementer | Phase 3 TDD: Create test project |
| 2026-01-30 07:46 | END | implementer | Phase 3 TDD: Tests in RED state |
| 2026-01-30 09:14 | START | implementer | T1-T5: Implement KojoQualityValidator tool |
| 2026-01-30 09:14 | END | implementer | T1-T5 complete: All tests GREEN, build succeeds |
| 2026-01-30 | DEVIATION | feature-reviewer | post-review | NEEDS_REVISION: spec inconsistencies (branches→entries naming) |
| 2026-01-30 09:20 | START | implementer | Bug fix: Add FilePath property to ValidationResult |
| 2026-01-30 09:20 | END | implementer | Bug fix complete: All 10 tests pass, build succeeds |

---

## Technical Design

### Approach

Create a standalone CLI tool that parses kojo YAML files and validates against configurable quality rules (branches, variations, lines). The tool will:

1. **Parse YAML structure** directly using YamlDotNet (same library as Era.Core)
2. **Count quality metrics** for each file (branches, variations per branch, lines per variation)
3. **Validate against thresholds** (default: 4x4x4, overridable via CLI flags)
4. **Support multiple modes**:
   - `--diff HEAD~1`: Validate files changed since specified commit (via `git diff --name-only`)
   - `--files "pattern"`: Validate specific files or glob patterns
5. **Return exit codes** for CI integration (0=PASS, 1=FAIL)

**Key Design Decision**: This is a **standalone tool**, NOT integrated into Era.Core. Rationale:
- Quality validation is a one-time migration concern (Phase 19 completion gate)
- Era.Core's YamlDialogueLoader uses `entries` format (simplified dialogue system)
- Kojo YAML uses `branches` format (legacy format for converted files)
- Standalone tool avoids polluting Era.Core with migration-specific logic

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `tools/KojoQualityValidator/KojoQualityValidator.csproj` with OutputType=Exe, TargetFramework=net10.0 |
| 2 | Create `tools/KojoQualityValidator/Program.cs` with CLI argument parsing and main entry point |
| 3 | Create `tools/KojoQualityValidator/QualityValidator.cs` with validation logic (Validate method) |
| 4 | QualityValidator.cs defines `QualityRule` record with `MinBranches` property (default: 4) |
| 5 | QualityValidator.cs defines `QualityRule` record with `MinLinesPerBranch` property (default: 4) |
| 6 | QualityValidator.cs defines `QualityRule` record with `MinLines` property (default: 4) |
| 7 | Program.cs defines `--diff` argument flag using argument parser |
| 8 | Program.cs uses `Process.Start("git", "diff --name-only {commit} -- *.yaml")` to get changed files |
| 9 | Program.cs defines `--files` argument flag for direct file/pattern specification |
| 10 | Program.cs uses `Directory.GetFiles(pattern, "*.yaml", SearchOption.AllDirectories)` for glob expansion |
| 11 | Program.cs defines `--min-branches` argument with int value (default: 4) |
| 12 | Program.cs defines `--min-variations` argument with int value (default: 4) |
| 13 | Program.cs defines `--min-lines` argument with int value (default: 4) |
| 14 | Ensure all code compiles without errors (standard .NET project setup) |
| 15 | Create `tools/KojoQualityValidator.Tests/` with xUnit tests for QualityValidator.cs logic |
| 16 | Create `tools/KojoQualityValidator.Tests/fixtures/quality-pass.yaml` with 4+ branches x 4+ variations x 4+ lines, ensure Main returns 0 |
| 17 | Create `tools/KojoQualityValidator.Tests/fixtures/quality-fail.yaml` with insufficient quality (e.g., 3 variations), ensure Main returns 1 |
| 18 | Write production-ready code without TODO/FIXME/HACK comments |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **YAML parsing library** | (A) YamlDotNet, (B) Custom parser | **A** YamlDotNet | Already used in Era.Core, mature library, handles edge cases |
| **Integration point** | (A) Standalone tool, (B) Era.Core extension | **A** Standalone | Quality validation is migration-specific, not runtime concern. Era.Core uses `entries` format, kojo uses `branches` format |
| **Data model** | (A) Reuse Era.Core DialogueFile, (B) Custom KojoFile model | **B** Custom model | Era.Core model is for `entries` format (simplified dialogue). Kojo YAML uses `branches` format (legacy). Custom model avoids schema confusion |
| **Git integration** | (A) LibGit2Sharp library, (B) Shell out to git | **B** Shell out | Simpler, no extra dependency, git is always available in CI. Use `Process.Start("git", "diff --name-only")` |
| **File pattern matching** | (A) Custom glob, (B) Directory.GetFiles | **B** Directory.GetFiles | Built-in .NET method supports patterns like `"Game/YAML/Kojo/**/*.yaml"` via SearchOption.AllDirectories |
| **Output format** | (A) JSON, (B) Human-readable text | **B** Text | CI pipelines prefer simple text output. Format: `✓/✗ filename  details` with summary line |
| **Exit code strategy** | (A) Exit code = failure count, (B) Binary 0/1 | **B** Binary | Standard CI convention: 0=success, non-zero=failure. Simplifies pipeline logic |
| **Argument parsing** | (A) Manual args parsing, (B) System.CommandLine | **B** System.CommandLine | Industry standard, handles --help generation, type conversion, validation |

### Data Structures

#### KojoFile (YAML deserialization model)

```csharp
// tools/KojoQualityValidator/Models/KojoFile.cs
public class KojoFile
{
    public string Character { get; set; } = string.Empty;
    public string Situation { get; set; } = string.Empty;
    public List<KojoBranch> Branches { get; set; } = new();
}

public class KojoBranch
{
    public List<string> Lines { get; set; } = new();
    public Dictionary<string, object>? Condition { get; set; }
}
```

**Rationale**: Matches actual YAML structure (`branches:`, `lines:`, `condition:`). Uses `Dictionary<string, object>` for condition to avoid parsing complexity (validation doesn't need condition logic).

#### QualityRule (Configuration)

```csharp
// tools/KojoQualityValidator/QualityValidator.cs
public record QualityRule(
    int MinBranches = 4,
    int MinLinesPerBranch = 4
    int MinLines = 4
);
```

**Rationale**: Record type for immutability. Default values match 4x4x4 requirement from full-csharp-architecture.md.

**Note**: "Variations" refers to BRANCHES (4 branch blocks), not sub-variations within a branch. Each branch has multiple dialogue lines, and we validate line count per branch.

**Clarification**: The quality rule is:
- **MinBranches**: At least N branches (e.g., 4 TALENT levels)
- **MinLinesPerBranch**: Minimum dialogue lines per branch (default: 4)
- **MinLines**: At least N lines per branch (e.g., 4 dialogue lines)

**Corrected model**:
```csharp
public record QualityRule(
    int MinBranches = 4,      // e.g., 4 TALENT branches
    int MinLinesPerBranch = 4 // e.g., 4 dialogue lines per branch
);
```

**AC Impact**: AC#5 matcher "MinLinesPerBranch" validates minimum dialogue lines per branch. The corrected model has 2 properties: MinBranches and MinLinesPerBranch.

#### ValidationResult (Output)

```csharp
// tools/KojoQualityValidator/Models/ValidationResult.cs
public class ValidationResult
{
    public string FilePath { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public int BranchCount { get; init; }
    public int MinLines { get; init; }
    public int MaxLines { get; init; }
}
```

**Rationale**: Contains all information needed for reporting (pass/fail, error details, quality metrics).

### Implementation Details

#### CLI Argument Structure

```bash
# Usage patterns
dotnet run --project tools/KojoQualityValidator -- --diff HEAD~1
dotnet run --project tools/KojoQualityValidator -- --files "Game/YAML/Kojo/**/*.yaml"
dotnet run --project tools/KojoQualityValidator -- --files "Game/YAML/Kojo/1_美鈴/*.yaml" --min-branches 3
```

**Argument definitions** (using System.CommandLine):
- `--diff <commit>`: Validate files changed since commit (mutually exclusive with `--files`)
- `--files <pattern>`: File pattern to validate (mutually exclusive with `--diff`)
- `--min-branches <int>`: Override default branch threshold (default: 4)
- `--min-lines <int>`: Override default lines threshold (default: 4)

**Validation**:
- At least one of `--diff` or `--files` must be specified
- `--min-*` values must be positive integers

#### Git Diff Integration

```csharp
// Program.cs - GetChangedFiles method
private static List<string> GetChangedFiles(string commit)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "git",
        Arguments = $"diff --name-only {commit} -- *.yaml",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(startInfo);
    if (process == null)
        throw new InvalidOperationException("Failed to start git process");

    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    if (process.ExitCode != 0)
        throw new InvalidOperationException($"git diff failed with exit code {process.ExitCode}");

    return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Where(f => f.EndsWith(".yaml"))
        .Select(f => Path.GetFullPath(f))
        .ToList();
}
```

**Edge cases handled**:
- Empty output (no changed files) → Return empty list, exit 0
- Git not in PATH → Throw with clear error message
- Non-zero exit code → Throw with error message

#### File Pattern Expansion

```csharp
// Program.cs - GetFilesFromPattern method
private static List<string> GetFilesFromPattern(string pattern)
{
    // Handle both absolute paths and relative patterns
    var basePath = Path.IsPathRooted(pattern)
        ? Path.GetDirectoryName(pattern) ?? throw new ArgumentException("Invalid pattern")
        : Directory.GetCurrentDirectory();

    var searchPattern = Path.GetFileName(pattern);

    // Support wildcard patterns
    var files = Directory.GetFiles(
        basePath,
        searchPattern,
        SearchOption.AllDirectories
    ).ToList();

    if (files.Count == 0)
        Console.WriteLine($"Warning: Pattern '{pattern}' matched 0 files");

    return files;
}
```

**Edge cases handled**:
- Pattern matches 0 files → Print warning, exit 0 (not an error)
- Invalid directory → Throw with clear error message
- Relative vs absolute paths → Handle both via Path.IsPathRooted

#### Validation Logic

```csharp
// QualityValidator.cs - Validate method
public ValidationResult Validate(string filePath, QualityRule rule)
{
    var yaml = File.ReadAllText(filePath);
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    var kojo = deserializer.Deserialize<KojoFile>(yaml);
    var errors = new List<string>();

    // Validate branch count
    if (kojo.Branches.Count < rule.MinBranches)
        errors.Add($"Branch count {kojo.Branches.Count} < {rule.MinBranches}");

    // Validate lines per branch
    var lineCounts = kojo.Branches.Select((b, i) => (Index: i, Count: b.Lines.Count)).ToList();
    foreach (var (index, count) in lineCounts)
    {
        if (count < rule.MinLines)
            errors.Add($"Branch[{index}]: Lines {count} < {rule.MinLines}");
    }

    return new ValidationResult
    {
        FilePath = filePath,
        IsValid = errors.Count == 0,
        Errors = errors,
        BranchCount = kojo.Branches.Count,
        MinLines = lineCounts.Min(x => x.Count),
        MaxLines = lineCounts.Max(x => x.Count)
    };
}
```

**Edge cases handled**:
- Empty file → YAML parse error, catch and report
- Missing `branches` field → Deserializes as empty list, fails branch count validation
- Empty `lines` array → Count=0, fails lines validation
- Invalid YAML syntax → Catch YamlException, report parse error

#### Output Format

```csharp
// Program.cs - PrintResults method
private static void PrintResults(List<ValidationResult> results)
{
    Console.WriteLine($"Validating {results.Count} files...\n");

    var passCount = 0;
    foreach (var result in results)
    {
        var icon = result.IsValid ? "✓" : "✗";
        var filename = Path.GetFileName(result.FilePath);

        if (result.IsValid)
        {
            Console.WriteLine($"{icon} {filename,-30} {result.BranchCount} branches × {result.MinLines}+ lines");
            passCount++;
        }
        else
        {
            Console.WriteLine($"{icon} {filename,-30} {string.Join(", ", result.Errors)}");
        }
    }

    Console.WriteLine($"\nResult: {passCount}/{results.Count} PASS, {results.Count - passCount}/{results.Count} FAIL");
}
```

**Output example**:
```
Validating 10 files...

✓ K1_愛撫_0.yaml               4 branches × 4+ lines
✓ K1_会話親密_0.yaml            4 branches × 4+ lines
✗ K1_日常_0.yaml               Branch count 3 < 4
✗ K10_会話親密_0.yaml           Branch[2]: Lines 3 < 4

Result: 8/10 PASS, 2/10 FAIL
```

### Project Structure

```
tools/KojoQualityValidator/
├── KojoQualityValidator.csproj          # Project file (net10.0, Exe)
├── Program.cs                            # CLI entry point, argument parsing, main loop
├── QualityValidator.cs                   # Core validation logic
├── Models/
│   ├── KojoFile.cs                       # YAML deserialization model
│   └── ValidationResult.cs               # Output model
└── README.md                             # Usage documentation

tools/KojoQualityValidator.Tests/
├── KojoQualityValidator.Tests.csproj     # Test project (xunit)
├── QualityValidatorTests.cs              # Unit tests for validation logic
├── fixtures/
│   ├── quality-pass.yaml                 # Test file meeting 4x4x4
│   └── quality-fail.yaml                 # Test file failing requirements
└── IntegrationTests.cs                   # End-to-end CLI tests
```

### Dependencies

```xml
<!-- KojoQualityValidator.csproj -->
<ItemGroup>
  <PackageReference Include="YamlDotNet" Version="16.2.0" />
  <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
</ItemGroup>
```

**Rationale**:
- YamlDotNet: Same version as Era.Core (consistency)
- System.CommandLine: Microsoft's official argument parsing library (beta but stable)
- NO Era.Core dependency: Standalone tool avoids coupling to game engine

### Risk Mitigation

| Risk | Mitigation |
|------|------------|
| YAML format mismatch (branches vs entries) | Document clearly that this tool validates `branches` format only. Add validation check to fail fast on `entries` format |
| Quality rules too strict | Default to 4x4x4 but make configurable via `--min-*` flags. Document relaxation patterns |
| Git diff fails in CI | Wrap git calls in try-catch, provide clear error messages. Document git requirement |
| Large file sets slow | Process files in parallel using `Parallel.ForEach` (optimization, not critical) |

### Testing Strategy

#### Unit Tests (QualityValidator.Tests.cs)

```csharp
[Fact]
public void Validate_WithValidFile_ReturnsSuccess()
{
    // 4 branches × 4+ lines
    var yaml = CreateValidYaml(branchCount: 4, linesPerBranch: 4);
    var result = validator.Validate(yaml, new QualityRule());
    Assert.True(result.IsValid);
}

[Fact]
public void Validate_WithInsufficientBranches_ReturnsFailure()
{
    var yaml = CreateValidYaml(branchCount: 3, linesPerBranch: 4);
    var result = validator.Validate(yaml, new QualityRule(MinBranches: 4));
    Assert.False(result.IsValid);
    Assert.Contains("Branch count 3 < 4", result.Errors);
}

[Fact]
public void Validate_WithInsufficientLines_ReturnsFailure()
{
    var yaml = CreateValidYaml(branchCount: 4, linesPerBranch: 3);
    var result = validator.Validate(yaml, new QualityRule(MinLines: 4));
    Assert.False(result.IsValid);
    Assert.Contains("Lines 3 < 4", result.Errors[0]);
}
```

#### Integration Tests (IntegrationTests.cs)

```csharp
[Fact]
public void CLI_WithPassingFile_ReturnsExitCode0()
{
    var exitCode = RunCLI("--files", "fixtures/quality-pass.yaml");
    Assert.Equal(0, exitCode);
}

[Fact]
public void CLI_WithFailingFile_ReturnsExitCode1()
{
    var exitCode = RunCLI("--files", "fixtures/quality-fail.yaml");
    Assert.Equal(1, exitCode);
}

[Fact]
public void CLI_WithDiffMode_ValidatesChangedFilesOnly()
{
    // Setup: Create test git repo, commit file, modify file
    var exitCode = RunCLI("--diff", "HEAD~1");
    Assert.Equal(0, exitCode); // Assuming modified file passes
}
```

#### Test Fixtures

**quality-pass.yaml**:
```yaml
character: 美鈴
situation: TEST
branches:
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
```

**quality-fail.yaml**:
```yaml
character: 美鈴
situation: TEST
branches:
- lines: ["Line 1", "Line 2", "Line 3"]  # Only 3 lines (fails MinLines=4)
  condition: {}
- lines: ["Line 1", "Line 2", "Line 3", "Line 4"]
  condition: {}
```

### AC Satisfaction Mapping

Each AC is directly satisfied by the design:

- **AC#1-3**: File structure created per project layout
- **AC#4-5**: QualityRule record with MinBranches/MinLinesPerBranch properties
- **AC#6-7**: --diff flag with git diff integration via Process.Start
- **AC#8-9**: --files flag with file pattern support via Directory.GetFiles
- **AC#10-11**: --min-branches/--min-lines CLI flags via System.CommandLine
- **AC#12**: Standard .NET build process (dotnet build)
- **AC#13**: xUnit test project with unit tests
- **AC#14-15**: Integration tests with fixture files, exit code validation
- **AC#16**: Production code without technical debt markers (TODO/FIXME/HACK)
