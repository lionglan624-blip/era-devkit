# Feature 634: Batch Conversion Tool

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

Implement batch conversion mode for ErbToYaml tool to process 117 kojo ERB files in directory batch mode.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Establish ErbToYaml as the single automated conversion pipeline for all 117 ERB kojo files, ensuring consistent YAML output structure and enabling parallelized batch processing for downstream features (F635-F643)

### Problem (Current Issue)

ErbToYaml currently processes single files only. Manual conversion of 117 files is impractical. Need `--batch` mode for directory processing.

### Goal (What to Achieve)

1. Add `--batch <directory>` mode to ErbToYaml
2. Process all ERB files in directory recursively
3. Generate YAML output maintaining directory structure
4. Report conversion success/failure per file

---


## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-633.md](feature-633.md) - PRINTDATA Parser Extension (Predecessor)
- [feature-635.md](feature-635.md) - Conversion Parallelization (Successor)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Downstream)
- [feature-349.md](feature-349.md) - Original DATALIST->YAML converter (Historical)
- [feature-351.md](feature-351.md) - Pilot conversion (Historical)
- [feature-361.md](feature-361.md) - Schema validation (Historical)
- [feature-636.md](feature-636.md) - Per-character conversion batch (F636)
- [feature-637.md](feature-637.md) - Per-character conversion batch (F637)
- [feature-638.md](feature-638.md) - Per-character conversion batch (F638)
- [feature-639.md](feature-639.md) - Per-character conversion batch (F639)
- [feature-640.md](feature-640.md) - Per-character conversion batch (F640)
- [feature-641.md](feature-641.md) - Per-character conversion batch (F641)
- [feature-642.md](feature-642.md) - Per-character conversion batch (F642)
- [feature-643.md](feature-643.md) - Per-character conversion batch (F643)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Architecture reference

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Manual conversion of 117 kojo ERB files to YAML is impractical
2. Why: ErbToYaml has no CLI entry point or batch processing capability -- it is a library-only project consumed via project reference
3. Why: ErbToYaml was built incrementally for single-file conversion as part of F349/F351 pilot work, focused on proving DatalistConverter correctness
4. Why: Phase 19 planning (F555) identified batch conversion as a separate tooling step to follow PRINTDATA parser extension (F633)
5. Why: The pipeline architecture separates parser (ErbParser), converter (ErbToYaml library), and orchestration (not yet built) into distinct concerns

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| Cannot convert 117 files at once | ErbToYaml is a library without CLI entry point or batch orchestration layer |
| No `--batch` mode exists | No Program.cs / executable project exists for ErbToYaml at all |

### Conclusion

The root cause is that ErbToYaml is a **class library** (no `<OutputType>Exe</OutputType>`, no `Program.cs`), not a CLI tool. The feature requires:
1. Converting ErbToYaml from a library to an executable (or creating a new CLI project that references it)
2. Adding argument parsing (`--batch <directory>`, single-file mode)
3. Adding directory traversal with recursive ERB file discovery
4. Adding per-file conversion orchestration (parse -> convert -> write YAML)
5. Adding summary reporting (success/failure per file)

The existing `DatalistConverter` handles individual DATALIST node conversion but lacks:
- File-level orchestration (parsing a whole ERB file and extracting all convertible structures)
- The concept of "character" and "situation" extraction from file paths/names
- PRINTDATA-aware conversion (F633 added parsing; conversion of PrintDataNode content is needed)

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F633 | [DONE] | Predecessor (parser) | Added PRINTDATA...ENDDATA parsing to ErbParser. PrintDataNode now exists in AST with GetDataForms() helper |
| F635 | [DRAFT] | Successor (parallelization) | Adds parallel processing to batch mode -- depends on F634 batch infrastructure |
| F636-F643 | [DRAFT] | Consumers | Per-character conversion batches that will invoke the batch converter |
| F644 | [DRAFT] | Downstream quality | Equivalence testing framework that validates conversion output |
| F349 | Historical | Foundation | Original DATALIST->YAML converter (DatalistConverter) |
| F351 | Historical | Pilot | Pilot conversion (Meiling COM_0) proved single-file pipeline works |
| F361 | Historical | Schema validation | Added schema validation to DatalistConverter |

### Pattern Analysis

KojoComparer tool (`tools/KojoComparer/`) already implements a similar batch pattern via `BatchProcessor.cs` with `ProcessAsync(erbDirectory, yamlDirectory, ...)`. This provides a proven reference architecture for directory traversal and per-file reporting in this codebase.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All building blocks exist: ErbParser (with PRINTDATA), DatalistConverter, schema validation. Only orchestration layer is missing |
| Scope is realistic | YES | ~4-6 new/modified files. KojoComparer BatchProcessor provides reference pattern. No novel algorithms needed |
| No blocking constraints | YES | F633 (PRINTDATA parser) is [DONE]. All dependencies are satisfied |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F633 | [DONE] | PRINTDATA parser extension -- PrintDataNode AST with Content and GetDataForms() |
| Successor | F635 | [DRAFT] | Parallelization -- requires batch infrastructure from F634 |
| Successor | F636 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |
| Successor | F637 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |
| Successor | F638 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |
| Successor | F639 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |
| Successor | F640 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |
| Successor | F641 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |
| Successor | F642 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |
| Successor | F643 | [DRAFT] | Per-character conversion batch -- consumes F634 batch tool |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| ErbParser (tools/ErbParser/) | Build-time | Low | Project reference already exists in ErbToYaml.csproj |
| YamlDotNet 16.2.1 | Runtime | Low | Already referenced in ErbToYaml.csproj |
| NJsonSchema 11.1.0 | Runtime | Low | Already referenced for schema validation |
| Talent.csv (eraTW reference path) | Runtime | Low | Required by TalentCsvLoader for condition resolution - path to be configurable |
| dialogue-schema.json (tools/YamlSchemaGen/) | Runtime | Low | Required for output schema validation |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F635 Conversion Parallelization | HIGH | Wraps F634 batch mode with parallel execution |
| F636-F643 Per-character conversion | HIGH | Invoke batch converter per character directory |
| F644 Equivalence Testing Framework | MEDIUM | Tests converted YAML output from F634 |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/ErbToYaml.csproj | Update | Add `<OutputType>Exe</OutputType>` to make it executable |
| tools/ErbToYaml/Program.cs | Create | CLI entry point with `--batch <dir>` and single-file argument parsing |
| tools/ErbToYaml/BatchConverter.cs | Create | Directory traversal, per-file orchestration, summary report generation |
| tools/ErbToYaml/FileConverter.cs | Create | Single-file conversion orchestration (parse ERB -> extract PrintData/Datalist -> convert -> write YAML) |
| tools/ErbToYaml/ConversionResult.cs | Create | Result model (Success/Failure per file with error details) |
| tools/ErbToYaml.Tests/BatchConverterTests.cs | Create | Tests for batch conversion logic |
| tools/ErbToYaml.Tests/FileConverterTests.cs | Create | Tests for single-file conversion orchestration |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| ErbToYaml is currently a library (no OutputType Exe) | ErbToYaml.csproj | MEDIUM - Must add OutputType or create separate CLI project. Adding OutputType changes project role but is simpler |
| DatalistConverter requires character + situation strings | DatalistConverter.Convert() signature | MEDIUM - Must derive character/situation from file path (e.g., `1_美鈴/KOJO_K1_愛撫.ERB` -> character=美鈴, situation=K1_愛撫) |
| ErbParser parses entire file to flat AST (no function boundaries) | ErbParser.cs | MEDIUM - Must process all top-level PrintDataNode/DatalistNode from parsed AST without function-level grouping |
| TalentCsvLoader requires Talent.csv path | TalentCsvLoader constructor | LOW - Must be configurable via CLI argument or convention |
| 11 subdirectories under 口上/ with varying file counts per character | Game/ERB/口上/ directory structure | LOW - Recursive discovery handles this naturally |
| File encoding is UTF-8 | ErbParser.Parse() uses Encoding.UTF8 | LOW - Standard .NET file I/O handles this |
| File extension case-sensitivity | Directory.GetFiles("*.ERB") is case-insensitive on Windows (NTFS) but case-sensitive on Linux | LOW - ERA games are Windows-only. Document as Windows-targeted tool |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

Philosophy: "Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format"

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Automated migration" | Must have CLI entry point that can be invoked without manual intervention | AC#1, AC#2 |
| "117 ERB kojo files" | Must recursively discover all ERB files in a directory tree | AC#3, AC#5 |
| "batch mode" (Goal #1) | `--batch <directory>` argument must be accepted and processed | AC#2, AC#3 |
| "maintaining directory structure" (Goal #3) | Output YAML files must mirror input directory hierarchy | AC#5 |
| "Report conversion success/failure per file" (Goal #4) | Summary report with per-file pass/fail status must be printed | AC#6, AC#7 |
| "Process all ERB files" (Goal #2) | Conversion must not stop on individual file failure | AC#7 |
| "consistent YAML output structure" (Philosophy) | Output YAML must pass dialogue-schema.json schema validation | AC#16 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ErbToYaml builds as executable | build | dotnet build tools/ErbToYaml | succeeds | - | [x] |
| 2 | Program.cs accepts --batch argument | code | Grep(tools/ErbToYaml/Program.cs) | contains | "--batch" | [x] |
| 3 | BatchConverter discovers ERB files recursively | test | dotnet test tools/ErbToYaml.Tests --filter BatchConverter | succeeds | exit code 0 | [x] |
| 4 | FileConverter orchestrates single-file parse-convert-write | test | dotnet test tools/ErbToYaml.Tests --filter FileConverter | succeeds | exit code 0 | [x] |
| 5 | Output YAML preserves input directory structure | test | dotnet test tools/ErbToYaml.Tests --filter DirectoryStructure | succeeds | exit code 0 | [x] |
| 6 | Batch summary reports total, success, failure counts | test | dotnet test tools/ErbToYaml.Tests --filter SummaryReport | succeeds | exit code 0 | [x] |
| 7 | Failed file does not stop batch processing | test | dotnet test tools/ErbToYaml.Tests --filter ContinueOnError | succeeds | exit code 0 | [x] |
| 8 | ConversionResult record has Success, FilePath, Error parameters | code | Grep(tools/ErbToYaml/ConversionResult.cs) | contains | "bool Success" | [x] |
| 9 | ErbToYaml.csproj has OutputType Exe | file | Grep(tools/ErbToYaml/ErbToYaml.csproj) | contains | "Exe" | [x] |
| 10 | No technical debt markers in new source and test files | code | Grep(path="tools/ErbToYaml*/", pattern="TODO|FIXME|HACK", type=cs) | count_equals | 0 | [x] |
| 11 | Program.cs returns exit code 1 on missing --batch argument value | test | dotnet test tools/ErbToYaml.Tests --filter InvalidArguments | succeeds | exit code 0 | [x] |
| 12 | ~~Handoff feature-670.md created~~ (REMOVED: resolved in F634 via AC#15) | - | - | - | - | [-] |
| 13 | PrintDataConverter unit tests pass | test | dotnet test tools/ErbToYaml.Tests --filter PrintDataConverter | succeeds | exit code 0 | [x] |
| 14 | PathAnalyzer unit tests pass | test | dotnet test tools/ErbToYaml.Tests --filter PathAnalyzer | succeeds | exit code 0 | [x] |
| 15 | FileConverter preserves conditional structure wrapping PRINTDATA | test | dotnet test tools/ErbToYaml.Tests --filter ConditionalPreservation | succeeds | exit code 0 | [x] |
| 16 | FileConverter validates generated YAML against dialogue-schema.json | test | dotnet test tools/ErbToYaml.Tests --filter SchemaValidation | succeeds | exit code 0 | [x] |
| 17 | Handoff feature-671.md created for PrintDataNode Variant mapping | file | Glob(Game/agents/feature-671.md) | exists | - | [x] |

### AC Details

**AC#1: ErbToYaml builds as executable**
- Verifies the project compiles after adding OutputType Exe and new files
- Must build without errors including all new classes (Program, BatchConverter, FileConverter, ConversionResult)
- Ensures existing DatalistConverter and TalentCsvLoader remain compatible

**AC#2: Program.cs accepts --batch argument**
- CLI entry point must parse `--batch <directory>` argument
- Must also support single-file mode (no --batch flag, just a file path) for backward-compatible usage
- Should print usage/help on invalid arguments

**AC#3: BatchConverter discovers ERB files recursively**
- Must use `SearchOption.AllDirectories` or equivalent to find `*.ERB` files
- Test: Given a temp directory with nested subdirectories containing .ERB files, BatchConverter finds all of them
- Edge case: empty subdirectories should not cause errors

**AC#4: FileConverter orchestrates single-file parse-convert-write**
- Pipeline: read ERB file -> ErbParser.Parse() -> extract DatalistNode/PrintDataNode -> DatalistConverter.Convert() -> write YAML
- Must derive character name and situation from file path (e.g., `1_美鈴/KOJO_K1_愛撫.ERB` -> character=美鈴, situation=K1_愛撫)
- Test with a minimal ERB file containing at least one DATALIST block

**AC#5: Output YAML preserves input directory structure**
- Given input directory `口上/1_美鈴/KOJO_K1_愛撫.ERB`, output must be at `<output>/1_美鈴/KOJO_K1_愛撫.yaml`
- Subdirectory hierarchy must be created automatically if it does not exist
- Test: convert files in nested directories, verify output paths mirror input relative paths

**AC#6: Batch summary reports total, success, failure counts**
- After processing all files, print summary: "Total: N, Success: M, Failed: K"
- The BatchConverter must return a result object containing these counts
- **Counting unit**: ERB input files (not YAML output files). Total=number of ERB files processed. Success=ERB files where all YAML outputs succeeded. Failed=ERB files where any YAML output failed.
- Pattern follows KojoComparer.BatchProcessor.BatchReport (TotalTests, PassedTests, FailedTests, Failures list)

**AC#7: Failed file does not stop batch processing**
- If one ERB file fails to parse or convert, processing continues with remaining files
- Failed files are recorded in the result with error details
- Test: include one malformed ERB file among valid files, verify all valid files are still processed

**AC#8: ConversionResult model has Success, FilePath, Error parameters**
- Per-file result record enabling callers to inspect individual outcomes
- Record parameters: Success (bool), FilePath (string, input path), Error (string?, null on success, message on failure)
- Follows established pattern from KojoComparer for result reporting

**AC#9: ErbToYaml.csproj has OutputType Exe**
- The csproj must contain `<OutputType>Exe</OutputType>` to enable direct execution
- This is the simplest approach vs. creating a separate CLI project
- Must not break existing ErbToYaml.Tests project reference

**AC#10: No technical debt markers in new files**
- Verifies all new source files in ErbToYaml directory contain no TODO, FIXME, or HACK comments
- Ensures clean code delivery with no deferred implementation notes or temporary workarounds
- Uses count_equals matcher with 0 to verify no technical debt markers exist

**AC#11: Program.cs returns exit code 1 on missing --batch argument value**
- Negative test case to verify proper error handling for invalid CLI arguments
- Tests that `--batch` without directory argument returns non-zero exit code
- Implements ENGINE type requirement for positive AND negative test coverage

**AC#12: Handoff feature-670.md created**
- Verifies the handoff feature file exists for PrintDataConverter conditional structure preservation
- Satisfies Option A handoff protocol requirement for creation task verification
- File existence confirms deferred item has concrete tracked destination

**AC#13: PrintDataConverter unit tests pass**
- Verifies unit tests for PrintDataConverter class specifically
- Covers PrintDataNode to YAML conversion logic and conditional flattening
- Uses --filter PrintDataConverter to isolate these tests from other components

**AC#14: PathAnalyzer unit tests pass**
- Verifies unit tests for PathAnalyzer path extraction logic specifically
- Covers character and situation extraction from file path patterns
- **Error handling**: PathAnalyzer.Extract() throws ArgumentException when path does not match expected pattern (e.g., no numbered directory prefix, no KOJO_ filename prefix)
- Tests include both valid patterns and error cases for non-matching paths
- Uses --filter PathAnalyzer to isolate these tests from other components

**AC#15: FileConverter preserves conditional structure wrapping PRINTDATA**
- When ERB AST contains IF/ELSEIF/ELSE wrapping PRINTDATA blocks, FileConverter must preserve conditional structure in YAML output
- Each branch becomes a YAML branch object with condition metadata (same pattern as DatalistConverter ConditionalBranches)
- Test: Given ERB with `IF condition → PRINTDATA A / ELSE → PRINTDATA B / ENDIF`, output YAML has two branches with condition info
- 50/91 kojo files (55%) have this pattern, so this is critical for conversion correctness

**AC#16: FileConverter validates generated YAML against dialogue-schema.json**
- After generating YAML content, FileConverter calls schema validation before writing to disk
- Uses existing NJsonSchema infrastructure (already referenced in ErbToYaml.csproj)
- Schema-invalid YAML is treated as a conversion failure (ConversionResult with Success=false)
- Test: Generate YAML from valid ERB, verify schema validation passes; generate invalid YAML, verify failure is reported

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 9 | Add OutputType Exe to ErbToYaml.csproj | [x] |
| 2 | 8 | Create ConversionResult record with Success, FilePath, Error fields | [x] |
| 3 | 2 | Create Program.cs with CLI entry point supporting --batch and single-file modes, composition root DI wiring, exit code 1 on invalid arguments | [x] |
| 4 | 3 | Create BatchConverter with recursive ERB file discovery | [x] |
| 5 | 4 | Create FileConverter with parse-convert-write orchestration | [x] |
| 6 | 14 | Create PathAnalyzer to extract character and situation from file paths | [x] |
| 7 | 13 | Create PrintDataConverter to convert PrintDataNode to YAML | [x] |
| 8 | 5 | Implement directory structure preservation in BatchConverter | [x] |
| 9 | 6 | Create BatchReport model and implement summary reporting | [x] |
| 10 | 7 | Implement continue-on-error behavior in BatchConverter | [x] |
| 11 | 3 | Create unit tests for BatchConverter (recursive discovery, continue-on-error, directory structure) | [x] |
| 12 | 4 | Create unit tests for FileConverter (single-file orchestration) | [x] |
| 20 | 13 | Create unit tests for PrintDataConverter (PRINTDATA to YAML conversion) | [x] |
| 21 | 14 | Create unit tests for PathAnalyzer (character and situation extraction) | [x] |
| 13 | 5 | Create unit tests for DirectoryStructure preservation | [x] |
| 14 | 6 | Create unit tests for SummaryReport (batch counts and reporting) | [x] |
| 15 | 7 | Create unit tests for ContinueOnError behavior | [x] |
| 16 | 1 | Verify ErbToYaml builds as executable with all new files | [x] |
| 17 | 10 | Verify all new files have zero technical debt markers (TODO/FIXME/HACK) | [x] |
| 18 | 11 | Create unit tests for invalid CLI argument handling (negative tests) | [x] |
| ~~19~~ | ~~12~~ | ~~Create feature-670.md for PrintDataConverter conditional structure preservation~~ (REMOVED: resolved in F634 via AC#15) | [-] |
| 22 | 15 | Implement conditional structure preservation in FileConverter for IF-wrapped PRINTDATA blocks | [x] |
| 23 | 15 | Create unit and integration tests for ConditionalPreservation (IF-wrapped PRINTDATA to conditional YAML, including FileConverter→PrintDataConverter delegation) | [x] |
| 24 | 16 | Integrate schema validation into FileConverter after YAML generation | [x] |
| 25 | 16 | Create unit tests for SchemaValidation (valid and invalid YAML against dialogue-schema.json) | [x] |
| ~~26~~ | ~~15~~ | ~~Create integration test for FileConverter conditional PRINTDATA delegation to PrintDataConverter~~ (MERGED into T23) | [-] |
| 27 | 17 | Create feature-671.md for PrintDataNode Variant metadata mapping | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T2 | Technical Design | ConversionResult record, ErbToYaml.csproj update |
| 2 | implementer | sonnet | T3 | Technical Design | Program.cs with CLI argument parsing |
| 3 | implementer | sonnet | T4-T7 | Technical Design | BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter |
| 4 | implementer | sonnet | T8-T10 | Technical Design | Directory preservation, BatchReport, error handling |
| 5 | implementer | sonnet | T11-T15 | AC Details | Unit tests for all components by category |
| 6 | ac-tester | haiku | T16 | Build command | Build verification |
| 7 | implementer | sonnet | T17-T18 | Verification ACs | Technical debt verification and negative test implementation |
| 8 | implementer | sonnet | T22-T23 | Conditional preservation | FileConverter IF-wrapped PRINTDATA handling and tests |
| 9 | implementer | sonnet | T24-T25 | Schema validation | Schema validation integration and tests |

**Constraints** (from Technical Design):

1. ErbToYaml.csproj must add `<OutputType>Exe</OutputType>` to enable executable output
2. BatchConverter must use `Directory.GetFiles(directory, "*.ERB", SearchOption.AllDirectories)` for recursive discovery
3. FileConverter must derive character/situation from file paths using pattern `口上/1_美鈴/KOJO_K1_愛撫.ERB` → character=`美鈴`, situation=`K1_愛撫`
4. PrintDataConverter must use `PrintDataNode.GetDataForms()` helper from F633 to extract DataformNode list
5. BatchConverter computes output path using `Path.Combine(outputDir, Path.GetRelativePath(inputDir, erbFile))` and passes pre-computed directory to FileConverter
6. BatchConverter must wrap each FileConverter call in try-catch, create ConversionResult with Success=false on exception, and continue to next file
7. If ERB file contains multiple DATALIST/PRINTDATA blocks, FileConverter generates multiple YAML files with indexed suffixes (e.g., `K1_愛撫_0.yaml`, `K1_愛撫_1.yaml`)

**Pre-conditions**:

- F633 (PRINTDATA Parser Extension) is [DONE], providing `PrintDataNode` AST with `GetDataForms()` helper
- ErbParser project reference exists in ErbToYaml.csproj
- DatalistConverter exists and handles single DATALIST node conversion
- TalentCsvLoader exists for talent condition resolution
- YamlDotNet 16.2.1 and NJsonSchema 11.1.0 are referenced in ErbToYaml.csproj

**Success Criteria**:

- All 16 ACs pass (build succeeds, CLI accepts --batch, recursive discovery works, single-file orchestration works, directory structure preserved, summary reports ERB file counts, errors don't stop batch, ConversionResult has required parameters, OutputType is Exe, zero technical debt markers, invalid argument handling, PrintDataConverter tests, PathAnalyzer tests with error cases, conditional structure preservation, schema validation)
- `dotnet build tools/ErbToYaml` succeeds with exit code 0
- Unit tests pass for all new components
- No TODO/FIXME/HACK markers in new code

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

### File Structure

**New Files**:

| File | Purpose |
|------|---------|
| `tools/ErbToYaml/Program.cs` | CLI entry point with --batch and single-file argument parsing |
| `tools/ErbToYaml/IDatalistConverter.cs` | Interface for existing DatalistConverter (testability) |
| `tools/ErbToYaml/IBatchConverter.cs` | Batch converter interface for testability |
| `tools/ErbToYaml/BatchConverter.cs` | Directory traversal and batch orchestration |
| `tools/ErbToYaml/IFileConverter.cs` | File converter interface for testability |
| `tools/ErbToYaml/FileConverter.cs` | Single-file conversion pipeline orchestration |
| `tools/ErbToYaml/IPrintDataConverter.cs` | Print data converter interface for testability |
| `tools/ErbToYaml/PrintDataConverter.cs` | Converts PrintDataNode to YAML lines |
| `tools/ErbToYaml/IPathAnalyzer.cs` | Path analyzer interface for testability |
| `tools/ErbToYaml/PathAnalyzer.cs` | Extracts character/situation from file paths (with validation) |
| `tools/ErbToYaml/ConversionResult.cs` | Per-file result record (Success, FilePath, Error) |
| `tools/ErbToYaml/BatchReport.cs` | Batch summary model (Total, Success, Failed counted by ERB file, Failures list) |
| `tools/ErbToYaml.Tests/BatchConverterTests.cs` | Tests for batch conversion logic |
| `tools/ErbToYaml.Tests/FileConverterTests.cs` | Tests for single-file orchestration |
| `tools/ErbToYaml.Tests/PrintDataConverterTests.cs` | Tests for PrintData conversion |
| `tools/ErbToYaml.Tests/PathAnalyzerTests.cs` | Tests for path extraction logic |

**Modified Files**:

| File | Change |
|------|--------|
| `tools/ErbToYaml/ErbToYaml.csproj` | Add `<OutputType>Exe</OutputType>` to PropertyGroup |

### Implementation Details

#### Program.cs Signature

```csharp
// tools/ErbToYaml/Program.cs
using System;
using System.Threading.Tasks;

namespace ErbToYaml;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Parse arguments: --batch <dir> or single file mode
        // Call BatchConverter or single-file conversion
        // Print summary
        // Return exit code: 0 if all succeeded, 1 if any failed
    }
}
```

#### ConversionResult Record

```csharp
// tools/ErbToYaml/ConversionResult.cs
namespace ErbToYaml;

public record ConversionResult(
    bool Success,
    string FilePath,
    string? Error
);
```

#### BatchReport Class

```csharp
// tools/ErbToYaml/BatchReport.cs
using System.Collections.Generic;

namespace ErbToYaml;

public class BatchReport
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public List<ConversionResult> Failures { get; set; } = new();
}
```

#### BatchConverter Interface

```csharp
// tools/ErbToYaml/BatchConverter.cs
using System.Threading.Tasks;

namespace ErbToYaml;

public class BatchConverter : IBatchConverter
{
    private readonly IFileConverter _fileConverter;
    public BatchConverter(IFileConverter fileConverter) { _fileConverter = fileConverter; }

    public async Task<BatchReport> ConvertAsync(
        string inputDirectory,
        string outputDirectory)
    {
        // 1. Use Directory.GetFiles(inputDirectory, "*.ERB", SearchOption.AllDirectories)
        // 2. For each ERB file:
        //    - Compute output directory preserving relative path structure
        //    - Wrap _fileConverter.ConvertAsync in try-catch
        //    - On success: increment success count
        //    - On failure: create ConversionResult with Error, add to Failures list, increment failed count, CONTINUE
        // 3. Return BatchReport with Total, Success, Failed, Failures (counted by ERB file)
    }
}
```

#### FileConverter Interface

```csharp
// tools/ErbToYaml/FileConverter.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErbToYaml;

public class FileConverter
{
    public async Task<List<ConversionResult>> ConvertAsync(
        string erbFilePath,
        string outputDirectory)
    {
        // 1. Extract character/situation from erbFilePath using _pathAnalyzer.Extract()
        // 2. Read ERB file content with File.ReadAllText(erbFilePath, Encoding.UTF8)
        // 3. Parse with ErbParser.Parse(content)
        // 4. Traverse AST with conditional awareness:
        //    - Top-level DatalistNode → _datalistConverter.Convert()
        //    - Top-level PrintDataNode → _printDataConverter.Convert()
        //    - Top-level IfNode containing PRINTDATA/DATALIST children →
        //      Preserve conditional structure: each IF/ELSEIF/ELSE branch
        //      becomes a YAML branch with condition metadata
        //      (same pattern as DatalistConverter ConditionalBranches)
        // 5. Validate generated YAML via _datalistConverter.ValidateYaml()
        // 6. Use provided outputDirectory (computed by BatchConverter with relative path structure)
        // 7. If multiple nodes, generate indexed filenames (e.g., K1_愛撫_0.yaml, K1_愛撫_1.yaml)
        // 8. Write each YAML file to output path
        // 9. Return List<ConversionResult> (one per generated YAML file)
    }
}
```

#### PrintDataConverter Interface

```csharp
// tools/ErbToYaml/PrintDataConverter.cs
using ErbParser.AST;

namespace ErbToYaml;

public class PrintDataConverter
{
    public string Convert(
        PrintDataNode printData,
        string character,
        string situation)
    {
        // 1. Use printData.GetDataForms() to extract DataformNode list
        // 2. Convert each DataformNode to YAML line
        // 3. Return YAML string with character/situation metadata
        // Note: Conditionals wrapping PRINTDATA are handled by FileConverter, not here
    }
}
```

#### PathAnalyzer Interface

```csharp
// tools/ErbToYaml/PathAnalyzer.cs
namespace ErbToYaml;

public class PathAnalyzer : IPathAnalyzer
{
    public (string Character, string Situation) Extract(string erbFilePath)
    {
        // Pattern: 口上/1_美鈴/KOJO_K1_愛撫.ERB
        // 1. Extract directory name: "1_美鈴" -> "美鈴" (remove leading number and underscore)
        // 2. Extract filename: "KOJO_K1_愛撫.ERB" -> "K1_愛撫" (remove "KOJO_" prefix and ".ERB" extension)
        // Return (Character: "美鈴", Situation: "K1_愛撫")
        // Throws ArgumentException if path does not match expected pattern
    }
}
```

### Test Requirements

**BatchConverterTests.cs**:

- Test recursive discovery finds all ERB files in nested directories
- Test empty subdirectories don't cause errors
- Test continue-on-error: one malformed ERB file among valid files doesn't stop batch
- Test BatchReport counts are correct (Total, Success, Failed)
- Test directory structure preservation (input nested paths mirror output nested paths)

**FileConverterTests.cs**:

- Test single-file conversion with minimal ERB file containing one DATALIST block
- Test character/situation extraction from file path
- Test multiple DATALIST/PRINTDATA blocks generate multiple YAML files with indexed suffixes

**PrintDataConverterTests.cs**:

- Test conversion of simple PrintDataNode with single DATAFORM
- Test conversion of PrintDataNode with multiple DATAFORMs
- Test output YAML includes character and situation metadata

**PathAnalyzerTests.cs**:

- Test extraction from pattern `1_美鈴/KOJO_K1_愛撫.ERB` returns (character=美鈴, situation=K1_愛撫)
- Test extraction handles various numbered directory patterns
- Test extraction handles filenames with multiple underscores

### Error Message Format

**BatchConverter Failures**:

- Parse error: `"Failed to parse {filePath}: {exception message}"`
- Conversion error: `"Failed to convert {filePath}: {exception message}"`
- Write error: `"Failed to write YAML for {filePath}: {exception message}"`

**Program.cs Summary Format**:

```
Total: {report.Total}, Success: {report.Success}, Failed: {report.Failed}
```

If failures exist, print details:
```
Failed files:
  - {failure.FilePath}: {failure.Error}
  - ...
```

## Review Notes

- [resolved-applied] Phase1-Uncertain iter5: FileConverter interface resolved - BatchConverter pre-computes output paths using Path.GetRelativePath, passes computed outputDirectory to FileConverter. FileConverter interface simplified to handle single-file conversion without path computation concerns.
- [resolved-applied] Phase2-Maintainability iter10: No dependency injection - BatchConverter/FileConverter use concrete dependencies making unit testing impossible. Requires interfaces (IBatchConverter, IFileConverter, etc.) and constructor injection for testability.
- [resolved-applied] Phase2-Maintainability iter10: PrintDataConverter conditional flattening - Investigation revealed conditionals WRAP PrintData blocks (50/91 files affected), not nest inside. FileConverter must recognize IF nodes containing PRINTDATA and preserve conditional structure in YAML (like DatalistConverter's ConditionalBranches pattern). F670 handoff removed; handled in F634 directly.
- [resolved-applied] Phase2-Maintainability iter10: AC#10 scope too narrow - Only checks tools/ErbToYaml/, excludes tools/ErbToYaml.Tests/ where tech debt could hide.
- [resolved-applied] Phase2-Maintainability iter10: BatchReport counting ambiguity - Unclear if Total/Success/Failed count ERB files or YAML outputs when multiple outputs per file.
- [resolved-applied] Phase2-Maintainability iter10: Hardcoded ERB extension - Case sensitivity and edge cases not handled for file matching.
- [resolved-applied] Phase2-Maintainability iter10: PathAnalyzer single pattern assumption - No validation or error handling for files not matching pattern.
- [resolved-applied] Phase2-Maintainability iter10: Schema validation not integrated - FileConverter never calls ValidateYaml despite capability existing.
- [resolved-deferred] Phase2-Maintainability iter6: PrintDataNode.Variant handling - 9 variants (PRINTDATA, PRINTDATAL, PRINTDATAW, etc.) produce identical output. Variant affects display behavior (L=newline, W=wait). Deferred to F671 (PrintData Variant Metadata Mapping). F634 converts content only; display variant semantics are a separate concern requiring dialogue-schema.json extension.

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| ~~PrintDataConverter conditional structure preservation~~ | ~~Resolved in F634: FileConverter handles IF-wrapped PRINTDATA with conditional structure preservation (AC#15, T22/T23)~~ | ~~Feature~~ | ~~F670~~ | ~~T19~~ |
| PrintDataNode Variant metadata mapping | 9 PRINTDATA variants (L, W, K, D, etc.) affect display behavior. F634 converts content only; variant semantics require dialogue-schema.json extension | Feature | F671 | T27 |
| ac-static-verifier complex Method format | Tool cannot parse `Grep(path=..., pattern=..., type=cs)` format. F634 AC#10 required manual verification | Feature | F632 (F672 absorbed) | Phase 8 DEVIATION |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-27 | Phase 1 | Initialized: [REVIEWED] → [WIP] |
| 2026-01-27 | Phase 2 | Investigation complete: codebase explored, patterns identified |
| 2026-01-27 | Phase 3 | TDD RED: 36 tests created across 4 test files (all fail as expected) |
| 2026-01-27 | Phase 4 | Implementation: All core classes created. ErbParser fixed for PRINTDATA in IF branches. 44/44 tests GREEN, 72/72 ErbParser regression pass |
| 2026-01-27 | Phase 6 | Verification: All 16 ACs [x]. Build/code/file/test all PASS. 48 total tests (44 ErbToYaml + 4 InvalidArguments). 0 deviations |
| 2026-01-27 | Phase 7 | Post-Review: Quality READY, Doc Consistency READY, SSOT N/A |
| 2026-01-27 | DEVIATION | ac-static-verifier | code AC#10 | FAIL - tool cannot parse complex Method format `Grep(path=..., pattern=..., type=cs)` (known limitation in testing SKILL). Manually verified: 0 TODO/FIXME/HACK in tools/ErbToYaml*/ |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Some ERB files may have structures not yet supported by ErbParser | Medium | Medium | BatchConverter should catch ParseException per file and report as failure without stopping entire batch |
| Character/situation extraction from file paths may have edge cases | Low | Medium | Define clear naming convention mapping; test against all 11 directory names |
| Converting ErbToYaml to Exe may break existing test project references | Low | Low | Test project references ErbToYaml as library; adding OutputType=Exe should not break ProjectReference |
| PrintDataNode conversion logic not yet implemented in DatalistConverter | Medium | High | FileConverter must handle PrintDataNode->YAML conversion, possibly extending DatalistConverter or creating new converter |
| Output directory structure creation may fail on Windows path length limits | Low | Low | Use relative paths; 口上 subdirectory names are short |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Architecture**: Convert ErbToYaml from library-only to executable with CLI entry point, following KojoComparer reference pattern for batch processing.

**Pipeline**: `Program.cs` (CLI) → `BatchConverter` (orchestration) → `FileConverter` (per-file logic) → existing `DatalistConverter` (DATALIST conversion) + new `PrintDataConverter` (PRINTDATA conversion)

**Key Components**:

1. **Program.cs**: CLI entry point with argument parsing
   - `--batch <directory>` mode for batch processing
   - Single-file mode (backward-compatible): `dotnet run -- <erbFile> <outputFile>`
   - Prints usage on invalid arguments

2. **BatchConverter**: Directory traversal and batch orchestration
   - Uses `Directory.GetFiles(directory, "*.ERB", SearchOption.AllDirectories)` for recursive discovery (mirrors KojoComparer.BatchProcessor pattern)
   - Invokes FileConverter per file, catches exceptions per file
   - Aggregates ConversionResult[] and generates BatchReport summary
   - Preserves input directory structure in output

3. **FileConverter**: Single-file conversion orchestration
   - Pipeline: Read ERB → ErbParser.Parse() → Traverse AST with conditional awareness → Convert to YAML → Write output
   - Derives character/situation from file path using PathAnalyzer helper
   - Handles both DATALIST (via DatalistConverter) and PRINTDATA (via new PrintDataConverter)
   - **Conditional awareness**: Recognizes IF nodes wrapping PRINTDATA blocks and preserves conditional structure in YAML output. Method: `ConvertConditionalPrintData(IfNode ifNode, string character, string situation)` iterates branches, calls PrintDataConverter per branch's PRINTDATA content, wraps results with condition metadata matching dialogue-schema.json branch structure
   - Returns ConversionResult (Success, FilePath, Error)

4. **PrintDataConverter**: Converts PrintDataNode to YAML (new)
   - Uses `PrintDataNode.GetDataForms()` helper (from F633) to extract DataformNode list
   - Converts extracted DataForms to YAML lines array
   - Handles single PRINTDATA block only (no conditional logic)
   - Conditionals wrapping PRINTDATA blocks are handled by FileConverter at AST traversal level

5. **PathAnalyzer**: Extracts character/situation from file paths (new)
   - Pattern: `口上/1_美鈴/KOJO_K1_愛撫.ERB` → character=`美鈴`, situation=`K1_愛撫`
   - Handles numbered directories (e.g., `1_美鈴` → `美鈴`)
   - Extracts situation from filename (e.g., `KOJO_K1_愛撫.ERB` → `K1_愛撫`)

6. **ConversionResult**: Per-file result model
   - Properties: `bool Success`, `string FilePath`, `string? Error`
   - Used by BatchConverter for aggregation and reporting

7. **BatchReport**: Batch summary model (similar to KojoComparer.BatchProcessor.BatchReport)
   - Properties: `int Total`, `int Success`, `int Failed`, `List<ConversionResult> Failures`
   - Printed by Program.cs after batch completion

**Directory Structure Preservation**:
- Input: `<inputDir>/1_美鈴/KOJO_K1_愛撫.ERB`
- Output: `<outputDir>/1_美鈴/KOJO_K1_愛撫.yaml`
- Use `Path.GetRelativePath(inputDir, erbFile)` to compute relative path, apply to output directory

**Error Handling**:
- Per-file try-catch in BatchConverter
- Exceptions stored in ConversionResult.Error, batch continues
- Final exit code: 0 if all succeeded, 1 if any failed

**OutputType Change**:
- Add `<OutputType>Exe</OutputType>` to ErbToYaml.csproj
- Existing ProjectReference from ErbToYaml.Tests should remain functional (test projects can reference executable projects)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `<OutputType>Exe</OutputType>` to ErbToYaml.csproj, ensure dotnet build succeeds with all new files (Program.cs, BatchConverter.cs, FileConverter.cs, PrintDataConverter.cs, PathAnalyzer.cs, ConversionResult.cs, BatchReport.cs) |
| 2 | Program.cs Main() method parses args, checks for `--batch` flag using `args.Contains("--batch")` or similar pattern (mirrors KojoComparer.Program.ParseArguments) |
| 3 | BatchConverter.ConvertAsync() uses `Directory.GetFiles(directory, "*.ERB", SearchOption.AllDirectories)` to recursively discover ERB files. Test with temp directory containing nested subdirectories and verify all files are found |
| 4 | FileConverter.ConvertAsync() orchestrates: (1) File.ReadAllText(erbPath) (2) ErbParser.Parse() (3) Extract DatalistNode/PrintDataNode from AST (4) Convert each via DatalistConverter/PrintDataConverter (5) Write YAML to output path. Test with minimal ERB file containing one DATALIST block |
| 5 | BatchConverter computes output path using `Path.Combine(outputDir, Path.GetRelativePath(inputDir, erbFile))` and replaces `.ERB` extension with `.yaml`. Test verifies nested input directories create corresponding nested output directories |
| 6 | BatchReport class contains Total, Success, Failed counters. BatchConverter aggregates ConversionResult[] and populates BatchReport. Program.cs prints summary: `Console.WriteLine($"Total: {report.Total}, Success: {report.Success}, Failed: {report.Failed}")`. Test verifies counts are correct |
| 7 | BatchConverter wraps each FileConverter.ConvertAsync() call in try-catch. On exception, creates ConversionResult with Success=false and Error=exception message, then continues to next file. Test includes one malformed ERB file among valid files and verifies batch completes |
| 8 | ConversionResult record/class with properties: `bool Success { get; init; }`, `string FilePath { get; init; }`, `string? Error { get; init; }`. Grep verifies "Success" property exists |
| 9 | ErbToYaml.csproj PropertyGroup contains `<OutputType>Exe</OutputType>`. Grep verifies "Exe" string exists in csproj file |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Library vs Executable | (A) Keep library, create separate CLI project (B) Convert library to executable with OutputType=Exe | B | Simpler: avoids extra project file, csproj changes, and nuget package management. Test projects can reference executables via ProjectReference without issues |
| Batch pattern source | (A) Design from scratch (B) Follow KojoComparer BatchProcessor pattern | B | Proven pattern already in codebase. Reduces design risk and provides consistency across tools |
| Directory structure | (A) Flatten output (B) Preserve input hierarchy | B | Goal #3 explicitly requires "maintaining directory structure". Per-character subdirectories are meaningful for organization |
| PrintDataNode conversion | (A) Extend DatalistConverter (B) Create PrintDataConverter | B | Separation of concerns: PrintDataNode has no conditional branches, no talent conditions. Simpler logic in dedicated converter avoids polluting DatalistConverter |
| Character/situation extraction | (A) Pass as CLI arguments (B) Derive from file paths | B | Batch mode processes 117 files; manual specification is impractical. File path pattern `1_美鈴/KOJO_K1_愛撫.ERB` is consistent and parseable |
| Error handling | (A) Stop on first error (B) Continue on error | B | Goal #4 requires reporting per-file results. Batch processing is more useful if failures don't block other conversions |
| Output format | (A) One YAML per DATALIST/PRINTDATA (B) One YAML per ERB file with multiple sections | A | Existing DatalistConverter produces one YAML per DATALIST node. Consistent with F349/F351 pilot work. If ERB file has 3 DATALIST blocks, generate 3 YAML files with suffix (e.g., K1_愛撫_0.yaml, K1_愛撫_1.yaml) |

### Interfaces / Data Structures

```csharp
// ConversionResult.cs
public record ConversionResult(
    bool Success,
    string FilePath,
    string? Error
);

// BatchReport.cs
public class BatchReport
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public List<ConversionResult> Failures { get; set; } = new();
}

// IDatalistConverter.cs (interface for existing DatalistConverter)
public interface IDatalistConverter
{
    string Convert(DatalistNode datalist, string character, string situation);
    void ValidateYaml(string yaml);  // Throws SchemaValidationException on failure; schema loaded in constructor. No-op if constructor used without schemaPath.
}

// IBatchConverter.cs
public interface IBatchConverter
{
    Task<BatchReport> ConvertAsync(
        string inputDirectory,
        string outputDirectory);
}

// BatchConverter.cs
public class BatchConverter : IBatchConverter
{
    private readonly IFileConverter _fileConverter;
    public BatchConverter(IFileConverter fileConverter) { _fileConverter = fileConverter; }
    public async Task<BatchReport> ConvertAsync(
        string inputDirectory,
        string outputDirectory);
    // Dependencies (talentCsvPath, schemaPath) are wired via DI in Program.cs
    // and injected into IFileConverter/IDatalistConverter constructors
}

// IFileConverter.cs
public interface IFileConverter
{
    Task<List<ConversionResult>> ConvertAsync(
        string erbFilePath,
        string outputDirectory);
}

// FileConverter.cs
public class FileConverter : IFileConverter
{
    private readonly IPathAnalyzer _pathAnalyzer;
    private readonly IPrintDataConverter _printDataConverter;
    private readonly IDatalistConverter _datalistConverter;
    public FileConverter(
        IPathAnalyzer pathAnalyzer,
        IPrintDataConverter printDataConverter,
        IDatalistConverter datalistConverter)
    {
        _pathAnalyzer = pathAnalyzer;
        _printDataConverter = printDataConverter;
        _datalistConverter = datalistConverter;
    }
    // Schema validation delegated to IDatalistConverter.ValidateYaml()
    public async Task<List<ConversionResult>> ConvertAsync(
        string erbFilePath,
        string outputDirectory);
}

// IPrintDataConverter.cs
public interface IPrintDataConverter
{
    string Convert(PrintDataNode printData, string character, string situation);
}

// PrintDataConverter.cs
public class PrintDataConverter : IPrintDataConverter
{
    public string Convert(
        PrintDataNode printData,
        string character,
        string situation);
}

// IPathAnalyzer.cs
public interface IPathAnalyzer
{
    (string Character, string Situation) Extract(string erbFilePath);
}

// PathAnalyzer.cs
public class PathAnalyzer : IPathAnalyzer
{
    public (string Character, string Situation) Extract(string erbFilePath);
    // Throws ArgumentException if path does not match expected pattern
}

// Program.cs Composition Root (DI wiring)
// All dependencies constructed here:
//   var datalistConverter = new DatalistConverter(talentCsvPath, schemaPath);
//     // DatalistConverter internally creates TalentCsvLoader and TalentConditionParser
//   var pathAnalyzer = new PathAnalyzer();
//   var printDataConverter = new PrintDataConverter();
//   var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter);
//   var batchConverter = new BatchConverter(fileConverter);

// Program.cs Main signature
static async Task<int> Main(string[] args)
```

**Note on multiple outputs per file**: If an ERB file contains multiple DATALIST/PRINTDATA blocks, FileConverter generates multiple YAML files with indexed suffixes (e.g., `K1_愛撫_0.yaml`, `K1_愛撫_1.yaml`). This is why FileConverter.ConvertAsync returns `List<ConversionResult>` instead of a single result.
