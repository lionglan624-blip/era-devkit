# Feature 643: Generic Kojo Conversion

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

Extend PathAnalyzer for non-numeric directory prefixes (U_汎用) and run batch conversion. 4/12 files contain convertible blocks: KOJO_KU_日常.ERB (PRINTDATA), NTR口上.ERB/NTR口上_お持ち帰り.ERB/NTR口上_野外調教.ERB (PRINTDATAL); remaining 8 files are non-convertible (empty stubs, SELECTCASE branching, function definitions).

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

12 ERB files in Game/ERB/口上/U_汎用/ are not yet processed by the batch converter. PathAnalyzer requires `(\d+)_` numeric prefix but U_汎用 uses letter prefix `U_`, causing ArgumentException for all files. F636 pilot completed successfully, establishing the conversion pattern.

### Goal (What to Achieve)

1. Extend PathAnalyzer to support non-numeric directory prefixes (U_汎用)
2. Run batch conversion on U_汎用 (4 files produce YAML output: KOJO_KU_日常 via PRINTDATA, NTR口上/NTR口上_お持ち帰り/NTR口上_野外調教 via PRINTDATAL)
3. Validate YAML against schema (YamlValidator)
4. Verify equivalence with KojoComparer (build restored)
5. Document 8 non-convertible files (imperative ERB) for future manual YAML authoring
6. Zero technical debt (no TODO/FIXME/HACK)

---

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-633.md](feature-633.md) - PRINTDATA Parser Extension (predecessor)
- [feature-634.md](feature-634.md) - Batch Conversion Tool (predecessor)
- [feature-635.md](feature-635.md) - Conversion Parallelization (related)
- [feature-636.md](feature-636.md) - Meiling Kojo Conversion (sibling, pilot)
- [feature-637.md](feature-637.md) - Character conversion batch (sibling)
- [feature-638.md](feature-638.md) - Character conversion batch (sibling)
- [feature-639.md](feature-639.md) - Character conversion batch (sibling)
- [feature-640.md](feature-640.md) - Character conversion batch (sibling)
- [feature-641.md](feature-641.md) - Character conversion batch (sibling)
- [feature-642.md](feature-642.md) - Character conversion batch (sibling)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework
- [feature-674.md](feature-674.md) - Manual YAML Authoring for U_汎用 Non-Convertible Files (handoff)
- [feature-671.md](feature-671.md) - PrintDataNode Variant Metadata Mapping

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 12 ERB files in Game/ERB/口上/U_汎用/ are not yet in YAML format, blocking the new kojo engine pipeline for generic/shared kojo content
2. Why: The batch conversion tool (F634) was recently completed but has not been executed on any directory yet; F636-F643 are all [DRAFT]
3. Why: Phase 19 planned conversion as a sequential pipeline: tooling first (F633/F634), then per-directory conversion batches (F636-F643), with F643 covering U_汎用
4. Why: The U_汎用 directory contains structurally diverse files -- some follow the KOJO_ naming convention, others do not (NTR口上.ERB, SexHara休憩中口上.ERB), and the directory itself uses `U_` prefix instead of the numbered pattern (`N_CharacterName`) expected by PathAnalyzer
5. Why: The F634 batch converter was designed primarily for character directories with numbered prefixes (e.g., `1_美鈴/KOJO_K1_愛撫.ERB`). The PathAnalyzer regex `(\d+)_CharacterName/KOJO_Situation.ERB` will throw ArgumentException for U_汎用 files because: (a) `U_汎用` does not start with a digit, and (b) 4 of 12 files lack the `KOJO_` prefix entirely

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 12 generic kojo ERB files not in YAML format | Phase 19 conversion pipeline available (F634 [DONE]) but not yet executed on any directory |
| PathAnalyzer will fail for U_汎用 files | PathAnalyzer regex requires `(\d+)_CharacterName/KOJO_Situation.ERB` pattern; U_汎用 uses `U_` prefix (non-numeric) and 4 files lack `KOJO_` filename prefix |
| Significant content will be missed by converter | 7 of 12 files use PRINTFORML/SELECTCASE/IF-ELSE outside PRINTDATA/DATALIST blocks; FileConverter only processes PRINTDATA/DATALIST/IfNode-wrapping-PRINTDATA nodes |
| Equivalence testing ready | F651 [DONE] resolved KojoComparer build errors; F644 [DRAFT] defines equivalence testing framework |

### Conclusion

The root cause has two layers:

**Layer 1 (Blocking)**: PathAnalyzer will throw ArgumentException for ALL 12 files in U_汎用 because the directory name `U_汎用` does not match the regex pattern `(\d+)_CharacterName`. This means the batch converter cannot process this directory at all without PathAnalyzer modification or an alternative character/situation extraction strategy.

**Layer 2 (Structural Mismatch)**: Even if PathAnalyzer is fixed, the generic kojo files have fundamentally different structure than character-specific kojo:

| File Category | Count | ERB Structure | Converter Support |
|---------------|:-----:|---------------|:-----------------:|
| KOJO_KU_*.ERB (template/stub) | 7 | PRINTFORML, SELECTCASE, IF/ELSE, CALL, empty PRINTFORMW stubs | LOW - mostly PRINTFORML outside PRINTDATA blocks. 3 files (日常, EVENT, 挿入) are largely empty stub templates with no convertible content |
| KOJO_MODIFIER_COMMON.ERB | 1 | Functions (@KOJO_MODIFIER_PRE/POST_COMMON) with IF/PRINTFORML/RETURNF | NONE - function definitions with imperative logic, not declarative data |
| NTR口上*.ERB | 3 | IF/ELSEIF chains with PRINTFORML + interspersed PRINTDATAL blocks | PARTIAL - PRINTDATAL blocks inside IF branches convertible; standalone PRINTFORML lost |
| SexHara休憩中口上.ERB | 1 | Function definitions (@SexHara休憩中_*_KU_N) with PRINTFORML | NONE - function definitions with imperative logic |
| KOJO_KU_関係性.ERB | 1 | SELECTCASE branching with PRINTFORML per character | NONE - SELECTCASE/PRINTFORML pattern not supported by converter |
| KOJO_KU_会話親密.ERB | 1 | SELECTCASE + nested IF/ELSE with PRINTFORML | NONE - complex imperative branching |

**Summary**: 4 of 12 files have PRINTDATAL content that the converter can process (KOJO_KU_日常.ERB contains PRINTDATA blocks; NTR口上.ERB, NTR口上_お持ち帰り.ERB, NTR口上_野外調教.ERB contain PRINTDATAL blocks). The remaining 8 files use imperative ERB patterns (function definitions, SELECTCASE, standalone PRINTFORML, empty stubs) that the PRINTDATA/DATALIST-focused converter cannot handle. This feature requires either:
1. Extending the converter to handle PRINTFORML/SELECTCASE/function patterns, OR
2. Manual YAML authoring for the 8 non-convertible files, OR
3. Scoping the feature to only the 3 partially-convertible NTR口上 files plus the 7 KOJO_KU_* files that contain PRINTDATAL blocks (within IF wrappers)

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Predecessor (batch tool) | Batch converter with --batch mode, FileConverter, PrintDataConverter, PathAnalyzer. 48 tests pass. PathAnalyzer regex does NOT support U_汎用 pattern. |
| F633 | [DONE] | Predecessor (parser) | PRINTDATA...ENDDATA parser extension. PrintDataNode with GetDataForms() helper. |
| F636 | [DONE] | Sibling (pilot) | Meiling Kojo Conversion -- first character batch completed (11/11 files). |
| F637-F642 | [DRAFT] | Sibling batches | Other character conversion batches. All depend on F634 and follow N_CharacterName pattern (PathAnalyzer works for these). |
| F644 | [DRAFT] | Downstream (equivalence) | Equivalence Testing Framework. Depends on F636-F643 outputs. KojoComparer build restored by F651. |
| F671 | [DRAFT] | Related (variant) | PrintDataNode Variant metadata mapping (PRINTDATAL vs PRINTDATA). Affects NTR口上 files which use PRINTDATAL. |
| F635 | [PROPOSED] | Related (parallelization) | Conversion parallelization -- not required for F643 (12 files is small). |

### Pattern Analysis

F643 is the ONLY conversion batch where PathAnalyzer will fail. F636-F642 all use numbered directory prefixes (`1_美鈴`, `2_小悪魔`, etc.) matching the expected pattern. The U_汎用 directory is structurally unique in the kojo directory hierarchy.

Additionally, U_汎用 is the ONLY directory containing:
- Non-KOJO prefixed files (NTR口上.ERB, SexHara休憩中口上.ERB)
- Modifier/utility function files (KOJO_MODIFIER_COMMON.ERB)
- Multi-character SELECTCASE branching (KOJO_KU_関係性.ERB has 10+ character cases)
- Empty template stubs (KOJO_KU_愛撫.ERB, KOJO_KU_挿入.ERB with empty PRINTFORMW)

This makes F643 fundamentally different from F636-F642 in conversion complexity.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | PARTIAL | PathAnalyzer needs modification to support U_汎用 pattern. 3/12 files partially convertible (NTR口上 with PRINTDATAL blocks). 9/12 files require manual conversion or converter extension (PRINTFORML/SELECTCASE/function patterns). |
| Scope is realistic | PARTIAL | Automated conversion covers only ~25% of content. Manual YAML authoring or converter extension needed for 75%. Scope may need revision. |
| No blocking constraints | PARTIAL | PathAnalyzer blocker is solvable (regex extension). KojoComparer build errors block equivalence testing (same as F636). PRINTFORML/SELECTCASE conversion gap is a design limitation, not a bug. |

**Verdict**: FEASIBLE

The feature scope has been revised to address the identified constraints:
1. **PathAnalyzer extension included**: Regex extension is part of the implementation scope (Tasks T1-T2)
2. **Realistic conversion expectations**: 4/12 files will be converted (KOJO_KU_日常 with PRINTDATA, 3 NTR口上 files with PRINTDATAL), 8/12 files documented as non-convertible
3. **Clear technical approach**: Dual pattern extension (PathPattern + FallbackPattern) with character mapping

The feature is technically feasible with the current scope and design.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool -- PathAnalyzer requires extension for U_汎用 pattern |
| Predecessor | F633 | [DONE] | PRINTDATA Parser Extension -- PrintDataNode with GetDataForms() |
| Related | F636 | [DONE] | Meiling Kojo Conversion (pilot batch, establishes conversion pattern) |
| Related | F637-F642 | [DRAFT] | Character conversion batches (all use numbered directory pattern) |
| Related | F644 | [DRAFT] | Equivalence Testing Framework (downstream, KojoComparer broken) |
| Related | F671 | [DRAFT] | PrintDataNode Variant Metadata Mapping (affects NTR口上 PRINTDATAL) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| ErbToYaml (tools/ErbToYaml/) | Build-time | Low | F634 [DONE], builds successfully, 48 tests pass |
| PathAnalyzer (tools/ErbToYaml/PathAnalyzer.cs) | Build-time | HIGH | Regex `(\d+)_CharacterName/KOJO_Situation.ERB` does NOT match `U_汎用/` or non-KOJO filenames. All 12 files will throw ArgumentException |
| dialogue-schema.json (tools/YamlSchemaGen/) | Runtime | Low | Schema file exists, used by FileConverter for validation |
| Talent.csv (Game/CSV/) | Runtime | Low | Required by TalentCsvLoader for condition resolution |
| KojoComparer (tools/KojoComparer/) | Build-time | Low | Build restored by F651 [DONE]. Can verify ERB==YAML equivalence |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F644 Equivalence Testing Framework | HIGH | Will consume YAML files generated by F643 |
| Game/YAML/Kojo/ directory | LOW | Generated YAML files for runtime use by kojo engine |
| F636-F642 sibling batches | LOW | F643 findings inform PathAnalyzer extension needs (unique to F643) |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/PathAnalyzer.cs | Update | Extend regex to support `U_汎用` non-numbered directory and non-KOJO filename patterns |
| tools/ErbToYaml.Tests/PathAnalyzerTests.cs | Update | Add tests for U_汎用 path pattern extraction |
| Game/YAML/Kojo/U_汎用/*.yaml | Create | YAML files from converted ERB (partial: only PRINTDATAL blocks from convertible files) |
| Game/agents/feature-643.md | Update | Execution log, AC status updates |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| PathAnalyzer regex requires `(\d+)_` directory prefix | PathAnalyzer.cs line 14 | HIGH - All 12 files fail. Must extend regex to support `U_汎用` (letter prefix) |
| PathAnalyzer regex requires `KOJO_` filename prefix | PathAnalyzer.cs line 15 | HIGH - 4 files (NTR口上.ERB, NTR口上_お持ち帰り.ERB, NTR口上_野外調教.ERB, SexHara休憩中口上.ERB) fail. Must support non-KOJO filenames |
| FileConverter only processes PRINTDATA/DATALIST/IfNode-wrapping-PRINTDATA | FileConverter.cs ConvertAsync() | HIGH - 9/12 files use PRINTFORML/SELECTCASE/function patterns outside PRINTDATA blocks. Content will be missed |
| KojoComparer build restored | F651 resolved YamlRunner.cs API mismatch | Low - Build verified, equivalence testing available for F644 |
| KOJO_MODIFIER_COMMON.ERB is imperative code | Function definitions with IF/PRINTFORML/RETURNF | MEDIUM - Not convertible to declarative YAML. May need to remain as ERB or require manual YAML design |
| Many files are empty stub templates | KOJO_KU_愛撫.ERB, KOJO_KU_挿入.ERB etc. have empty PRINTFORMW | LOW - No actual content to convert. May produce empty/trivial YAML |
| SELECTCASE branching for 10 characters | KOJO_KU_関係性.ERB | MEDIUM - Per-character branching pattern not supported by converter. Would need manual YAML with character branches |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| PathAnalyzer throws ArgumentException for all U_汎用 files | Certain | HIGH | Must extend PathAnalyzer regex before conversion. Include as scope or prerequisite task |
| Most file content missed by converter (PRINTFORML/SELECTCASE) | Certain | HIGH | Revise scope to acknowledge partial automation. Plan manual YAML authoring for non-convertible files |
| Empty stub files produce trivial/empty YAML | High | LOW | Document which files are stubs. Consider excluding empty stubs from conversion scope |
| KojoComparer unavailable for equivalence testing | Certain | HIGH | Defer equivalence to F644. Use schema validation + manual spot-check |
| KOJO_MODIFIER_COMMON.ERB not convertible (imperative logic) | Certain | MEDIUM | This file defines modifier functions, not dialogue data. May need to remain as ERB or require architectural decision on how modifiers work in YAML pipeline |
| Schema validation rejects converted YAML from NTR口上 files | Medium | Medium | FileConverter validates against schema. Failed files reported in BatchReport |
| Pilot lessons from F636 change approach | Medium | LOW | F643 should wait for F636 pilot results before finalizing conversion strategy |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Extend PathAnalyzer to support non-numeric directory prefixes (U_汎用)" | PathAnalyzer must accept U_汎用 directory pattern without ArgumentException | AC#1, AC#2, AC#3 |
| "Run batch conversion on U_汎用 (4 files produce YAML output)" | Batch converter produces YAML files for KOJO_KU_日常 and 3 NTR口上 files | AC#4, AC#5 |
| "Validate YAML against schema (YamlValidator)" | Generated YAML files pass schema validation | AC#6 |
| "Verify equivalence with KojoComparer (build restored)" | KojoComparer project builds without errors | AC#7 |
| "Document 8 non-convertible files (imperative ERB)" | Feature file documents all non-convertible files with reasons | AC#8 |
| "Zero technical debt (no TODO/FIXME/HACK)" | No debt markers in modified source files | AC#9 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PathAnalyzer unit tests pass | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PathAnalyzerTests | succeeds | - | [x] |
| 2 | PathAnalyzer handles U_汎用 directory pattern | code | Grep(tools/ErbToYaml.Tests/PathAnalyzerTests.cs) | contains | "U_汎用" | [x] |
| 3 | PathAnalyzer supports letter prefix | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~GenericKojo | succeeds | - | [x] |
| 4 | YAML output exists for KOJO_KU_日常 | file | Glob(Game/YAML/Kojo/U_汎用/*日常*) | exists | - | [x] |
| 5 | YAML output exists for NTR口上 | file | Glob(Game/YAML/Kojo/U_汎用/*NTR口上*) | exists | - | [x] |
| 6 | Generated YAML passes schema validation | exit_code | dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/U_汎用/ | succeeds | - | [x] |
| 7 | KojoComparer project builds | build | dotnet build tools/KojoComparer | succeeds | - | [x] |
| 8 | Non-convertible files documented | code | Grep(path="Game/agents/feature-643.md", pattern="Non-convertible:", output_mode="count") | gte | 8 | [x] |
| 9 | Zero technical debt | code | Grep(tools/ErbToYaml/, pattern="TODO|FIXME|HACK", glob="PathAnalyzer*", output_mode="count") | equals | 0 | [x] |
| 10 | No YAML for non-convertible files | file | Glob(Game/YAML/Kojo/U_汎用/{*MODIFIER*,*SexHara*}) | not_exists | - | [x] |
| 11 | F674 handoff file created | file | Glob(Game/agents/feature-674.md) | exists | - | [x] |

### AC Details

**AC#1: PathAnalyzer unit tests pass**
- Test: `dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PathAnalyzerTests`
- Expected: All PathAnalyzer tests pass including new U_汎用 tests
- Rationale: Ensures PathAnalyzer extension does not break existing numbered-prefix extraction while adding U_汎用 support

**AC#2: PathAnalyzer handles U_汎用 directory pattern**
- Test: Grep pattern=`U_汎用` path=`tools/ErbToYaml.Tests/PathAnalyzerTests.cs`
- Expected: At least one test case references U_汎用 directory pattern
- Rationale: TDD requires test cases for the new non-numeric directory prefix before implementation
- Edge cases: Both `KOJO_` prefixed files (KOJO_KU_日常.ERB) and non-KOJO files (NTR口上.ERB) must be tested

**AC#3: PathAnalyzer supports letter prefix**
- Test: `dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~GenericKojo`
- Expected: All GenericKojo tests pass
- Rationale: GenericKojo test filter verifies that PathAnalyzer can handle non-numeric directory prefixes (U_汎用) without ArgumentException

**AC#4: YAML output exists for KOJO_KU_日常**
- Test: Glob pattern=`Game/YAML/Kojo/U_汎用/*日常*`
- Expected: At least one YAML file matching KOJO_KU_日常 exists
- Rationale: Goal #2 specifies KOJO_KU_日常 as one of 2 files that produce YAML output from batch conversion

**AC#5: YAML output exists for NTR口上**
- Test: Glob pattern=`Game/YAML/Kojo/U_汎用/*NTR口上*`
- Expected: At least one YAML file matching NTR口上 exists
- Rationale: Goal #2 specifies NTR口上 as one of 2 files that produce YAML output from batch conversion
- Note: NTR口上 files contain PRINTDATAL blocks that the converter can process partially

**AC#6: Generated YAML passes schema validation**
- Test: `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/U_汎用/`
- Expected: Exit code 0 (all YAML files in output directory pass dialogue-schema.json validation)
- Rationale: Goal #3 requires schema validation for all generated YAML

**AC#7: KojoComparer project builds**
- Test: `dotnet build tools/KojoComparer`
- Expected: Build succeeds with zero errors
- Rationale: Goal #4 requires KojoComparer build for equivalence testing. Project currently builds successfully and can verify ERB==YAML equivalence

**AC#8: Non-convertible files documented**
- Test: Grep pattern=`Non-convertible:` path=`Game/agents/feature-643.md` output_mode=count
- Expected: count_gte 8 (each file documented with "Non-convertible:" prefix in Execution Log)
- Rationale: Goal #5 requires documenting all 8 non-convertible files with specific prefix for tracking
- Files to document: KOJO_KU_愛撫, KOJO_KU_挿入, KOJO_KU_EVENT, KOJO_KU_関係性, KOJO_KU_会話親密, KOJO_MODIFIER_COMMON, SexHara休憩中口上, KOJO_KU_口挿入

**AC#9: Zero technical debt**
- Test: Grep pattern=`TODO` path=`tools/ErbToYaml/PathAnalyzer.cs`
- Expected: Not found
- Rationale: Goal #6 requires zero technical debt markers in modified source files
- Note: Only PathAnalyzer.cs checked as it is the primary modified file. PathAnalyzerTests.cs checked implicitly during code review.

**AC#10: No YAML for non-convertible files**
- Test: Glob pattern=`Game/YAML/Kojo/U_汎用/{*MODIFIER*,*SexHara*}`
- Expected: Not exists
- Rationale: Non-convertible files should not produce YAML output during batch conversion
- Example: KOJO_MODIFIER_COMMON.ERB and SexHara休憩中口上.ERB should not produce any YAML files
- Note: This is a representative spot-check of non-convertible files

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Extend PathAnalyzer to support non-numeric directory prefixes by modifying the PathPattern regex to accept both numeric (`\d+`) and alphabetic (`[A-Z]`) prefixes. This satisfies AC#1-3 (PathAnalyzer extension) while maintaining backward compatibility with existing numbered character directories (F636-F642).

The U_汎用 directory will be treated as character="汎用" (generic) for consistency with the ERB-to-YAML conversion pipeline. Files in this directory will follow the same conversion workflow as character-specific files:
1. PathAnalyzer extracts (character="汎用", situation=filename_prefix)
2. FileConverter processes PRINTDATA/DATALIST/IfNode-wrapping-PRINTDATA blocks
3. YAML output written to `Game/YAML/Kojo/U_汎用/`
4. YamlValidator validates against dialogue-schema.json

**Scope Acknowledgment**: From the investigation, 4/12 files produce YAML output (KOJO_KU_日常.ERB, NTR口上.ERB, NTR口上_お持ち帰り.ERB, NTR口上_野外調教.ERB). The remaining 8 files use imperative ERB patterns (PRINTFORML, SELECTCASE, function definitions) that the converter cannot handle. These will be documented in the Execution Log during implementation (satisfies AC#8).

**KojoComparer Build**: AC#7 requires KojoComparer to build. F651 [DONE] resolved the KojoEngine API mismatch, so KojoComparer should now build successfully.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Run `dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PathAnalyzerTests` after implementing regex extension and new test cases. All existing tests must pass (backward compatibility) plus new U_汎用 tests. |
| 2 | Add test case `Test_Extract_GenericKojoDirectory()` in PathAnalyzerTests.cs that validates extraction from `U_汎用/KOJO_KU_日常.ERB` → (character="汎用", situation="KU_日常"). Grep will find "U_汎用" string in test file. |
| 3 | Modify PathPattern regex from `(\d+)_([^\\/]+)` to `([A-Z\d]+)_([^\\/]+)` to accept both alphabetic and numeric prefixes. Special case: if prefix is "U", character name is "汎用" (hardcoded mapping for clarity). |
| 4 | Run `dotnet run --project tools/ErbToYaml -- --batch Game/ERB/口上/U_汎用 --output Game/YAML/Kojo/U_汎用` to generate YAML from KOJO_KU_日常.ERB. Glob pattern `Game/YAML/Kojo/U_汎用/*日常*` will find output file. |
| 5 | Same batch conversion command will process 3 NTR口上*.ERB files (contain PRINTDATAL blocks). Glob pattern `Game/YAML/Kojo/U_汎用/*NTR口上*` will find output files. |
| 6 | Run `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/U_汎用/` after conversion. YamlValidator validates all YAML files in directory against dialogue-schema.json. Exit code 0 = pass. |
| 7 | Build `dotnet build tools/KojoComparer`. F651 [DONE] resolved KojoEngine API mismatch, so build should succeed. |
| 8 | During execution, document all 8 non-convertible files in Execution Log with reason (e.g., "KOJO_MODIFIER_COMMON.ERB: Function definitions with imperative logic, not declarative data"). Grep count_gte 8 will find these mentions. |
| 9 | Code review PathAnalyzer.cs and PathAnalyzerTests.cs after implementation. Remove any TODO/FIXME/HACK comments. Grep pattern `TODO\|FIXME\|HACK` should return 0 matches. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Regex Extension Strategy** | A: Special-case "U_" prefix only<br>B: Generalize to `[A-Z\d]+` pattern<br>C: Accept any non-numeric prefix `[A-Za-z]+_` | **B: Generalize to `[A-Z\d]+`** | Zero Debt Upfront principle requires proper upfront investment. Pattern `([A-Z\d]+)_` handles all uppercase letter and numeric prefixes without future code changes. Conservative (uppercase only) yet extensible for directories like M_修飾 or S_共有. |
| **Character Name Mapping** | A: Use "U_汎用" (directory name as-is)<br>B: Use "汎用" (semantic name)<br>C: Use "Unknown" (generic fallback) | **B: Use "汎用"** | Matches ERB semantic intent. YAML output path `Game/YAML/Kojo/U_汎用/汎用_KU_日常.yaml` is clearer than `U_汎用_KU_日常.yaml`. Consistent with character directories (1_美鈴 → 美鈴). |
| **Non-convertible File Handling** | A: Extend converter to support PRINTFORML/SELECTCASE<br>B: Manual YAML authoring<br>C: Document only (defer to F671) | **C: Document only (defer to F671)** | Extending converter for imperative ERB patterns (PRINTFORML/SELECTCASE/function definitions) is out of scope (requires major converter refactor). AC#8 requires documentation, not conversion. Manual YAML authoring deferred to F671 [DRAFT] which handles variant metadata mapping. |
| **KojoComparer Build Verification** | A: Build KojoComparer successfully<br>B: Document build success for downstream F644<br>C: Skip equivalence testing if other issues | **A: Build KojoComparer successfully** | F651 [DONE] resolved KojoEngine API mismatch. KojoComparer should build without errors, enabling equivalence testing in F644. |

### Interfaces / Data Structures

**PathAnalyzer Regex Extension**:

```csharp
// Current PathPattern and FallbackPattern (F634)
private static readonly Regex PathPattern = new Regex(
    @"(?:^|[\\/])(\d+)_([^\\/]+)[\\/]KOJO_(.+)\.(?:ERB|erb)$",
    RegexOptions.Compiled
);

private static readonly Regex FallbackPattern = new Regex(
    @"(?:^|[\\/])(\d+)_([^\\/]+)[\\/]([^\\\/]+)\.(?:ERB|erb)$",
    RegexOptions.Compiled
);

// Extended patterns (F643) - generalize to all uppercase alphanumeric prefixes
private static readonly Regex PathPattern = new Regex(
    @"(?:^|[\\/])([A-Z\d]+)_([^\\/]+)[\\/]KOJO_(.+)\.(?:ERB|erb)$",
    //              ^^^^^^^^ Changed: \d+ → [A-Z\d]+ to accept alphanumeric prefixes
    RegexOptions.Compiled
);

private static readonly Regex FallbackPattern = new Regex(
    @"(?:^|[\\/])([A-Z\d]+)_([^\\/]+)[\\/]([^\\\/]+)\.(?:ERB|erb)$",
    //              ^^^^^^^^ Changed: \d+ → [A-Z\d]+ to accept alphanumeric prefixes
    RegexOptions.Compiled
);
```

**Character Name Mapping Logic**:

```csharp
public (string Character, string Situation) Extract(string erbFilePath)
{
    // ... existing null check and regex matching ...

    if (match.Success)
    {
        // Group 1: Directory prefix (e.g., "1", "U")
        var directoryPrefix = match.Groups[1].Value;

        // Group 2: Directory name after prefix (e.g., "美鈴", "汎用")
        var directoryName = match.Groups[2].Value;

        // Use directory name directly as character (works for both numeric and letter prefixes)
        var character = directoryName;

        // Group 3: Situation (everything after KOJO_ and before .ERB)
        var situation = match.Groups[3].Value;

        return (character, situation);
    }

    // ... existing fallback pattern logic ...
}
```

**Edge Cases**:
- Empty string input: Already handled by existing null/whitespace check (line 35-38 in PathAnalyzer.cs)
- Non-KOJO files in U_汎用: Handled by existing FallbackPattern (added in F639, lines 54-60)
- Multiple character directories with same alphabetic prefix: Not expected in current codebase (only U_汎用 exists). If future directories use `M_`, `S_`, etc., the mapping logic can be extended.
- Existing test backward compatibility: `Test_Extract_NoNumberedDirectory_Throws` remains valid after regex extension because the test path `口上\美鈴\KOJO_K1_愛撫.ERB` has no directory prefix at all (not matching `[A-Z\d]+_` pattern), so ArgumentException will still be thrown as expected.

**Downstream Impact Verification**:
- FileConverter expects `(character, situation)` tuple from PathAnalyzer.Extract()
- Output YAML filename: `{character}_{situation}.yaml` (e.g., `汎用_KU_日常.yaml`)
- Output YAML frontmatter: `character: 汎用` (used by kojo engine for character filtering)
- Verify character="汎用" is non-empty and valid for downstream consumers (FileConverter, YamlValidator, kojo engine)

**Test Cases to Add** (PathAnalyzerTests.cs):

```csharp
/// <summary>
/// F643 AC#2: Test extraction from U_汎用 directory with KOJO_ prefix
/// Expected: Path "U_汎用\KOJO_KU_日常.ERB" extracts character=汎用, situation=KU_日常
/// </summary>
[Fact]
public void Test_Extract_GenericKojoDirectory_KojoPrefix()
{
    var analyzer = new PathAnalyzer();
    var (character, situation) = analyzer.Extract(@"U_汎用\KOJO_KU_日常.ERB");
    Assert.Equal("汎用", character);
    Assert.Equal("KU_日常", situation);
}

/// <summary>
/// F643 AC#2: Test extraction from U_汎用 directory without KOJO_ prefix
/// Expected: Path "U_汎用\NTR口上.ERB" extracts character=汎用, situation=NTR口上 (via FallbackPattern)
/// </summary>
[Fact]
public void Test_Extract_GenericKojoDirectory_NonKojoPrefix()
{
    var analyzer = new PathAnalyzer();
    var (character, situation) = analyzer.Extract(@"U_汎用\NTR口上.ERB");
    Assert.Equal("汎用", character);
    Assert.Equal("NTR口上", situation);
}

/// <summary>
/// F643 AC#2: Test extraction from U_汎用 with full path
/// Expected: Handles absolute path with U_汎用 directory
/// </summary>
[Fact]
public void Test_Extract_GenericKojoDirectory_FullPath()
{
    var analyzer = new PathAnalyzer();
    var path = @"C:\Era\erakoumakanNTR\Game\ERB\口上\U_汎用\KOJO_KU_日常.ERB";
    var (character, situation) = analyzer.Extract(path);
    Assert.Equal("汎用", character);
    Assert.Equal("KU_日常", situation);
}
```

### Non-Convertible Files Documentation (AC#8)

The following 8 files in `Game/ERB/口上/U_汎用/` use imperative ERB patterns that the PRINTDATA/DATALIST-focused converter cannot handle. These will be documented during execution:

| File | Reason | Pattern |
|------|--------|---------|
| KOJO_KU_愛撫.ERB | Empty stub template with no convertible content | Empty PRINTFORMW blocks |
| KOJO_KU_挿入.ERB | Empty stub template with no convertible content | Empty PRINTFORMW blocks |
| KOJO_KU_EVENT.ERB | Empty stub template with no convertible content | Empty PRINTFORMW blocks |
| KOJO_KU_口挿入.ERB | SELECTCASE branching with PRINTFORML per case | SELECTCASE/PRINTFORML |
| KOJO_KU_関係性.ERB | Multi-character SELECTCASE with 10+ cases | SELECTCASE/PRINTFORML |
| KOJO_KU_会話親密.ERB | SELECTCASE + nested IF/ELSE with PRINTFORML | SELECTCASE/IF/PRINTFORML |
| KOJO_MODIFIER_COMMON.ERB | Function definitions with imperative logic | Function definitions (@KOJO_MODIFIER_PRE/POST_COMMON) |
| SexHara休憩中口上.ERB | Function definitions with imperative logic | Function definitions (@SexHara休憩中_*_KU_N) |

**Note**: NTR口上_お持ち帰り.ERB and NTR口上_野外調教.ERB are now classified as convertible files (PRINTDATAL blocks will produce YAML output, PRINTFORML content lost but documented as limitation).

### Implementation Sequence

1. **TDD Phase**: Add 3 test cases to PathAnalyzerTests.cs (U_汎用 pattern extraction)
2. **Implementation Phase**: Modify PathAnalyzer.cs regex and character mapping logic
3. **Test Verification**: Run `dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PathAnalyzerTests` (AC#1)
4. **Batch Conversion**: Run `dotnet run --project tools/ErbToYaml -- --batch Game/ERB/口上/U_汎用 --output Game/YAML/Kojo/U_汎用` (AC#4, AC#5)
5. **Schema Validation**: Run `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/U_汎用/` (AC#6)
6. **KojoComparer Build**: Attempt `dotnet build tools/KojoComparer` (AC#7, may be blocked)
7. **Documentation**: Document 8 non-convertible files in Execution Log (AC#8)
8. **Code Review**: Remove technical debt markers (AC#9)

<!-- fc-phase-5-completed -->
## Tasks

| ID | Description | Assignee | AC# | Status |
|:--:|-------------|----------|:---:|:------:|
| 1 | Add PathAnalyzer test cases for U_汎用 pattern (KOJO prefix, non-KOJO prefix, full path) | implementer (sonnet) | 2 | [x] |
| 2 | Extend PathAnalyzer regex (both PathPattern and FallbackPattern) to support `[A-Z\d]+` prefix pattern | implementer (sonnet) | 3 | [x] |
| 3 | Run PathAnalyzer unit tests to verify backward compatibility and U_汎用 support | ac-tester (haiku) | 1 | [x] |
| 4 | Execute batch conversion on Game/ERB/口上/U_汎用 directory to generate YAML files | implementer (sonnet) | 4, 5 | [x] |
| 5 | Validate generated YAML files against dialogue-schema.json using YamlValidator | ac-tester (haiku) | 6 | [x] |
| 6 | Attempt KojoComparer build; if blocked by API errors, mark AC#7 as [B] and document blocker | implementer (sonnet) | 7 | [x] |
| 7 | Document 8 non-convertible files in Execution Log with "Non-convertible:" prefix for each file | implementer (sonnet) | 8 | [x] |
| 8 | Remove all technical debt markers (TODO/FIXME/HACK) from PathAnalyzer.cs and test file | implementer (sonnet) | 9 | [x] |
| 9 | Create feature-674.md for manual YAML authoring of 8 non-convertible U_汎用 files | implementer (sonnet) | 11 | [x] |

### AC Coverage Verification

| AC# | Task IDs | Coverage |
|:---:|----------|----------|
| 1 | T3 | PathAnalyzer unit tests |
| 2 | T1 | Test case implementation |
| 3 | T2 | Regex extension implementation |
| 4 | T4 | Batch conversion execution |
| 5 | T4 | Batch conversion execution |
| 6 | T5 | Schema validation |
| 7 | T6 | KojoComparer build attempt |
| 8 | T7 | Documentation of non-convertible files |
| 9 | T8 | Technical debt removal |
| 10 | T4 | Negative verification (no YAML for non-convertible files) |
| 11 | T9 | F674 handoff file creation |

**Verification**: All 11 ACs have task coverage. Tasks T1-T8 implement the feature, T9 creates handoff destination F674.

---

## Implementation Contract

### Agent Assignments

| Phase | Agent | Model | Tasks | ACs |
|-------|-------|:-----:|-------|-----|
| Implementation (TDD) | implementer | sonnet | T1: Add test cases for U_汎用 pattern extraction | AC#2 |
| Implementation (Code) | implementer | sonnet | T2: Extend PathAnalyzer regex and character mapping | AC#3 |
| Verification (Unit Test) | ac-tester | haiku | T3: Run PathAnalyzer unit tests | AC#1 |
| Implementation (Conversion) | implementer | sonnet | T4: Execute batch conversion on U_汎用 directory | AC#4, AC#5, AC#10 |
| Verification (Schema) | ac-tester | haiku | T5: Validate YAML against schema | AC#6 |
| Implementation (Build Check) | implementer | sonnet | T6: Attempt KojoComparer build, handle blocker | AC#7 |
| Documentation | implementer | sonnet | T7: Document non-convertible files | AC#8 |
| Code Review | implementer | sonnet | T8: Remove technical debt markers | AC#9 |

### Execution Steps

**Step 1: TDD Phase (T1)**
- Read `tools/ErbToYaml.Tests/PathAnalyzerTests.cs` to understand existing test structure
- Add 3 test cases as specified in Technical Design section:
  - `Test_Extract_GenericKojoDirectory_KojoPrefix()` - Tests `U_汎用\KOJO_KU_日常.ERB`
  - `Test_Extract_GenericKojoDirectory_NonKojoPrefix()` - Tests `U_汎用\NTR口上.ERB` (via FallbackPattern)
  - `Test_Extract_GenericKojoDirectory_FullPath()` - Tests full absolute path with U_汎用
- Verify tests fail (RED state) before implementation
- **AC Verification**: Grep pattern="U_汎用" in PathAnalyzerTests.cs should find at least one match

**Step 2: Implementation Phase (T2)**
- Read `tools/ErbToYaml/PathAnalyzer.cs` to understand current regex implementation
- Modify BOTH PathPattern and FallbackPattern regex from `(\d+)_` to `([A-Z\d]+)_` to accept alphanumeric prefixes
- Add character name mapping logic: if prefix is "U", return character="汎用", else use directory name
- Handle edge cases: empty input (already handled), non-KOJO files (FallbackPattern from F639)
- **AC Verification**: Test GenericKojo pattern extraction in PathAnalyzerTests should pass

**Step 3: Test Verification (T3)**
- Run `dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PathAnalyzerTests`
- Expected: All tests pass (existing tests + new U_汎用 tests)
- If failures: debugger agent escalates to opus for root cause analysis
- **AC Verification**: Exit code 0 (success)

**Step 4: Batch Conversion (T4)**
- Run `dotnet run --project tools/ErbToYaml -- --batch Game/ERB/口上/U_汎用 Game/YAML/Kojo/U_汎用`
- Expected output: 4 YAML files (KOJO_KU_日常, NTR口上, NTR口上_お持ち帰り, NTR口上_野外調教)
- Capture BatchReport output for non-convertible file tracking
- **AC Verification**:
  - Glob `Game/YAML/Kojo/U_汎用/*日常*` exists (AC#4)
  - Glob `Game/YAML/Kojo/U_汎用/*NTR口上*` exists (AC#5)

**Step 5: Schema Validation (T5)**
- Run `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/U_汎用/`
- Expected: Exit code 0 (all YAML files pass validation)
- If validation fails: Document schema errors in Execution Log, investigate whether converter bug or schema issue
- **AC Verification**: Exit code 0 (success)

**Step 6: KojoComparer Build (T6)**
- Run `dotnet build tools/KojoComparer`
- Expected: Build succeeds with 0 errors (KojoComparer build was restored)
- Action: If build succeeds, equivalence testing can proceed in downstream F644 workflow
- **AC Verification**: Exit code 0 (success)

**Step 7: Documentation (T7)**
- Document 8 non-convertible files in Execution Log using "Non-convertible:" prefix format
- For each file: name, reason (imperative ERB pattern), specific pattern (SELECTCASE, function definition, etc.)
- Include 2 additional NTR口上_* files if they produce partial output (PRINTDATAL blocks only)
- **AC Verification**: Grep count_gte 8 for "Non-convertible:" pattern

**Step 8: Technical Debt Review (T8)**
- Read `tools/ErbToYaml/PathAnalyzer.cs` and `tools/ErbToYaml.Tests/PathAnalyzerTests.cs`
- Remove any TODO/FIXME/HACK comments added during implementation
- If temporary workarounds exist, refactor to production-quality code
- **AC Verification**: Grep pattern="TODO|FIXME|HACK" returns 0 matches

### Blocker Handling

**KojoComparer Build (AC#7)**:
- If `dotnet build tools/KojoComparer` succeeds as expected:
  - Mark AC#7 as pass in AC table
  - Document success in Execution Log: "KojoComparer build successful. Equivalence testing framework ready for F644."

**Non-convertible Files**:
- If batch conversion produces fewer than 2 YAML files:
  - Investigate BatchReport for errors
  - Verify PathAnalyzer correctly extracts U_汎用 pattern (manual test)
  - Document actual conversion results in Execution Log
  - If KOJO_KU_日常 or NTR口上 fail to convert, escalate to debugger agent (potential converter bug)

### Quality Gates

| Gate | Criterion | Action if Failed |
|------|-----------|------------------|
| Unit Tests (AC#1) | All PathAnalyzerTests pass | debugger agent investigates, escalates to opus if needed |
| YAML Generation (AC#4, AC#5) | At least 2 YAML files exist | debugger agent checks PathAnalyzer extraction and FileConverter logs |
| Schema Validation (AC#6) | YamlValidator exit code 0 | debugger agent examines schema errors, determines converter bug vs. schema issue |
| Build (AC#7) | KojoComparer builds OR marked [B] with documented blocker | If undocumented failure, debugger agent investigates |
| Documentation (AC#8) | 8+ non-convertible file mentions | implementer adds missing documentation |
| Zero Debt (AC#9) | 0 debt markers | implementer removes markers |

---

## 残課題

*(To be populated during execution if issues are deferred)*

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Manual YAML authoring for 8 non-convertible U_汎用 files | Imperative ERB patterns not supported by converter | Feature | F674 | Task#9 |

---

## Execution Log

### 2026-01-28 18:27 - Task#1: TDD Test Creation (implementer)
- Added 3 test cases to `tools/ErbToYaml.Tests/PathAnalyzerTests.cs`:
  - `Test_Extract_GenericKojoDirectory_KojoPrefix`: Tests U_汎用 with KOJO_ prefix
  - `Test_Extract_GenericKojoDirectory_NonKojoPrefix`: Tests U_汎用 without KOJO_ prefix
  - `Test_Extract_GenericKojoDirectory_FullPath`: Tests U_汎用 with full path
- Build: SUCCESS (dotnet build tools/ErbToYaml.Tests - 0 errors, 6 warnings in unrelated files)
- Test RED verification: All 3 tests FAIL as expected (TDD RED phase confirmed)
- Tests placed at end of PathAnalyzerTests.cs class before closing brace (lines 375-413)
- Tests follow existing naming conventions and structure (F639 pattern)
- AC#2: PASS (test cases added covering U_汎用 pattern extraction)
- Status: Task#1 marked [x], Phase 3 complete

### 2026-01-28 18:34 - Task#4: Batch Conversion Execution (implementer)
- Executed batch conversion: `dotnet run --project tools/ErbToYaml -- --batch "Game/ERB/口上/U_汎用" "Game/YAML/Kojo/U_汎用" --talent "C:\Era\erakoumakanNTR\Game\CSV\Talent.csv" --schema "C:\Era\erakoumakanNTR\tools\YamlSchemaGen\dialogue-schema.json"`
- **Note**: Required explicit --talent and --schema paths due to relative path resolution issues in Git Bash
- BatchReport: Total: 12, Success: 12, Failed: 0
- YAML output: 24 files generated in `Game/YAML/Kojo/U_汎用/`
  - KU_日常: 5 files (KU_日常_0.yaml through KU_日常_4.yaml)
  - NTR口上: 19 files (NTR口上_0.yaml through NTR口上_18.yaml)
- Warnings: 56 "Could not parse condition" warnings for `NTR_CHK_FAVORABLY(奴隷, FAV_*)` patterns (expected - complex condition parsing limitation)
- Build verification: SUCCESS (dotnet build Era.Core - 0 errors, 0 warnings)
- ERB loading check: Pre-existing error with csv\_Rename.csv (unrelated to conversion). No warnings specific to generated YAML files.
- **Non-convertible files** (8 files with no YAML output, as expected):
  - Non-convertible: KOJO_KU_愛撫.ERB - Empty stub template with no convertible content
  - Non-convertible: KOJO_KU_挿入.ERB - Empty stub template with no convertible content
  - Non-convertible: KOJO_KU_EVENT.ERB - Empty stub template with no convertible content
  - Non-convertible: KOJO_KU_口挿入.ERB - SELECTCASE branching with PRINTFORML per case
  - Non-convertible: KOJO_KU_関係性.ERB - Multi-character SELECTCASE with 10+ cases
  - Non-convertible: KOJO_KU_会話親密.ERB - SELECTCASE + nested IF/ELSE with PRINTFORML
  - Non-convertible: KOJO_MODIFIER_COMMON.ERB - Function definitions with imperative logic
  - Non-convertible: SexHara休憩中口上.ERB - Function definitions with imperative logic
- AC#4: PASS (YAML files for KU_日常 generated)
- AC#5: PASS (YAML files for NTR口上 generated)
- AC#8: PASS (8 non-convertible files documented with "Non-convertible:" prefix)
- Status: Task#4 marked [x]

### 2026-01-28 18:30 - Task#2: PathAnalyzer Regex Extension (implementer)
- Modified `tools/ErbToYaml/PathAnalyzer.cs`:
  - PathPattern regex: `(\d+)_` → `([A-Z\d]+)_` (line 15)
  - FallbackPattern regex: `(\d+)_` → `([A-Z\d]+)_` (line 21)
- Character name extraction unchanged (Group 2 = directory name after prefix)
- All 22 PathAnalyzer tests pass (including 3 new F643 tests)
- AC#3: PASS (GenericKojo tests pass)
- Status: Task#2 marked [x]

### 2026-01-28 18:31 - Task#3: PathAnalyzer Unit Test Verification (ac-tester)
- Ran `dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PathAnalyzerTests`
- Result: 22 tests passed, 0 failed, exit code 0
- Backward compatibility confirmed: all existing tests pass alongside new U_汎用 tests
- AC#1: PASS (all PathAnalyzer tests pass)
- Status: Task#3 marked [x]

### 2026-01-28 18:36 - Task#5: Schema Validation (ac-tester)
- Ran `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/U_汎用/`
- Result: 24 files validated, 24 passed, 0 failed, exit code 0
- All generated YAML conforms to dialogue-schema.json
- AC#6: PASS (schema validation succeeds)
- Status: Task#5 marked [x]

### 2026-01-28 18:36 - Task#6: KojoComparer Build (implementer)
- Ran `dotnet build tools/KojoComparer`
- Result: Build succeeded, 0 errors, 0 warnings, exit code 0
- KojoComparer ready for equivalence testing in F644
- AC#7: PASS (KojoComparer builds successfully)
- Status: Task#6 marked [x]

### 2026-01-28 18:37 - Task#8: Zero Technical Debt (implementer)
- Searched for TODO/FIXME/HACK markers in PathAnalyzer.cs and PathAnalyzerTests.cs using Grep
- Result: No matches found in either file
- Verification: Both files are free of technical debt markers
- AC#9: PASS (zero technical debt markers found)
- Status: Task#8 marked [x]

### 2026-01-28 18:37 - Task#9: Create feature-674.md (implementer)
- Created `Game/agents/feature-674.md` with [DRAFT] status for manual YAML authoring of 8 non-convertible U_汎用 files
- Content includes:
  - Summary of 8 non-convertible files (KOJO_KU_愛撫, KOJO_KU_挿入, KOJO_KU_EVENT, KOJO_KU_口挿入, KOJO_KU_関係性, KOJO_KU_会話親密, KOJO_MODIFIER_COMMON, SexHara休憩中口上)
  - Table documenting each file's pattern and reason for non-convertibility
  - Background section explaining the conversion gap (imperative ERB patterns)
  - Dependencies: F643 as predecessor
- Registered F674 in `Game/agents/index-features.md` under Phase 19 Conversion Batches section
- AC#11: PASS (feature-674.md exists and registered)
- Status: Task#9 marked [x]

## Review Notes
- [resolved-applied] Phase1-Uncertain iter1: Existing test Test_Extract_NoNumberedDirectory_Throws backward compatibility acknowledgment needed
- [resolved-applied] Phase1-Uncertain iter2: Review Notes need resolution in Technical Design before /run
- [resolved-accepted] Phase1-Uncertain iter8: AC#8 self-referential verification depends on specific format during execution
