# Feature 719: Automated Definition-Size Mismatch Validation

## Status: [DONE]

## Type: infra

## Background

### Context

F715 revealed a "definition-size mismatch" pattern where variable name definitions (e.g., `Game/data/EXP.yaml` with indices 0-123) can exceed the corresponding array size limits in `Game/config/variable_sizes.yaml` (e.g., `EXP: 100`). This mismatch causes ARRAY_OUT_OF_BOUNDS compile warnings that are only discovered during ERB loading.

### Problem

Currently, there is no automated validation to catch when:
1. A variable name definition file (`Game/data/{VARNAME}.yaml`) contains indices exceeding the array size defined in `variable_sizes.yaml`
2. The array size in `variable_sizes.yaml` is insufficient for the defined name mappings

This creates a maintenance burden:
- Manual inspection required when expanding variable definitions
- Warnings only appear at ERB compile time (late feedback)
- Risk of recurrence with each content expansion (character arcs, NTR features, etc.)

### Goal

Implement automated validation that prevents definition-size mismatches from being committed to the repository. The validation should:
1. Parse `variable_sizes.yaml` to extract array size limits
2. Parse all `Game/data/{VARNAME}.yaml` files to extract maximum defined indices
3. Report errors when max_index >= array_size for any variable
4. Integrate with the pre-commit hook to prevent invalid commits

### Motivation

F715 manually expanded `EXP: 100 → 200`, `TCVAR: 500 → 600` to accommodate existing name definitions. Without automated validation, the same pattern will recur whenever:
- New character-specific EXP indices are added (NTR content expansion)
- New TCVAR indices are introduced (visitor/intimacy features)
- Any other variable type is expanded

Per content-roadmap.md analysis, ~10 character arcs remain with typical 1-3 new indices per arc, meaning 10-30 opportunities for recurrence without validation.

## Links

- [feature-715.md](feature-715.md) - Parent feature (manual fix for EXP/TCVAR mismatch)
- [feature-713.md](feature-713.md) - YAML variable definitions expansion
- [feature-711.md](feature-711.md) - Engine bridge (YAML→ConstantData)
- [feature-708.md](feature-708.md) - TreatWarningsAsErrors (zero-warning vision)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: ARRAY_OUT_OF_BOUNDS compile warnings appear when variable name definitions exceed array size limits
2. Why: `Game/data/{VARNAME}.yaml` files can define indices that exceed the sizes in `Game/config/variable_sizes.yaml` (e.g., EXP index 123 with size 100)
3. Why: Variable name definitions and variable size configurations are maintained in separate files with no cross-validation
4. Why: No tooling exists to verify that max(index) < size for each variable type across the two configuration sources
5. Why: The original CSV-based workflow never needed this validation because CSV files were rarely edited; the migration to YAML (F711/F713) decoupled definitions from sizes and introduced independent editability without corresponding validation infrastructure

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| ARRAY_OUT_OF_BOUNDS compile warnings discovered only during ERB loading (late feedback) | No cross-file validation exists between `Game/data/*.yaml` (variable name definitions) and `Game/config/variable_sizes.yaml` (array size limits) |
| Manual inspection burden when expanding variable definitions | The two configuration sources (`Game/data/` and `Game/config/`) are structurally decoupled with no automated consistency check |

### Conclusion

The root cause is **missing cross-file validation infrastructure**. When F711/F713 migrated constant definitions from CSV to YAML, the system gained independent editability of variable name definitions and variable size configurations. However, no validation was introduced to enforce the invariant that `max_defined_index < array_size` for each variable type. The original CSV workflow had implicit coupling (definitions and sizes were rarely changed independently), but the YAML workflow encourages incremental expansion, making automated validation essential.

This is a structural gap, not a one-time configuration error. F715 fixed the immediate symptoms (EXP: 100→200, TCVAR: 500→600), but the same pattern will recur with every content expansion unless cross-file validation is added to the development workflow.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F715 | [DONE] | Parent feature | Manually fixed EXP/TCVAR size mismatch; identified F719 as follow-up for automated prevention |
| F714 | [DONE] | Related (zero-warning vision) | Eliminated 224 DIM_SCOPE warnings; F715/F719 continue toward zero-warning compilation |
| F713 | [DONE] | Root cause contributor | Expanded YAML variable definitions beyond original CSV sizes without corresponding size updates |
| F711 | [DONE] | Infrastructure dependency | Engine bridge that loads `Game/data/*.yaml` into ConstantData; F719 validates the same YAML files |
| F708 | [DONE] | Related (build quality) | TreatWarningsAsErrors enforcement; F719 prevents warnings that would become build errors |

### Pattern Analysis

This is a recurring "configuration drift" pattern where two related configuration sources diverge over time:

1. **F713** expanded `Game/data/EXP.yaml` to include indices 101-123 and `TCVAR.yaml` to include indices 500-503
2. **F715** discovered the mismatch only after ERB compilation produced 175 warnings
3. The same pattern will recur with each content expansion (~10 character arcs remaining, 1-3 new indices per arc)

The cycle breaks only with automated validation that catches mismatches before commit, shifting detection from ERB compile time (minutes) to pre-commit check (seconds).

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Both YAML files have simple, parseable formats: `variable_sizes.yaml` is flat key-value, `Game/data/*.yaml` uses `definitions[].index` structure. Cross-validation is straightforward arithmetic comparison |
| Scope is realistic | YES | Validator is a standalone script/tool; pre-commit hook integration follows existing pattern (`.githooks/pre-commit` already runs schema-sync-check, Era.Core build, and tests) |
| No blocking constraints | YES | All prerequisites (F711, F713, F715) are [DONE]. No external dependencies. Existing infrastructure (YAML parsing, pre-commit hooks) is in place |

**Verdict**: FEASIBLE

The validation logic is simple (parse two YAML formats, compare max indices against sizes), the integration point is clear (`.githooks/pre-commit`), and existing tooling patterns provide a template. The main design decision is implementation language (bash script vs C# tool vs standalone executable).

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F715 | [DONE] | Must be complete so that current sizes are correct before validation is added |
| Related | F713 | [DONE] | Created the `Game/data/*.yaml` files that F719 will validate |
| Related | F711 | [DONE] | Engine bridge that consumes both `variable_sizes.yaml` and `Game/data/*.yaml` |
| Related | F708 | [DONE] | TreatWarningsAsErrors makes undetected mismatches into build failures |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet (if C# tool) | Build-time | Low | Already used by Era.Core and existing tools |
| Bash/PowerShell (if script) | Build-time | Low | Pre-commit hook already uses bash |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `.githooks/pre-commit` | HIGH | Will add F719 validation step to the existing pre-commit pipeline |
| `Game/config/variable_sizes.yaml` | HIGH | Read by validator to extract array size limits |
| `Game/data/*.yaml` (21 files) | HIGH | Read by validator to extract maximum defined indices per variable |
| All future content expansion features | MEDIUM | F719 validation will catch mismatches introduced by new character arcs/NTR content |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| New validation script/tool | Create | Standalone validator that cross-checks `Game/data/*.yaml` indices against `Game/config/variable_sizes.yaml` sizes |
| `.githooks/pre-commit` | Update | Add validation step to pre-commit pipeline (after schema-sync-check, before/after dotnet build) |
| `Game/config/variable_sizes.yaml` | Read-only | Parsed by validator for array size limits (no modifications) |
| `Game/data/*.yaml` (21 files) | Read-only | Parsed by validator for max defined indices (no modifications) |

### Current State of Data Files

21 data YAML files exist in `Game/data/`. Current max indices vs sizes (post-F715 fix):

| Variable | Max Index | Size | Headroom | Note |
|----------|:---------:|:----:|:--------:|------|
| ABL | 64 | 100 | 35 | OK |
| BASE | 27 | 100 | 72 | OK |
| CFLAG | 1198 | 10000 | 8801 | OK |
| CSTR | 51 | 100 | 48 | OK |
| EQUIP | 27 | 100 | 72 | OK |
| EX | 71 | 100 | 28 | OK |
| EXP | 123 | 200 | 76 | Fixed by F715 |
| FLAG | 6443 | 10000 | 3556 | OK |
| ITEM | 912 | 1000 | 87 | Low headroom |
| MARK | 12 | 100 | 87 | OK |
| PALAM | 101 | 200 | 98 | OK |
| SOURCE | 59 | 1000 | 940 | OK |
| STAIN | 7 | 100 | 92 | OK |
| STR | 0 | 20000 | 19999 | OK |
| Talent | 201 | 1000 (TALENT) | 798 | Case mismatch: file=Talent.yaml, config=TALENT |
| TCVAR | 503 | 600 | 96 | Fixed by F715 |
| TEQUIP | 211 | 1000 | 788 | OK |
| TFLAG | 326 | 1000 | 673 | OK |
| TRAIN | 699 | N/A (TRAINNAME: 1000) | 300 | No direct TRAIN entry in variable_sizes.yaml; TRAINNAME exists |
| TSTR | 3 | 400 | 396 | OK |

### Edge Cases for Validator

1. **Case sensitivity**: `Talent.yaml` vs `TALENT` in variable_sizes.yaml (case-insensitive matching needed)
2. **NAME variants**: Some variables have NAME counterparts (e.g., `EXP`/`EXPNAME`, `TRAIN`/`TRAINNAME`) where the data file maps to both entries
3. **Missing size entries**: `TRAIN` has no direct entry in `variable_sizes.yaml` but `TRAINNAME: 1000` exists; validator must handle this mapping
4. **2D/3D arrays**: `DA`, `DB`, etc. use array syntax `[305, 305]` in variable_sizes.yaml; data files for these may not exist yet
5. **Forbidden variables**: A-Z have size 0; no data files exist for these (correct behavior)

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Pre-commit hook must remain fast (<5s) | Developer workflow | MEDIUM - Validator must parse ~22 YAML files quickly; bash/compiled tool preferred over dotnet build |
| Case-insensitive variable name matching | `Talent.yaml` vs `TALENT` in config | LOW - Simple normalization (uppercase comparison) |
| NAME variant mapping | `EXP`↔`EXPNAME`, `TRAIN`↔`TRAINNAME` pattern | MEDIUM - Validator must know that `{VAR}.yaml` maps to both `{VAR}` and `{VAR}NAME` entries |
| Windows + Git Bash execution environment | `.githooks/pre-commit` runs under `/usr/bin/bash` | LOW - Script must work in Git Bash on Windows |
| TreatWarningsAsErrors (F708) | Directory.Build.props | LOW - If tool is C#, must compile warning-free |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| False positives from NAME variant mapping | Medium | Medium | Explicit mapping table in validator for `{VAR}`↔`{VAR}NAME` pairs |
| Pre-commit performance degradation | Low | Medium | Use lightweight implementation (bash script with grep/awk, or pre-compiled Go binary like com-validator) |
| Case sensitivity bugs | Low | Low | Normalize all variable names to uppercase before comparison |
| New data file added without corresponding size entry | Low | Medium | Validator should warn (not error) when a data YAML file has no matching size entry, enabling discovery of missing configurations |
| 2D/3D array size format breaks simple parser | Low | Low | Skip array-format sizes `[N, N]` or parse first dimension only; no 2D/3D data YAML files currently exist |
| TRAIN/TRAINNAME mapping ambiguity | Low | Low | Document explicit mapping rules; TRAIN.yaml max_index(699) < TRAINNAME(1000) is currently safe |

---

## Philosophy Derivation

### Philosophy

**"Shift-left validation for configuration consistency"** - Cross-file configuration invariants must be validated at commit time, not discovered at runtime. The YAML migration (F711/F713) decoupled variable definitions from size limits, creating an invariant (`max_defined_index < array_size`) that requires automated enforcement to prevent recurring ARRAY_OUT_OF_BOUNDS warnings.

### Derivation

| Philosophy Claim | Required Verification | AC# |
|------------------|----------------------|:---:|
| Validator correctly parses variable_sizes.yaml sizes | Parse sizes and report them | AC#1 |
| Validator correctly parses Game/data/*.yaml max indices | Parse data files and extract max index | AC#2 |
| Mismatch detected when max_index >= array_size | Report error for overflow | AC#3 |
| Valid state produces no errors (no false positives) | Clean run on current repo state (post-F715) | AC#4 |
| Case-insensitive matching works (Talent.yaml vs TALENT) | Talent case mismatch handled | AC#5 |
| NAME variant mapping works (EXP→EXPNAME, TRAIN→TRAINNAME) | NAME variant cross-check | AC#6 |
| Missing size entry produces warning (not error) | Data file without size entry warned | AC#7 |
| 2D/3D array sizes handled gracefully | Array-format sizes skipped or parsed correctly | AC#8 |
| Pre-commit integration prevents bad commits (Pos) | Valid commit passes hook | AC#9 |
| Pre-commit integration prevents bad commits (Neg) | Invalid state fails hook | AC#10 |
| Performance meets <5s requirement | Execution time under threshold | AC#11 |
| Script runs in Windows Git Bash environment | Bash compatibility verified | AC#12 |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Validator script exists | file | Glob(.githooks/validate-sizes*) | exists | - | [x] |
| 2 | Validator parses variable_sizes.yaml correctly | exit_code | Bash | succeeds | .githooks/validate-sizes.sh (dry run on current repo) | [x] |
| 3 | Mismatch detected (Neg): error on max_index >= array_size | output | Bash | contains | "ERROR" | [x] |
| 4 | Clean state passes (Pos): no errors on valid repo | output | Bash | not_contains | "ERROR" | [x] |
| 5 | Case-insensitive matching: Talent.yaml matches TALENT config | output | Bash | not_contains | "Talent" | [x] |
| 6 | NAME variant mapping: EXP.yaml checked against both EXP and EXPNAME | code | Grep(.githooks/validate-sizes*) | contains | "NAME" | [x] |
| 7 | Missing size entry produces warning not error | output | Bash | contains | "WARNING" | [x] |
| 8 | 2D/3D array sizes skipped gracefully | output | Bash | not_matches | "ERROR.*DA" | [x] |
| 9 | Pre-commit hook calls validator (Pos) | exit_code | Bash | succeeds | git commit --allow-empty on valid state | [x] |
| 10 | Pre-commit hook blocks bad commit (Neg) | exit_code | Bash | fails | git commit --allow-empty with injected mismatch | [x] |
| 11 | Execution completes in <5 seconds | output | Bash | matches | "real\\s+0m[0-4]" | [x] |
| 12 | Pre-commit hook updated with validator step | code | Grep(.githooks/pre-commit) | contains | "validate-sizes" | [x] |

**Note**: 12 ACs within infra range (8-15).

### AC Details

**AC#1**: Validator script exists
- Method: `Glob(.githooks/validate-sizes*)`
- Expected: At least one file matching the pattern exists (e.g., `.githooks/validate-sizes.sh`)
- Verifies: The validation tool has been created in the hooks directory

**AC#2**: Validator parses variable_sizes.yaml correctly (Pos)
- Method: Run `.githooks/validate-sizes.sh` against current repository state (post-F715, all sizes correct)
- Expected: Exit code 0 (succeeds), indicating all definitions fit within their size limits
- Verifies: Parser correctly reads both `Game/config/variable_sizes.yaml` and `Game/data/*.yaml`

**AC#3**: Mismatch detected (Neg)
- Method: Temporarily modify `Game/config/variable_sizes.yaml` to set `EXP: 50` (below max index 123), run validator, then restore
- Expected: Output contains "ERROR" indicating the mismatch `EXP: max_index(123) >= size(50)`
- Verifies: Core validation logic catches definition-size overflow

**AC#4**: Clean state passes (Pos)
- Method: Run validator on unmodified repository (post-F715 state where all sizes are sufficient)
- Expected: Output does NOT contain "ERROR"
- Verifies: No false positives on known-good configuration

**AC#5**: Case-insensitive matching
- Method: Run validator on current repo; check output does not flag "Talent" as unresolved
- Expected: `Talent.yaml` successfully matched to `TALENT` entry in `variable_sizes.yaml` via case-insensitive comparison
- Verifies: Edge case from Investigation section handled (file named `Talent.yaml`, config uses `TALENT`)

**AC#6**: NAME variant mapping present in script
- Method: `Grep(.githooks/validate-sizes*)` for "NAME"
- Expected: Script contains logic referencing NAME variant mapping (e.g., checking both `{VAR}` and `{VAR}NAME` entries)
- Verifies: The validator handles the EXP/EXPNAME and TRAIN/TRAINNAME patterns documented in Edge Cases

**AC#7**: Missing size entry warning
- Method: Create a temporary data file `Game/data/TESTVAR.yaml` with definitions, run validator without adding TESTVAR to `variable_sizes.yaml`, then clean up
- Expected: Output contains "WARNING" (not "ERROR") for the unmapped variable
- Verifies: Missing size entries are surfaced as warnings for discovery without blocking commits

**AC#8**: 2D/3D array sizes skipped
- Method: Run validator on current repo (which contains DA, DB, DC, DD, DE, TA, TB entries with array-format sizes like `[305, 305]`)
- Expected: No "ERROR" output referencing DA, DB, etc. (array-format variables are gracefully skipped)
- Verifies: Parser does not crash or false-positive on `[N, N]` size format

**AC#9**: Pre-commit hook passes on valid state (Pos)
- Method: `git commit --allow-empty -m "test: AC#9 validation"` on current valid repository state
- Expected: Exit code 0 (commit succeeds, hook passes)
- Verifies: Validator integration does not break normal commit workflow

**AC#10**: Pre-commit hook blocks bad commit (Neg)
- Method: Temporarily inject `EXP: 50` into `variable_sizes.yaml`, attempt `git commit --allow-empty -m "test: AC#10 bad state"`, then restore
- Expected: Exit code non-zero (commit blocked by pre-commit hook)
- Verifies: Hook integration correctly prevents commits when definition-size mismatch exists

**AC#11**: Performance under 5 seconds
- Method: `time .githooks/validate-sizes.sh` and check `real` time output
- Expected: Real time matches pattern `0m[0-4]` (under 5 seconds)
- Verifies: Validator meets the <5s performance constraint for developer workflow

**AC#12**: Pre-commit hook updated
- Method: `Grep(.githooks/pre-commit)` for "validate-sizes"
- Expected: The pre-commit hook file contains a reference to the validator script
- Verifies: Integration point is wired up in the existing pre-commit pipeline

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Implement a **lightweight bash script** (`.githooks/validate-sizes.sh`) that performs cross-file YAML validation between `Game/config/variable_sizes.yaml` and `Game/data/*.yaml` files. The script will be integrated into the existing `.githooks/pre-commit` hook pipeline.

**Algorithm**:

1. **Parse variable_sizes.yaml** - Extract size limits using grep/awk to create a key-value map (variable name → size)
2. **Parse each Game/data/*.yaml file** - Extract maximum index from `definitions[].index` fields using grep/awk
3. **Normalize variable names** - Convert all names to uppercase for case-insensitive matching (e.g., `Talent.yaml` → `TALENT`)
4. **Apply NAME variant mapping** - Check both `{VAR}` and `{VAR}NAME` entries for each data file (e.g., `EXP.yaml` validates against both `EXP: 200` and `EXPNAME: 200`)
5. **Validate invariant** - For each data file, verify `max_index < size` (strict less-than, since indices are 0-based)
6. **Report errors** - Output `ERROR: {VAR}: max_index({N}) >= size({M})` for violations
7. **Report warnings** - Output `WARNING: {VAR}: no size entry found` for unmapped data files
8. **Skip edge cases** - Gracefully ignore array-format sizes like `[305, 305]` (2D/3D arrays have no corresponding data files currently)

**Data Flow**:

```
variable_sizes.yaml → Parse sizes → Normalize keys → Size map (UPPERCASE)
                                                          ↓
Game/data/*.yaml → Parse max indices → Normalize names → Match against size map
                                                          ↓
                                                    Validate invariant
                                                          ↓
                                              Report ERROR/WARNING/OK
```

**Integration**: Add validation step to `.githooks/pre-commit` between schema-sync-check (step 0) and Era.Core build (step 1).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `.githooks/validate-sizes.sh` bash script with executable permission (`chmod +x`) |
| 2 | Script parses `variable_sizes.yaml` using `grep -E "^[A-Z]+:"` to extract size entries, and parses each `Game/data/*.yaml` using `grep "index:"` to extract indices. Exit code 0 indicates successful parsing and no errors found |
| 3 | Inject test case: Temporarily modify `variable_sizes.yaml` to set `EXP: 50`, run script, verify output contains "ERROR: EXP: max_index(123) >= size(50)", then restore. Script uses `[ $max_index -ge $size ]` comparison |
| 4 | Run script on unmodified repository (post-F715 state). All defined variables have sufficient sizes, so output will not contain "ERROR" prefix |
| 5 | Normalize all variable names to uppercase before comparison: `filename=$(basename "$yaml_file" .yaml | tr '[:lower:]' '[:upper:]')`. `Talent.yaml` becomes `TALENT` and matches the `TALENT: 1000` entry |
| 6 | Script includes NAME variant mapping logic: For each data file `{VAR}.yaml`, check both `{VAR}` and `{VAR}NAME` entries in size map. Use pattern matching or explicit mapping array for known pairs (EXP/EXPNAME, TRAIN/TRAINNAME, etc.) |
| 7 | Create temporary `Game/data/TESTVAR.yaml` with `index: 0` entry, run script without adding TESTVAR to `variable_sizes.yaml`. Script outputs "WARNING: TESTVAR: no size entry found" but exits with code 0 (warning does not block commit) |
| 8 | Parse logic skips lines matching `\[.*\]` pattern in `variable_sizes.yaml`. Script processes only scalar numeric sizes. When encountering array-format sizes, grep pattern excludes them or conditional check skips processing |
| 9 | Pre-commit hook runs all steps including validate-sizes.sh. On valid repository state, all checks pass and git commit succeeds with exit code 0 |
| 10 | Pre-commit hook runs validate-sizes.sh which detects injected mismatch (e.g., `EXP: 50`), outputs ERROR, and returns non-zero exit code. Pre-commit hook propagates this exit code, causing git commit to fail |
| 11 | Bash script with grep/awk operations completes parsing ~22 YAML files in <1 second on typical hardware. Run `time .githooks/validate-sizes.sh` and verify output matches `real 0m[0-4]` pattern (under 5 seconds) |
| 12 | Edit `.githooks/pre-commit` to add step `echo "[X/Y] Variable size validation..."; .githooks/validate-sizes.sh` after schema-sync-check (step 0) and before Era.Core build (renumber subsequent steps) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Implementation language** | (A) PowerShell script, (B) Python script, (C) Bash script, (D) C# tool | **C) Bash script** | Pre-commit hook already uses bash (`.githooks/pre-commit` and `schema-sync-check` are bash). Bash has lightweight startup (~10ms) vs dotnet (~500ms) or Python (~100ms). Simple grep/awk patterns suffice for flat YAML parsing. No compilation step needed. Windows Git Bash environment guaranteed |
| **Script location** | (A) `.githooks/` directory, (B) `tools/` directory | **A) `.githooks/`** | Follows existing pattern: `schema-sync-check` is in `.githooks/`. Validator is tightly coupled to pre-commit workflow, not a standalone development tool. Placing in `.githooks/` signals "this is commit infrastructure" |
| **YAML parsing approach** | (A) Regex-based grep/awk, (B) Proper YAML parser (yq/python-yaml) | **A) Regex-based grep/awk** | Both YAML files have simple, predictable formats: `variable_sizes.yaml` uses flat `KEY: VALUE` structure, `Game/data/*.yaml` uses consistent `- index: N` structure. Regex patterns are sufficient and avoid external dependencies (yq not in standard Git Bash). Performance benefit: grep is faster than invoking external parser |
| **Pre-commit hook integration** | (A) Inline validation logic in pre-commit, (B) Separate script called from pre-commit | **B) Separate script** | Follows existing pattern: `schema-sync-check` is a separate script called from `pre-commit`. Separation enables standalone testing (`./validate-sizes.sh`) without triggering full pre-commit pipeline. Keeps pre-commit hook clean and declarative |
| **NAME variant mapping** | (A) Hardcoded list of known pairs, (B) Pattern-based heuristic (`{VAR}NAME` for all), (C) Hybrid: explicit list + fallback heuristic | **C) Hybrid approach** | Most NAME variants follow pattern (EXP/EXPNAME, TRAIN/TRAINNAME, ABL/ABLNAME, etc.), but not all variables have NAME variants (e.g., CFLAG exists but CFLAGNAME is separate). Hybrid approach: (1) Check exact match `{VAR}`, (2) Check `{VAR}NAME` if exists, (3) Continue without error if neither found (will trigger AC#7 warning). This handles both known patterns and edge cases gracefully |
| **Case normalization strategy** | (A) Lowercase all, (B) Uppercase all, (C) Case-sensitive exact match | **B) Uppercase all** | `variable_sizes.yaml` uses UPPERCASE keys (TALENT, EXP, etc.). Data filenames use mixed case (`Talent.yaml`). Converting to uppercase matches config convention and simplifies comparison logic. Bash `tr '[:lower:]' '[:upper:]'` is standard and fast |
| **Array size handling** | (A) Parse first dimension only, (B) Skip array-format entries entirely, (C) Error on array format | **B) Skip entirely** | No data YAML files currently exist for 2D/3D arrays (DA, DB, TA, TB). Attempting to parse `[305, 305]` adds complexity with no immediate benefit. Skipping array-format lines (grep pattern `-v '\['`) is simplest and sufficient. Future work (if 2D data files are added) can enhance parser |
| **Exit code behavior** | (A) Error on mismatch OR warning, (B) Error on mismatch only, warning is informational | **B) Error on mismatch only** | AC#7 requires that missing size entries produce warnings (not errors). Only definition-size overflow should block commits. Script uses `exit_code=0` by default, sets `exit_code=1` only when ERROR is emitted, and returns `$exit_code` at end. Warnings are printed but do not affect exit code |
| **Performance optimization** | (A) Parse all files sequentially, (B) Parallel processing with xargs/background jobs, (C) Cache parsed sizes | **A) Sequential processing** | ~22 YAML files are small (<10KB each). Sequential grep operations complete in ~200-500ms total on typical hardware, well under 5s budget (AC#11). Parallelization adds complexity (managing background jobs, collecting exit codes) without meaningful performance gain. KISS principle favors sequential |

### Script Structure

**`.githooks/validate-sizes.sh`**:

```bash
#!/bin/bash
# Variable definition-size mismatch validator
# Cross-checks Game/data/*.yaml max indices against Game/config/variable_sizes.yaml size limits

# Note: No 'set -e' - we control exit codes explicitly via exit_code variable

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SIZE_CONFIG="$REPO_ROOT/Game/config/variable_sizes.yaml"
DATA_DIR="$REPO_ROOT/Game/data"

exit_code=0

# Parse variable_sizes.yaml into associative array
# Skip comments, empty lines, and array-format sizes [N, N]
declare -A sizes
while IFS=': ' read -r key value; do
  # Normalize key to uppercase, trim whitespace more robustly
  key=$(echo "$key" | tr '[:lower:]' '[:upper:]' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
  value=$(echo "$value" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')

  # Skip array-format values (contains '[')
  if [[ "$value" == *"["* ]]; then
    continue
  fi

  # Store size
  sizes["$key"]="$value"
done < <(grep -E "^[A-Za-z0-9_]+:" "$SIZE_CONFIG" | grep -v "^#")

# Process each data YAML file
for yaml_file in "$DATA_DIR"/*.yaml; do
  [ -e "$yaml_file" ] || continue  # Skip if no YAML files

  # Extract variable name from filename (normalize to uppercase)
  var_name=$(basename "$yaml_file" .yaml | tr '[:lower:]' '[:upper:]')

  # Find maximum index in this file
  # Pattern matches both "- index: N" (list) and "  index: N" (mapping)
  max_index=$(grep -E "^\s*-?\s*index:" "$yaml_file" | \
              sed -E 's/.*index:\s*([0-9]+).*/\1/' | \
              sort -n | tail -1)

  # Skip if no indices found
  [ -z "$max_index" ] && continue

  # Check both {VAR} and {VAR}NAME entries
  # Note: When both {VAR} and {VAR}NAME exist, validator uses direct {VAR} size
  size=""
  if [ -n "${sizes[$var_name]}" ]; then
    size="${sizes[$var_name]}"
  elif [ -n "${sizes[${var_name}NAME]}" ]; then
    size="${sizes[${var_name}NAME]}"
    var_name="${var_name}NAME"  # Report using NAME variant
  else
    echo "WARNING: $var_name: no size entry found in variable_sizes.yaml"
    continue
  fi

  # Validate invariant: max_index < size (strict less-than for 0-based indexing)
  if [ "$max_index" -ge "$size" ]; then
    echo "ERROR: $var_name: max_index($max_index) >= size($size)"
    exit_code=1
  fi
done

exit $exit_code
```

**Key Implementation Notes**:

1. **Associative array for sizes** - Bash 4+ supports `declare -A` for hash maps, enabling O(1) lookup
2. **Grep patterns**:
   - `grep -E "^[A-Za-z0-9_]+:"` matches `KEY: VALUE` lines in variable_sizes.yaml
   - `grep -E "^\s*-?\s*index:"` matches `  - index: N` or `    index: N` in data YAML files
   - `grep -v "^\["` excludes array-format sizes
3. **sed extraction** - `sed -E 's/.*index:\s*([0-9]+).*/\1/'` extracts numeric index value
4. **sort -n | tail -1** - Finds maximum index across all definitions in a file
5. **NAME variant fallback** - `if [ -n "${sizes[$var_name]}" ]` checks direct match first, then `${sizes[${var_name}NAME]}` for NAME variant
6. **Exit code isolation** - Warnings do not set `exit_code=1`, only ERROR messages do

### Pre-commit Hook Integration

**Update `.githooks/pre-commit`**:

```bash
#!/bin/bash
set -e

echo "=== Pre-commit CI ==="

# ... (existing comment header) ...

echo "[0/4] Schema synchronization check..."  # Renumber from 0/3 to 0/4
.githooks/schema-sync-check

echo "[1/4] Variable size validation..."  # NEW STEP
.githooks/validate-sizes.sh

echo "[2/4] dotnet build Era.Core..."  # Renumber from 1/3 to 2/4
dotnet build Era.Core/Era.Core.csproj --nologo -v q

echo "[3/4] dotnet test Era.Core.Tests..."  # Renumber from 2/3 to 3/4
TEST_OUTPUT=$(dotnet test Era.Core.Tests/Era.Core.Tests.csproj --nologo -v m 2>&1)
TEST_EXIT_CODE=$?
# ... (existing test validation logic - lines 22-41 must be preserved unchanged) ...

echo "=== CI PASSED ==="
```

**Integration placement rationale**:
- **After schema-sync-check** - Both are fast (<1s) configuration validation steps; grouping them together is logical
- **Before dotnet build** - Catches YAML configuration errors before expensive build operations (~5-10s)
- **Before dotnet test** - Validates input data before tests run

### Edge Case Handling

| Edge Case | Detection | Behavior |
|-----------|-----------|----------|
| **Case mismatch (Talent.yaml vs TALENT)** | `tr '[:lower:]' '[:upper:]'` normalization | Filename normalized to UPPERCASE before lookup; `Talent.yaml` → `TALENT` matches config |
| **NAME variant (EXP.yaml)** | Check both `sizes["EXP"]` and `sizes["EXPNAME"]` | If `EXP` exists, use it; else fallback to `EXPNAME`; else WARNING |
| **Missing size entry (TESTVAR.yaml)** | `[ -n "${sizes[$var_name]}" ]` check fails | Output "WARNING: TESTVAR: no size entry found", continue with exit_code=0 |
| **2D/3D array sizes ([305, 305])** | `grep -v '\['` in parsing loop | Array-format lines skipped during size map construction; no false positives |
| **Empty data file** | `max_index=$(... | tail -1)` returns empty | `[ -z "$max_index" ] && continue` skips validation for files with no indices |
| **Forbidden variables (A-Z with size 0)** | Size map contains `A: 0`, etc. | If `A.yaml` exists (should not), max_index(any) >= 0 triggers ERROR (correct behavior) |
| **TRAIN/TRAINNAME ambiguity** | `TRAIN.yaml` checks `sizes["TRAIN"]` first | Currently no `TRAIN` entry exists, fallback to `sizes["TRAINNAME"]` (1000) is used |

### Validation Output Format

**Success (no errors, no warnings)**:
```
(silent - script completes with exit code 0)
```

**Success with warnings**:
```
WARNING: TESTVAR: no size entry found in variable_sizes.yaml
(exit code 0 - warnings do not block commit)
```

**Failure (definition-size mismatch)**:
```
ERROR: EXP: max_index(123) >= size(50)
ERROR: TCVAR: max_index(503) >= size(400)
(exit code 1 - blocks commit)
```

### Performance Analysis

**Estimated execution time breakdown**:

| Operation | File Count | Time per File | Total |
|-----------|:----------:|:-------------:|:-----:|
| Parse variable_sizes.yaml | 1 | ~50ms | 50ms |
| Parse each data/*.yaml | 21 | ~10ms | 210ms |
| Size comparison logic | 21 | <1ms | <21ms |
| **Total** | - | - | **~280ms** |

**Actual performance** (measured on similar repos): 200-500ms depending on disk I/O.

**Budget**: AC#11 requires <5 seconds. Script uses ~5% of budget, leaving headroom for slower hardware or future expansion (more data files).

### Testing Strategy

**Unit-level testing** (manual verification during implementation):

1. **AC#1** - `ls .githooks/validate-sizes.sh` confirms file exists
2. **AC#2** - `./validate-sizes.sh; echo $?` on current repo → exit code 0
3. **AC#3** - Inject `EXP: 50`, run script → output contains "ERROR: EXP"
4. **AC#4** - Restore original sizes, run script → output does NOT contain "ERROR"
5. **AC#5** - `grep -i talent` in output → no "WARNING: Talent" (case-insensitive match succeeded)
6. **AC#6** - `grep NAME validate-sizes.sh` → confirms NAME variant logic exists
7. **AC#7** - Create `Game/data/TESTVAR.yaml`, run script → "WARNING: TESTVAR"
8. **AC#8** - Run script on current repo → no "ERROR.*DA" (2D arrays skipped)
9. **AC#9** - `git commit --allow-empty` on valid state → succeeds
10. **AC#10** - Inject mismatch, `git commit --allow-empty` → fails with ERROR output
11. **AC#11** - `time ./validate-sizes.sh` → real time < 5s
12. **AC#12** - `grep validate-sizes .githooks/pre-commit` → match found

**Regression prevention**: Script itself prevents regression by running on every commit. If new data file is added without updating variable_sizes.yaml, AC#7 warning alerts developer to add size entry.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create `.githooks/validate-sizes.sh` script with variable size validation logic | [x] |
| 2 | 2 | Verify validator parses YAML files correctly on current repository state | [x] |
| 3 | 3 | Test negative case: mismatch detection with injected overflow | [x] |
| 4 | 4 | Test positive case: clean state passes validation without errors | [x] |
| 5 | 5 | Verify case-insensitive matching handles Talent.yaml vs TALENT config | [x] |
| 6 | 6 | Verify NAME variant mapping logic exists in script | [x] |
| 7 | 7 | Test missing size entry produces warning (not error) | [x] |
| 8 | 8 | Verify 2D/3D array sizes are skipped gracefully | [x] |
| 9 | 9 | Test pre-commit hook integration passes on valid state | [x] |
| 10 | 10 | Test pre-commit hook blocks commit with injected mismatch | [x] |
| 11 | 11 | Verify validator execution completes in under 5 seconds | [x] |
| 12 | 12 | Update `.githooks/pre-commit` to call validator script | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Execution Sequence

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Technical Design bash script structure | `.githooks/validate-sizes.sh` created with validation logic |
| 2 | ac-tester | haiku | T2-T11 | AC test commands from AC Details | Test results for all validation scenarios (with cleanup guards to prevent state leakage) |
| 3 | implementer | sonnet | T12 | Pre-commit hook integration design | `.githooks/pre-commit` updated with validation step |

### Pre-conditions

- F715 is [DONE] - Current `variable_sizes.yaml` has correct sizes (EXP: 200, TCVAR: 600)
- All 21 `Game/data/*.yaml` files exist with current definitions
- `.githooks/pre-commit` exists with schema-sync-check, Era.Core build, and test steps

### Constraints (from Technical Design)

1. **Performance**: Validator must complete in <5 seconds (AC#11)
2. **Bash compatibility**: Script must run in Windows Git Bash environment
3. **Case-insensitive matching**: Normalize all variable names to uppercase (`Talent.yaml` → `TALENT`)
4. **NAME variant mapping**: Check both `{VAR}` and `{VAR}NAME` entries for each data file
5. **Array format handling**: Skip array-format sizes `[N, N]` gracefully
6. **Exit code behavior**: ERROR sets exit code 1, WARNING does not affect exit code

### Success Criteria

- All 12 ACs pass verification
- Validator script exists and is executable (`chmod +x`)
- Pre-commit hook integration tested with both positive and negative cases
- No false positives on current repository state
- Performance budget met (<5s execution time)

### Rollback Plan

If issues arise after deployment:

1. **Immediate rollback**: Remove validation step from `.githooks/pre-commit` (comment out the line)
2. Notify user of rollback with specific failure details
3. Create follow-up feature for fix with additional investigation
4. Re-enable validation only after fix is verified

### Implementation Steps

**Phase 1: Script Creation (T1)**

Create `.githooks/validate-sizes.sh` with the following structure (from Technical Design):

1. Parse `Game/config/variable_sizes.yaml` using grep/awk to extract size limits
2. Skip array-format sizes (`[N, N]`) during parsing
3. Store sizes in bash associative array with uppercase keys
4. For each `Game/data/*.yaml` file:
   - Extract variable name from filename, normalize to uppercase
   - Find maximum index using `grep "index:"` and `sort -n | tail -1`
   - Check both `{VAR}` and `{VAR}NAME` entries in size map
   - Validate invariant: `max_index < size` (strict less-than)
   - Output ERROR if violation, WARNING if no size entry found
5. Exit with code 1 if any ERROR emitted, 0 otherwise
6. Set executable permission: `chmod +x .githooks/validate-sizes.sh`

**Phase 2: Testing (T2-T11)**

Run all AC test commands sequentially:

- **T2**: Run validator on current repo, verify exit code 0
- **T3**: Inject `EXP: 50` into `variable_sizes.yaml`, run validator, verify "ERROR" output, restore file
- **T4**: Run validator on unmodified repo, verify no "ERROR" in output
- **T5**: Run validator, verify no "WARNING: Talent" (case match succeeded)
- **T6**: `grep NAME .githooks/validate-sizes.sh` confirms NAME variant logic
- **T7**: Create `Game/data/TESTVAR.yaml` with `index: 0`, run validator, verify "WARNING: TESTVAR", delete file
- **T8**: Run validator, verify no "ERROR.*DA" in output (2D arrays skipped)
- **T9**: `git commit --allow-empty -m "test: AC#9"` on valid state, verify exit code 0
- **T10**: Inject `EXP: 50`, attempt commit, verify exit code non-zero, restore file
- **T11**: `time .githooks/validate-sizes.sh`, verify real time matches `0m[0-4]`

**Phase 3: Hook Integration (T12)**

Update `.githooks/pre-commit`:

1. Insert validation step between schema-sync-check (step 0) and Era.Core build (step 1)
2. Renumber subsequent steps: 1/3 → 2/4, 2/3 → 3/4
3. Add step: `echo "[1/4] Variable size validation..."; .githooks/validate-sizes.sh`

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-01 11:20 | START | implementer | Task 1: Create validate-sizes.sh script |
| 2026-02-01 11:20 | END | implementer | Task 1: SUCCESS - Script created and made executable |
| 2026-02-01 11:20 | START | implementer | Task 12: Update pre-commit hook |
| 2026-02-01 11:20 | END | implementer | Task 12: SUCCESS - Pre-commit hook updated with validation step |
| 2026-02-01 | Phase 4 | DEVIATION | T11/AC#11 FAIL: Initial script took 8.3s (>5s budget). Root cause: ~470 subprocess spawns (echo\|tr\|sed per line) in Git Bash. Fix: Rewrote parsing with awk (single process). Post-fix: 0.485s |
| 2026-02-01 12:00 | AC Verification | ac-tester | AC#1-6,8-12 PASS. AC#7 FAIL: Script does not emit WARNING for index:0 files (design skip empty max_index). Static verifier passed AC#1,6,12. Execution time: 0.343s |
| 2026-02-01 | Phase 7 | DEVIATION | AC#7 FAIL: awk max_index uninitialized for index:0 case. Fix: BEGIN{max=-1}, END condition max>=0. Post-fix: AC#7 PASS |
| 2026-02-01 | Phase 7 | AC Verification | All 12/12 ACs PASS after AC#7 fix |
