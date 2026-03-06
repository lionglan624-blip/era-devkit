# Feature 229: Infrastructure Verification

## Status: [DONE]

## Type: erb

## Keywords (検索用)

`pre-commit`, `hook`, `verify-logs`, `IMPLE_FEATURE_ID`, `eratw-reader`, `path`, `portable`, `log`, `output`, `test`

---

## Summary

Verify and fix infrastructure tools discovered during F223 audit: pre-commit hook functionality, verify-logs.py glob pattern bug, eratw-reader hardcoded path, IMPLE_FEATURE_ID placement, and test log output location.

---

## Background

### Philosophy (Mid-term Vision)

Infrastructure tools must be portable, reliable, and verifiable. Pre-commit hooks provide the final safety net against broken implementations. Log verification should accurately detect all test results. File placement must be consistent and predictable across all environments.

### Problem (Current Issue)

F223 audit revealed multiple infrastructure issues:

| Issue | Problem | Impact |
|:-----:|---------|--------|
| I1 | pre-commit hook exists but operation unconfirmed | CI may not protect as expected |
| I2 | verify-logs.py output format not standardized | Inconsistent reporting |
| I4 | ~~eratw-reader path hardcoded~~ **RESOLVED by F225** | ~~Non-portable~~ Config-based (ERATW_PATH env var) |
| I5 | verify-logs.py glob pattern bug: `--scope feature:N` doesn't find `kojo/feature-N/` logs | AC test results not detected (e.g., F188/F194 show OK:0/0 despite 160 PASSes) |
| W4 | /do workflow claims "CI verified" but hook functionality unconfirmed | False confidence |
| P1 | IMPLE_FEATURE_ID created as `c:Eraera紅魔館protoNTR.gitIMPLE_FEATURE_ID` in root | do.md L203 uses relative path, CWD-dependent |
| P3 | Test logs scattered in root directory (`*.log`) | Output location not unified |

**Note**: Malformed file `c:Eraera紅魔館protoNTR.gitIMPLE_FEATURE_ID` no longer exists (verified 2025-12-27).

### Goal (What to Achieve)

1. Verify pre-commit hook executes correctly and provides protection
2. Standardize verify-logs.py output format
3. ~~Make eratw-reader path portable~~ (already resolved by F225)
4. Fix verify-logs.py glob pattern to detect nested kojo logs
5. Fix IMPLE_FEATURE_ID creation path (malformed file already cleaned)
6. Unify test log output location to `.tmp/` or `Game/logs/debug/`

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Pre-commit hook executes | exit_code | bash | succeeds | - | [x] |
| 2 | Pre-commit regression test runs | output | bash | contains | "[1/2] regression tests..." | [x] |
| 3 | verify-logs.py standardized output | output | python | contains | `OK:` or `FAIL:` with count | [x] |
| 4 | eratw-reader config-based path | code | Grep | contains | "ERATW_PATH\|CLAUDE.md" | [x] |
| 5 | verify-logs.py finds nested kojo logs | exit_code | python | succeeds | - | [x] |
| 6 | do.md uses absolute path for IMPLE_FEATURE_ID | code | Grep | contains | "git rev-parse --show-toplevel" | [x] |
| 7 | Malformed IMPLE file removed | file | Glob | not_exists | - | [x] |
| 8 | No root log pollution | file | Glob | not_exists | *.log in project root (depth 1) | [x] |
| 9 | do.md documents unified log location | code | Grep | contains | "logs/debug\|.tmp" in do.md | [x] |
| 10 | Build succeeds | build | dotnet | succeeds | engine/uEmuera.Headless.csproj | [x] |

### AC Details

**AC1-2 Test**: Execute hook directly (--dry-run does NOT trigger hooks)
```bash
git config core.hooksPath .githooks
bash .githooks/pre-commit 2>&1
```
**Expected**: Hook executes (exit code 0) and shows "[1/2] regression tests"

**AC3 Test**: Run verify-logs.py and check output format
```bash
python tools/verify-logs.py --dir Game/logs/prod --scope regression
```
**Expected**: Output contains `OK:` or `FAIL:` with count (format: `{label}:         OK:{passed}/{total}`)

**AC4 Test**: Check eratw-reader.md references configuration
```bash
grep -E "(CLAUDE\.md|config|env)" .claude/agents/eratw-reader.md
```
**Expected**: Path sourced from config, not hardcoded

**AC5 Test**: Run verify-logs with feature scope (tests glob fix)
```bash
python tools/verify-logs.py --dir Game/logs/prod --scope feature:188
echo $?  # Should be 0 (succeeds when logs found)
```
**Expected**: Exit code 0 indicates logs found in nested kojo/feature-188/ directory
**Note**: Current state: FAIL (OK:0/0 due to glob bug). After Task 5 fix: PASS.

**AC6 Test**: Check do.md uses absolute path for IMPLE_FEATURE_ID
```bash
grep -E "git rev-parse --show-toplevel|REPO_ROOT" .claude/commands/do.md
```
**Expected**: do.md uses absolute path derivation for .git/IMPLE_FEATURE_ID

**Note**: AC7 already satisfied - malformed file no longer exists.

**AC8 Test**: Verify no log files in project root (depth 1 only)
```bash
find . -maxdepth 1 -name "*.log" | wc -l  # Should be 0
```
**Note**: AC8 scope is depth 1 only. Logs in `engine/` and `Game/agents/logs/` are **out of scope** for F229 (potential Quick Win for future cleanup).

**AC9 Test**: Verify do.md references unified log location
```bash
grep -E "logs/debug|\.tmp" .claude/commands/do.md
```
**Expected**: do.md references `logs/debug/` or `.tmp/` for test outputs

**AC10 Test**:
```bash
dotnet build
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Execute `bash .githooks/pre-commit` and verify exit code 0 | [x] |
| 2 | 2 | Execute pre-commit and verify output contains "[1/2] regression tests" | [x] |
| 3 | 3 | Standardize verify-logs.py output format | [x] |
| 4 | 4 | ~~Move eratw-reader path~~ (resolved by F225) | [x] |
| 5 | 5 | Fix verify-logs.py glob pattern to find nested kojo logs (`**/{scope_pattern}/*`) | [x] |
| 6 | 6 | Update do.md IMPLE_FEATURE_ID to use `git rev-parse --show-toplevel` for absolute path | [x] |
| 7 | 7 | ~~Remove malformed IMPLE file~~ (already removed) | [x] |
| 8 | 8 | Move root *.log files to .tmp/ (4 files found) | [x] |
| 9 | 9 | ~~do.md documents unified log location~~ (do.md L358, L362) | [x] |
| 10 | 10 | Verify `dotnet build engine/uEmuera.Headless.csproj` succeeds | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Technical Details

### I5: verify-logs.py Glob Pattern Bug

**Location**: `tools/verify-logs.py:32`

**Current code**:
```python
result_files = list(ac_dir.glob(f"{scope_pattern}/*-result.json"))
```

**Problem**: Pattern `feature-188/*-result.json` doesn't match `kojo/feature-188/*-result.json`

**Why it fails**: The pattern looks for files directly at `ac/feature-188/`, but actual files are at `ac/kojo/feature-188/` (nested under type directory).

**Fix** (uses `**/` prefix for recursive directory matching):
```python
# Note: When scope_pattern is "**" (for scope=all), use original pattern
# to avoid "**/**/*" double recursion
if scope_pattern == "**":
    result_files = list(ac_dir.glob(f"{scope_pattern}/*-result.json"))
else:
    result_files = list(ac_dir.glob(f"**/{scope_pattern}/*-result.json"))
```

### P1: IMPLE_FEATURE_ID Path Issue

**Location**: `.claude/commands/do.md:203`

**Current code**:
```bash
echo "{ID}" > .git/IMPLE_FEATURE_ID
```

**Problem**: Relative path is CWD-dependent. When executed from wrong directory, creates malformed filename.

**Evidence**: Existing file `c:Eraera紅魔館protoNTR.gitIMPLE_FEATURE_ID` in root directory.

**Root cause**: Command executed when CWD was `c:\Era\era紅魔館protoNTR`, causing `.git/` to be interpreted as filename prefix.

**Fix**: Use absolute path or ensure CWD before execution:
```bash
# Option 1: Absolute path from repo root
REPO_ROOT=$(git rev-parse --show-toplevel)
echo "{ID}" > "$REPO_ROOT/.git/IMPLE_FEATURE_ID"

# Option 2: Ensure CWD
cd "$(git rev-parse --show-toplevel)"
echo "{ID}" > .git/IMPLE_FEATURE_ID
```

### I4: eratw-reader Path Portability

**Status**: ✅ RESOLVED by F225

eratw-reader.md now uses configurable path:
1. Environment variable: `ERATW_PATH` (highest priority)
2. CLAUDE.md default: `c:\Era\eraTW`
3. Fallback: hardcoded path (deprecated)

### P3: Test Log Output Unification

**Current scattered logs** (in root):
```
parallel-test-full.log
test-output.log
test-parallel-20.log
test-parallel-sc002.log
```
**Note**: Verified 2025-12-27: These 4 files EXIST at project root. Task 8 will move them to `.tmp/`.

**Proposed unified location**: `.tmp/test-*.log` (temporary outputs) or `Game/logs/debug/` (debug logs)

**Update locations**:
- do.md Phase 6 test execution commands
- Any scripts generating test logs
- .gitignore (ensure `.tmp/*.log` ignored)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | verify | FL | Glob for malformed IMPLE file | NOT_FOUND (clean) |
| 2025-12-27 | verify | FL | Glob for root *.log files | FOUND: 4 files |
| 2025-12-27 | fl | FL | Iterations 1-10 | Fixes applied, pending issues exist |
| 2025-12-27 | update | FL | Type: infra → erb | Implementation required (AC5,6,8) |
| 2025-12-27 | finalize | FINALIZER | Mark all ACs [x], Tasks [O] | DONE |

---

## Dependencies

- **F225** (SSOT Consolidation): Infrastructure verification should occur after SSOT unification to avoid verifying against obsolete documentation

---

## Links

- [F223](feature-223.md) - Parent audit feature (Issue Inventory Categories 5 and 8)
- [F225](feature-225.md) - SSOT Consolidation (prerequisite)
- [F218](feature-218.md) - Context-aware pre-commit scope
- [F212](feature-212.md) - Pre-commit scope definition

---

## Notes

### Cleanup Checklist

Before marking this feature [DONE]:
1. ~~Remove malformed file~~ ✅ Already removed (verified 2025-12-27)
2. Move any root `*.log` files to `.tmp/` or `Game/logs/debug/`
3. Verify `.gitignore` covers new log locations
4. Test pre-commit hook with dry-run commit
5. Run verify-logs.py with `--scope feature:188` to confirm glob fix

### Verification Priority

**P1 (Critical)**:
- I5 (verify-logs glob bug): Affects AC detection reliability
- P1 (IMPLE_FEATURE_ID placement): Creates filesystem pollution

**P2 (Important)**:
- I1, W4 (pre-commit verification): CI confidence
- P3 (log location): Workspace cleanliness

**P3 (Nice-to-have)**:
- I2 (verify-logs output format): Better UX but not blocking
