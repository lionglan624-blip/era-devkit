# Feature 639: Sakuya Kojo Conversion

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

## Type: erb

## Created: 2026-01-27

---

## Summary

Convert Sakuya (4_咲夜) kojo ERB files to YAML format with equivalence verification.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

15 ERB files in Game/ERB/口上/4_咲夜/ need conversion to YAML for the new kojo engine. Highest file count among characters.

### Goal (What to Achieve)

1. Convert 15 Sakuya kojo files using batch converter
2. Verify equivalence with KojoComparer
3. Validate YAML against schema
4. Zero technical debt (no TODO/FIXME/HACK)

---

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-634.md](feature-634.md) - Batch Conversion Tool (Predecessor)
- [feature-636.md](feature-636.md) - Meiling Kojo Conversion (parallel sibling)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (downstream)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 15 ERB files in Game/ERB/口上/4_咲夜/ need conversion to YAML for the new kojo engine
2. Why: The kojo engine migration (Phase 19) requires all ERB kojo files to be converted to YAML format for the new rendering pipeline
3. Why: The current ERB format is not machine-parseable for structured dialogue delivery; YAML provides schema-validated, structured data
4. Why: ERB files mix control flow (IF/ELSE), display commands (PRINTDATA/DATALIST), and dialogue content in a flat scripting format with no semantic separation
5. Why: The original Emuera engine was designed for imperative scripting, not structured dialogue data. Migration to YAML separates data from control flow, enabling schema validation and tooling

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 15 Sakuya ERB files remain unconverted | Phase 19 batch conversion pipeline (F634) is ready but per-character conversion batches have not been executed yet |
| ERB kojo files cannot be validated against a schema | ERB format mixes data and control flow; YAML format enables schema-based validation |

### Conclusion

The root cause is that the batch conversion tool (F634, [DONE]) exists and is functional, but the per-character conversion execution has not yet been performed. Sakuya is the highest-complexity character with 15 files (35,593 total lines) across 4 file categories: KOJO (7 files), NTR口上 (6 files), SexHara (1 file), WC (1 file).

**CRITICAL FINDING**: PathAnalyzer (tools/ErbToYaml/PathAnalyzer.cs) only matches files with `KOJO_` prefix pattern (`N_CharacterName/KOJO_Situation.ERB`). 8 of 15 Sakuya files have non-KOJO prefixes (NTR口上_, SexHara休憩中口上, WC系口上). These files will throw `ArgumentException` in PathAnalyzer.Extract(). This is a **systemic issue** affecting ALL character directories (every directory has 2-8 non-KOJO files). This must be handled before conversion can succeed.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Predecessor (batch tool) | Batch conversion tool with BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter. PathAnalyzer only handles KOJO_ prefix files |
| F633 | [DONE] | Predecessor (parser) | PRINTDATA...ENDDATA parser extension. PrintDataNode AST with GetDataForms() |
| F636 | [DONE] | Parallel sibling | Meiling conversion (11 files, PathAnalyzer limitation resolved by F639) |
| F637 | [DRAFT] | Parallel sibling | Koakuma conversion (11 files, same PathAnalyzer limitation) |
| F638 | [DRAFT] | Parallel sibling | Patchouli conversion (11 files, same PathAnalyzer limitation) |
| F640-F643 | [DRAFT] | Parallel siblings | Other character conversions (all affected by PathAnalyzer limitation) |
| F644 | [DRAFT] | Downstream | Equivalence Testing Framework - validates converted YAML output |
| F555 | [DONE] | Planning | Phase 19 Planning - defined conversion strategy and quality gates |
| F671 | [DRAFT] | Related | PrintDataNode Variant metadata mapping (deferred from F634) |

### Pattern Analysis

**PathAnalyzer limitation is systemic**: Every character directory (1_美鈴 through U_汎用) contains non-KOJO files. The current PathAnalyzer regex pattern `(\d+)_([^\\/]+)[\\/]KOJO_(.+)\.(?:ERB|erb)$` will reject:
- `NTR口上_*.ERB` (found in all 11 directories)
- `SexHara休憩中口上.ERB` (found in 10 of 11 directories)
- `WC系口上.ERB` (found in 10 of 11 directories)

This affects ALL conversion features F636-F643, not just F639. The PathAnalyzer must be extended to handle non-KOJO filename patterns before any conversion batch can fully succeed.

**Option A**: Extend PathAnalyzer to handle all filename patterns (broader regex or fallback extraction).
**Option B**: Scope F639 to KOJO-prefixed files only (7/15) and defer non-KOJO files to a follow-up feature.

This decision affects the feature's scope and AC definition.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All building blocks exist: ErbToYaml batch tool (F634 [DONE]), ErbParser with PRINTDATA (F633 [DONE]), KojoComparer, YamlValidator |
| Scope is realistic | YES | Scope expanded to include PathAnalyzer extension (Option 1 selected by user). All 15 files convertible after extension |
| No blocking constraints | YES | PathAnalyzer extension included in scope. F634 is [DONE] so tooling is available |

**Verdict**: FEASIBLE

Scope expanded per user decision: Option 1 (include PathAnalyzer extension in F639). This benefits all sibling conversion features F636-F643.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool - provides BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter |
| Predecessor | F633 | [DONE] | PRINTDATA Parser Extension - provides PrintDataNode AST |
| Blocker-for | F636-F643 | [DRAFT] | PathAnalyzer extension required for non-KOJO file conversion in all sibling character conversion features |
| Predecessor | F650 | [DONE] | ErbParser ENDDATA Edge Case Fix - unblocks AC#3 |
| Predecessor | F651 | [DONE] | KojoComparer KojoEngine API Update - unblocks AC#5 |
| Related | F644 | [DRAFT] | Equivalence Testing Framework (downstream quality validation) |
| Related | F671 | [DRAFT] | PrintDataNode Variant metadata mapping (display behavior semantics) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| tools/ErbToYaml/ (F634) | Build-time | Low | Batch converter CLI tool, [DONE] |
| tools/KojoComparer/ | Build-time | Low | Equivalence testing (ERB vs YAML output comparison) |
| tools/YamlValidator/ | Build-time | Low | Schema validation CLI |
| dialogue-schema.json | Runtime | Low | YAML schema for validation (used by FileConverter) |
| Talent.csv (eraTW reference) | Runtime | Low | Required by TalentCsvLoader for condition resolution |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F644 Equivalence Testing Framework | MEDIUM | Tests Sakuya converted YAML against original ERB output |
| F645 Kojo Quality Validator | LOW | Validates converted YAML quality (4x4x4 rules) |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/PathAnalyzer.cs | Update | Extend regex to handle non-KOJO filename patterns (NTR口上_, SexHara_, WC系) |
| tools/ErbToYaml/FileConverter.cs | Update | Replace hardcoded KOJO_ prefix stripping with generic prefix removal + empty-string guard |
| Game/ERB/口上/4_咲夜/*.yaml (15+ files) | Create | Converted YAML output files for all 15 Sakuya ERB files |

### Sakuya ERB Files (15 files, 35,593 total lines)

| # | File | Lines | Category | PRINTDATA/DATALIST Count |
|:-:|------|------:|----------|-------------------------:|
| 1 | KOJO_K4_愛撫.ERB | 4,917 | KOJO | 478 |
| 2 | KOJO_K4_会話親密.ERB | 3,901 | KOJO | 344 |
| 3 | KOJO_K4_口挿入.ERB | 3,077 | KOJO | 305 |
| 4 | KOJO_K4_挿入.ERB | 2,654 | KOJO | 268 |
| 5 | KOJO_K4_EVENT.ERB | 1,854 | KOJO | 40 |
| 6 | KOJO_K4_日常.ERB | 288 | KOJO | 9 |
| 7 | KOJO_K4_乳首責め.ERB | 196 | KOJO | 22 |
| 8 | NTR口上_シナリオ8.ERB | 3,553 | NTR | 265 |
| 9 | NTR口上_シナリオ1-7.ERB | 3,496 | NTR | 229 |
| 10 | NTR口上_シナリオ9.ERB | 3,697 | NTR | 206 |
| 11 | NTR口上_お持ち帰り.ERB | 2,411 | NTR | 104 |
| 12 | NTR口上_シナリオ11-22.ERB | 1,905 | NTR | 88 |
| 13 | NTR口上_野外調教.ERB | 1,084 | NTR | 56 |
| 14 | WC系口上.ERB | 1,552 | WC | 24 |
| 15 | SexHara休憩中口上.ERB | 1,008 | SexHara | 38 |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| PathAnalyzer regex only matches KOJO_ prefix | tools/ErbToYaml/PathAnalyzer.cs line 14-17 | HIGH - 8/15 files will fail with ArgumentException |
| Non-KOJO files use different function naming | ERB files (NTR_KOJO_K4, SexHara休憩中_Easy_K4, WC_DescriptiveStyle_K4) | MEDIUM - situation extraction logic differs per category |
| FileConverter strips "KOJO_" prefix from output filename | tools/ErbToYaml/FileConverter.cs line 94 | MEDIUM - Non-KOJO files would need different filename processing |
| Complex conditional structures in NTR/WC files | WC系口上.ERB uses multi-level IF/ELSEIF/ELSE for state branching | MEDIUM - Deep conditional nesting may exceed FileConverter's single-level IF handling |
| Large file sizes (up to 4,917 lines) | KOJO_K4_愛撫.ERB | LOW - Batch converter handles large files; performance is not a concern |
| Sakuya has NTR scenario split across 5 files | NTR口上_シナリオ1-7, 8, 9, 11-22 (split by scenario numbers) | LOW - Each file is independent, no cross-file dependencies |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| PathAnalyzer rejects 8/15 non-KOJO files | Certain | High | Must extend PathAnalyzer before conversion or scope to KOJO-only |
| Complex nested conditionals in NTR/WC files exceed converter capability | Medium | Medium | BatchConverter continue-on-error will report failures; manual review for complex files |
| Schema validation rejects converted YAML for non-standard structures | Medium | Medium | FileConverter validates before writing; failures reported in BatchReport |
| Equivalence testing (KojoComparer) may not cover NTR/WC file types | Low | Medium | KojoComparer processes ERB/YAML pairs by directory; file type agnostic |
| PathAnalyzer extension in F639 creates coupling with sibling features | Low | Low | Extension is additive (new patterns alongside existing KOJO_ pattern); no breaking changes |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Convert 15 Sakuya kojo files" | All 15 files must be converted, not just KOJO-prefixed subset | AC#3, AC#4 |
| "using batch converter" | Batch conversion tool must execute successfully on Sakuya directory | AC#3 |
| "Verify equivalence with KojoComparer" | KojoComparer must confirm ERB-YAML output equivalence for all converted files | AC#5 |
| "Validate YAML against schema" | All generated YAML files must pass schema validation | AC#6 |
| "Zero technical debt" | No TODO/FIXME/HACK markers in new or modified code | AC#8 |
| PathAnalyzer must handle non-KOJO prefixes (NTR口上_, SexHara_, WC系) | PathAnalyzer regex extended to extract character/situation from all filename patterns | AC#1, AC#2 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PathAnalyzer tests pass (new + regression) | test | dotnet test tools/ErbToYaml.Tests/ --filter PathAnalyzer | succeeds | - | [x] |
| 2 | PathAnalyzer extracts from NTR/SexHara/WC filenames | code | Grep(tools/ErbToYaml/PathAnalyzer.cs) | matches | "NTR口上\|SexHara\|WC系" | [x] |
| 3 | Batch conversion succeeds for Sakuya directory | exit_code | dotnet run --project tools/ErbToYaml/ -- --batch "Game/ERB/口上/4_咲夜" ".tmp/yaml/4_咲夜" --talent "Game/CSV/Talent.csv" --schema "tools/YamlSchemaGen/dialogue-schema.json" | succeeds | - | [x] |
| 4 | 100+ YAML output files created | file | Glob(".tmp/yaml/4_咲夜/*.yaml") | gte | 100 | [x] |
| 5 | KojoComparer equivalence verification | exit_code | dotnet run --project tools/KojoComparer/ -- "Game/ERB/口上/4_咲夜" ".tmp/yaml/4_咲夜" | succeeds | - | [B]→F644 |
| 6 | Schema validation passes for all YAML | exit_code | dotnet run --project tools/YamlValidator/ -- ".tmp/yaml/4_咲夜" | succeeds | - | [x] |
| 7 | Build succeeds | build | dotnet build tools/ErbToYaml/ | succeeds | - | [x] |
| 8 | Zero technical debt in modified files | code | Grep(tools/ErbToYaml/PathAnalyzer.cs,tools/ErbToYaml/FileConverter.cs) | not_contains | "TODO\|FIXME\|HACK\|workaround" | [x] |
| 9 | Remove hardcoded KOJO-only prefix replacement | code | Grep(tools/ErbToYaml/FileConverter.cs) | not_contains | "Replace(\"KOJO_\", \"\")" | [x] |
| 10 | FileConverter prefix logic produces valid filenames | test | dotnet test tools/ErbToYaml.Tests/ --filter FileConverter | succeeds | - | [x] |

**Note**: 10 ACs within erb type range (8-15). AC#1 verifies PathAnalyzer tests (both new non-KOJO tests and existing regression tests). AC#10 verifies FileConverter's new prefix logic produces non-empty, valid output filenames for all file categories (KOJO, NTR, SexHara, WC).

### AC Details

**AC#1: PathAnalyzer tests pass (new + regression)**
- New unit tests must be added to PathAnalyzerTests.cs covering NTR口上_, SexHara休憩中口上, WC系口上 filename patterns
- Tests verify Extract() returns correct (Character, Situation) tuples for each non-KOJO pattern
- Edge cases: filenames with Japanese characters, underscore variations, mixed patterns
- Regression safety: All existing KOJO_ prefix tests must continue to pass
- Method: `dotnet test tools/ErbToYaml.Tests/ --filter PathAnalyzer` must pass with all test cases

**AC#2: PathAnalyzer extracts from NTR/SexHara/WC filenames**
- PathAnalyzer.cs must be extended with additional regex patterns or fallback extraction logic
- The regex must handle: `NTR口上_シナリオ8.ERB`, `SexHara休憩中口上.ERB`, `WC系口上.ERB`
- Pattern must extract character name from parent directory (e.g., `4_咲夜` → `咲夜`)
- Situation extraction: filename stem without extension (e.g., `NTR口上_シナリオ8` → situation)
- Method: Grep for `NTR口上|SexHara|WC系` in PathAnalyzer.cs confirms all three non-KOJO pattern categories are handled

**AC#3: Batch conversion succeeds for Sakuya directory**
- BatchConverter must process all 15 ERB files in `Game/ERB/口上/4_咲夜/` without fatal errors
- Continue-on-error behavior (F634 AC#7) means individual file failures are reported but don't halt the batch
- Success = exit code 0 with batch report showing conversion attempted for all files
- Method: Run ErbToYaml CLI with --batch flag targeting Sakuya directory with output to `.tmp/yaml/4_咲夜/`

**AC#4: 100+ YAML output files created**
- At least 100 YAML files must exist in the output directory after batch conversion (based on 2,276 total PRINTDATA/DATALIST blocks across 15 files)
- FileConverter creates one YAML per convertible top-level node (PRINTDATA/DATALIST/IF-wrapped), not one per ERB file. With 478+ PRINTDATA blocks in just the largest file, total output count should be in the hundreds
- Note: gte requires manual verification per testing SKILL known limitations
- Method: Glob count of `*.yaml` files in output directory

**AC#5: KojoComparer equivalence verification**
- KojoComparer must be run against the original ERB directory and the converted YAML output
- Verifies that YAML output produces semantically equivalent dialogue output as the original ERB
- KojoComparer processes ERB/YAML pairs by directory and is file-type agnostic
- Method: Run KojoComparer CLI comparing source and output directories

**AC#6: Schema validation passes for all YAML**
- All generated YAML files must validate against `dialogue-schema.json`
- YamlValidator CLI processes all YAML files in the directory
- Failures indicate structural issues in the converter output
- Method: Run YamlValidator CLI against output directory

**AC#7: Build succeeds**
- `dotnet build tools/ErbToYaml/` must compile without errors after PathAnalyzer modifications
- Verifies no compilation regressions from code changes
- Method: Standard dotnet build

**AC#8: Zero technical debt in modified files**
- PathAnalyzer.cs and FileConverter.cs must not contain TODO, FIXME, HACK, or workaround markers
- Ensures both extensions are complete, not partial workarounds
- Method: Grep for debt markers in both modified files

**AC#9: Remove hardcoded KOJO-only prefix replacement**
- FileConverter.cs line 94 currently uses `Replace("KOJO_", "")` for output filenames
- Must be replaced with character-agnostic logic that reuses PathAnalyzer.Extract() result
- Eliminates hardcoded prefix knowledge and creates single source of truth
- Method: Grep confirms the hardcoded KOJO-only replacement is removed

**AC#10: FileConverter prefix logic produces valid filenames**
- Unit tests for FileConverter's new prefix removal logic
- Test cases: KOJO_K4_愛撫.ERB → baseFilename='K4_愛撫' (backward compatible), NTR口上_シナリオ8.ERB → baseFilename='NTR口上_シナリオ8', SexHara休憩中口上.ERB → baseFilename='SexHara休憩中口上', WC系口上.ERB → baseFilename='WC系口上'
- Regression case: confirms KOJO output filename remains 'K4_愛撫.yaml' (unchanged from current behavior)
- Method: `dotnet test tools/ErbToYaml.Tests/ --filter FileConverter` must pass

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Extend PathAnalyzer and FileConverter to handle non-KOJO filename patterns (NTR口上_, SexHara休憩中口上, WC系口上) using a **two-tier pattern matching strategy**: primary regex for KOJO_ prefix files (existing), and fallback logic for non-KOJO files (new).

**Rationale**: The current PathAnalyzer uses a single regex pattern optimized for KOJO_ files. Extending this to handle non-KOJO files requires:
1. **PathAnalyzer extension**: Add fallback pattern matching when primary KOJO_ regex fails
2. **FileConverter filename processing**: Replace hardcoded `Replace("KOJO_", "")` with generic prefix removal logic
3. **Test coverage**: Add unit tests for all non-KOJO filename patterns

This approach satisfies all ACs while maintaining backward compatibility (AC#9: existing KOJO tests still pass) and enabling batch conversion of all 15 Sakuya files.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add PathAnalyzerTests.cs test cases for NTR口上_, SexHara休憩中口上, WC系口上 patterns. Tests verify Extract() returns correct (Character, Situation) tuples. |
| 2 | Extend PathAnalyzer.Extract() with fallback logic: if primary KOJO_ regex fails, extract character from parent directory name (e.g., `4_咲夜` → `咲夜`) and situation from filename stem (e.g., `NTR口上_シナリオ8` → `NTR口上_シナリオ8`). |
| 3 | Run `dotnet run --project tools/ErbToYaml/ -- --batch "Game/ERB/口上/4_咲夜" ".tmp/yaml/4_咲夜"` after PathAnalyzer/FileConverter modifications. BatchConverter.ConvertAsync() processes all 15 ERB files with continue-on-error. |
| 4 | Verify output with `Glob(".tmp/yaml/4_咲夜/*.yaml")` after batch conversion. Expected: 100+ YAML files (multiple per ERB source file due to one-YAML-per-convertible-node, based on 2,276 total PRINTDATA/DATALIST blocks). Verify with gte matcher. Note: gte matcher requires manual verification per testing SKILL limitations. |
| 5 | Run `dotnet run --project tools/KojoComparer/ -- "Game/ERB/口上/4_咲夜" ".tmp/yaml/4_咲夜"` to verify ERB-YAML equivalence. KojoComparer is file-type agnostic (processes by directory). |
| 6 | Run `dotnet run --project tools/YamlValidator/ -- ".tmp/yaml/4_咲夜"` to validate all generated YAML against dialogue-schema.json. FileConverter validates before writing (line 128-141). |
| 7 | Run `dotnet build tools/ErbToYaml/` after code changes to verify no compilation regressions. |
| 8 | Grep PathAnalyzer.cs and FileConverter.cs for `TODO\|FIXME\|HACK\|workaround` markers after implementation. Ensure all logic is complete, not partial workarounds. Includes existing FileConverter.cs line 29 workaround comment. |
| 9 | Replace hardcoded `Replace("KOJO_", "")` in FileConverter.cs line 94 with character-agnostic logic using existing PathAnalyzer result. Eliminates hardcoded prefix knowledge. Grep confirms hardcoded replacement is removed. |
| 10 | Run `dotnet test tools/ErbToYaml.Tests/ --filter FileConverter` to verify FileConverter prefix removal tests pass. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| PathAnalyzer extension strategy | A: Expand primary regex to match all patterns<br>B: Add fallback logic for non-KOJO files<br>C: Create separate analyzer classes per file type | B: Fallback logic | Option A: Complex regex with multiple OR branches is fragile and hard to maintain. Option C: Over-engineering for 3 file types. **Option B**: Simple, maintainable, preserves existing KOJO_ regex (AC#1 regression safety), allows graceful degradation. |
| Character extraction for non-KOJO | A: Hardcode mapping (4_咲夜 → 咲夜)<br>B: Extract from parent directory name with pattern `(\d+)_([^\\/]+)` | B: Pattern extraction | Option A: Brittle, requires mapping updates. **Option B**: Reuses existing directory name extraction logic, consistent with KOJO_ pattern approach. |
| Situation extraction for non-KOJO | A: Use full filename stem (NTR口上_シナリオ8)<br>B: Strip prefix and use suffix (シナリオ8)<br>C: Use filename stem as-is | A: Full filename stem | Option B: Loses semantic information (NTR vs WC vs SexHara). Option C: Same as A. **Option A**: Preserves full context, consistent with KOJO_ pattern where situation includes prefix (e.g., COM_K1_0). |
| FileConverter filename processing | A: Replace all known prefixes (KOJO_, NTR口上_, etc.)<br>B: Use Path.GetFileNameWithoutExtension() directly<br>C: Reuse PathAnalyzer.Extract() situation result | C: Reuse PathAnalyzer situation | Option A: Requires maintaining parallel prefix list to PathAnalyzer patterns. Option B: Outputs full filename stem with prefixes, inconsistent with current behavior. **Option C**: Eliminates duplication, single source of truth, leverages PathAnalyzer's already-correct situation extraction. |
| Error handling for unknown patterns | A: Throw ArgumentException (existing behavior)<br>B: Return (Character: "Unknown", Situation: filename) | A: Throw ArgumentException | **Option A**: Fail-fast behavior aligns with existing PathAnalyzer contract (line 47-50). Option B: Silent failure masks configuration errors. Batch converter's continue-on-error (AC#7) handles exceptions gracefully. |

### Interfaces / Data Structures

**No new interfaces or data structures required.** This feature extends existing components (PathAnalyzer, FileConverter) without API changes.

#### PathAnalyzer.Extract() Extension

```csharp
private static readonly Regex FallbackPattern = new Regex(@"(?:^|[\\/])(\d+)_([^\\/]+)[\\/](.+)\.(?:ERB|erb)$", RegexOptions.Compiled);

public (string Character, string Situation) Extract(string erbFilePath)
{
    // Step 1: Try primary KOJO_ pattern (existing regex)
    var match = PathPattern.Match(erbFilePath);
    if (match.Success)
    {
        return (match.Groups[2].Value, match.Groups[3].Value);
    }

    // Step 2: Fallback for non-KOJO files
    // Pattern: N_CharacterName/Filename.ERB
    var fallbackMatch = FallbackPattern.Match(erbFilePath);

    if (fallbackMatch.Success)
    {
        var character = fallbackMatch.Groups[2].Value;  // e.g., "咲夜"
        var filename = fallbackMatch.Groups[3].Value;   // e.g., "NTR口上_シナリオ8"
        return (character, filename);
    }

    // Step 3: No match - throw exception (existing behavior)
    throw new ArgumentException($"Path does not match expected pattern: {erbFilePath}");
}
```

#### FileConverter Filename Processing (Line 94 Replacement)

```csharp
// Before (line 94):
string baseFilename = Path.GetFileNameWithoutExtension(erbFilePath).Replace("KOJO_", "");

// After (reuse existing PathAnalyzer result to eliminate duplication):
// Note: (character, situation) is already extracted at line 61
string baseFilename = situation;
```

**Note**: PathAnalyzer.Extract() already returns the appropriate situation value for both KOJO and non-KOJO files. KOJO files return character-prefixed situations (e.g., "K4_愛撫") while non-KOJO files return full filename stems (e.g., "NTR口上_シナリオ8", "SexHara休憩中口上"). FileConverter simply uses the situation as-is for the output filename, eliminating any hardcoded prefix logic.

#### Test Structure (PathAnalyzerTests.cs additions)

Add test methods after line 323 (end of existing tests):

```csharp
// AC#1: Non-KOJO prefix tests
[Fact]
public void Test_Extract_NtrKojoPrefix()
{
    var analyzer = new PathAnalyzer();
    var (character, situation) = analyzer.Extract(@"4_咲夜\NTR口上_シナリオ8.ERB");
    Assert.Equal("咲夜", character);
    Assert.Equal("NTR口上_シナリオ8", situation);
}

[Fact]
public void Test_Extract_SexHaraPrefix()
{
    var analyzer = new PathAnalyzer();
    var (character, situation) = analyzer.Extract(@"4_咲夜\SexHara休憩中口上.ERB");
    Assert.Equal("咲夜", character);
    Assert.Equal("SexHara休憩中口上", situation);
}

[Fact]
public void Test_Extract_NtrPrefix_DifferentCharacter()
{
    var analyzer = new PathAnalyzer();
    var (character, situation) = analyzer.Extract(@"1_美鈴\NTR口上_シナリオ1.ERB");
    Assert.Equal("美鈴", character);
    Assert.Equal("NTR口上_シナリオ1", situation);
}

[Fact]
public void Test_Extract_WcKojoPrefix()
{
    var analyzer = new PathAnalyzer();
    var (character, situation) = analyzer.Extract(@"4_咲夜\WC系口上.ERB");
    Assert.Equal("咲夜", character);
    Assert.Equal("WC系口上", situation);
}
```

### Implementation Sequence

1. **Test-first (TDD)**: Add 3 test methods to PathAnalyzerTests.cs (AC#1)
2. **PathAnalyzer.cs**: Add fallback regex pattern (AC#2)
3. **FileConverter.cs**: Replace hardcoded KOJO_ stripping with PathAnalyzer situation reuse (AC#9)
4. **Verify build**: `dotnet build tools/ErbToYaml/` (AC#7)
5. **Run tests**: `dotnet test tools/ErbToYaml.Tests/ --filter PathAnalyzer` (AC#1)
6. **Batch conversion**: Run ErbToYaml on Sakuya directory (AC#3)
7. **Verify output**: Check file count and schema validation (AC#4, AC#6)
8. **Equivalence test**: Run KojoComparer (AC#5)
9. **Debt check**: Grep for TODO/FIXME/HACK (AC#8)

### Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Fallback regex too permissive, matches unintended files | Medium | Require numbered directory prefix in fallback pattern (`(\d+)_`), consistent with primary pattern |
| Filename collision after prefix removal (e.g., two files with same base name) | Low | FileConverter already handles multi-node files with index suffix (line 148-155). Index suffix prevents collisions. |
| Non-KOJO files contain complex conditionals exceeding converter capability | Medium | BatchConverter continue-on-error (F634 AC#7) reports failures without halting batch. Failures tracked in BatchReport. |
| KojoComparer or YamlValidator not compatible with non-KOJO file types | Low | Both tools are file-type agnostic (process by directory/file pairs). No file type-specific logic. |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add unit tests for PathAnalyzer non-KOJO patterns | [x] |
| 2 | 2 | Extend PathAnalyzer with fallback extraction logic | [x] |
| 3 | 9 | Replace hardcoded KOJO prefix stripping in FileConverter | [x] |
| 4 | 7 | Build ErbToYaml project | [x] |
| 5 | 1 | Run PathAnalyzer unit tests (new + regression) | [x] |
| 6 | 3 | Run batch conversion on Sakuya directory | [x] |
| 7 | 4 | Verify YAML output file count | [x] |
| 8 | 5 | Run KojoComparer equivalence verification | [B]→F644 |
| 9 | 6 | Run YamlValidator schema validation | [x] |
| 10 | 8 | Verify zero technical debt in modified files | [x] |
| 11 | 10 | Add unit tests for FileConverter prefix removal logic | [x] |
| 12 | 8 | Remove FileConverter.cs line 29 workaround comment | [x] |
| 13 | - | Create F649 for FileConverter DatalistConverter interface refactor | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | PathAnalyzerTests.cs structure + test patterns from Technical Design | 3 new test methods added after line 323 |
| 2 | implementer | sonnet | T2 | PathAnalyzer.cs + fallback pattern pseudocode from Technical Design | Fallback extraction logic in Extract() method |
| 3 | implementer | sonnet | T3, T12 | FileConverter.cs + prefix removal logic from Technical Design | Simplified prefix logic using situation as-is for line 94, remove line 29 workaround comment (cosmetic only) |
| 3.5 | implementer | sonnet | T11 | FileConverterTests.cs + test cases from AC#10 | Unit tests for prefix removal (KOJO_, NTR口上_, SexHara, WC) |
| 4 | ac-tester | haiku | T4 | AC#7 build command | Build success verification |
| 5 | ac-tester | haiku | T5, T11 | AC#1, AC#10 test commands | Test success verification (PathAnalyzer + FileConverter tests, regression) |
| 6 | ac-tester | haiku | T6 | AC#3 batch conversion command | Batch conversion success verification |
| 7 | ac-tester | haiku | T7 | AC#4 Glob command | File count verification (15+ YAML files) |
| 8 | ac-tester | haiku | T8 | AC#5 KojoComparer command | Equivalence verification success |
| 9 | ac-tester | haiku | T9 | AC#6 YamlValidator command | Schema validation success |
| 10 | ac-tester | haiku | T10 | AC#8 Grep command | Technical debt check |

**Constraints** (from Technical Design):
1. PathAnalyzer extension must use fallback pattern after primary KOJO_ regex fails (two-tier matching)
2. Fallback pattern must extract character from parent directory name with pattern `(\d+)_([^\\/]+)`
3. Situation extraction uses full filename stem (e.g., `NTR口上_シナリオ8`) to preserve semantic context
4. FileConverter must reuse PathAnalyzer.Extract() situation result for output filename, eliminating hardcoded prefix logic and ensuring consistent character/situation extraction.
5. Existing KOJO_ tests must continue to pass (regression safety, included in AC#1)
6. Test-first implementation: T1 adds tests before T2 implements PathAnalyzer logic

**Pre-conditions**:
- F634 (Batch Conversion Tool) is [DONE] and functional
- F633 (PRINTDATA Parser Extension) is [DONE] and functional
- tools/ErbToYaml/ builds successfully
- Game/ERB/口上/4_咲夜/ contains 15 ERB files

**Success Criteria**:
- All 10 ACs pass verification
- 100+ YAML files generated in output directory (actual count based on 2,276 total PRINTDATA/DATALIST blocks across 15 files)
- No build errors, no test failures
- Zero TODO/FIXME/HACK/workaround markers in modified files

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation
4. PathAnalyzer fallback logic is additive (non-breaking), so revert only affects new non-KOJO file support

## Review Notes

- [resolved-applied] Phase1 iter4: AC Coverage (Technical Design) for AC#4 contradicts AC Details. AC Coverage says '15 YAML files (one per ERB source file)' and references 'count_equals matcher' but AC Definition Table uses `count_gte` and AC Details says 'output count will exceed 15' due to one-YAML-per-convertible-node. Fix: align all sections to use count_gte model.
- [resolved-skipped] Phase1 iter4: Implementation Contract uses non-standard Phase 3.5 numbering. No SSOT rule prohibits fractional phases but integer sequence would be cleaner. Fix would require renumbering all subsequent phases (4→5, 5→6, etc.) with risk of breaking cross-references.
- [resolved-applied] Phase1 iter6: AC#4 Type `file` with Matcher `count_gte` may not be documented in testing SKILL Method Column Usage table. The table only shows exists/not_exists and contains/matches matchers for file type, not count-based matchers. Changed to Type `output` for count-based verification.
- [resolved-applied] Phase1 iter8: AC#9 false positive risk - `contains "pathAnalyzer.Extract"` would match existing line 61 call `_pathAnalyzer.Extract()`, not necessarily new line 94 usage. Minor verification gap; behavior verified via AC#10 tests. Fixed by changing to `not_contains` for old pattern.
- [resolved-skipped] Phase1 iter9: AC#8 vs FileConverter.cs line 29 - AC#8 checks not_contains 'workaround' but existing line 29 has workaround comment. Implementation Contract sequences T12 (removal) before Phase 10 (AC#8 check), so dependency is handled correctly. Informational only.

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|---------|-------------|---------------|---------------|
| FileConverter.cs line 29 DatalistConverter workaround refactor | T12 only removes comment text (cosmetic), structural workaround pattern (_talentLoader = null) remains unresolved | F649 | FileConverter DatalistConverter interface refactor | T13 |
| AC#5 KojoComparer equivalence verification | KojoComparer CLI only supports per-file comparison; AC#5 assumed directory-level batch comparison | F644 | Equivalence Testing Framework | - |

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-28 08:40 | START | implementer - T1 + T11 (TDD Phase 3) |
| 2026-01-28 08:40 | END | implementer - T1 + T11 - SUCCESS |
| 2026-01-28 08:43 | START | implementer - T2, T3, T12 |
| 2026-01-28 08:43 | END | implementer - T2, T3, T12 - SUCCESS |
| 2026-01-28 | DEVIATION | Batch conversion | exit=1 | 14/15 success, 1 fail: NTR口上_お持ち帰り.ERB:1574 ENDDATA without matching PRINTDATA (PRE-EXISTING parser limitation). 214 YAML files generated. |
| 2026-01-28 | DEVIATION | feature-reviewer | NEEDS_REVISION | T6/T8 status inconsistent with AC status, T13 (F680 creation) not executed |
| 2026-01-28 | FIX | Phase 7 | T6→[-], T8→[B], T13 executed (F649 created as DRAFT), F680→F649 in Handoffs |
| 2026-01-28 | BLOCKED | Phase 8 | User decision: [BLOCKED]. Created F650 (parser fix), F651 (KojoComparer API update) as DRAFT. Added as Predecessors. |
| 2026-01-28 | RESUME | Phase 6 | F650 [DONE], F651 [DONE]. Re-running blocked ACs. |
| 2026-01-28 | PASS | AC#3 | Batch conversion 15/15 success, exit=0. 228 YAML files generated. |
| 2026-01-28 | PASS | AC#4 | 228 YAML files (≥100 required). |
| 2026-01-28 | PASS | AC#6 | Schema validation 228/228 passed, exit=0. |
| 2026-01-28 | DEVIATION | AC#5 | KojoComparer CLI requires per-file args (--erb/--function/--yaml/--talent), not directory-level comparison. AC#5 method incompatible with current KojoComparer interface. |
| 2026-01-28 | DEVIATION | feature-reviewer | NEEDS_REVISION | (1) AC#5 method incompatible with KojoComparer CLI, (2) F636 stale [DRAFT]→[DONE], (3) Status [WIP] misleading |
| 2026-01-28 | FIX | Phase 7 | AC#5→[B]→F644 (user approved defer). F636 status updated to [DONE]. Handoff added for F644. |
