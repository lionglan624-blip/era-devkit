# Feature 818: ac-static-verifier Cross-Repo and WSL Support

## Status: [PROPOSED]

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
| Parent Feature | F813 |
| Discovery Phase | Phase 9 |
| Timestamp | 2026-03-04 |

### Observable Symptom
ac-static-verifier.py fails for cross-repo features: (1) code type ACs with paths in C:\Era\core are rejected as "not in subpath of C:\Era\devkit", (2) file type AC Glob patterns for core repo paths return "not found", (3) build type ACs run native dotnet instead of WSL, failing with NU1301 NuGet source errors.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python src/tools/python/ac-static-verifier.py --feature 813 --ac-type code` |
| Exit Code | 1 |
| Error Output | `Error: 'C:\Era\core\src\Era.Core\Counter\CounterMessage.cs' is not in the subpath of 'C:\Era\devkit'` |
| Expected | Cross-repo Grep paths resolve correctly |
| Actual | Verifier rejects paths outside devkit root |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Main verifier script - path validation too strict |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | PRE-EXISTING limitation discovered during F813 verification |

### Parent Session Observations
F813 is the first cross-repo infra feature to run ac-static-verifier. Previous features were either devkit-only or used manual ac-tester verification. The verifier needs: (1) repo-root override parameter or multi-repo config, (2) WSL dotnet execution for build ACs (matching pre-commit hook pattern), (3) cross-repo Glob/Grep support.

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->
## Background

### Philosophy (Mid-term Vision)
Pipeline Continuity -- ac-static-verifier is the SSOT for automated AC verification across all feature types. Cross-repo features (those targeting files in `C:\Era\core`, `C:\Era\engine`, `C:\Era\game`) must have the same automated verification coverage as single-repo devkit features for code-type and file-type ACs. Build-type ACs targeting cross-repo solution/project files require WSL-compatible paths in the build command (see constraint C8). This ensures the 5-repo ecosystem benefits from static AC verification for the most common AC types.

### Problem (Current Issue)
The ac-static-verifier was designed as a single-repo tool in F268 -- all path resolution is anchored to a single `self.repo_root` with no mechanism to detect or handle absolute paths pointing to external repositories. This causes three distinct failures for cross-repo features like F813:

1. **Path reporting crash**: `_expand_glob_path` joins `self.repo_root / file_path` (line 133), but on Windows, Python's `Path.__truediv__` silently returns the absolute right-hand path when it IS absolute, so file I/O succeeds. However, the 5 `relative_to(self.repo_root)` call sites (lines 196, 546, 576, 661, 1010) then raise `ValueError` because the found file is under `C:\Era\core`, not `C:\Era\devkit`. The error message ("not in subpath") is misleading -- the file was found but cannot be converted to a relative display path.

2. **Build AC uses native dotnet**: `verify_build_ac` runs `subprocess.run(build_command.split())` (lines 888-895) invoking native Windows dotnet, but Smart App Control blocks native dotnet execution. The pre-commit hook has the correct WSL wrapper pattern (`.githooks/pre-commit:10-12`) but this was never propagated to the verifier.

3. **Output directory mismatch**: Line 1146 writes to `self.repo_root / "Game" / "logs"` (legacy pre-5-zone path) while the docstring (line 15), CLAUDE.md File Placement rules, and `verify-logs.py` (line 254) all expect `_out/logs/prod/ac/`.

Because no `is_absolute()` check exists anywhere in the verifier, and no concept of multiple repository roots exists, all cross-repo features are excluded from automated AC verification.

### Goal (What to Achieve)
Enable ac-static-verifier to verify ACs for cross-repo features by: (1) detecting absolute paths and resolving them directly without repo_root prepend, (2) handling `relative_to` safely for files outside repo_root using a centralized helper, (3) wrapping build AC commands in WSL following the pre-commit hook pattern, and (4) correcting the output directory to `_out/logs/prod/ac/`.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does the verifier crash on cross-repo paths? | `relative_to(self.repo_root)` raises `ValueError` when matched file is under a different repo root | `ac-static-verifier.py:196` (and 4 other sites) |
| 2 | Why does `relative_to` receive cross-repo paths? | `_expand_glob_path` joins `self.repo_root / file_path`, but Python pathlib on Windows returns the absolute right-hand path as-is, so the file IS found under `C:\Era\core` | `ac-static-verifier.py:133` |
| 3 | Why is there no detection of absolute vs relative paths? | No `Path.is_absolute()` check exists anywhere in the verifier; no concept of multiple repository roots | Zero matches for `is_absolute` in `ac-static-verifier.py` |
| 4 | Why was multi-repo never considered? | The verifier was created in F268 for devkit-only features; all prior features (F268 through F813) targeted only devkit-local paths | `feature-813.md:722-724` (3 DEVIATION entries) |
| 5 | Why (Root)? | F813 was the first cross-repo feature to exercise the verifier, exposing the structural single-repo design assumption that was never tested against external paths | `ac-static-verifier.py:67-72` (single `repo_root` constructor) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | `ValueError: 'C:\Era\core\...' is not in the subpath of 'C:\Era\devkit'` | Single-repo design assumption: no absolute path detection, no multi-root support, no `is_absolute()` check |
| Where | 5 `relative_to()` call sites in reporting layer (lines 196, 546, 576, 661, 1010) | Constructor accepts only one `repo_root` (`ac-static-verifier.py:67-72`); `_expand_glob_path` unconditionally joins repo_root (line 133) |
| Fix | Wrap each `relative_to` in try/except | Add `is_absolute()` detection, centralized safe-relative helper, WSL build wrapper, and correct output directory |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F813 | [DONE] | Parent -- first cross-repo feature to expose the limitation |
| F268 | [DONE] | Original creator of ac-static-verifier.py |
| F817 | [DONE] | Most recent verifier fix (pipe-escaping) |
| F798 | [DONE] | Verifier regex/GTE parser fixes |
| F792 | [DONE] | Verifier count_equals/pipe escape |
| F814 | [DRAFT] | Phase 22 Planning -- likely to create more cross-repo features |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Path resolution fix | FEASIBLE | Python `Path.is_absolute()` provides clean detection; `os.path.relpath` or try/except for safe relative paths |
| WSL build wrapper | FEASIBLE | Pattern exists in `.githooks/pre-commit:10-12` (`wsl_dotnet()` function) |
| Output directory fix | FEASIBLE | Single line change: `"Game"` to `"_out"` on line 1146 |
| Backward compatibility | FEASIBLE | All existing single-repo features use relative paths; `is_absolute()` check is purely additive |
| Test infrastructure | FEASIBLE | 23 existing test files following established patterns; `tmp_path` fixtures for cross-repo simulation |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Cross-repo AC verification | HIGH | Enables automated verification for F813 and future cross-repo features (F814+) |
| Build AC execution | HIGH | Fixes NU1301 NuGet failures by using WSL wrapper matching pre-commit hook pattern |
| Log consumer compatibility | MEDIUM | Corrects output dir so `verify-logs.py` can find AC verification results |
| Existing single-repo features | LOW | No behavioral change for relative paths; `is_absolute()` detection is additive |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Smart App Control blocks native dotnet | CLAUDE.md WSL section | Build ACs MUST use WSL wrapper with `/home/siihe/.dotnet/dotnet` |
| `Path.relative_to()` raises ValueError for unrelated trees | Python pathlib stdlib | Cross-repo paths require safe alternative (e.g., `os.path.relpath` or try/except) |
| Single `--repo-root` CLI argument | `ac-static-verifier.py:1174-1177` | Current CLI only accepts one root; may need `--allow-external-paths` or auto-detection |
| AC path format uses forward slashes | F813 AC convention | Paths like `C:/Era/core/...` work in Python pathlib on Windows |
| Python `Path.__truediv__` replaces base when right is absolute | Python pathlib | Absolute paths bypass repo_root prepend silently; error surfaces later at `relative_to` |
| 5 `relative_to` call sites follow identical pattern | Lines 196, 546, 576, 661, 1010 | All must be updated consistently; centralized helper method recommended |
| Backward compatibility with 275+ existing features | All prior features | Changes must not break existing single-repo feature verification |
| Environment variables define repo paths | CLAUDE.md | `CORE_PATH`, `ENGINE_PATH`, `GAME_PATH` available but optional |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| WSL subprocess hanging or timing out | LOW | HIGH | Use `timeout` parameter in subprocess call |
| Path normalization differences across Windows/WSL | MEDIUM | MEDIUM | Use `Path.resolve()` for consistent normalization |
| Breaking existing single-repo test suite | LOW | HIGH | 23 existing test files provide regression coverage |
| Environment variables not set in some contexts | LOW | MEDIUM | Fall back to absolute path detection (no env var dependency) |
| WSL path conversion errors (`C:\` to `/mnt/c/`) | LOW | MEDIUM | Reuse exact pattern from `.githooks/pre-commit` |
| WSL subprocess adds latency to build ACs | MEDIUM | LOW | Acceptable trade-off for correctness |
| Output dir change breaks consumers of old `Game/logs/` path | LOW | LOW | Old path was wrong per CLAUDE.md; clean break appropriate |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Cross-repo AC pass rate | `python src/tools/python/ac-static-verifier.py --feature 813 --ac-type code` | 0% (crashes with ValueError) | All cross-repo ACs fail |
| Build AC pass rate | `python src/tools/python/ac-static-verifier.py --feature 813 --ac-type build` | 0% (NU1301 NuGet error) | Native dotnet blocked by Smart App Control |
| Existing test count | Count of test files in `src/tools/python/tests/test_ac_verifier_*.py` | 23 files | Regression baseline |
| Output directory | Grep `"Game" / "logs"` in `ac-static-verifier.py` | 1 occurrence (line 1146) | Legacy path |

**Baseline File**: `_out/tmp/baseline-818.txt` (to be generated at /run Phase 2)

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Cross-repo paths use absolute `C:/Era/{repo}/...` format | F813 AC table | ACs must test both absolute (cross-repo) and relative (same-repo) paths |
| C2 | WSL dotnet must use specific binary path `/home/siihe/.dotnet/dotnet` | `.githooks/pre-commit:10-12` | Build AC test must verify WSL invocation pattern |
| C3 | Output dir must be `_out/logs/prod/ac/...` | CLAUDE.md File Placement + `verify-logs.py:254` | AC must verify output goes to `_out/`, not `Game/` |
| C4 | All 5 `relative_to` sites must handle cross-repo paths | Lines 196, 546, 576, 661, 1010 | ACs must cover each matcher type (matches, not_matches, count, file) with cross-repo paths |
| C5 | Backward compatibility with 275+ existing features | All prior features | ACs must include regression test for single-repo verification |
| C6 | Windows-to-WSL path conversion required for build ACs | WSL2 environment | Build AC must translate `C:\Era\...` to `/mnt/c/Era/...` |
| C7 | Tests can use temp dirs to simulate cross-repo | Test isolation | Tests should not depend on `C:\Era\core` existence; use `tmp_path` fixtures |
| C8 | WSL build wrapper converts cwd only, not build_command arguments | Technical Design WSL subprocess pattern | Build ACs targeting cross-repo solution/project files must specify WSL-compatible paths in the build command (e.g., `/mnt/c/Era/core/Core.sln`). No automation needed: AC authors specify WSL paths directly. |

### Constraint Details

**C1: Cross-repo Path Format**
- **Source**: F813 AC Definition Table contains ~20 ACs with `C:/Era/core/` paths using forward-slash Windows absolute format
- **Verification**: Check F813 feature file AC table for path patterns
- **AC Impact**: ac-designer must include ACs testing absolute path resolution for Grep, Glob, and content matching

**C2: WSL Dotnet Binary Path**
- **Source**: `.githooks/pre-commit` lines 10-12 define `wsl_dotnet()` function with hardcoded path
- **Verification**: Run `wsl -- /home/siihe/.dotnet/dotnet --version` to confirm
- **AC Impact**: Build AC must verify the subprocess command starts with WSL invocation, not native dotnet

**C3: Output Directory Correction**
- **Source**: CLAUDE.md File Placement table specifies `_out/logs/prod/ac/`; `verify-logs.py:254` defaults to `_out/logs/prod`
- **Verification**: Grep for `"Game" / "logs"` in `ac-static-verifier.py`
- **AC Impact**: AC must verify no `Game/logs` reference remains in output path construction

**C4: Five relative_to Crash Sites**
- **Source**: Lines 196 (`_search_pattern_native`), 546/576/661 (`_verify_content` matches/not_matches/count), 1010 (`verify_file_ac`)
- **Verification**: Grep for `relative_to` in `ac-static-verifier.py`
- **AC Impact**: Each matcher type needs a cross-repo path test to confirm no ValueError

**C5: Backward Compatibility**
- **Source**: 275+ features with relative paths; 23 existing test files
- **Verification**: Run existing test suite after changes
- **AC Impact**: Include regression test verifying existing single-repo patterns still work

**C6: WSL Path Conversion**
- **Source**: WSL mounts Windows drives at `/mnt/{drive_letter}/`
- **Verification**: `wsl -- ls /mnt/c/Era/devkit` confirms mount
- **AC Impact**: Build AC wrapper must convert `C:\Era\devkit` to `/mnt/c/Era/devkit`

**C7: Test Isolation**
- **Source**: Test best practices; CI environments may lack full 5-repo setup
- **Verification**: Tests use `tmp_path` or `tempfile` to create simulated cross-repo directory structures
- **AC Impact**: No hard dependency on `C:\Era\core` existence in unit tests

**C8: WSL Build Wrapper Cwd Scope**
- **Source**: Technical Design WSL subprocess pattern — `_convert_to_wsl_path` applies to `cd` target only, `build_args` passed as-is
- **Verification**: Review `verify_build_ac` implementation; confirm only `self.repo_root` is converted, not `build_command` arguments
- **AC Impact**: No AC needed for argument conversion; AC authors specify WSL-compatible paths directly in build commands (e.g., `dotnet build /mnt/c/Era/core/Core.sln`)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F813 | [DONE] | Parent feature that discovered the cross-repo limitation |
| Related | F817 | [DONE] | Most recent verifier fix (pipe-escaping); changes may overlap |
| Related | F268 | [DONE] | Original verifier creation |

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

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Cross-repo features must have the same automated verification coverage...for code-type and file-type ACs" | Absolute paths detected and resolved without repo_root prepend | AC#1, AC#15 |
| "Cross-repo features must have the same automated verification coverage...for code-type and file-type ACs" | All 5 relative_to crash sites replaced with centralized safe helper | AC#2, AC#3, AC#4 |
| "Pipeline Continuity -- ac-static-verifier is the SSOT for automated AC verification" | Output directory follows CLAUDE.md File Placement rules | AC#5, AC#6 |
| "Pipeline Continuity -- ac-static-verifier is the SSOT for automated AC verification" | Build ACs execute via WSL matching pre-commit hook pattern | AC#7, AC#8, AC#9, AC#18, AC#19, AC#21, AC#22, AC#23 |
| "The 5-repo ecosystem benefits from static AC verification for the most common AC types" | Backward compatibility with existing single-repo verification | AC#10, AC#20 |
| "The 5-repo ecosystem benefits from static AC verification for the most common AC types" | Test coverage exists for cross-repo path handling | AC#11, AC#12, AC#13, AC#14, AC#15, AC#16, AC#17 |
| "Build-type ACs targeting cross-repo...require WSL-compatible paths (C8)" | Build_command arguments not auto-converted; authors specify WSL paths | (Constraint C8 — no AC; acknowledged limitation) |

### AC Definition Table

**Note**: 23 ACs (exceeds 8-15 guideline; justified by constraint C4 requiring per-matcher-type tests for all 4 types + WSL path conversion coverage + helper method existence + relative path regression + WSL behavioral tests + timeout). Covers all 3 failure modes + backward compatibility + test coverage + complete per-matcher behavioral verification + WSL build behavioral coverage.

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|:----:|--------|:-------:|----------|:------:|
| 1 | `_expand_glob_path` detects absolute paths | code | Grep(src/tools/python/ac-static-verifier.py) | contains | is_absolute | [ ] |
| 2 | Safe relative path helper exists | code | Grep(src/tools/python/ac-static-verifier.py) | matches | def _safe_relative | [ ] |
| 3 | Safe relative helper is called at all crash sites | code | Grep(src/tools/python/ac-static-verifier.py) | gte | self._safe_relative_path( (5) | [ ] |
| 4 | Only `_safe_relative_path` body contains `relative_to(self.repo_root)` | code | Grep(src/tools/python/ac-static-verifier.py) | count_equals | .relative_to(self.repo_root) (1) | [ ] |
| 5 | Output directory uses `_out` not `Game` | code | Grep(src/tools/python/ac-static-verifier.py) | not_contains | "Game" / "logs" | [ ] |
| 6 | Output directory path correct | code | Grep(src/tools/python/ac-static-verifier.py) | contains | "_out" / "logs" / "prod" / "ac" | [ ] |
| 7 | Build AC uses WSL subprocess | code | Grep(src/tools/python/ac-static-verifier.py) | contains | "wsl", "--", "bash" | [ ] |
| 8 | Build AC converts Windows path to WSL mount path | code | Grep(src/tools/python/ac-static-verifier.py) | contains | /mnt/ | [ ] |
| 9 | Build AC references WSL dotnet binary | code | Grep(src/tools/python/ac-static-verifier.py) | contains | .dotnet/dotnet | [ ] |
| 10 | Existing test suite passes | test | Bash | succeeds | python -m pytest src/tools/python/tests/test_ac_verifier*.py | [ ] |
| 11 | Cross-repo path test exists | code | Grep(src/tools/python/tests/) | contains | is_absolute | [ ] |
| 12 | Cross-repo safe relative test exists | code | Grep(src/tools/python/tests/) | contains | _safe_relative | [ ] |
| 13 | Cross-repo not_matches matcher test exists | code | Grep(src/tools/python/tests/) | contains | test_cross_repo_not_matches | [ ] |
| 14 | Cross-repo file matcher test exists | code | Grep(src/tools/python/tests/) | contains | test_cross_repo_file | [ ] |
| 15 | `_expand_glob_path` absolute path bypass test exists | code | Grep(src/tools/python/tests/) | contains | test_expand_glob_path_absolute | [ ] |
| 16 | Cross-repo matches matcher test exists | code | Grep(src/tools/python/tests/) | contains | test_cross_repo_matches | [ ] |
| 17 | Cross-repo count matcher test exists | code | Grep(src/tools/python/tests/) | contains | test_cross_repo_count | [ ] |
| 18 | WSL path conversion test exists | code | Grep(src/tools/python/tests/) | contains | test_convert_to_wsl_path | [ ] |
| 19 | `_convert_to_wsl_path` helper method exists | code | Grep(src/tools/python/ac-static-verifier.py) | matches | def _convert_to_wsl_path | [ ] |
| 20 | Relative path regression test exists | code | Grep(src/tools/python/tests/) | contains | test_expand_glob_path_relative | [ ] |
| 21 | WSL build subprocess behavioral test exists | code | Grep(src/tools/python/tests/) | contains | test_verify_build_ac_uses_wsl | [ ] |
| 22 | Cross-repo build path conversion test exists | code | Grep(src/tools/python/tests/) | contains | test_verify_build_ac_cross_repo_path | [ ] |
| 23 | WSL subprocess has timeout parameter | code | Grep(src/tools/python/ac-static-verifier.py) | contains | timeout= | [ ] |

### AC Details

**AC#3: Safe relative helper is called at all crash sites**
- **Rationale**: The 5 `relative_to(self.repo_root)` call sites (lines 196, 546, 576, 661, 1010) must each be replaced with `self._safe_relative_path(...)`. The `gte` matcher with threshold 5 verifies at least 5 calls exist, ensuring all crash sites invoke the centralized helper rather than raw `relative_to`.
- **Derivation**: 5 call sites identified in Root Cause Analysis lines 196, 546, 576, 661, 1010. Each replacement produces one `self._safe_relative_path(` call. Threshold = 5 (minimum required replacements).

**AC#4: Only `_safe_relative_path` body contains `relative_to(self.repo_root)`**
- **Rationale**: There are currently 5 occurrences of `.relative_to(self.repo_root)` at lines 196, 546, 576, 661, and 1010. After replacement, only 1 occurrence should remain: inside the `_safe_relative_path` helper body itself (which wraps the call in try/except). The `count_equals` matcher with expected `1` verifies all 5 raw call sites are replaced while correctly accounting for the helper's own usage.

**AC#5: Output directory uses `_out` not `Game`**
- **Rationale**: Line 1146 uses `self.repo_root / "Game" / "logs"` which is the legacy pre-5-zone path. Verifying the `"Game" / "logs"` pathlib join pattern is absent confirms the fix. AC#6 positively confirms the replacement path. The Technical Design constrains implementation to pathlib `/` operator syntax, making the `not_contains` pattern reliable.

**AC#9: Build AC references WSL dotnet binary**
- **Rationale**: Per constraint C2, the WSL dotnet binary path `/home/siihe/.dotnet/dotnet` must be used. This AC verifies the path is referenced in the build verification code, matching the `.githooks/pre-commit` pattern.

**AC#13: Cross-repo not_matches matcher test exists**
- **Rationale**: Per constraint C4, the `not_matches` matcher type must work with cross-repo absolute paths. A test named `test_cross_repo_not_matches` verifies this specific matcher type handles absolute paths through `_safe_relative_path`.

**AC#14: Cross-repo file matcher test exists**
- **Rationale**: Per constraint C4, the `file` matcher type must work with cross-repo absolute paths. A test named `test_cross_repo_file` verifies file-type ACs handle absolute paths through `_safe_relative_path`.

**AC#15: `_expand_glob_path` absolute path bypass test exists**
- **Rationale**: AC#1 verifies the `is_absolute()` check EXISTS in code, but does not verify behavioral correctness. A unit test named `test_expand_glob_path_absolute` verifies that when given an absolute path (e.g., `C:/Era/core/src/File.cs`), the function returns the absolute path directly WITHOUT prepending `repo_root`. This closes the semantic gap between "code presence" (AC#1) and "correct behavior" (AC#15).

**AC#16: Cross-repo matches matcher test exists**
- **Rationale**: Per constraint C4 line 546, the `matches` matcher in `_verify_content` calls `_safe_relative_path`. A test named `test_cross_repo_matches` verifies this matcher type handles absolute cross-repo paths correctly.

**AC#17: Cross-repo count matcher test exists**
- **Rationale**: Per constraint C4 line 661, the `count` matcher in `_verify_content` calls `_safe_relative_path`. A test named `test_cross_repo_count` verifies this matcher type handles absolute cross-repo paths correctly.

**AC#18: WSL path conversion test exists**
- **Rationale**: `_convert_to_wsl_path` is a new method with non-trivial logic (drive letter extraction, backslash conversion, `/mnt/` prefix). A behavioral test named `test_convert_to_wsl_path` verifies correct conversion for both forward-slash (`C:/Era/devkit`) and backslash (`C:\Era\devkit`) Windows paths.

**AC#19: `_convert_to_wsl_path` helper method exists**
- **Rationale**: Ensures the WSL path conversion is implemented as a centralized helper method (not inlined). This is required for testability (AC#18) and maintainability (single point of change for path conversion logic).

**AC#10: Existing test suite passes**
- **Rationale**: Backward compatibility verification per constraint C5. The 23 existing test files cover single-repo feature verification patterns. Passing all existing tests after changes ensures no regression for 275+ existing features.

**AC#20: Relative path regression test exists**
- **Rationale**: AC#10 broadly verifies existing tests pass, but no specific AC confirms `_expand_glob_path` with relative paths still prepends `repo_root`. A targeted test `test_expand_glob_path_relative` explicitly verifies that relative path behavior is preserved after adding the `is_absolute()` guard, closing the semantic gap between broad test pass (AC#10) and specific behavioral preservation.

**AC#21: WSL build subprocess behavioral test exists**
- **Rationale**: AC#7-AC#9 verify code presence (WSL invocation text exists in source), but do not verify behavioral correctness (that `verify_build_ac` actually emits a WSL subprocess command when invoked). A behavioral test `test_verify_build_ac_uses_wsl` mocks subprocess and asserts the command list starts with `["wsl", "--", "bash", "-c", ...]`.

**AC#22: Cross-repo build path conversion test exists**
- **Rationale**: The WSL build wrapper applies `_convert_to_wsl_path` to `self.repo_root` for the `cd` target. A behavioral test `test_verify_build_ac_cross_repo_path` verifies that `verify_build_ac` correctly converts the repo_root to a WSL mount path in the subprocess `cd` command, ensuring the WSL invocation targets the correct directory.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Detecting absolute paths and resolving them directly without repo_root prepend | AC#1, AC#15 |
| 2 | Handling relative_to safely for files outside repo_root using a centralized helper | AC#2, AC#3, AC#4 |
| 3 | Correcting the output directory to `_out/logs/prod/ac/` | AC#5, AC#6 |
| 4 | Wrapping build AC commands in WSL following the pre-commit hook pattern | AC#7, AC#8, AC#9, AC#18, AC#19, AC#21, AC#22, AC#23 |
| 5 | Backward compatibility with existing single-repo verification | AC#10, AC#20 |
| 6 | Test coverage for cross-repo path handling | AC#11, AC#12, AC#13, AC#14, AC#15, AC#16, AC#17 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Four targeted fixes applied to `src/tools/python/ac-static-verifier.py` (~1199 lines), each isolated to a specific failure mode identified in the Root Cause Analysis. No architectural changes; the single-repo design is extended additively.

1. **Cross-repo path detection** (AC#1): Add `Path.is_absolute()` guard in `_expand_glob_path` before the `self.repo_root / file_path` join. When the path is absolute, assign `target_file = file_path_obj` directly. Applied at two code paths: the comma-separated branch (line 116) and the single-pattern branch (line 133).

2. **Centralized safe-relative helper** (AC#2, AC#3, AC#4): Add `_safe_relative_path(self, path: Path) -> str` instance method that wraps `path.relative_to(self.repo_root)` in a try/except, falling back to `str(path)` on `ValueError`. Replace all 5 raw `relative_to(self.repo_root)` call sites (lines 196, 546, 576, 661, 1010) with `self._safe_relative_path(...)`. This eliminates the `ValueError` crash for files outside `repo_root` while preserving relative path display for in-repo files.

3. **Output directory correction** (AC#5, AC#6): Line 1146 changes `self.repo_root / "Game" / "logs" / "prod" / "ac"` to `self.repo_root / "_out" / "logs" / "prod" / "ac"`. Single-line fix aligning with CLAUDE.md File Placement and `verify-logs.py:254`.

4. **WSL build wrapper** (AC#7, AC#8, AC#9): Add `_convert_to_wsl_path(self, windows_path: str) -> str` helper that converts `C:/Era/devkit` (or backslash form) to `/mnt/c/Era/devkit`. In `verify_build_ac`, replace `subprocess.run(build_command.split(), ...)` with a WSL-wrapped invocation: `["wsl", "--", "bash", "-c", f"cd {wsl_repo_root} && {wsl_dotnet} {build_command}"]`. Uses `MSYS_NO_PATHCONV=1` as environment variable and hardcoded `/home/siihe/.dotnet/dotnet` per constraint C2.

5. **Tests** (AC#10, AC#11-AC#22): Existing 23 test files provide regression coverage (AC#10). New `test_ac_verifier_cross_repo.py` created with 11 test functions: `is_absolute` path detection (AC#11), `_safe_relative_path` cross-repo behavior (AC#12), per-matcher cross-repo tests for not_matches/file/matches/count (AC#13-AC#17), `_expand_glob_path` absolute path bypass (AC#15), `_convert_to_wsl_path` conversion (AC#18), `_expand_glob_path` relative path regression (AC#20), `verify_build_ac` WSL subprocess behavioral test (AC#21), and cross-repo build path conversion test (AC#22). AC#19 is satisfied by `_convert_to_wsl_path` existence in the implementation (Task 5). Tests use `tmp_path` fixtures per constraint C7; no dependency on `C:\Era\core` existence.

**AC Coverage**: AC#1 through AC#23

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | `_expand_glob_path` gains `file_path_obj.is_absolute()` check at both branch entry points (comma-separated and single-pattern) |
| 2 | New `def _safe_relative_path(self, path: Path) -> str` method added to `ACVerifier` class |
| 3 | At least 5 `self._safe_relative_path(` calls exist (one per replaced crash site) |
| 4 | After replacement, `count_equals` Grep on `.relative_to(self.repo_root)` finds exactly 1 occurrence (inside `_safe_relative_path` body only) |
| 5 | Line 1146: `"Game"` removed from path construction; `not_contains` Grep on `"Game" / "logs"` finds zero occurrences |
| 6 | Line 1146: `"_out"` added to path construction; `contains` Grep on `"_out" / "logs" / "prod" / "ac"` finds one occurrence |
| 7 | WSL-wrapped subprocess command contains `"wsl", "--", "bash"` list pattern in `verify_build_ac` |
| 8 | `_convert_to_wsl_path` produces `/mnt/` prefix; WSL command string references `/mnt/` |
| 9 | Hardcoded `/home/siihe/.dotnet/dotnet` referenced in `verify_build_ac` WSL invocation; `.dotnet/dotnet` satisfies `contains` match |
| 10 | All 23 existing test files continue to pass after changes; existing single-repo relative-path behavior unchanged |
| 11 | `test_ac_verifier_cross_repo.py` contains `is_absolute` string (tests call `_expand_glob_path` with absolute path) |
| 12 | `test_ac_verifier_cross_repo.py` contains `_safe_relative` string (tests call `_safe_relative_path` directly) |
| 13 | `test_ac_verifier_cross_repo.py` contains `test_cross_repo_not_matches` test function |
| 14 | `test_ac_verifier_cross_repo.py` contains `test_cross_repo_file` test function |
| 15 | `test_ac_verifier_cross_repo.py` contains `test_expand_glob_path_absolute` test function |
| 16 | `test_ac_verifier_cross_repo.py` contains `test_cross_repo_matches` test function |
| 17 | `test_ac_verifier_cross_repo.py` contains `test_cross_repo_count` test function |
| 18 | `test_ac_verifier_cross_repo.py` contains `test_convert_to_wsl_path` test function |
| 19 | `def _convert_to_wsl_path` method defined in `ACVerifier` class |
| 20 | `test_ac_verifier_cross_repo.py` contains `test_expand_glob_path_relative` test function verifying relative paths still prepend repo_root |
| 21 | `test_ac_verifier_cross_repo.py` contains `test_verify_build_ac_uses_wsl` test function verifying WSL subprocess invocation |
| 22 | `test_ac_verifier_cross_repo.py` contains `test_verify_build_ac_cross_repo_path` test function verifying cross-repo build path conversion |
| 23 | WSL subprocess.run includes `timeout=` parameter to prevent indefinite hangs (per Risks table mitigation) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Safe relative path strategy | try/except in `_safe_relative_path`, `os.path.relpath` | try/except | Idiomatic Python pathlib; `os.path.relpath` produces unhelpful `../../` paths for cross-repo |
| WSL dotnet binary path | Hardcoded path, environment variable | Hardcoded `/home/siihe/.dotnet/dotnet` | Matches pre-commit hook pattern (constraint C2); consistent |
| Test file organization | Single `test_ac_verifier_cross_repo.py`, multiple test files | Single file | File-per-concern pattern; both AC#11 and AC#12 are cross-repo |
| MSYS path conversion | Set `MSYS_NO_PATHCONV=1`, do not set | Set as subprocess env var | Required per CLAUDE.md; prevents Git Bash path mangling |
| `is_absolute()` guard placement | Both comma-branch and single-pattern branch, single-pattern only | Both branches | Comma-separated patterns recurse; both entry points need the guard |

### Interfaces / Data Structures

**New methods added to `ACVerifier`**:

```python
def _safe_relative_path(self, path: Path) -> str:
    """Convert path to display string, using relative path if within repo_root."""
    try:
        return str(path.relative_to(self.repo_root))
    except ValueError:
        return str(path)

def _convert_to_wsl_path(self, windows_path: str) -> str:
    """Convert Windows path (C:/Era/devkit or C:\\Era\\devkit) to WSL mount path (/mnt/c/Era/devkit)."""
    path = windows_path.replace('\\', '/')
    if len(path) >= 2 and path[1] == ':':
        drive = path[0].lower()
        return f'/mnt/{drive}{path[2:]}'
    return path
```

**Modified method signatures**: None. All existing method signatures preserved.

**WSL subprocess pattern** (replacing lines 888-895):

```python
# Module-level or class-level constant (matches pre-commit pattern of named variable at top)
WSL_DOTNET_PATH = "/home/siihe/.dotnet/dotnet"

# In verify_build_ac:
wsl_repo_root = self._convert_to_wsl_path(str(self.repo_root))
wsl_dotnet = WSL_DOTNET_PATH
# Strip leading 'dotnet' from build_command to avoid duplication with wsl_dotnet
# e.g., "dotnet build devkit.sln" → "build devkit.sln"
if build_command.startswith("dotnet "):
    build_args = build_command[len("dotnet "):]
elif build_command.strip() == "dotnet":
    build_args = "build"  # default subcommand
else:
    build_args = build_command
env = {**os.environ, "MSYS_NO_PATHCONV": "1"}  # Harmless when called from Python; needed if invoked from Git Bash
result = subprocess.run(
    ["wsl", "--", "bash", "-c", f"cd {wsl_repo_root} && {wsl_dotnet} {build_args}"],
    capture_output=True,
    text=True,
    encoding='utf-8',
    errors='replace',
    timeout=300,
    env=env
)
```

Note: `import os` is NOT present in the file and MUST be added to the imports section. The WSL pattern requires `os.environ`.

**WSL build cwd scope**: The wrapper uses `_convert_to_wsl_path(self.repo_root)` for the `cd` target only. The `build_args` string is passed as-is to the WSL bash command. For cross-repo build targets (e.g., `C:\Era\core`), AC table authors must specify WSL-compatible paths in the build command (e.g., `dotnet build /mnt/c/Era/core/Core.sln`), as path conversion is only applied to the cwd, not to build_command arguments.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Add `is_absolute()` detection in `_expand_glob_path` (comma-branch and single-pattern branch) | | [ ] |
| 2 | 2 | Add `_safe_relative_path` helper method to `ACVerifier` class | | [ ] |
| 3 | 3, 4 | Replace all 5 `relative_to(self.repo_root)` call sites with `_safe_relative_path` | | [ ] |
| 4 | 5, 6 | Fix output directory from `"Game" / "logs"` to `"_out" / "logs"` (line 1146) | | [ ] |
| 5 | 7, 8, 9, 19, 23 | Add WSL build wrapper with `_convert_to_wsl_path` helper and WSL subprocess invocation in `verify_build_ac` (includes adding `import os` to imports, timeout=300) | | [ ] |
| 6 | 10 | Run existing test suite to verify backward compatibility | | [ ] |
| 7 | 11, 12, 13, 14, 15, 16, 17, 20 | Create cross-repo path tests in `test_ac_verifier_cross_repo.py`: `is_absolute` detection (AC#11), `_safe_relative_path` behavior (AC#12), per-matcher cross-repo tests (`test_cross_repo_matches`, `test_cross_repo_not_matches`, `test_cross_repo_count`, `test_cross_repo_file`) (AC#13-AC#17), `_expand_glob_path` absolute/relative path tests (AC#15, AC#20) | | [ ] |
| 8 | 18, 21, 22 | Create WSL build tests in `test_ac_verifier_cross_repo.py`: `_convert_to_wsl_path` conversion test (AC#18), `verify_build_ac` WSL subprocess behavioral test (AC#21), cross-repo build path conversion test (AC#22) | | [ ] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

<!-- Tasks tagged [I] require engine-dev or cross-repo investigation before implementation -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Pre-conditions

- `src/tools/python/ac-static-verifier.py` exists and all 23 existing test files pass
- Python `pathlib.Path.is_absolute()` and try/except are available (stdlib, no new dependencies)
- `import os` is NOT currently present in ac-static-verifier.py — must be added for `os.environ` usage in WSL wrapper
- WSL2 Ubuntu 24.04 is available at `/home/siihe/.dotnet/dotnet`
- The 5 `relative_to(self.repo_root)` call sites are at lines ~196, 546, 576, 661, 1010 (verify with Grep before editing)
- The legacy output path `"Game" / "logs"` exists at line ~1146 (verify with Grep before editing)

### Post-conditions

- `_expand_glob_path` handles absolute paths without repo_root prepend (AC#1)
- `_safe_relative_path` method exists and all 5 raw `relative_to(self.repo_root)` calls are replaced (AC#2, AC#3, AC#4)
- Output directory uses `_out/logs/prod/ac/` instead of `Game/logs/prod/ac/` (AC#5, AC#6)
- `verify_build_ac` invokes WSL subprocess with `/home/siihe/.dotnet/dotnet` (AC#7, AC#8, AC#9)
- All 23 existing test files continue to pass (AC#10)
- `test_ac_verifier_cross_repo.py` exists with `is_absolute`, `_safe_relative_path`, per-matcher cross-repo behavioral tests, `_expand_glob_path` absolute/relative path tests, `_convert_to_wsl_path` test, and WSL build behavioral tests (AC#11-AC#22)

### Rollback Plan

- All changes are confined to `src/tools/python/ac-static-verifier.py` and the new `src/tools/python/tests/test_ac_verifier_cross_repo.py`
- Git revert of both files restores the previous state
- Existing 23 test files are unchanged; reverting leaves all prior tests intact

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| No mandatory handoffs | All fixes are self-contained within ac-static-verifier.py | - | - | - | - | - |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Related Features table | Removed F699, F702, F679 (feature files do not exist)
- [fix] Phase1-RefCheck iter1: Links section | Added missing links for F798, F792, F814
- [fix] Phase1-RefCheck iter1: Baseline Measurement | Clarified baseline file is generated at /run Phase 2
- [fix] Phase2-Review iter1: Key Decisions table | Restructured columns to match template (Decision, Options Considered, Selected, Rationale)
- [fix] Phase2-Review iter1: Implementation Contract | Template is flexible on internal format; Pre-conditions/Post-conditions/Rollback Plan is established infra pattern (no change needed)
- [fix] Phase2-Review iter1: Success Criteria subsection | Removed (not in template; redundant with AC table)
- [fix] Phase2-Uncertain iter1: AC#7 | Strengthened from fragile `wsl` to specific `"wsl", "--", "bash"` subprocess pattern
- [fix] Phase2-Review iter1: AC#6 | Strengthened to verify full path `"_out" / "logs" / "prod" / "ac"`
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#13 for per-matcher cross-repo behavioral tests (constraint C4 coverage)
- [fix] Phase2-Review iter2: Technical Design AC Coverage | Added AC#13 and AC#14 rows; updated Approach to AC#1 through AC#14
- [fix] Phase2-Review iter2: Post-conditions | Added AC#13 and AC#14 to test_ac_verifier_cross_repo.py post-condition
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#14 for _expand_glob_path absolute path bypass behavioral test
- [fix] Phase2-Review iter3: AC#4 | Changed from not_contains to count_equals=1 (safe helper body correctly retains one occurrence)
- [fix] Phase2-Review iter4: AC#13 | Changed from contains to count_gte=4 (ensures all 4 matcher types have cross_repo tests)
- [fix] Phase2-Review iter5: AC#13-15 | Split AC#13 into specific per-type ACs (not_matches, file) + renumbered expand_glob_path to AC#15
- [fix] Phase2-Review iter5: AC#5 Details | Added note about Technical Design constraining pathlib syntax
- [fix] Phase2-Review iter6: AC#16, AC#17 | Added matches and count matcher type test ACs (C4 complete coverage)
- [fix] Phase3-Maintainability iter7: Task 5 + Pre-conditions | Corrected import os status (NOT present, must be added)
- [fix] Phase3-Maintainability iter7: AC#18 | Added _convert_to_wsl_path behavioral test AC
- [fix] Phase3-Maintainability iter7: MSYS_NO_PATHCONV | Added clarifying comment (harmless from Python, needed from Git Bash)
- [fix] Phase2-Review iter8: WSL subprocess pattern | Added dotnet prefix stripping to prevent duplicate 'dotnet dotnet' in build command
- [fix] Phase2-Review iter9: AC#3 | Changed from redundant contains to count_gte=5 for call-site verification
- [fix] Phase2-Review iter9: AC#19 | Added def _convert_to_wsl_path existence AC
- [fix] Phase2-Review iter9: Approach item 5 | Updated test description to reference AC#10-AC#18 (8 test functions)
- [fix] Phase2-Review iter10: AC#3, AC#4 | Fixed count matcher format from unsupported pipe-separated to Format B `Pattern (N)`
- [fix] Phase2-Review iter1(FL): AC Coverage table / Key Decisions | Added missing blank line separator
- [fix] Phase2-Review iter1(FL): WSL subprocess pattern | Added WSL build cwd scope documentation for cross-repo build targets
- [fix] Phase2-Review iter2(FL): AC#10 | Changed Type from test to build (ac-static-verifier only dispatches code/build/file)
- [fix] Phase2-Review iter2(FL): WSL build cwd scope note | Clarified verify_build_ac handles path conversion internally (not caller responsibility)
- [fix] Phase2-Review iter2(FL): AC#20 | Added relative path regression test AC (test_expand_glob_path_relative)
- [fix] Phase2-Review iter3(FL): AC#21 | Added WSL build subprocess behavioral test AC (test_verify_build_ac_uses_wsl)
- [fix] Phase2-Review iter3(FL): AC#22 | Added cross-repo build path conversion test AC (test_verify_build_ac_cross_repo_path)
- [fix] Phase2-Review iter4(FL): AC#10 | Reverted Type from build back to test (verify_build_ac WSL wrapper wraps dotnet, incompatible with pytest; test type verified by ac-tester not ac-static-verifier)
- [fix] Phase2-Review iter4(FL): WSL build cwd scope note | Resolved contradiction: _convert_to_wsl_path applies to cd target only, build_args passed as-is
- [fix] Phase2-Review iter4(FL): AC#22 detail | Corrected: tests repo_root cd conversion, not build_command path conversion
- [fix] Phase2-Review iter5(FL): Approach item 5 | Updated from AC#11-AC#18/8 functions to AC#11-AC#22/11 functions with missing test descriptions
- [fix] Phase2-Review iter5(FL): Philosophy | Scoped from absolute "same coverage" to "same coverage for code-type and file-type ACs" + added C8 constraint for build target path limitation
- [resolved-applied] Phase2-Review iter6(FL): AC#10 Type | User decision: Type=test maintained. ac-static-verifier skips test-type ACs; ac-tester verifies backward compatibility during /run Phase 8.
- [fix] Phase3-Maintainability iter6(FL): C8 | Removed speculative "Future feature may add" language (Leak Prevention: no tracking destination → replaced with factual statement)
- [fix] Phase3-Maintainability iter6(FL): WSL subprocess pattern | Extracted WSL_DOTNET_PATH as module-level constant (matches pre-commit named variable pattern)
- [fix] Phase3-Maintainability iter6(FL): Task 7 | Split into Task 7 (cross-repo path tests, AC#11-AC#20) and Task 8 (WSL build tests, AC#18/AC#21/AC#22)
- [fix] Phase2-Review iter7(FL): Philosophy Derivation table | Added C8 build-type limitation acknowledgment row (traceability for explicitly excluded scope)
- [fix] Phase3-Maintainability iter8(FL): WSL subprocess pattern + AC#23 | Added timeout=300 to subprocess.run and AC#23 for timeout verification (Leak Prevention: Risks table committed to timeout mitigation)
- [fix] Phase2-Review iter9(FL): Philosophy Derivation row 5 | Corrected misquote: added "for the most common AC types" qualifier to match scoped Philosophy text
- [fix] Phase2-Review iter9(FL): Philosophy Derivation row 4 | Added AC#23 to WSL build pipeline derivation (was in Goal Coverage but not Philosophy Derivation)
- [fix] Phase2-Review iter1(FL2): Constraint Details | Added C8 detail block (template completeness: all constraints in table must have detail blocks)
<!-- fc-phase-6-completed -->

---

## Links

- [feature-813.md](feature-813.md) - Parent feature (cross-repo limitation discovered during F813 Phase 9)
- [feature-817.md](feature-817.md) - Most recent verifier fix (pipe-escaping)
- [feature-268.md](feature-268.md) - Original ac-static-verifier creation
- [feature-798.md](feature-798.md) - Verifier regex/GTE parser fixes
- [feature-792.md](feature-792.md) - Verifier count_equals/pipe escape
- [feature-814.md](feature-814.md) - Phase 22 Planning
