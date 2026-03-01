# Feature 787: Test Log Lifecycle Management

## Status: [DONE]
<!-- fl-reviewed: 2026-02-13T00:00:00Z -->

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

## Review Context
<!-- Written by /run F786 Phase 9 deep-explorer findings. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F786 |
| Discovery Point | /run Phase 9 — deep-explorer CI review (verify-logs.py `--scope all` returns ERR:85\|6791) |
| Timestamp | 2026-02-12 |

### Identified Gap

F784 restored CI (pre-commit + GitHub Actions). F786 removed regression dead code and dispositioned orphaned obligations. However, neither feature addressed the **data problem**: `_out/logs/prod/ac/` contains ~360 point-in-time test result snapshots from ~100+ completed features. These accumulate indefinitely because no lifecycle management exists — logs are created during `/run` but never cleaned up when a feature reaches [DONE].

As a result:
- `verify-logs.py --scope all` reports ERR:85|6791 (85 historical failures across 6791 total tests)
- The tool returns meaningful results only when scoped to a specific feature (`--scope feature:{ID}`)
- 10 orphaned engine TRX files in `ac/engine/` include 2 with genuine test failures (KojoComparer State extraction, dated 2026-02-05)
- There is no mechanism to prevent future accumulation

Three layers of CI exist, serving different purposes:

| Layer | Tool | Purpose | Status |
|-------|------|---------|--------|
| Pre-commit hook | `.githooks/pre-commit` | Synchronous gate (C# build + test) | Working (F784) |
| GitHub Actions | `.github/workflows/test.yml` | Async CI on push | Working (F784) |
| verify-logs.py | `src/tools/python/verify-logs.py` | Post-hoc AC log aggregation for /run Phase 9 | **Broken for `--scope all`** |

Root cause: verify-logs.py treats `_out/logs/prod/ac/` as current test state, but the directory is actually an append-only archive of historical snapshots. Feature-scoped mode works correctly; global mode aggregates noise.

Stale log breakdown:

| Directory | Files | Content |
|-----------|:-----:|---------|
| `ac/build/` | ~38 | build-result.json from [DONE] features |
| `ac/code/` | ~73 | code-result.json from [DONE] features |
| `ac/file/` | ~83 | file-result.json from [DONE] features |
| `ac/kojo/` | ~30+ | kojo test results from [DONE] features |
| `ac/engine/` | ~11 | TRX files (2 KojoComparer FAIL, rest orphaned) |
| Other | ~10 | f266/, f267/, feature-727/ (non-standard naming) |

### Files Involved
| File | Relevance |
|------|-----------|
| `src/tools/python/verify-logs.py` | Needs `--scope all` redesign or lifecycle-aware filtering |
| `_out/logs/prod/ac/` | Contains ~360 stale files to clean |
| `_out/logs/prod/ac/engine/` | Contains 10 orphaned TRX files (2 with FAIL) |
| `.claude/skills/testing/SKILL.md` | Documents verify-logs.py usage (Log Verification section) |
| `.claude/skills/run-workflow/PHASE-9.md` | Uses `--scope feature:{ID}` (correct, unaffected) |

### Parent Review Observations

**F784 contribution**: HIGH — restored dead CI, expanded pre-commit coverage from 56% to 79%
**F786 contribution**: LOW — removed dead code that returned 0/0, documented obligation dispositions
**Gap**: Neither feature ensured verify-logs.py returns meaningful results for global health monitoring

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

"Automated test coverage should be comprehensive and continuously enforced. Tests that exist should run; promises should be fulfilled; stale references should not mislead." (Inherited from F784/F786.) Test result logs should reflect current reality — historical snapshots from completed features should not pollute health monitoring. SSOT: ongoing test log lifecycle is managed within the /run workflow and finalizer, not by external ad-hoc cleanup; one-time migration scripts are acceptable for initial state remediation of pre-existing accumulation.

### Problem (Current Issue)

The `/run` workflow creates AC log files in `_out/logs/prod/ac/` during Phase 7 verification (via `ac-static-verifier.py`, line 919) and test execution, but no step in the entire workflow lifecycle — neither the finalizer skill (`finalizer/SKILL.md`) nor Phase 10 finalization (`PHASE-10.md`) — removes, archives, or marks these logs as stale when a feature transitions to [DONE]. The log directory was designed as a write-only append store with no corresponding destruction path. As a result, `verify-logs.py --scope all` (line 32-33, `**/*-result.json` glob) aggregates ~376 historical log files from ~100+ completed features, reporting ERR:87|7233 — a number that reflects historical noise, not current test health. Additionally, `verify_engine_logs()` (line 60-93) is scope-unaware, always scanning all TRX files regardless of `--scope` parameter. The legacy `/complete-feature` command had a Step 7 "Cleanup Test Logs" (complete-feature.md:99-104) targeting `uEmuera/logs/*.trx`, but this cleanup responsibility was lost when the workflow migrated to `/run`.

### Goal (What to Achieve)

Establish a complete log lifecycle for `_out/logs/prod/ac/`: (1) bulk-clean existing stale logs from [DONE] features and non-standard legacy directories, (2) add a prevention mechanism in the finalizer or Phase 10 that automatically cleans feature-scoped logs when a feature reaches [DONE], (3) redesign or remove `verify-logs.py --scope all` so it no longer conflates historical snapshots with current test state, (4) disposition KojoComparer TRX failures before deletion (2 FAIL TRX files require triage), and (5) make engine TRX logs scope-aware to support lifecycle filtering. After completion, `verify-logs.py --scope all` (or its replacement) should return meaningful results, and `--scope feature:{ID}` must continue to work unchanged for active /run workflows.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does `verify-logs.py --scope all` return ERR:87\|7233? | The `**/*-result.json` glob captures all historical log files from completed features, not just active ones | `src/tools/python/verify-logs.py:32-33` |
| 2 | Why do historical log files exist in `_out/logs/prod/ac/`? | `ac-static-verifier.py` writes logs during `/run` Phase 7 verification but nothing removes them afterward | `src/tools/python/ac-static-verifier.py:919-922`, `.claude/skills/run-workflow/PHASE-7.md:62-68` |
| 3 | Why is there no cleanup after feature completion? | Neither the finalizer skill nor Phase 10 of the /run workflow includes any log cleanup step | `.claude/skills/finalizer/SKILL.md` (no log mention), `.claude/skills/run-workflow/PHASE-10.md` (no cleanup step) |
| 4 | Why was cleanup never added to the workflow? | The original `/complete-feature` command had cleanup (Step 7, complete-feature.md:99-104) but targeted the wrong path (`uEmuera/logs/*.trx`); when workflow migrated to `/run`, cleanup responsibility was lost entirely | `.claude/commands/complete-feature.md:99-104` |
| 5 | Why (Root)? | The test infrastructure was designed with log creation paths (ac-static-verifier.py, headless test runner) but no log destruction or lifecycle management paths. The per-feature scope mode (`--scope feature:{ID}`) worked correctly for active workflows, so the accumulation problem was invisible until `--scope all` was run | `.claude/skills/run-workflow/PHASE-9.md:78-79` (uses `--scope feature:{ID}`, not `--scope all`) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | `verify-logs.py --scope all` reports ERR:87\|7233 (85 failures across 7233 tests) | No log destruction path exists in the workflow — logs are created during `/run` Phase 7 but never cleaned when features reach [DONE] |
| Where | `src/tools/python/verify-logs.py` output and `_out/logs/prod/ac/` directory | `/run` workflow architecture (finalizer/SKILL.md, PHASE-10.md) lacking lifecycle management; lost cleanup from `/complete-feature` migration |
| Fix | Delete stale files manually (one-time band-aid) | Add automatic cleanup to finalizer/Phase 10 + redesign `--scope all` to be lifecycle-aware |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F786 | [DONE] | Predecessor — created F787 as mandatory handoff; removed regression dead code |
| F784 | [DONE] | Related — restored CI (pre-commit + GitHub Actions); verify-logs.py was not in scope |
| F785 | [CANCELLED] | Related — cancelled regression test recovery; superseded by F786 disposition |
| F782 | [DRAFT] | Related — Post-Phase Review Phase 20; may create features that generate new AC logs |
| F205 | [DONE] (archived) | Origin — introduced verify-logs.py |
| F268 | [DONE] (archived) | Origin — introduced ac-static-verifier.py |
| F499 | [DONE] (archived) | Origin — created test-strategy.md |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Technical complexity | FEASIBLE | Changes limited to Python script (verify-logs.py), workflow docs (finalizer/SKILL.md, PHASE-10.md), and gitignored log files |
| Dependencies | FEASIBLE | F786 predecessor is [DONE]; no blocking dependencies |
| Risk level | FEASIBLE | All affected files are local (gitignored logs, Python tools, markdown docs); pre-commit and GitHub Actions CI unaffected (.githooks/pre-commit:1-69, .github/workflows/test.yml:1-43) |
| Scope | FEASIBLE | Single feature scope; no engine/build/Unity changes needed |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| verify-logs.py | HIGH | `--scope all` mode redesigned or removed; `verify_engine_logs()` made scope-aware |
| _out/logs/prod/ac/ | HIGH | ~376 stale log files deleted (bulk cleanup); ~10 non-standard directories removed |
| Finalizer workflow | MEDIUM | New cleanup step added to remove feature-scoped logs on [DONE] transition |
| testing/SKILL.md | LOW | Does not reference `--scope all`; only documents `--scope feature:{ID}` which is unaffected. No changes needed |
| test-strategy.md | LOW | References `--scope all` as usage example (line 169); command syntax preserved after redesign, only internal behavior changes. No changes needed |
| /run Phase 9 | LOW | `--scope feature:{ID}` unaffected; continues to work as designed |
| Pre-commit / GitHub Actions | LOW | Not affected — neither uses verify-logs.py |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `--scope feature:{ID}` must continue to work | `PHASE-9.md:78-79` | Redesign must preserve feature-scoped verification |
| Log files are gitignored (local-only) | `Game/.gitignore:17` | Cleanup must be runtime script/workflow action, not git operation |
| `verify_engine_logs()` has separate TRX scanning logic | `verify-logs.py:60-93` | Engine log path needs independent lifecycle consideration |
| Logs must exist during active /run workflow | `PHASE-7.md:62-68` | Cleanup timing: after Phase 9 verification, during/after Phase 10 |
| `--scope all` is not used in any automated workflow | `PHASE-9.md` (only uses `--scope feature:{ID}`) | Low risk to redesign or remove |
| Pre-commit hook and GitHub Actions do not use verify-logs.py | `.githooks/pre-commit:1-69`, `.github/workflows/test.yml:1-43` | Changes to verify-logs.py have no CI side effects |
| AC result files are always nested under `feature-{ID}/` subdirectories | `ac-static-verifier.py:919-922` (writes to `ac/{type}/feature-{ID}/`) | POST-FILTER in `verify_ac_logs()` extracts feature ID from path components; non-nested result files would be silently dropped. If future code writes result files directly under `ac/{type}/` without `feature-{ID}/` subdirectory, POST-FILTER must be updated |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Cleanup removes logs for [WIP] feature | LOW | MEDIUM | Only clean logs for features confirmed [DONE]; finalizer knows feature ID |
| KojoComparer TRX failures represent genuine issues | MEDIUM | LOW | Triage and disposition before deletion; track if needed |
| Future features continue accumulating without prevention | HIGH | MEDIUM | Must add prevention mechanism (finalizer step), not just one-time cleanup |
| verify-logs.py redesign breaks undocumented usage | LOW | LOW | `--scope all` is not part of any automated workflow |
| complete-feature.md Step 7 remains stale | LOW | LOW | Update or mark as superseded if in scope |
| Finalizer cleanup step verified by text presence only | MEDIUM | MEDIUM | AC#9/AC#10 check Grep text in skill files, not behavioral execution; future refactors could silently remove the step. Monitor in subsequent FL reviews |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Stale AC log file count | `find _out/logs/prod/ac -name "*-result.json" -o -name "*.trx" \| wc -l` | ~376 | Approximate; exact count varies by local state |
| verify-logs.py --scope all result | `python src/tools/python/verify-logs.py --scope all` | ERR:87\|7233 | 87 failures across 7233 total tests (historical noise) |
| Non-standard directories | `ls _out/logs/prod/ac/ \| grep -v "build\|code\|file\|kojo\|engine"` | f266, f267, feature-727, test | Legacy naming conventions |
| Finalizer cleanup steps | `grep -c "cleanup\|log\|ac/" .claude/skills/finalizer/SKILL.md` | 0 | No log-related content exists |

**Baseline File**: `.tmp/baseline-787.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `--scope feature:{ID}` must remain functional | PHASE-9.md:78-79 | AC must verify feature-scoped mode still works after changes |
| C2 | Log files are gitignored (local-only) | Game/.gitignore:17 | Use file system checks (Glob/Bash), not git operations |
| C3 | `verify_engine_logs()` lacks scope parameter | verify-logs.py:60-93 | AC must test both scoped and global engine log behavior |
| C4 | `--scope all` is unused in production workflows | PHASE-9.md | Low-impact to redesign; AC can verify new behavior |
| C5 | Feature type is infra | feature-787.md:17 | AC types: code, build, file, exit_code, output (not kojo). Output allowed for script stdout verification |
| C6 | KojoComparer TRX failures may represent genuine issues | Engine TRX data | Disposition before bulk cleanup; AC should verify disposition |
| C7 | Prevention mechanism must survive workflow updates | Workflow architecture | AC must verify cleanup step exists in finalizer or PHASE-10 skill file |
| C8 | Non-standard naming directories exist alongside standard ones | Data inspection | Cleanup must target all naming variants (f{ID}/, feature-{ID}/) |
| C9 | complete-feature.md Step 7 has stale cleanup path | complete-feature.md:103 | If in scope, AC should verify corrected or superseded state |

### Constraint Details

**C1: Feature-scoped verification preserved**
- **Source**: PHASE-9.md:78-79 uses `--scope feature:{ID}` exclusively
- **Verification**: Run `verify-logs.py --scope feature:{active_ID}` after changes and confirm correct behavior
- **AC Impact**: Must include regression test for feature-scoped mode

**C2: Gitignored log files**
- **Source**: Game/.gitignore:17 (`logs/`)
- **Verification**: `git status` should show no log file changes
- **AC Impact**: All cleanup verification must use Bash/Glob file system checks, not git-based assertions

**C3: Engine TRX scope gap**
- **Source**: verify-logs.py:60-93 — `verify_engine_logs()` always scans all TRX regardless of scope
- **Verification**: Read verify_engine_logs() source to confirm no scope parameter
- **AC Impact**: Either fix scope awareness or document/accept global engine scan behavior

**C4: --scope all unused**
- **Source**: No reference to `--scope all` in any workflow skill file (PHASE-7, PHASE-9, PHASE-10)
- **Verification**: Grep workflow skills for `--scope all`
- **AC Impact**: Redesign freedom is high; AC should verify new behavior matches design intent

**C5: Infra feature type**
- **Source**: feature-787.md:17
- **Verification**: N/A
- **AC Impact**: Use file, code, build, exit_code, output AC types; do not use kojo types. Output type permitted for script stdout verification (AC#2 cleanup --dry-run, AC#8 verify-logs.py --scope all)

**C6: KojoComparer disposition**
- **Source**: 2 FAIL TRX files in ac/engine/ (KojoComparer State extraction, dated 2026-02-05)
- **Verification**: Read TRX files to determine if failures are genuine regressions or stale snapshots
- **AC Impact**: Disposition decision should be documented; deletion requires explicit tracking if genuine

**C7: Prevention mechanism durability**
- **Source**: Philosophy "stale references should not mislead" requires ongoing prevention, not one-time cleanup
- **Verification**: Grep finalizer/SKILL.md or PHASE-10.md for cleanup step
- **AC Impact**: AC must verify the mechanism exists in the skill file, not just that cleanup was run once

**C8: Non-standard naming cleanup**
- **Source**: f266/, f267/, feature-727/, test/ directories in ac/ root
- **Verification**: List ac/ subdirectories and check for non-standard names
- **AC Impact**: Cleanup script or step must handle both `feature-{ID}/` and `f{ID}/` naming patterns

**C9: Legacy complete-feature.md**
- **Source**: complete-feature.md:99-104 targets wrong path (uEmuera/logs/*.trx)
- **Verification**: Read complete-feature.md Step 7
- **AC Impact**: If in scope, verify cleanup path corrected or command marked as superseded by /run

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F786 | [DONE] | Test Infrastructure Transition Obligations — removed regression code, created F787 as mandatory handoff |
| Related | F784 | [DONE] | Test Infrastructure Remediation — restored CI (pre-commit + GitHub Actions); verify-logs.py was not in scope |
| Related | F785 | [CANCELLED] | Regression Test Recovery — cancelled; superseded by F786 disposition |
| Related | F782 | [DRAFT] | Post-Phase Review Phase 20 — may create features generating new AC logs |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "stale references should not mislead" | Historical log files from [DONE] features must be removed | AC#1, AC#2 |
| "Test result logs should reflect current reality" | `--scope all` must return meaningful results (not historical noise) | AC#7, AC#8 |
| "historical snapshots from completed features should not pollute health monitoring" | Non-standard legacy directories must be cleaned | AC#3, AC#4, AC#5, AC#6 |
| "SSOT: test log lifecycle is managed within the /run workflow and finalizer, not by external ad-hoc cleanup" | Prevention mechanism must exist in finalizer or Phase 10 skill file | AC#9, AC#10, AC#15, AC#16 |
| "promises should be fulfilled" | Feature-scoped verification (existing contract) must continue to work unchanged | AC#11, AC#14 |
| "comprehensive and continuously enforced" | Engine TRX logs must be lifecycle-aware or scope-aware; prevention mechanism must trigger on every [DONE] transition | AC#9, AC#10, AC#12 |
| "promises should be fulfilled" | KojoComparer TRX failures must be dispositioned before deletion | AC#13 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Bulk cleanup script executes successfully | exit_code | python src/tools/python/cleanup-stale-ac-logs.py | succeeds | - | [x] |
| 2 | Bulk cleanup script reports stale log deletion | output | python src/tools/python/cleanup-stale-ac-logs.py --dry-run | contains | "non-active feature" | [x] |
| 3 | Non-standard legacy directory f266 removed | file | Glob(_out/logs/prod/ac/f266/) | not_exists | - | [x] |
| 4 | Non-standard legacy directory f267 removed | file | Glob(_out/logs/prod/ac/f267/) | not_exists | - | [x] |
| 5 | Non-standard legacy directory feature-727 removed | file | Glob(_out/logs/prod/ac/feature-727/) | not_exists | - | [x] |
| 6 | Non-standard legacy directory test removed | file | Glob(_out/logs/prod/ac/test/) | not_exists | - | [x] |
| 7 | verify-logs.py --scope all redesigned | code | Grep(src/tools/python/verify-logs.py) | contains | "active_features" | [x] |
| 8 | verify-logs.py --scope all lifecycle filtering works | output | python src/tools/python/verify-logs.py --scope all | contains | "active features" | [x] |
| 9 | Finalizer cleanup step added | file | Grep(.claude/skills/finalizer/SKILL.md) | contains | "logs/prod/ac" | [x] |
| 10 | PHASE-10 or finalizer documents log cleanup | file | Grep(.claude/skills/) | contains | "log cleanup" | [x] |
| 11 | Feature-scoped verification preserved | code | Grep(src/tools/python/verify-logs.py) | matches | "scope.*startswith.*feature" | [x] |
| 12 | Engine TRX logs scope-aware | code | Grep(src/tools/python/verify-logs.py) | matches | "verify_engine_logs.*scope" | [x] |
| 13 | KojoComparer TRX failures dispositioned | file | Grep(pm/features/feature-787.md) | contains | "DISPOSITION-DECISION: KojoComparer" | [x] |
| 14 | Feature-scoped verification functional | exit_code | python src/tools/python/verify-logs.py --scope feature:787 | succeeds | - | [x] |
| 15 | Finalizer cleanup has [DONE]-only guard | file | Grep(.claude/skills/finalizer/SKILL.md) | matches | "DONE.*log cleanup" | [x] |
| 16 | Finalizer Step 2B skip instruction updated after renumbering | file | Grep(.claude/skills/finalizer/SKILL.md) | matches | "Skip Steps 3-7.*Step 8" | [x] |

### AC Details

**AC#1: Bulk cleanup script executes successfully**
- **Test**: `python src/tools/python/cleanup-stale-ac-logs.py` exits with code 0
- **Expected**: Script completes without errors. Deletes stale log directories for non-active features and legacy directories.
- **Rationale**: Verifies the cleanup script runs to completion. Philosophy: "stale references should not mislead." [C2: file system check, not git]

**AC#2: Bulk cleanup script reports stale log deletion**
- **Test**: `python src/tools/python/cleanup-stale-ac-logs.py --dry-run` output contains "non-active feature"
- **Expected**: Dry-run output identifies non-active feature directories for deletion, confirming the inverse approach (delete everything NOT in active set) works correctly.
- **Rationale**: Verifies the script correctly identifies stale logs via get_active_features() inverse filtering, independent of transient file system state. [C8: both naming variants targeted]

**AC#3: Non-standard legacy directory f266 removed**
- **Test**: `Glob("_out/logs/prod/ac/f266/")` returns not_exists
- **Expected**: f266 directory removed from ac/ root.
- **Rationale**: Legacy directory with inconsistent naming convention and stale data. [C8: non-standard naming cleanup]

**AC#4: Non-standard legacy directory f267 removed**
- **Test**: `Glob("_out/logs/prod/ac/f267/")` returns not_exists
- **Expected**: f267 directory removed from ac/ root.
- **Rationale**: Legacy directory with inconsistent naming convention and stale data. [C8: non-standard naming cleanup]

**AC#5: Non-standard legacy directory feature-727 removed**
- **Test**: `Glob("_out/logs/prod/ac/feature-727/")` returns not_exists
- **Expected**: feature-727 directory removed from ac/ root.
- **Rationale**: Legacy directory with inconsistent naming convention and stale data. [C8: non-standard naming cleanup]

**AC#6: Non-standard legacy directory test removed**
- **Test**: `Glob("_out/logs/prod/ac/test/")` returns not_exists
- **Expected**: test directory removed from ac/ root.
- **Rationale**: Legacy directory with inconsistent naming convention and stale data. [C8: non-standard naming cleanup]

**AC#7: verify-logs.py --scope all redesigned**
- **Test**: `Grep(path="src/tools/python/verify-logs.py", pattern="active_features")` finds lifecycle-aware filtering logic
- **Expected**: The `--scope all` code path must reference `active_features` (or equivalent concept) to filter based on active feature state rather than globbing all historical files indiscriminately.
- **Rationale**: Root cause is that `--scope all` uses `**/*-result.json` glob without filtering. Redesign must introduce lifecycle awareness. "active_features" is a specific enough identifier that would not appear accidentally. [C4: unused in production, high redesign freedom]

**AC#8: verify-logs.py --scope all lifecycle filtering works**
- **Test**: `python src/tools/python/verify-logs.py --scope all` output contains "active features"
- **Expected**: Output references "active features" filtering, confirming `get_active_features()` is invoked and lifecycle-aware filtering is operational. Not dependent on execution timing or active feature log state.
- **Rationale**: Verifies the `--scope all` redesign is functionally connected to lifecycle filtering. Complements AC#7 (code string presence) with behavioral output verification. [C4: redesign verified]

**AC#9: Finalizer cleanup step added**
- **Test**: `Grep(path=".claude/skills/finalizer/SKILL.md", pattern="logs/prod/ac")` finds cleanup step
- **Expected**: Finalizer SKILL.md contains a step referencing `logs/prod/ac` for cleanup when a feature transitions to [DONE]. This is the prevention mechanism.
- **Rationale**: Philosophy: "SSOT: test log lifecycle is managed within the /run workflow and finalizer." Baseline: 0 log-related content in finalizer/SKILL.md. [C7: prevention mechanism durability]

**AC#10: PHASE-10 or finalizer documents log cleanup**
- **Test**: `Grep(path=".claude/skills/", pattern="log cleanup")` finds documentation
- **Expected**: At least one workflow skill file (finalizer/SKILL.md or run-workflow/PHASE-10.md) contains "log cleanup" documentation explaining when and how AC logs are removed during the feature lifecycle.
- **Rationale**: The prevention mechanism must be documented in workflow skills (not just implemented once), so it survives future workflow updates. [C7: survives workflow updates]

**AC#11: Feature-scoped verification preserved**
- **Test**: `Grep(path="src/tools/python/verify-logs.py", pattern="scope.*startswith.*feature")` confirms the feature-scoped scope parsing logic remains intact
- **Expected**: The `scope.startswith("feature:")` code path must remain functional. The regex `scope.*startswith.*feature` matches the existing conditional logic on line 144.
- **Rationale**: PHASE-9.md:78-79 exclusively uses `--scope feature:{ID}`. Any redesign must preserve this regression-free. [C1: regression prevention]

**AC#12: Engine TRX logs scope-aware**
- **Test**: `Grep(path="src/tools/python/verify-logs.py", pattern="verify_engine_logs.*scope")` confirms scope parameter was added to the function signature
- **Expected**: The `verify_engine_logs()` function definition must include a scope parameter. The regex `verify_engine_logs.*scope` matches the updated function signature.
- **Rationale**: Baseline: `verify_engine_logs(prod_dir)` has no scope parameter (line 60). After the fix, it should accept scope to filter TRX files appropriately. [C3: engine TRX scope gap]

**AC#13: KojoComparer TRX failures dispositioned**
- **Test**: `Grep(path="pm/features/feature-787.md", pattern="DISPOSITION-DECISION: KojoComparer")` finds unique disposition marker in the Execution Log
- **Expected**: The Execution Log contains a `DISPOSITION-DECISION: KojoComparer` marker documenting the disposition decision for the 2 FAIL TRX files (KojoComparer State extraction, dated 2026-02-05) — whether they are genuine regressions or stale snapshots — before deletion.
- **Rationale**: Philosophy: "promises should be fulfilled." Unique marker avoids self-referential false positive from documentation text. [C6: KojoComparer disposition]

**AC#14: Feature-scoped verification functional**
- **Test**: `python src/tools/python/verify-logs.py --scope feature:787` exits with code 0
- **Expected**: Exit code 0 (success/OK). Feature-scoped mode must work functionally after all changes.
- **Rationale**: AC#11 verifies the code text pattern exists, but AC#14 verifies the feature-scoped path actually runs successfully. Together they provide both static and dynamic verification. [C1: regression prevention]

**AC#15: Finalizer cleanup has [DONE]-only guard**
- **Test**: `Grep(path=".claude/skills/finalizer/SKILL.md", pattern="DONE.*log cleanup")` finds guard condition
- **Expected**: Finalizer SKILL.md contains text linking [DONE] status to cleanup execution (matching the pattern "[DONE] transition (log cleanup)" from line 149), ensuring [CANCELLED] features skip the cleanup step.
- **Rationale**: Technical Design requires explicit guard (`IF status_transition == "[DONE]" THEN execute cleanup ELSE skip`). Without this AC, the [CANCELLED] path could silently execute cleanup. [C7: prevention mechanism durability]

**AC#16: Finalizer Step 2B skip instruction updated after renumbering**
- **Test**: `Grep(path=".claude/skills/finalizer/SKILL.md", pattern="Skip Steps 3-7.*Step 8")` finds updated skip range
- **Expected**: Finalizer SKILL.md Step 2B references the correct post-renumbering step range (Steps 3-7 → Step 8), not the pre-renumbering range (Steps 3-6 → Step 7).
- **Rationale**: After inserting new Step 5 (cleanup) and renumbering Steps 5→6, 6→7, 7→8, the [BLOCKED]/[CANCELLED] path skip instruction must be updated. Without this AC, the skip instruction would point to the wrong step number.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Bulk-clean existing stale logs from [DONE] features | AC#1, AC#2 |
| 2 | Clean non-standard legacy directories | AC#3, AC#4, AC#5, AC#6 |
| 3 | Add prevention mechanism in finalizer or Phase 10 | AC#9, AC#10, AC#15, AC#16 |
| 4 | Redesign verify-logs.py --scope all | AC#7, AC#8 |
| 5 | --scope feature:{ID} must continue to work unchanged | AC#11, AC#14 |
| 6 | verify-logs.py --scope all returns meaningful results after completion | AC#8 |
| 7 | Engine TRX lifecycle consideration | AC#12 |
| 8 | KojoComparer TRX failures dispositioned before deletion | AC#13 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

**Three-part implementation** addressing bulk cleanup, prevention, and tool redesign:

1. **Bulk Cleanup Script**: Create a Python script (`src/tools/python/cleanup-stale-ac-logs.py`) for initial bulk cleanup of stale AC logs from [DONE] features. The script is idempotent and safe to re-run, but its primary purpose is the one-time migration. After execution, it should be archived to `src/tools/dotnet/_archived/` since the finalizer prevention mechanism (Part 2) handles ongoing cleanup. The script will:
   - Read `index-features.md` Active Features table to extract active (non-[DONE]/non-[CANCELLED]) feature IDs (inverse approach — more reliable than enumerating [DONE] IDs from manually-maintained history)
   - Delete `_out/logs/prod/ac/*/feature-{ID}/` directories where ID is NOT in active set
   - Delete non-standard legacy directories (`f266/`, `f267/`, `feature-727/`, `test/`)
   - Disposition and delete orphaned engine TRX files after recording KojoComparer failures
   - Report deletion statistics (files/directories removed)

2. **Prevention Mechanism**: Add log cleanup step to finalizer workflow:
   - Update `.claude/skills/finalizer/SKILL.md` to insert cleanup as new Step 5 (after Step 4 "Unblock Dependent Features" which is marked CRITICAL), renumbering subsequent steps (current Step 5→6, 6→7, 7→8)
   - Cleanup commands: `rm -rf _out/logs/prod/ac/*/feature-{ID}` (removes feature subdirectories) + `rm -f _out/logs/prod/ac/engine/feature-{ID}*.trx` (removes root-level engine TRX files). Note: these are agent instructions in the SKILL.md file — the implementing agent must expand `{ID}` with the actual feature ID at runtime
   - Cleanup is non-blocking: failure logs a warning but does not prevent subsequent steps (unblock cascade must never be blocked by cleanup errors). Cleanup result (success/warning/skipped) MUST be included in the Finalizer Final Report output so failures are visible and not silently lost
   - Only execute on [DONE] transition (not [BLOCKED] or [CANCELLED]). Explicit guard required: `IF status_transition == "[DONE]" THEN execute cleanup ELSE skip` — the [CANCELLED] path shares Steps 3-7 with [DONE] and would execute the new Step 5 without this guard
   - Document in `.claude/skills/run-workflow/PHASE-10.md` as part of finalization lifecycle

3. **verify-logs.py Redesign**: Make `--scope all` lifecycle-aware:
   - Add `get_active_features()` function to read `index-features.md` and extract non-[DONE]/non-[CANCELLED] feature IDs
   - Modify `verify_ac_logs()` to accept lifecycle filtering: when `scope="all"`, only scan directories matching active feature IDs
   - Add `scope` parameter to `verify_engine_logs()` for consistency (engine TRX files are rare, but should follow same pattern)
   - Preserve `--scope feature:{ID}` behavior unchanged (used by PHASE-9)

**Rationale**: This approach satisfies all 16 ACs while addressing the root cause (no destruction path) identified in 5 Whys analysis. The bulk cleanup resolves historical noise immediately, prevention mechanism ensures future features don't accumulate, and tool redesign makes `--scope all` meaningful for ongoing health monitoring.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Bulk cleanup script (`src/tools/python/cleanup-stale-ac-logs.py`) executes all cleanup operations (feature-scoped directories, legacy directories, engine TRX) and exits with code 0 on success |
| 2 | Bulk cleanup script `--dry-run` mode outputs identification of non-active feature directories for deletion without executing. Output contains "non-active feature" confirms inverse filtering via `get_active_features()` is operational |
| 3 | Bulk cleanup script explicitly deletes `ac/f266/`. Directory verified via Glob not_exists |
| 4 | Bulk cleanup script explicitly deletes `ac/f267/`. Directory verified via Glob not_exists |
| 5 | Bulk cleanup script explicitly deletes `ac/feature-727/`. Directory verified via Glob not_exists |
| 6 | Bulk cleanup script explicitly deletes `ac/test/`. Directory verified via Glob not_exists |
| 7 | Add `get_active_features()` function to verify-logs.py (lines 20-35) that parses index-features.md Active Features table and returns set of non-[DONE]/non-[CANCELLED] feature IDs. Grep for "active_features" confirms presence |
| 8 | After bulk cleanup + tool redesign, `python src/tools/python/verify-logs.py --scope all` output contains "active features", confirming lifecycle-aware filtering is operational via `get_active_features()` invocation |
| 9 | Add Step 5 (renumbered) to finalizer/SKILL.md after Step 4 "Unblock Dependent Features": "Clean AC logs: `rm -rf _out/logs/prod/ac/*/feature-{ID}`". Grep for "logs/prod/ac" confirms presence |
| 10 | Document log lifecycle in finalizer/SKILL.md Step 5 (renumbered) with comment: "# Remove feature-scoped AC logs on [DONE] transition (log cleanup)". Grep for "log cleanup" confirms presence |
| 11 | verify-logs.py line 144 `scope.startswith("feature:")` logic remains unchanged. Grep with regex `scope.*startswith.*feature` confirms preservation |
| 12 | Modify `verify_engine_logs(prod_dir: Path)` signature to `verify_engine_logs(prod_dir: Path, scope: str = "all")` on line 60. Add scope filtering logic to only scan TRX files matching active features when scope="all". Grep `verify_engine_logs.*scope` confirms signature change |
| 13 | Read 2 FAIL TRX files from `ac/engine/` (KojoComparer, dated 2026-02-05), record disposition decision in feature-787.md Execution Log with justification before bulk cleanup deletes them. Grep `KojoComparer.*disposition` matches the record |
| 14 | Run `python src/tools/python/verify-logs.py --scope feature:787` after all changes to confirm feature-scoped mode still executes successfully. AC#11 verifies code text pattern; AC#14 verifies runtime behavior |
| 15 | Add explicit `IF status_transition == "[DONE]"` guard condition to finalizer cleanup step. Grep `DONE.*log cleanup` verifies the guard exists in finalizer/SKILL.md |
| 16 | Update finalizer Step 2B skip instruction from "Skip Steps 3-6 → Go to Step 7" to "Skip Steps 3-7 → Go to Step 8" after renumbering. Grep `Skip Steps 3-7.*Step 8` verifies updated range |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Cleanup timing (when to remove logs) | A: During Phase 9 (before verification), B: During Phase 10 (after verification in finalizer), C: Manual ad-hoc script only | B: Finalizer Step 5 (renumbered) (after [DONE] status update) | Logs must exist during Phase 9 verification (PHASE-9.md:78-79 uses verify-logs.py). Finalizer runs after all ACs pass, so cleanup is safe. Manual-only violates Philosophy ("SSOT: lifecycle managed within workflow") |
| Bulk cleanup implementation | A: Bash one-liner, B: Python script, C: Manual deletion | B: Python script (`cleanup-stale-ac-logs.py`) | Python provides better error handling, reporting, and consistency with verify-logs.py. Script can read index-features.md programmatically to determine [DONE] status. Bash one-liner is brittle for ~376 files across multiple directories |
| `--scope all` redesign approach | A: Remove `--scope all` entirely, B: Make it lifecycle-aware (filter by active features), C: Deprecate and recommend `--scope feature:{ID}` only | B: Lifecycle-aware filtering | AC#7-8 explicitly require meaningful `--scope all` result. Removal breaks existing CLI contract. Lifecycle awareness aligns with Philosophy ("logs should reflect current reality") |
| Engine TRX scope handling | A: Keep global scan (no scope parameter), B: Add scope parameter for consistency, C: Separate lifecycle cleanup script | B: Add scope parameter to `verify_engine_logs()` | AC#12 requires scope awareness. Engine TRX files are created by /run workflow (headless test runner), so they should follow same lifecycle as AC JSON logs. Separate script violates DRY |
| Non-standard directory cleanup | A: Include in bulk cleanup script, B: Manual one-time deletion, C: Document but defer | A: Include in script with explicit paths | AC#3-6 requires verification. Scripting ensures idempotent execution and clear audit trail. Manual deletion is error-prone for multiple directories |
| KojoComparer TRX disposition | A: Delete without review, B: Read and record before deletion, C: Preserve indefinitely | B: Record disposition in Execution Log | AC#13 + Philosophy ("promises should be fulfilled") requires triage. 2 FAIL TRX files from 2026-02-05 may represent genuine regression or stale snapshots. Disposition must be documented before deletion |
| Finalizer cleanup scope | A: Delete only feature-{ID} dirs, B: Delete all AC types for feature, C: Delete entire ac/ directory | B: `rm -rf ac/*/feature-{ID}` + `rm -f ac/engine/feature-{ID}*.trx` (all AC types + root-level engine TRX) | AC logs use two patterns: `ac/{type}/feature-{ID}/` (subdirectories) and `ac/engine/feature-{ID}-*.trx` (root-level files). Both patterns must be cleaned to prevent accumulation |
| Engine TRX root-level handling | A: Treat root-level TRX as legacy (clean all), B: Always include in scope="all" (no feature filter), C: Parse feature ID from filename pattern | C: Parse feature ID from `feature-{ID}-*.trx` filename pattern; treat non-feature-associated TRX (displaymode-tests.trx, test-result.trx) as project-wide tests included in scope="engine" | Root-level engine TRX files follow mixed naming: `feature-{ID}-*.trx` (feature-associated) and generic names (project-wide). Filtering must handle both. `KojoComparer-*.trx` files are project-wide, dispositioned by AC#13 |

### Interfaces / Data Structures

**New Function: `get_active_features()`**

```python
def get_active_features(repo_root: Path) -> set[str]:
    """Extract active feature IDs from index-features.md.

    Returns:
        Set of feature IDs (numeric strings, e.g., {"217", "268", "787"})
        that are NOT in [DONE] or [CANCELLED] status (includes [DRAFT], [PROPOSED], [REVIEWED], [WIP], [BLOCKED]).
    """
    index_path = repo_root / "Game" / "agents" / "index-features.md"
    active_features = set()

    # Scan all lines between "## Active Features" and "## Recently Completed" headers.
    # NOTE: Active Features section contains MULTIPLE markdown tables separated by
    # ### Phase subsection headers and bold text labels (e.g., "### Phase 19: ...",
    # "**Quality Validation** (F644-F645):"). Extract numeric IDs from ANY table row
    # matching "| {numeric_ID} | [{STATUS}] | ..." regardless of intervening headers.
    # Expected format: "| {numeric_ID} | [{STATUS}] | ... |" (bare numeric IDs without F prefix)
    # Extract {ID} where STATUS is NOT [DONE] and NOT [CANCELLED]
    # (includes [DRAFT], [PROPOSED], [REVIEWED], [WIP], [BLOCKED] — [BLOCKED] features may have partial /run logs)
    #
    # Fallback behavior: if index file is unreadable or zero features parsed,
    # log WARNING to stderr and return None (sentinel).
    # Caller (verify_ac_logs/verify_engine_logs) checks for None:
    #   - If None: print WARN message, skip lifecycle filtering, report WARN status
    #     (not OK, not ERR — indicates lifecycle filtering unavailable)
    # Rationale: verify-logs.py should not hard-fail for index issues, but must NOT
    # silently revert to scanning all logs (that would reproduce the exact broken behavior
    # F787 is designed to fix). WARN status makes the regression visible.
    # The cleanup script can hard-fail since it requires accurate feature data.

    return active_features
```

**Modified Function: `verify_ac_logs()` Integration**

```python
def verify_ac_logs(prod_dir: Path, scope: str = "all"):
    """Modified verify_ac_logs with lifecycle filtering.

    Integration approach: POST-FILTER.
    1. Glob all *-result.json files as before (unchanged discovery)
    2. When scope == "all", filter results by extracting feature ID from
       path components (e.g., ac/build/feature-{ID}/build-result.json → ID)
       and keeping only paths where ID is in active_features set.
    3. When scope == "feature:{ID}", existing logic unchanged (line 144).

    NOTE: main() must pass scope string ("all" or "feature:{ID}") directly
    to this function, replacing the current ac_scope_pattern glob construction.
    """
    ac_dir = prod_dir / "ac"
    active_features = None  # Only needed for scope == "all"
    # Derive repo_root from script location (not prod_dir.parent which is user-configurable via --dir)
    repo_root = Path(__file__).resolve().parent.parent

    if scope == "all":
        active_features = get_active_features(repo_root)
        if active_features is None:
            # Fallback: WARN status, do not revert to scanning all logs
            print("WARN: Cannot determine active features", file=sys.stderr)
            return {"status": "WARN", "passed": 0, "total": 0, "failed_files": []}

    # ... existing glob logic ...
    # POST-FILTER: for each matched result file, extract feature ID from path
    # path pattern: ac/{type}/feature-{ID}/{type}-result.json
    # if scope == "all" and active_features is not None:
    #     keep only results where extracted ID is in active_features
```

**Modified Function: `verify_engine_logs()`**

```python
def verify_engine_logs(prod_dir: Path, scope: str = "all"):
    """Verify Engine test logs (TRX/XML format).

    Args:
        prod_dir: Production logs directory
        scope: Verification scope ("all", "feature:{ID}", "engine")
               When "all", only scan TRX files for active features.
               When "feature:{ID}", scan only that feature's TRX files.
               When "engine", scan all TRX files (no current callers; preserved as explicit opt-in for future direct engine-only verification needs; removes scope if unused after v1 deployment).
    """
    engine_dir = prod_dir / "ac" / "engine"
    if not engine_dir.exists():
        return {"passed": 0, "total": 0, "failed_files": []}

    # TRX files have mixed naming: feature-{ID}/ dirs, feature-{ID}-*.trx root files, generic root files
    # If scope == "all", filter by active features (parse ID from dir names and filename pattern)
    # If scope == "feature:{ID}", filter by feature ID (both dir and filename pattern)
    # If scope == "engine", scan all (no filter) — includes generic TRX (displaymode-tests, test-result)

    # ... rest of TRX parsing logic unchanged

    # NOTE: main() must pass scope string to verify_engine_logs() when
    # check_engine is True: replace verify_engine_logs(prod_dir) with
    # verify_engine_logs(prod_dir, scope). Also update main() to set
    # check_engine=True for scope="feature:{ID}" path (currently False at
    # line 148) so feature-scoped engine TRX verification is wired up.
```

**Bulk Cleanup Script Structure: `src/tools/python/cleanup-stale-ac-logs.py`**

```python
#!/usr/bin/env python3
"""
One-time bulk cleanup of stale AC logs from [DONE] features.

Usage:
    python src/tools/python/cleanup-stale-ac-logs.py [--dry-run]
"""

import argparse
from pathlib import Path

def get_active_features(repo_root: Path) -> set[str]:
    """Reuse same logic as verify-logs.py get_active_features().
    Returns set of non-[DONE]/non-[CANCELLED] feature IDs from index-features.md.
    Inverse approach: delete everything NOT matching active features.
    More reliable than forward-enumerating [DONE] IDs from manually-maintained history."""
    # Parse Active Features table(s) from index-features.md
    # Scan all rows between "## Active Features" and "## Recently Completed"
    # Return set of feature IDs (numeric strings)
    pass

def cleanup_feature_logs(ac_dir: Path, active_features: set[str], dry_run: bool):
    """Delete ac/*/feature-{ID}/ where ID is NOT in active set."""
    # For each AC type subdir (build, code, file, kojo, engine, erb, test)
    # Delete feature-{ID} subdirs where ID is NOT in active_features
    pass

def cleanup_engine_trx(ac_dir: Path, active_features: set[str], dry_run: bool):
    """Delete root-level engine TRX files for non-active features."""
    # Pattern: ac/engine/feature-{ID}-*.trx (root-level, not in subdirs)
    # Parse feature ID from filename pattern feature-{ID}-*.trx
    # Delete where ID is NOT in active_features
    # Also delete generic orphaned TRX (after KojoComparer disposition)
    pass

def cleanup_legacy_dirs(ac_dir: Path, dry_run: bool):
    """Delete non-standard naming directories."""
    # Delete: f266/, f267/, feature-727/, test/
    pass

def main():
    # Parse args (--dry-run flag)
    # Get repo root
    # Read active features (inverse approach)
    # Execute cleanup
    # Report statistics (files deleted, dirs deleted)
    pass
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 13 | Disposition KojoComparer TRX failures before cleanup | | [x] |
| 2 | 1,2,3,4,5,6 | Create and execute bulk cleanup script (cleanup-stale-ac-logs.py) | | [x] |
| 3 | 7,8 | Redesign verify-logs.py --scope all with active_features filtering | | [x] |
| 4 | 12 | Add scope parameter to verify_engine_logs() | | [x] |
| 5 | 9,10,15,16 | Add log cleanup step to finalizer/SKILL.md Step 5 (renumbered) and document lifecycle in PHASE-10.md | | [x] |
| 6 | 11,14 | Verify feature-scoped verification preserved (regression test) | | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1: Read 2 FAIL TRX files from `_out/logs/prod/ac/engine/` (KojoComparer, 2026-02-05), analyze failures, document disposition decision in Execution Log | Disposition record in feature-787.md |
| 2 | implementer | sonnet | T2: Create `src/tools/python/cleanup-stale-ac-logs.py` following Technical Design structure. Use inverse approach: read index-features.md Active Features table for active IDs, delete feature-{ID} subdirs where ID is NOT active + non-standard legacy dirs (f266/, f267/, feature-727/, test/). Report statistics. Execute with --dry-run first, then actual cleanup | Script created, stale logs deleted, statistics reported |
| 3 | implementer | sonnet | T3: Add `get_active_features()` to verify-logs.py (parse index-features.md for non-[DONE]/non-[CANCELLED] IDs). Modify `verify_ac_logs()` to filter by active features when scope="all". Preserve `scope.startswith("feature:")` logic unchanged | verify-logs.py redesigned, --scope all lifecycle-aware |
| 4 | implementer | sonnet | T4: Modify `verify_engine_logs(prod_dir: Path)` signature to add `scope: str = "all"` parameter. Add scope filtering logic: parse feature ID from dir names (`feature-{ID}/`) and root-level filenames (`feature-{ID}-*.trx`). When scope="all", filter by active features; scope="feature:{ID}" filters by ID; scope="engine" scans all including generic TRX | verify_engine_logs() scope-aware with mixed data-shape handling |
| 5 | implementer | sonnet | T5: Update `.claude/skills/finalizer/SKILL.md` — insert new Step 5 after Step 4 "Unblock Dependent Features" (renumbering current Steps 5→6, 6→7, 7→8): Add cleanup commands: `rm -rf _out/logs/prod/ac/*/feature-{ID}` + `rm -f _out/logs/prod/ac/engine/feature-{ID}*.trx` with comment "# Remove feature-scoped AC logs on [DONE] transition (log cleanup)". Only execute on [DONE], not [BLOCKED]/[CANCELLED]. Also update Step 2B skip instruction from "Skip Steps 3-6 → Go to Step 7" to "Skip Steps 3-7 → Go to Step 8" after renumbering. Also update `.claude/skills/run-workflow/PHASE-10.md` to document log cleanup lifecycle, referencing finalizer Step 5 (renumbered) cleanup mechanism and retention policy | Finalizer cleanup step added, Step 2B updated, PHASE-10.md documentation updated |
| 6 | ac-tester | haiku | T6: Run `python src/tools/python/verify-logs.py --scope feature:787` to confirm feature-scoped mode still works after all changes. Expected: exit code 0 or meaningful feature-scoped result (not affected by redesign) | Feature-scoped verification confirmed functional |

### Pre-conditions

- F786 is [DONE] (predecessor)
- Feature 787 status is [REVIEWED] (FL passed)
- Current branch is clean (git status shows no uncommitted changes in workflow files)

### Execution Order

1. **Phase 1 MUST execute first** — KojoComparer disposition before bulk cleanup deletes evidence
2. **Phase 3 MUST execute before Phase 4** — both modify verify-logs.py; Phase 3 adds `get_active_features()` which Phase 4 references. After Phase 3, run `python src/tools/python/verify-logs.py --scope all` and `--scope feature:787` to validate get_active_features() before Phase 4 extends usage to engine logs
3. **Phase 2 MUST execute after Phase 4** — bulk cleanup should use redesigned verify-logs.py for post-cleanup verification; cleanup script's `get_active_features()` is independently implemented but verification relies on redesigned tool
4. **Phase 5 executes after Phase 4** — finalizer/SKILL.md and PHASE-10.md updates
5. **Phase 6 MUST execute last** — regression test after all changes applied

### Build Verification Steps

After Phase 3 (get_active_features + --scope all redesign):

```bash
python src/tools/python/verify-logs.py --scope all
# Expected: exit code 0 or WARN (validates get_active_features())

python src/tools/python/verify-logs.py --scope feature:787
# Expected: exit code 0 (regression test before Phase 4 modifies verify_engine_logs)
```

After Phase 4 (verify_engine_logs scope-aware):

```bash
python src/tools/python/verify-logs.py --scope all
# Expected: exit code 0 (AC#8)

python src/tools/python/verify-logs.py --scope feature:787
# Expected: exit code 0 or meaningful result (AC#11 regression test)
```

After Phase 2 (bulk cleanup):

```bash
find _out/logs/prod/ac -name "*-result.json" | wc -l
# Expected: 0 or very low count (only active feature logs remain)

ls _out/logs/prod/ac/f266 2>/dev/null
# Expected: "No such file or directory" (AC#3)
```

After Phase 5 (finalizer update):

```bash
grep -c "logs/prod/ac" .claude/skills/finalizer/SKILL.md
# Expected: ≥1 (AC#9)
```

### Success Criteria

- All 16 ACs marked `[x]` PASS
- `verify-logs.py --scope all` returns exit code 0
- Zero stale log files remain in `_out/logs/prod/ac/`
- Finalizer includes cleanup step at Step 5 (renumbered)
- Feature-scoped verification (`--scope feature:{ID}`) continues to work unchanged

### Error Handling

| Error | Action |
|-------|--------|
| KojoComparer TRX files not found | Check `_out/logs/prod/ac/engine/` path, adjust glob pattern if needed |
| cleanup-stale-ac-logs.py fails to parse index files | STOP → Report to user (index format may have changed) |
| verify-logs.py --scope all still returns errors after redesign | Investigate active_features filtering logic, verify non-[DONE]/non-[CANCELLED] extraction |
| Feature-scoped mode breaks after redesign | REVERT changes to `scope.startswith("feature:")` code path (line 144), preserve original logic |
| Finalizer cleanup step conflicts with existing numbering | Renumber subsequent steps (Step 5 (renumbered) becomes new insertion point after Step 4 Unblock Dependent Features) |

### Rollback Plan

If issues arise after deployment:

1. **verify-logs.py changes**: Revert commit to restore original `--scope all` behavior
2. **Finalizer cleanup step**: Comment out Step 5 (renumbered) in finalizer/SKILL.md (logs will accumulate but no data loss)
3. **Bulk cleanup**: Cannot rollback deleted logs (gitignored files). If needed, re-run features to regenerate AC logs
4. **Notify user**: Report rollback reason and create follow-up feature for fix

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| complete-feature.md Step 7 stale cleanup path (uEmuera/logs/*.trx) | Out of scope for F787 (C9 identified but deprioritized); /complete-feature is legacy command superseded by /run workflow | Feature | F782 | N/A (Option B — F782 exists as [DRAFT]) |
| verify_engine_logs() scope='engine' parameter removal evaluation | Speculative extensibility with no current callers; evaluate usage after F787 deployment and remove if unused | Feature | F782 | N/A (Option B — F782 exists as [DRAFT]) |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-13 08:12 | DISPOSITION | implementer | Task#1 KojoComparer TRX | DISPOSITION-DECISION: KojoComparer — Both TRX files (KojoComparer-result.trx, KojoComparer-full-result.trx) dated 2026-02-05 12:50-12:51 report identical failure: DiscoverTestCases_WithRealFiles_ReturnsTestCases failed with "Assert.All() Failure: 650 out of 650 items in the collection did not pass" (COM ID 300 validation). Re-ran the test (2026-02-13 08:12) and it now PASSES (1 passed, 0 failed). Multiple KojoComparer commits occurred on 2026-02-05 after the TRX timestamp (2bf29a81, 3ccdf0d1, f4445596, 5861fd97, 180f3580), fixing the test. The TRX files are stale snapshots, not genuine regressions. Safe to delete during bulk cleanup. |
| 2026-02-13 08:15 | END | implementer | Task#3 verify-logs.py redesign | SUCCESS — Added get_active_features() function, modified verify_ac_logs() to accept scope string and filter by active features when scope="all", updated main() to pass scope directly. Verified: --scope all returns "Filtering by active features: 11 active" output, --scope feature:787 still works unchanged, scope.startswith("feature:") pattern preserved. |
| 2026-02-13 08:17 | DEVIATION | orchestrator | Build Verification (post-Task#3) | verify-logs.py --scope all exit=1 — Engine TRX KojoComparer failures still reported because verify_engine_logs() not yet scope-aware (Task#4 pending). AC filtering works correctly (AC:OK:0/0). Expected intermediate state. |
| 2026-02-13 08:18 | END | implementer | Task#4 verify_engine_logs scope | SUCCESS — Modified verify_engine_logs() signature to add scope parameter (default "all"). Added scope filtering logic: when scope="all", calls get_active_features() and filters TRX files by feature ID (extracted from subdirectory names and root-level filename patterns feature-{ID}-*.trx). Non-feature-associated root-level TRX files (displaymode-tests.trx, test-result.trx) are excluded from scope="all" (only included in scope="engine"). When scope="feature:{ID}", filters to only that feature's TRX files. Also updated main() to pass scope to verify_engine_logs() and set check_engine=True for feature-scoped mode. Verified: --scope all returns OK:0/0 with "Filtering engine logs by active features: 11 active", --scope feature:787 returns OK:0/0. Grep confirms function signature matches "verify_engine_logs.*scope" pattern (AC#12). |
| 2026-02-13 08:21 | END | implementer | Task#2 bulk cleanup script | SUCCESS — Created src/tools/python/cleanup-stale-ac-logs.py following Technical Design structure with inverse approach: reads index-features.md Active Features table to extract active feature IDs (11 active: 774-787), deletes feature-{ID} subdirectories where ID is NOT in active set + non-standard legacy directories (f266/, f267/, feature-727/, test/) + orphaned engine TRX files. Executed with --dry-run first (output contains "non-active feature" per AC#2), then actual cleanup. Results: 238 directories deleted, 10 TRX files deleted, 0 active feature dirs/files skipped. Verified: AC#3-6 legacy directories removed (f266, f267, feature-727, test all return "not found" as expected). |
| 2026-02-13 08:24 | END | implementer | Task#5 finalizer/PHASE-10 docs | SUCCESS — Updated .claude/skills/finalizer/SKILL.md: inserted new Step 5 "AC Log Cleanup (log cleanup)" after Step 4 "Unblock Dependent Features" (line 145), renumbered subsequent steps (Step 5→6, 6→7, 7→8), updated Step 2B skip instruction from "Skip Steps 3-6 → Go to Step 7" to "Skip Steps 3-7 → Go to Step 8" (line 65). Step 5 includes cleanup commands (`rm -rf _out/logs/prod/ac/*/feature-{ID}` + `rm -f _out/logs/prod/ac/engine/feature-{ID}*.trx`), [DONE]-only guard ("Only execute on [DONE] transition. If status is [BLOCKED] or [CANCELLED], skip this step"), non-blocking error handling, and cleanup result reporting requirement in Final Report. Also updated .claude/skills/run-workflow/PHASE-10.md Step 10.2: added Log Lifecycle Note documenting automatic cleanup during [DONE] transition, referencing finalizer Step 5. Verified: Grep confirms "logs/prod/ac" (AC#9), "log cleanup" (AC#10), "DONE.*log" pattern (AC#15), "Skip Steps 3-7.*Step 8" (AC#16). |
| 2026-02-13 08:26 | END | ac-tester | Task#6 regression test (AC#11, AC#14) | SUCCESS -- AC#11: Grep pattern "scope.*startswith.*feature" found in src/tools/python/verify-logs.py (3 occurrences), confirming feature-scoped verification logic preserved. AC#14: python src/tools/python/verify-logs.py --scope feature:787 executed with exit code 0 (success), output shows OK:0/0. Feature-scoped mode remains fully functional after all redesign changes. Regression test PASSED. |
| 2026-02-13 08:28 | DEVIATION | ac-static-verifier | AC#15 file verification | FAIL — Pattern `DONE.*cleanup\|DONE.*log` not matched. Pipe `\|` treated as literal by verifier, not regex alternation. Actual text: line 147 "Only execute on [DONE] transition" + line 149 "[DONE] transition (log cleanup)" contains both DONE+cleanup and DONE+log. Guard exists but regex matching failed due to escaped pipe. |
| 2026-02-13 08:32 | FIX | debugger | AC#15 regex pattern | Updated AC#15 Expected from `"DONE.*cleanup\|DONE.*log"` to `"DONE.*log cleanup"` to avoid alternation operator. Pattern now matches line 149 exactly. Also reverted AC#14, AC#15 status from [-] to [ ] (reset for re-verification), and Tasks#5-6 from [-] to [x] (tasks were completed, AC failure was test definition defect). |
| 2026-02-13 08:35 | DEVIATION | feature-reviewer | Phase 8.1 Quality Review | NEEDS_REVISION — 2 major issues: (1) TRX glob duplication bug in verify_engine_logs() — `engine_dir.glob("*.trx") + engine_dir.glob("**/*.trx")` double-counts root-level files. (2) DRY violation — duplicated feature-ID extraction logic between scope="all" and scope.startswith("feature:") blocks. |
| 2026-02-13 08:37 | FIX | debugger | Debug Iteration 2 | SUCCESS — Fixed both issues: (1) TRX glob: changed line 161 from `list(engine_dir.glob("*.trx")) + list(engine_dir.glob("**/*.trx"))` to `list(engine_dir.glob("**/*.trx"))` (covers both root-level and subdirectories without duplication). (2) DRY violation: extracted helper function `extract_feature_id(trx_file: Path) -> str \| None` (lines 138-153) that returns feature ID from directory name or filename pattern. Refactored both scope="all" and scope.startswith("feature:") blocks to call this helper (lines 186-190, 195-199), eliminating 44 lines of duplicated logic. Verified: `python src/tools/python/verify-logs.py --scope all` returns OK:12/12, `--scope feature:787` returns OK:12/12. Grep confirmed all AC-relevant patterns preserved: active_features (22 occurrences), scope.*startswith.*feature (3 occurrences), verify_engine_logs.*scope (2 occurrences). |
| 2026-02-13 08:40 | DEVIATION | feature-reviewer | Phase 8.2 Doc Consistency | NEEDS_REVISION — testing/SKILL.md Log Verification section (lines 263-272) documents verify-logs.py behavior that changed in F787 but was not updated. --scope all now filters by active features, engine TRX scanning is scope-aware. |
| 2026-02-13 08:41 | FIX | orchestrator | testing/SKILL.md update | Updated Log Verification section: added --scope parameter documentation, scope behavior table, updated glob patterns to reflect scope-filtered behavior. |

---

## Review Notes

<!-- FL persist_pending entries will be recorded here -->
- [resolved-applied] Phase2-Pending iter1: AC#2 (line 314) — Replaced not_exists with cleanup script --dry-run output verification (POST-LOOP user decision: option A)
- [resolved-applied] Phase2-Uncertain iter1: AC#1 (line 309) — Replaced Glob not_exists with cleanup script exit_code succeeds (POST-LOOP user decision: option A)
- [resolved-skipped] Phase2-Uncertain iter1: Review Context template ownership — /run Phase 9 origin doesn't perfectly fit either template variant; current Review Context format is acceptable
- [fix] Phase2-Review iter1: Risks table | Added risk row for text-based verification durability of AC#9/AC#10
- [fix] Phase2-Review iter1: Origin table | Merged 'Source' field into 'Discovery Point' (4→3 fields per template)
- [fix] Phase2-Review iter1: Tasks table Task#1 | Removed [I] tag (AC#13 Expected is deterministic)
- [resolved-applied] Phase2-Pending iter2: AC#1 (line 309) — Glob pattern replaced with cleanup script exit_code (POST-LOOP user decision: option A)
- [resolved-skipped] Phase2-Uncertain iter2: Review Notes category codes — No explicit MUST requirement in template; entries remain readable without codes
- [fix] Phase2-Review iter2: Execution Order line 568 | Changed Phase 3-4 from parallel to sequential (concurrent Edit on same file risk)
- [resolved-applied] Phase2-Pending iter3: AC#8 (line 298) — Replaced exit_code succeeds with output contains "active features" (POST-LOOP user decision: option A)
- [fix] Phase2-Review iter3: AC#3 → AC#3-6 | Split single AC row into 4 separate rows, one per legacy directory (Glob single-level wildcard only tests one path)
- [fix] Phase2-Review iter3: Task#6 merged into Task#5 | Removed standalone Task#6 (PHASE-10 docs) — SSOT confusion with dual documentation, AC#10 satisfied by Task#5 alone. Task#7→Task#6 renumbered. Implementation Contract Phases 5-6 merged.
- [resolved-applied] Phase2-Pending iter4: AC#13 — Changed to unique marker "DISPOSITION-DECISION: KojoComparer" (POST-LOOP user decision: option A)
- [fix] Phase2-Review iter4: Key Decisions table | Added Engine TRX root-level handling decision; updated Finalizer cleanup scope to include root-level TRX pattern (`rm -f ac/engine/feature-{ID}*.trx`)
- [fix] Phase2-Review iter4: Technical Design | Updated finalizer cleanup commands, verify_engine_logs interface to handle mixed data shapes (dirs + root-level TRX + generic TRX), Implementation Contract Phases 4-5 updated
- [fix] Phase3-Maintainability iter1: Technical Design line 386 | Changed cleanup script from "one-time" to "idempotent, reusable" with explicit re-runnable design
- [fix] Phase3-Maintainability iter1: Technical Design get_active_features() | Added format validation requirement (raise ValueError if zero features parsed from non-empty file)
- [fix] Phase3-Maintainability iter1: Technical Design + references | Replaced fractional "Step 3.5" with proper "Step 5 (renumbered)" throughout feature file (7 occurrences)
- [fix] Phase3-Maintainability iter1: Technical Design verify_engine_logs | Clarified scope="engine" as backward-compatibility preservation
- [fix] Phase2-Review iter2: AC Definition Table | Renumbered AC#3a-3d to AC#3-6, shifted subsequent ACs by +3 (old 4→7, 5→8, ..., 11→14). Updated all cross-references in Philosophy Derivation, Goal Coverage, Tasks, AC Coverage, Key Decisions, Build Verification
- [resolved-skipped] Phase2-Uncertain iter2: Technical Design cleanup placement — Finalizer retained per Philosophy and Technical Design (POST-LOOP user decision: option A)
- [fix] Phase2-Review iter3: Success Criteria + Approach | Changed "All 10 ACs" to "All 14 ACs" (2 occurrences, stale count from before renumbering)
- [fix] Phase2-Review iter3: AC Coverage table | Added missing AC#14 row to Technical Design AC Coverage
- [fix] Phase2-Review iter3: Philosophy Derivation | Added AC#14 to "promises should be fulfilled" → AC#11, AC#14
- [fix] Phase2-Review iter3: Mandatory Handoffs | Added C9 (complete-feature.md stale cleanup path) with destination F782
- [resolved-applied] Phase2-Uncertain iter3: No functional AC for lifecycle filtering correctness — AC#8 now verifies output contains "active features" (behavioral check); AC#2 verifies cleanup script --dry-run identifies stale logs (POST-LOOP user decision: option A)
- [resolved-skipped] Phase2-Uncertain iter3: AC#9/AC#10 text-presence-only verification — Accepted risk; behavioral verification infeasible for workflow skill files
- [fix] Phase2-Review iter4: Success Criteria line 622 | Fixed remaining "All 10 ACs" → "All 14 ACs" (case-sensitive miss from iter3)
- [fix] Phase2-Review iter4: get_active_features() comment | Changed F{ID} to {numeric_ID} (bare IDs without F prefix, matching actual index-features.md format)
- [fix] Phase2-Review iter4: get_active_features() status filter | Changed from whitelist [WIP,REVIEWED,PROPOSED] to exclusion [not DONE, not CANCELLED] — includes [BLOCKED] (may have partial /run logs). Updated 4 occurrences across Technical Design, AC Coverage, Implementation Contract, Error Handling
- [resolved-applied] Phase2-Uncertain iter4: Parallel parsers — resolved by switching cleanup script from get_done_features() (forward enumeration) to inverse approach reusing get_active_features() logic. Single parsing pattern, no more parallel parsers.
- [fix] Phase2-Review iter5: Cleanup script | Replaced get_done_features() forward enumeration with inverse approach: get_active_features() + delete non-active. More reliable than manually-maintained history file. Updated Approach, script structure, Implementation Contract Phase 2.
- [fix] Phase2-Review iter5: get_active_features() spec | Added multi-table structure note — index-features.md has 5 separate tables under Active Features with Phase subsection headers. Spec now says "scan ALL rows between headers".
- [fix] Phase2-Review iter6: Impact Analysis | Downgraded testing/SKILL.md and test-strategy.md from LOW to NONE — neither file actually needs updating (SKILL.md doesn't reference --scope all; test-strategy.md syntax preserved)
- [fix] Phase3-Maintainability iter7: get_active_features() error handling | Changed from hard-fail ValueError to fallback (return empty set, scan all logs). verify-logs.py should not fail for index-features.md issues. Cleanup script can hard-fail since it requires accurate data.
- [resolved-skipped] Phase1-RefCheck iter1: F644 in Links — Confirmed false positive; F644 appears only in code comment as example of index-features.md format
- [resolved-skipped] Phase3-Maintainability iter1: get_active_features() duplication — Implementation decision; design explicitly accepts duplication for one-time migration tool
- [resolved-applied] Phase3-Maintainability iter1: 5 unresolved [pending] items — Being resolved in POST-LOOP Step 2 as planned
- [fix] Phase3-Maintainability iter1: Technical Design line 401 | Clarified cleanup script as one-time migration tool; archive to src/tools/dotnet/_archived/ after execution
- [fix] Phase3-Maintainability iter1: Technical Design line 409 | Changed finalizer insertion from Step 4 to Step 5 (after CRITICAL "Unblock Dependent Features"); added non-blocking error handling requirement
- [fix] Phase3-Maintainability iter1: Technical Design verify_engine_logs scope='engine' | Clarified as no-current-callers opt-in; remove if unused after v1
- [fix] Phase2-Review iter2: Mandatory Handoffs line 663 | Changed "Existing Feature" to "Feature", simplified Destination ID from "F782 (Post-Phase Review)" to "F782"
- [fix] Phase2-Review iter1: Mandatory Handoffs line 663 | Changed Creation Task from prose instruction to "N/A (Option B — F782 exists as [DRAFT])" per template validation rules
- [fix] Phase2-Review iter2: Impact Analysis lines 149-150 | Changed "NONE" to "LOW" per template enum (HIGH/MEDIUM/LOW)
- [fix] Phase2-Review iter2: Execution Order line 598 | Changed "after Phase 3-4" to "after Phase 4" to remove parallelism ambiguity
- [fix] Phase2-Review iter2: Implementation Contract Phase 5 + T5 description | Added Step 2B skip instruction update to T5 scope (renumber "Skip Steps 3-6 → Go to Step 7" to "Skip Steps 3-7 → Go to Step 8")
- [resolved-skipped] Phase2-Uncertain iter2: Philosophy Derivation semantic mapping — Both arrangements defensible; current mapping is acceptable
- [resolved-skipped] Phase2-Uncertain iter2: Task-AC alignment AC#11 — T6 regression test role is correct; separation of implementation and regression ACs is intentional
- [resolved-applied] Phase2-Pending iter3: Implementation Contract Phase 5 — Aligned to "after Step 4 Unblock" per Technical Design (POST-LOOP auto-fix)
- [fix] Phase2-Review iter3: Technical Design get_active_features() fallback | Changed from silent empty-set fallback to None sentinel with WARN status. Prevents silent regression to broken behavior.
- [resolved-skipped] Phase2-Uncertain iter3: Mandatory Handoffs F782 — Option B validation passes (file exists); #T{N} not required by template for research-type DRAFTs
- [resolved-applied] Phase2-Pending iter4: Cleanup script engine TRX — Added cleanup_engine_trx() function for root-level feature-{ID}-*.trx files (POST-LOOP auto-fix)
- [resolved-skipped] Phase3-Maintainability iter4: Cleanup script archive policy — Archive-after-execution retained: finalizer Step 5 prevention mechanism handles ongoing feature-{ID} cleanup; non-standard naming (f266, f267, feature-727, test) is legacy-only and will not recur under current workflow; truly novel debris patterns require investigation as a new Feature, not blind automated cleanup
- [resolved-skipped] Phase3-Maintainability iter4: get_active_features() format contract — Loop issue; None sentinel + WARN status already handles format deviations
- [fix] Phase3-Maintainability iter4: Execution Order + Build Verification | Added intermediate verification step between Phase 3 and Phase 4 to validate get_active_features() before Phase 4 extends usage to engine logs
- [resolved-applied] Phase2-Pending iter5: Execution Order — Phase 2 now explicitly "MUST execute after Phase 4" (POST-LOOP auto-fix)
- [fix] Phase2-Review iter5: Technical Design line 412 | Added explicit [DONE] status guard requirement for finalizer cleanup step — [CANCELLED] path shares Steps 3-7 and would execute cleanup without guard
- [resolved-applied] Phase2-Pending iter6: No AC for [DONE]-only guard — Added AC#15 with Grep for "DONE.*cleanup|DONE.*log" pattern (POST-LOOP user decision: option A)
- [fix] Phase2-Review iter6: Philosophy Derivation line 284 | Added AC#9, AC#10 to "comprehensive and continuously enforced" mapping — "continuously enforced" aspect was missing from AC Coverage
- [fix] Phase2-Review iter6: Goal text line 90 + Goal Coverage | Added "(4) disposition KojoComparer TRX failures before deletion" to Goal text — Goal item 8 was derived from Risk/Constraint, not stated in Goal
- [fix] Phase3-Maintainability iter1: Mandatory Handoffs | Added scope='engine' removal evaluation as handoff to F782
- [resolved-skipped] Phase2-Uncertain iter2: Review Context Review Evidence sub-section — Semantically inapplicable to /run Phase 9 origin; overlaps with Review Context template ownership pending
- [fix] Phase2-Review iter2: Review Context | Merged "Deep Explorer Findings Summary" content into "Identified Gap" (removed non-template sub-section heading)
- [fix] Phase2-Review iter2: Review Context | Reordered sub-sections to match template: Files Involved before Parent Review Observations
- [fix] Phase2-Review iter2: Philosophy line 80 | Qualified "not by external ad-hoc cleanup" to allow one-time migration scripts for initial state remediation
- [resolved-applied] Phase2-Pending iter2: WARN exit code unspecified — AC#8 no longer uses exit_code matcher; output verification avoids WARN exit code ambiguity (POST-LOOP user decision: option A)
- [fix] Phase2-Review iter2: Technical Design line 409 | Added cleanup result reporting requirement to Finalizer Final Report output (success/warning/skipped)
- [fix] Phase2-Review iter3: Technical Design Interfaces | Added verify_ac_logs() integration design — POST-FILTER approach with pseudocode for active_features set integration with glob-based architecture
- [resolved-skipped] Phase2-Pending iter3: AC#12 signature-only verification — Accepted: AC#7 verifies active_features code presence, AC#8 verifies end-to-end --scope all behavioral output, Implementation Contract Phase 4 explicitly requires filtering logic. Code-grep for engine-specific filtering body would be fragile and implementation-dependent. Risk row (line 175) already acknowledges text-based verification limits
- [fix] Phase2-Review iter4: Technical Constraints | Added directory structure invariant — AC result files must be nested under feature-{ID}/ subdirectories for POST-FILTER path extraction to work
- [resolved-applied] Phase2-Uncertain iter4: Defense-in-depth AC gap — AC#8 now verifies output references "active features", confirming filtering is operational regardless of log state (POST-LOOP user decision: option A)
- [fix] Phase2-Review iter5: verify_ac_logs() pseudocode | Fixed WARN return dict to include "failed_files": [] (format_result() requires this key)
- [fix] Phase2-Review iter5: verify_ac_logs() pseudocode | Added main() integration note (pass scope string directly, replace ac_scope_pattern glob construction)
- [fix] Phase2-Review iter5: verify_ac_logs() pseudocode | Changed repo_root derivation from prod_dir.parent to Path(__file__) (prod_dir is user-configurable via --dir)
- [fix] Phase2-Review iter6: verify_engine_logs() pseudocode | Added main() integration note — main() must pass scope to verify_engine_logs() and set check_engine=True for feature:{ID} path
- [fix] Phase2-Review iter6: Goal text line 88 | Added "(5) make engine TRX logs scope-aware" — Goal Coverage item 7 had no corresponding Goal text
- [fix] PostLoop-UserFix: AC#1/AC#2 | Replaced not_exists Glob matchers with cleanup script exit_code/output verification (user decision: option A)
- [resolved-applied] Phase2-Uncertain iter1: AC#2/AC#8 use 'output' type but C5 prohibits it — resolved by updating C5 to allow output type for script stdout verification (POST-LOOP user decision: option A)
- [fix] Phase2-Review iter1: AC#8 Method/Details | Removed --dry-run from AC#8 Method (verify-logs.py has no --dry-run flag; only cleanup script does)
- [fix] Phase2-Review iter1: AC Coverage table | Added missing AC#15 row to Technical Design AC Coverage
- [fix] Phase2-Review iter2: AC Coverage row 8 | Updated stale description from "exit code 0, Bash succeeds" to "output contains active features" matching AC Definition Table
- [fix] Phase2-Review iter2: AC Coverage row 9 | Changed "between status update and unblocking" to "after Step 4 Unblock Dependent Features" matching Technical Design
- [fix] Phase2-Review iter2: Philosophy Derivation | Added AC#15 to SSOT lifecycle management row (AC#9, AC#10 → AC#9, AC#10, AC#15)
- [fix] Phase2-Review iter3: Approach + Success Criteria | Changed "all 14 ACs" to "all 16 ACs" (2 occurrences, stale after AC#15 addition)
- [fix] Phase2-Review iter4: Success Criteria line 702 | Fixed "All 14 ACs" → "All 16 ACs" (case-sensitivity miss from iter3 replace_all)
- [fix] Phase2-Review iter4: AC Coverage rows 1-2 | Updated stale Glob-based descriptions to match current AC#1 (exit_code/succeeds) and AC#2 (output/contains "non-active feature")
- [fix] Phase2-Review iter4: Error Handling line 716 | Changed "between status update and unblocking" to "after Step 4 Unblock Dependent Features"
- [resolved-skipped] Phase3-Maintainability iter5: get_active_features() duplication — loop repeat of resolved-skipped iter1
- [resolved-skipped] Phase3-Maintainability iter5: AC#9/AC#10/AC#15 text-only verification — loop repeat of resolved-skipped iter3
- [resolved-skipped] Phase3-Maintainability iter5: Goal Coverage item 4 weak verification — loop repeat of 3x user decisions iter3/iter4
- [resolved-skipped] Phase3-Maintainability iter5: Generic TRX handling in scope='all' — loop repeat; design decision in Key Decisions
- [resolved-skipped] Phase3-Maintainability iter5: Execution Order Phase 2 dependency — loop repeat of resolved-applied iter5
- [fix] Phase3-Maintainability iter5: AC Definition Table | Added AC#16 for Step 2B skip instruction verification after renumbering
- [fix] Phase3-Maintainability iter5: Technical Design | Added agent instruction note for cleanup commands (expand {ID} at runtime)
- [fix] Phase3-Maintainability iter5: AC count + cascading updates | Updated count 15→16, Goal Coverage, Tasks T5, Philosophy Derivation, AC Coverage table
- [resolved-skipped] Phase4-ACValidation iter6: AC#2 output type vs C5 — loop repeat of pending Phase2-Uncertain iter1 (consolidated)
- [resolved-skipped] Phase4-ACValidation iter6: AC#8 output type vs C5 — loop repeat of pending Phase2-Uncertain iter1 (consolidated)
- [fix] PostLoop-UserFix: AC Design Constraint C5 | Updated to allow 'output' type for script stdout verification (user decision: option A)

---

<!-- fc-phase-6-completed -->

## Links

[Predecessor: F786](feature-786.md) - Test Infrastructure Transition Obligations (discovery source)
[Related: F784](feature-784.md) - Test Infrastructure Remediation (CI revival)
[Related: F785](feature-785.md) - Regression Test Recovery (cancelled, superseded by F786)
[Related: F782](feature-782.md) - Post-Phase Review Phase 20
[Related: F205](archive/feature-205.md) - Origin of verify-logs.py
[Related: F268](archive/feature-268.md) - Origin of ac-static-verifier.py
[Related: F499](archive/feature-499.md) - Origin of test-strategy.md
