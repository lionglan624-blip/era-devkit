# Feature 640: Remilia Kojo Conversion

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

## Created: 2026-01-27

---

## Summary

Convert Remilia (5_レミリア) kojo ERB files to YAML format with equivalence verification.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

10 ERB files in Game/ERB/口上/5_レミリア/ need conversion to YAML for the new kojo engine. PathAnalyzer FallbackPattern now supports all file naming patterns.

### Goal (What to Achieve)

1. Convert all 10 Remilia kojo files using batch converter (KOJO + non-KOJO via FallbackPattern)
2. Track equivalence verification requirement for downstream F644
3. Validate YAML against schema
4. Zero technical debt (no TODO/FIXME/HACK)

---

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Line 4272
- [feature-633.md](feature-633.md) - F633 PRINTDATA Parser Extension [DONE]
- [feature-634.md](feature-634.md) - F634 Batch Conversion Tool (Predecessor, [DONE])
- [feature-635.md](feature-635.md) - F635 Conversion Parallelization (Related)
- [feature-644.md](feature-644.md) - F644 Equivalence Testing Framework (Downstream)
- [feature-671.md](feature-671.md) - F671 PrintData Variant Metadata Mapping (Related)
- [feature-636.md](feature-636.md) - F636 Meiling Kojo Conversion (Sibling)
- [feature-637.md](feature-637.md) - F637 Koakuma Kojo Conversion (Sibling)
- [feature-638.md](feature-638.md) - F638 Patchouli Kojo Conversion (Sibling)
- [feature-639.md](feature-639.md) - F639 Sakuya Kojo Conversion (Sibling)
- [feature-641.md](feature-641.md) - F641 Flandre Kojo Conversion (Sibling)
- [feature-642.md](feature-642.md) - F642 Secondary Characters Kojo Conversion (Sibling)
- [feature-643.md](feature-643.md) - F643 Generic Kojo Conversion (Sibling)
- [feature-645.md](feature-645.md) - F645 Related Feature
- [feature-648.md](feature-648.md) - F648 PathAnalyzer Pattern Extension [CANCELLED] (superseded by F639 FallbackPattern)
- [feature-649.md](feature-649.md) - F649 BatchConverter Code Duplication Refactoring (Follow-up)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 10 ERB files in Game/ERB/口上/5_レミリア/ exist only as ERB and need YAML equivalents for the new kojo engine
2. Why: Phase 19 requires automated migration of all 117 ERB kojo files to YAML, and Remilia's 10 files are one per-character batch
3. Why: The kojo engine is transitioning from ERB to YAML format for maintainability, schema validation, and structured dialogue management
4. Why: ERB files lack schema enforcement, structured conditions, and are difficult to validate statically -- YAML with dialogue-schema.json provides all three
5. Why: The root cause is that Remilia dialogue content exists solely in unstructured ERB format without structured YAML representation, blocking schema-validated dialogue delivery

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 10 Remilia ERB files have no YAML equivalents | No automated conversion has been executed for 5_レミリア directory |
| Remilia dialogue cannot benefit from schema validation | ERB format lacks structured metadata (character, situation, conditions) that YAML+schema provides |

### Conclusion

The root cause is straightforward: the F634 batch converter tool is [DONE] and ready, but has not yet been executed against the 5_レミリア directory. This is a conversion execution task, not a tooling problem.

**Critical finding**: Of the 10 ERB files in 5_レミリア/, only 6 follow the KOJO_ prefix pattern that PathAnalyzer supports:
- `KOJO_K5_EVENT.ERB` (14,908 bytes)
- `KOJO_K5_会話親密.ERB` (168,502 bytes)
- `KOJO_K5_口挿入.ERB` (182,659 bytes)
- `KOJO_K5_愛撫.ERB` (254,088 bytes)
- `KOJO_K5_挿入.ERB` (153,518 bytes)
- `KOJO_K5_日常.ERB` (3,658 bytes)

The remaining 4 files use non-KOJO naming patterns:
- `NTR口上.ERB` (387,321 bytes) -- largest file
- `NTR口上_お持ち帰り.ERB` (91,675 bytes)
- `SexHara休憩中口上.ERB` (44,019 bytes)
- `WC系口上.ERB` (77,157 bytes)

PathAnalyzer now supports a FallbackPattern (`(?:^|[\\/])(\d+)_([^\\/]+)[\\/](.+)\.(?:ERB|erb)$`) that handles non-KOJO files. All 10 files should be convertible with the updated tooling. The original assumption that non-KOJO files would fail is no longer valid.

**Note**: `KOJO_K5_愛撫.ERB.bak` also exists (258,464 bytes) but is not an ERB file and will be ignored by `*.ERB` pattern matching.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Predecessor (batch converter) | Provides BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter. All infrastructure for conversion |
| F633 | [DONE] | Indirect predecessor | PRINTDATA parser extension. PrintDataNode AST with GetDataForms() used by PrintDataConverter |
| F635 | [DONE] | Related (parallelization) | Optional performance improvement. Not required for F640 but would speed up execution |
| F636 | [DONE] | Sibling (Meiling conversion) | Same pattern: per-character batch conversion. 11 files |
| F637 | [DONE] | Sibling (Koakuma conversion) | Same pattern. Different character |
| F638 | [WIP] | Sibling (Patchouli conversion) | Same pattern. Different character |
| F639 | [WIP] | Sibling (Sakuya conversion) | Same pattern. 15 files (largest character batch) |
| F641 | [DONE] | Sibling (Flandre conversion) | Same pattern. Different character |
| F642 | [DONE] | Sibling (Secondary Characters conversion) | Same pattern but multiple characters |
| F643 | [DRAFT] | Sibling (Generic Kojo conversion) | Same pattern for 汎用 directory |
| F644 | [DRAFT] | Downstream (equivalence testing) | Depends on F636-F643 outputs to verify ERB=YAML equivalence |
| F649 | [DONE] | Downstream (refactoring) | BatchConverter.cs code duplication elimination |
| F671 | [DRAFT] | Related (variant metadata) | PrintData variant semantics (PRINTDATAL/W/K/D) not yet mapped. F640 converts content only |

### Pattern Analysis

All per-character conversion features (F636-F643) follow an identical pattern: run F634 batch converter against a character's directory, verify output with KojoComparer, validate YAML against schema. The key differentiator is the **non-KOJO file naming problem**: each character directory contains both KOJO-prefixed files (supported by PathAnalyzer) and non-KOJO files (NTR口上, SexHara, WC系) that will fail PathAnalyzer extraction.

This is a **systemic issue** affecting all per-character conversions (F636-F643), not unique to F640. The non-KOJO files represent a significant portion of content (e.g., NTR口上.ERB at 387KB is the largest file in 5_レミリア).

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All 10 files can be converted. PathAnalyzer FallbackPattern handles non-KOJO patterns. |
| Scope is realistic | YES | Batch converter execution is straightforward. KojoComparer exists. Schema validation exists. The scope question is whether to include non-KOJO files |
| No blocking constraints | YES | F634 is [DONE]. All tooling ready. Non-KOJO file handling is a scope decision, not a blocker |

**Verdict**: FEASIBLE

The feature is feasible with scope clarification needed: either (a) scope to 6 KOJO-prefixed files only and defer non-KOJO files to a follow-up feature addressing PathAnalyzer limitations, or (b) expand scope to include PathAnalyzer enhancement for non-KOJO patterns. Option (a) is recommended to maintain feature scope discipline and parallelism with sibling features.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool -- provides BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter, Program.cs |
| Related | F635 | [PROPOSED] | Conversion Parallelization -- optional performance improvement, not blocking |
| Related | F636-F639, F641-F643 | [DRAFT] | Sibling per-character conversions -- independent, can run in parallel |
| Successor | F644 | [DRAFT] | Equivalence Testing Framework -- will verify F640 outputs |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| tools/ErbToYaml (batch converter) | Build-time | Low | F634 [DONE], all components exist and tested |
| tools/KojoComparer | Build-time | Low | Exists with BatchProcessor.ProcessAsync() for equivalence testing |
| tools/YamlSchemaGen/dialogue-schema.json | Runtime | Low | Schema exists, used by DatalistConverter.ValidateYaml() |
| Game/ERB/口上/5_レミリア/ | Runtime | Low | 10 ERB files verified to exist (6 KOJO + 4 non-KOJO) |
| eraTW Talent.csv | Runtime | Low | Required by TalentCsvLoader for condition resolution |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F644 Equivalence Testing Framework | MEDIUM | Will verify converted YAML output matches original ERB |
| F645 Kojo Quality Validator | LOW | Quality validation of converted YAML |
| Game/ERB/口上/5_レミリア/ YAML output directory | HIGH | New YAML files will be produced alongside existing ERB files |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| .tmp/f640-output/5_レミリア/KOJO_K5_EVENT.yaml | Create | Converted from KOJO_K5_EVENT.ERB (14KB) |
| .tmp/f640-output/5_レミリア/KOJO_K5_会話親密.yaml | Create | Converted from KOJO_K5_会話親密.ERB (168KB) |
| .tmp/f640-output/5_レミリア/KOJO_K5_口挿入.yaml | Create | Converted from KOJO_K5_口挿入.ERB (182KB) |
| .tmp/f640-output/5_レミリア/KOJO_K5_愛撫.yaml | Create | Converted from KOJO_K5_愛撫.ERB (254KB) |
| .tmp/f640-output/5_レミリア/KOJO_K5_挿入.yaml | Create | Converted from KOJO_K5_挿入.ERB (153KB) |
| .tmp/f640-output/5_レミリア/KOJO_K5_日常.yaml | Create | Converted from KOJO_K5_日常.ERB (3KB) |
| Game/agents/feature-648.md | Create | [DRAFT] feature for PathAnalyzer non-KOJO pattern support |
| Game/agents/feature-649.md | Create | [DRAFT] feature for BatchConverter code duplication refactoring |

**Note**: 4 non-KOJO files (NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB) will be reported as conversion failures by BatchConverter due to PathAnalyzer pattern mismatch. Output YAML directory is typically separate from ERB source directory (e.g., `<outputDir>/5_レミリア/`).

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| PathAnalyzer supports KOJO_ prefix pattern (PathPattern) and non-KOJO files (FallbackPattern) | PathAnalyzer.cs regex `(\d+)_([^\\/]+)[\\/]KOJO_(.+)\.(?:ERB\|erb)$` plus FallbackPattern | LOW - All 10 files can be converted |
| Large file sizes (up to 387KB for NTR口上.ERB) | Game/ERB/口上/5_レミリア/ file sizes | MEDIUM - Large ERB files may have complex AST structures. ErbParser and converters need to handle them without timeout or memory issues |
| PrintData variant semantics not mapped | F671 [DRAFT] | LOW - F640 converts content only; PRINTDATA/PRINTDATAL/PRINTDATAW variants produce same YAML content. Display behavior mapping deferred to F671 |
| KOJO_K5_愛撫.ERB is largest KOJO file (254KB) | File inspection | LOW - May contain many DATALIST/PRINTDATA blocks generating multiple YAML output files with indexed suffixes |
| BatchConverter counts by ERB file, not YAML output | F634 BatchReport design | LOW - A single ERB file producing 3 YAML files counts as 1 success if all 3 pass schema validation |
| Condition parsing depends on TalentCsvLoader | FileConverter conditional branch handling | LOW - TalentCsvLoader is optional. Missing talent names produce empty conditions (schema-valid) |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Non-KOJO files may have unsupported AST structures | Low | Medium | BatchConverter continue-on-error ensures partial success. Inspect failure details per file |
| Large ERB files (254KB 愛撫, 168KB 会話親密) may have unsupported AST structures | Medium | Medium | BatchConverter continue-on-error ensures partial success. Inspect failure details per file |
| ErbParser may not handle all ERB constructs in Remilia files | Medium | Medium | F634 AC#7 ensures batch continues on parse failures. Manual review of failed files |
| Schema validation rejects valid conversion output | Low | Medium | Review dialogue-schema.json coverage. F634 AC#16 already validates schema integration |
| KojoComparer equivalence test reveals conversion discrepancies | Medium | Low | Expected during migration. Discrepancies documented and tracked for manual review |
| KOJO_K5_EVENT.ERB may have non-standard structure (event kojo vs standard kojo) | Medium | Low | EVENT files may use different patterns. BatchConverter handles gracefully via continue-on-error |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Automated migration" (Philosophy) | Batch converter must execute without manual intervention | AC#1 |
| "Convert 10 Remilia kojo files" (Goal #1, all 10 files via FallbackPattern) | All 10 ERB files produce YAML output files | AC#2 |
| "Verify equivalence with KojoComparer" (Goal #2, deferred) | Track equivalence verification requirement | Deferred to F644 |
| "Validate YAML against schema" (Goal #3) | All YAML output files must pass dialogue-schema.json validation | AC#4 |
| "Zero technical debt" (Goal #4) | No TODO/FIXME/HACK in generated YAML output | AC#5 |
| 4 non-KOJO files are out of scope (Root Cause) | BatchConverter reports expected failures for non-KOJO files | AC#6 |
| Conversion produces correct character metadata | YAML files contain correct character (レミリア) and situation metadata | AC#7 |
| No regression in existing tools | ErbToYaml and KojoComparer projects build successfully | AC#8 |
| PathAnalyzer enhancement properly tracked (TBD Prohibition) | F648 feature file must be created for non-KOJO pattern support | AC#9 |
| BatchConverter refactoring properly tracked (TBD Prohibition) | F649 feature file must be created for code duplication elimination | AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Batch converter executes on 5_レミリア directory (exit code 0 expected, all files convertible with FallbackPattern) | exit_code | `dotnet run --project tools/ErbToYaml -- --batch "Game/ERB/口上/5_レミリア" ".tmp/f640-output" --talent "Game/CSV/Talent.csv" --schema "tools/YamlSchemaGen/dialogue-schema.json"` | succeeds | - | [x] |
| 2 | At least 10 YAML files created from all ERBs (KOJO + non-KOJO via FallbackPattern) | file | Glob(`.tmp/f640-output/**/*.yaml`) | gte | 10 | [x] |
| 4 | All YAML files pass schema validation | exit_code | `dotnet run --project tools/YamlValidator -- --schema tools/YamlSchemaGen/dialogue-schema.json --validate-all .tmp/f640-output` | succeeds | - | [x] |
| 5a | No TODO markers in output | file | Grep(`.tmp/f640-output/`) | not_contains | `TODO` | [x] |
| 5b | No FIXME markers in output | file | Grep(`.tmp/f640-output/`) | not_contains | `FIXME` | [x] |
| 5c | No HACK markers in output | file | Grep(`.tmp/f640-output/`) | not_contains | `HACK` | [x] |
| 6 | Non-KOJO files successfully converted via FallbackPattern | output | Same as AC#1 (check stdout) | not_contains | `FAIL` | [x] |
| 7 | YAML metadata contains correct character name | file | Grep(`.tmp/f640-output/**/*.yaml`) | contains | `レミリア` | [x] |
| 8 | Tool projects build successfully | build | `dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer` | succeeds | - | [x] |
| 9 | F648 feature file exists for PathAnalyzer enhancement | file | `Glob("Game/agents/feature-648.md")` | exists | - | [x] |
| 10 | F649 feature file exists for BatchConverter refactoring | file | `Glob("Game/agents/feature-649.md")` | exists | - | [x] |

**Note**: AC#3 removed: deferred to F644 (see 残課題).

### AC Details

**AC#1: Batch converter executes on 5_レミリア directory**
- Verifies F634 batch converter can process the Remilia character directory end-to-end
- The `--batch` mode processes all `*.ERB` files in the specified directory
- Exit code 1 expected because non-KOJO files produce failures (report.Failed > 0)
- Output directory `.tmp/f640-output` keeps conversion results separate from source
- **Note**: AC#1 (exit code) and AC#6 (stdout) verify different aspects of the SAME execution
- **Implementation Context**: ac-tester phases both EXECUTE the commands AND verify the results per the AC Method column

**AC#2: At least 10 YAML files created from all ERBs**
- At least 10 files expected from all ERB files: 6 KOJO-prefixed (KOJO_K5_EVENT, KOJO_K5_会話親密, KOJO_K5_口挿入, KOJO_K5_愛撫, KOJO_K5_挿入, KOJO_K5_日常) + 4 non-KOJO files (NTR口上, NTR口上_お持ち帰り, SexHara休憩中口上, WC系口上) via FallbackPattern
- Note: AC accepts both 1:1 ERB-to-YAML mapping and indexed outputs for multi-DATALIST files (e.g., KOJO_K5_愛撫_001.yaml, _002.yaml)
- Edge case: KOJO_K5_愛撫.ERB.bak must NOT produce output (not matching `*.ERB` pattern)

**AC#3: KojoComparer equivalence passes for converted files (REMOVED)**
- Deferred to F644 Equivalence Testing Framework (KojoComparer lacks CLI batch mode)
- Single-file comparison available but not practical for 6 KOJO files
- AC#3 removed to avoid placeholder test; equivalence verification tracked in 残課題

**AC#4: All YAML files pass schema validation**
- Uses dialogue-schema.json from YamlSchemaGen
- Validates structure: character, situation, conditions, dialogue entries
- Ensures generated YAML is consumable by the kojo engine

**AC#5: No technical debt markers in output**
- Conversion should produce clean YAML without placeholder markers
- Searches for `TODO`, `FIXME`, `HACK` patterns in all output files

**AC#6: Non-KOJO files successfully converted via FallbackPattern**
- Verifies all 10 files convert successfully via FallbackPattern (KOJO_ via PathPattern, non-KOJO via FallbackPattern). No FAIL entries expected in stdout. This confirms PathAnalyzer enhancement is working correctly.

**AC#7: YAML metadata contains correct character name**
- PathAnalyzer extracts character name from directory path `5_レミリア`
- All YAML files should have `character: レミリア` (or equivalent metadata field)
- Verifies correct character-to-file mapping

**AC#8: Tool projects build successfully**
- Ensures no build regressions introduced during conversion execution
- Both ErbToYaml (converter) and KojoComparer (verifier) must compile cleanly

**AC#9: F648 feature file created for PathAnalyzer enhancement**
- Verifies creation of follow-up feature for PathAnalyzer non-KOJO pattern support
- File existence confirms handoff task completion and proper TBD Prohibition compliance

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

F640 executes the F634 batch converter against the 5_レミリア directory with a three-phase verification strategy: conversion, equivalence, and schema validation.

**Phase 1: Batch Conversion**
Execute `tools/ErbToYaml` batch converter on `Game/ERB/口上/5_レミリア` directory:
- Input: All `*.ERB` files in 5_レミリア directory (10 files total)
- Output: `.tmp/f640-output/` directory containing YAML files
- Expected: 6 KOJO-prefixed files produce YAML output, 4 non-KOJO files fail gracefully

**Phase 2: Equivalence Verification**
Deferred to F644 Equivalence Testing Framework (KojoComparer lacks CLI batch mode):
- Input: Original ERB files from `Game/ERB/口上/5_レミリア` + converted YAML from `.tmp/f640-output`
- Output: Placeholder test until F644 provides batch comparison capability
- Expected: F644 will implement batch equivalence verification

**Phase 3: Schema Validation**
Execute `tools/YamlValidator` against `dialogue-schema.json`:
- Input: All YAML files from `.tmp/f640-output`
- Output: Validation report with schema violations (if any)
- Expected: Zero schema violations

**Phase 4: Quality Verification**
Grep all output YAML for technical debt markers and verify character metadata:
- Search for TODO/FIXME/HACK patterns (AC#5)
- Verify character name "レミリア" appears in metadata (AC#7)
- Review batch report for expected non-KOJO failures (AC#6)

**Rationale**:
- Uses existing F634 infrastructure (no new tools needed)
- Three-phase verification ensures conversion correctness at multiple levels
- `.tmp/` output keeps converted files separate from source until verified
- Continue-on-error behavior (F634 AC#7) allows partial success with clear failure reporting

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Execute `dotnet run --project tools/ErbToYaml -- --batch "Game/ERB/口上/5_レミリア" .tmp/f640-output`. BatchConverter processes all ERB files, returns exit code 1 due to expected non-KOJO failures |
| 2 | Use Glob to count YAML files in `.tmp/f640-output/**/*.yaml`. Expect at least 10 outputs from all ERB files (allows indexed outputs for multi-DATALIST files) |
| 4 | Execute `dotnet run --project tools/YamlValidator -- --schema tools/YamlSchemaGen/dialogue-schema.json --validate-all .tmp/f640-output`. YamlValidator verifies all YAML files conform to dialogue-schema.json structure |
| 5 | Grep `.tmp/f640-output/**/*.yaml` with pattern `TODO\|FIXME\|HACK` using `not_contains` matcher. Conversion should produce clean YAML without placeholder markers |
| 6 | Check AC#1 command stdout for "NTR口上" string. BatchConverter will report PathAnalyzer ArgumentException for 4 non-KOJO files (NTR口上, NTR口上_お持ち帰り, SexHara, WC系), confirming scope boundary |
| 7 | Grep all YAML files for "レミリア" character name. PathAnalyzer extracts character from directory path `5_レミリア` and embeds in YAML metadata |
| 8 | Execute `dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer` before and after conversion. Verifies no build regressions introduced |
| 9 | Create Game/agents/feature-648.md as [DRAFT] feature for PathAnalyzer non-KOJO pattern support. File must exist after /run completion |
| 10 | Create Game/agents/feature-649.md as [DRAFT] feature for BatchConverter code duplication refactoring. File must exist after /run completion |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Scope (KOJO vs all files) | A) All 10 files (PathAnalyzer FallbackPattern support), B) 6 KOJO files only, C) KOJO files first then manual non-KOJO | A) All 10 files | PathAnalyzer FallbackPattern now supports non-KOJO files. All files in 5_レミリア are convertible. |
| Output directory | A) Same directory as ERB (in-place), B) Separate `.tmp/` directory, C) Version-controlled `Game/ERB/口上_YAML/` | B) Separate `.tmp/` directory | Keeps conversion output isolated until verified. Prevents mixing unverified YAML with source ERB. Easy cleanup if conversion fails. Production deployment can move YAML to final location after verification |
| Validation strategy | A) Schema only, B) Equivalence only, C) Both schema + equivalence | C) Both schema + equivalence | Schema validation ensures YAML is structurally correct. Equivalence testing ensures YAML is semantically identical to ERB. Both are necessary: schema validates consumability, equivalence validates correctness |
| Batch report failure handling | A) Fail entire batch on any file error, B) Continue on error with detailed report | B) Continue on error with detailed report | F634 AC#7 implements continue-on-error. Allows partial success if any files fail AST parsing. BatchReport.FailureDetails provides per-file error tracking |
| Large file handling | A) Skip large files (e.g., 254KB 愛撫), B) Process all with timeout protection, C) Process all without special handling | C) Process all without special handling | F634 tools are designed to handle AST complexity. 254KB is not excessively large for modern parsers. If timeout occurs, it will be caught and reported via continue-on-error. No premature optimization needed |
| YamlValidator invocation | A) Validate during conversion (integrated), B) Validate after conversion (separate step) | B) Validate after conversion (separate step) | Separation of concerns: ErbToYaml focuses on conversion correctness, YamlValidator focuses on schema compliance. Allows independent tool evolution. Easier debugging when failures occur |

### Data Structures

No new data structures required. F640 uses existing F634 components:

**BatchConverter** (from F634):
```csharp
public class BatchConverter
{
    public async Task<BatchReport> ConvertAsync(string inputDirectory, string outputDirectory, BatchOptions? options = null)
    // Processes all *.ERB files in inputDirectory, writes YAML to outputDirectory
    // Returns BatchReport with success/failure counts and details
}

public class BatchReport
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public List<ConversionResult> Failures { get; set; }
}
```

**PathAnalyzer** (from F634):
```csharp
public class PathAnalyzer : IPathAnalyzer
{
    private static readonly Regex KojoFilePattern =
        new Regex(@"(?:^|[\\/])(\d+)_([^\\/]+)[\\/]KOJO_(.+)\.(?:ERB|erb)$");

    public (int charId, string charName, string situation)
        ExtractMetadata(string path)
    // Throws ArgumentException for non-KOJO files (expected for 4 files)
}
```

**FileConverter** (from F634):
```csharp
public class FileConverter
{
    public async Task<ConversionResult> ConvertFileAsync(
        string erbPath, string outputDir, TalentCsvLoader talentLoader)
    // Parses ERB → AST → YAML, validates against schema
}
```

### Implementation Notes

**Non-KOJO File Handling**:
All 10 files are handled by PathAnalyzer: 6 KOJO-prefixed files match PathPattern, 4 non-KOJO files match FallbackPattern. No ArgumentException expected.

**Indexed YAML Output**:
If a single ERB file contains multiple DATALIST blocks, FileConverter produces multiple YAML files with indexed suffixes (e.g., `KOJO_K5_愛撫_001.yaml`, `KOJO_K5_愛撫_002.yaml`). AC#2 accepts at least 10 YAML outputs (1:1 mapping or indexed outputs). BatchConverter counts by ERB file (1 success), not YAML file count (potentially >1 per ERB).

**TalentCsvLoader Dependency**:
FileConverter uses TalentCsvLoader to resolve condition metadata (e.g., `TALENT:33` → talent name). If talent CSV is unavailable or incomplete, conditions remain as raw talent IDs in YAML (still schema-valid). This is acceptable for F640; complete metadata mapping is deferred to downstream features.

**Error Recovery**:
F634 BatchConverter implements continue-on-error (AC#7). If a KOJO file fails conversion due to unsupported AST structure, the batch continues processing remaining files. Failures in BatchReport provides per-file error tracking for debugging.

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Execute batch converter on 5_レミリア directory and produce YAML output in .tmp/f640-output | [x] |
| 2 | 2 | Verify at least 10 YAML files created from all ERBs (KOJO + non-KOJO via FallbackPattern) | [x] |
| 4 | 4 | Run schema validation on output YAML | [x] |
| 5a | 5a | Verify no TODO markers in output | [x] |
| 5b | 5b | Verify no FIXME markers in output | [x] |
| 5c | 5c | Verify no HACK markers in output | [x] |
| 6 | 6 | Verify non-KOJO files reported as failures | [x] |
| 7 | 7 | Verify correct character metadata in YAML | [x] |
| 8 | 8 | Build verification for tool projects | [x] |
| 9 | 9 | Verify F648 feature file exists for PathAnalyzer non-KOJO pattern support | [x] |
| 10 | 10 | Verify F649 feature file exists for BatchConverter code duplication refactoring | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | ac-tester | haiku | T1 | AC#1 batch converter command | Batch conversion execution with .tmp/f640-output directory |
| 2 | ac-tester | haiku | T2 | AC#2 YAML file count verification | Glob count of output YAML files |
| 3 | ac-tester | haiku | T4 | AC#4 YamlValidator command | Schema validation report |
| 4 | ac-tester | haiku | T5a, T5b, T5c, T6, T7 | AC#5-7 quality verification commands | Grep results for tech debt, failures, metadata. Note: T6 reuses command output from Phase 1 |
| 5 | ac-tester | haiku | T8 | AC#8 build commands | Build verification results |
| 6 | ac-tester | haiku | T9 | AC#9 F648 existence verification | F648 feature file already exists as [DRAFT] |
| 7 | ac-tester | haiku | T10 | AC#10 F649 existence verification | F649 feature file already exists as [DRAFT] |

**Constraints** (from Technical Design):

1. Output directory must be `.tmp/f640-output/` to isolate conversion results from source
2. Batch converter must process `Game/ERB/口上/5_レミリア` directory with all `*.ERB` files
3. Expected at least 10 YAML outputs from all files (6 KOJO-prefixed + 4 non-KOJO via FallbackPattern): KOJO_K5_EVENT, KOJO_K5_会話親密, KOJO_K5_口挿入, KOJO_K5_愛撫, KOJO_K5_挿入, KOJO_K5_日常, NTR口上, NTR口上_お持ち帰り, SexHara休憩中口上, WC系口上
4. All 10 files (6 KOJO-prefixed via PathPattern, 4 non-KOJO via FallbackPattern) will be converted successfully
5. AC#2 accepts at least 10 YAML outputs (1:1 mapping or indexed outputs for multi-DATALIST files)
6. YamlValidator must validate against `tools/YamlSchemaGen/dialogue-schema.json`
7. BatchConverter continue-on-error (F634 AC#7) allows partial success with failure reporting
8. BatchConverter.cs code duplication is pre-existing (F634) and tracked for refactoring in F649. F640 does not modify this code

**Pre-conditions**:

- F634 Batch Conversion Tool is [DONE], providing BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter
- F633 PRINTDATA Parser Extension is [DONE], providing PrintDataNode with GetDataForms() helper
- Game/ERB/口上/5_レミリア directory exists with 10 ERB files (6 KOJO-prefixed + 4 non-KOJO)
- tools/KojoComparer project exists (batch mode deferred to F644)
- tools/YamlValidator project exists with dialogue-schema.json validation
- eraTW Talent.csv is accessible for condition metadata resolution (optional)

**Success Criteria**:

- Batch converter completes execution (exit code 0 expected - all files convertible via FallbackPattern)
- At least 10 YAML files created from all ERB files (allows indexed outputs)
- All YAML files pass dialogue-schema.json validation
- No TODO/FIXME/HACK markers in any output YAML file
- All YAML files contain "レミリア" character metadata
- ErbToYaml and KojoComparer projects build successfully

**Rollback Plan**:

This feature does not modify existing code -- it only executes existing tools to produce new YAML output in `.tmp/` directory. If issues arise:

1. Delete `.tmp/f640-output/` directory to remove conversion artifacts
2. Review batch report and individual file failure details
3. If tool bugs discovered, create follow-up feature for tool fixes (e.g., PathAnalyzer enhancement for non-KOJO patterns)
4. If ERB file has unsupported constructs, document in 残課題 for manual review or parser enhancement

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| KojoComparer equivalence verification for 6 converted files | F644 | AC#3 removed due to lack of CLI batch mode; deferred to F644 Equivalence Testing Framework |
| BatchConverter.cs code duplication (sequential vs parallel paths) | F649 | Technical debt: near-identical logic blocks should be refactored for maintainability |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

## Review Notes

- [resolved-applied] Phase6-FinalRefCheck iter10: F649 feature file referenced in Links section and required by AC#10, but does not exist. User decision: F649 [DRAFT] creation required before /run execution
- [resolved-invalid] Phase0-RefCheck iter1: F649 circular dependency: AC#10 requires F649 file existence but Task#10 creates F649. Previous resolution was F649 [DRAFT] creation before /run, but F649 still doesn't exist. Need to implement the previous decision or revise approach.
- [resolved-applied] Phase1-Pending iter2: F649 creation resolved — Task#10 in Implementation Contract Phase 7 creates F649 during /run. AC#10 verifies post-execution. No circular dependency.
- [resolved-applied] Phase1-Uncertain iter2: AC#1 and AC#6 Method columns contain identical commands which is confusing. Suggested fix: AC#6 Method should reference 'Same as AC#1 (stdout)' for clarity. However, no SSOT rule prohibits duplicate Method entries.
- [resolved-applied] Phase1-Pending iter3: Resolved by iter2 F649 resolution and iter2 AC#6 resolution.
- [resolved-applied] Phase1-Pending iter4: Resolved by iter2 F649 resolution chain.
- [resolved-applied] Phase1-Pending iter4: F649 creation handled by Task#10 during /run Phase 7. AC#10 verifies post-execution existence.
- [resolved-applied] Phase1-Uncertain iter4: AC#6 Method clarity resolved — ac-tester reads full feature file including AC Details (ac-tester.md lines 23-25). AC Details lines 252-253 explain AC#6 checks stdout from same AC#1 execution.
- [resolved-invalid] Phase1-Invalid iter4: Reviewer incorrectly claimed F648 exists on disk, but F648 file does not exist. Task#9 must still CREATE the file, not verify existence.
- [resolved-applied] Phase1-Pending iter5: All pending items resolved in iter6 batch resolution.
- [resolved-applied] Phase1-Uncertain iter5: ac-tester reads full feature file including AC Details, not only Method column. Current Method + AC Details provide sufficient execution context.
- [resolved-applied] Phase1-Uncertain iter8: F648 now exists as [DRAFT] on disk. AC#9/Task#9 changed from 'Create' to 'Verify exists'. Implementation Contract Phase 6 changed from implementer to ac-tester.
- [resolved-invalid] Phase1-Uncertain iter8: Goal #2 has no AC coverage. Resolved: Goal #2 "Track equivalence verification requirement" is documentation-level tracking satisfied by Philosophy Derivation table "Deferred to F644" entry and 残課題 row. No AC needed for deferred documentation tracking.

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|-----------------|---------------|
| PathAnalyzer non-KOJO pattern enhancement | FallbackPattern implemented in F639. F648 [CANCELLED] (superseded) | Feature | F648 | Task#9 (verify exists) |
| BatchConverter.cs code duplication (sequential vs parallel paths) | Technical debt: near-identical logic blocks should be refactored for maintainability | Feature | F649 | Task#10 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-28T15:10 | DEVIATION | ac-tester | AC#8 Build verification | KojoComparer build FAIL (PRE-EXISTING: YamlRunner.cs not updated for KojoEngine interface changes since F444) |
| 2026-01-28T15:15 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: Related Features table status mismatches with index-features.md |
| 2026-01-28T15:25 | DECISION | user | Option A selected | [BLOCKED] - wait for F651 to fix KojoComparer build (AC#8 blocker) |
| 2026-01-28T16:00 | RESUME | orchestrator | F651 [DONE] verified | KojoComparer + ErbToYaml build OK. AC#8 [B]→[x] |
