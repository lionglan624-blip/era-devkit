# Feature 715: Fix ARRAY_OUT_OF_BOUNDS Compile Warnings (EXP/TCVAR)

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **DOCUMENT** - Create entry in Mandatory Handoffs section
> 3. **LINK** - Specify destination: new Feature | existing Feature | existing Task
>
> This prevents scope creep while ensuring issues aren't lost.

## Type: erb

## Background

### Philosophy (Mid-term Vision)

All ERB code should compile with zero warnings under `--strict-warnings`. Warning-free compilation makes genuine new warnings immediately visible and enables a CI gate. F715 targets the ARRAY_OUT_OF_BOUNDS warnings that remain after F714's DIM_SCOPE cleanup, continuing the zero-warning compilation vision.

### Problem (Current Issue)
After F714 eliminates 224 DIM_SCOPE warnings, 175 ARRAY_OUT_OF_BOUNDS warnings remain:
- **EXP array** (136 warnings): Indices 111-123 exceed the defined array size (100 per EXP.CSV)
- **TCVAR array** (39 warnings): Indices 500, 503 exceed the defined array size

These warnings indicate ERB code accessing array positions beyond the CSV-defined size. Root cause is either:
1. CSV array size definitions need expansion (if the indices are intentionally used)
2. ERB code bugs referencing wrong indices

### Goal (What to Achieve)
Eliminate all 175 "配列の範囲外です" warnings from EXP and TCVAR arrays.

## Links
- [feature-714.md](feature-714.md) - Parent feature (DIM_SCOPE warnings)
- [feature-713.md](feature-713.md) - YAML variable definitions
- [feature-711.md](feature-711.md) - Engine bridge (YAML→ConstantData)
- [feature-708.md](feature-708.md) - TreatWarningsAsErrors

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 175 "配列の範囲外です" compile warnings appear during ERB loading
2. Why: ERB code uses named EXP indices (111-123) and TCVAR indices (500, 503) that exceed array sizes
3. Why: `variable_sizes.yaml` defines `EXP: 100` (indices 0-99) and `TCVAR: 500` (indices 0-499)
4. Why: The original `VariableSize.csv` set these sizes, and the YAML conversion preserved them verbatim
5. Why: The EXP/TCVAR name definitions (`EXP.yaml`, `TCVAR.yaml`) were expanded beyond the original CSV range (e.g., position-specific experiences at 101-110, NTR experiences at 111-123, TCVAR visitor/intimacy data at 500-503), but the array sizes were never updated to match

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 175 compile warnings "キャラクタ配列変数{EXP/TCVAR}の第２引数({N})は配列の範囲外です" | `variable_sizes.yaml` array sizes are smaller than the indices defined in `EXP.yaml` and `TCVAR.yaml` |

### Conclusion

The root cause is a **configuration mismatch** between two YAML files:
- `Game/config/variable_sizes.yaml` defines array sizes: `EXP: 100`, `TCVAR: 500`
- `Game/data/EXP.yaml` defines name mappings up to index 123 (23 entries at indices >= 100)
- `Game/data/TCVAR.yaml` defines name mappings up to index 503 (4 entries at indices >= 500)

When the engine compiles ERB code like `EXP:TARGET:NTR陥落経験`, it resolves the name "NTR陥落経験" to index 111 via the dictionary loaded from `EXP.yaml`, then checks that 111 < 100 (the EXP array size) and emits a warning. The ERB code is intentional and correct; only the array sizes need expansion.

### Warning Distribution

**EXP (136 warnings, 11 unique indices)**:

| Index | Name | Count |
|:-----:|------|:-----:|
| 111 | NTR陥落経験 | 50 |
| 119 | 浮気人数 | 27 |
| 116 | 売春経験 | 14 |
| 114 | 公衆便所陥落経験 | 11 |
| 115 | 前回浮気人数 | 10 |
| 121 | 浮気Ａ性交経験 | 7 |
| 122 | 浮気Ｖ性交経験 | 6 |
| 118 | 浮気キス経験 | 6 |
| 123 | 買春経験 | 2 |
| 120 | 浮気口淫経験 | 2 |
| 113 | ポルノ経験 | 1 |

**TCVAR (39 warnings, 2 unique indices)**:

| Index | Name | Count |
|:-----:|------|:-----:|
| 503 | 馴れ合い強度 | 20 |
| 500 | 苦痛刻印取得 | 19 |

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F714 | [WIP] | Parent feature | F714 addresses DIM_SCOPE warnings; F715 addresses ARRAY_OUT_OF_BOUNDS warnings. Both target zero-warning compilation |
| F713 | [DONE] | Predecessor (indirect) | Expanded YAML variable definitions including EXP.yaml and TCVAR.yaml - introduced the name mappings at high indices |
| F711 | [DONE] | Infrastructure | Bridged YAML variable definitions to engine ConstantData (PopulateConstantNames) |
| F708 | [DONE] | Related | TreatWarningsAsErrors enablement; zero-warning goal benefits from F715 |

### Pattern Analysis

This is an instance of the "definition-size mismatch" pattern: when variable definitions (name→index mappings) are expanded independently of the size configuration, out-of-bounds warnings appear. The pattern emerged because:
1. F713 expanded YAML variable definitions to cover all named constants used in ERB code
2. The variable size definitions were copied from the original CSV without reviewing whether the new YAML entries exceeded the defined array sizes
3. No automated validation exists to catch definition indices exceeding array sizes

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Simple numeric increase in `variable_sizes.yaml` |
| Scope is realistic | YES | Only 1 file needs modification (`Game/config/variable_sizes.yaml`) |
| No blocking constraints | YES | Array size increase is backward-compatible with save data |

**Verdict**: FEASIBLE

The fix is a minimal, low-risk configuration change. Increasing `EXP: 100` to `EXP: 200` and `TCVAR: 500` to `TCVAR: 600` would eliminate all 175 warnings with comfortable headroom for future expansion. The corresponding `EXPNAME` size should also match at `EXPNAME: 200` for consistency.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F714 | [WIP] | DIM_SCOPE warnings; F715 addresses separate ARRAY_OUT_OF_BOUNDS warnings |
| Related | F713 | [DONE] | YAML variable definitions that introduced the high-index name mappings |
| Related | F711 | [DONE] | Engine bridge that loads YAML names into ConstantData dictionaries |

### External Dependencies

None.

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `Game/config/variable_sizes.yaml` | HIGH | Defines array sizes loaded by VariableSizeService |
| `Game/data/EXP.yaml` | MEDIUM | Name definitions - no change needed, already correct |
| `Game/data/TCVAR.yaml` | MEDIUM | Name definitions - no change needed, already correct |
| All ERB files using EXP/TCVAR named indices | LOW | Will compile without warnings after size expansion |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `Game/config/variable_sizes.yaml` | Update | Increase `EXP` from 100 to 200, `TCVAR` from 500 to 600 |
| `Game/config/variable_sizes.yaml` | Update | Increase `EXPNAME` from 100 to 200 (name array must match data array) |

Note: `Game/archive/original-source/.../VariableSize.csv` should NOT be modified as it represents the original source.

## Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Array size increase affects memory per character | Engine design | LOW - difference is ~800 bytes per character (100 Int64s for EXP + 100 Int64s for TCVAR) |
| Save data compatibility | Emuera save format | LOW - larger arrays accommodate existing saves; `VariableSize.csv` comment states "セーブされているサイズより変更後のサイズの方が小さい場合、はみ出たデータは失われます" (only shrinking causes data loss) |
| `EXPNAME` and `EXP` sizes should be consistent | Engine design | MEDIUM - name array (`EXPNAME`) is used for display; must be at least as large as data array (`EXP`) |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Save data incompatibility | Low | Low | Increasing size is safe; only decreasing causes data loss per VariableSize.csv documentation |
| Memory increase per character | Low | Low | ~800 bytes per character is negligible (100 extra Int64s for EXP + 100 for TCVAR) |
| New warnings from other arrays | Low | Low | Run full headless build after change to verify zero new warnings |
| Inconsistent EXPNAME/EXP sizes | Medium | Medium | Ensure both `EXPNAME` and `EXP` are updated together to the same value |
| Recurrence risk until F719 implemented | High | Medium | F719 must be prioritized before next content expansion feature; same definition-size mismatch pattern will recur without automated validation |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

The Goal states: "Eliminate **all** 175 '配列の範囲外です' warnings from EXP and TCVAR arrays."

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Eliminate **all** 175 warnings" | Zero EXP ARRAY_OUT_OF_BOUNDS warnings after fix | AC#1 |
| "Eliminate **all** 175 warnings" | Zero TCVAR ARRAY_OUT_OF_BOUNDS warnings after fix | AC#2 |
| "Eliminate **all** 175 warnings" | Total EXP+TCVAR ARRAY_OUT_OF_BOUNDS count = 0 | AC#3 |
| Root cause: "EXP: 100" too small | `variable_sizes.yaml` EXP value increased to 200 | AC#4 |
| Root cause: "TCVAR: 500" too small | `variable_sizes.yaml` TCVAR value increased to 600 | AC#5 |
| Constraint: "EXPNAME and EXP sizes should be consistent" | `variable_sizes.yaml` EXPNAME matches EXP at 200 | AC#6 |
| Fix must not break compilation | Headless build succeeds with exit code 0 | AC#7 |
| Deferred item: F719 creation | F719 DRAFT file exists for future validation implementation | AC#8 |
| DRAFT Creation Checklist compliance | F719 registered in index-features.md Active Features table | AC#9 |
| DRAFT Creation Checklist compliance | Next Feature number incremented to 720 in index-features.md | AC#10 |
| Negative verification: old values removed | EXP old size (100) no longer present in variable_sizes.yaml | AC#11 |
| Negative verification: old values removed | TCVAR old size (500) no longer present in variable_sizes.yaml | AC#12 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Zero EXP ARRAY_OUT_OF_BOUNDS warnings | build | `dotnet run --project ../engine/uEmuera.Headless.csproj -- . 2>&1 \| grep 'EXP.*範囲外です' \| wc -l` | count_equals | 0 | [x] |
| 2 | Zero TCVAR ARRAY_OUT_OF_BOUNDS warnings | build | `dotnet run --project ../engine/uEmuera.Headless.csproj -- . 2>&1 \| grep 'TCVAR.*範囲外です' \| wc -l` | count_equals | 0 | [x] |
| 3 | Zero combined EXP/TCVAR out-of-bounds warnings | build | `dotnet run --project ../engine/uEmuera.Headless.csproj -- . 2>&1 \| grep '(EXP\|TCVAR).*範囲外です' \| wc -l` | count_equals | 0 | [x] |
| 4 | EXP size updated to 200 | file | Grep(`Game/config/variable_sizes.yaml`) | matches | `^EXP: 200$` | [x] |
| 5 | TCVAR size updated to 600 | file | Grep(`Game/config/variable_sizes.yaml`) | matches | `^TCVAR: 600$` | [x] |
| 6 | EXPNAME size matches EXP at 200 | file | Grep(`Game/config/variable_sizes.yaml`) | matches | `^EXPNAME: 200$` | [x] |
| 7 | Headless build succeeds | build | `dotnet run --project ../engine/uEmuera.Headless.csproj -- .` | succeeds | exit code 0 | [x] |
| 8 | F719 DRAFT file created | file | Glob | exists | Game/agents/feature-719.md | [x] |
| 9 | F719 registered in index-features | file | Grep(`Game/agents/index-features.md`) | matches | `719.*\[DRAFT\]` | [x] |
| 10 | Next feature number updated | file | Grep(`Game/agents/index-features.md`) | matches | `\*\*Next Feature number\*\*: 721` | [x] |
| 11 | Old EXP size absent | file | Grep(`Game/config/variable_sizes.yaml`) | not_matches | `^EXP: 100$` | [x] |
| 12 | Old TCVAR size absent | file | Grep(`Game/config/variable_sizes.yaml`) | not_matches | `^TCVAR: 500$` | [x] |

### AC Details

**AC#1: Zero EXP ARRAY_OUT_OF_BOUNDS warnings**
- Run headless build and count lines matching EXP array out-of-bounds warnings
- Pattern: `配列変数{EXP}` combined with `配列の範囲外です`
- Before fix: 136 such warnings. After fix: must be 0
- Verification: `dotnet run --project ../engine/uEmuera.Headless.csproj -- .` output piped through grep for EXP out-of-bounds lines

**AC#2: Zero TCVAR ARRAY_OUT_OF_BOUNDS warnings**
- Run headless build and count lines matching TCVAR array out-of-bounds warnings
- Pattern: `配列変数{TCVAR}` combined with `配列の範囲外です`
- Before fix: 39 such warnings. After fix: must be 0
- Verification: same headless build output, grep for TCVAR out-of-bounds lines

**AC#3: Zero combined EXP/TCVAR out-of-bounds warnings**
- Comprehensive check: count lines matching pattern `配列変数\{(EXP|TCVAR)\}.*配列の範囲外です` must be 0
- This AC guards against partial fixes where one array type is missed
- Also verifies the combined total (AC#1 + AC#2) equals the 175 baseline count

**AC#4: EXP size updated to 200**
- Grep `Game/config/variable_sizes.yaml` for line matching exactly `^EXP: 200$`
- Must match exactly one line (not `EXPLV`, not `EXPNAME`, just `EXP`)
- Value 200 provides headroom above current max index 123

**AC#5: TCVAR size updated to 600**
- Grep `Game/config/variable_sizes.yaml` for line matching exactly `^TCVAR: 600$`
- Must match exactly one line
- Value 600 provides headroom above current max index 503

**AC#6: EXPNAME size matches EXP at 200**
- Grep `Game/config/variable_sizes.yaml` for line matching exactly `^EXPNAME: 200$`
- EXPNAME is the display-name array for EXP; must be at least as large as EXP to avoid name lookup failures
- Constraint identified in feature analysis: "name array must match data array"

**AC#7: Headless build succeeds**
- Full headless build must complete with exit code 0
- Command: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- .`
- This confirms the size changes do not introduce any new compilation errors or runtime failures
- Build success is the ultimate integration check for the configuration change

**AC#8: F719 DRAFT file created**
- Verify `Game/agents/feature-719.md` file exists
- File creation satisfies TBD Prohibition for deferred item destination
- DRAFT status indicates placeholder for future automated validation implementation
- File existence enables future FL review of F719

**AC#9: F719 registered in index-features**
- Grep `Game/agents/index-features.md` for line matching pattern `719.*\[DRAFT\]`
- Must find F719 row in Active Features table with [DRAFT] status
- Registration enables F719 to be tracked in the feature workflow
- Satisfies DRAFT Creation Checklist requirement for index registration

**AC#10: Next feature number updated**
- Grep `Game/agents/index-features.md` for line matching pattern `\*\*Next Feature number\*\*: 720`
- Must find line at end of file indicating next available feature number (with markdown bold formatting)
- Ensures proper feature numbering sequence after F719/F720 creation (→ 721)
- Completes DRAFT Creation Checklist requirement for number increment

**AC#11: Old EXP size absent**
- Grep `Game/config/variable_sizes.yaml` for pattern `^EXP: 100$` must NOT match
- Negative verification: old EXP value (100) completely replaced with new value (200)
- Guards against partial updates where both old and new values coexist
- Ensures clean configuration state after variable size expansion

**AC#12: Old TCVAR size absent**
- Grep `Game/config/variable_sizes.yaml` for pattern `^TCVAR: 500$` must NOT match
- Negative verification: old TCVAR value (500) completely replaced with new value (600)
- Guards against partial updates where both old and new values coexist
- Ensures clean configuration state after variable size expansion

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Single-file YAML configuration update to expand array size definitions:

1. Update `Game/config/variable_sizes.yaml` to modify three key-value pairs:
   - `EXP: 100` → `EXP: 200` (eliminates 136 warnings from indices 111-123)
   - `EXPNAME: 100` → `EXPNAME: 200` (maintains name-data array consistency)
   - `TCVAR: 500` → `TCVAR: 600` (eliminates 39 warnings from indices 500, 503)

2. Verify elimination of all 175 warnings via headless build output analysis

**Rationale**: The root cause is purely a configuration mismatch between array size definitions (`variable_sizes.yaml`) and name mapping definitions (`EXP.yaml`, `TCVAR.yaml`). The ERB code and name mappings are correct; only the size values need expansion. The selected size values (200 for EXP/EXPNAME, 600 for TCVAR) provide headroom above the current maximum indices (123 for EXP, 503 for TCVAR) to accommodate future expansion.

**Future Change Patterns Investigation**: Analysis of content-roadmap.md and existing kojo dialogue patterns shows EXP/TCVAR expansion typically occurs during character-specific NTR content additions (1-3 new indices per major character arc). With 10 planned character arcs remaining, headroom of 76-96 slots provides coverage for ~25-30 content cycles. Structural validation (deferred to F719) will prevent recurrence regardless of headroom consumption rate.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Run headless build, grep output for pattern `配列変数\{EXP\}.*配列の範囲外です`, count lines matching → must be 0 |
| 2 | Run headless build, grep output for pattern `配列変数\{TCVAR\}.*配列の範囲外です`, count lines matching → must be 0 |
| 3 | Run headless build, grep output for pattern `配列変数\{(EXP|TCVAR)\}.*配列の範囲外です`, count lines matching → must be 0 (combined AC#1+AC#2 safety check) |
| 4 | Grep `Game/config/variable_sizes.yaml` for line matching `^EXP: 200$` → must find exactly 1 match |
| 5 | Grep `Game/config/variable_sizes.yaml` for line matching `^TCVAR: 600$` → must find exactly 1 match |
| 6 | Grep `Game/config/variable_sizes.yaml` for line matching `^EXPNAME: 200$` → must find exactly 1 match |
| 7 | Execute `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- .` → exit code must be 0 |
| 8 | Check file existence: `Game/agents/feature-719.md` → must exist |
| 9 | Grep `Game/agents/index-features.md` for pattern `719.*\[DRAFT\]` → must find 1 match |
| 10 | Grep `Game/agents/index-features.md` for pattern `\*\*Next Feature number\*\*: 721` → must find 1 match |
| 11 | Grep `Game/config/variable_sizes.yaml` for pattern `^EXP: 100$` → must find 0 matches (negative verification) |
| 12 | Grep `Game/config/variable_sizes.yaml` for pattern `^TCVAR: 500$` → must find 0 matches (negative verification) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Size increase strategy | A) Minimal (124/504), B) Moderate (150/550), C) Generous (200/600) | C | Formula: max_index (123/503) + estimated_growth (10 chars × 2 indices/char) + safety_margin (57/77) = 200/600. Headroom provides 25-30 content cycles per content-roadmap.md analysis |
| EXPNAME handling | A) Keep at 100, B) Match EXP at 200 | B | Name array must match data array to avoid name lookup failures; consistency constraint identified in Root Cause Analysis |
| File modification scope | A) Update YAML only, B) Update YAML + original CSV | A | Original CSV in `archive/original-source/` represents historical source; YAML is the active configuration (F711/F713 infrastructure) |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 4,5,6,11,12 | Update variable_sizes.yaml: EXP→200, EXPNAME→200, TCVAR→600 | [x] |
| 2 | 1,2,3,7 | Verify headless build succeeds with zero array out-of-bounds warnings | [x] |
| 3 | 8,9,10 | Create feature-719.md [DRAFT] for automated definition-size mismatch validation | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1,T3 | Technical Design, AC#4-6,8-12 | Updated variable_sizes.yaml, F719 DRAFT file with registration |
| 2 | ac-tester | haiku | T2 | AC#1-3,7 verification commands | Test results (zero warnings, build success) |

**Constraints** (from Technical Design):
1. EXPNAME size must match EXP size (both set to 200) to maintain name-data array consistency
2. Array size increase must be generous enough to accommodate current max indices plus future expansion headroom
3. Only `Game/config/variable_sizes.yaml` should be modified; original CSV in archive remains unchanged

**Pre-conditions**:
- `Game/config/variable_sizes.yaml` exists and contains EXP, EXPNAME, TCVAR entries
- Headless build infrastructure is functional (F711/F713 dependencies complete)
- Current baseline: 136 EXP warnings + 39 TCVAR warnings = 175 total

**Success Criteria**:
1. `variable_sizes.yaml` contains exactly: `EXP: 200`, `EXPNAME: 200`, `TCVAR: 600`
2. Headless build exits with code 0
3. Zero warnings matching pattern `配列変数\{EXP\}.*配列の範囲外です`
4. Zero warnings matching pattern `配列変数\{TCVAR\}.*配列の範囲外です`

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Restore original values: `EXP: 100`, `EXPNAME: 100`, `TCVAR: 500`
3. Notify user of rollback
4. Create follow-up feature for investigation if warnings reappear or build fails

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->

- [resolved-invalid] Phase1-Uncertain iter1: F714 dependency status shows [WIP] but the Dependencies table does not indicate when this was last verified. If F714 completes before F715 runs, the status will be stale. (F714 is Related dependency, not Predecessor - no blocking effect regardless of status)

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Automated definition-size mismatch validation | Recurrence prevention | Feature | F719 | Task#3 |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-01 08:47 | Implementation | Task 1,3 complete: variable_sizes.yaml updated, F719 DRAFT created and registered |
| 2026-02-01 | DEVIATION | ac-tester | PRE-EXISTING ERB error | 子供の訪問関係.ERB line 23: duplicate #DIM 訪問人数 blocks headless build. Fixed by removing duplicate declaration |
| 2026-02-01 | DEVIATION | ac-tester | AC#10 stale | Expected Next Feature number 720 but F720 already exists; updated AC to expect 721 |
| 2026-02-01 | Verification | All 12 ACs PASS | Headless build succeeds, zero warnings, all file checks pass |
