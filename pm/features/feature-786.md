# Feature 786: Test Infrastructure Transition Obligations

## Status: [DONE]
<!-- fl-reviewed: 2026-02-12T00:00:00Z -->

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
<!-- Written by FL POST-LOOP Step 6.3. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F784 |
| Discovery Point | Philosophy Gate (POST-LOOP Step 6.3) |
| Source | F784 FL Review — Transition ACs leak (phase-5-19-content-migration.md:2601-2608) |
| Timestamp | 2026-02-12 |

### Identified Gap

The Test Infrastructure Transition section in `phase-5-19-content-migration.md` defines 7 ACs (N+1 through N+7) that were orphaned when Phase renumbering broke the trigger mechanism. F784 fixes the Phase number references but does not execute the actual obligations. Of the 7:

- N+1 (pre-commit scope expansion) → Covered by F784 AC#6
- N+6 (regression tests archived) → F785 [CANCELLED] — ERB-based flow test revival is unnecessary

The remaining 5 obligations have no Feature assignment:

| AC | Description | Obligation |
|:--:|-------------|------------|
| N+2 | pre-commit removes ErbLinter | Remove ErbLinter references from pre-commit hook |
| N+3 | do.md Phase 3 uses dotnet test | Update do.md to use dotnet test command |
| N+4 | testing SKILL removes --unit/--flow | Remove deprecated --unit/--flow flags from testing SKILL |
| N+5 | verify-logs.py removes regression | Remove regression log verification from verify-logs.py |
| N+7 | do.md outputs TRX to logs | Add TRX output configuration to do.md |

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | phase-5-19-content-migration.md:2601-2608 |
| Derived Task | Execute or disposition orphaned Transition obligations |
| Comparison Result | 5 of 7 obligations have no Feature assignment |
| DEFER Reason | N/A (direct execution) |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/verify-logs.py | N+5 dead code removal target |
| .githooks/pre-commit | N+2 verification target |
| docs/architecture/phases/phase-5-19-content-migration.md | Obligation source + annotation target |
| _out/logs/prod/regression/ | Stale log cleanup target |
| .claude/skills/testing/SKILL.md | N+4 deferred (--unit/--flow references) |

### Parent Review Observations

**F784 Completion Summary**:
- CI revival: GitHub Actions .NET 10 SDK + actions v4 + erakoumakanNTR.sln (18 projects)
- Pre-commit expansion: engine.Tests added (56%→79% coverage, 5-step configuration)
- Phase reference fix: Phase 15→19, Phase 16→20 in Transition section
- Structural fix: Added cross-phase trigger verification to Post-Phase Review

**F785 Cancellation Reason**:
- ERB-based flow test (24 scenarios) revival is unnecessary
- Project is in ERB→C# migration; regression tests should be C#/YAML-based
- N+6 (regression tests archived) obligation was 'ERB flow test state management' but policy change requires redefinition as C#/YAML regression tests

**Current Obligation Assessment** (confirmed in F784 /run Phase 9):

| AC | Current State | Assessment |
|:--:|------|------|
| N+2 | No ErbLinter references in pre-commit (already removed) | Verification only or unnecessary |
| N+3 | `do.md` replaced by `/run`. Need to verify target file existence | Target may not exist |
| N+4 | `--unit` is **currently in use** for kojo AC testing. `--flow` is archived | **Premature** — removing --unit breaks kojo verification |
| N+5 | `verify-logs.py` still contains regression-related code | Executable |
| N+7 | `do.md` → `/run` already replaced. TRX output handled by `/run` Phase 7 | Target may not exist |

**`/fc` Investigation Points**:
1. Verify whether target files for each obligation still exist
2. Determine whether N+4 (--unit/--flow removal) should be rescheduled after C# migration completion
3. Decide whether to redefine N+6 (regression) as C#/YAML regression tests or include in scope
4. Convert only executable obligations to ACs (record obligations with absent targets as 'completed')

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

"Automated test coverage should be comprehensive and continuously enforced. Tests that exist should run; promises should be fulfilled; stale references should not mislead." (Inherited from F784.) The SSOT for test infrastructure obligations is `phase-5-19-content-migration.md` Transition section; every obligation must be either executed or dispositioned with a concrete destination — no orphaned promises.

### Problem (Current Issue)

Five Test Infrastructure Transition obligations (N+2, N+3, N+4, N+5, N+7) remain orphaned because the obligation definitions were frozen as static ACs targeting specific files and conditions at a point in time, but the codebase evolved independently: `do.md` was replaced by `/run` command (invalidating N+3 and N+7), ErbLinter was already removed from pre-commit (satisfying N+2 organically), `--unit` became foundational to kojo AC testing (making N+4 premature), and `verify-logs.py` still contains dead regression code (N+5 actionable). The obligations lack per-item status tracking, so satisfied and obsolete items appear identical to genuinely pending work.

### Goal (What to Achieve)

Close all 5 orphaned obligations by executing the one actionable change (N+5: remove dead regression code from `verify-logs.py` and clean stale regression log files), recording verified dispositions for already-satisfied (N+2) and obsolete (N+3, N+7) obligations, and deferring premature work (N+4) to F782 with a concrete Task entry. Annotate the Transition section in `phase-5-19-content-migration.md` with per-obligation status so no obligation remains ambiguous. Remove stale Regression output documentation from `run-workflow/PHASE-9.md` that references the removed regression function.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are 5 obligations unexecuted? | F784 fixed the Phase number references (15->19, 16->20) but did not execute the obligations themselves | `pm/features/feature-784.md:619` (handoff to F786) |
| 2 | Why did F784 not execute them? | The obligations target specific files and conditions that needed individual investigation to determine current applicability | `docs/architecture/phases/phase-5-19-content-migration.md:2572-2608` |
| 3 | Why do the obligations not match current state? | The codebase evolved independently — do.md was replaced by /run, ErbLinter was already removed, --unit became actively required for kojo testing | `.claude/commands/do.md` absent; `.githooks/pre-commit` has zero ErbLinter references; `.claude/skills/testing/KOJO.md` structured around --unit |
| 4 | Why was there no mechanism to track obligation satisfaction? | The Transition section used a blanket Phase 20 completion trigger with no per-obligation status annotations | `phase-5-19-content-migration.md:2610` — "Phase 20 完了前にこれらの変更を行ってはいけない" |
| 5 | Why (Root)? | Obligations were designed as a monolithic batch gated by Phase 20 completion, but the codebase evolved faster than the gate — some obligations were satisfied organically, some became impossible (target absent), and only one remains directly actionable | `phase-5-19-content-migration.md:2546` (Transition section definition date vs current state) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 5 unexecuted obligations with no Feature assignment | Monolithic obligation batch with static ACs frozen against a dynamic codebase |
| Where | `phase-5-19-content-migration.md` Transition section, lines 2572-2608 | Obligation design model: batch trigger (Phase 20) without per-item lifecycle tracking |
| Fix | Execute all 5 obligations | Disposition each obligation individually: execute (N+5), record satisfied/obsolete (N+2, N+3, N+7), defer with concrete destination (N+4) |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F784 | [DONE] | Predecessor — fixed Phase references, handed off obligation execution |
| F785 | [CANCELLED] | Related — N+6 disposition (ERB flow test revival deemed unnecessary) |
| F647 | [DONE] | Related — Phase 20 Planning |
| F774-F781 | [DRAFT] | Related — Phase 20 sub-features (all unstarted) |
| F782 | [DRAFT] | Related — Post-Phase Review Phase 20 (deferral target for N+4) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| N+2 (ErbLinter removal) | ALREADY_DONE | `.githooks/pre-commit` contains zero ErbLinter references (3/3 agreement) |
| N+3 (do.md uses dotnet test) | OBSOLETE | `.claude/commands/do.md` does not exist; only in `.tmp/recovery/` archives (3/3 agreement) |
| N+4 (testing SKILL removes --unit/--flow) | PREMATURE | `--unit` actively used in 6+ non-archived files: KOJO.md, SKILL.md, PHASE-3.md, PHASE-5.md, engine.Tests, KojoComparer (3/3 agreement) |
| N+5 (verify-logs.py removes regression) | FEASIBLE | `verify-logs.py:60-88` contains dead `verify_regression_logs()` function; `test/regression/` absent; `check_regression = True` always (3/3 agreement) |
| N+7 (do.md outputs TRX) | OBSOLETE | `.claude/commands/do.md` does not exist (3/3 agreement) |
| Stale regression logs | FEASIBLE | `_out/logs/prod/regression/` contains 24 stale JSON result files (2/3 discovery) |
| Obligation annotation | FEASIBLE | `phase-5-19-content-migration.md` Transition section is editable (3/3 agreement) |

**Verdict**: NEEDS_REVISION

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| verify-logs.py | MEDIUM | Remove dead regression code (function, flag, branch, call site); remaining unit/flow verification functions unaffected |
| _out/logs/prod/regression/ | LOW | Delete 24 stale JSON result files that could mislead (report non-zero regression results from absent test scenarios) |
| phase-5-19-content-migration.md | LOW | Add per-obligation status annotations to Transition section — documentation only |
| Testing workflow | NONE | No changes to active --unit/--flow flags; kojo testing unaffected |
| Pre-commit hook | NONE | N+2 already satisfied; no changes needed |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Phase 20 completion gate | `phase-5-19-content-migration.md:2610` | Blanket prohibition "Phase 20 完了前にこれらの変更を行ってはいけない" — spirit does not apply to N+5 dead code or disposition recording, but does apply to N+4 |
| --unit foundational to kojo testing | `.claude/skills/testing/KOJO.md:1-155`, `run-workflow/PHASE-3.md:49`, `PHASE-5.md:29,73` | Cannot remove --unit; 95+ kojo AC test JSON files depend on it |
| --flow in twilight state | `.claude/skills/testing/SKILL.md:3,45,500`, `ENGINE.md:82-93`, `run-workflow/SKILL.md:97`, `erb-syntax/SKILL.md:104` | --flow partially archived (HTML comment) but still in active non-comment lines; removal scope larger than originally defined |
| do.md absent from active commands | Only in `.tmp/recovery/` archives | N+3 and N+7 targets do not exist; cannot execute |
| verify-logs.py always runs regression | `verify-logs.py:165` (`check_regression = True`) | Dead code path: function runs but returns 0/0 for empty directory |
| test/regression/ absent | Directory does not exist | `verify_regression_logs()` operates on non-existent path |
| test/_archived/regression/ | 48 archived files | Archived regression tests exist but are not referenced by verify-logs.py |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Premature --unit removal breaks kojo testing | HIGH (if N+4 executed) | HIGH | Defer N+4 entirely to F782; do NOT execute in F786 |
| verify-logs.py regression removal causes unexpected behavior | LOW | LOW | Function returns 0/0 for empty directory; removal is safe; remaining functions (unit, flow) unaffected |
| Stale regression logs mislead future tooling | MEDIUM | MEDIUM | Include _out/logs/prod/regression/ cleanup in N+5 scope |
| N+4 deferral forgotten after F786 completes | MEDIUM | MEDIUM | Add concrete Task to F782 per Deferred Task Protocol; record in Mandatory Handoffs |
| Phase 20 gate violation perception | LOW | LOW | Document reasoning: N+5 targets dead code unrelated to Phase 20 content; disposition recording is documentation-only |
| --flow references in other skills overlooked during future N+4 execution | MEDIUM | LOW | Document expanded scope in deferral notes: --flow spans SKILL.md, ENGINE.md, run-workflow, erb-syntax, kojo-init.md |

---

## Baseline Measurement
<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| verify-logs.py regression code locations | Grep "(?i)regression" in src/tools/python/verify-logs.py | 8 Grep matches (lines 60, 61, 62, 153, 165, 174, 194, 205) + 5 reg_* variable lines (206-208, 219-221) across 7 functional categories | Function definition (60-88), help text (153), flag (165), scope branch (174), call (194), output formatting (205-208), total aggregation (219-221) |
| Stale regression log files | ls _out/logs/prod/regression/ | 24 JSON files | Orphaned output from archived flow tests |
| Obligation annotations in Transition section | Grep "N\+" in phase-5-19-content-migration.md Transition section | 0 per-obligation status annotations | All obligations appear as undifferentiated pending |

**Baseline File**: `.tmp/baseline-786.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | N+4 MUST NOT be executed — --unit actively used | `.claude/skills/testing/KOJO.md`, `engine.Tests`, `KojoComparer` | No AC may verify --unit removal; N+4 must be deferred to F782 |
| C2 | do.md does not exist in active commands | Glob results (only in `.tmp/recovery/`) | N+3 and N+7 ACs should verify not_exists for do.md, confirming obsolete status |
| C3 | N+2 already satisfied organically | `.githooks/pre-commit` has zero ErbLinter references | AC should verify current state (not_contains ErbLinter), not perform removal |
| C4 | verify-logs.py regression is dead code across 7+ touchpoints | `verify-logs.py:60-88,153,165,174,194,205-208,219-221` | AC must verify ALL regression-related code removed AND remaining functions preserved |
| C5 | Stale regression logs exist | `_out/logs/prod/regression/` (24 files) | AC should verify directory cleanup (not_exists or empty) |
| C6 | Obligation annotations required in Transition section | `phase-5-19-content-migration.md` Transition section | AC should verify per-obligation status annotations present |
| C7 | F782 is correct N+4 deferral destination | F782 [DRAFT] — Post-Phase Review Phase 20 | Record per Deferred Task Protocol; AC verifies Mandatory Handoffs entry |
| C8 | --flow spans 5+ active skill/command files beyond testing SKILL | `ENGINE.md:82-93`, `run-workflow/SKILL.md:97`, `erb-syntax/SKILL.md:104`, `kojo-init.md:171`, `engine-dev/SKILL.md:476` | N+4 deferral notes must document expanded --flow removal scope for F782 |

### Constraint Details

**C1: N+4 Execution Prohibition**
- **Source**: 3/3 investigators confirmed --unit is actively used in kojo testing (KOJO.md, PHASE-3.md, PHASE-5.md, engine.Tests, KojoComparer); Phase 20 gate constraint applies
- **Verification**: `Grep("--unit", ".claude/skills/testing/KOJO.md")` returns multiple matches; `Grep("--unit", "engine.Tests/")` returns matches in ProcessLevelParallelRunnerTests.cs
- **AC Impact**: ac-designer must NOT create any AC that requires --unit or --flow removal from active files; N+4 disposition is "DEFERRED to F782" only

**C2: do.md Absence**
- **Source**: 3/3 investigators confirmed do.md only exists in `.tmp/recovery/` archives
- **Verification**: `Glob(".claude/commands/do.md")` returns empty
- **AC Impact**: N+3 and N+7 ACs verify the file does not exist in active command path, confirming these obligations are obsolete

**C3: N+2 Already Satisfied**
- **Source**: 3/3 investigators confirmed zero ErbLinter references in `.githooks/pre-commit`
- **Verification**: `Grep("ErbLinter", ".githooks/pre-commit")` returns zero matches
- **AC Impact**: AC verifies current state only (not_contains); no code change required

**C4: verify-logs.py Regression Dead Code**
- **Source**: 3/3 investigators + Round 2 review identified 7+ touchpoints: `verify_regression_logs()` function (lines 60-88), help text "regression" (line 153), `check_regression = True` flag (line 165, set but never used as guard), scope branch `elif scope == "regression"` (line 174), unconditional call (line 194), output formatting (lines 205-208), total calculation aggregation (lines 219-221)
- **Verification**: Run `verify-logs.py` with and without regression code; behavior identical since `test/regression/` does not exist
- **AC Impact**: AC must verify removal of ALL regression-related code (function, flag, scope branch, call, formatting, aggregation) AND preservation of remaining verify functions (`verify_ac_logs`, `verify_engine_logs`)
- **Note**: `check_regression` at line 165 is set to `True` but never used as a guard — the `verify_regression_logs()` call at line 194 is unconditional

**C5: Stale Regression Logs**
- **Source**: 2/3 investigators discovered 24 stale JSON files in `_out/logs/prod/regression/`
- **Verification**: `ls _out/logs/prod/regression/` shows 24 files
- **AC Impact**: AC should verify cleanup of stale files (directory empty or absent after cleanup)

**C6: Obligation Annotation**
- **Source**: 3/3 investigators agreed Transition section needs per-obligation status tracking
- **Verification**: Current Transition section has no per-obligation status annotations
- **AC Impact**: AC verifies annotations present for all 7 obligations (N+1 through N+7) with status and destination

**C7: F782 Deferral Destination**
- **Source**: 3/3 investigators identified F782 [DRAFT] as the correct destination for N+4
- **Verification**: `pm/features/feature-782.md` exists with Status [DRAFT]
- **AC Impact**: Mandatory Handoffs entry must reference F782; deferral must follow Deferred Task Protocol

**C8: --flow Expanded Scope**
- **Source**: Investigators + Round 2 review identified --flow references in ENGINE.md, run-workflow/SKILL.md, erb-syntax/SKILL.md, kojo-init.md, and engine-dev/SKILL.md:476 beyond original testing SKILL scope
- **Verification**: `Grep("--flow", ".claude/skills/")` and `Grep("--flow", ".claude/commands/")` return multiple active-file matches
- **AC Impact**: N+4 deferral documentation must note expanded scope (5+ files) so F782 addresses all --flow locations, not just testing SKILL

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F784 | [DONE] | Test Infrastructure Remediation — fixed Phase references, handed off obligation execution |
| Related | F785 | [CANCELLED] | Regression Test Recovery — N+6 disposition (ERB flow test revival unnecessary) |
| Related | F782 | [DRAFT] | Post-Phase Review Phase 20 — deferral target for N+4 (--unit/--flow removal) |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "promises should be fulfilled" | Every obligation (N+2 through N+7) must be dispositioned — executed, recorded as satisfied/obsolete, or deferred with concrete destination | AC#1, AC#2, AC#3, AC#6, AC#7, AC#11, AC#14 |
| "stale references should not mislead" | Dead regression code in verify-logs.py must be removed; stale regression log files must be cleaned | AC#4, AC#5, AC#12 |
| "comprehensive and continuously enforced" | Remaining test verification functions (verify_ac_logs, verify_engine_logs) must be preserved after regression removal, and obligation status must be annotated | AC#7, AC#8, AC#9, AC#13 |
| "every obligation must be either executed or dispositioned" (SSOT claim) | Per-obligation status tracking in phase-5-19-content-migration.md Transition section with status and destination for all 7 obligations | AC#7, AC#11, AC#14 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | N+2 verified: pre-commit has no ErbLinter references | file | Grep(.githooks/pre-commit) | not_contains | "ErbLinter" | [x] |
| 2 | N+3/N+7 verified: do.md absent from active commands (both obligations confirmed obsolete) | file | Glob(.claude/commands/do.md) | not_exists | - | [x] |
| 3 | N+5 executed: verify_regression_logs function removed | code | Grep(src/tools/python/verify-logs.py) | not_contains | "verify_regression_logs" | [x] |
| 4 | N+5 executed: all regression references removed from verify-logs.py | code | Grep(src/tools/python/verify-logs.py) | not_matches | "(?i)(regression\|reg_(dir\|result\|line\|details))" | [x] |
| 5 | N+5 executed: stale regression log directory cleaned | file | Glob(_out/logs/prod/regression/*-result.json) | not_exists | - | [x] |
| 6 | N+4 deferred: F782 contains N+4 obligation note from F786 | file | Grep(pm/features/feature-782.md) | contains | "N+4" | [x] |
| 7 | Transition section annotated: N+5 obligation has DONE status | file | Grep(docs/architecture/phases/phase-5-19-content-migration.md) | matches | "N\\+5.*DONE" | [x] |
| 8 | Remaining functions preserved: verify_ac_logs exists | code | Grep(src/tools/python/verify-logs.py) | contains | "def verify_ac_logs" | [x] |
| 9 | Remaining functions preserved: verify_engine_logs exists | code | Grep(src/tools/python/verify-logs.py) | contains | "def verify_engine_logs" | [x] |
| 10 | N+4 deferral notes document expanded --flow scope in F782 | file | Grep(pm/features/feature-782.md) | contains | "engine-dev/SKILL.md" | [x] |
| 11 | Transition section annotated: N+4 obligation has DEFERRED status | file | Grep(docs/architecture/phases/phase-5-19-content-migration.md) | matches | "N\\+4.*DEFERRED" | [x] |
| 12 | PHASE-9.md stale Regression output removed | file | Grep(.claude/skills/run-workflow/PHASE-9.md) | not_matches | "(?i)regression" | [x] |
| 13 | verify-logs.py executes successfully after rewrite | build | python src/tools/python/verify-logs.py --help | succeeds | - | [x] |
| 14 | All 7 obligations annotated in Transition section | file | Grep(docs/architecture/phases/phase-5-19-content-migration.md, "N\\+[1-7].*(?:DONE\|OBSOLETE\|DEFERRED)") | count_equals | 7 | [x] |
| 15 | N+6 verified: archived regression tests exist | file | Glob(test/_archived/regression/*) | exists | - | [x] |

**Note**: 15 ACs is at the upper bound of the infra range (8-15). AC#2 combines N+3 and N+7 verification (both target do.md) for efficiency. AC#7, AC#11, and AC#14 provide 3-point coverage of the obligation status table (N+5 DONE + N+4 DEFERRED + count=7). AC#15 verifies N+6 DONE claim.

### AC Details

**AC#1: N+2 verified: pre-commit has no ErbLinter references**
- **Test**: `Grep("ErbLinter", ".githooks/pre-commit")` returns zero matches
- **Expected**: No ErbLinter references in pre-commit hook (already satisfied organically)
- **Rationale**: Confirms N+2 obligation is already fulfilled without requiring code change. (Constraint C3)

**AC#2: N+3/N+7 verified: do.md absent from active commands**
- **Test**: `Glob(".claude/commands/do.md")` returns empty
- **Expected**: File does not exist in active commands directory
- **Rationale**: N+3 targets "do.md Phase 3 uses dotnet test" and N+7 targets "do.md outputs TRX to logs" — both obligations reference a file replaced by the `/run` command, making both obsolete. Single check confirms both dispositions. (Constraint C2)

**AC#3: N+5 executed: verify_regression_logs function removed**
- **Test**: `Grep("verify_regression_logs", "src/tools/python/verify-logs.py")` returns zero matches
- **Expected**: Function definition and all call sites removed
- **Rationale**: Core of N+5 obligation. The function (lines 60-88) is dead code since `test/regression/` does not exist. (Constraint C4)

**AC#4: N+5 executed: all regression references removed from verify-logs.py**
- **Test**: `Grep("(?i)(regression|reg_(dir|result|line|details))", "src/tools/python/verify-logs.py")` returns zero matches
- **Expected**: No occurrence of "regression" (case-insensitive) OR regression-prefixed variables (`reg_dir`, `reg_result`, `reg_line`, `reg_details`) in the file — covers function (lines 60-88), help text (line 153), flag `check_regression` (line 165), scope branch (line 174), unconditional call (line 194), output formatting `reg_line`/`reg_details` (lines 205-208), and total calculation `reg_result` aggregation (lines 219-221). The broadened pattern catches `reg_*` variables on lines 219-221 that don't contain the substring "regression" but are stale regression references.
- **Rationale**: Comprehensive check ensuring ALL 7+ regression touchpoints are removed, not just the function definition. Round 2 reviewers identified that the original 4-location count missed help text, output formatting, and total calculation. The `check_regression` flag at line 165 is set but never used as a guard — the call at line 194 is unconditional. Case-insensitive matcher prevents verification gap from capitalized "Regression" in lines 61 and 205. Broadened to include `reg_*` variable names to close verification gap where lines 219-221 use `reg_result` without containing "regression". (Constraint C4)

**AC#5: N+5 executed: stale regression log directory cleaned**
- **Test**: `Glob("_out/logs/prod/regression/*-result.json")` returns empty
- **Expected**: No stale JSON result files remain (baseline: 24 files)
- **Rationale**: Stale regression log files could mislead future tooling by showing non-zero regression results from archived test scenarios. (Constraint C5)

**AC#6: N+4 deferred: F782 contains N+4 obligation note from F786**
- **Test**: `Grep("N+4", "pm/features/feature-782.md")` returns match
- **Expected**: F782 contains a note about the N+4 --unit/--flow removal obligation deferred from F786
- **Rationale**: Ensures bidirectional deferral tracking — F786 Mandatory Handoffs references F782, and F782 itself contains the obligation context. Prevents deferral from being lost when F782 undergoes /fc. (Constraint C1, C7)

**AC#7: Transition section annotated: N+5 obligation has DONE status**
- **Test**: `Grep("N\\+5.*DONE", "docs/architecture/phases/phase-5-19-content-migration.md")` returns match
- **Expected**: Transition section contains N+5 obligation with DONE status annotation
- **Rationale**: Spot-check verification of obligation status table. Combined with AC#11 (N+4 DEFERRED) and AC#14 (count=7), provides 3-point coverage of the obligation status annotations. (Constraint C6)

**AC#8: Remaining functions preserved: verify_ac_logs exists**
- **Test**: `Grep("def verify_ac_logs", "src/tools/python/verify-logs.py")` returns match
- **Expected**: `verify_ac_logs` function definition present
- **Rationale**: Regression removal must not damage remaining functionality. `verify_ac_logs` handles AC test log verification (JSON format). (Constraint C4)

**AC#9: Remaining functions preserved: verify_engine_logs exists**
- **Test**: `Grep("def verify_engine_logs", "src/tools/python/verify-logs.py")` returns match
- **Expected**: `verify_engine_logs` function definition present
- **Rationale**: Regression removal must not damage remaining functionality. `verify_engine_logs` handles Engine test log verification (TRX/XML format). (Constraint C4)

**AC#10: N+4 deferral notes document expanded --flow scope in F782**
- **Test**: `Grep("engine-dev/SKILL.md", "pm/features/feature-782.md")` returns match
- **Expected**: F782 contains engine-dev/SKILL.md among the expanded --flow scope files for N+4 deferral
- **Rationale**: Round 2 reviewers identified engine-dev/SKILL.md:476 as an additional --flow reference not in the original 4-file scope. F782 must be aware of all 5+ files to avoid incomplete cleanup. Verifying the deferral destination (not self-referential feature-786.md) ensures the scope information reaches F782. (Constraint C8)

**AC#11: Transition section annotated: N+4 obligation has DEFERRED status**
- **Test**: `Grep("N\\+4.*DEFERRED", "docs/architecture/phases/phase-5-19-content-migration.md")` returns match
- **Expected**: Obligation status table contains N+4 entry with DEFERRED status
- **Rationale**: Provides 2-point coverage of the obligation status table alongside AC#7 (N+5 DONE). Verifies that the deferred obligation is correctly annotated.

**AC#12: PHASE-9.md stale Regression output removed**
- **Test**: `Grep("(?i)regression", ".claude/skills/run-workflow/PHASE-9.md")` returns zero matches
- **Expected**: No "regression" references (case-insensitive) in PHASE-9.md expected output format (lines 82-88) or associated Note — catches both capital-R "Regression:" output format and lowercase "regression" in explanatory text
- **Rationale**: F786 removes all regression code from verify-logs.py; PHASE-9.md documents expected output including "Regression: OK:{M}/{M}" which becomes stale. Direct consequence of F786 changes.

**AC#13: verify-logs.py executes successfully after rewrite**
- **Test**: `python src/tools/python/verify-logs.py --help` exits with code 0
- **Expected**: Script executes without syntax errors, import errors, or runtime errors
- **Rationale**: Comprehensive rewrite (Option B) modifies multiple code sections. Static text verification (AC#3, AC#4, AC#8, AC#9) confirms content but not executability. --help flag triggers module loading and argument parser initialization without side effects.

**AC#14: All 7 obligations annotated in Transition section**
- **Test**: `Grep("N\\+[1-7].*(?:DONE|OBSOLETE|DEFERRED)", "docs/architecture/phases/phase-5-19-content-migration.md")` returns count_equals 7
- **Expected**: Exactly 7 lines matching the pattern (one per obligation N+1 through N+7)
- **Rationale**: AC#7 (N+5 DONE) and AC#11 (N+4 DEFERRED) provide spot-checks of 2 obligations, but the Philosophy claim "every obligation must be either executed or dispositioned" requires all 7. count_equals 7 ensures no obligation is omitted from the annotation table. (Constraint C6)

**AC#15: N+6 verified: archived regression tests exist**
- **Test**: `Glob("test/_archived/regression/*")` returns at least one match
- **Expected**: Archived regression test files exist, confirming N+6 DONE claim
- **Rationale**: N+6 obligation ("regression tests archived") is claimed as DONE in the Obligation Status table based on `_archived/regression/` existing and `regression/` being absent. AC#14 verifies the annotation exists but not the claim's accuracy. AC#15 provides runtime verification that the archived files actually exist (baseline: 48 files). (Constraint C6)

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Execute N+5: remove dead regression code from verify-logs.py | AC#3, AC#4, AC#8, AC#9, AC#13 |
| 2 | Execute N+5: clean stale regression log files | AC#5 |
| 3 | Record verified disposition for N+2 (already satisfied) | AC#1 |
| 4 | Record verified dispositions for N+3 and N+7 (obsolete) | AC#2 |
| 5 | Defer N+4 to F782 with concrete Task entry | AC#6, AC#10 |
| 6 | Annotate Transition section with per-obligation status | AC#7, AC#11, AC#14 |
| 7 | Remove stale Regression output documentation from PHASE-9.md | AC#12 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature uses a **disposition-based approach** where each of the 5 orphaned obligations (N+2, N+3, N+4, N+5, N+7) is individually dispositioned according to its current applicability:

1. **Verification-only** (N+2): Pre-commit hook already has no ErbLinter references — verify current state with Grep, no code change required
2. **Obsolete** (N+3, N+7): Target file (do.md) was replaced by /run command — verify absence with Glob to confirm obsolete status
3. **Executable** (N+5): verify-logs.py contains dead regression code across 7+ touchpoints — remove all regression-related code (function, help text, flag, scope branch, call site, output formatting, total aggregation) and clean 24 stale log files
4. **Deferred** (N+4): --unit actively used in kojo testing, Phase 20 gate applies — defer to F782 with concrete Task entry per Deferred Task Protocol, document expanded --flow scope (5+ files beyond original testing SKILL)

After dispositions, annotate the Transition section in `phase-5-19-content-migration.md` with per-obligation status (DONE/OBSOLETE/DEFERRED) and destination/reference for all 7 obligations (N+1 through N+7), resolving the root cause (monolithic batch trigger with no per-item lifecycle tracking).

**Rationale**: The Root Cause Analysis identified that obligations were designed as a monolithic batch gated by Phase 20 completion, but the codebase evolved independently. Individual disposition allows us to: (1) execute the one actionable change immediately (N+5 dead code removal), (2) record already-satisfied state without redundant edits (N+2), (3) confirm obsolete obligations whose targets no longer exist (N+3, N+7), and (4) defer premature work with concrete tracking (N+4). The per-obligation status annotation prevents future ambiguity and satisfies the Philosophy claim "every obligation must be either executed or dispositioned with concrete destination."

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Grep `.githooks/pre-commit` for "ErbLinter" — expect zero matches (verification-only, no code change) |
| 2 | Glob `.claude/commands/do.md` — expect not_exists (confirms N+3 and N+7 obsolete: target replaced by /run) |
| 3 | Remove `verify_regression_logs()` function (lines 60-88) from `src/tools/python/verify-logs.py` — Grep for function name returns zero matches post-removal |
| 4 | Remove ALL regression-related code from `src/tools/python/verify-logs.py`: function (60-88), help text (153), flag `check_regression` (165), scope branch `elif scope == "regression"` (174), unconditional call (194), output formatting `reg_line`/`reg_details` (205-208), total calculation `reg_result` (219-221) — Grep for "(?i)(regression\|reg_(dir\|result\|line\|details))" returns zero matches |
| 5 | Delete `_out/logs/prod/regression/` directory or all 24 JSON files within — Glob for `*-result.json` returns not_exists |
| 6 | Verify F782 contains N+4 obligation note — Grep feature-782.md for "N+4" |
| 7 | Edit `phase-5-19-content-migration.md` Transition section to add per-obligation annotations for all 7 obligations with status and destination — Grep for "N\\+5.*DONE" confirms annotation presence |
| 8 | Preserve `verify_ac_logs()` function during regression removal — Grep for "def verify_ac_logs" returns match |
| 9 | Preserve `verify_engine_logs()` function during regression removal — Grep for "def verify_engine_logs" returns match |
| 10 | Document expanded --flow scope in deferral notes in F782, including engine-dev/SKILL.md:476 among 5+ affected files — Grep feature-782.md for "engine-dev/SKILL.md" |
| 11 | Grep `phase-5-19-content-migration.md` for "N\\+4.*DEFERRED" — expect match confirming N+4 DEFERRED annotation |
| 12 | Remove all regression references from PHASE-9.md expected output (lines 82-88) and associated Note (line 88) — Grep for "(?i)regression" returns zero matches |
| 13 | Run `python src/tools/python/verify-logs.py --help` — expect exit code 0 (succeeds) |
| 14 | Grep `phase-5-19-content-migration.md` for "N\\+[1-7].*(?:DONE\|OBSOLETE\|DEFERRED)" — count_equals 7 (all obligations annotated) |
| 15 | Glob `test/_archived/regression/*` — expect exists (confirms N+6 DONE: archived regression tests present) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| N+5 removal scope | (A) Remove only `verify_regression_logs()` function; (B) Remove function + call site + flag; (C) Remove all 7+ regression touchpoints (function, help text, flag, scope branch, call, formatting, aggregation) | C | Round 2 review identified that original 4-location count missed help text (line 153), output formatting variables `reg_line`/`reg_details` (205-208), and total calculation `reg_result` aggregation (219-221). Option C ensures comprehensive cleanup with zero stale references, satisfying Philosophy "stale references should not mislead." The `check_regression` flag at line 165 is set to `True` but never used as a guard (call at line 194 is unconditional), so removal is safe. |
| N+4 deferral destination | (A) Create new feature F786b; (B) Defer to F782 (Post-Phase Review Phase 20); (C) Add to F647 (Phase 20 Planning) | B | F782 is the Post-Phase Review for Phase 20, making it the correct destination per Phase-based deferral protocol. Phase 20 gate constraint (phase-5-19-content-migration.md:2610) explicitly prohibits execution before Phase 20 completion, and --unit is actively used in kojo testing (cannot remove until C# migration completes). F782 will verify Phase 20 completion and assess --unit deprecation readiness. |
| --flow scope documentation | (A) Reference only testing SKILL (original N+4 scope); (B) List expanded scope (ENGINE.md, run-workflow, erb-syntax, kojo-init.md, engine-dev/SKILL.md) | B | Investigators + Round 2 review identified 5+ files with --flow references beyond testing SKILL. F782 must be aware of all locations to avoid incomplete cleanup. Documenting expanded scope prevents future rework. |
| verify-logs.py removal method | (A) Manual edit to remove each location individually; (B) Comprehensive rewrite excluding all regression logic; (C) Automated regex replacement | B | Option B (comprehensive rewrite) is safer than option A (risk of missing a location) and more maintainable than option C (regex cannot handle multi-line function removal). The implementer will read the current file, identify all regression-related sections, and write a clean version preserving only `verify_ac_logs` and `verify_engine_logs` functionality. |
| Obligation annotation format | (A) Inline status in existing Transition section prose; (B) Add status table after obligation definitions; (C) Append status inline to each obligation as `<!-- Status: DONE -->` | B | Option B (status table) provides clear tabular format matching the project's documentation style (see Feasibility Assessment table, Impact Analysis table). Table format allows at-a-glance status verification and is grep-friendly for AC#8 verification. Format: `\| N+{N} \| {Description} \| {Status} \| {Destination/Reference} \|` |
| Regression log cleanup strategy | (A) Delete individual files; (B) Delete entire `_out/logs/prod/regression/` directory; (C) Move to `test/_archived/regression/` | B | Directory contains only stale regression logs (24 JSON files, no other content). Full directory deletion is cleaner than individual file deletion and prevents misleading future tooling. Moving to archived would preserve files that have no value (they reference non-existent test scenarios). AC#6 verifies Glob returns not_exists. |

### Interfaces / Data Structures

No new interfaces or data structures required. This feature performs code removal, file cleanup, and documentation annotation using existing tools (Edit, Bash rm/rmdir, Grep verification).

**verify-logs.py structure after removal**:
```python
# Preserved functions (no changes to signatures or behavior)
def verify_ac_logs(...) -> tuple[int, int]:
    """Verify AC test logs (JSON format)."""
    # Existing implementation unchanged

def verify_engine_logs(...) -> tuple[int, int]:
    """Verify Engine test logs (TRX/XML format)."""
    # Existing implementation unchanged

# Main function updated
def main():
    # Remove: check_regression flag (line 165)
    # Remove: elif scope == "regression" branch (line 174)
    # Remove: verify_regression_logs() call (line 194)
    # Remove: reg_line/reg_details formatting (lines 205-208)
    # Remove: reg_result aggregation (lines 219-221)
    # Preserve: verify_ac_logs() and verify_engine_logs() calls
    # Preserve: ac_result and eng_result aggregation
```

**Obligation status table format** (to be added in phase-5-19-content-migration.md Transition section):
```markdown
### Obligation Status (as of F786 completion)

| AC | Description | Status | Destination/Reference |
|:--:|-------------|--------|----------------------|
| N+1 | pre-commit scope expansion | DONE | F784 AC#6 |
| N+2 | pre-commit removes ErbLinter | DONE | Already satisfied (F786 AC#1 verification) |
| N+3 | do.md Phase 3 uses dotnet test | OBSOLETE | do.md replaced by /run command |
| N+4 | testing SKILL removes --unit/--flow | DEFERRED | F782 (--unit actively used; expanded scope: 5+ files) |
| N+5 | verify-logs.py removes regression | DONE | F786 AC#3-5, AC#8-9 |
| N+6 | regression tests archived | DONE | Already satisfied (_archived/regression/ exists, regression/ absent) |
| N+7 | do.md outputs TRX to logs | OBSOLETE | do.md replaced by /run command (Phase 7 TRX output) |
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Verify N+2 obligation satisfied: pre-commit has no ErbLinter references (input for Task#5 annotation) | | [x] |
| 2 | 2 | Verify N+3 and N+7 obligations obsolete: do.md absent from active commands (input for Task#5 annotation) | | [x] |
| 3 | 3,4,8,9,13 | Execute N+5 obligation: remove all regression code from verify-logs.py and preserve remaining functions | | [x] |
| 4 | 5 | Execute N+5 obligation: clean stale regression log files | | [x] |
| 5 | 7,11,14,15 | Annotate Transition section with per-obligation status for all 7 obligations (AC#15 verifies N+6 DONE claim) | | [x] |
| 6 | 6,10 | Document N+4 deferral with expanded --flow scope and add N+4 obligation note to F782's Review Context section (survives /fc overwrite) | | [x] |
| 7 | 12 | Remove Regression output references from run-workflow/PHASE-9.md | | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | ac-tester | haiku | AC#1 (N+2 ErbLinter verification) | PASS/FAIL |
| 2 | ac-tester | haiku | AC#2 (do.md absence) | PASS/FAIL |
| 3 | implementer | sonnet | Task#3: Remove all regression code from verify-logs.py | Edited verify-logs.py preserving verify_ac_logs and verify_engine_logs |
| 4 | ac-tester | haiku | AC#3, AC#4, AC#8, AC#9 (regression removal verification) | PASS/FAIL |
| 5 | ac-tester | haiku | AC#13 (execution verification) | PASS/FAIL |
| 6 | implementer | sonnet | Task#4: Delete _out/logs/prod/regression/ directory | Directory removed |
| 7 | ac-tester | haiku | AC#5 (stale log cleanup) | PASS/FAIL |
| 8 | implementer | sonnet | Task#5: Add obligation status table to phase-5-19-content-migration.md Transition section | Annotated with per-obligation status (DONE/OBSOLETE/DEFERRED) for N+1 through N+7 |
| 9 | ac-tester | haiku | AC#7, AC#11, AC#14, AC#15 (annotation + N+6 verification) | PASS/FAIL |
| 10 | implementer | sonnet | Task#6: Add N+4 obligation note and expanded --flow scope to F782 Review Context section | F782 Review Context has N+4 note with engine-dev/SKILL.md scope |
| 11 | ac-tester | haiku | AC#6, AC#10 (F782 deferral verification) | PASS/FAIL |
| 12 | implementer | sonnet | Task#7: Remove Regression output references from PHASE-9.md | PHASE-9.md updated |
| 13 | ac-tester | haiku | AC#12 (PHASE-9.md stale documentation) | PASS/FAIL |
| 14 | finalizer | haiku | Feature-786.md | Status [DONE], Execution Log updated |

**Execution Order**

**Verification-first approach**: Start with disposition verification (Tasks 1-2) to confirm current state before executing changes (Tasks 3-6). This validates that N+2 is indeed already satisfied, and N+3/N+7 targets are absent, matching the Feasibility Assessment conclusions.

1. **Tasks 1-2** (Phases 1-2): Verify disposition claims (N+2 already satisfied, N+3/N+7 obsolete)
2. **Tasks 3-4** (Phases 3-7): Execute N+5 obligation (dead code removal + log cleanup)
3. **Task 5** (Phases 8-9): Record all dispositions in Transition section annotation
4. **Tasks 6-7** (Phases 10-13): Document N+4 deferral with expanded scope + PHASE-9.md cleanup
5. **Phase 14**: Finalize

**Success Criteria**

- All 15 ACs pass verification
- verify-logs.py contains zero occurrences of "regression" but preserves verify_ac_logs and verify_engine_logs functions
- _out/logs/prod/regression/ directory does not exist or contains no files
- phase-5-19-content-migration.md Transition section has obligation status table with entries for N+1 through N+7
- Mandatory Handoffs references F782 with expanded --flow scope documentation including engine-dev/SKILL.md
- run-workflow/PHASE-9.md contains no "Regression" references in expected output

**Rollback Plan**

If issues arise after deployment:
1. Revert verify-logs.py changes with `git checkout HEAD~1 -- src/tools/python/verify-logs.py`
2. Restore regression logs if needed (not recommended, they are stale)
3. Revert Transition section annotation with `git checkout HEAD~1 -- docs/architecture/phases/phase-5-19-content-migration.md`
4. Create follow-up feature for fix

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| N+4 --unit/--flow removal | --unit actively used in kojo testing; Phase 20 gate applies; --flow scope larger than originally defined (spans 5+ files: testing/SKILL.md, ENGINE.md, run-workflow/SKILL.md, erb-syntax/SKILL.md, kojo-init.md, engine-dev/SKILL.md:476) | Feature | F782 | Task#6 |
| Stale AC logs + verify-logs.py lifecycle | ~360 stale AC logs from [DONE] features pollute `--scope all`; no lifecycle cleanup mechanism exists | Feature | F787 | /run Phase 9 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-12 16:22 | START | implementer | Task 3 | - |
| 2026-02-12 16:22 | END | implementer | Task 3 | SUCCESS |
| 2026-02-12 16:24 | START | implementer | Task 4 | - |
| 2026-02-12 16:24 | END | implementer | Task 4 | SUCCESS |
| 2026-02-12 16:25 | START | implementer | Task 5 | - |
| 2026-02-12 16:25 | END | implementer | Task 5 | SUCCESS |
| 2026-02-12 16:40 | START | implementer | Task 6 | - |
| 2026-02-12 16:40 | END | implementer | Task 6 | SUCCESS |
| 2026-02-12 16:41 | START | implementer | Task 7 | - |
| 2026-02-12 16:41 | END | implementer | Task 7 | SUCCESS |
| 2026-02-12 16:42 | DEVIATION | Bash | git rm -r _out/logs/prod/regression/ | exit 128 (gitignored files not tracked) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: [REF-LINK] Links section | Removed broken F646 link (feature-646.md does not exist)
- [fix] Phase2-Review iter1: [FMT-TABLE] Mandatory Handoffs table | Changed 'T6' to 'Task#6' per template format
- [fix] Phase2-Review iter1: [AC-MAP] Philosophy Derivation table | Added AC#9, AC#10 to 'comprehensive and continuously enforced' mapping
- [fix] Phase2-Review iter1: [FMT-ORDER] Section ordering | Moved Acceptance Criteria before Technical Design per template
- [fix] Phase2-Review iter1: [FMT-STRUCT] Review Context section | Restructured to template format (Origin, Identified Gap, Review Evidence, Files Involved, Parent Review Observations)
- [resolved-applied] Phase2-Review iter1: [AC-DUP] AC#5/AC#11 redundancy — both verify not_contains "regression" in verify-logs.py with identical Type/Method/Matcher/Expected. AC#11 adds zero discriminating power beyond AC#5. Recommend removing AC#11 and updating Task#3 mapping.
- [fix] PostLoop-UserFix iter7: [AC-DUP] Removed AC#11 (duplicate of AC#5). Renumbered AC#12→AC#11. Updated Task#3 mapping, Goal Coverage, Implementation Contract, Obligation Status table.
- [resolved-applied] Phase2-Uncertain iter1: [DEP-ACT] F782 deferral actionability — F786 defers N+4 to F782 but F782 is [DRAFT] with no Tasks table. Task#6 writes to feature-786.md only, not F782. Template validation (Option B: Referenced Feature exists → OK) technically passes, but CLAUDE.md protocol implies adding content to destination feature.
- [fix] PostLoop-UserFix iter7: [DEP-ACT] Extended Task#6 to also add N+4 obligation note to F782. Bidirectional reference ensured.
- [fix] Phase2-Review iter2: [FMT-CODE] Review Notes [pending] entries | Added category codes [AC-DUP] and [DEP-ACT]
- [resolved-applied] Phase2-Review iter2: [AC-COV] AC#8 matcher only checks 1 of 7 obligations — contains "N+5.*DONE" verifies N+5 annotation but not N+1/N+2/N+3/N+4/N+6/N+7. Philosophy claim "every obligation must be dispositioned" is only partially verified. Recommend adding second AC (e.g., contains "N+4.*DEFERRED") for minimum 2-point coverage.
- [fix] PostLoop-UserFix iter7: [AC-COV] Added AC#12 (matches "N\+4.*DEFERRED") for 2-point obligation status table coverage.
- [fix] Phase2-Review iter3: [FMT-ORDER] Section ordering | Moved Dependencies before Acceptance Criteria per template order
- [fix] Phase2-Review iter3: [FMT-STRUCT] Review Context | Removed duplicate Philosophy subsection (content already in Background > Philosophy)
- [resolved-applied] Phase2-Review iter3: [AC-PRE] AC#7/AC#12 pre-satisfied — AC#7 (contains "F782" in feature-786.md) and AC#12 (contains "engine-dev/SKILL.md" in feature-786.md) pass without implementation work; content already exists in Mandatory Handoffs table from /fc phase. Tasks 6-7 describe creating content that already exists.
- [fix] PostLoop-UserFix iter7: [AC-PRE] Changed AC#7 to verify F782 content (Grep feature-782.md for "N+4") instead of self-referential feature-786.md check.
- [resolved-applied] Phase2-Uncertain iter3: [TASK-MAP] Tasks 1-2 verify-only but Goal says "recording" — verification-first architecture is intentional (documented in Execution Order), actual recording is Task#5. Semantic gap between Goal and Task descriptions.
- [fix] PostLoop-UserFix iter7: [TASK-MAP] Added "(input for Task#5 annotation)" to Task#1 and Task#2 descriptions.
- [resolved-applied] Phase2-Uncertain iter3: [AC-DUP] AC#2/AC#3 identical verification — both Glob(.claude/commands/do.md)/not_exists for different obligations (N+3 vs N+7). Feature justifies as traceability but single Glob result satisfies both.
- [fix] PostLoop-UserFix iter8: [AC-DUP] Merged AC#2 and AC#3 into single AC#2 (N+3/N+7 combined verification). Renumbered AC#4→AC#3 through AC#12→AC#11. Updated all Task mappings, Philosophy Derivation, Goal Coverage, AC Details, Technical Design AC Coverage, Implementation Contract, Success Criteria, and Obligation Status table references.
- [resolved-applied] Phase2-Uncertain iter4: [DOC-LABEL] N+6 disposition label incorrect — Technical Design Obligation Status table marks N+6 as "N/A | F785 [CANCELLED]" but test/regression/ does not exist (verified by Glob) and _archived/regression/ has 48 files. N+6 AC condition (not_exists) is already satisfied. Label should be "DONE (already satisfied)". Goal count of 5 orphaned obligations remains correct (N+6 is satisfied, not orphaned).
- [resolved-applied] Phase3-Maintainability iter4: [LEAK-DOC] run-workflow/PHASE-9.md (lines 83-88) documents expected output as "Regression: OK:{M}/{M}" with note about "Regression: OK:0/0". After F786 removes regression code from verify-logs.py, this documentation will be stale. Recommend adding Task + AC to update PHASE-9.md (remove Regression line from expected output and note).
- [fix] Phase4-ACValidation iter4: [AC-MATCH] AC#8 | Changed Matcher from 'contains' to 'matches' — Expected "N+5.*DONE" uses regex syntax incompatible with literal contains matcher
- [fix] Phase2-Review iter5: [LANG-POL] Parent Review Observations | Translated Japanese content to English per Language Policy
- [fix] Phase2-Review iter5: [FMT-CODE] [fix] entries | Added category codes to all [fix] entries per Review Notes format
- [fix] Phase2-Review iter5: [FMT-TABLE] Files Involved header | Changed 'Role' to 'Relevance' per template
- [fix] Phase2-Review iter6: [FMT-STRUCT] Implementation Contract | Demoted ### subsection headings to **bold** (Execution Order, Success Criteria, Rollback Plan are content, not template-defined subsections)
- [resolved-applied] Phase2-Uncertain iter7: [BASE-ACC] Baseline regression count "11 locations" inconsistent with feature's own "7+ touchpoints" (AC#5 Details). Grep returns 8 matching lines. Informational; no AC depends on this count. Minor.
- [resolved-applied] Phase2-Uncertain iter7: [AC-CASE] AC#5 not_contains "regression" case-sensitive; misses capitalized "Regression" (lines 61, 205) and reg_* variables (reg_result, reg_line, reg_details). Practically mitigated by comprehensive rewrite (option B) but verification gap exists.
- [resolved-applied] Phase2-Review iter7: [AC-EXEC] No AC verifies verify-logs.py executes successfully after comprehensive rewrite. Project supports build/succeeds AC type. Recommend adding AC: build | python src/tools/python/verify-logs.py --dir _out/logs/prod --scope all | succeeds.
- [fix] PostLoop-UserFix iter8: [DOC-LABEL] Obligation Status table | N+6 label changed from N/A to DONE (already satisfied)
- [fix] PostLoop-UserFix iter8: [LEAK-DOC] AC/Task tables | Added AC#12 (PHASE-9.md Regression removal) and Task#8
- [fix] PostLoop-UserFix iter8: [BASE-ACC] Baseline Measurement | Corrected regression count from 11 to 8 Grep matches + 5 reg_* lines
- [fix] PostLoop-UserFix iter8: [AC-CASE] AC Definition Table | Changed AC#4 from not_contains to not_matches (?i)regression
- [fix] PostLoop-UserFix iter8: [AC-EXEC] AC/Task tables | Added AC#13 (verify-logs.py execution verification)
- [fix] Phase2-Review iter9: [FMT-COUNT] AC count Note + Success Criteria | Changed "11 ACs" to "13 ACs"
- [fix] Phase2-Review iter9: [FMT-TABLE] AC Coverage table | Renumbered and reordered to match AC Definition Table after AC#2/AC#3 merge
- [fix] Phase2-Review iter9: [AC-GROUP] Implementation Contract Phase 4 | Removed AC#10 (deferral scope, tested in Phase 11)
- [fix] Phase2-Review iter9: [AC-PRE] AC#10 | Changed from self-referential Grep(feature-786.md) to Grep(feature-782.md) to verify F782 receives expanded --flow scope
- [fix] Phase2-Review iter10: [FMT-COUNT] AC count Note + Success Criteria | Changed "13 ACs" to "14 ACs" after adding AC#14
- [fix] Phase2-Review iter10: [AC-DESC] AC#7 Description | Changed "all 7 obligations" to "N+5 obligation has DONE status" to match Matcher
- [fix] Phase2-Review iter10: [AC-COV] Added AC#14 (count_equals 7 obligations) for Philosophy coverage
- [fix] Phase2-Review iter10: [GOAL-COV] Goal section | Added PHASE-9.md Regression removal to Goal statement
- [fix] Phase2-Review iter10: [TASK-RED] Removed Task#7 (verification-only duplicate of AC#6). Renumbered Task#8→Task#7
- [fix] Phase2-Review iter10: [AC-MAP] Philosophy Derivation row 4 | Changed AC#8 to AC#7, AC#11, AC#14
- [fix] Phase2-Review iter10: [TASK-MAP] Task#5 AC mapping | Added AC#14. Task#6 AC mapping | Added AC#6
- [resolved-applied] Phase2-Uncertain iter10: [DEP-DUR] F782 note durability — Task#6 adds N+4 note to F782 [DRAFT]. When F782 undergoes /fc, consensus-synthesizer may overwrite the note. Consider specifying Review Context section placement.
- [fix] PostLoop-UserFix iter13: [DEP-DUR] Task#6 + Implementation Contract Phase 10 | Specified Review Context section placement for F782 N+4 note
- [fix] Phase2-Review iter11: [FMT-PHASE] Execution Order | Updated phase references (3-7, 8-9, 10-13, 14) to match Implementation Contract table
- [fix] Phase2-Review iter11: [FMT-TABLE] AC#14 Expected column | Moved pattern to Method, Expected now holds only count value "7"
- [fix] Phase2-Review iter11: [AC-MAP] Philosophy Derivation row 2 | Moved AC#6 from "stale references" to "promises should be fulfilled"
- [fix] Phase2-Review iter11: [FMT-TABLE] Files Involved separator | Aligned separator width with header
- [resolved-applied] Phase2-Uncertain iter11: [AC-COV] N+6 DONE claim — Obligation Status table marks N+6 as DONE but no AC verifies _archived/regression/ exists. Verified during /fc investigation (line 181) but not at /run time.
- [fix] PostLoop-UserFix iter13: [AC-COV] Added AC#15 (Glob _archived/regression/ exists) for N+6 DONE claim verification
- [fix] Phase2-Review iter12: [AC-MAP] Philosophy Derivation row 1 | Replaced AC#8 with AC#11, AC#14 (obligation disposition ACs)
- [resolved-skipped] Phase2-Review iter1: [TASK-RED] Tasks 1-2 are verification-only (ac-tester dispatch) with no implementation artifact. Functionally redundant with ACs themselves. Current descriptions include "(input for Task#5 annotation)" but produce no output.
- [fix] Phase2-Review iter1: [AC-MATCH] AC#4 Matcher | Broadened from "(?i)regression" to "(?i)(regression|reg_(dir|result|line|details))" to close verification gap for reg_* variables on lines 219-221
- [fix] Phase2-Review iter2: [LANG-POL] Review Context Identified Gap | Translated "ERBベースのflow test復活は不要" to English
- [fix] Phase2-Review iter2: [AC-MATCH] AC#12 Matcher | Changed from not_contains "Regression" to not_matches "(?i)regression" for case-insensitive consistency with AC#4
- [fix] Phase2-Review iter3: [FMT-STRUCT] Review Context heading | Changed from "## Review Context (FL POST-LOOP Step 6.3)" to "## Review Context" with HTML comment; moved **Source** into Origin table
- [fix] Phase7-FinalRefCheck iter4: [REF-LINK] Links F774-F781 anchor | Changed #phase-20 to #phase-20-equipment--shop-systems to match actual heading

---

<!-- fc-phase-6-completed -->
## Links

[Predecessor: F784](feature-784.md) - Test Infrastructure Remediation (source of orphaned obligations)
[Related: F785](feature-785.md) - Regression Test Recovery [CANCELLED] — ERB flow test revival unnecessary
[Related: F647](feature-647.md) - Phase 20 Planning
[Related: F774-F781](index-features.md#phase-20-equipment--shop-systems) - Phase 20 sub-features (all unstarted)
[Related: F782](feature-782.md) - Post-Phase Review Phase 20 (deferral target for N+4)

**Key Files**:
- `docs/architecture/phases/phase-5-19-content-migration.md` - Test Infrastructure Transition section (obligation source, lines 2546-2610)
- `.githooks/pre-commit` - ErbLinter removal verification (N+2)
- `.claude/skills/testing/SKILL.md` - --unit/--flow references (N+4, deferred)
- `src/tools/python/verify-logs.py` - Regression dead code removal (N+5)
- `_out/logs/prod/regression/` - Stale regression log files (24 JSON files)
