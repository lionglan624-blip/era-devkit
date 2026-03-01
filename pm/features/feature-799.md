# Feature 799: ac-static-verifier Binary Warning Test Fix

## Status: [DONE]
<!-- fl-reviewed: 2026-02-21T01:06:51Z -->

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

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F798 |
| Discovery Phase | Phase 7 |
| Timestamp | 2026-02-21 |

### Observable Symptom
`test_skipped_files_warning_output` in `src/tools/python/tests/test_ac_verifier_binary.py` expects `INFO: Skipped 3 binary file(s) in binaries/` on stderr, but stderr is empty because `ACVerifier.verbose` defaults to False and the test does not enable verbose mode before checking warning output.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python -m pytest src/tools/python/tests/test_ac_verifier_binary.py::test_skipped_files_warning_output` |
| Exit Code | 1 |
| Error Output | `AssertionError: Expected skipped file warning in stderr, got: ` |
| Expected | Test PASS (warning message on stderr) |
| Actual | Test FAIL (empty stderr) |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/tests/test_ac_verifier_binary.py | Contains failing test (line ~202) |
| src/tools/python/ac-static-verifier.py | Binary file warning logic gated by `self.verbose` |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | - |

### Parent Session Observations
The test was introduced as part of binary file handling (likely F704/F776 era). The test creates binary files, expands a directory path, and checks stderr for skip warnings. However, `ACVerifier.__init__` sets `self.verbose = False` by default, and binary file skip warnings at line 526-527 are gated by `if self.verbose`. The test needs to either set `verbose=True` on the verifier instance or the warning should be unconditional.

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Pipeline Continuity - ac-static-verifier test suite must be fully green to serve as a reliable regression safety net for all features using static AC verification. The test suite is the SSOT for verifier correctness, and any silently broken test undermines confidence in AC verification across all features.

### Problem (Current Issue)
The test `test_skipped_files_warning_output` in `src/tools/python/tests/test_ac_verifier_binary.py` fails because commit `ad17c046` changed binary file skip warnings from unconditional stderr output to verbose-gated output (requiring `self.verbose == True`), but did not update this test to pass `verbose=True` when constructing the `ACVerifier` instance. As a result, the warning at `src/tools/python/ac-static-verifier.py:165-166` is never executed during the test, producing empty stderr where the assertion at line 202 expects `"INFO: Skipped 3 binary file(s) in binaries/"`.

### Goal (What to Achieve)
Fix `test_skipped_files_warning_output` to pass `verbose=True` to the `ACVerifier` constructor so the test correctly exercises the verbose warning path, restoring the test to a passing state without changing the production default of `verbose=False`.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does the test fail? | `captured.err` is empty because the `print(..., file=sys.stderr)` call is never executed | `src/tools/python/tests/test_ac_verifier_binary.py:202` |
| 2 | Why is the print call not executed? | The print is gated by `if skipped_count > 0 and self.verbose:` and `self.verbose` is `False` | `src/tools/python/ac-static-verifier.py:165` |
| 3 | Why is `self.verbose` False? | The test constructs `ACVerifier("702", "code", tmppath)` without passing `verbose=True`, and the default is `False` | `src/tools/python/tests/test_ac_verifier_binary.py:190`, `src/tools/python/ac-static-verifier.py:67` |
| 4 | Why was the test not updated? | Commit `ad17c046` added the `verbose` parameter and gated warnings behind it but did not update the pre-existing test | Commit `ad17c046` |
| 5 | Why (Root)? | The commit changed runtime behavior (unconditional warning to verbose-only) without a corresponding test update for the test that specifically verifies that warning output | F702 AC#6 originally verified unconditional logging; `ad17c046` broke the contract silently |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | `test_skipped_files_warning_output` fails with empty stderr | Commit `ad17c046` gated warning output behind `self.verbose` without updating the test |
| Where | `src/tools/python/tests/test_ac_verifier_binary.py:202` assertion | `src/tools/python/ac-static-verifier.py:165` verbose gate + test line 190 missing `verbose=True` |
| Fix | Remove the verbose gate (makes warnings unconditional again) | Add `verbose=True` to the test's `ACVerifier` constructor call |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F798 | [WIP] | Parent feature that discovered this deviation during Phase 7 |
| F702 | [DONE] | Original feature that introduced binary file handling and this test |
| F792 | [DONE] | count_equals matcher feature that also touched ac-static-verifier |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Code change scope | FEASIBLE | Single line change: add `verbose=True` to constructor call at line 190 |
| Production impact | FEASIBLE | No production code changes needed; only test file modified |
| Regression risk | FEASIBLE | Other 4 tests in test_ac_verifier_binary.py are unaffected (none use verbose) |
| Design intent preservation | FEASIBLE | `verbose=False` default preserved per commit `ad17c046` design intent |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| ac-static-verifier test suite | HIGH | Restores 1 failing test to passing state, achieving full green suite |
| Production code | LOW | No changes to `src/tools/python/ac-static-verifier.py` |
| Other test files | LOW | No other tests reference the `verbose` parameter |
| CI pipeline | MEDIUM | Prevents false negatives from silently broken test |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `verbose=False` default is deliberate and must not be changed | Commit `ad17c046` design intent | Fix must add `verbose=True` to test, not change production default |
| `capsys` fixture requires pytest runner | `src/tools/python/tests/test_ac_verifier_binary.py:167` | Test cannot be verified via direct `python` execution; must use `pytest` |
| Warning message format must match exactly | `src/tools/python/ac-static-verifier.py:166` | Test assertion depends on exact format `INFO: Skipped {N} binary file(s) in {path}` |
| Other 4 tests in same file must remain passing | Test isolation and regression safety | Fix must only modify the failing test function |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Fix masks a deeper design issue | LOW | LOW | Commit `ad17c046` message is explicit about suppression intent; verbose=True in test correctly exercises the verbose path |
| Other tests break from change | LOW | LOW | No other tests reference verbose; change is isolated to one function |
| Warning message format changes in future | LOW | LOW | Format string at ac-static-verifier.py:166 is stable and well-defined |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Failing test count | `python -m pytest src/tools/python/tests/test_ac_verifier_binary.py::test_skipped_files_warning_output --tb=no -q` | 1 FAILED | Test fails with empty stderr |
| Full binary test suite | `python -m pytest src/tools/python/tests/test_ac_verifier_binary.py --tb=no -q` | 4 passed, 1 failed | Only test_skipped_files_warning_output fails |

**Baseline File**: `.tmp/baseline-799.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Test must pass with pytest runner | `capsys` fixture dependency at line 167 | AC verification must use `pytest` not direct `python` execution |
| C2 | Production code `verbose=False` default must be preserved | Commit `ad17c046` design decision | AC must NOT require changing `ac-static-verifier.py` default |
| C3 | Warning message format must match exactly | `src/tools/python/ac-static-verifier.py:166` format string | AC expected string must match `INFO: Skipped {N} binary file(s) in {path}` |
| C4 | All 5 tests in test_ac_verifier_binary.py must pass after fix | Regression safety | AC should verify full test file passes |
| C5 | Full ac-static-verifier test suite must pass | Pipeline Continuity philosophy | AC should verify `pytest src/tools/python/tests/test_ac_verifier_*.py` passes |

### Constraint Details

**C1: pytest Runner Required**
- **Source**: `capsys` is a pytest-specific fixture used at `src/tools/python/tests/test_ac_verifier_binary.py:167`
- **Verification**: Run `python -m pytest src/tools/python/tests/test_ac_verifier_binary.py::test_skipped_files_warning_output`
- **AC Impact**: All ACs verifying this test must use pytest as the test runner

**C2: Preserve verbose=False Default**
- **Source**: Commit `ad17c046` ("fix: suppress binary file warnings in ac-static-verifier by default") deliberately made verbose=False the default
- **Verification**: `ACVerifier.__init__` signature at `src/tools/python/ac-static-verifier.py:67` shows `verbose: bool = False`
- **AC Impact**: AC must verify the fix uses `verbose=True` in the test constructor, not changing the production default

**C3: Warning Message Format**
- **Source**: `src/tools/python/ac-static-verifier.py:166` -- `print(f"INFO: Skipped {skipped_count} binary file(s) in {file_path}", file=sys.stderr)`
- **Verification**: Read the format string at ac-static-verifier.py:166
- **AC Impact**: AC expected values must match this exact format

**C4: Full Binary Test Suite Pass**
- **Source**: 5 tests exist in `test_ac_verifier_binary.py` (binary_files_excluded, mixed_directory, directory_only_binary, skipped_files_warning_output, unicode_error_fallback)
- **Verification**: Run `python -m pytest src/tools/python/tests/test_ac_verifier_binary.py -v`
- **AC Impact**: AC must verify all 5 tests pass, not just the fixed one

**C5: Full Verifier Test Suite Pass**
- **Source**: Pipeline Continuity philosophy requires the entire verifier test suite to be green
- **Verification**: Run `python -m pytest src/tools/python/tests/test_ac_verifier_*.py`
- **AC Impact**: AC should include a broader regression check across all verifier test files

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F798 | [WIP] | Parent feature that discovered this deviation during Phase 7 |
| Related | F702 | [DONE] | Original feature that introduced binary file handling and the test |
| Related | F792 | [DONE] | count_equals/gte matchers feature that also modified ac-static-verifier |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

<!-- fc-phase-3-completed -->

## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "test suite must be fully green" | The previously failing test must now pass | AC#1 |
| "test suite must be fully green" | All 5 tests in the binary test file must pass | AC#3 |
| "test suite must be fully green" | All verifier test files must pass (no regression) | AC#4 |
| "reliable regression safety net" | The fix must not break other tests | AC#3, AC#4 |
| "test suite is the SSOT for verifier correctness" | The test must actually exercise the verbose warning code path | AC#2 |
| "any silently broken test undermines confidence" | Production default verbose=False must be preserved | AC#5, AC#6, AC#7 |
| "any silently broken test undermines confidence" | The fix must be in the test file, not the production code | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Fixed test passes via pytest | exit_code | Bash | succeeds | - | [x] |
| 2 | verbose=True is in test_skipped_files_warning_output function | code | Grep(src/tools/python/tests/test_ac_verifier_binary.py) | matches | `ACVerifier.*verbose=True` | [x] |
| 3 | All 5 binary test file tests pass | exit_code | Bash | succeeds | - | [x] |
| 4 | Full verifier test suite passes (no regression) | exit_code | Bash | succeeds | - | [x] |
| 5 | Production ACVerifier.__init__ default is verbose=False | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `verbose: bool = False` | [x] |
| 6 | Production verbose gate unchanged | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `if skipped_count > 0 and self.verbose:` | [x] |
| 7 | Production CLI --verbose flag preserved | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `"--verbose"` | [x] |
| 8 | Only test file contains the fix (verbose=True not in production) | code | Grep(src/tools/python/ac-static-verifier.py) | not_contains | `verbose=True` | [x] |

### AC Details

**AC#1: Fixed test passes via pytest**
- **Test**: `python -m pytest src/tools/python/tests/test_ac_verifier_binary.py::test_skipped_files_warning_output --tb=short -q`
- **Expected**: Exit code 0 (1 passed)
- **Rationale**: Directly verifies the fix resolves the failing test. Uses pytest runner per C1 constraint (capsys fixture requires pytest).

**AC#2: verbose=True is in test_skipped_files_warning_output function**
- **Test**: `Grep(path="src/tools/python/tests/test_ac_verifier_binary.py", pattern="ACVerifier.*verbose=True")`
- **Expected**: Matches pattern `ACVerifier.*verbose=True`
- **Rationale**: Ensures verbose=True is passed as a constructor argument to ACVerifier (not set as an attribute after construction or in a different function). This is the precise fix location at line 190.

**AC#3: All 5 binary test file tests pass**
- **Test**: `python -m pytest src/tools/python/tests/test_ac_verifier_binary.py -v --tb=short`
- **Expected**: Exit code 0 (5 passed)
- **Rationale**: Per C4, all 5 tests must pass. Ensures the fix does not regress the other 4 tests in the same file.

**AC#4: Full verifier test suite passes (no regression)**
- **Test**: `python -m pytest src/tools/python/tests/test_ac_verifier_*.py --tb=short -q`
- **Expected**: Exit code 0 (all tests pass)
- **Rationale**: Per C5 and the Pipeline Continuity philosophy, the entire verifier test suite must be green. Catches any cross-file regressions.

**AC#5: Production ACVerifier.__init__ default is verbose=False**
- **Test**: `Grep("verbose: bool = False", "src/tools/python/ac-static-verifier.py")`
- **Expected**: File contains `verbose: bool = False`
- **Rationale**: Per C2, the production default must remain False. Commit ad17c046 deliberately made verbose=False the default to suppress warnings in normal operation.

**AC#6: Production verbose gate unchanged**
- **Test**: `Grep("if skipped_count > 0 and self.verbose:", "src/tools/python/ac-static-verifier.py")`
- **Expected**: File contains `if skipped_count > 0 and self.verbose:`
- **Rationale**: Ensures the verbose gate logic at line 165 is not modified. The warning must remain conditional on self.verbose in production.

**AC#7: Production CLI --verbose flag preserved**
- **Test**: `Grep("--verbose", "src/tools/python/ac-static-verifier.py")`
- **Expected**: File contains `"--verbose"` (the argparse CLI flag definition)
- **Rationale**: Verifies the CLI `--verbose` flag introduced by commit ad17c046 remains intact. Combined with AC#5 (default) and AC#6 (gate logic), ensures the full verbose infrastructure is preserved.

**AC#8: Only test file contains the fix (verbose=True not in production)**
- **Test**: `Grep("verbose=True", "src/tools/python/ac-static-verifier.py")`
- **Expected**: Pattern NOT found in production file
- **Rationale**: Ensures the fix is confined to the test file. Production code must never contain verbose=True as a hardcoded value, preserving the design intent of ad17c046.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Fix test to pass verbose=True to ACVerifier constructor | AC#2 |
| 2 | Test correctly exercises verbose warning path | AC#1 |
| 3 | Restore test to passing state | AC#1, AC#3 |
| 4 | Without changing production default of verbose=False | AC#5, AC#6, AC#7, AC#8 |

---

<!-- fc-phase-4-completed -->

## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

The fix is a single-line change in `src/tools/python/tests/test_ac_verifier_binary.py` at line 190. The current call:

```python
verifier = ACVerifier("702", "code", tmppath)
```

must become:

```python
verifier = ACVerifier("702", "code", tmppath, verbose=True)
```

This change is the complete implementation. No production code in `src/tools/python/ac-static-verifier.py` is modified. The rationale: commit `ad17c046` deliberately gated binary-skip warnings behind `self.verbose` (default `False`) to suppress noise in normal operation. The test `test_skipped_files_warning_output` is specifically designed to verify that warning output is produced; it must opt in to verbose mode to exercise that code path. Adding `verbose=True` at construction time makes the verifier instance emit the warning to stderr during `_expand_glob_path`, satisfying the `capsys` assertion at line 202.

All 8 ACs are satisfied by this single change:
- AC#1: The test now passes because stderr receives the expected warning.
- AC#2: The keyword `verbose=True` appears in the test file, scoped to the `ACVerifier(...)` constructor call.
- AC#3: All 5 tests in `test_ac_verifier_binary.py` pass (the other 4 are unaffected by this change).
- AC#4: Full verifier test suite (`test_ac_verifier_*.py`) passes with no cross-file regressions.
- AC#5, AC#6: Production `ac-static-verifier.py` is untouched; `verbose: bool = False` default and `if skipped_count > 0 and self.verbose:` gate remain exactly as committed in `ad17c046`.
- AC#7: The CLI `--verbose` flag definition remains in production code.
- AC#8: The string `verbose=True` does not appear anywhere in the production file.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Adding `verbose=True` to the constructor causes `_expand_glob_path` to emit the warning; `capsys` captures it; line 202 assertion passes; pytest exits 0 |
| 2 | After the edit, the pattern `ACVerifier.*verbose=True` matches line 190 of the test file |
| 3 | The 4 other tests (`binary_files_excluded`, `mixed_directory`, `directory_only_binary`, `unicode_error_fallback`) do not pass `verbose` and do not assert on stderr content; they are unaffected |
| 4 | No other verifier test file references binary-skip warning behavior; the fix is isolated to one function and introduces no API changes |
| 5 | `src/tools/python/ac-static-verifier.py` is not touched; `ACVerifier.__init__` signature at line 67 retains `verbose: bool = False` |
| 6 | `src/tools/python/ac-static-verifier.py` is not touched; verbose gate at line 165 (`if skipped_count > 0 and self.verbose:`) is unchanged |
| 7 | `src/tools/python/ac-static-verifier.py` is not touched; the `--verbose` argparse CLI flag definition at line 1157 remains intact |
| 8 | `verbose=True` appears only in the test file (the fix); production `ac-static-verifier.py` never contains this literal |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|--------------------|----------|-----------|
| Where to apply the fix | (A) Add `verbose=True` to test constructor; (B) Make warning unconditional in production | (A) Add `verbose=True` to test constructor | Option B would revert the deliberate design of commit `ad17c046` which explicitly suppressed warnings by default. Option A preserves production behavior and correctly exercises the verbose code path that the test is meant to cover. |
| Scope of change | (A) One line in one function; (B) Refactor test setup | (A) One line in one function | Minimum viable fix. Root cause is a missing keyword argument; no broader refactoring is warranted. |
| Constructor argument style | (A) Positional: `ACVerifier("702", "code", tmppath, True)`; (B) Keyword: `ACVerifier("702", "code", tmppath, verbose=True)` | (B) Keyword | AC#2 pattern `ACVerifier.*verbose=True` requires keyword form. Keyword form is also more readable and resilient to future signature changes. |

### Interfaces / Data Structures

<!-- N/A for this feature -->

The only interface involved is `ACVerifier.__init__(feature_id, ac_type, base_path, verbose=False)` in `src/tools/python/ac-static-verifier.py`. Its signature is not changed. The fix is purely a call-site update in the test.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| None | - | - |

---

<!-- fc-phase-5-completed -->

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2 | Add `verbose=True` to `ACVerifier` constructor call in `test_skipped_files_warning_output` at `src/tools/python/tests/test_ac_verifier_binary.py:190` | | [x] |
| 2 | 3, 4 | Verify all 5 tests in `test_ac_verifier_binary.py` pass and no regression in full verifier test suite | | [x] |
| 3 | 5, 6, 7, 8 | Confirm production `src/tools/python/ac-static-verifier.py` is unmodified (verbose=False default and verbose gate intact) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | `src/tools/python/tests/test_ac_verifier_binary.py` line 190: add `verbose=True` to `ACVerifier("702", "code", tmppath)` constructor call | Modified test file with `ACVerifier("702", "code", tmppath, verbose=True)` at line 190 |
| 2 | ac-tester | sonnet | All 8 ACs in this feature | AC verification results (all [ ] → [x]) |

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-21 | START | initializer | Status [REVIEWED]→[WIP] | OK |
| 2026-02-21 | IMPL | implementer | Task#1 verbose=True added at line 190 | SUCCESS |
| 2026-02-21 | VERIFY | ac-tester | AC#1-8 verification | 8/8 PASS |
| 2026-02-21 | VERIFY | ac-static-verifier | code ACs #2,#5-8 | 5/5 PASS |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [resolved-applied] Phase2-Pending iter1: [AC-003] AC#4 is a duplicate of AC#1 — both verify the same pytest exit code 0 for test_skipped_files_warning_output with different flags. Creates false coverage depth in Goal Coverage table. Fix: remove AC#4 and redistribute Goal Coverage to AC#1, or replace AC#4 with an independent verification (e.g., Bash-level stderr capture).
- [fix] PostLoop-UserFix post-loop: AC Definition Table | Removed duplicate AC#4, renumbered AC#5-10 → AC#4-9, updated all cross-references (Goal Coverage, Tasks, Technical Design, Implementation Contract)
- [fix] Phase2-Review iter2: AC Definition Table | Removed redundant AC#2 (subset of AC#3), renumbered AC#3-9 → AC#2-8, updated all cross-references
- [fix] Phase2-Review iter3: Philosophy Derivation | Moved AC#7 from "fix must be in test file" row to "Production default verbose=False must be preserved" row (better logical fit)

---

<!-- fc-phase-6-completed -->

## Links
[Related: F798](feature-798.md) - Parent feature that discovered this deviation during Phase 7
[Related: F702](archive/feature-702.md) - Original feature that introduced binary file handling and this test
[Related: F792](feature-792.md) - count_equals matcher feature that also modified ac-static-verifier
