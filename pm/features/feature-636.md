# Feature 636: Meiling Kojo Conversion

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

Convert Meiling (1_美鈴) kojo ERB files to YAML format with equivalence verification.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

11 ERB files in Game/ERB/口上/1_美鈴/ need conversion to YAML for the new kojo engine.

### Goal (What to Achieve)

1. Convert 11 Meiling kojo files using batch converter
2. Verify YAML schema validity via YamlValidator CLI (equivalence deferred to F644)
3. Zero technical debt (no TODO/FIXME/HACK)

**Final Result**: All 11 files processed by batch converter (80 YAML files). 9/11 ERB files produced YAML output; 2 files (KOJO_K1_乳首責め.ERB, SexHara休憩中口上.ERB) contain no DATALIST/PRINTDATA blocks and produced zero output (converter reports success as no errors occurred). Initial execution (2026-01-27) converted 7/11 files; 4 files were blocked by PathAnalyzer naming convention. After F639 blocker resolution (FallbackPattern support), RESUME execution (2026-01-28) successfully processed all 11 files (exit code 0).

---


## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-633.md](feature-633.md) - PRINTDATA...ENDDATA parser (predecessor)
- [feature-634.md](feature-634.md) - Batch Conversion Tool (predecessor)
- [feature-635.md](feature-635.md) - Pilot Conversion (related)
- [feature-650.md](feature-650.md) - ErbParser STRDATA Edge Case Fix (blocker resolution)
- [feature-637.md](feature-637.md) - Other character conversion (sibling)
- [feature-638.md](feature-638.md) - Other character conversion (sibling)
- [feature-639.md](feature-639.md) - Other character conversion (sibling)
- [feature-640.md](feature-640.md) - Other character conversion (sibling)
- [feature-641.md](feature-641.md) - Other character conversion (sibling)
- [feature-642.md](feature-642.md) - Other character conversion (sibling)
- [feature-643.md](feature-643.md) - Other character conversion (sibling)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework
- [feature-671.md](feature-671.md) - PRINTDATAL metadata (related)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Line 4268

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 11 ERB files in Game/ERB/口上/1_美鈴/ are not yet in YAML format, blocking the new kojo engine pipeline
2. Why: The batch conversion tool (F634) was only recently completed; no character directories have been converted yet
3. Why: Phase 19 planned conversion as a sequential pipeline: tooling first (F633/F634), then per-character conversion batches (F636-F643)
4. Why: Each character directory contains unique ERB structures (DATALIST, PRINTDATA, PRINTDATAL, IF/ELSEIF conditional wrappers) requiring tool-assisted conversion with verification
5. Why: ERB-to-YAML migration is a structural transformation (procedural ERB with embedded conditionals to declarative YAML with branch metadata), requiring automated conversion plus equivalence verification to prevent semantic drift

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 11 Meiling ERB files not in YAML format | Phase 19 conversion pipeline was not yet available; F634 batch converter is now [DONE] but no conversion runs have been executed |
| KojoComparer cannot verify equivalence yet | KojoComparer has build errors (KojoEngine.Render API mismatch); F644 Equivalence Testing Framework is [DRAFT] |

### Conclusion

The root cause is that the conversion pipeline (F634 batch tool) is now available but has not been executed on any character directory. F636 is the first conversion batch, serving as a pilot for the remaining 7 character batches (F637-F643). The 11 ERB files in 1_美鈴/ contain a mix of:
- **DATALIST** blocks (e.g., KOJO_K1_愛撫.ERB with TALENT 4-tier branching, 16 DATALISTs)
- **PRINTDATAL/PRINTDATA** blocks (e.g., KOJO_K1_日常.ERB, NTR口上.ERB)
- **IF/ELSEIF conditional wrappers** around PRINTDATA blocks (e.g., NTR口上.ERB with nested TALENT checks)
- **PRINTFORML statements** interspersed with PRINTDATA (NTR口上.ERB) -- these are outside PRINTDATA blocks and may not be captured by the converter

A secondary risk is that KojoComparer currently does not build (CS1061: KojoEngine lacks Render method), so equivalence verification via KojoComparer is not currently possible. F644 (Equivalence Testing Framework) is [DRAFT] and depends on F636-F643 completing first. For F636, equivalence verification must rely on manual spot-checking or schema validation only.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Predecessor (batch tool) | Batch converter with --batch mode, FileConverter, PrintDataConverter, PathAnalyzer. All 48 tests pass. |
| F633 | [DONE] | Predecessor (parser) | PRINTDATA...ENDDATA parser extension. PrintDataNode with GetDataForms() helper. |
| F635 | [PROPOSED] | Related (parallelization) | Conversion parallelization -- not required for F636 (sequential is sufficient for 11 files) |
| F637-F643 | [DRAFT] | Sibling batches | Other character conversion batches. F636 serves as pilot -- lessons learned feed into F637-F643 |
| F644 | [DRAFT] | Downstream (equivalence) | Equivalence Testing Framework. Depends on F636-F643 outputs. Currently blocked. |
| F671 | [DRAFT] | Related (variant) | PrintDataNode Variant metadata mapping (PRINTDATAL vs PRINTDATA display behavior). F634 converts content only. |

### Pattern Analysis

F636 is the first per-character conversion batch. It serves as a pilot that will establish the conversion pattern for F637-F643. Key patterns to track:
- Conversion success rate (how many of 11 files convert cleanly?)
- Common failure modes (unsupported ERB constructs?)
- PRINTFORML statements outside PRINTDATA blocks (converter may not capture these)
- Schema validation pass rate
- Any PRINTDATA variant issues (PRINTDATAL vs PRINTDATA -- deferred to F671)

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | F634 batch converter [DONE] with 48 tests passing. All building blocks (ErbParser, DatalistConverter, PrintDataConverter, PathAnalyzer, BatchConverter) exist and are tested. |
| Scope is realistic | YES | 11 ERB files in single directory. Batch converter handles directory traversal, per-file conversion, error reporting. Expected to be a single batch run + verification. |
| No blocking constraints | PARTIAL | F634 is [DONE] (no blocker). KojoComparer has build errors (KojoEngine API mismatch), so equivalence testing via KojoComparer is not available. Schema validation via ErbToYaml's built-in validator works. YamlValidator CLI also available as fallback. |

**Verdict**: FEASIBLE

The conversion itself is feasible via the F634 batch tool. Equivalence verification via KojoComparer is not available due to build errors, but schema validation provides a partial quality gate. Goal #2 ("Verify equivalence with KojoComparer") may need to be scoped down to "Verify YAML schema validity" for F636, with full equivalence deferred to F644.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool with --batch mode, FileConverter, PrintDataConverter, PathAnalyzer |
| Predecessor | F633 | [DONE] | PRINTDATA Parser Extension providing PrintDataNode AST |
| Related | F635 | [PROPOSED] | Conversion Parallelization (not required for F636) |
| Related | F644 | [DRAFT] | Equivalence Testing Framework (downstream consumer of F636 output) |
| Related | F671 | [DRAFT] | PrintDataNode Variant Metadata Mapping (display behavior semantics) |
| ~~Blocker~~ | F673 | [DONE] | ~~PathAnalyzer Enhancement~~ **RESOLVED**: F639 commit 7bd308e added FallbackPattern |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| ErbToYaml (tools/ErbToYaml/) | Build-time | Low | F634 [DONE], builds successfully, 48 tests pass |
| dialogue-schema.json (tools/YamlSchemaGen/) | Runtime | Low | Schema file exists, used by FileConverter for validation |
| Talent.csv (Game/CSV/) | Runtime | Low | Required by TalentCsvLoader for condition resolution in DatalistConverter |
| YamlValidator (tools/YamlValidator/) | Build-time | Low | Backup validation tool. Builds and runs independently. |
| KojoComparer (tools/KojoComparer/) | Build-time | HIGH | Does NOT build -- CS1061 error: KojoEngine missing Render method. Cannot use for equivalence testing. |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F644 Equivalence Testing Framework | HIGH | Will consume YAML files generated by F636 to test ERB==YAML equivalence |
| F637-F643 sibling batches | MEDIUM | F636 pilot results inform conversion approach for remaining characters |
| Game/YAML/Kojo/ directory | LOW | Generated YAML files placed here for runtime use by kojo engine |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Game/YAML/Kojo/1_美鈴/*.yaml | Create | 11+ YAML files generated from ERB conversion (may produce multiple YAML per ERB) |
| Game/agents/feature-636.md | Update | Execution log, AC status updates |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| KojoComparer does not build | YamlRunner.cs CS1061 error (KojoEngine API) | HIGH - Cannot verify ERB==YAML equivalence via automated tool. Must rely on schema validation only. |
| PRINTFORML outside PRINTDATA | NTR口上.ERB has PRINTFORML mixed with PRINTDATA blocks | MEDIUM - FileConverter processes PRINTDATA/DATALIST nodes only. PRINTFORML outside these blocks may be lost in conversion. |
| PrintDataNode Variant semantics | F671 deferred | LOW - PRINTDATAL vs PRINTDATA display behavior not preserved in YAML. Content is correct but display variant metadata is missing. |
| ERB file count verified | ls output shows exactly 11 files | LOW - Count matches feature summary. No discrepancy. |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Some ERB files contain unsupported constructs (PRINTFORML outside PRINTDATA) | High | Medium | BatchConverter reports per-file failures. NTR口上.ERB likely affected. Review converter output for completeness. |
| Schema validation rejects some converted YAML | Medium | Medium | FileConverter already validates against schema (F634 AC#16). Failed files reported in BatchReport. |
| KojoComparer unavailable for equivalence testing | Certain | High | Defer equivalence to F644. Use schema validation + manual spot-check for F636. |
| Pilot reveals systematic conversion issues affecting F637-F643 | Medium | High | Document lessons learned in F636 execution log. Adjust approach for sibling batches. |
| Multiple YAML outputs per ERB (indexed suffixes) cause confusion | Low | Low | Document naming convention. FileConverter handles this per F634 design. |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

Philosophy: "Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format"

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Convert 11 Meiling kojo files using batch converter" | Batch converter must execute on 1_美鈴/ directory and produce YAML output | AC#1, AC#2 |
| "Verify YAML schema validity via YamlValidator CLI (equivalence deferred to F644)" | All generated YAML files must pass schema validation | AC#3, AC#4 |
| "Zero technical debt (no TODO/FIXME/HACK)" | No debt markers in generated YAML or conversion logs | AC#5 |
| "11 ERB files" (exact count verified by ls) | Conversion must process exactly 11 input files | AC#6 |
| "Automated migration" (batch, not manual) | Batch converter exit code and summary report confirm automated run | AC#1 |
| Pilot for F637-F643 (pattern analysis) | Conversion failures documented; lessons learned recorded | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Batch converter exits successfully | exit_code | `dotnet run --project tools/ErbToYaml -- --batch Game/ERB/口上/1_美鈴 Game/YAML/Kojo/1_美鈴` | succeeds | exit code 0 | [x] |
| 2 | YAML output files exist | file | Glob `Game/YAML/Kojo/1_美鈴/*.yaml` | gte | 11 | [x] |
| 3 | Batch report shows 11 files processed | output | AC#1 stdout output | contains | "Total: 11" | [x] |
| 4 | YamlValidator passes all generated YAML | exit_code | `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/1_美鈴` | succeeds | exit code 0 | [x] |
| 5 | No technical debt markers in generated YAML | file | Grep `TODO|FIXME|HACK` in `Game/YAML/Kojo/1_美鈴/*.yaml` | not_contains | "TODO|FIXME|HACK" | [x] |
| 6 | All 11 ERB source files consumed | file | Glob `Game/ERB/口上/1_美鈴/*.ERB` | count_equals | 11 | [x] |
| 7 | Execution log updated with conversion results (success or failure) | file | Grep `2026-0[1-9]` in feature-636.md Execution Log | contains | "2026-0" | [x] |
| 8 | ErbToYaml project builds | build | `dotnet build tools/ErbToYaml` | succeeds | - | [x] |
| 9 | YamlValidator project builds | build | `dotnet build tools/YamlValidator` | succeeds | - | [x] |
| 10 | All YAML files contain content (front-matter check) | composite | Count YAML files: `Glob("Game/YAML/Kojo/1_美鈴/*.yaml")`, Count unique front-matter files: `Grep("^---", path="Game/YAML/Kojo/1_美鈴", glob="*.yaml") | wc -l`, verify counts equal | count_equals | file_count == front_matter_count | [x] |
| 11 | Batch report shows no conversion failures | output | AC#1 stdout output | contains | "Failed: 0" | [x] |
| 12 | No schema validation errors in output | output | `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/1_美鈴` | not_contains | "ERROR" | [x] |

### AC Details

**AC#1: Batch converter exits successfully**
- Runs `dotnet run --project tools/ErbToYaml -- --batch Game/ERB/口上/1_美鈴 Game/YAML/Kojo/1_美鈴`
- Exit code 0 means all 11 files converted without error
- If exit code 1, check BatchReport failures -- some ERB files may have unsupported constructs (e.g., PRINTFORML outside PRINTDATA in NTR口上.ERB)
- Edge case: If some files fail but others succeed, exit code is 1 (partial failure). This AC requires all 11 to succeed for PASS.

**AC#2: YAML output files exist**
- Glob pattern `Game/YAML/Kojo/1_美鈴/*.yaml` must find at least 11 files
- Uses `gte` (not `equals`) because one ERB file may produce multiple YAML files (e.g., indexed outputs for multi-function ERB files)
- FileConverter per F634 design handles splitting and naming

**AC#3: Batch report shows 11 files processed**
- The batch converter stdout includes `Total: 11, Success: N, Failed: M`
- Verifies the converter discovered and attempted all 11 ERB files
- This is a count verification on the input side (ERB files found), complementing AC#2 (output side)

**AC#4: YamlValidator passes all generated YAML**
- Independent schema validation using YamlValidator CLI tool
- Provides a second validation layer beyond FileConverter's built-in validation
- Validates against dialogue-schema.json

**AC#5: No technical debt markers in generated YAML**
- Grep for `TODO|FIXME|HACK` across all generated YAML files
- Goal #4 explicitly requires zero technical debt
- Generated content should be clean; any markers would indicate converter issues

**AC#6: All 11 ERB source files consumed**
- Pre-condition verification: confirms source directory contains exactly 11 ERB files
- Prevents silent scope drift (e.g., if files were added/removed since feature creation)
- Verified by ls on 2026-01-27: KOJO_K1_EVENT.ERB, KOJO_K1_愛撫.ERB, KOJO_K1_会話親密.ERB, KOJO_K1_口挿入.ERB, KOJO_K1_挿入.ERB, KOJO_K1_日常.ERB, KOJO_K1_乳首責め.ERB, NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB

**AC#7: Conversion failures documented in execution log**
- If any files fail conversion (AC#1 exit code 1), failure details must be recorded in the Execution Log section
- Risk: NTR口上.ERB contains PRINTFORML outside PRINTDATA blocks, which may not be captured by the converter
- This AC is conditional: if all files succeed (AC#1 PASS), this AC is also PASS (no failures to document). If AC#1 fails, failures must appear in the log.

**AC#8: ErbToYaml project builds**
- Build prerequisite for AC#1 and AC#3
- Confirms F634 tool is still in working state

**AC#9: YamlValidator project builds**
- Build prerequisite for AC#4
- Independent build verification

**AC#10: No YAML files with empty content**
- Every generated YAML file must contain actual content (not be empty or whitespace-only)
- Compares Glob file count against Grep match count for `^---$` pattern
- Assumption: F634 converter always produces YAML files starting with `---` front-matter marker (single occurrence per file)
- If this assumption fails, use alternative file size check (> 0 bytes) or per-file validation script

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature uses the **batch converter workflow** established by F634. The implementation is a sequential execution of the ErbToYaml batch converter with validation steps:

1. **Pre-flight checks** (AC#6, AC#8, AC#9): Verify source files exist, build prerequisites
2. **Batch conversion** (AC#1, AC#2, AC#3): Execute `dotnet run --project tools/ErbToYaml -- --batch` with source and destination paths
3. **Schema validation** (AC#4): Run YamlValidator CLI on generated YAML files
4. **Quality checks** (AC#5, AC#10): Grep for debt markers and empty files
5. **Failure documentation** (AC#7): Record any conversion failures in execution log

**Rationale**: This approach leverages the F634 batch converter infrastructure that has already been validated (48 tests passing). No new tooling is required. The converter handles directory traversal, per-file conversion, error reporting, and schema validation (AC#4 duplicates converter's built-in validation for defense-in-depth).

**How this satisfies the ACs**: Each AC maps to a discrete verification step. Build checks (AC#8, AC#9) run first to fail fast. Batch converter execution (AC#1) produces files (AC#2, AC#3). Independent validation layers (AC#4, AC#5, AC#10) provide quality gates. Failure documentation (AC#7) is conditional on AC#1 result.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Run `dotnet run --project tools/ErbToYaml -- --batch Game/ERB/口上/1_美鈴 Game/YAML/Kojo/1_美鈴` and verify exit code 0 |
| 2 | Use Glob tool to count files matching `Game/YAML/Kojo/1_美鈴/*.yaml`, verify count >= 11 |
| 3 | Parse batch converter stdout for "Total: 11" line (BatchReport summary) |
| 4 | Run `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/1_美鈴` and verify exit code 0 |
| 5 | Use Grep tool with pattern `TODO\|FIXME\|HACK` across `Game/YAML/Kojo/1_美鈴/*.yaml`, verify no matches |
| 6 | Use Glob tool to count files matching `Game/ERB/口上/1_美鈴/*.ERB`, verify count == 11 |
| 7 | If AC#1 fails (exit code != 0), extract failure details from stdout and record in Execution Log section |
| 8 | Run `dotnet build tools/ErbToYaml` and verify exit code 0 |
| 9 | Run `dotnet build tools/YamlValidator` and verify exit code 0 |
| 10 | Use Grep tool to verify all generated YAML files contain front-matter (at least 11 matches) |
| 11 | Parse batch converter stdout for "Failed: 0" line to confirm no conversion failures |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Conversion method | Manual ERB-to-YAML rewrite vs F634 batch converter | F634 batch converter | Manual rewrite would introduce human error and violate "automated migration" philosophy. F634 tool is [DONE] with 48 tests passing. |
| Equivalence verification | KojoComparer vs schema validation only | Schema validation only (YamlValidator) | KojoComparer does not build (CS1061 error). Equivalence deferred to F644. Schema validation provides structural correctness. |
| Failure handling | Stop on first failure vs continue and report all | Continue and report all | Batch converter design (F634) continues processing all files and produces BatchReport with per-file status. This maximizes information for pilot analysis. |
| Output directory creation | Manual `mkdir` vs let converter create | Let converter create | PathAnalyzer in F634 handles output directory creation (CreateOutputDirectory call in BatchConverter.cs). No manual setup needed. |
| PRINTFORML handling | Pre-process ERB to wrap PRINTFORML in PRINTDATA vs accept incomplete conversion | Accept incomplete conversion, document as known limitation | PRINTFORML outside PRINTDATA blocks (NTR口上.ERB) is out of scope for F634 converter. Document in AC#7 execution log. Risk: partial content loss. |

### Execution Sequence

```
1. AC#6: Verify source files exist (11 ERB files)
   └─> If count != 11, FAIL with scope drift error

2. AC#8, AC#9: Build ErbToYaml and YamlValidator
   └─> If either fails, FAIL with build error

3. AC#1: Run batch converter
   └─> If exit code 0: Continue to AC#2-6
   └─> If exit code 1: Continue to AC#2-6, but AC#7 triggers failure documentation

4. AC#2: Count generated YAML files (>= 11)
   └─> If count < 11, FAIL with output missing error

5. AC#3: Parse BatchReport for "Total: 11"
   └─> Verifies converter discovered all source files

6. AC#4: Run YamlValidator on generated files
   └─> If validation fails, FAIL with schema error

7. AC#5: Grep for technical debt markers
   └─> If matches found, FAIL with debt marker error

8. AC#10: Grep for YAML front-matter (verify non-empty)
   └─> If any file missing marker, FAIL with empty file error

9. AC#7: Document failures (conditional)
   └─> If AC#1 failed, extract failure details and record in log
```

### Known Limitations

| Limitation | Impact | Tracking |
|------------|--------|----------|
| PRINTFORML outside PRINTDATA blocks may be lost | NTR口上.ERB contains mixed PRINTFORML/PRINTDATA. Converter processes PRINTDATA blocks only. | Document in AC#7 execution log. Full fix requires pre-processor or manual intervention (deferred). |
| PRINTDATAL variant metadata not preserved | F671 deferred. Display behavior semantics (PRINTDATAL vs PRINTDATA) not encoded in YAML. | Accept for F636. F671 will add variant metadata layer. |
| No ERB==YAML equivalence testing | KojoComparer does not build. Cannot verify semantic equivalence. | Defer to F644 Equivalence Testing Framework. |

### No New Interfaces / Data Structures

This feature uses existing infrastructure from F634. No new data structures required. All interfaces (BatchConverter, FileConverter, PrintDataConverter, DatalistConverter, PathAnalyzer, SchemaValidator) are already defined in tools/ErbToYaml/.

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 6 | Verify 11 ERB source files exist in Game/ERB/口上/1_美鈴/ | [x] |
| 2 | 8 | Build ErbToYaml project | [x] |
| 3 | 9 | Build YamlValidator project | [x] |
| 4 | 1 | Execute batch converter on 1_美鈴 directory | [x] |
| 5 | 2 | Verify YAML output files exist (>= 11) | [x] |
| 6 | 3 | Verify batch report shows 11 files processed | [x] |
| 7 | 4 | Run YamlValidator on generated YAML files | [x] |
| 8 | 5 | Check for technical debt markers in generated YAML | [x] |
| 9 | 10 | Verify no YAML files with empty content | [x] |
| 10 | 7 | Document conversion failures in execution log | [x] |
| 11 | 11 | Verify no conversion failures in batch report | [x] |
| 12 | 12 | Verify no schema validation errors in output | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to exactly one Task for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Execution Plan

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T12 | ACs, Technical Design | Conversion execution and validation |

### Execution Sequence

**Phase 1: Pre-flight Checks (T1-T3)**

1. **T1: Verify source files exist**
   - Command: `Glob("Game/ERB/口上/1_美鈴/*.ERB")`
   - Expected: Count == 11
   - On failure: STOP with scope drift error

2. **T2: Build ErbToYaml**
   - Command: `dotnet build tools/ErbToYaml`
   - Expected: Exit code 0
   - On failure: STOP with build error

3. **T3: Build YamlValidator**
   - Command: `dotnet build tools/YamlValidator`
   - Expected: Exit code 0
   - On failure: STOP with build error

**Phase 2: Batch Conversion (T4)**

4. **T4: Execute batch converter**
   - Command: `dotnet run --project tools/ErbToYaml -- --batch Game/ERB/口上/1_美鈴 Game/YAML/Kojo/1_美鈴`
   - Expected: Exit code 0, BatchReport shows "Total: 11"
   - On exit code 1: Continue to validation but trigger T10 (failure documentation)
   - Capture stdout for BatchReport parsing in T6

**Phase 3: Validation (T5-T9)**

5. **T5: Verify YAML output exists**
   - Command: `Glob("Game/YAML/Kojo/1_美鈴/*.yaml")`
   - Expected: Count >= 11
   - On failure: STOP with missing output error

6. **T6: Verify batch report**
   - Action: Parse T4 stdout for "Total: 11" line
   - Expected: Line exists with correct count
   - On failure: STOP with incomplete processing error

7. **T7: Run YamlValidator**
   - Command: `dotnet run --project tools/YamlValidator -- Game/YAML/Kojo/1_美鈴`
   - Expected: Exit code 0
   - On failure: STOP with schema validation error

8. **T8: Check for technical debt markers**
   - Command: `Grep("TODO\|FIXME\|HACK", glob="*.yaml", path="Game/YAML/Kojo/1_美鈴")`
   - Expected: No matches
   - On failure: STOP with debt marker error

9. **T9: Verify non-empty YAML files**
   - Command: `Grep("^---$", glob="*.yaml", path="Game/YAML/Kojo/1_美鈴")`
   - Expected: All files match (every file has front-matter)
   - On failure: STOP with empty file error

**Phase 4: Failure Documentation (T10)**

10. **T10: Document failures (conditional)**
    - Condition: If T4 exit code != 0
    - Action: Extract failure details from T4 stdout, parse BatchReport failed files list
    - Record in Execution Log section with format:
      ```
      | Date | Phase | Note |
      | 2026-01-27 | Conversion | Failed files: [list]. Error: [details] |
      ```
    - If T4 succeeded (exit code 0), record success:
      ```
      | Date | Phase | Note |
      | 2026-01-27 | Conversion | All 11 files converted successfully |
      ```

### Constraints

1. **PRINTFORML limitation**: PRINTFORML statements outside PRINTDATA blocks (e.g., NTR口上.ERB) may not be captured by the converter. This is a known limitation of F634 design. Document in T10 if NTR口上.ERB fails.

2. **No KojoComparer equivalence testing**: KojoComparer does not build (CS1061 error). Equivalence verification deferred to F644. This feature uses schema validation only.

3. **Sequential execution required**: Tasks T1-T3 must complete successfully before T4. Tasks T5-T9 depend on T4 completion. Do not parallelize.

### Pre-conditions

- F634 Batch Conversion Tool is [DONE] and builds successfully
- Source directory `Game/ERB/口上/1_美鈴/` contains exactly 11 ERB files
- Output directory `Game/YAML/Kojo/1_美鈴/` either does not exist or will be created by PathAnalyzer
- dialogue-schema.json exists in tools/YamlSchemaGen/

### Success Criteria

- All 12 ACs marked as [x] in AC Definition Table
- Exit code 0 from T2, T3, T4, T7
- Count verification passes for T1 (== 11), T5 (>= 11)
- Grep checks pass for T8 (no matches), T9 (all match)
- Execution Log updated with conversion results (T10)

### Rollback Plan

This feature generates new files only (no modification of existing ERB files). If issues arise:

1. **During execution**: STOP and report error. Do not proceed to next phase.
2. **After completion**: If generated YAML files are incorrect:
   - Delete `Game/YAML/Kojo/1_美鈴/` directory
   - Document failure in execution log
   - Create follow-up feature for investigation if systematic issues found

**Note**: No git revert needed as this feature adds files only (not modifying existing code). Generated YAML files can be safely deleted and regenerated.

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| PRINTFORML outside PRINTDATA blocks may be lost | NTR口上.ERB contains mixed PRINTFORML/PRINTDATA, converter processes PRINTDATA blocks only | F644 (Equivalence Testing Framework) | F644 | F644 exists as [DRAFT] - tracked there |
| PRINTDATAL variant metadata not preserved | F671 deferred, display behavior semantics not encoded in YAML | F671 (PrintDataNode Variant Metadata Mapping) | F671 | F671 exists as [DRAFT] - tracked there |
| ERB==YAML semantic equivalence verification | KojoComparer does not build, cannot verify semantic equivalence | F644 (Equivalence Testing Framework) | F644 | F644 exists as [DRAFT] - tracked there |
| Lessons learned from pilot conversion | Document conversion patterns, failure modes for sibling batches | F637-F643 (sibling character batches) | F637-F643 | T10 documents in execution log for sibling reference |
| PRINTFORML content recovery if conversion loss confirmed | F634 converter processes only PRINTDATA blocks, actual fix requires converter enhancement or manual recovery | F644 (Equivalence Testing Framework) | F644 | F644 will detect content loss; if confirmed, F644 execution creates follow-up feature for recovery |
| ~~4 ERB files failed PathAnalyzer naming convention~~ | **RESOLVED**: F639 commit 7bd308e added FallbackPattern. Re-run batch conversion to process these 4 files | N/A (resolved) | N/A | **DONE** - PathAnalyzer now supports non-KOJO prefix files |
| 2 ERB files produced no YAML output (zero convertible blocks) | KOJO_K1_乳首責め.ERB and SexHara休憩中口上.ERB contain no DATALIST/PRINTDATA blocks. Converter reports success but generates no output. Content uses PRINTFORML or other non-convertible constructs only. | F644 (Equivalence Testing Framework) | F644 | F644 will detect missing YAML coverage for these files |

---

## Review Notes
- [applied] Phase1-Uncertain iter1: AC#10 logic imprecise - gte matcher with potential double-counting. Fixed by changing to composite verification with count_equals comparing Glob file count against unique front-matter file count.

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-28 07:08 | Pre-flight | T1: 11 ERB source files verified. T2: ErbToYaml builds successfully. T3: YamlValidator builds successfully. |
| 2026-01-28 07:08 | Conversion | T4: Batch converter executed. Exit code: 1 (partial failure). Total: 11, Success: 7, Failed: 4. Failed files: NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB. Failure reason: Path does not match expected pattern (N_CharacterName/KOJO_Situation.ERB). These files lack the KOJO_ prefix required by PathAnalyzer. |
| 2026-01-28 07:08 | Validation | T5: 67 YAML files generated (>= 11 requirement met). T6: Batch report confirms "Total: 11" processed. T7: YamlValidator passed all 67 files (exit code 0). T8: No technical debt markers found. T9: All YAML files contain valid content. |
| 2026-01-28 07:08 | Completion | T10-T12: Execution log updated. AC#1 and AC#11 failed due to 4 files not matching naming convention. However, all successfully converted files (7/11) produced schema-valid YAML (67 files total). Zero technical debt. Pilot conversion partially successful. |
| 2026-01-28 | DEVIATION | Batch converter | exit code 1 | 4/11 files failed: NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB. PathAnalyzer requires KOJO_ prefix pattern. |
| 2026-01-28 | DEVIATION | feature-reviewer | NEEDS_REVISION | Missing Mandatory Handoff for 4 failed files. Fixing. |
| 2026-01-28 10:47 | RESUME | Pre-flight | T1: 11 ERB source files verified. T2: ErbToYaml builds successfully (0 warnings, 0 errors). T3: YamlValidator builds successfully (0 warnings, 0 errors). |
| 2026-01-28 10:47 | RESUME | Conversion | T4: Batch converter executed with explicit paths. Exit code: 0 (full success). Total: 11, Success: 11, Failed: 0. All files converted successfully including previously blocked NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB. Blocker resolution (F639 FallbackPattern) confirmed working. |
| 2026-01-28 10:47 | RESUME | Validation | T5: 80 YAML files generated (>> 11 requirement). T6: Batch report confirms "Total: 11, Success: 11, Failed: 0". T7: YamlValidator passed all 80 files (exit code 0). T8: No technical debt markers found. T9: All YAML files non-empty (0 empty files). T11: Batch report confirms "Failed: 0". T12: No "ERROR" in YamlValidator output. |
| 2026-01-28 10:47 | RESUME | Completion | T10: Execution log updated. All 12 ACs passed. 11 ERB files processed → 80 YAML files (9/11 ERB files produced output; 2 files have no DATALIST/PRINTDATA blocks). Zero technical debt. All files schema-valid. |
| 2026-01-28 | DEVIATION | feature-reviewer | NEEDS_REVISION | Post-review: (1) 2 ERB files (KOJO_K1_乳首責め.ERB, SexHara休憩中口上.ERB) have no YAML output despite batch success — confirmed no DATALIST/PRINTDATA blocks in source. (2) YAML file count was 80, not 85. Both corrected. |
