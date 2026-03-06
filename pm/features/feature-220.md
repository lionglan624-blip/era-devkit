# Feature 220: Flow Test Directory Mode Fix

## Status: [DONE]

## Type: engine

## Background

### Philosophy (Mid-term Vision)
Test infrastructure should work reliably with both file patterns and directory paths.

### Problem (Current Issue)
During F188 execution, discovered that `--flow tests/regression/` directory mode fails silently:
- Directory mode finds all `*.json` files but FlowTestScenario pairing expects `scenario-*.json` naming convention, causing mismatched input file derivation for non-conforming JSON files
- No error message, just 0/0 tests run
- Glob pattern `scenario-*.json` works correctly

Additionally, when `HasInputFile` is false:
- Error is logged but test still runs
- Child process starts without `--input-file`
- Process hangs waiting for stdin (60s timeout)

### Goal (What to Achieve)
1. Directory paths should auto-expand to `scenario-*.json` pattern
2. Tests with missing input files should be skipped with clear error
3. No silent failures or hangs

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Directory path expands to scenarios (positive) | output | --flow | contains | "passed" | [x] |
| 2 | Missing input file shows SKIP (negative/error handling) | output | --flow | contains | "SKIP:" | [x] |
| 3 | Non-existent directory shows error (negative) | output | --flow | contains | "Directory not found" | [x] |
| 4 | Empty directory shows no tests (edge case) | output | --flow | contains | "0/0" | [x] |

### AC Details

**Test AC1**: `dotnet run ... --flow Game/tests/ac/engine/feature-220/`
**Fixture**: `scenario-valid.json` + `input-valid.txt` (directory expansion finds this)
**Expected**: Output contains "passed" (proves directory expansion worked)

**Test AC2**: `dotnet run ... --flow Game/tests/ac/engine/feature-220/scenario-missing-input.json`
**Fixture**: `scenario-missing-input.json` (WITHOUT input-missing-input.txt)
**Expected**: Output matches "SKIP: scenario-missing-input (missing input file)" (early exit prevents 60s timeout)

**Test AC3**: `dotnet run ... --flow Game/tests/nonexistent/`
**Expected**: Error output contains "Directory not found"

**Test AC4**: `dotnet run ... --flow Game/tests/ac/engine/feature-220-empty/`
**Expected**: Output contains "0/0"

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | - | Create directories Game/tests/ac/engine/feature-220/ and feature-220-empty/ (note: engine/ doesn't exist yet), add scenario-valid.json + input-valid.txt and scenario-missing-input.json (no input) | [x] |
| 1 | 1 | In HeadlessRunner.cs ExpandInjectPath(), change Directory.GetFiles pattern from `*.json` to `scenario-*.json` (prevents pairing errors with non-scenario JSON files) | [x] |
| 2 | 2 | In HeadlessRunner.cs Main() flow test loop (lines 1285-1297), when !scenario.HasInputFile, output "SKIP: {scenario.Name} (missing input file)" to stdout and skip adding to scenarios list (continue) | [x] |
| 3 | 3 | In ExpandInjectPath(), add Directory.Exists check and output "Directory not found: {path}" error for non-existent directories | [x] |
| 4 | 4 | Verify empty directory shows "0/0" (no code change if scenario-*.json pattern correctly returns 0 files) | [x] |
| 5 | - | Update testing SKILL.md: (1) update `--flow tests/regression/` NG pattern note, (2) add Method column to AC Definition Format (align with feature-template.md F165) | [x] |

**Test Fixtures** (setup before testing):
- `Game/tests/ac/engine/feature-220/scenario-valid.json` + `input-valid.txt` (AC1)
- `Game/tests/ac/engine/feature-220/scenario-missing-input.json` without input file (AC2)
- `Game/tests/ac/engine/feature-220-empty/` empty directory (AC4)

---

## Review Notes
- Discovered during F188 kojo implementation
- `--unit` mode works correctly with directories
- `--flow` mode only works with explicit glob patterns

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-26T00:00:00Z | Initialization | initializer | Status PROPOSED→WIP, verified feature structure | READY |
| 2025-12-26T00:01:00Z | Implementation | implementer | Tasks 0-5 complete | SUCCESS |
| 2025-12-26T00:02:00Z | Verification | smoke-test | All 4 ACs verified | PASS |
| 2025-12-26T00:03:00Z | Review | feature-reviewer | Post-review complete | READY |

## Links
- [feature-188.md](feature-188.md) - Discovery context
