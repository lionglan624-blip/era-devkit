# Feature 841: Build CWD Cross-Repo Resolution in ac-static-verifier

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T07:23:31Z -->

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

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)
The ac-static-verifier is the SSOT for automated AC verification across all 5 repositories. Cross-repo operations (file resolution, build execution) must resolve paths and working directories consistently using the shared `_CROSS_REPO_PREFIX_MAP`, ensuring any AC targeting engine, core, game, or dashboard repos executes in the correct context.

### Problem (Current Issue)
`verify_build_ac` in `ac-static-verifier.py` unconditionally uses `self.repo_root` (devkit root) as the working directory for all build commands (line 999 for WSL dotnet path via `cd {wsl_repo_root}`, line 1026 for non-dotnet path via `cwd=str(self.repo_root)`). Unlike `_expand_glob_path` (line 165), which already iterates `_CROSS_REPO_PREFIX_MAP` to resolve file paths to their correct repo root, `verify_build_ac` has no cross-repo prefix detection logic. When a build-type AC targets a cross-repo project (e.g., `dotnet build engine/uEmuera.Headless.csproj`), the build runs from devkit root instead of the engine repo root, and the project path argument retains the cross-repo prefix instead of being stripped. This is currently a latent bug because no existing features use cross-repo build-type ACs, but F839 already worked around the issue by embedding explicit `cd /mnt/c/Era/core &&` in its build command.

### Goal (What to Achieve)
Add cross-repo CWD resolution to `verify_build_ac` so that build commands whose arguments contain a cross-repo prefix (engine/, core/, game/, dashboard/) automatically: (1) resolve the CWD to the correct repo root via `_CROSS_REPO_PREFIX_MAP` with env var override support, and (2) strip the cross-repo prefix from the build argument. Both the WSL dotnet path and the non-dotnet path must be updated. Commands containing explicit `cd ` directives must be left unchanged to avoid double CWD application.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why would cross-repo build ACs fail? | The build command executes from devkit root instead of the target repo root | ac-static-verifier.py:999, :1026 |
| 2 | Why does it execute from devkit root? | `verify_build_ac` hardcodes `self.repo_root` as CWD for both execution paths | ac-static-verifier.py:999 (`cd {wsl_repo_root}`), :1026 (`cwd=str(self.repo_root)`) |
| 3 | Why is there no cross-repo CWD resolution? | `verify_build_ac` was written before F838 added `_CROSS_REPO_PREFIX_MAP` support | F818 added WSL support; F838 added prefix map for glob/code paths only |
| 4 | Why did F838 not also fix build CWD? | F838 scoped to file/code path resolution (`_expand_glob_path`); build CWD is a separate concern | F838 scope: `_expand_glob_path` at line 165 |
| 5 | Why (Root)? | `verify_build_ac` lacks the prefix-scanning logic that `_expand_glob_path` already implements for file paths | ac-static-verifier.py:165-169 vs :976-1068 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Cross-repo build commands would run from wrong directory | `verify_build_ac` has no `_CROSS_REPO_PREFIX_MAP` iteration for CWD resolution |
| Where | `verify_build_ac` lines 999, 1009, 1026 | Missing prefix detection logic between command parsing (line 988-993) and execution (line 997+) |
| Fix | Embed explicit `cd /path &&` in each build command (F839 workaround) | Scan build_args for cross-repo prefixes, resolve CWD, strip prefix from args |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F838 | [DONE] | Predecessor: provides `_CROSS_REPO_PREFIX_MAP` and `_expand_glob_path` pattern |
| F818 | [DONE] | Original cross-repo + WSL support for ac-static-verifier |
| F839 | [DONE] | Used explicit CWD workaround in build command (evidence of latent bug) |
| F840 | [DRAFT] | Sibling: engine test isolation (may benefit from cross-repo build CWD) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Code complexity | FEASIBLE | ~30 lines new logic + pattern exists in `_expand_glob_path` (line 165-169) |
| Test complexity | FEASIBLE | ~60-100 lines mock-based tests; no real cross-repo builds needed |
| Dependency availability | FEASIBLE | F838 [DONE], `_CROSS_REPO_PREFIX_MAP` already exists at line 45-50 |
| Risk level | FEASIBLE | Latent bug fix; no existing cross-repo build ACs affected by change |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| ac-static-verifier.py | MEDIUM | Two execution paths modified (WSL dotnet + non-dotnet) |
| Existing build ACs | LOW | All current build ACs are devkit-local; no prefix match means no behavior change |
| Future cross-repo features | HIGH | Enables build-type ACs targeting engine/core/game/dashboard without manual CWD workarounds |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Dual execution paths | ac-static-verifier.py:997-1027 | Both WSL dotnet and non-dotnet paths must be updated |
| Prefix in path-like argument tokens, not flags | Investigation consensus | Scanner must check tokens for prefix match, not command start |
| `_convert_to_wsl_path` for WSL CWD | ac-static-verifier.py:114-120 | Resolved cross-repo root must be converted for WSL path |
| Env var overrides | `_CROSS_REPO_PREFIX_MAP` design | Must use `os.environ.get(env_var, default)` pattern |
| Explicit `cd` skip | F839 workaround pattern | Commands containing `cd ` must skip auto-resolution to avoid double CWD |
| Backward compatibility | All existing build ACs | Commands without cross-repo prefixes must behave identically |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| No existing cross-repo build ACs to validate | LOW | LOW | Mock-based tests verify CWD resolution logic without real builds |
| Double CWD application for explicit `cd` commands | MEDIUM | MEDIUM | Skip auto-resolution when `build_command` contains `cd ` substring |
| Build command with multiple cross-repo paths | LOW | LOW | Use first matching prefix (consistent with `_expand_glob_path` behavior) |
| Prefix collision with devkit-local paths | LOW | LOW | No devkit directories named engine/, core/, game/, dashboard/ exist |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Existing build tests | `grep -c "def test.*build" src/tools/python/tests/test_ac_verifier_method_build.py` | Existing tests pass | Regression guard |
| Cross-repo build CWD tests | N/A | 0 | No cross-repo build tests exist yet |

**Baseline File**: `_out/tmp/baseline-841.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Cannot use build-type AC to test build CWD feature | Self-referential constraint | Must use code-type ACs (Grep/matches) to verify implementation |
| C2 | Mock-based testing required | No real cross-repo builds in CI | Tests must mock subprocess calls, not execute actual builds |
| C3 | Both WSL dotnet and non-dotnet paths need verification | ac-static-verifier.py:997-1027 | Separate ACs for each execution path's CWD resolution |
| C4 | Explicit `cd` skip logic needed | F839 workaround pattern | AC must verify that commands with explicit `cd` bypass auto-resolution |
| C5 | Prefix stripping in build args | Investigation consensus | AC must verify project path is rewritten after CWD change |
| C6 | Env var override support | `_CROSS_REPO_PREFIX_MAP` design | AC should verify env var takes precedence over default path |

### Constraint Details

**C1: Self-Referential AC Prohibition**
- **Source**: All 3 investigators identified this constraint
- **Verification**: build-type AC would test the very code being modified
- **AC Impact**: Use code-type (Grep) ACs for structural verification; use devkit-local build AC for regression only

**C2: Mock-Based Testing**
- **Source**: No cross-repo projects available in CI/test environment
- **Verification**: Tests must pass without engine/core/game/dashboard repos present
- **AC Impact**: Test ACs must verify mock call arguments (CWD, command args), not build output

**C3: Dual Execution Path Coverage**
- **Source**: ac-static-verifier.py lines 997-1016 (WSL dotnet) and 1017-1027 (non-dotnet)
- **Verification**: Read both code paths in `verify_build_ac`
- **AC Impact**: At minimum one AC per execution path verifying CWD resolution

**C4: Explicit cd Skip**
- **Source**: F839 used `wsl -- bash -c 'cd /mnt/c/Era/core && ...'` as workaround
- **Verification**: Check F839 build command format
- **AC Impact**: AC must test that explicit `cd` commands are not modified

**C5: Prefix Stripping**
- **Source**: Build arg `engine/uEmuera.Headless.csproj` must become `uEmuera.Headless.csproj` after CWD change
- **Verification**: Token-level inspection of build args before/after
- **AC Impact**: AC must verify both CWD change AND argument rewriting

**C6: Env Var Override**
- **Source**: `_CROSS_REPO_PREFIX_MAP` uses `os.environ.get(env_var, default)` pattern
- **Verification**: Set env var, verify resolved path differs from default
- **AC Impact**: Test with mocked env var to confirm override behavior

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F838 | [DONE] | Provides `_CROSS_REPO_PREFIX_MAP` used for CWD resolution |
| Related | F818 | [DONE] | Original cross-repo + WSL support for ac-static-verifier |
| Related | F839 | [DONE] | Used explicit CWD workaround in build command (evidence of latent bug) |
| Related | F840 | [DRAFT] | Sibling: engine test isolation (may benefit from cross-repo build CWD) |

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

<!-- fc-phase-2-completed -->

---

<!-- fc-phase-3-completed -->

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "SSOT for automated AC verification across all 5 repositories" | Build CWD resolution must use the shared `_CROSS_REPO_PREFIX_MAP` via `_resolve_cross_repo_root` (same as file resolution) | AC#1, AC#15, AC#16 |
| "must resolve paths and working directories consistently" | Both WSL dotnet and non-dotnet execution paths must resolve CWD identically | AC#2, AC#3 |
| "any AC targeting engine, core, game, or dashboard repos executes in the correct context" | Cross-repo build commands must resolve to the correct repo root and strip prefix from args | AC#4, AC#6, AC#7 |
| "consistently using the shared `_CROSS_REPO_PREFIX_MAP`" | Env var overrides must be respected for CWD resolution | AC#9 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | verify_build_ac delegates to _resolve_build_cwd | code | Grep(src/tools/python/ac-static-verifier.py, pattern="verify_build_ac[\\s\\S]*_resolve_build_cwd", multiline=true) | matches | `_resolve_build_cwd` | [x] |
| 2 | WSL dotnet path uses resolved cross-repo CWD | code | Grep(src/tools/python/ac-static-verifier.py, pattern="_convert_to_wsl_path.*cross_repo") | matches | `_convert_to_wsl_path` | [x] |
| 3 | Non-dotnet path uses resolved cross-repo CWD | code | Grep(src/tools/python/ac-static-verifier.py, pattern="cwd=.*cross_repo") | matches | `cwd=` | [x] |
| 4 | Prefix stripped from args via shared helper | code | Grep(src/tools/python/ac-static-verifier.py, pattern="\\[len\\(prefix\\):\\]") | matches | `len(prefix)` | [x] |
| 5 | Explicit cd commands skip auto-resolution | code | Grep(src/tools/python/ac-static-verifier.py, pattern="\"cd \" in build_command") | matches | `cd ` | [x] |
| 6 | WSL dotnet cross-repo CWD test exists | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="def test.*wsl.*cross_repo|def test.*cross_repo.*wsl") | matches | `cross_repo` | [x] |
| 7 | Non-dotnet cross-repo CWD test exists | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="def test.*non_dotnet.*cross|def test.*cross.*non_dotnet") | matches | `cross` | [x] |
| 8 | Explicit cd skip test exists | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="def test.*cd.*skip|def test.*skip.*cd|def test.*explicit.*cd") | matches | `cd` | [x] |
| 9 | Env var override test exists | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="def test.*env.*override|def test.*override.*env") | matches | `env` | [x] |
| 10 | Backward compatibility test (no prefix unchanged) | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="def test.*no_prefix.*backward|def test_no_prefix_backward") | matches | `no_prefix` | [x] |
| 11 | All build tests pass | exit_code | pytest src/tools/python/tests/test_ac_verifier_method_build.py -v | matches | `0` | [x] |
| 12 | Tests use subprocess mock (no real builds) | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="mock.*subprocess|patch.*subprocess|monkeypatch.*subprocess") | matches | `subprocess` | [x] |
| 13 | Cross-repo test count sufficient | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="def test_") | gte | 9 | [x] |
| 14 | All 4 cross-repo prefixes tested | code | Grep(src/tools/python/tests/test_ac_verifier_method_build.py, pattern="engine/\|core/\|game/\|dashboard/") | gte | 4 | [x] |
| 15 | Shared `_resolve_cross_repo_root` helper exists | code | Grep(src/tools/python/ac-static-verifier.py, pattern="def _resolve_cross_repo_root") | matches | `_resolve_cross_repo_root` | [x] |
| 16 | `_expand_glob_path` delegates to shared helper | code | Grep(src/tools/python/ac-static-verifier.py, pattern="_expand_glob_path[\\s\\S]*_resolve_cross_repo_root", multiline=true) | matches | `_resolve_cross_repo_root` | [x] |

### AC Details

**AC#1: verify_build_ac delegates to _resolve_build_cwd**
- **Test**: Multiline Grep for `_resolve_build_cwd` call within `verify_build_ac` method body
- **Expected**: `verify_build_ac` calls `_resolve_build_cwd` for cross-repo CWD resolution
- **Rationale**: C1 constraint requires code-type verification. Combined with AC#15 (`_resolve_cross_repo_root` exists with `_CROSS_REPO_PREFIX_MAP`) and the delegation chain (`_resolve_build_cwd` → `_resolve_cross_repo_root`), this transitively proves the build path uses the SSOT prefix map.

**AC#13: Cross-repo test count sufficient**
- **Test**: Count `def test_` occurrences in test file
- **Expected**: `gte 9` (3 existing tests + 6 new: WSL cross-repo, non-dotnet cross-repo, cd skip, env override, backward compat, all-prefixes parameterized)
- **Rationale**: Derivation: 3 existing (test_method_command_positive, test_method_command_negative, test_backward_compat_expected_column) + minimum 6 new tests covering C2-C6 constraints + all-prefix coverage = 9 total.

**AC#14: All 4 cross-repo prefixes tested**
- **Test**: Grep for all 4 prefix strings (engine/, core/, game/, dashboard/) in test file
- **Expected**: `gte 4` (at least one test reference per prefix)
- **Rationale**: Philosophy claims "SSOT across all 5 repositories". A regression removing one prefix entry must be caught. Parameterized test covers all 4 non-devkit prefixes.

**AC#15: Shared `_resolve_cross_repo_root` helper exists**
- **Test**: Grep for method definition
- **Expected**: Method exists on `ACVerifier` class
- **Rationale**: DRY extraction of `_CROSS_REPO_PREFIX_MAP` iteration shared by `_expand_glob_path` and `_resolve_build_cwd`.

**AC#16: `_expand_glob_path` delegates to shared helper**
- **Test**: Multiline Grep for `_resolve_cross_repo_root` usage within `_expand_glob_path` method body
- **Expected**: `_expand_glob_path` calls `_resolve_cross_repo_root` instead of directly iterating `_CROSS_REPO_PREFIX_MAP`
- **Rationale**: Ensures both code paths use the shared helper, eliminating duplicated prefix matching logic.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Resolve CWD to correct repo root via _CROSS_REPO_PREFIX_MAP with env var override | AC#1, AC#2, AC#3, AC#9 |
| 2 | Strip cross-repo prefix from build argument | AC#4 |
| 3 | Both WSL dotnet and non-dotnet paths updated | AC#2, AC#3, AC#6, AC#7 |
| 4 | Commands with explicit cd left unchanged | AC#5, AC#8 |
| 5 | Backward compatibility (no-prefix commands unchanged) | AC#10 |
| 6 | All build tests pass (regression + new) | AC#11, AC#13 |
| 7 | Tests use subprocess mock (no real builds) | AC#12 |
| 8 | All 4 cross-repo prefixes have test coverage | AC#14 |
| 9 | Shared cross-repo prefix resolution (DRY) | AC#15, AC#16 |

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->

---

## Technical Design

### Approach

Introduce a private helper method `_resolve_build_cwd` on `ACVerifier` that encapsulates all cross-repo prefix detection logic. This method accepts the full `build_command` string and returns a tuple `(resolved_cwd: Optional[str], stripped_args: str)`:

- `resolved_cwd` is `None` when no cross-repo prefix is found (fallback to `self.repo_root`)
- `stripped_args` is the build command with the matching prefix token rewritten to its bare path (prefix stripped)

`verify_build_ac` calls `_resolve_build_cwd` once before branching into the WSL dotnet or non-dotnet code path, then uses the resolved CWD in each branch. This keeps the branch-specific subprocess logic unchanged except for the CWD source.

**Explicit `cd` skip**: Before scanning for prefixes, `_resolve_build_cwd` checks whether `build_command` contains the substring `"cd "`. If it does, it returns `(None, build_command)` unchanged, leaving the embedded `cd` directive to handle CWD as before (F839 pattern preserved).

**Prefix scan**: Iterates `_CROSS_REPO_PREFIX_MAP` items. For each token in `build_args` (the portion after `"dotnet "` for WSL path, or the full command for non-dotnet), checks `token.startswith(prefix)`. On first match: resolves the repo root via `os.environ.get(env_var, default)`, and rewrites that token to `token[len(prefix):]`. Consistent with `_expand_glob_path` "first match wins" behavior.

**WSL dotnet path**: Uses `_convert_to_wsl_path(str(resolved_cwd))` to convert the resolved Windows repo root to a WSL mount path for the `cd` command inside the `bash -c` string.

**Non-dotnet path**: Passes `cwd=str(resolved_cwd)` directly to `subprocess.run`.

**Backward compatibility**: Commands with no cross-repo prefix token produce `resolved_cwd = None`; both execution paths then fall back to `self.repo_root` unchanged.

This approach satisfies all ACs:
- AC#1: `_resolve_build_cwd` references `_CROSS_REPO_PREFIX_MAP` inside `verify_build_ac`'s call chain
- AC#2: WSL dotnet path uses `_convert_to_wsl_path(str(cross_repo_root))` on the resolved CWD
- AC#3: Non-dotnet path uses `cwd=str(cross_repo_root)` from resolved CWD
- AC#4: Prefix token is stripped from build args after resolution
- AC#5: Explicit `cd ` check exits early without modification
- AC#6–AC#10: New tests in `test_ac_verifier_method_build.py` covering each scenario

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | `verify_build_ac` calls `_resolve_build_cwd`; multiline Grep matches `_resolve_build_cwd` within the method body. SSOT chain: `verify_build_ac` → `_resolve_build_cwd` → `_resolve_cross_repo_root` → `_CROSS_REPO_PREFIX_MAP` (AC#15 verifies last link) |
| 2 | WSL dotnet branch: `wsl_cross_repo_root = self._convert_to_wsl_path(str(cross_repo_root))` used in the `cd` command; Grep matches `_convert_to_wsl_path.*cross_repo` |
| 3 | Non-dotnet branch: `cwd=str(cross_repo_root)` passed to `subprocess.run`; Grep matches `cwd=.*cross_repo` |
| 4 | Shared helper `_resolve_cross_repo_root` contains `[len(prefix):]` for prefix stripping; Grep matches the expression |
| 5 | Guard clause: `if "cd " in build_command: return (None, build_command)`; Grep matches `"cd " in build_command` directly |
| 6 | New test `test_wsl_cross_repo_cwd` in `test_ac_verifier_method_build.py` mocks subprocess and asserts WSL bash-c string contains engine/core repo root path |
| 7 | New test `test_non_dotnet_cross_repo_cwd` mocks subprocess and asserts `cwd` kwarg equals resolved cross-repo root |
| 8 | New test `test_explicit_cd_skip` passes command with `"cd "` and asserts no CWD substitution occurs (mock captures original `cwd=self.repo_root`) |
| 9 | New test `test_env_var_override_cwd` sets `ENGINE_PATH=/tmp/custom_engine`, asserts resolved CWD uses custom path |
| 10 | New test `test_no_prefix_backward_compat` passes a devkit-local `dotnet build devkit.sln`, asserts CWD remains `self.repo_root` |
| 11 | All 9+ tests in `test_ac_verifier_method_build.py` must pass via pytest |
| 12 | New tests use `unittest.mock.patch("subprocess.run")` — Grep matches `mock.*subprocess|patch.*subprocess` |
| 13 | `def test_` count in `test_ac_verifier_method_build.py` reaches ≥ 9 (3 existing + 6 new) |
| 14 | New parameterized test `test_all_cross_repo_prefixes` verifies `_resolve_build_cwd` resolves CWD correctly for each of the 4 cross-repo prefixes (engine/, core/, game/, dashboard/) |
| 15 | `def _resolve_cross_repo_root(self, text: str)` method defined on `ACVerifier`; Grep matches method definition |
| 16 | `_expand_glob_path` body contains call to `_resolve_cross_repo_root`; multiline Grep matches from method def to call |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Helper method vs inline logic | A: Inline prefix scan in each branch; B: Extract `_resolve_build_cwd` helper | B: Helper method | Two execution branches need identical prefix resolution. DRY prevents divergence. Helper is independently testable. |
| Token scanning scope | A: Scan only first non-flag token; B: Scan all space-split tokens | B: All tokens | Build commands like `dotnet build engine/uEmuera.Headless.csproj --no-restore` have the cross-repo path as a non-first token. Scanning all tokens matches actual usage. |
| Multiple prefix matches | A: Error on ambiguity; B: First match wins | B: First match wins | Consistent with `_expand_glob_path` behavior (line 169 `break`). No realistic build command targets two cross-repo prefixes simultaneously. |
| `cd` skip detection | A: Check for absolute WSL paths (`/mnt/`); B: Check substring `"cd "` | B: `"cd "` substring | Direct, language-agnostic. Matches the F839 workaround format exactly (`cd /mnt/c/Era/core &&`). Absolute path check would miss `cd` on Windows paths. |
| WSL path conversion | A: Duplicate path conversion logic; B: Reuse `_convert_to_wsl_path` | B: Reuse | `_convert_to_wsl_path` (line 114) already handles both `C:/` and `C:\` formats. No duplication needed. |
| Return type for helper | A: Tuple `(Optional[Path], str)` for CWD + stripped args; B: Dataclass | A: Tuple | Simple, matches Python idiom for small multi-value returns. Only two callers (WSL branch, non-dotnet branch) in same method. |

### Interfaces / Data Structures

No new interfaces. Two new private methods on `ACVerifier`:

**Shared helper** (used by both `_resolve_build_cwd` and refactored `_expand_glob_path`):

```python
def _resolve_cross_repo_root(self, text: str) -> tuple[Optional[Path], str]:
    """Find first matching cross-repo prefix in text using _CROSS_REPO_PREFIX_MAP.

    Returns (repo_root, stripped_text) where:
    - repo_root: resolved repo root Path if a prefix matched, else None
    - stripped_text: text with the matching prefix removed, or original text if no match
    """
    for prefix, (env_var, default) in _CROSS_REPO_PREFIX_MAP.items():
        if text.startswith(prefix):
            return Path(os.environ.get(env_var, default)), text[len(prefix):]
    return None, text
```

**Build CWD resolver** (delegates prefix matching to shared helper):

```python
def _resolve_build_cwd(self, build_command: str) -> tuple[Optional[Path], str]:
    """Resolve cross-repo CWD for a build command.

    Returns (cross_repo_root, build_args) where:
    - cross_repo_root: resolved repo root Path if a cross-repo prefix was found, else None
    - build_args: build_command with the matching prefix token stripped, or
      the original command if no prefix matched or an explicit 'cd ' was found

    Commands containing 'cd ' are returned unchanged (None, build_command).
    """
    # Explicit cd directive: leave unchanged to avoid double CWD application
    if "cd " in build_command:
        return None, build_command

    # Strip 'dotnet ' prefix to get the argument tokens
    if build_command.startswith("dotnet "):
        args_portion = build_command[len("dotnet "):]
    else:
        args_portion = build_command

    # Scan tokens for cross-repo prefix match via shared helper
    tokens = args_portion.split()
    for token in tokens:
        cross_repo_root, stripped_token = self._resolve_cross_repo_root(token)
        if cross_repo_root is not None:
            build_args = build_command.replace(token, stripped_token, 1)
            return cross_repo_root, build_args

    return None, build_command
```

Usage pattern in `verify_build_ac` (replacing lines 998-1026):

```python
# Resolve cross-repo CWD and strip prefix from build args
cross_repo_root, resolved_command = self._resolve_build_cwd(build_command)

if resolved_command.startswith("dotnet ") or resolved_command.strip() == "dotnet":
    build_args = resolved_command[len("dotnet "):] if resolved_command.startswith("dotnet ") else "build"
    cross_repo_root = cross_repo_root if cross_repo_root is not None else self.repo_root
    wsl_cross_repo_root = self._convert_to_wsl_path(str(cross_repo_root))
    result = subprocess.run(
        ["wsl", "--", "bash", "-c", f"cd {wsl_cross_repo_root} && {wsl_dotnet} {build_args}"],
        ...
    )
else:
    cross_repo_root = cross_repo_root if cross_repo_root is not None else self.repo_root
    result = subprocess.run(
        resolved_command.split(),
        cwd=str(cross_repo_root),
        ...
    )
```

### Upstream Issues

<!-- No upstream issues found during design. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#2 Grep pattern `_convert_to_wsl_path.*cross_repo` requires variable named `cross_repo_root` | AC Definition Table, AC#2 | Implementation must name the variable `cross_repo_root` (or similar containing `cross_repo`) to match the AC pattern. The Technical Design code stub uses `wsl_cross_repo_root` for the WSL-converted string, which satisfies `_convert_to_wsl_path.*cross_repo`. No change needed — pattern will match. |
| AC#3 Grep pattern `cwd=.*cross_repo` requires variable named containing `cross_repo` | AC Definition Table, AC#3 | Code stub updated: non-dotnet branch now uses `cross_repo_root` directly so `cwd=str(cross_repo_root)` satisfies the pattern. Resolved. |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 5 | Add `_resolve_build_cwd` private helper method to `ACVerifier`: use `_resolve_cross_repo_root` for prefix detection, strip prefix from build args via shared helper, return resolved repo root; guard explicit `cd ` commands with early return | | [x] |
| 2 | 2 | Update WSL dotnet execution path in `verify_build_ac` to use resolved `cross_repo_root` from `_resolve_build_cwd` call (made once before the branch split), applying `_convert_to_wsl_path(str(cross_repo_root))` for the `cd` command in the bash-c string | | [x] |
| 3 | 3 | Update non-dotnet execution path in `verify_build_ac` to use resolved `cross_repo_root` from `_resolve_build_cwd` call, passing `cwd=str(cross_repo_root)` to `subprocess.run` | | [x] |
| 4 | 6, 12 | Add `test_wsl_cross_repo_cwd` test in `test_ac_verifier_method_build.py`: mock `subprocess.run`, pass build command with `engine/` prefix, assert WSL bash-c string contains engine repo WSL path | | [x] |
| 5 | 7, 12 | Add `test_non_dotnet_cross_repo_cwd` test: mock `subprocess.run`, pass non-dotnet command with cross-repo prefix, assert `cwd` kwarg equals resolved cross-repo root | | [x] |
| 6 | 8, 12 | Add `test_explicit_cd_skip` test: pass command containing `cd `, assert no CWD substitution occurs and `cwd` kwarg remains `self.repo_root` | | [x] |
| 7 | 9, 12 | Add `test_env_var_override_cwd` test: set `ENGINE_PATH` env var to custom path, assert resolved CWD uses the custom path instead of the default | | [x] |
| 8 | 10, 12, 13 | Add `test_no_prefix_backward_compat` test: pass devkit-local `dotnet build devkit.sln` command, assert CWD remains `self.repo_root` unchanged | | [x] |
| 9 | 11, 13 | Run `pytest src/tools/python/tests/test_ac_verifier_method_build.py -v` and verify all tests pass (exit code 0) | | [x] |
| 10 | 14, 12, 13 | Add `test_all_cross_repo_prefixes` parameterized test: verify `_resolve_build_cwd` resolves CWD correctly for each of the 4 cross-repo prefixes (engine/, core/, game/, dashboard/) using `@pytest.mark.parametrize` | | [x] |
| 11 | 4, 15 | Extract `_resolve_cross_repo_root(self, text: str) -> tuple[Optional[Path], str]` shared helper: iterate `_CROSS_REPO_PREFIX_MAP`, find first matching prefix, return `(repo_root, stripped_text)`. Both `_resolve_build_cwd` and `_expand_glob_path` delegate to this | | [x] |
| 12 | 16 | Refactor `_expand_glob_path` to use `_resolve_cross_repo_root` instead of directly iterating `_CROSS_REPO_PREFIX_MAP` | | [x] |

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
| 1 | implementer | sonnet | feature-841.md Tasks 1-3, 11-12, `src/tools/python/ac-static-verifier.py` | Modified `ac-static-verifier.py` with `_resolve_cross_repo_root` shared helper + `_resolve_build_cwd` + updated `verify_build_ac` + refactored `_expand_glob_path` |
| 2 | implementer | sonnet | feature-841.md Tasks 4-8, 10, `src/tools/python/tests/test_ac_verifier_method_build.py` | New tests added to `test_ac_verifier_method_build.py` |
| 3 | tester | sonnet | feature-841.md Task 9, `test_ac_verifier_method_build.py` | pytest pass result (exit code 0) |

### Pre-conditions

- F838 is [DONE]: `_CROSS_REPO_PREFIX_MAP` exists in `ac-static-verifier.py`
- `_convert_to_wsl_path` method exists on `ACVerifier` (line ~114)
- `test_ac_verifier_method_build.py` exists with at least 3 existing tests

### Execution Order

**Phase 1 — Production code (Tasks 1-3, 11-12)**

1. Read `ac-static-verifier.py` to locate:
   - `_CROSS_REPO_PREFIX_MAP` (line ~45-50)
   - `_convert_to_wsl_path` method (line ~114)
   - `_expand_glob_path` method (line ~165) — identify existing prefix iteration logic
   - `verify_build_ac` method (line ~976-1068) — identify the two execution branches
2. Add `_resolve_cross_repo_root(self, text: str) -> tuple[Optional[Path], str]` shared helper per Technical Design stub (Task 11)
3. Refactor `_expand_glob_path` to delegate prefix matching to `_resolve_cross_repo_root` (Task 12)
4. Add `_resolve_build_cwd(self, build_command: str) -> tuple[Optional[Path], str]` method per Technical Design stub, using `_resolve_cross_repo_root` for prefix detection (Task 1)
5. In `verify_build_ac` WSL dotnet branch: call `_resolve_build_cwd`, use result as `cross_repo_root`, pass `_convert_to_wsl_path(str(cross_repo_root))` to the bash-c `cd` command (Task 2)
6. In `verify_build_ac` non-dotnet branch: call `_resolve_build_cwd`, use result as `cross_repo_root`, pass `cwd=str(cross_repo_root)` to `subprocess.run` (variable name must contain `cross_repo` to match AC#3 grep pattern) (Task 3)

**Phase 2 — Test code (Tasks 4-8, 10)**

5. Read `test_ac_verifier_method_build.py` to understand existing test structure and mock patterns
6. Add 6 new tests (Tasks 4-8, 10) using `unittest.mock.patch("subprocess.run")`:
   - `test_wsl_cross_repo_cwd`: `dotnet build engine/uEmuera.Headless.csproj` → assert bash-c contains engine WSL path, prefix stripped from args
   - `test_non_dotnet_cross_repo_cwd`: `some-tool core/SomeTool.csproj` → assert `cwd` kwarg equals resolved core root
   - `test_explicit_cd_skip`: `wsl -- bash -c 'cd /mnt/c/Era/core && ...'` → assert `cwd` is still `self.repo_root`
   - `test_env_var_override_cwd`: monkeypatch `ENGINE_PATH=/tmp/custom_engine`, assert resolved CWD equals `/tmp/custom_engine`
   - `test_no_prefix_backward_compat`: `dotnet build devkit.sln` → assert `cwd` remains `self.repo_root`
   - `test_all_cross_repo_prefixes`: `@pytest.mark.parametrize` over engine/, core/, game/, dashboard/ → assert each prefix resolves to correct repo root

**Phase 3 — Verification (Task 9)**

7. Run `pytest src/tools/python/tests/test_ac_verifier_method_build.py -v`
8. Confirm exit code 0 and test count >= 9

### Build Verification Steps

```bash
# Run build tests only (fast)
cd /mnt/c/Era/devkit && python -m pytest src/tools/python/tests/test_ac_verifier_method_build.py -v

# Count test functions to verify AC#13
grep -c "def test_" src/tools/python/tests/test_ac_verifier_method_build.py
```

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert HEAD`
2. Notify user of rollback
3. Create follow-up feature to investigate the failing scenario
4. All existing build ACs are devkit-local (no cross-repo prefix); revert leaves them unaffected

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

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
| 2026-03-06T07:30 | PHASE_START | orchestrator | Phase 1 Initialize | F841 [REVIEWED]→[WIP] |
<!-- run-phase-1-completed -->
| 2026-03-06T07:32 | PHASE_COMPLETE | orchestrator | Phase 2 Investigation | Explorer: codebase mapped |
<!-- run-phase-2-completed -->
| 2026-03-06T07:45 | START | implementer | Phase 1 Tasks 1-3, 11-12 | Production code implementation |
| 2026-03-06T07:48 | END | implementer | Phase 1 Tasks 1-3, 11-12 | SUCCESS — 250 Python tests pass, all AC patterns verified |
| 2026-03-06T07:50 | START | implementer | Phase 2 Tasks 4-8, 10 | Test code implementation |
| 2026-03-06T07:53 | END | implementer | Phase 2 Tasks 4-8, 10 | SUCCESS — 12/12 tests pass |
| 2026-03-06T07:54 | END | orchestrator | Phase 3 pytest verification | 12 passed, exit 0 |
<!-- run-phase-4-completed -->
| 2026-03-06T07:56 | DEVIATION | Bash | ac-static-verifier --ac-type code | exit 1: 11/15 passed, AC#1,5,14,16 FAIL (AC pattern issues) |
| 2026-03-06T07:58 | END | orchestrator | Phase 7 manual verification | AC#1,5,14,16 verified PASS via Grep (verifier pattern compat issue). ac-tester: OK:16/16 |
<!-- run-phase-7-completed -->
| 2026-03-06T08:00 | END | orchestrator | Phase 8 Post-Review | feature-reviewer OK, 8.2 skipped (no extensibility), 8.3 N/A |
<!-- run-phase-8-completed -->
| 2026-03-06T08:05 | END | orchestrator | Phase 9 Report & Approval | DEVIATION: 1 (ac-static-verifier pattern compat → F842). User approved |
<!-- run-phase-9-completed -->
| 2026-03-06T08:07 | END | orchestrator | Phase 10 Commit | e9b36b6 |
| 2026-03-06T08:08 | CodeRabbit | 0 findings | - |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: AC Definition Table, AC#3 | Code stub non-dotnet branch used `effective_root` instead of `cross_repo_root`, mismatching AC#3 pattern
- [fix] Phase2-Review iter1: Interfaces/Data Structures, helper stub | Stub used `stripped_command`/`stripped_token` instead of `build_args`/inline `token[len(prefix):]`, mismatching AC#4 pattern
- [fix] Phase2-Review iter1: AC Definition Table, AC#5 | Fragile multiline pattern replaced with direct `"cd " in build_command` grep
- [fix] Phase2-Uncertain iter1: Tasks 2-3 | Clarified tasks use result of single `_resolve_build_cwd` call, not independent calls
- [resolved-skipped] Phase3-Maintainability iter2: [CON-001] `cd ` substring check (`"cd " in build_command`) may false-positive on tokens ending in `cd` followed by space (e.g., `abcd efg`). Key Decision explicitly chose this approach over regex. User review needed to decide if precision improvement is warranted.
- [fix] Phase3-Maintainability iter2: AC/Tasks/Implementation Contract | Added AC#14 parameterized test for all 4 cross-repo prefixes + Task#10
- [fix] Phase3-Maintainability iter2: Technical Design, Usage pattern code stub | Renamed shadowed `build_command` to `resolved_command` for clarity
- [fix] Phase2-Review iter3: Implementation Contract, Phase 1 step 3 | Updated `effective_root` to `cross_repo_root` to match Technical Design stub and AC#2 pattern
- [resolved-applied] Phase3-Maintainability iter4: [EXT-001] `_expand_glob_path` and `_resolve_build_cwd` both iterate `_CROSS_REPO_PREFIX_MAP` independently. Future cross-repo AC types would need a third copy. Consider extracting shared `_resolve_cross_repo_root` helper. Potential follow-up feature.
- [fix] PostLoop-UserFix post-loop: AC/Tasks/Design | Added shared `_resolve_cross_repo_root` helper extraction (AC#15-16, Task#11-12, updated Task#1)
- [fix] Phase2-Review iter1-reloop: AC#1 pattern + AC Details + AC Coverage + Philosophy Derivation | Updated to verify delegation chain instead of direct _CROSS_REPO_PREFIX_MAP reference (broken by shared helper extraction)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->

## Links

[Predecessor: F838](feature-838.md) - Cross-repo prefix mapping (prerequisite)
[Related: F818](feature-818.md) - Original cross-repo + WSL support
[Related: F839](feature-839.md) - Explicit CWD workaround evidence
[Related: F840](feature-840.md) - Engine test isolation (sibling)
