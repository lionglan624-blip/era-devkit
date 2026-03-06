# Feature 641: Flandre Kojo Conversion

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

Convert Flandre (6_フラン) kojo ERB files to YAML format with equivalence verification.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

10 ERB files in Game/ERB/口上/6_フラン/ need conversion to YAML for the new kojo engine.

### Goal (What to Achieve)

1. Run batch converter on 10 Flandre kojo files (6 KOJO-prefixed convert successfully, 4 non-KOJO report expected failures)
2. Verify equivalence with KojoComparer
3. Validate YAML against schema
4. Zero technical debt (no TODO/FIXME/HACK)

---

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-633.md](feature-633.md) - PRINTDATA Parser Extension (Predecessor)
- [feature-634.md](feature-634.md) - Batch Conversion Tool (Predecessor)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Blocks AC#5)
- [feature-648.md](feature-648.md) - Non-KOJO File Conversion Support (Handoff)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 10 ERB files in Game/ERB/口上/6_フラン/ need conversion to YAML for the new kojo engine
2. Why: The kojo engine is migrating from ERB-based dialogue to YAML-based dialogue for better maintainability, schema validation, and tooling support
3. Why: ERB kojo files mix code and content, making them difficult to validate, test, and author without deep ERB knowledge
4. Why: Phase 19 (Kojo Conversion) was planned in F555 to systematically convert all 117 ERB kojo files across 11 character directories using automated tooling
5. Why: The conversion pipeline (ErbParser + ErbToYaml batch converter from F634) now exists and is ready to process character directories in parallel batches

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| Flandre kojo dialogue is in ERB format only | Phase 19 migration requires per-character batch conversion using F634 tooling |
| 10 ERB files not yet converted | F641 is one of 8 parallel conversion batches (F636-F643) consuming the F634 batch converter |

### Conclusion

The root cause is straightforward: F641 is a standard conversion batch within the Phase 19 pipeline. The F634 batch converter tool is [DONE], providing all required infrastructure (BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter). F641 needs to invoke the batch converter against the `6_フラン/` directory and verify the output.

**Key finding**: Of the 10 ERB files in `6_フラン/`, only 6 use the `KOJO_` prefix pattern expected by PathAnalyzer. The remaining 4 files (`NTR口上.ERB`, `NTR口上_お持ち帰り.ERB`, `SexHara休憩中口上.ERB`, `WC系口上.ERB`) do NOT match the `N_CharacterName/KOJO_Situation.ERB` regex pattern and will cause `ArgumentException` in PathAnalyzer.Extract(). The batch converter's continue-on-error behavior (F634 AC#7) will handle these failures gracefully, reporting them as failed conversions. These non-KOJO files are NTR/special situation kojo that use a different naming convention and may require separate handling.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Predecessor (batch tool) | Provides BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter. All 17 ACs passed. |
| F633 | [DONE] | Predecessor (parser) | PRINTDATA...ENDDATA parsing in ErbParser. PrintDataNode AST with GetDataForms() helper. |
| F555 | [DONE] | Planning origin | Phase 19 Planning. Defined F641 as "Flandre Kojo Conversion (10 files)". |
| F636 | [DRAFT] | Parallel sibling | Meiling Kojo Conversion (11 files). Same conversion pattern. |
| F637 | [DRAFT] | Parallel sibling | Koakuma Kojo Conversion. Same conversion pattern. |
| F638 | [DRAFT] | Parallel sibling | Patchouli Kojo Conversion. Same conversion pattern. |
| F639 | [DRAFT] | Parallel sibling | Sakuya Kojo Conversion. Same conversion pattern. |
| F640 | [DRAFT] | Parallel sibling | Remilia Kojo Conversion. Same conversion pattern. |
| F642 | [DRAFT] | Parallel sibling | Secondary Characters Conversion (grouped). Same conversion pattern. |
| F643 | [DRAFT] | Parallel sibling | Generic Kojo Conversion. Same conversion pattern. |
| F644 | [DRAFT] | Downstream quality | Equivalence Testing Framework. Will validate F641's output. |
| F671 | [DRAFT] | Deferred from F634 | PrintDataNode Variant metadata mapping. F634 converts content only; variant semantics deferred. |

### Pattern Analysis

All 8 conversion batch features (F636-F643) follow an identical pattern: invoke F634 batch converter against a specific character directory, verify output, validate schema. This is a repeatable workflow. Non-KOJO-prefixed files (NTR口上, SexHara, WC系) appear in multiple character directories and consistently fail PathAnalyzer. This is a systemic issue across all conversion batches, not unique to F641.

**Philosophy Coverage**: F641 covers 10 of 117 files as one of 8 parallel conversion batches (F636-F643). Full Philosophy coverage ("Automated migration of 117 ERB kojo files to YAML format") is achieved by the union of all 8 features.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | F634 batch converter is [DONE] with all infrastructure. 6 of 10 files match KOJO_ pattern and are directly convertible. |
| Scope is realistic | YES | Batch conversion is a single command invocation + verification. Minimal new code needed. |
| No blocking constraints | YES | F634 predecessor is [DONE]. All tooling exists. |

**Verdict**: FEASIBLE

**Note**: 4 of 10 files will fail PathAnalyzer (non-KOJO prefix). This is expected and handled by continue-on-error. The feature should document expected success count (6/10 KOJO files) and track non-KOJO file handling as a deferred scope item.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool - provides BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter |
| Related | F633 | [DONE] | PRINTDATA Parser Extension - ErbParser PRINTDATA support used by F634 |
| Related | F636-F640, F642-F643 | [DRAFT] | Parallel sibling conversion batches |
| Related | F644 | [DRAFT] | Equivalence Testing Framework - downstream quality validation |
| Related | F671 | [DRAFT] | PrintDataNode Variant metadata mapping - deferred from F634 |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| tools/ErbToYaml/ | Build-time | Low | F634 batch converter (already built and tested) |
| tools/ErbParser/ | Build-time | Low | ERB AST parser (used by ErbToYaml) |
| tools/KojoComparer/ | Build-time | Low | Equivalence verification tool |
| tools/YamlSchemaGen/dialogue-schema.json | Runtime | Low | YAML schema for validation |
| Game/ERB/口上/6_フラン/ | Input | Low | 10 source ERB files confirmed to exist |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F644 Equivalence Testing Framework | MEDIUM | Will verify converted YAML output matches original ERB behavior |
| F645 Kojo Quality Validator | LOW | Downstream quality validation |
| F646 Post-Phase Review Phase 19 | LOW | Completion tracking |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Game/ERB/口上/6_フラン/KOJO_K6_EVENT.ERB | Input (read-only) | Source ERB file for conversion |
| Game/ERB/口上/6_フラン/KOJO_K6_会話親密.ERB | Input (read-only) | Source ERB file for conversion |
| Game/ERB/口上/6_フラン/KOJO_K6_口挿入.ERB | Input (read-only) | Source ERB file for conversion |
| Game/ERB/口上/6_フラン/KOJO_K6_愛撫.ERB | Input (read-only) | Source ERB file for conversion |
| Game/ERB/口上/6_フラン/KOJO_K6_挿入.ERB | Input (read-only) | Source ERB file for conversion |
| Game/ERB/口上/6_フラン/KOJO_K6_日常.ERB | Input (read-only) | Source ERB file for conversion |
| (output directory)/6_フラン/*.yaml | Create | Generated YAML files from batch conversion |

**Non-convertible files** (PathAnalyzer pattern mismatch - no KOJO_ prefix):
| File | Reason |
|------|--------|
| NTR口上.ERB | NTR kojo - no KOJO_ prefix, different naming convention |
| NTR口上_お持ち帰り.ERB | NTR kojo variant - no KOJO_ prefix |
| SexHara休憩中口上.ERB | SexHara kojo - no KOJO_ prefix |
| WC系口上.ERB | WC kojo - no KOJO_ prefix |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| PathAnalyzer requires KOJO_ prefix pattern | F634 PathAnalyzer regex: `(\d+)_([^\\/]+)[\\/]KOJO_(.+)\.ERB$` | HIGH - 4 of 10 files (40%) will fail extraction. BatchConverter handles this via continue-on-error. |
| KOJO files use PRINTDATA with TALENT branching | ERB file structure (confirmed in KOJO_K6_愛撫.ERB) | MEDIUM - FileConverter handles IF-wrapped PRINTDATA (F634 AC#15) |
| PrintDataNode variant semantics not mapped | F671 deferred scope | LOW - Content converts correctly; display variant (L/W/K/D) metadata is not preserved |
| .ERB.bak file exists (KOJO_K6_愛撫.ERB.bak) | Legacy backup | LOW - BatchConverter glob `*.ERB` does not match `.ERB.bak` |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| 4 non-KOJO files fail PathAnalyzer | Certain | Medium | Expected behavior. Document as known limitation. Track non-KOJO conversion as deferred scope. |
| KOJO files contain unsupported ERB structures | Low | Medium | F634 continue-on-error handles gracefully. KOJO_K6_愛撫.ERB confirmed to use standard PRINTDATA+DATALIST+TALENT pattern. |
| Schema validation rejects generated YAML | Low | Medium | F634 AC#16 validates against dialogue-schema.json. Fix schema or converter if failures occur. |
| Equivalence verification fails (YAML != ERB behavior) | Medium | High | KojoComparer tool exists for detailed diff analysis. Manual review may be needed. |
| PrintDataNode variant information lost | Certain | Low | Deferred to F671. Content is preserved; only display variant metadata (L/W/K/D) is not mapped. |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

Philosophy: "Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format"

Goal claims from feature:
1. "Convert 10 Flandre kojo files using batch converter"
2. "Verify equivalence with KojoComparer"
3. "Validate YAML against schema"
4. "Zero technical debt (no TODO/FIXME/HACK)"

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Convert 10 Flandre kojo files" (Goal #1) | Batch converter must be invoked against 6_フラン directory and produce output | AC#1, AC#2 |
| "10 files" with "6 KOJO-prefixed convertible" (Investigation) | Exactly 6 YAML files must be generated from 6 KOJO_ source files | AC#3 |
| "4 non-KOJO files fail gracefully" (Investigation) | Batch converter reports 4 failures without stopping | AC#4 |
| "Verify equivalence with KojoComparer" (Goal #2) | KojoComparer must pass on all 6 converted YAML files | AC#5 |
| "Validate YAML against schema" (Goal #3) | All 6 YAML files must pass dialogue-schema.json validation | AC#6 |
| "Zero technical debt" (Goal #4) | No TODO/FIXME/HACK markers in generated YAML output | AC#7 |
| "Automated migration" (Philosophy) | Build must succeed before conversion can run | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ErbToYaml batch conversion executes on 6_フラン directory | exit_code | dotnet run --project tools/ErbToYaml -- --batch "Game/ERB/口上/6_フラン" ".tmp/f641-output" --talent "Game/CSV/Talent.csv" --schema "tools/YamlSchemaGen/dialogue-schema.json" | fails | exit code 1 | [x] |
| 2 | Batch summary reports 10 total, 6 success, 4 failed | output | dotnet run --project tools/ErbToYaml -- --batch (stdout capture) | contains | "Total: 10, Success: 6, Failed: 4" | [x] |
| 3 | 6 KOJO source prefixes have YAML output | file | ls .tmp/f641-output/ prefix check (K6_EVENT, K6_愛撫, K6_会話親密, K6_口挿入, K6_挿入, K6_日常) | count_equals | 6 prefixes | [x] |
| 4 | Non-KOJO files reported as failures (not crash) | output | dotnet run --project tools/ErbToYaml -- --batch (stderr/stdout capture) | contains | "NTR口上.ERB" | [x] |
| 5 | KojoComparer equivalence passes for all 6 YAML files | exit_code | dotnet run --project tools/KojoComparer -- (batch mode) | succeeds | exit code 0 | [B] |
| 6 | All YAML files pass schema validation | exit_code | dotnet run --project tools/YamlValidator -- --schema tools/YamlSchemaGen/dialogue-schema.json --validate-all .tmp/f641-output | succeeds | exit code 0 | [x] |
| 7 | No technical debt markers in generated YAML | code | Grep(path=".tmp/f641-output/", pattern="TODO|FIXME|HACK", glob="*.yaml") | count_equals | 0 | [x] |
| 8 | ErbToYaml project builds successfully | build | dotnet build tools/ErbToYaml | succeeds | - | [x] |

### AC Details

**AC#1: ErbToYaml batch conversion executes on 6_フラン directory**
- Invokes the F634 batch converter against the Flandre kojo directory
- Output is written to `.tmp/f641-output/` (temporary directory, gitignored)
- The batch converter processes all 10 ERB files found, with continue-on-error for non-KOJO files
- Exit code 0 indicates the batch completed (individual file failures are expected and reported, not fatal)

**AC#2: Batch summary reports 10 total, 6 success, 4 failed**
- Verifies the exact conversion counts match investigation findings
- 10 total ERB files in directory (excluding .bak)
- 6 KOJO-prefixed files convert successfully
- 4 non-KOJO files (NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB) fail PathAnalyzer pattern matching
- Uses contains matcher since summary is embedded in larger output

**AC#3: 6 YAML files generated for KOJO-prefixed sources**
- Verifies exactly 6 YAML files exist in output directory
- Expected files: KOJO_K6_EVENT.yaml, KOJO_K6_会話親密.yaml, KOJO_K6_口挿入.yaml, KOJO_K6_愛撫.yaml, KOJO_K6_挿入.yaml, KOJO_K6_日常.yaml
- count_equals 6 ensures no extra or missing files
- .ERB.bak file must NOT produce output (BatchConverter globs *.ERB only)

**AC#4: Non-KOJO files reported as failures (not crash)**
- Verifies continue-on-error behavior from F634 AC#7 works in practice
- Each non-KOJO file should appear in failure output with PathAnalyzer error
- The batch must not abort; all 10 files must be attempted
- Checking for "NTR口上.ERB" in output confirms at least one non-KOJO failure is reported

**AC#5: KojoComparer equivalence passes for all 6 YAML files**
- KojoComparer compares original ERB files against generated YAML
- All 6 KOJO-prefixed file pairs must pass equivalence check
- This is the primary correctness verification - ensures YAML content matches ERB source
- Exit code 0 means all comparisons passed

**AC#6: All 6 YAML files pass schema validation**
- YamlValidator validates each YAML file against dialogue-schema.json
- Schema defines required structure: character, situation, dialogue entries
- Exit code 0 means all files conform to the schema
- This ensures YAML output is consumable by the downstream kojo engine

**AC#7: No technical debt markers in generated YAML**
- Generated YAML files must not contain TODO, FIXME, or HACK strings
- count_equals 0 ensures zero debt in output artifacts
- This is a Goal #4 requirement: "Zero technical debt"

**AC#8: ErbToYaml project builds successfully**
- Prerequisite check: the F634 batch converter tool must build before conversion
- Verifies no build regressions in ErbToYaml project
- This is a gate for all other ACs (conversion cannot run if build fails)

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The implementation leverages the F634 batch conversion infrastructure (BatchConverter, FileConverter, PathAnalyzer, PrintDataConverter) to convert Flandre kojo files in a single automated pipeline. The approach consists of three sequential phases:

1. **Conversion Phase**: Invoke `dotnet run --project tools/ErbToYaml -- --batch "Game/ERB/口上/6_フラン" --output .tmp/f641-output` to process all 10 ERB files. The BatchConverter's continue-on-error behavior ensures graceful handling of non-KOJO file failures.

2. **Verification Phase**: Run KojoComparer to verify equivalence between original ERB and generated YAML for the 6 successfully converted KOJO files.

3. **Validation Phase**: Run YamlValidator against the output directory to ensure all 6 YAML files conform to dialogue-schema.json.

**Why this approach satisfies the ACs**:
- AC#1-4 (Conversion): Batch converter handles both success cases (6 KOJO files) and expected failures (4 non-KOJO files) in a single invocation
- AC#5 (Equivalence): KojoComparer provides automated correctness verification
- AC#6 (Schema): YamlValidator ensures downstream consumability
- AC#7 (Zero Debt): Grep verifies no debt markers in generated output
- AC#8 (Build): Pre-flight check ensures tooling is operational

The design requires no new code. All tooling exists from F634. The feature is purely a consumption/verification workflow.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Execute `dotnet run --project tools/ErbToYaml -- --batch "Game/ERB/口上/6_フラン" ".tmp/f641-output"`. Verify exit code 1. The batch converter will process all 10 ERB files with continue-on-error and return exit code 1 due to 4 expected non-KOJO failures. |
| 2 | Capture stdout from batch conversion command. Parse the summary line for "Total: 10, Success: 6, Failed: 4" using contains matcher. F634 BatchConverter.Run() emits this format. **Note**: File count is coupled to current directory state (10 ERB files). If directory contents change, AC must be updated. |
| 3 | Use `Glob(.tmp/f641-output/6_フラン/*.yaml)` to count generated YAML files. Verify count equals 6. Expected files: KOJO_K6_EVENT.yaml, KOJO_K6_会話親密.yaml, KOJO_K6_口挿入.yaml, KOJO_K6_愛撫.yaml, KOJO_K6_挿入.yaml, KOJO_K6_日常.yaml. |
| 4 | Capture stdout/stderr from batch conversion command. Verify output contains "NTR口上.ERB" (one of the 4 non-KOJO files). This confirms failures are reported, not crashed. |
| 5 | Execute `dotnet run --project tools/KojoComparer -- "Game/ERB/口上/6_フラン" ".tmp/f641-output/6_フラン"`. Verify exit code 0. KojoComparer compares all ERB files in source directory against corresponding YAML files in output directory. |
| 6 | Execute `dotnet run --project tools/YamlValidator -- --schema tools/YamlSchemaGen/dialogue-schema.json --validate-all .tmp/f641-output/6_フラン`. Verify exit code 0. YamlValidator validates all YAML files in directory. |
| 7 | Execute `Grep(path=".tmp/f641-output/", pattern="TODO|FIXME|HACK", glob="*.yaml", output_mode="count")`. Verify count equals 0. This ensures zero technical debt markers in generated artifacts. |
| 8 | Execute `dotnet build tools/ErbToYaml` as pre-flight check. Verify exit code 0. This must succeed before any conversion attempts. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Output directory | A) Commit to repo, B) Use .tmp/ (gitignored), C) Use separate test fixture directory | B (.tmp/f641-output) | Temporary output is appropriate for conversion verification. Committed output would bloat repo and create merge conflicts across parallel F636-F643 features. Actual YAML files will be committed by downstream integration features. |
| Error handling for non-KOJO files | A) Pre-filter to only KOJO files, B) Rely on BatchConverter continue-on-error, C) Create separate converter for non-KOJO files | B (continue-on-error) | F634 continue-on-error (AC#7) is designed for this scenario. Pre-filtering would mask PathAnalyzer limitations. Non-KOJO conversion is deferred scope tracked in F671 and future features. |
| Equivalence verification approach | A) Manual inspection, B) Automated KojoComparer, C) Runtime equivalence tests | B (KojoComparer) | KojoComparer tool exists specifically for ERB-YAML equivalence verification. Automated verification is faster and more reliable than manual review. Runtime tests are deferred to F644 (Equivalence Testing Framework). |
| Schema validation tool | A) YamlValidator (C# CLI), B) com-validator (Node.js, Japanese messages), C) Manual schema check | A (YamlValidator) | YamlValidator is the canonical schema validation tool used by F634. com-validator is for user-facing validation with Japanese localization; automated testing uses YamlValidator for consistency. |
| AC execution order | A) Run all ACs in parallel, B) Gate ACs sequentially (build → convert → verify → validate), C) Independent AC execution | B (gated sequence) | AC#8 (build) must pass before AC#1 (conversion). AC#1-4 (conversion) must complete before AC#5-6 (verification/validation). AC#7 (debt check) can run independently. Sequential gates prevent cascading failures. |

### Implementation Notes

**No new code required**. This feature is a pure consumption workflow of F634 infrastructure.

**Test execution environment**:
- Working directory: repository root (C:\Era\erakoumakanNTR)
- Output directory: .tmp/f641-output/ (created by batch converter, gitignored)
- Source directory: Game/ERB/口上/6_フラン/ (10 ERB files, 6 KOJO-prefixed)

**Expected file counts**:
- Input: 10 ERB files (6 KOJO-prefixed, 4 non-KOJO)
- Output: 6 YAML files (corresponding to KOJO-prefixed sources)
- Skipped: 1 backup file (KOJO_K6_愛撫.ERB.bak, not matched by *.ERB glob)

**Known limitations** (documented, not resolved in this feature):
- Non-KOJO files (NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB) fail PathAnalyzer pattern matching. These files use different naming conventions and require separate handling. Deferred to future features.
- PrintDataNode variant semantics (L/W/K/D display types) are not mapped in generated YAML. Content is preserved; metadata loss is tracked in F671.

**Test data**:
- KOJO_K6_愛撫.ERB (investigated in Root Cause Analysis) uses standard PRINTDATA+DATALIST+TALENT pattern supported by F634
- All 6 KOJO files confirmed to follow this pattern (no exotic ERB structures expected)

**Verification strategy**:
- Conversion success: Exit code + file count + summary output
- Correctness: KojoComparer equivalence check (content parity)
- Conformance: YamlValidator schema check (structural validity)
- Quality: Grep debt marker check (zero technical debt)

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Run ErbToYaml batch converter on 6_フラン directory | [x] |
| 2 | 2 | Verify batch conversion summary reports 10 total, 6 success, 4 failed | [x] |
| 3 | 3 | Verify 6 YAML files generated for KOJO-prefixed sources | [x] |
| 4 | 4 | Verify non-KOJO files reported as failures without crash | [x] |
| 5 | 5 | Run KojoComparer equivalence verification on all 6 YAML files | [ ] |
| 6 | 6 | Run YamlValidator schema validation on all 6 YAML files | [x] |
| 7 | 7 | Verify zero technical debt markers in generated YAML | [x] |
| 8 | 8 | Build ErbToYaml project successfully | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to exactly one Task for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | ac-tester | haiku | T8 | AC#8 test command | Build verification |
| 2 | ac-tester | haiku | T1-T4 | AC#1-4 test commands | Conversion execution and verification |
| 3 | ac-tester | haiku | T5-T7 | AC#5-7 test commands | Equivalence, schema, and quality verification |

**Constraints** (from Technical Design):
1. Output directory must be `.tmp/f641-output/` (temporary, gitignored)
2. Batch converter must use continue-on-error for non-KOJO files
3. Exactly 6 YAML files must be generated (KOJO-prefixed sources only)
4. 4 non-KOJO files expected to fail PathAnalyzer (NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB)
5. All verification tools (KojoComparer, YamlValidator) must run from repository root

**Pre-conditions**:
- F634 batch conversion tool exists and is buildable
- 10 ERB files exist in `Game/ERB/口上/6_フラン/` (6 KOJO-prefixed, 4 non-KOJO)
- `tools/YamlSchemaGen/dialogue-schema.json` exists
- `.tmp/` directory is gitignored

**Setup/Teardown**:
- Output directory `.tmp/f641-output/` is created automatically by the batch converter
- No explicit cleanup required - temporary output is gitignored and can be overwritten on subsequent runs

**Success Criteria**:
1. Build succeeds (AC#8 passes)
2. Batch conversion executes without fatal error (AC#1 passes)
3. Exactly 6 YAML files generated (AC#3 passes)
4. Summary reports 10 total, 6 success, 4 failed (AC#2 passes)
5. Non-KOJO failures reported in output (AC#4 passes)
6. KojoComparer passes for all 6 files (AC#5 passes)
7. YamlValidator passes for all 6 files (AC#6 passes)
8. Zero technical debt markers in output (AC#7 passes)

**Execution Sequence**:
1. **Phase 1 (Gate)**: Build ErbToYaml project (T8/AC#8). If this fails, STOP - conversion cannot proceed.
2. **Phase 2 (Conversion)**: Execute batch converter (T1/AC#1), verify summary (T2/AC#2), count files (T3/AC#3), check error reporting (T4/AC#4). These tasks verify the conversion pipeline executes correctly with expected success/failure counts.
3. **Phase 3 (Verification)**: Run KojoComparer (T5/AC#5), YamlValidator (T6/AC#6), debt check (T7/AC#7). These tasks verify the quality and correctness of generated output.

**Rollback Plan**:

This feature generates temporary output only (`.tmp/f641-output/`). No rollback needed - simply delete the output directory.

If conversion or verification issues are discovered:
1. Document the issue in 残課題 section
2. Create follow-up feature for investigation (if needed)
3. Do NOT commit generated YAML files until all ACs pass

## Review Notes
- [resolved-applied] Phase2-Maintainability iter2: Handoff destination updated from 'F555 Phase 19 残課題' to 'F648' (new feature for non-KOJO conversion). Originally referenced F672 but that was invalid (F672 is ac-static-verifier, CANCELLED).
- [resolved-deferred] Phase3-ACValidation iter4: KojoComparer CLI batch mode will be added in separate feature/task - AC#5 remains as design intent.
- [resolved-invalid] Phase3-ACValidation iter4: Feature Type changed to 'infra' - erb positive/negative AC requirement no longer applicable.
- [resolved-invalid] Phase1-Uncertain iter5: Feature Type changed to 'infra' - erb testing archival no longer relevant.
- [resolved-accepted] Phase1-Uncertain iter6: AC#2 string format verified and fragility documented - design choice maintained.
- [resolved-applied] Phase1-Pending iter6: AC#1 matcher changed from 'succeeds' to 'fails' (exit code 1 expected).
- [resolved-accepted] Phase1-Uncertain iter6: File listing via PowerShell confirmed correct - Glob tool limitation documented.
- [resolved-applied] Phase1-Pending iter7: AC#1 matcher changed from 'succeeds' to 'fails' (duplicate resolution).
- [resolved-accepted] Phase1-Uncertain iter7: AC#2 fragility vs. specificity tradeoff - documentation approach maintained.
- [resolved-accepted] Phase1-Uncertain iter9: AC#2 coupling documentation approach confirmed as valid for verification workflow.

## Mandatory Handoffs

| Item | Destination | Note |
|------|-------------|------|
| Non-KOJO file conversion (NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB) | F648 | These 4 files use different naming conventions and require separate PathAnalyzer pattern or converter. Tracked as systemic issue across all F636-F643 conversion batches. |
| PrintDataNode variant metadata mapping | F671 | Display variant (L/W/K/D) semantics not preserved in YAML conversion. Content is correct; metadata deferred. |
| KojoComparer batch equivalence verification (AC#5 BLOCKED) | F644 | KojoComparer has no batch mode. F644 [DRAFT] already plans `--all` mode. AC#5 deferred to F644 completion. |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-28 | Phase 1 | READY:641:infra - Status [REVIEWED] → [WIP] |
| 2026-01-28 | Phase 2 | Investigation complete. All 10 ERB files confirmed. KojoComparer is single-file only (no batch mode). |
| 2026-01-28 | Phase 4 | DEVIATION: Batch converter first run failed (exit 1) - Talent.csv default path resolution broken from Git Bash. Fixed with explicit --talent argument. |
| 2026-01-28 | Phase 4 | Batch conversion succeeded: Total: 10, Success: 6, Failed: 4. 69 YAML files generated (1:N per function, not 1:1 per ERB). |
| 2026-01-28 | Phase 4 | AC#3 MISMATCH: Expected 6 YAML files but converter produces 69 (one per function). AC design assumed 1:1 ERB→YAML but F634 produces 1:N. |
| 2026-01-28 | Phase 4 | AC#5 BLOCKED: KojoComparer has no batch mode. Review Notes already deferred this to separate feature. |
| 2026-01-28 | Phase 4 | AC#6 PASS: All 69 YAML files pass schema validation. |
| 2026-01-28 | Phase 4 | AC#7 PASS: Zero TODO/FIXME/HACK in generated YAML. |
| 2026-01-28 | Phase 4 | AC#8 PASS: ErbToYaml builds successfully. |
| 2026-01-28 | Phase 4 | AC#3 updated: 6 unique KOJO prefixes confirmed (K6_EVENT:3, K6_愛撫:21, K6_会話親密:12, K6_口挿入:15, K6_挿入:13, K6_日常:5 = 69 total YAML). |
| 2026-01-28 | Phase 4 | AC#5 BLOCKED → F644 (Equivalence Testing Framework [DRAFT]). KojoComparer batch mode planned there. |
| 2026-01-28 | Phase 6 | AC verification: 7/8 PASS, 1 BLOCKED (AC#5). Tasks updated. |
| 2026-01-28 | Phase 7 | DEVIATION: Doc-check NEEDS_REVISION - F672 handoff invalid (F672 is ac-static-verifier [CANCELLED]). Fixed: created F648 for non-KOJO conversion. |
| 2026-01-28 | Phase 7 | Quality review: OK. SSOT check: N/A (no new code/interfaces). |
| 2026-01-28 | Phase 8 | Report: 7/8 AC PASS, 1 BLOCKED. User waived AC#5 (Option B) → proceed to [DONE]. |
