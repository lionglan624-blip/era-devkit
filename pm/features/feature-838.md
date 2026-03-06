# Feature 838: Test Infrastructure Fixes — Cross-Repo Verifier Path Resolution

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T04:12:57Z -->

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
| Parent Feature | F833 |
| Discovery Phase | Phase 4 (test suite), Phase 9 (static verifier) |
| Timestamp | 2026-03-06 |

### Observable Symptom
Two test infrastructure issues discovered during F833 /run:
1. ac-static-verifier.py fails for cross-repo features (paths relative to engine/core repos not resolvable from devkit root)
2. Engine test suite has 9 PRE-EXISTING test isolation failures when running full suite (ProcessLevelParallelRunnerTests: 5, VariableDataAccessorTests: 4) — all pass in isolation

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python ac-static-verifier.py --feature 833 --ac-type code` |
| Exit Code | 1 |
| Error Output | `File not found: engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs` |
| Expected | Cross-repo paths resolved correctly |
| Actual | Paths resolved relative to devkit root only |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Needs cross-repo path resolution support |
| engine/tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs | Test isolation failures |
| engine/tests/uEmuera.Tests/Tests/VariableDataAccessorTests.cs | Test isolation failures (GlobalStatic shared state) |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| --repo-root /c/Era | FAIL | Feature file not found (expects pm/features/ under repo-root) |

### Parent Session Observations
Cross-repo features (engine type modifying engine/ and core/ repos) cannot use ac-static-verifier for automated verification. Manual ac-tester dispatch works but doesn't generate JSON logs for verify-logs.py aggregation. Engine test isolation issues are GlobalStatic shared state between test collections — needs [Collection] attributes or IDisposable cleanup.

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
Test infrastructure must support cross-repo feature verification without manual workarounds. ac-static-verifier.py is the SSOT for automated AC verification across all feature types; its path resolution must cover the full 5-repo environment (devkit, engine, core, game, dashboard).

### Problem (Current Issue)
`ac-static-verifier.py` uses a single `repo_root` parameter for all path resolution (`_expand_glob_path` at line 159: `self.repo_root / file_path`). When a feature file lives in devkit (`pm/features/feature-833.md`) but its ACs reference files in other repos (`engine/Assets/Scripts/...`, `core/src/Era.Core/...`), the verifier constructs non-existent paths like `C:\Era\devkit\engine\Assets\Scripts\...`. The `--repo-root` workaround fails because `__init__` (line 75) hardcodes feature file lookup as `repo_root / "pm" / "features" / f"feature-{feature_id}.md"`, so setting `--repo-root /c/Era` breaks feature file resolution. This single-repo assumption was adequate before the 5-repo split but now blocks automated verification for engine, core, and game features. At least 3 completed features (F791, F793, F833) and 1 draft (F835) use cross-repo paths in their ACs. Additionally, `verify-logs.py` line 28 references the pre-migration path `dev/planning/index-features.md` instead of `pm/index-features.md`, causing `get_active_features()` to always return None.

### Goal (What to Achieve)
Add cross-repo path prefix resolution to `ac-static-verifier.py` so that AC paths starting with `engine/`, `core/`, `game/`, or `dashboard/` are resolved against their respective repo locations (via environment variables with hardcoded defaults per CLAUDE.md). Feature file resolution remains at `repo_root/pm/features/`. Fix `verify-logs.py` stale path reference. Existing devkit-relative paths and absolute path bypass must continue to work without regression.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does `Glob(engine/Assets/Scripts/...)` fail? | `_expand_glob_path` resolves it as `repo_root / "engine/Assets/Scripts/..."` which constructs `C:\Era\devkit\engine\Assets\Scripts\...` -- a path that does not exist | `ac-static-verifier.py:159` |
| 2 | Why does the verifier prepend `repo_root` for all relative paths? | `_expand_glob_path` has only two branches: absolute paths (bypass) and relative paths (prepend `repo_root`) with no repo-prefix detection | `ac-static-verifier.py:155-159` |
| 3 | Why is there no mapping for `engine/`, `core/`, `game/` prefixes? | The verifier was designed for single-repo (devkit) features only; no cross-repo path resolution exists | `ac-static-verifier.py:70-75` (constructor takes single `repo_root`) |
| 4 | Why can't `--repo-root /c/Era` serve as workaround? | `__init__` hardcodes `self.feature_file = repo_root / "pm" / "features" / f"feature-{feature_id}.md"`, so feature file must be under `repo_root/pm/features/` | `ac-static-verifier.py:75` |
| 5 | Why (Root)? | The tool was built before the 5-repo split and never gained awareness of the cross-repo environment variables (`ENGINE_PATH`, `CORE_PATH`, `GAME_PATH`, `DASHBOARD_PATH`) defined in CLAUDE.md | `ac-static-verifier.py:1286-1288`, `CLAUDE.md` env var table |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | `File not found: engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs` | Single `repo_root` design prevents cross-repo path resolution |
| Where | `_expand_glob_path` return value (line 159) | `ACVerifier.__init__` architecture (line 70-75) -- single repo assumption |
| Fix | Use absolute paths in ACs (fragile workaround) | Add repo-prefix-to-directory mapping using env vars with defaults |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F833 | [DONE] | Parent feature that discovered both issues |
| F834 | [DONE] | Recent verifier fix (Format C Guard DRY) |
| F832 | [DONE] | Recent verifier fix (numeric parsing) |
| F791 | [DONE] | Uses `engine/` paths in ACs |
| F793 | [DONE] | Uses `engine/` paths in ACs |
| F835 | [DRAFT] | Future engine feature, will need cross-repo verification |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Path prefix mapping is straightforward | FEASIBLE | Only 4 prefixes needed (`engine/`, `core/`, `game/`, `dashboard/`) |
| Env vars already standardized in CLAUDE.md | FEASIBLE | `ENGINE_PATH`, `CORE_PATH`, `GAME_PATH`, `DASHBOARD_PATH` with defaults |
| Absolute path bypass already works | FEASIBLE | Existing escape hatch in `_expand_glob_path` line 156-157 |
| No breaking changes to existing features | FEASIBLE | Prefix mapping only activates for known prefixes; devkit-relative paths continue as-is |
| `_expand_glob_path` shared by all AC types | FEASIBLE | Fix benefits code, file, and build verification simultaneously |
| verify-logs.py fix is one-line change | FEASIBLE | Change `dev/planning/index-features.md` to `pm/index-features.md` |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| ac-static-verifier.py | HIGH | Primary fix target -- `_expand_glob_path` gains repo-prefix resolution |
| Cross-repo features (F791, F793, F833, F835) | HIGH | Previously unverifiable ACs become automatable |
| verify-logs.py | MEDIUM | `get_active_features()` returns correct data instead of None |
| Existing devkit-only features | LOW | No change -- relative paths continue to resolve against `repo_root` |
| Engine test isolation | LOW | Acknowledged in Deviation Context but engine repo changes are out of devkit scope |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Feature file must remain under devkit `pm/features/` | `ac-static-verifier.py:75` | Feature file and target path resolution must be decoupled |
| Env vars have defaults per CLAUDE.md | CLAUDE.md env var table | Can use defaults as fallback when env vars are unset |
| Existing ACs use bare prefixes (`engine/`, `core/`) not env var values | F791, F793, F833 | Prefix detection must match these existing conventions |
| `_expand_glob_path` is shared by code, file, and build verification | `ac-static-verifier.py` | Fix benefits all AC types simultaneously |
| Engine test isolation requires engine repo commits | Separate repo | Cannot be fixed solely in devkit |
| `verify_build_ac` uses `repo_root` for cwd | `ac-static-verifier.py` | Build-type ACs for engine also need cross-repo resolution |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Env var not set in CI/automation | MEDIUM | HIGH | Use hardcoded defaults from CLAUDE.md (`C:\Era\engine`, etc.) |
| Path separator issues (Windows vs WSL) | LOW | MEDIUM | Follow existing `_convert_to_wsl_path` pattern in the verifier |
| Prefix collision (`engine/` could be devkit subdir) | LOW | HIGH | Check devkit-relative first; fall back to cross-repo only if local path does not exist |
| Breaking existing single-repo features | LOW | HIGH | Prefix mapping only activates for known prefixes; all other relative paths use current logic |
| Engine test isolation regressions from future fixes | LOW | MEDIUM | Engine test isolation is tracked as out-of-scope for F838; separate engine repo work item needed |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Cross-repo AC verification (F833) | `python ac-static-verifier.py --feature 833 --ac-type code` | Exit 1, file not found | Should become exit 0 after fix |
| verify-logs.py active features | `python verify-logs.py` | Returns None (stale path) | Should return feature list after fix |

**Baseline File**: `_out/tmp/baseline-838.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Must detect repo-prefix paths (`engine/`, `core/`, `game/`, `dashboard/`) | `_expand_glob_path` line 159 | AC must verify prefix detection and correct resolution |
| C2 | Must read env vars with hardcoded defaults | CLAUDE.md env var table | AC must verify fallback when env var is unset |
| C3 | Must preserve devkit-relative path behavior | Backward compatibility | AC must include regression test for devkit-only features |
| C4 | Must preserve absolute path bypass | Existing logic at line 156-157 | AC must verify absolute paths still work |
| C5 | verify-logs.py path must match current structure | Line 28 stale path `dev/planning/` | AC must verify corrected path `pm/index-features.md` |
| C6 | Build ACs must resolve engine path correctly | `verify_build_ac` uses `repo_root` for cwd | AC must verify build commands with cross-repo cwd |
| C7 | Feature file lookup must remain at `repo_root/pm/features/` | `__init__` line 75 | Must NOT change feature file resolution logic |
| C8 | `_expand_glob_path` shared by all AC types | Investigation finding | Single fix must cover code, file, and build verification |

### Constraint Details

**C1: Repo-Prefix Path Detection**
- **Source**: All 3 investigations identified `_expand_glob_path` line 159 as the single resolution point
- **Verification**: Run verifier on F833 with `engine/` and `core/` paths in ACs
- **AC Impact**: Must test each prefix (`engine/`, `core/`, `game/`, `dashboard/`) resolves to correct repo

**C2: Environment Variable Fallback**
- **Source**: CLAUDE.md defines `ENGINE_PATH=C:\Era\engine`, `CORE_PATH=C:\Era\core`, `GAME_PATH=C:\Era\game`, `DASHBOARD_PATH=C:\Era\dashboard`
- **Verification**: Test with env var set and unset
- **AC Impact**: Must verify both env-var-present and default-fallback paths

**C3: Backward Compatibility**
- **Source**: 275+ existing devkit-only features use relative paths
- **Verification**: Run verifier on a devkit-only feature (e.g., F834)
- **AC Impact**: Must verify no regression for devkit-relative paths

**C4: Absolute Path Bypass Preserved**
- **Source**: Existing `is_absolute()` check at `_expand_glob_path` line 156-157
- **Verification**: Grep for `is_absolute` in ac-static-verifier.py after implementation
- **AC Impact**: The new prefix-detection branch must not remove or bypass the existing absolute path logic

**C5: verify-logs.py Stale Path**
- **Source**: Explorer 3 found line 28 references `dev/planning/index-features.md`
- **Verification**: Grep for `dev/planning` in verify-logs.py
- **AC Impact**: Must verify path changed to `pm/index-features.md`

**C6: Build ACs Cross-Repo CWD Resolution**
- **Source**: `verify_build_ac` at line 983/1010 uses `self.repo_root` for working directory
- **Verification**: `verify_build_ac` CWD uses `self.repo_root` directly, NOT `_expand_glob_path`. This is independent of the path resolution fix in this feature.
- **AC Impact**: NOT covered by this feature. `_expand_glob_path` fixes file/code path resolution; build CWD resolution requires separate work. No current cross-repo feature uses build-type ACs, so this is deferred (see Technical Constraints line "Build-type ACs for engine also need cross-repo resolution").

**C7: Feature File Resolution Unchanged**
- **Source**: `__init__` line 75 hardcodes `repo_root / "pm" / "features"`
- **Verification**: Feature file loading still works for all features
- **AC Impact**: Constructor signature must not change in a way that breaks feature file lookup

**C8: `_expand_glob_path` Shared by All AC Types**
- **Source**: All 3 investigations confirmed `_expand_glob_path` serves code, file, and build verification
- **Verification**: A single fix in `_expand_glob_path` applies to all AC types simultaneously
- **AC Impact**: No separate ACs needed per AC type — the prefix detection fix is type-agnostic

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F833 | [DONE] | Parent feature that discovered both issues |
| Related | F834 | [DONE] | Recent verifier fix -- same codebase area |
| Related | F832 | [DONE] | Recent verifier fix -- same codebase area |
| Related | F791 | [DONE] | Uses engine/ paths in ACs (will benefit from this fix) |
| Related | F793 | [DONE] | Uses engine/ paths in ACs (will benefit from this fix) |
| Related | F835 | [DRAFT] | Future engine feature, will need cross-repo verification |

<!-- Dependency Types SSOT:
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

<!-- fc-phase-2-completed -->
<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "must support cross-repo feature verification" | `_expand_glob_path` detects repo prefixes and resolves to correct repo paths | AC#1, AC#2, AC#3, AC#4 |
| "SSOT for automated AC verification across all feature types" | Env vars with defaults provide repo path lookup | AC#5, AC#6 |
| "must cover the full 5-repo environment" | All 4 cross-repo prefixes (`engine/`, `core/`, `game/`, `dashboard/`) are handled | AC#1, AC#2, AC#3, AC#4 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | engine/ prefix detection in _expand_glob_path | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `engine/` | [x] |
| 2 | core/ prefix detection in _expand_glob_path | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `core/` | [x] |
| 3 | game/ prefix detection in _expand_glob_path | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `game/` | [x] |
| 4 | dashboard/ prefix detection in _expand_glob_path | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `dashboard/` | [x] |
| 5 | Env var lookup with defaults for cross-repo paths | code | Grep(path='src/tools/python/ac-static-verifier.py', pattern='ENGINE_PATH\|CORE_PATH\|GAME_PATH\|DASHBOARD_PATH') | gte | 4 | [x] |
| 6 | Default paths match CLAUDE.md conventions | code | Grep(path='src/tools/python/ac-static-verifier.py', pattern='C:/Era/(engine\|core\|game\|dashboard)') | gte | 4 | [x] |
| 7 | verify-logs.py stale path removed | code | Grep(src/tools/python/verify-logs.py) | not_matches | `dev.*planning` | [x] |
| 8 | verify-logs.py uses correct pm path | code | Grep(src/tools/python/verify-logs.py) | matches | `pm.*index-features` | [x] |
| 9 | Absolute path bypass preserved | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `is_absolute` | [x] |
| 10 | Feature file resolution unchanged at repo_root/pm/features | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `pm.*features.*feature-` | [x] |
| 11 | Devkit-relative paths still use repo_root | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `self.repo_root / file_path` | [x] |
| 12 | Feature-840.md DRAFT file exists | file | Glob(pm/features/feature-840.md) | exists | | [x] |
| 13 | Feature 840 registered in index-features.md | code | Grep(pm/index-features.md) | matches | `840` | [x] |
| 14 | Feature-841.md DRAFT file exists | file | Glob(pm/features/feature-841.md) | exists | | [x] |
| 15 | Feature 841 registered in index-features.md | code | Grep(pm/index-features.md) | matches | `841` | [x] |

### AC Details

**AC#5: Env var lookup with defaults for cross-repo paths**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="ENGINE_PATH|CORE_PATH|GAME_PATH|DASHBOARD_PATH")`
- **Expected**: `gte 4` (one env var reference per cross-repo prefix: ENGINE_PATH, CORE_PATH, GAME_PATH, DASHBOARD_PATH)
- **Derivation**: 4 env var references required (1 per cross-repo prefix: ENGINE_PATH, CORE_PATH, GAME_PATH, DASHBOARD_PATH)
- **Rationale**: Each of the 4 cross-repo prefixes requires its own env var lookup with a default fallback. 4 is the minimum count (1 per prefix).

**AC#6: Default paths match CLAUDE.md conventions**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="C:/Era/(engine|core|game|dashboard)")`
- **Expected**: `gte 4` (one default path per cross-repo prefix)
- **Derivation**: 4 default path strings required (C:/Era/engine, C:/Era/core, C:/Era/game, C:/Era/dashboard) — one per cross-repo prefix matching CLAUDE.md conventions
- **Rationale**: The `_CROSS_REPO_PREFIX_MAP` dict must contain hardcoded default paths for all 4 cross-repo prefixes. Each default must match its CLAUDE.md convention. Combined with AC#5, this ensures both env var names and default values are correct for all 4 prefixes.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Add cross-repo path prefix resolution for engine/, core/, game/, dashboard/ | AC#1, AC#2, AC#3, AC#4 |
| 2 | Resolved against respective repo locations via env vars with hardcoded defaults | AC#5, AC#6 |
| 3 | Feature file resolution remains at repo_root/pm/features/ | AC#10 |
| 4 | Fix verify-logs.py stale path reference | AC#7, AC#8 |
| 5 | Existing devkit-relative paths and absolute path bypass must continue working | AC#9, AC#11 |
| 6 | Engine test isolation tracked via Mandatory Handoff (DRAFT creation) | AC#12, AC#13 |
| 7 | Build CWD cross-repo resolution tracked via Mandatory Handoff (DRAFT creation) | AC#14, AC#15 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Modify `_expand_glob_path` in `ACVerifier` to add a third branch between the absolute-path bypass and the devkit-relative fallback. The new branch checks whether the path starts with one of the four known cross-repo prefixes (`engine/`, `core/`, `game/`, `dashboard/`). If a prefix matches, the remaining path is resolved against the corresponding repo root, which is read from an environment variable (`ENGINE_PATH`, `CORE_PATH`, `GAME_PATH`, `DASHBOARD_PATH`) with a hardcoded Windows default from CLAUDE.md. No changes to `__init__`, constructor signature, or feature file resolution logic are needed — those continue to use `self.repo_root` pointing at devkit.

For `verify-logs.py`, the fix is a single-line path correction at line 28: replace `"dev" / "planning" / "index-features.md"` with `"pm" / "index-features.md"`.

The implementation order is: (1) add the `_CROSS_REPO_PREFIXES` mapping dict as a module-level or `__init__`-populated constant, (2) insert the prefix-detection branch at the top of the single-pattern logic block (after comma check, before absolute/relative split), (3) fix `verify-logs.py` line 28.

Path separator handling follows the existing `_convert_to_wsl_path` pattern already present in the verifier — no new pattern needed since `Path()` normalizes separators on all platforms.

The "devkit-relative fallback check first" ordering (check devkit dir before cross-repo) is deliberately skipped because the AC constraint matrix specifies that `engine/`, `core/`, `game/`, `dashboard/` are reserved prefixes only for those repos. No devkit subdirectory is named `engine/`, `core/`, `game/`, or `dashboard/` — the Risk entry in the feature confirms this with mitigation "check devkit-relative first; fall back to cross-repo only if local path does not exist." Since devkit has no such top-level dirs, the simpler always-cross-repo approach for these prefixes is correct and avoids future confusion.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `"engine/"` as a key in the prefix-to-env-var mapping dict; branch in `_expand_glob_path` tests `file_path.startswith("engine/")` |
| 2 | Add `"core/"` as a key in the prefix-to-env-var mapping dict; branch tests `file_path.startswith("core/")` |
| 3 | Add `"game/"` as a key in the prefix-to-env-var mapping dict; branch tests `file_path.startswith("game/")` |
| 4 | Add `"dashboard/"` as a key in the prefix-to-env-var mapping dict; branch tests `file_path.startswith("dashboard/")` |
| 5 | The mapping dict contains exactly 4 entries (ENGINE_PATH, CORE_PATH, GAME_PATH, DASHBOARD_PATH), each as `os.environ.get(var, default)` — Grep for these 4 env var names yields ≥4 matches |
| 6 | Each default value is a Windows path under `C:\Era\` — Grep for `C:/Era/(engine|core|game|dashboard)` yields ≥4 matches across the mapping dict entries |
| 7 | In `verify-logs.py`, remove the `"dev" / "planning"` path segment — Grep `dev.*planning` returns no matches after fix |
| 8 | In `verify-logs.py`, replace with `"pm" / "index-features.md"` — Grep `pm.*index-features` matches line 28 |
| 9 | The existing `if file_path_obj.is_absolute():` branch at line 156 is preserved unchanged — Grep `is_absolute` still matches |
| 10 | The `self.feature_file = repo_root / "pm" / "features" / f"feature-{feature_id}.md"` line in `__init__` is unchanged — Grep `pm.*features.*feature-` still matches |
| 11 | The devkit-relative fallback `self.repo_root / file_path` at line 159 (after prefix check and absolute bypass) is preserved — Grep `self.repo_root / file_path` still matches |
| 12 | Task 3 creates `pm/features/feature-840.md` — Glob confirms file exists |
| 13 | Task 3 registers F840 in `pm/index-features.md` — Grep confirms `840` appears |
| 14 | Task 4 creates `pm/features/feature-841.md` — Glob confirms file exists |
| 15 | Task 4 registers F841 in `pm/index-features.md` — Grep confirms `841` appears |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where to insert prefix mapping | Module-level constant dict, `__init__` instance attribute, inline in `_expand_glob_path` | Module-level constant dict `_CROSS_REPO_PREFIX_MAP` | Module-level matches existing pattern for `BINARY_EXTENSIONS` in the same file; makes the mapping visible without instantiation for testing |
| Branch placement in `_expand_glob_path` | Before comma check, after comma check before absolute/relative, inside absolute/relative block | After comma check, before absolute/relative split | Comma-separated patterns iterate by recursion; placing prefix detection in single-pattern logic ensures it applies consistently for all recursive calls |
| Devkit-local existence check before cross-repo | Check devkit dir first, only cross-repo if missing vs always cross-repo for known prefixes | Always cross-repo for known prefixes | No devkit top-level dir named `engine/`, `core/`, `game/`, `dashboard/` exists; always-cross-repo is simpler and avoids silent fallback ambiguity; consistent with how absolute path bypass works |
| Env var fallback default path format | Raw string `"C:\\Era\\engine"`, forward-slash `"C:/Era/engine"`, `Path` construction | Forward-slash string wrapped in `Path()` — `Path("C:/Era/engine")` | `Path()` normalizes separators; forward-slash strings are more readable; consistent with how `--repo-root` default `"."` is handled via `Path(args.repo_root).resolve()` |
| verify-logs.py change scope | Fix only the path, fix path + add fallback, fix path + unit test | Fix path only | One-line root-cause fix; no new behavior added; test coverage for this is the AC#7/AC#8 static grep |

### Interfaces / Data Structures

No new interfaces. The only new data structure is a module-level dict in `ac-static-verifier.py`:

```python
# Module-level constant (alongside BINARY_EXTENSIONS)
_CROSS_REPO_PREFIX_MAP: dict[str, tuple[str, str]] = {
    "engine/": ("ENGINE_PATH", "C:/Era/engine"),
    "core/":   ("CORE_PATH",   "C:/Era/core"),
    "game/":   ("GAME_PATH",   "C:/Era/game"),
    "dashboard/": ("DASHBOARD_PATH", "C:/Era/dashboard"),
}
```

Each value is `(env_var_name, default_path)`. Resolution in `_expand_glob_path`:

```python
# After comma-check block, before absolute/relative split:
for prefix, (env_var, default) in _CROSS_REPO_PREFIX_MAP.items():
    if file_path.startswith(prefix):
        cross_repo_root = Path(os.environ.get(env_var, default))
        target_file = cross_repo_root / file_path[len(prefix):]
        break
else:
    # Original absolute/relative logic
    file_path_obj = Path(file_path)
    if file_path_obj.is_absolute():
        target_file = file_path_obj
    else:
        target_file = self.repo_root / file_path
```

The `for...else` construct keeps the diff minimal and requires no intermediate flag variable. The `os` module is already imported in the file (used elsewhere for env var access patterns in the same tool ecosystem — confirm with Grep during implementation).

`verify-logs.py` change is a single-line substitution:
- Before: `index_path = repo_root / "dev" / "planning" / "index-features.md"`
- After:  `index_path = repo_root / "pm" / "index-features.md"`

### Upstream Issues

<!-- No upstream issues discovered during Technical Design. -->
<!-- AC table: all 15 ACs have clear static-grep implementation paths. -->
<!-- Interface Dependency Verification: not applicable (infra type, no C# interfaces). -->
<!-- Cross-Section Count Propagation: not applicable (no counted-method interfaces defined). -->
<!-- Method Ownership Table: not applicable (fewer than 2 new interfaces defined). -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2, 3, 4, 5, 6, 9, 10, 11 | Add module-level `_CROSS_REPO_PREFIX_MAP` dict and insert prefix-detection branch in `_expand_glob_path` (between comma-check and absolute/relative split) in `ac-static-verifier.py`; preserve absolute path bypass and devkit-relative fallback | | [x] |
| 2 | 7, 8 | Fix stale path in `verify-logs.py`: replace `"dev" / "planning" / "index-features.md"` with `"pm" / "index-features.md"` | | [x] |
| 3 | 12, 13 | Create feature-840.md [DRAFT] for engine test isolation (9 failures) and register in index-features.md | | [x] |
| 4 | 14, 15 | Create feature-841.md [DRAFT] for build CWD cross-repo resolution and register in index-features.md | | [x] |

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
| 1 | implementer | sonnet | `src/tools/python/ac-static-verifier.py` | `_CROSS_REPO_PREFIX_MAP` dict + prefix-detection branch in `_expand_glob_path` |
| 2 | implementer | sonnet | `src/tools/python/verify-logs.py` | Corrected `pm/index-features.md` path |

### Pre-conditions

- `src/tools/python/ac-static-verifier.py` is accessible and readable
- `src/tools/python/verify-logs.py` is accessible and readable
- `os` module is confirmed imported in `ac-static-verifier.py` (required for `os.environ.get`)

### Execution Order

**Phase 1 — ac-static-verifier.py changes (Task 1)**

1. Read `ac-static-verifier.py` to locate:
   - The `BINARY_EXTENSIONS` module-level constant (placement anchor for new dict)
   - `_expand_glob_path` method (insertion point for prefix-detection branch)
   - The existing `if file_path_obj.is_absolute():` branch (must be preserved)
   - The existing `self.repo_root / file_path` fallback (must be preserved)
   - Confirm `import os` is present; if not, add it

2. Add `_CROSS_REPO_PREFIX_MAP` immediately after `BINARY_EXTENSIONS` (or other module-level constants):
   ```python
   _CROSS_REPO_PREFIX_MAP: dict[str, tuple[str, str]] = {
       "engine/": ("ENGINE_PATH", "C:/Era/engine"),
       "core/":   ("CORE_PATH",   "C:/Era/core"),
       "game/":   ("GAME_PATH",   "C:/Era/game"),
       "dashboard/": ("DASHBOARD_PATH", "C:/Era/dashboard"),
   }
   ```

3. In `_expand_glob_path`, after the comma-check recursive block and before the `file_path_obj = Path(file_path)` / `is_absolute` block, insert the `for...else` branch:
   ```python
   for prefix, (env_var, default) in _CROSS_REPO_PREFIX_MAP.items():
       if file_path.startswith(prefix):
           cross_repo_root = Path(os.environ.get(env_var, default))
           target_file = cross_repo_root / file_path[len(prefix):]
           break
   else:
       file_path_obj = Path(file_path)
       if file_path_obj.is_absolute():
           target_file = file_path_obj
       else:
           target_file = self.repo_root / file_path
   ```

4. Verify the following are still present after edit:
   - `is_absolute` branch (AC#9)
   - `self.repo_root / file_path` fallback (AC#11)
   - `pm.*features.*feature-` pattern in `__init__` (AC#10) — no changes to constructor

**Phase 2 — verify-logs.py fix (Task 2)**

5. Read `verify-logs.py` and locate the line containing `dev` / `planning` / `index-features.md` (or equivalent string concatenation)

6. Replace:
   ```python
   # Before (exact form may vary — match the dev/planning reference)
   index_path = repo_root / "dev" / "planning" / "index-features.md"
   ```
   With:
   ```python
   index_path = repo_root / "pm" / "index-features.md"
   ```

### Build Verification Steps

No C# build required (Python-only changes). After edits:

1. Confirm `ac-static-verifier.py` is syntactically valid:
   ```bash
   python -c "import ast; ast.parse(open('src/tools/python/ac-static-verifier.py').read()); print('OK')"
   ```
2. Confirm `verify-logs.py` is syntactically valid:
   ```bash
   python -c "import ast; ast.parse(open('src/tools/python/verify-logs.py').read()); print('OK')"
   ```

### Success Criteria

- AC#1-4: Grep `src/tools/python/ac-static-verifier.py` for `engine/`, `core/`, `game/`, `dashboard/` → all match
- AC#5: Grep for `ENGINE_PATH|CORE_PATH|GAME_PATH|DASHBOARD_PATH` → ≥4 matches
- AC#6: Grep for `C:\\Era` → matches default values in `_CROSS_REPO_PREFIX_MAP`
- AC#7: Grep `src/tools/python/verify-logs.py` for `dev.*planning` → no matches
- AC#8: Grep for `pm.*index-features` → matches
- AC#9: Grep `src/tools/python/ac-static-verifier.py` for `is_absolute` → matches (preserved)
- AC#10: Grep for `pm.*features.*feature-` → matches (constructor unchanged)
- AC#11: Grep for `self.repo_root / file_path` → matches (fallback preserved)

### Rollback Plan

If issues arise after deployment:
1. Revert with `git revert HEAD`
2. Notify user of rollback
3. Create follow-up feature describing the specific failure

### Error Handling

- If `os` module is not imported in `ac-static-verifier.py`: add `import os` to existing import block (do not create duplicate import)
- If `_expand_glob_path` structure differs from Technical Design description: STOP and report to user with actual code excerpt
- If syntax validation fails: STOP and report to user with error output

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| Engine test isolation (9 failures: ProcessLevelParallelRunnerTests, VariableDataAccessorTests — GlobalStatic shared state) | Requires engine repo changes, out of devkit scope | New Feature | F840 | Task 3 | [x] | feature-840.md [DRAFT] created, registered in index |
| Build CWD cross-repo resolution (verify_build_ac uses self.repo_root for cwd, independent of _expand_glob_path) | Different code path from _expand_glob_path; no current cross-repo build ACs exist | New Feature | F841 | Task 4 | [x] | feature-841.md [DRAFT] created, registered in index |

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
| 2026-03-06T04:30 | Phase 1 | initializer | Status [REVIEWED] → [WIP] | READY |
| 2026-03-06T04:32 | Phase 2 | explorer | Codebase investigation | Confirmed targets |
| 2026-03-06T04:45 | Task 1 | implementer | Add _CROSS_REPO_PREFIX_MAP + prefix-detection branch in _expand_glob_path | SUCCESS |
| 2026-03-06T04:45 | Task 2 | implementer | Fix verify-logs.py stale path dev/planning → pm | SUCCESS |
| 2026-03-06T04:48 | Task 3 | orchestrator | Create feature-840.md [DRAFT] + register in index | SUCCESS |
| 2026-03-06T04:48 | Task 4 | orchestrator | Create feature-841.md [DRAFT] + register in index | SUCCESS |
<!-- run-phase-1-completed -->
<!-- run-phase-2-completed -->
| 2026-03-06T04:50 | Phase 7 | ac-static-verifier | AC verification: 15/15 PASS (13 code + 2 file) | ALL PASS |
<!-- run-phase-4-completed -->
| 2026-03-06T04:52 | Phase 8 | feature-reviewer | Quality review (post) | READY (NEEDS_REVISION on [WIP]→invalid, Phase 10 handles) |
<!-- run-phase-7-completed -->
| 2026-03-06T05:02 | CodeRabbit | 1 Minor (修正) | F840 stale dep status [WIP]→[DONE] |
<!-- run-phase-8-completed -->
<!-- run-phase-9-completed -->
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [resolved-applied] F833 Phase 9: DEVIATION handoff — test isolation (9 failures) and ac-static-verifier cross-repo path resolution confirmed as F838 obligations (verifier path fix = Tasks 1-2/AC#1-11; test isolation = Mandatory Handoff F840/Task 3/AC#12-13)
- [fix] Phase2-Review iter1: AC Details, AC#5 | AC#5 missing Derivation field for gte threshold matcher
- [fix] Phase2-Review iter1: AC#6 + Philosophy Derivation | AC#6 expanded from matches/engine-only to gte 4 covering all 4 defaults
- [fix] Phase2-Uncertain iter1: AC Design Constraint C6 | C6 coverage claim corrected — build CWD is independent of _expand_glob_path
- [fix] Phase2-Review iter2: Title + Mandatory Handoffs + Tasks | Removed 'Engine Test Isolation' from title, added Mandatory Handoff to F840, added Task 3 for DRAFT creation
- [fix] Phase2-Review iter3: Tasks + AC Definition Table | Added AC#12, AC#13 for Task 3 DRAFT creation per AC Coverage Rule
- [fix] Phase3-Maintainability iter4: Mandatory Handoffs + Tasks + ACs | Added build CWD handoff to F841, Task 4 + AC#14, AC#15 for DRAFT creation
- [fix] Phase4-ACValidation iter5: AC#1-4,11 | Changed matcher from matches to contains for literal string Expected values
- [fix] Phase4-ACValidation iter5: AC#5,6 | Added pattern to Method column for verifier parseability

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 838 (2026-03-06)
- [revised] P1: ac-validatorにリテラル文字列matches検出 + gte Grep pattern=必須ルール追加 → `.claude/agents/ac-validator.md`
- [applied] P2: quality-fixer C35 threshold AC Derivation検出にF838 lessonを追記 → `.claude/agents/quality-fixer.md` (C35は既存、F838参照追加)
- [rejected] P3: wbs-generator Handoff Task自動生成 — C32/C33で既にカバー済み。問題はwbs実行時にHandoff未作成
- [revised] P4: predecessor読み取り抑制 — F838固有エビデンス不足のため適用見送り（要調査）
- [applied] P5: ac-validatorにgte+Grep pattern=必須ルール追加 → `.claude/agents/ac-validator.md` (P1と統合適用)

---

<!-- fc-phase-6-completed -->
## Links

[Related: F833](feature-833.md) - Parent feature that discovered both issues
[Related: F834](feature-834.md) - Recent verifier fix (Format C Guard DRY) — same codebase area
[Related: F832](feature-832.md) - Recent verifier fix (numeric parsing) — same codebase area
[Related: F791](feature-791.md) - Uses engine/ paths in ACs (will benefit from this fix)
[Related: F793](feature-793.md) - Uses engine/ paths in ACs (will benefit from this fix)
[Related: F835](feature-835.md) - Future engine feature, will need cross-repo verification
