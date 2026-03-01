# Feature 798: ac-static-verifier Regex and GTE Parser Fixes

## Status: [DONE]
<!-- fl-reviewed: 2026-02-21T00:08:13Z -->

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
| Parent Feature | F794 |
| Discovery Phase | Phase 9 |
| Timestamp | 2026-02-21 |

### Observable Symptom
ac-static-verifier.py reports 3 false FAIL results for correctly implemented ACs: AC#6 and AC#7 (count_equals with `^\s+` anchored regex returns actual_count=0 despite interface files containing matching lines), and AC#34 (gte matcher fails to parse `>= N` format from AC Expected column).

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python src/tools/python/ac-static-verifier.py --feature 794 --ac-type code` |
| Exit Code | 1 |
| Error Output | `AC#6: expected_count=7, actual_count=0; AC#7: expected_count=11, actual_count=0; AC#34: Expected value must be in 'pattern = N' format for gte matcher` |
| Expected | 29/29 PASS (all ACs verified by ac-tester) |
| Actual | 26/29 PASS, 3 FAIL (tool parsing issues) |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Contains regex matching and gte parser logic |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | - |

### Parent Session Observations
The `^\s+` anchored pattern works correctly via Claude Code's Grep tool but fails in ac-static-verifier.py's regex engine. The gte matcher expects `pattern = N` format but the AC table uses `pattern >= N` format. Both are parser limitations, not implementation issues.

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Pipeline Continuity - ac-static-verifier is the SSOT for automated AC verification across all features. Its regex engine and format parser must correctly handle all AC pattern formats defined in the testing SKILL, ensuring that phase completion gates produce accurate PASS/FAIL results without false negatives.

### Problem (Current Issue)
ac-static-verifier.py produces false FAIL results for 3 of F794's ACs because of two independent bugs in its verification engine. First, `re.findall(search_pattern, content)` at `src/tools/python/ac-static-verifier.py:621` does not pass the `re.MULTILINE` flag, causing `^`-anchored patterns (like `^\s+(void|int)\s+\w+\(`) to match only at position 0 of the file string rather than at the start of each line. Since `Era.Core/Interfaces/IBodySettings.cs` begins with `namespace` (not whitespace), the pattern returns zero matches despite 7 qualifying method declarations at lines 11-18. Second, the Format A regex parser at `src/tools/python/ac-static-verifier.py:588` only recognizes `= N` format (`` `pattern` = N ``), but `gte`/`gt`/`lt`/`lte` matchers naturally use comparison operator notation (`>= N`, `> N`, etc.) in AC Expected columns. AC#34's Expected value `` `\[(Fact|Theory)\]` >= 4 `` fails to match either Format A or Format B, triggering the error branch at lines 600-610. Additionally, the same `re.MULTILINE` gap exists in `re.search` calls for the `matches` (line 521) and `not_matches` (line 551) matchers, representing a latent defect that would surface when any future AC uses `^`-anchored patterns with these matchers.

### Goal (What to Achieve)
Fix the ac-static-verifier regex engine and format parser so that: (1) all `re.findall` and `re.search` calls include `re.MULTILINE` for correct `^`/`$` line-anchor semantics, (2) Format A parser accepts comparison operators (`>=`, `>`, `<=`, `<`) alongside `=`, and (3) all existing tests continue to pass (backward compatibility).

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do AC#6 and AC#7 report actual_count=0? | `re.findall(search_pattern, content)` returns zero matches for `^`-anchored patterns | `src/tools/python/ac-static-verifier.py:621` |
| 2 | Why does `re.findall` return zero matches? | Without `re.MULTILINE`, Python treats `^` as matching only at the start of the entire string (position 0) | Python `re` module documentation |
| 3 | Why does position 0 fail? | `IBodySettings.cs` starts with `namespace`, not whitespace, so `^\s+` cannot match at position 0 | `Era.Core/Interfaces/IBodySettings.cs:1` |
| 4 | Why was `re.MULTILINE` not included? | The count matcher implementation was tested only with non-anchored patterns (`def foo\(`) that do not require line-start semantics | `src/tools/python/tests/test_ac_verifier_count_equals_content.py:287-324` |
| 5 | Why (Root)? | F792 designed the count matcher for the `count_equals` use case without anticipating that AC patterns would use `^`/`$` line anchors, and no test suite covers `^`-anchored patterns in any matcher | `src/tools/python/ac-static-verifier.py:572-621` (no `re.MULTILINE` anywhere in file) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 3 ACs report false FAIL (actual_count=0, format parse error) | Missing `re.MULTILINE` flag in regex calls; Format A parser only supports `= N` |
| Where | ac-static-verifier output for F794 ACs | `src/tools/python/ac-static-verifier.py` lines 521, 551, 588, 621 |
| Fix | Manually waive the 3 failed ACs | Add `re.MULTILINE` to all regex calls; extend Format A regex to accept comparison operators |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F794 | [DONE] | Parent -- deviation discovered during F794 Phase 9 AC verification |
| F792 | [DONE] | Introduced `count_equals`/`gte` matchers and Format A/B/C parsers containing the bugs |
| F782 | [DRAFT] | Indirect -- Post-Phase Review depends on F794; F798 unblocks complete ac-static-verifier usage |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Root cause identified | FEASIBLE | Two distinct bugs traced to specific lines (521, 551, 588, 621) |
| Fix complexity | FEASIBLE | Single-flag additions (3 call sites) and regex extension (1 regex) |
| Backward compatibility | FEASIBLE | `re.MULTILINE` only affects `^`/`$` semantics; format extension is additive |
| Test coverage | FEASIBLE | 21+ existing test files provide regression safety net |
| No external dependencies | FEASIBLE | Pure Python `re` module (standard library) |
| Scope containment | FEASIBLE | All changes confined to `src/tools/python/ac-static-verifier.py` and its test files |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| ac-static-verifier regex engine | HIGH | Enables correct `^`/`$` line-anchor matching for all matchers (count, matches, not_matches) |
| AC Expected format parser | MEDIUM | Enables natural operator notation (`>= N`, `> N`, etc.) for comparison matchers |
| Existing AC verification | LOW | No regression; `re.MULTILINE` does not affect non-anchored patterns or `.` (dot) behavior |
| Future features | MEDIUM | Unblocks AC authors from using `^`/`$` anchored patterns freely |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `re.MULTILINE` changes `^` and `$` semantics for all patterns using them | Python `re` module | Must verify no existing ACs rely on `^` matching only at string start |
| Format A regex must remain backward-compatible with `= N` format | `src/tools/python/tests/test_ac_verifier_count_equals_content.py:287-324` | Extended regex must still match existing `` `pattern` = N `` format |
| Existing 21+ test files (120+ tests) must pass after changes | `src/tools/python/tests/test_ac_verifier_*.py` | Full regression safety net required |
| `re.MULTILINE` does not affect `.` (dot) behavior | Python `re` module | Only `^` and `$` anchors are affected; no risk to `.`-based patterns |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| `re.MULTILINE` changes behavior for existing `$`-anchored patterns | LOW | MEDIUM | Search all feature files for `$` in Expected columns; verify no ACs depend on end-of-string matching |
| Format A regex extension matches unintended patterns | LOW | LOW | Use precise regex with optional comparison operator; backtick boundary prevents ambiguity |
| Operator format ambiguity (e.g., `= 5` vs `>= 5` for count_equals) | LOW | MEDIUM | Matcher column already specifies comparison type; operator in Expected is notation only |
| Missing additional regex call sites beyond the 3 identified | LOW | MEDIUM | Full-file grep for `re.search` and `re.findall` to confirm all sites covered |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| F794 AC verification | `python src/tools/python/ac-static-verifier.py --feature 794 --ac-type code` | 26/29 PASS, 3 FAIL | AC#6, AC#7, AC#34 are false FAILs |
| Existing test suite | `python -m pytest src/tools/python/tests/test_ac_verifier_*.py` | All PASS | Regression baseline |

**Baseline File**: `.tmp/baseline-798.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `re.MULTILINE` must be added to all three regex call sites (lines 521, 551, 621) | Python `re` semantics; investigation evidence | AC must verify `^`-anchored patterns return correct counts and correct matches/not_matches results |
| C2 | Format A parser must accept `>=`, `>`, `<=`, `<` operators in addition to `=` | AC#34 Expected format evidence (`src/tools/python/ac-static-verifier.py:588`) | AC must verify each comparison operator format parses correctly |
| C3 | Existing Format A (`= N`) and Format B (`Pattern (N)`) must remain functional | Backward compatibility requirement | AC must include regression tests for both existing formats |
| C4 | `matches`/`not_matches` matchers must support `^`-anchored patterns via MULTILINE | Latent defect at lines 521, 551 | AC must verify `^`-anchored patterns work with matches and not_matches matchers |
| C5 | Operator in Expected is informational; matcher column determines comparison semantics | Design consistency | AC should verify that operator notation does not override matcher behavior |

### Constraint Details

**C1: re.MULTILINE Flag Addition**
- **Source**: All 3 investigations independently identified missing `re.MULTILINE` at lines 521, 551, 621
- **Verification**: `grep -n "re.MULTILINE" src/tools/python/ac-static-verifier.py` currently returns zero results
- **AC Impact**: AC must create test content with `^`-anchored pattern targeting non-first-line content and verify correct count/match behavior

**C2: Format A Comparison Operator Extension**
- **Source**: F794 AC#34 uses `` `\[(Fact|Theory)\]` >= 4 `` which fails Format A regex `r'^`(.+)`\s*=\s*(\d+)$'`
- **Verification**: Run ac-static-verifier against F794 AC#34 to confirm parse error
- **AC Impact**: AC must test each operator format (`>= N`, `> N`, `<= N`, `< N`) alongside existing `= N`

**C3: Backward Compatibility**
- **Source**: 21+ existing test files with 120+ tests depend on current `= N` and `Pattern (N)` formats
- **Verification**: Run full test suite after changes
- **AC Impact**: AC must verify existing format examples still parse correctly

**C4: matches/not_matches MULTILINE Support**
- **Source**: Same root cause as C1; `re.search` at lines 521, 551 lacks `re.MULTILINE`
- **Verification**: Create test with `^`-anchored pattern that matches only at non-first-line position
- **AC Impact**: AC must verify matches returns True and not_matches returns False for `^`-anchored pattern targeting mid-file content

**C5: Operator-Matcher Consistency**
- **Source**: Design question raised by all 3 investigators: operator in Expected is notation, not logic
- **Verification**: Verify that `gte` matcher with `= N` format and `gte` matcher with `>= N` format produce identical results
- **AC Impact**: AC should confirm that the comparison operator in Expected does not alter matcher behavior

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F794 | [DONE] | Parent feature -- deviation discovered during F794 Phase 9 |
| Related | F792 | [DONE] | Introduced count_equals/gte matchers and Format A/B/C parsers containing the bugs |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
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
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "ac-static-verifier is the SSOT for automated AC verification" | Regex engine must correctly handle all `^`/`$` line-anchor patterns so PASS/FAIL results are accurate | AC#1, AC#2, AC#3, AC#4 |
| "format parser must correctly handle all AC pattern formats" | Format A parser must accept comparison operators (`>=`, `>`, `<=`, `<`) alongside `=` | AC#5, AC#6, AC#7 |
| "phase completion gates produce accurate PASS/FAIL results without false negatives" | Existing tests and formats must continue working (backward compatibility) | AC#8, AC#9, AC#10 |
| "ac-static-verifier is the SSOT for automated AC verification" | All `re.search` and `re.findall` calls must include `re.MULTILINE` flag | AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | count_equals with `^`-anchored pattern returns correct count (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_count_equals_caret_anchor_multiline | succeeds | 0 | [x] |
| 2 | matches with `^`-anchored pattern returns True for mid-file content (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_matches_caret_anchor_multiline | succeeds | 0 | [x] |
| 3 | not_matches with `^`-anchored pattern returns False when pattern exists mid-file (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_not_matches_caret_anchor_multiline | succeeds | 0 | [x] |
| 4 | `$`-anchored pattern works correctly with MULTILINE (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_dollar_anchor_multiline | succeeds | 0 | [x] |
| 5 | Format A parser accepts `>= N` operator (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_gte_operator | succeeds | 0 | [x] |
| 6 | Format A parser accepts `> N`, `<= N`, `< N` operators (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_all_comparison_operators | succeeds | 0 | [x] |
| 7 | Format A operator does not override matcher semantics (Neg) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_operator_does_not_override_matcher | succeeds | 0 | [x] |
| 8 | Existing Format A `= N` backward compatibility (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_equals_backward_compat | succeeds | 0 | [x] |
| 9 | Existing Format B `Pattern (N)` backward compatibility (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_b_backward_compat | succeeds | 0 | [x] |
| 10 | Full existing test suite regression (Pos) | exit_code | pytest src/tools/python/tests/test_ac_verifier_*.py | succeeds | 0 | [x] |
| 11 | re.MULTILINE present at all three call sites | code | Grep(src/tools/python/ac-static-verifier.py) | count_equals | `re\.MULTILINE` = 3 | [x] |

### AC Details

**AC#1: count_equals with `^`-anchored pattern returns correct count (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_count_equals_caret_anchor_multiline`
- **Expected**: Test creates a multi-line file where `^`-anchored pattern (e.g., `^\s+(void|int)`) matches lines that are NOT on the first line. The count_equals matcher must return the correct count (>0), not 0.
- **Rationale**: C1 requires `re.MULTILINE` at line 621 (`re.findall`). Without it, `^` only matches at string position 0, producing actual_count=0 for patterns targeting indented method declarations. This directly reproduces the F794 AC#6/AC#7 false FAIL.

**AC#2: matches with `^`-anchored pattern returns True for mid-file content (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_matches_caret_anchor_multiline`
- **Expected**: Test creates a file where a `^`-anchored pattern matches content on a non-first line. The `matches` matcher must return PASS (pattern_found=True).
- **Rationale**: C4 requires `re.MULTILINE` at line 521 (`re.search` for matches). Without it, `^`-anchored patterns cannot match mid-file line starts, producing false FAIL.

**AC#3: not_matches with `^`-anchored pattern returns False when pattern exists mid-file (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_not_matches_caret_anchor_multiline`
- **Expected**: Test creates a file where a `^`-anchored pattern matches content on a non-first line. The `not_matches` matcher must return FAIL (pattern_found=True, so not_matches = FAIL), confirming the pattern was correctly found.
- **Rationale**: C4 requires `re.MULTILINE` at line 551 (`re.search` for not_matches). Without it, the pattern would not be found, causing not_matches to incorrectly return PASS (false negative).

**AC#4: `$`-anchored pattern works correctly with MULTILINE (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_dollar_anchor_multiline`
- **Expected**: Test creates a multi-line file and uses a `$`-anchored pattern to match end-of-line content that is not at the end of the entire file string. The count_equals matcher returns the correct count.
- **Rationale**: `re.MULTILINE` changes both `^` and `$` semantics. This AC verifies `$` anchor correctness alongside `^`, ensuring complete line-anchor support per C1.

**AC#5: Format A parser accepts `>= N` operator (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_gte_operator`
- **Expected**: Test uses Format A Expected value `` `pattern` >= N `` with `gte` matcher. The parser extracts the pattern and numeric value correctly, returning PASS when actual count >= N.
- **Rationale**: C2 requires Format A to accept `>=` operator. This directly reproduces the F794 AC#34 false FAIL where `` `\[(Fact|Theory)\]` >= 4 `` triggered a parse error.

**AC#6: Format A parser accepts `> N`, `<= N`, `< N` operators (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_all_comparison_operators`
- **Expected**: Test verifies each comparison operator format (`` `pattern` > N ``, `` `pattern` <= N ``, `` `pattern` < N ``) parses correctly and the corresponding matcher logic produces the correct PASS/FAIL result.
- **Rationale**: C2 requires all comparison operators. AC#5 covers `>=`; this AC covers the remaining three operators to ensure complete Format A extension.

**AC#7: Format A operator does not override matcher semantics (Neg)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_operator_does_not_override_matcher`
- **Expected**: (Positive equivalence) Test uses `gte` matcher with Format A `` `pattern` = N `` (equals sign, not `>=`). The gte comparison logic still applies correctly (i.e., operator in Expected is informational notation, matcher column determines comparison). Both `` `pattern` = 5 `` and `` `pattern` >= 5 `` with `gte` matcher and actual_count=5 produce the same PASS result. (Negative conflict) Test uses `lt` matcher with Format A `` `pattern` >= 5 `` and actual_count=5. Since `lt` means actual < threshold, this should FAIL (5 is not < 5), even though `>=` notation would suggest PASS. This confirms the operator token is correctly discarded when it contradicts the matcher column.
- **Rationale**: C5 requires operator notation does not override matcher behavior. The matcher column is the semantic authority; the operator in Expected is cosmetic. The conflict case (lt + >= notation) is the only way to prove the operator is truly discarded.

**AC#8: Existing Format A `= N` backward compatibility (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_a_equals_backward_compat`
- **Expected**: Test uses the existing `` `pattern` = N `` format with `count_equals` matcher. Parser extracts pattern and count correctly, returning PASS when actual count equals N.
- **Rationale**: C3 requires backward compatibility for existing `= N` format. This ensures the Format A regex extension does not break the original format.

**AC#9: Existing Format B `Pattern (N)` backward compatibility (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_multiline.py::test_format_b_backward_compat`
- **Expected**: Test uses the existing `Pattern (N)` format with `count_equals` matcher. Parser extracts pattern and count correctly, returning PASS when actual count equals N.
- **Rationale**: C3 requires backward compatibility for existing Format B. This ensures changes to Format A regex do not inadvertently break Format B parsing.

**AC#10: Full existing test suite regression (Pos)**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_*.py`
- **Expected**: All existing 120+ tests across 21+ test files pass with exit code 0.
- **Rationale**: C3 backward compatibility. The full test suite regression ensures no existing behavior is broken by the `re.MULTILINE` addition or Format A regex extension.

**AC#11: re.MULTILINE present at all three call sites**
- **Test**: Grep pattern=`re\.MULTILINE` path="src/tools/python/ac-static-verifier.py"
- **Expected**: Exactly 3 occurrences (lines 521, 551, 621 — the three regex call sites identified in investigation)
- **Rationale**: C1 requires `re.MULTILINE` at all three call sites. This static verification ensures the flag is added to every site without over- or under-application. Currently 0 occurrences exist (baseline).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | All `re.findall` and `re.search` calls include `re.MULTILINE` for correct `^`/`$` line-anchor semantics | AC#1, AC#2, AC#3, AC#4, AC#11 |
| 2 | Format A parser accepts comparison operators (`>=`, `>`, `<=`, `<`) alongside `=` | AC#5, AC#6, AC#7 |
| 3 | All existing tests continue to pass (backward compatibility) | AC#8, AC#9, AC#10 |

---

<!-- fc-phase-4-completed -->

## Technical Design

### Approach

Two independent minimal fixes in `src/tools/python/ac-static-verifier.py`, plus a new test file.

**Fix 1 — Add `re.MULTILINE` to all three regex call sites:**

- Line 521: `re.search(pattern, content)` → `re.search(pattern, content, re.MULTILINE)`
- Line 551: `re.search(pattern, content)` → `re.search(pattern, content, re.MULTILINE)`
- Line 621: `re.findall(search_pattern, content)` → `re.findall(search_pattern, content, re.MULTILINE)`

`re.MULTILINE` causes `^` to match at the start of each line (not just position 0 of the string) and `$` to match at the end of each line (not just end of string). This has zero effect on patterns that do not use `^` or `$`, so all existing non-anchored patterns are fully backward-compatible.

**Fix 2 — Extend Format A regex to accept comparison operators:**

Line 588 current:
```python
format_a = re.match(r'^`(.+)`\s*=\s*(\d+)$', pattern)
```

Line 588 after fix:
```python
format_a = re.match(r'^`(.+)`\s*(?:>=|<=|>|<|=)\s*(\d+)$', pattern)
```

The non-capturing group `(?:>=|<=|>|<|=)` matches any of the five operator strings. Ordering matters: `>=` and `<=` must come before `>` and `<` to prevent the two-character operators from being partially consumed. The operator itself is discarded; only the regex pattern (group 1) and the numeric threshold (group 2) are extracted. The matcher column (`gte`, `gt`, `lte`, `lt`, `count_equals`) remains the sole authority on which comparison is applied at lines 645-654.

**Rationale for this approach:** The fixes are surgical (three one-liner changes and one regex change). No new abstractions, no architectural changes, no helper functions needed. The existing test infrastructure (`importlib.util` loading pattern) is reused verbatim in the new test file.

**New test file:** `src/tools/python/tests/test_ac_verifier_multiline.py` — 9 test functions covering ACs#1-9 (AC#10 is verified by running full test suite regression; AC#11 is verified by static Grep).

---

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Write `test_count_equals_caret_anchor_multiline`: create a temp file whose first line is `namespace Foo;` and lines 2-8 are `    void Bar()` style declarations. Call `verify_code_ac` with Format A `` `^\s+void ` = 7 `` and `count_equals` matcher. After Fix 1, `re.findall` with `re.MULTILINE` finds all 7 occurrences. Assert `result["result"] == "PASS"` and `actual_count == 7`. |
| 2 | Write `test_matches_caret_anchor_multiline`: create a temp file with `namespace X;\n    public void Method()`. Call `verify_code_ac` with matcher `matches` and pattern `^\s+public`. After Fix 1, `re.search` at line 521 with `re.MULTILINE` finds the second line. Assert `result["result"] == "PASS"`. |
| 3 | Write `test_not_matches_caret_anchor_multiline`: same file as AC#2. Call `verify_code_ac` with matcher `not_matches` and pattern `^\s+public`. After Fix 1, `re.search` at line 551 finds the match, so `not pattern_found` is False. Assert `result["result"] == "FAIL"` (pattern exists, so not_matches correctly FAILs). |
| 4 | Write `test_dollar_anchor_multiline`: create a temp file with lines ending in `{`, e.g. `class Foo {` and `void Bar() {`. Use Format A `` `\{$` = 2 `` with `count_equals`. After Fix 1, `re.findall` with `re.MULTILINE` treats `$` as end-of-line and counts both lines. Assert `actual_count == 2`, `result["result"] == "PASS"`. |
| 5 | Write `test_format_a_gte_operator`: create a temp file with 5 occurrences of `[Theory]`. Call `verify_code_ac` with matcher `gte` and expected `` `\[Theory\]` >= 4 ``. After Fix 2, Format A regex matches. Assert `result["result"] == "PASS"` (5 >= 4). |
| 6 | Write `test_format_a_all_comparison_operators`: parameterized loop over `> 3` (pass: 5>3), `<= 6` (pass: 5<=6), `< 10` (pass: 5<10) using matchers `gt`, `lte`, `lt` respectively. Each sub-case asserts PASS. Also test a failing boundary: `> 5` with `gt` and actual=5 asserts FAIL. |
| 7 | Write `test_format_a_operator_does_not_override_matcher`: create file with 5 occurrences. (Positive equivalence) Run `gte` matcher twice: once with `` `pattern` = 5 `` and once with `` `pattern` >= 5 ``. Assert both produce `result["result"] == "PASS"` and identical `actual_count`. (Negative conflict) Run `lt` matcher with `` `pattern` >= 5 `` and actual_count=5. Assert `result["result"] == "FAIL"` (5 is not < 5, proving operator `>=` is discarded and `lt` semantics apply). |
| 8 | Write `test_format_a_equals_backward_compat`: replicate existing `test_count_equals_format_a_backtick_regex` logic but in the new file. File has 2 `def foo(` occurrences, expected `` `def foo\(` = 2 ``, matcher `count_equals`. Assert PASS and `actual_count == 2`. |
| 9 | Write `test_format_b_backward_compat`: file has 3 occurrences of literal `Result<Unit>`, expected `Result<Unit> (3)`, matcher `count_equals`. Assert PASS and `actual_count == 3`. |
| 10 | Satisfied by running all tests with `pytest src/tools/python/tests/test_ac_verifier_*.py`. No dedicated test function; the AC is a run-level verification. The implementer must confirm the full suite passes after applying both fixes. |
| 11 | Static Grep verification: `Grep(src/tools/python/ac-static-verifier.py)` with pattern `re\.MULTILINE`, matcher `count_equals`, expected `` `re\.MULTILINE` = 3 ``. After Fix 1, exactly 3 occurrences exist (lines 521, 551, 621). |

---

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|--------------------|----------|-----------|
| Where to add `re.MULTILINE` | (A) Per-call flag argument; (B) compile pattern with `re.compile(..., re.MULTILINE)` before call sites | A — per-call flag | Minimal diff; patterns are single-use strings not reused elsewhere; `re.compile` would require refactoring all three call sites and the surrounding structure |
| Format A operator regex ordering | (A) `(?:>=\|<=\|>\|<\|=)`; (B) `(?:=\|>=\|<=\|>\|<)` | A | In Python regex alternation, the engine tries left-to-right and stops at first match. `>=` must precede `>` and `<=` must precede `<`; otherwise `>` would consume the first char of `>=`, leaving `=` unmatched. Option B would silently misparse `>=` as `=` with a leftover `>`. |
| What happens to the extracted operator token | (A) Use it to override matcher behavior; (B) Discard it (only group 1 and group 2 extracted) | B | C5 specifies that operator in Expected is notational; matcher column is semantic authority. Discarding is also safer: adding operator-override logic would change behavior for all existing ACs that use `=` with `gte` matcher. |
| Test file placement | (A) Add tests to existing `test_ac_verifier_count_equals_content.py`; (B) New `test_ac_verifier_multiline.py` | B | AC table specifies the new file name explicitly. Grouping by concern (multiline behavior vs. count_equals semantics) is cleaner and avoids churn to an existing file. |
| Handling `re.MULTILINE` effect on `$` | (A) Scope fix only to `^` patterns; (B) Include `re.MULTILINE` unconditionally at all sites | B | `re.MULTILINE` affects both `^` and `$`; applying it only conditionally would require detecting anchor usage in every pattern. Unconditional application is correct: `$` matching end-of-line is the natural behavior AC authors expect, and no existing AC uses `$` to match only end-of-string. |

---

### Interfaces / Data Structures

<!-- N/A: This feature makes no changes to public APIs, class signatures, or data structures. All changes are internal to the `_verify_content` method of `ACVerifier`. The `ACDefinition` dataclass, `ACVerifier` class interface, and JSON output schema are unchanged. -->

---

### Upstream Issues

<!-- No upstream issues found. -->
<!-- The three previously identified call sites (lines 521, 551, 621) are confirmed as the only `re.search`/`re.findall` calls in the file. A full-file search for `re.search` and `re.findall` should be performed by the implementer before closing, but based on reading lines 510-670 no additional sites exist in the verification logic. -->
<!-- AC#11 uses `= 3` which will only be satisfied after Fix 1 is applied. If a fourth call site were discovered during implementation, AC#11 would need to be updated. The implementer must flag this before submitting. -->

---

<!-- fc-phase-5-completed -->

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Confirm baseline: grep `re\.MULTILINE` in `src/tools/python/ac-static-verifier.py` returns 0 occurrences (prerequisite step, no AC — verified manually in Implementation Contract Phase 1) | | [x] |
| 2 | 1, 2, 3, 4, 11 | Add `re.MULTILINE` flag to `re.search` (lines 521 and 551) and `re.findall` (line 621) in `src/tools/python/ac-static-verifier.py` | | [x] |
| 3 | 5, 6, 7 | Extend Format A regex at line 588 of `src/tools/python/ac-static-verifier.py` to accept `>=`, `<=`, `>`, `<`, `=` operators via `(?:>=\|<=\|>\|<\|=)` non-capturing group; operator token is discarded (matcher column remains semantic authority per C5, verified by AC#7 conflict test) | | [x] |
| 4 | 1, 2, 3, 4, 5, 6, 7, 8, 9 | Create `src/tools/python/tests/test_ac_verifier_multiline.py` with 9 test functions: `test_count_equals_caret_anchor_multiline`, `test_matches_caret_anchor_multiline`, `test_not_matches_caret_anchor_multiline`, `test_dollar_anchor_multiline`, `test_format_a_gte_operator`, `test_format_a_all_comparison_operators`, `test_format_a_operator_does_not_override_matcher`, `test_format_a_equals_backward_compat`, `test_format_b_backward_compat` | | [x] |
| 5 | 10, 11 | Run `pytest src/tools/python/tests/test_ac_verifier_*.py` (full suite regression) and verify `re.MULTILINE` count = 3 via grep | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Task#1: grep `src/tools/python/ac-static-verifier.py` for `re.MULTILINE`; confirm 0 occurrences | Baseline confirmation (0 matches) |
| 2 | implementer | sonnet | Task#2: Edit `src/tools/python/ac-static-verifier.py` — add `re.MULTILINE` as third argument to `re.search` at lines 521 and 551, and to `re.findall` at line 621 | Modified `src/tools/python/ac-static-verifier.py` with 3 MULTILINE additions |
| 3 | implementer | sonnet | Task#3: Edit `src/tools/python/ac-static-verifier.py` — replace Format A regex at line 588 from `r'^`(.+)`\s*=\s*(\d+)$'` to `r'^`(.+)`\s*(?:>=\|<=\|>\|<\|=)\s*(\d+)$'` | Modified `src/tools/python/ac-static-verifier.py` with extended Format A regex |
| 4 | implementer | sonnet | Task#4: Create `src/tools/python/tests/test_ac_verifier_multiline.py` using the `importlib.util` loading pattern from existing test files; implement all 9 test functions per Technical Design AC Coverage section | New file `src/tools/python/tests/test_ac_verifier_multiline.py` with 9 passing tests |
| 5 | implementer | sonnet | Task#5: Run `pytest src/tools/python/tests/test_ac_verifier_*.py`; run `grep -c "re.MULTILINE" src/tools/python/ac-static-verifier.py` | pytest exit code 0; grep count = 3 |

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| AC#10 [B] PRE-EXISTING: test_skipped_files_warning_output fails (verbose=False) | Test expects stderr warning but verbose defaults to False | F799 | 799 | Phase 9.8 |

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
| 2026-02-21 | IMPL | implementer | Task#1 baseline grep | 0 occurrences confirmed |
| 2026-02-21 | IMPL | implementer | Task#2 add re.MULTILINE | 3 sites modified (521, 551, 621) |
| 2026-02-21 | IMPL | implementer | Task#3 extend Format A regex | Line 588 updated |
| 2026-02-21 | IMPL | implementer | Task#4 create test file | 9 tests, all PASS |
| 2026-02-21 | DEVIATION | orchestrator | Task#5 pytest full suite | 123 passed, 1 failed (PRE-EXISTING: test_skipped_files_warning_output in test_ac_verifier_binary.py) |
| 2026-02-21 | VERIFY | orchestrator | Task#5 grep re.MULTILINE count | 3 occurrences confirmed |
| 2026-02-21 00:30 | START | implementer | Task 2, 3, 4 | - |
| 2026-02-21 00:30 | END | implementer | Task 2: add re.MULTILINE to lines 521, 551, 621 | SUCCESS |
| 2026-02-21 00:30 | END | implementer | Task 3: extend Format A regex at line 588 | SUCCESS |
| 2026-02-21 00:30 | END | implementer | Task 4: create test_ac_verifier_multiline.py (9 tests) | SUCCESS |

---

## Review Notes

<!-- FL persist_pending entries will be recorded here -->
- [fix] Phase2-Review iter1: feature-798.md lines 5-17 | Section order violates template: Status → Type → Scope Discipline should be Status → Scope Discipline → Type
- [resolved-invalid] Phase2-Pending iter1: AC#11 only counts re.MULTILINE occurrences (=3) but does not verify placement at correct call sites (lines 521, 551, 621). Invalidated iter5: source code confirms only 3 re.search/re.findall calls exist in verification logic (lines 521, 551, 621), so count=3 guarantees full placement coverage.
- [resolved-applied] Phase2-Pending iter1: AC#7 labeled (Neg) but test logic is entirely positive. Need to add a conflict test case where matcher and operator notation disagree (e.g., lt matcher with >= N notation) to truly verify operator is discarded. Rename label from (Neg) to appropriate designation.
- [fix] Phase2-Review iter2: AC#7 AC Details and AC Coverage | Added negative conflict test case (lt matcher with >= 5 notation, actual=5, assert FAIL) to verify operator is truly discarded
- [fix] Phase2-Review iter3: Task#3 Description | Added operator-discard verification note to align Task#3 with AC#7 conflict test coverage
- [fix] Phase2-Review iter4: Task#1 AC# column | Changed AC#11 → '-' (Task#1 is prerequisite baseline step, not verifiable by AC#11 which checks post-fix state)

---

<!-- fc-phase-6-completed -->

## Links

[Related: F794](feature-794.md) - Parent feature; deviation discovered during F794 Phase 9 AC verification
[Related: F792](feature-792.md) - Introduced count_equals/gte matchers and Format A/B/C parsers containing the bugs
[Related: F782](feature-782.md) - Indirect; Post-Phase Review depends on F794; F798 unblocks complete ac-static-verifier usage
