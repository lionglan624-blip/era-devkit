# Feature 846: feature-status.py Active→Recently Completed migration gap

## Status: [CANCELLED]

## Type: infra

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F842 |
| Discovery Phase | Phase 10 (CodeRabbit Review) |
| Timestamp | 2026-03-06 |

### Observable Symptom
CodeRabbit review of F842 commit found F828 and F843 listed as [DONE] in the Active Features table of index-features.md, despite both being completed features that should have been moved to Recently Completed by their respective finalizer sessions.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `coderabbit review --plain --type committed --base-commit HEAD~1` |
| Exit Code | 0 |
| Error Output | 1 finding: "F843 appears in both Active Features and Recently Completed" |
| Expected | [DONE] features removed from Active, present only in Recently Completed |
| Actual | F828 [DONE] in Active only (not in Recently Completed); F843 [DONE] in both Active and Recently Completed |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/feature-status.py | Tool responsible for atomic status transitions including Active→Recently Completed migration |
| pm/index-features.md | Contains Active Features and Recently Completed tables |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| Manual Edit of index-features.md | Fixed | Workaround — root cause in feature-status.py or finalizer workflow not addressed |

### Parent Session Observations
Two distinct failure patterns observed: (1) F843 appeared in both Active and Recently Completed — `feature-status.py set 843 DONE` added to Recently Completed but failed to remove from Active. (2) F828 appeared only in Active as [DONE] — `feature-status.py set 828 DONE` may not have been run at all, or it failed to move the entry. F842 session JSONL confirms F842's own `feature-status.py set 842 DONE` executed correctly (output: "Active -> Recently Completed"). The bug is in **F828 and F843's respective finalizer sessions**, not F842's. Investigation: (a) find F828/F843 finalizer subagent JSONLs, (b) check if `feature-status.py set {ID} DONE` was invoked, (c) if invoked, check its output for anomalies (e.g., "Active -> Recently Completed" missing).

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
feature-status.py is the SSOT tool for atomic feature status transitions. When it reports success, the Active and Recently Completed tables must be consistent -- no [DONE] entries in Active, no missing entries in Recently Completed. Silent failures in this tool undermine the entire feature lifecycle's integrity.

### Problem (Current Issue)
`cmd_set` in `feature-status.py` always returns exit code 0 (line 455) regardless of whether `move_to_recently_completed` succeeded or failed. When `move_to_recently_completed` returns `False` (lines 206-208), the failure is silently ignored (lines 416-417: no else branch, no error message, no non-zero exit). This causes the finalizer to report READY_TO_COMMIT while the index remains stale. Additionally, `move_to_recently_completed` destructively removes the Active row via `lines.pop(row_idx)` (line 212) BEFORE attempting RC insertion (lines 217-234), so if the insertion loop fails to find a suitable point, the row is permanently lost from the in-memory state. A contributing factor is that `find_index_row` (lines 162-168) searches the entire index file without scoping to the Active Features section, meaning it can match rows in Recently Completed or other sections.

### Goal (What to Achieve)
Make `feature-status.py`'s Active-to-Recently-Completed migration fail-safe: (1) return non-zero exit code when `move_to_recently_completed` fails, (2) make the pop+insert operation atomic (only remove from Active after RC insertion is confirmed), (3) scope `find_index_row` to the Active Features section for completion transitions, and (4) add unit test coverage for the completion path.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why did F828 remain in Active and F843 appear in both Active and RC? | `move_to_recently_completed` either failed silently or produced a partial result, yet `cmd_set` reported success | `feature-status.py:416-417` (no error handling on False return) |
| 2 | Why did `cmd_set` report success when the move failed? | `cmd_set` always returns 0 regardless of `move_to_recently_completed` result | `feature-status.py:455` (always `return 0`) |
| 3 | Why does `move_to_recently_completed` fail silently? | It returns `False` with no error output when `find_index_row` returns None or when the RC insertion loop falls through | `feature-status.py:206-208,234-236` |
| 4 | Why can the RC insertion loop fail after the Active row is already removed? | `lines.pop(row_idx)` on line 212 destructively removes the row BEFORE the RC insertion is attempted on lines 217-234 | `feature-status.py:210-212` (pop before validated insert) |
| 5 | Why was there no error detection or atomicity guard? | The tool was designed without error propagation for the completion path and has zero unit test coverage | No test files exist for `feature-status.py` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F828 [DONE] stuck in Active; F843 in both Active and RC | `cmd_set` returns exit 0 on `move_to_recently_completed` failure; destructive pop before validated insert |
| Where | `pm/index-features.md` table inconsistency | `src/tools/python/feature-status.py:416-417,455,210-212` |
| Fix | Manual edit of index-features.md (already done) | Non-zero exit on failure, atomic pop+insert, section-scoped search, unit tests |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F842 | [DONE] | Parent feature -- CodeRabbit review of F842 commit discovered this bug |
| F828 | [DONE] | Affected feature -- was [DONE] in Active only, not moved to RC |
| F843 | [DONE] | Affected feature -- was in both Active and RC simultaneously |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Root cause identifiable | FEASIBLE | Silent exit 0 + destructive pop clearly traced in code (lines 212, 416, 455) |
| Fix scope bounded | FEASIBLE | All changes isolated to single file `feature-status.py` (~240 lines of index logic) |
| Test infrastructure | FEASIBLE | Pure Python functions operating on string lists; stdlib sufficient for unit tests |
| Backward compatibility | FEASIBLE | Success path unchanged; only failure path gets error reporting |
| No external blockers | FEASIBLE | Pure Python tool, no cross-repo impact |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Finalizer workflow | HIGH | Finalizer trusts exit code 0 = success; fix enables failure detection |
| Index consistency | HIGH | Prevents [DONE] features from remaining in Active Features table |
| Phase 10.2.1 workaround | MEDIUM | Post-finalizer verification gate becomes defense-in-depth rather than primary mitigation |
| Feature lifecycle SSOT | MEDIUM | Ensures index-features.md accurately reflects feature status transitions |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Active Features has multiple sub-tables with `###` phase headers and `**bold**` sub-sections | `pm/index-features.md:49-84` | `find_index_row` scoping must handle nested structure of Active Features |
| Active table uses `F{ID}` format, RC uses bare `{ID}` (no F prefix) | `pm/index-features.md` row formats | Regex `F?{fid}` matches both; section scoping prevents cross-section matches |
| `ac_ops` import at module level | `feature-status.py:29` | Test file must handle import path or mock `ac_ops` |
| No file-level locking in Python on Windows | Python stdlib limitation | Concurrent access protection needs workflow-level serialization, not code-level locking |
| Exit code contract with finalizer | `.claude/skills/finalizer/SKILL.md:76-89` | Non-zero exit must mean actual failure; finalizer trusts exit code |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Section-scoped `find_index_row` breaks on future index restructuring | LOW | LOW | Use explicit section boundary markers (`## Active Features` / `## Recently Completed`) |
| Fix changes exit code, breaking existing workflow expectations | LOW | MEDIUM | Only return non-zero when move actually fails; success path unchanged |
| Concurrent file access race condition remains after code fix | MEDIUM | MEDIUM | Document serialization requirement; workflow-level enforcement sufficient for single-caller pattern |
| Finalizer subagent does not invoke tool at all (behavioral deviation) | MEDIUM | MEDIUM | Phase 10.2.1 post-finalizer gate catches this case independently of tool fix |
| Test fixtures diverge from actual index format over time | LOW | LOW | Use realistic index snapshots in test fixtures |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| `cmd_set` exit code on move failure | `python feature-status.py set 999 DONE --dry-run` (nonexistent feature) | 0 | Currently always returns 0 |
| Unit test count for feature-status.py | `find src/tools/python -name "test_feature*"` | 0 | No test files exist |

**Baseline File**: `_out/tmp/baseline-846.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `cmd_set` must return non-zero exit code when `move_to_recently_completed` fails | `feature-status.py:455` | AC must verify exit code is non-zero on failed move |
| C2 | Active row must not be removed until RC insertion is confirmed | `feature-status.py:210-212` | AC must verify atomicity: no partial state (row lost from both sections) |
| C3 | `find_index_row` in move context must only search Active Features section | `feature-status.py:162-168` | AC must test with feature present in RC but not Active -- should return "not found" |
| C4 | Tool must print error message when move fails | `feature-status.py:416-417` | AC must verify stderr/stdout contains error message on failure |
| C5 | Existing success path must not regress | All callers (finalizer, FL workflow) | AC must verify normal DONE transition still works correctly |
| C6 | Unit tests must cover edge cases | No existing tests | AC must require coverage for: empty Active, feature in RC only, feature in both, missing RC section |

### Constraint Details

**C1: Non-zero exit on move failure**
- **Source**: `feature-status.py:455` always returns 0; lines 416-417 have no else branch
- **Verification**: Run `cmd_set` with a feature ID not present in Active and check exit code
- **AC Impact**: AC must test both successful move (exit 0) and failed move (exit non-zero)

**C2: Atomic pop+insert**
- **Source**: `feature-status.py:210-212` pops Active row before RC insertion is attempted (lines 217-234)
- **Verification**: Create index fixture where RC section has no valid insertion point; verify Active row is not lost
- **AC Impact**: AC must verify that when RC insertion fails, the Active row is preserved (not silently dropped)

**C3: Section-scoped search for completion**
- **Source**: `feature-status.py:162-168` `find_index_row` regex matches across entire file
- **Verification**: Create index fixture with feature in RC but not Active; verify move returns failure (not finding RC row)
- **AC Impact**: AC must test cross-section matching prevention

**C4: Error message on failure**
- **Source**: `feature-status.py:416-417` only prints on success, silent on failure
- **Verification**: Run with nonexistent feature ID, check stdout/stderr for error message
- **AC Impact**: AC must verify descriptive error output when move fails

**C5: Success path regression guard**
- **Source**: Finalizer and FL workflow depend on exit code 0 for success
- **Verification**: Run normal DONE transition on valid fixture; verify exit 0 and correct table state
- **AC Impact**: AC must include positive test for the happy path

**C6: Edge case test coverage**
- **Source**: Zero existing test coverage for feature-status.py
- **Verification**: Test file exists with test functions covering documented edge cases
- **AC Impact**: AC must specify minimum test scenario count
- **Collection Members** (MANDATORY): Edge cases to cover: (1) feature not in Active (not found), (2) feature in RC only (section scoping), (3) feature in both Active and RC (deduplication), (4) missing RC section header, (5) empty Active table, (6) normal successful move

---

<!-- fc-phase-3-completed -->
<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Three focused surgical changes to `src/tools/python/feature-status.py`, plus a new test file:

**1. Introduce `find_active_row(lines, fid)` — section-scoped Active-only search (satisfies C3, AC#6)**

Replace the call to `find_index_row(lines, fid)` inside `move_to_recently_completed` with a new helper that restricts its search to lines between `## Active Features` and the next `##`-level section. This prevents the cross-section false-match described in C3 (a feature present in RC but not Active would return None correctly, triggering a graceful failure).

```python
def find_active_row(lines: list[str], fid: str) -> int | None:
    """Find the line index of a feature row within the Active Features section only."""
    pat = re.compile(r"^\|\s*F?" + fid + r"\s*\|")
    in_active = False
    for i, line in enumerate(lines):
        stripped = line.strip()
        if stripped == "## Active Features":
            in_active = True
            continue
        if in_active and stripped.startswith("## "):
            break  # left Active Features section
        if in_active and pat.match(line):
            return i
    return None
```

The Active Features section header in `pm/index-features.md` is `## Active Features` (line 47 confirmed by Read). The section ends when the next `## ` line is encountered (`## Recently Completed`). The existing sub-headers (`### Phase N`) and `**bold**` sub-table headers do not start with exactly `## `, so they are transparently skipped.

**2. Make `move_to_recently_completed` atomic — insert first, pop second (satisfies C2, AC#4)**

Replace the destructive-first sequence with a safe two-pass approach: locate the RC insertion point first, then insert the RC row, then remove the Active row (adjusted for index shift). If no RC insertion point is found, return `False` without touching Active.

```python
def move_to_recently_completed(
    lines: list[str], fid: str, status_emoji: str = "\u2705"
) -> bool:
    """Remove row from Active and add to Recently Completed (atomic)."""
    row_idx = find_active_row(lines, fid)
    if row_idx is None:
        return False

    removed_line = lines[row_idx]
    title = extract_title_from_index_row(removed_line) or get_feature_title(fid)
    rc_row = f"| {fid} | {status_emoji} | {title} | [feature-{fid}.md](feature-{fid}.md) |\n"

    # Find RC insertion point BEFORE modifying Active
    in_rc = False
    insert_idx = None
    for i, line in enumerate(lines):
        if "## Recently Completed" in line:
            in_rc = True
            continue
        if in_rc:
            stripped = line.strip()
            if (
                stripped.startswith("<!--")
                or stripped == ""
                or stripped.startswith("| ID")
                or stripped.startswith("|:")
            ):
                continue
            insert_idx = i
            break

    if insert_idx is None:
        return False  # No valid RC insertion point — Active row preserved

    # Insertion point found: insert RC row, then remove Active row (adjusted for shift)
    lines.insert(insert_idx, rc_row)
    # row_idx shifts by +1 if it was after the insertion point; it was before RC section
    lines.pop(row_idx)  # row_idx < insert_idx always (Active is above RC)
    return True
```

Note: `row_idx` is always less than `insert_idx` because Active Features section precedes Recently Completed in the file. The `lines.pop(row_idx)` here is safe — it only executes after RC insertion is confirmed, and `row_idx` is not shifted by the insert (insert was at a higher index). AC#4 verifies the old pattern `lines\.pop\(row_idx\)` no longer appears in the destructive-first position; this new usage appears after RC insertion is confirmed and is acceptable because the semantics are now atomic.

**3. Propagate failure from `move_to_recently_completed` in `cmd_set` (satisfies C1, C4, AC#2, AC#3)**

Add an `else` branch at lines 416-417 in `cmd_set`:

```python
    if new_status in COMPLETION_STATUSES:
        emoji = "\u2705" if new_status == "DONE" else "\u274c"
        if move_to_recently_completed(index_lines, fid, emoji):
            print("Index: Active -> Recently Completed")
        else:
            print(
                f"ERROR: Failed to move F{fid} to Recently Completed. "
                "Feature row not found in Active Features section.",
                file=sys.stderr,
            )
            return 1
```

This adds one new `return 1`, bringing the total to 5 (satisfying AC#2 `gte 5`). The error message matches `ERROR.*Recently Completed` (satisfying AC#3).

**4. Unit test file `src/tools/python/test_feature_status.py` (satisfies AC#1, AC#5, AC#7, AC#9, AC#10)**

Use `unittest` with in-memory fixtures (list-of-strings representing index content). The `ac_ops` module-level import in `feature-status.py` is handled by mocking or by importing only the target functions directly. Tests use `sys.path` manipulation to load `feature-status.py` from the same directory and import the two helper functions under test: `find_active_row` and `move_to_recently_completed`.

Six test functions covering all C6 edge cases:

| Test Function | C6 Edge Case |
|--------------|-------------|
| `test_feature_not_in_active` | (1) feature not in Active → returns None |
| `test_feature_in_rc_only` | (2) feature in RC but not Active → section scoping returns None |
| `test_feature_in_both_active_and_rc` | (3) feature in both → move succeeds, removes from Active only |
| `test_missing_rc_section` | (4) no RC section header → returns False, Active row preserved |
| `test_empty_active_table` | (5) Active section with no data rows → returns None/False |
| `test_successful_move` | (6) normal happy-path → returns True, row in RC, not in Active |

The test file uses `unittest.mock.patch` on `ac_ops` before the import to handle the module-level import in `feature-status.py`.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `src/tools/python/test_feature_status.py` |
| 2 | Add `else: return 1` in `cmd_set` after failed `move_to_recently_completed`; existing 4 + 1 new = 5 total |
| 3 | Error message `"ERROR: Failed to move F{fid} to Recently Completed..."` printed to stderr on failure |
| 4 | Rewrite `move_to_recently_completed` so `lines.pop(row_idx)` only executes after `lines.insert(insert_idx, rc_row)` — old pattern `lines\.pop\(row_idx\)` before RC insertion is removed |
| 5 | `test_successful_move` runs the happy path through `move_to_recently_completed` returning True; all tests pass via pytest |
| 6 | Introduce `find_active_row` function; call it in `move_to_recently_completed` — at least 1 reference to `find_active_row` will match the grep pattern |
| 7 | Six `def test_` functions in the test file covering all C6 scenarios |
| 8 | No syntax errors introduced — verified by `python -m py_compile` |
| 9 | Test file syntactically valid — verified by `python -m py_compile` |
| 10 | All six unit tests pass via `python -m pytest` |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Atomic insert-then-pop ordering | A: pop-then-insert (current, buggy), B: insert-then-pop (corrected), C: copy-filter without mutation | B: insert-then-pop | Minimally invasive; Active section always precedes RC in the file so `row_idx < insert_idx` — no index arithmetic needed beyond confirming insert_idx exists first |
| Section-scoped search: new function vs parameter | A: new `find_active_row` function, B: add `section` parameter to `find_index_row` | A: new function | Avoids changing signature of `find_index_row` which is called elsewhere (line 206 was only caller in move path; `update_index_status` uses its own loop). Cleaner ISP: each function has single responsibility |
| AC#4 `not_matches` test validity | The old pattern `lines\.pop\(row_idx\)` still appears in the rewritten code (but after RC insertion) | The new `lines.pop(row_idx)` is in a new semantic context, but the literal string `lines.pop(row_idx)` remains in the source | Flag as upstream issue — AC#4 `not_matches` pattern is too broad; it tests absence of the literal string but the fix moves pop to AFTER insert, not eliminates it entirely |
| Test isolation for `ac_ops` import | A: mock `ac_ops` at sys.modules before import, B: extract helpers to separate module, C: test only the two functions by importing at function level | A: mock at sys.modules | Non-invasive to production code; `unittest.mock.patch.dict(sys.modules, {'ac_ops': MagicMock()})` before `import feature_status` covers the module-level import |
| Error output destination (stdout vs stderr) | A: print to stderr, B: print to stdout | A: stderr | Consistent with existing error messages in `cmd_set` (lines 384, 392 both use `file=sys.stderr`) |

### Interfaces / Data Structures

No new interfaces. The change adds one new module-level function and modifies two existing functions in `feature-status.py`.

**New function signature:**
```python
def find_active_row(lines: list[str], fid: str) -> int | None:
    """Find the line index of a feature row within the Active Features section only.

    Searches only between '## Active Features' and the next '## ' section header.
    Returns None if the feature is not found in the Active Features section.
    """
```

**Modified function — `move_to_recently_completed`**: Same signature, new body that uses `find_active_row` and atomic insert-then-pop ordering.

**Modified function — `cmd_set`**: Same signature; `else: return 1` branch added after `move_to_recently_completed` call.

**Test fixture pattern** (minimal valid index snapshot):
```python
FIXTURE_BOTH_SECTIONS = [
    "## Active Features\n",
    "\n",
    "| ID | Status | Name | Depends On | Links |\n",
    "|:---|:------:|:-----|:-----------|:------|\n",
    "| F999 | [DONE] | Test Feature | - | |\n",
    "\n",
    "## Recently Completed\n",
    "\n",
    "| ID | Status | Name | Links |\n",
    "|:---|:------:|:-----|:------|\n",
]
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#4 `not_matches` pattern `lines\.pop\(row_idx\)` is over-broad: the fixed code still contains `lines.pop(row_idx)` (but after RC insertion is confirmed, making it safe). The pattern tests absence of the literal string, which will fail even on the correctly-fixed code. | AC Definition Table, AC Details (AC#4) | Change AC#4 Expected pattern to `lines\.pop\(row_idx\)` with Matcher `not_matches` targeting only the destructive-first context, OR rewrite AC#4 to check for the presence of `lines\.insert\(insert_idx` appearing BEFORE `lines\.pop\(row_idx\)` in the function body. Simpler fix: change AC#4 to `matches` on `lines\.insert\(insert_idx,` to verify the insert-first pattern exists. |

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "SSOT tool for atomic feature status transitions" | Insert-then-pop must be atomic -- RC insertion confirmed before Active row removal | AC#4 |
| "When it reports success, the Active and Recently Completed tables must be consistent" | Non-zero exit code when move fails, so callers detect inconsistency | AC#2, AC#3 |
| "no [DONE] entries in Active, no missing entries in Recently Completed" | Section-scoped search prevents cross-section false matches | AC#6 |
| "Silent failures in this tool undermine the entire feature lifecycle's integrity" | Error message printed on failure; exit code propagated | AC#2, AC#3 |
| "SSOT tool for atomic feature status transitions" | Modified tool must remain syntactically valid and all tests must pass | AC#8, AC#9, AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Test file exists for feature-status.py | file | Glob(src/tools/python/test_feature_status*.py) | exists | - | [ ] |
| 2 | cmd_set has error path for move failure (return 1 count increased) | code | Grep(path=src/tools/python/feature-status.py, pattern=return 1) | gte | 5 | [ ] |
| 3 | Error message printed when move_to_recently_completed fails | code | Grep(path=src/tools/python/feature-status.py, pattern=ERROR.*Recently Completed|failed.*move) | matches | - | [ ] |
| 4 | Atomic insert-then-pop: RC insertion precedes Active row removal | code | Grep(path=src/tools/python/feature-status.py, pattern=lines\.insert\(insert_idx) | matches | - | [ ] |
| 5 | Success path preserved: exit 0 on valid DONE transition | exit_code | python src/tools/python/test_feature_status.py | succeeds | 0 | [ ] |
| 6 | Section-scoped search: move uses Active-only row lookup | code | Grep(path=src/tools/python/feature-status.py, pattern=find_active_row|active_section|section.*Active) | gte | 1 | [ ] |
| 7 | Unit tests cover minimum 6 edge case scenarios | code | Grep(path=src/tools/python/test_feature_status*.py, pattern=def test_) | gte | 6 | [ ] |
| 8 | feature-status.py has no syntax errors | exit_code | python -m py_compile src/tools/python/feature-status.py | succeeds | 0 | [ ] |
| 9 | Test file has no syntax errors | exit_code | python -m py_compile src/tools/python/test_feature_status.py | succeeds | 0 | [ ] |
| 10 | All unit tests pass | exit_code | python -m pytest src/tools/python/test_feature_status.py -v | succeeds | 0 | [ ] |

### AC Details

**AC#2: cmd_set has error path for move failure (return 1 count increased)**
- **Test**: `Grep(path=src/tools/python/feature-status.py, pattern="return 1")` count >= 5
- **Expected**: `gte 5`
- **Rationale**: Currently 4 `return 1` statements exist (lines 387, 394, 663, 678). The fix must add at least 1 new `return 1` in the completion path when `move_to_recently_completed` fails. Total >= 5.
- **Derivation**: 4 existing `return 1` + 1 new for move failure path = 5 minimum.

**AC#4: Atomic insert-then-pop: RC insertion precedes Active row removal**
- **Test**: `Grep(path=src/tools/python/feature-status.py, pattern="lines\\.insert\\(insert_idx")`
- **Expected**: Pattern `lines\.insert\(insert_idx` must match — the insert-first pattern confirms RC row is added before Active row is removed
- **Rationale**: Current code pops the Active row at line 212 before RC insertion is attempted at lines 217-234. If RC insertion fails, the row is permanently lost. The fix must insert the RC row first using a pre-computed `insert_idx`, then remove the Active row. The presence of `lines.insert(insert_idx,` confirms the atomic ordering.

**AC#6: Section-scoped search: move uses Active-only row lookup**
- **Test**: `Grep(path=src/tools/python/feature-status.py, pattern="find_active_row|active_section|section.*Active")`
- **Expected**: `gte 1`
- **Rationale**: C3 constraint requires that `find_index_row` (or its replacement) in the completion path only searches the Active Features section, not the entire index file. The fix must introduce section-aware logic -- either a new function (`find_active_row`) or section boundary parameters. At least 1 reference to this logic must exist.
- **Derivation**: 1 occurrence minimum -- the move_to_recently_completed function must use section-scoped lookup instead of whole-file `find_index_row`.

**AC#7: Unit tests cover minimum 6 edge case scenarios**
- **Test**: `Grep(path=src/tools/python/test_feature_status*.py, pattern="def test_")` count >= 6
- **Expected**: `gte 6`
- **Rationale**: C6 constraint enumerates 6 edge cases: (1) feature not in Active, (2) feature in RC only, (3) feature in both Active and RC, (4) missing RC section header, (5) empty Active table, (6) normal successful move. Each requires at least one test function.
- **Derivation**: 6 edge cases from C6 constraint Collection Members, 1:1 mapping to test functions.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Return non-zero exit code when move_to_recently_completed fails | AC#2 |
| 2 | Make pop+insert operation atomic (only remove from Active after RC insertion confirmed) | AC#4 |
| 3 | Scope find_index_row to Active Features section for completion transitions | AC#6 |
| 4 | Add unit test coverage for the completion path | AC#1, AC#5, AC#7, AC#9, AC#10 |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| None | - | - | F846 has no dependencies |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | INFORMATIONAL | F{ID} is waiting for this feature. |
| Related | Bidirectional | INFORMATIONAL | Related but not blocking. |
-->

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 6 | Add `find_active_row(lines, fid)` function to `src/tools/python/feature-status.py` — searches only between `## Active Features` and the next `## ` section header, returns `int | None` | | [ ] |
| 2 | 4 | Rewrite `move_to_recently_completed` in `feature-status.py` to use insert-then-pop atomic ordering: locate RC insertion point first, call `lines.insert(insert_idx, rc_row)`, then `lines.pop(row_idx)`; use `find_active_row` for Active-only lookup; return `False` without touching Active if no RC insertion point found | | [ ] |
| 3 | 2, 3 | Add `else: return 1` branch in `cmd_set` after failed `move_to_recently_completed` call, printing error message matching `ERROR.*Recently Completed` to `sys.stderr` | | [ ] |
| 4 | 1, 7, 9 | Create `src/tools/python/test_feature_status.py` with 6 `def test_` functions covering all C6 edge cases: (1) feature not in Active, (2) feature in RC only, (3) feature in both Active and RC, (4) missing RC section header, (5) empty Active table, (6) normal successful move; mock `ac_ops` via `unittest.mock.patch.dict(sys.modules, {'ac_ops': MagicMock()})` before import | | [ ] |
| 5 | 5, 8, 10 | Verify `python -m py_compile src/tools/python/feature-status.py` exits 0, then run `python -m pytest src/tools/python/test_feature_status.py -v` and confirm all 6 tests pass | | [ ] |

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
| 1 | implementer | sonnet | `src/tools/python/feature-status.py` | `find_active_row` function added; `move_to_recently_completed` rewritten with atomic insert-then-pop; `cmd_set` updated with `else: return 1` error branch |
| 2 | implementer | sonnet | `src/tools/python/feature-status.py` (modified) | `src/tools/python/test_feature_status.py` created with 6 unit test functions |
| 3 | tester | sonnet | `src/tools/python/feature-status.py`, `src/tools/python/test_feature_status.py` | `python -m py_compile` exits 0 for both files; `python -m pytest` exits 0 with all 6 tests passing |

### Pre-conditions

- `src/tools/python/feature-status.py` exists and is the current production file
- Python 3.x available in PATH
- `pytest` available (or installable via `pip install pytest`)
- No other in-flight modifications to `feature-status.py`

### Execution Order

**Phase 1 — Surgical changes to `feature-status.py` (Tasks 1, 2, 3)**

Step 1.1 — Add `find_active_row` function (Task 1):
- Insert new module-level function `find_active_row(lines: list[str], fid: str) -> int | None` (signature from Technical Design "Interfaces / Data Structures")
- Placement: immediately before `move_to_recently_completed` in the file
- Function searches lines between `## Active Features` (exact match on `stripped == "## Active Features"`) and the next line where `stripped.startswith("## ")` — breaks on that line
- Returns the integer index of the matching row, or `None` if not found
- Pattern: `re.compile(r"^\|\s*F?" + fid + r"\s*\|")` — matches both `F{fid}` (Active format) and bare `{fid}` (RC format, not that it matters here)

Step 1.2 — Rewrite `move_to_recently_completed` (Task 2):
- Replace existing body with atomic insert-then-pop implementation (from Technical Design "Approach" section 2)
- Step 1: call `find_active_row(lines, fid)` — if `None`, return `False` immediately (Active row preserved)
- Step 2: build `rc_row` from `removed_line` title extraction
- Step 3: scan for RC insertion point (`insert_idx`) by finding `## Recently Completed` then the first non-header, non-empty, non-comment row after it
- Step 4: if `insert_idx is None`, return `False` — Active row preserved (no modification to `lines`)
- Step 5: `lines.insert(insert_idx, rc_row)` — RC row inserted first
- Step 6: `lines.pop(row_idx)` — Active row removed after RC insertion confirmed (`row_idx < insert_idx` always holds because Active section precedes RC section in the file)
- Step 7: return `True`

Step 1.3 — Update `cmd_set` error path (Task 3):
- Locate the `if move_to_recently_completed(index_lines, fid, emoji):` block (lines 416-417 per Technical Design)
- Current code: only `print("Index: Active -> Recently Completed")` on success, no else branch
- Add `else:` branch: print error message `f"ERROR: Failed to move F{fid} to Recently Completed. Feature row not found in Active Features section."` to `file=sys.stderr`, then `return 1`
- Pattern must match `ERROR.*Recently Completed` (AC#3) and must not match `failed.*move` (both covered by the message)
- This adds 1 new `return 1`, bringing total to 5 (satisfies AC#2 `gte 5`)

**Phase 2 — Create test file (Task 4)**

Step 2.1 — Create `src/tools/python/test_feature_status.py`:
- File must begin with `sys.modules` mock for `ac_ops` BEFORE importing `feature_status`:
  ```python
  import sys
  from unittest.mock import MagicMock, patch
  with patch.dict(sys.modules, {'ac_ops': MagicMock()}):
      import feature_status  # or importlib approach
  ```
  (Exact import pattern per Key Decision "Test isolation for `ac_ops` import" — Selected: A: mock at sys.modules)
- Implement 6 test functions named `test_*` covering C6 edge cases exactly:
  1. `test_feature_not_in_active` — feature ID not present anywhere in Active section → `find_active_row` returns `None`
  2. `test_feature_in_rc_only` — feature present in RC section only, not in Active → section-scoped `find_active_row` returns `None` (cross-section false-match prevention)
  3. `test_feature_in_both_active_and_rc` — feature in both sections → `move_to_recently_completed` returns `True`, Active row removed, RC row not duplicated
  4. `test_missing_rc_section` — no `## Recently Completed` header in fixture → `move_to_recently_completed` returns `False`, Active row preserved
  5. `test_empty_active_table` — Active section exists but contains no data rows → `find_active_row` returns `None` / `move_to_recently_completed` returns `False`
  6. `test_successful_move` — normal happy-path fixture (from Technical Design "Interfaces / Data Structures" `FIXTURE_BOTH_SECTIONS`) → `move_to_recently_completed` returns `True`, row present in RC, not in Active
- Use `unittest.TestCase` or plain pytest functions; fixtures use in-memory `list[str]` (no file I/O)

**Phase 3 — Verification (Task 5)**

Step 3.1: `python -m py_compile src/tools/python/feature-status.py` — must exit 0 (AC#8)

Step 3.2: `python -m py_compile src/tools/python/test_feature_status.py` — must exit 0 (AC#9)

Step 3.3: `python -m pytest src/tools/python/test_feature_status.py -v` — must exit 0 with all 6 tests passing (AC#5, AC#10)

### Success Criteria

- `find_active_row` function exists in `feature-status.py` (AC#6 `gte 1` grep match)
- `lines.insert(insert_idx,` pattern present in `feature-status.py` (AC#4)
- `return 1` count in `feature-status.py` is 5 or more (AC#2)
- Error message matching `ERROR.*Recently Completed` present in `feature-status.py` (AC#3)
- `test_feature_status.py` file exists (AC#1)
- `test_feature_status.py` contains at least 6 `def test_` functions (AC#7)
- Both files pass `python -m py_compile` (AC#8, AC#9)
- All 6 unit tests pass via `pytest` (AC#5, AC#10)

### Error Handling

- If `pytest` is not installed: `pip install pytest` then retry Step 3.3
- If import of `feature_status` in tests fails due to `ac_ops` not being in path: verify `sys.modules` mock is applied before the import statement
- If `find_active_row` is defined but `move_to_recently_completed` still calls old `find_index_row`: search for and replace the call site inside `move_to_recently_completed` specifically
- If `row_idx >= insert_idx` assertion fails (Active section after RC in file): STOP and report to user — index file structure has changed

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert HEAD`
2. Notify user of rollback
3. Create follow-up feature for fix

---

## Mandatory Handoffs

<!-- CRITICAL: Read .claude/reference/deferred-task-protocol.md Option B Guards BEFORE adding entries.
- Option A (Mandatory Handoff): Out-of-scope issue tracked here. REQUIRES: Destination + ID.
- Option B (New Feature): Separate feature-{ID}.md created. REQUIRES: ID assigned.
- Option C (Absorbed): Issue absorbed into current feature scope. REQUIRES: AC coverage.
- Validation: Every entry MUST have non-empty Destination ID (no "TBD" allowed).
- DRAFT Creation: If creating a new feature during /fc, create [DRAFT] immediately (not deferred). -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

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

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

## Links

[Related: F842](feature-842.md) - Parent feature — CodeRabbit review of F842 commit discovered this bug
[Related: F828](feature-828.md) - Affected feature — was [DONE] in Active only, not moved to RC
[Related: F843](feature-843.md) - Affected feature — was in both Active and RC simultaneously
