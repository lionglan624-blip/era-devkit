# Feature 722: ac-static-verifier Matcher Coverage (count_equals, gt/gte/lt/lte)

## Status: [DONE]

## Type: infra

## Links

| Link | Description |
|------|-------------|
| [F608](feature-608.md) | Phase 1 AC Pattern Coverage Audit (predecessor) |
| [F613](feature-613.md) | Phase 2 AC Pattern Coverage Audit (predecessor) |
| [F717](feature-717.md) | Tool Test Coverage (trigger) |
| [F233](feature-233.md) | Character Chara Assignment (consumer) |
| [F144](feature-144.md) | Agent Documentation Consistency (consumer) |
| [F143](feature-143.md) | Agent Documentation Line Count (consumer) |
| [F234](feature-234.md) | FL Workflow Integration (consumer) |

## Background

### Problem (Current Issue)
ac-static-verifier.py does not support `count_equals`, `gt`, `gte`, `lt`, `lte` matchers. These are documented in the testing skill's Known Limitations. When ACs use these matchers, the verifier reports "Unknown matcher" and FAIL, requiring manual verification as a workaround.

Discovered during F717 execution: AC#7 uses `count_equals` and ac-static-verifier returned exit code 1.

### Goal (What to Achieve)
Add support for `count_equals`, `gt`, `gte`, `lt`, `lte` matchers to ac-static-verifier.py, and remove them from Known Limitations.

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: ac-static-verifier reports "Unknown matcher" FAIL for count_equals, gt, gte, lt, lte matchers
2. Why: The `classify_pattern()` method returns `PatternType.UNKNOWN` for these matchers, and `_verify_content()` falls through to the else branch returning FAIL
3. Why: When these matchers were defined in the Testing SKILL, no corresponding implementation was added to ac-static-verifier.py
4. Why: The verifier was built incrementally - Phase 1 (F608) covered contains/equals/exists, Phase 2 added not_contains/matches/succeeds/fails/not_exists, but numeric matchers were deferred
5. Why: Numeric matchers require different verification semantics (counting occurrences, numeric comparison) that don't fit the existing pattern-found/not-found boolean model

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| ac-static-verifier returns exit code 1 for ACs using count_equals/gt/gte/lt/lte | `classify_pattern()` has no branch for these matchers (returns UNKNOWN), and `_verify_content()` has no handler for them (returns "Unknown matcher" FAIL) |

### Conclusion

The root cause is a systematic implementation gap: all numeric-oriented matchers were deferred during the incremental build of ac-static-verifier. The existing architecture uses a boolean (pattern_found) model that needs extension to support numeric values (count of matches, numeric comparison of values).

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F608 | [DONE] | Predecessor audit | Phase 1 AC Pattern Coverage Audit - identified equals as unsupported, established methodology |
| F613 | [DONE] | Predecessor audit | Phase 2 AC Pattern Coverage Audit - documented gt/gte/lt/lte/count_equals as NOT SUPPORTED |
| F717 | [DONE] | Trigger | Tool Test Coverage feature - AC#7 used count_equals, verifier failed |
| F233 | [DONE] | Consumer | Uses count_equals matcher in ACs (issue inventory counting) |
| F144 | [DONE] | Consumer | Uses lte matcher in ACs (line count verification) |
| F143 | [DONE] | Consumer | Uses lte matcher in ACs (agent doc line counts) |
| F234 | [DONE] | Consumer | Uses gte matcher in ACs (phase count verification) |

### Pattern Analysis

This is a known, documented gap. F608 and F613 audits both identified and tracked these missing matchers. The gap has persisted because: (1) most features use contains/not_contains/exists which are supported, (2) features using numeric matchers were verified manually, and (3) no dedicated implementation feature was created until now.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Clear extension points exist in classify_pattern() and _verify_content() |
| Scope is realistic | YES | 5 matchers to add, well-defined semantics, existing test infrastructure |
| No blocking constraints | YES | No external dependencies, no architectural changes needed |

**Verdict**: FEASIBLE

The implementation requires:
1. **count_equals**: Count pattern occurrences across files, compare to expected number. Extension of existing `_search_pattern_native()` to return match count.
2. **gt/gte/lt/lte**: Parse expected value as numeric, count pattern occurrences (for file/code type) or parse numeric value (for variable type), then compare. These matchers make sense for file/code types where you count occurrences of a pattern.

Both fit naturally into the existing `_verify_content()` method as additional elif branches. The `classify_pattern()` method needs new return values or can map these to a new PatternType (e.g., NUMERIC or COUNT).

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F608 | [DONE] | Phase 1 audit that established the coverage gap methodology |
| Related | F613 | [DONE] | Phase 2 audit that documented these specific gaps |

### External Dependencies

None. Pure Python implementation with no external library requirements.

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| .claude/skills/testing/SKILL.md | MEDIUM | Known Limitations section lists these matchers - must be updated on completion |
| Game/agents/feature-*.md (multiple) | LOW | Existing features already use these matchers (verified manually); automated verification becomes available |
| .claude/skills/run-workflow/ | LOW | Uses ac-static-verifier during /run Phase 4 AC verification |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ac-static-verifier.py | Update | Add count_equals, gt, gte, lt, lte matcher support to classify_pattern(), _verify_content(), and verify_file_ac() |
| .claude/skills/testing/SKILL.md | Update | Remove count_equals and gt/gte/lt/lte from Known Limitations table |
| tools/tests/test_ac_verifier_*.py | Create | New test files for count_equals and numeric comparison matchers |
| tools/tests/test_ac_verifier_unknown_fallback.py | Update | Update tests that assert UNKNOWN/FAIL for these matchers (they will now PASS) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| count_equals requires counting occurrences, not just boolean found/not-found | Architecture of _verify_content() | MEDIUM - need to extend _search_pattern_native() or add new counting method |
| gt/gte/lt/lte need numeric Expected values | AC table format | LOW - Expected column values must be parseable as integers |
| file/code types use Grep-based search; numeric matchers need occurrence counting | Existing matcher dispatch | LOW - can reuse _search_pattern_native() and count matched_files or line occurrences |
| Existing tests assert UNKNOWN/FAIL for these matchers | test_ac_verifier_unknown_fallback.py | LOW - tests must be updated to reflect new supported status |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Ambiguous counting semantics (count files vs count lines vs count occurrences) | Medium | Medium | Define clearly: count_equals counts files matching pattern (consistent with matched_files); gt/gte/lt/lte compare this count to expected |
| Non-numeric Expected values cause runtime errors | Low | Low | Validate Expected as integer, return FAIL with clear error if not numeric |
| Breaking existing tests in test_ac_verifier_unknown_fallback.py | High | Low | Update tests as part of implementation - tests for equals/gt/gte should expect PASS or new PatternType, not UNKNOWN |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

The Feature Goal states: "Add support for `count_equals`, `gt`, `gte`, `lt`, `lte` matchers to ac-static-verifier.py, and remove them from Known Limitations."

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Add support for count_equals" | count_equals matcher implemented and functional | AC#1, AC#2 |
| "Add support for gt, gte, lt, lte" | All four numeric comparison matchers implemented | AC#3, AC#4, AC#5, AC#6 |
| "remove them from Known Limitations" | Testing SKILL Known Limitations updated | AC#7 |
| (implicit) Existing behavior preserved | Supported matchers still work correctly | AC#8 |
| (implicit) classify_pattern handles new matchers | New matchers return appropriate PatternType | AC#9 |
| (implicit) Non-numeric Expected handled | Invalid Expected values produce FAIL with clear error | AC#10 |
| (implicit) Test coverage for new matchers | pytest tests exist and pass | AC#11 |
| (implicit) Existing tests updated | Unknown fallback tests updated for new matchers | AC#12 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | count_equals matcher PASS (Pos) | exit_code | pytest tools/tests/test_ac_verifier_count_equals.py | succeeds | - | [x] |
| 2 | count_equals matcher FAIL on mismatch (Neg) | code | Grep(tools/tests/test_ac_verifier_count_equals.py) | contains | "result[\"result\"] == \"FAIL\"" | [x] |
| 3 | gt matcher implemented | code | Grep(tools/ac-static-verifier.py) | contains | "matcher == \"gt\"" | [x] |
| 4 | gte matcher implemented | code | Grep(tools/ac-static-verifier.py) | contains | "matcher == \"gte\"" | [x] |
| 5 | lt matcher implemented | code | Grep(tools/ac-static-verifier.py) | contains | "matcher == \"lt\"" | [x] |
| 6 | lte matcher implemented | code | Grep(tools/ac-static-verifier.py) | contains | "matcher == \"lte\"" | [x] |
| 7 | Known Limitations removed | file | Grep(.claude/skills/testing/SKILL.md) | not_contains | "ac-static-verifier: count_equals" | [x] |
| 8 | Existing matchers preserved | exit_code | pytest tools/tests/test_ac_verifier_normal.py | succeeds | - | [x] |
| 9 | classify_pattern handles new matchers | code | Grep(tools/ac-static-verifier.py) | matches | "count_equals|gt|gte|lt|lte" | [x] |
| 10 | Non-numeric Expected returns FAIL | code | Grep(tools/tests/test_ac_verifier_count_equals.py) | contains | "non-numeric" | [x] |
| 11 | New matcher tests pass | exit_code | pytest tools/tests/test_ac_verifier_numeric.py | succeeds | - | [x] |
| 12 | Unknown fallback tests updated | exit_code | pytest tools/tests/test_ac_verifier_unknown_fallback.py | succeeds | - | [x] |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The implementation adds count-based matcher support to ac-static-verifier.py with minimal changes:

1. **classify_pattern() extension**: Add `PatternType.COUNT` enum value and recognition logic for count_equals, gt, gte, lt, lte matchers. This prevents these matchers from returning UNKNOWN.

2. **verify_file_ac() extension**: Add elif branch for count-based matchers (line 749) that:
   - Validates Expected is numeric (int parse with error handling)
   - Counts files using `len(matched_files)` (already populated by _expand_glob_path)
   - Performs numeric comparison (==, >, >=, <, <=)
   - Returns PASS/FAIL with actual_count and expected_count in details

3. **Test coverage**: Create test files for positive/negative cases and update unknown_fallback tests to remove assertions expecting these matchers to fail.

4. **Documentation update**: Remove count_equals and gt/gte/lt/lte from Known Limitations in testing SKILL.

**Scope limitation**: File type only (Glob-based ACs). Code type (Grep-based ACs) deferred because legacy features use non-standard formats and no recent features require it.

### Implementation Details

#### 1. PatternType Enum Extension

Add new enum value to `PatternType` class (line 40):

```python
class PatternType(Enum):
    """Pattern type classification for AC verification."""
    LITERAL = auto()
    REGEX = auto()
    GLOB = auto()
    COMPLEX_METHOD = auto()
    COUNT = auto()  # NEW: For count_equals, gt, gte, lt, lte
    UNKNOWN = auto()
```

#### 2. classify_pattern() Method Update

Add new branch at line 93 (before `return PatternType.UNKNOWN`):

```python
# Count/Numeric matchers: count_equals, gt, gte, lt, lte
if ac.matcher.lower() in ("count_equals", "gt", "gte", "lt", "lte"):
    return PatternType.COUNT

return PatternType.UNKNOWN
```

#### 3. verify_file_ac() Method Extension

Add new elif branch at line 749 (after `elif matcher == "not_exists"` at line 747-748, before the `else` at line 749):

```python
elif matcher in ("count_equals", "gt", "gte", "lt", "lte"):
    # Numeric comparison matchers
    # For file type, count files matching the glob/path pattern
    # Expected column contains the numeric threshold

    # Validate Expected is numeric
    try:
        expected_count = int(ac.expected)
    except ValueError:
        return {
            "ac_number": ac.ac_number,
            "result": "FAIL",
            "details": {
                "error": f"Expected value must be numeric for {matcher} matcher, got: {ac.expected}",
                "file_path": file_path,
                "matcher": matcher,
                "matched_files": []
            }
        }

    actual_count = len(matched_files) if file_exists else 0

    # Perform numeric comparison
    if matcher == "count_equals":
        passed = actual_count == expected_count
    elif matcher == "gt":
        passed = actual_count > expected_count
    elif matcher == "gte":
        passed = actual_count >= expected_count
    elif matcher == "lt":
        passed = actual_count < expected_count
    elif matcher == "lte":
        passed = actual_count <= expected_count
    else:
        passed = False

    return {
        "ac_number": ac.ac_number,
        "result": "PASS" if passed else "FAIL",
        "details": {
            "file_path": file_path,
            "matcher": matcher,
            "expected_count": expected_count,
            "actual_count": actual_count,
            "matched_files": matched_files
        }
    }
```

**Note**: This branch is added in verify_file_ac() at line 748, which handles file type ACs with Glob() method. The count is based on `len(matched_files)` which is already populated by `_expand_glob_path()` call at line 735.

#### 4. Code Type Support (Deferred)

For code type (Grep-based ACs) with count-based matchers, the implementation is more complex because:
- The pattern to search for needs to be specified separately from the Expected count
- F233 and F377 use legacy formats that predate ac-static-verifier's current method conventions
- No recent features use code type + count_equals (F717 trigger uses file type)

**Decision**: Initial implementation focuses on file type only (AC#1-7, 9-12). Code type support deferred to future enhancement (tracked in Handoff section if needed).

**Note on semantics**: After analysis, the matchers have two different use patterns:

- **count_equals**: Pattern column contains the search pattern. Count occurrences of that pattern across files and compare to Expected number.
- **gt/gte/lt/lte**: Pattern column contains the expected number. Count total files matching the file_path (glob pattern) and compare to Expected number.

Actually, reviewing the ACs and the existing codebase, I need to reconsider. Looking at the feature template and existing usage in [DONE] features:

- F233 uses `count_equals` with Expected="2" - this counts files matching pattern
- F144 uses `lte` with Expected="100" - this counts lines or occurrences
- F143 uses `lte` with Expected="350" - this counts lines
- F234 uses `gte` with Expected="5" - this counts items (phases)

All numeric matchers follow the same pattern: **count something** (files matching pattern, lines in file, items in list) and **compare to Expected value**. The "something" to count varies by context, but for file/code types in ac-static-verifier, it's "number of files where pattern is found".

Let me revise the design to be consistent:

**Unified semantics**: All count-based matchers count the number of files where the pattern is found, then compare to Expected integer.

#### 4. Test File Structure

**test_ac_verifier_count_equals.py**: Tests count_equals matcher
- Positive case: Pattern found in expected number of files → PASS
- Negative case: Pattern found in different number of files → FAIL
- Edge case: Non-numeric Expected value → FAIL with error message

**test_ac_verifier_numeric.py**: Tests gt/gte/lt/lte matchers
- Positive cases for each matcher (comparison true)
- Negative cases for each matcher (comparison false)
- Non-numeric Expected value edge case

**test_ac_verifier_unknown_fallback.py update**: Remove assertions expecting UNKNOWN/FAIL for count_equals, gt, gte matchers (lines 56-92, 156-167). These matchers are now supported and should not trigger unknown fallback.

### Files to Modify

| File | Lines | Change Description |
|------|-------|-------------------|
| tools/ac-static-verifier.py | 40-46 | Add `COUNT = auto()` to PatternType enum |
| tools/ac-static-verifier.py | 93-95 | Add if branch for count matchers in classify_pattern() |
| tools/ac-static-verifier.py | 749-795 | Add elif branch for count matchers in verify_file_ac() |
| tools/tests/test_ac_verifier_count_equals.py | NEW | Create test file with 3+ test cases (positive, negative, non-numeric) |
| tools/tests/test_ac_verifier_numeric.py | NEW | Create test file with 8+ test cases (4 matchers × pos/neg) |
| tools/tests/test_ac_verifier_unknown_fallback.py | 56-92, 156-167 | Remove or update test cases expecting count_equals, gt, gte to fail |
| .claude/skills/testing/SKILL.md | 432-433 | Remove lines for gt/gte/lt/lte and count_equals from Known Limitations table |

### AC Coverage Matrix

| AC# | Technical Component | Verification Method |
|:---:|---------------------|---------------------|
| 1 | count_equals implementation | pytest test_ac_verifier_count_equals.py (positive path) |
| 2 | count_equals negative path | Grep for FAIL assertion in test file |
| 3 | gt matcher branch in _verify_content | Grep for `matcher == "gt"` in code |
| 4 | gte matcher branch in _verify_content | Grep for `matcher == "gte"` in code |
| 5 | lt matcher branch in _verify_content | Grep for `matcher == "lt"` in code |
| 6 | lte matcher branch in _verify_content | Grep for `matcher == "lte"` in code |
| 7 | Known Limitations removal | Grep for absence of "ac-static-verifier: count_equals" |
| 8 | Regression prevention | pytest test_ac_verifier_normal.py still passes |
| 9 | classify_pattern update | Grep for new matcher recognition code |
| 10 | Non-numeric Expected handling | Grep for "non-numeric" in test file |
| 11 | New matcher tests | pytest test_ac_verifier_numeric.py |
| 12 | Unknown fallback test update | pytest test_ac_verifier_unknown_fallback.py after removing gt/gte/count_equals assertions |

### Key Decisions

1. **PatternType.COUNT instead of PatternType.NUMERIC**: The name "COUNT" better reflects the semantics - we're counting files and comparing to a threshold. "NUMERIC" is too generic.

2. **File type only (initial scope)**: Implementation focuses on file type ACs with Glob() method (e.g., F717 AC#7). Code type support deferred because:
   - Legacy features (F233, F377) use non-standard Method formats
   - No recent features use code type + count_equals
   - File type is the trigger case and has simpler semantics

3. **Unified counting semantics**: All count-based matchers (count_equals, gt, gte, lt, lte) count the number of files matching the glob pattern specified in Method column. They differ only in the comparison operator applied to the count. This simplifies implementation and makes behavior consistent.

4. **Parameter role for count matchers**: Expected column holds the numeric threshold. Method column (Glob pattern) specifies which files to count. This is consistent with exists/not_exists matchers where Method specifies the target and Expected is unused ("-").

5. **Error handling for non-numeric Expected**: Return FAIL with descriptive error message rather than raising exception. This is consistent with existing error handling patterns (e.g., invalid regex in matches matcher).

6. **Implementation location**: Add count matcher handling directly in verify_file_ac() method (after exists/not_exists branches) rather than in _verify_content(). This is because file type with Glob doesn't use _verify_content (which is for Grep-based content search). Keeps changes localized and clear.

7. **Test file organization**: Separate test_ac_verifier_count_equals.py and test_ac_verifier_numeric.py for clarity. count_equals is a specific equality check, while gt/gte/lt/lte are threshold comparisons - separating them makes test intent clearer and allows for focused test cases.

8. **File-level counting (not line counting)**: These matchers count files matching the glob pattern. Line-level counting (e.g., F233 counting 14 table rows) is not supported and would require fundamentally different implementation (grep -c style counting). File-level counting is simpler, sufficient for current needs (F717), and consistent with glob semantics.

### Design Summary

This design adds count-based matcher support with three focused changes:
1. Extend PatternType enum (1 line)
2. Update classify_pattern (3 lines)
3. Add count matcher branch in verify_file_ac (47 lines)

Total implementation: ~50 lines in ac-static-verifier.py, plus test files.

**Impact**: Resolves Known Limitation documented in F608/F613 audits, enables automated verification for F717 and future features using count-based matchers. File type only (sufficient for current usage patterns).

**Risk**: Low. Changes are localized to new code paths (count matchers didn't work before, so no regression risk). Existing matchers protected by AC#8 (test_ac_verifier_normal.py regression check).

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 9 | Add `PatternType.COUNT` enum value and update `classify_pattern()` to recognize count_equals, gt, gte, lt, lte matchers | [x] |
| 2 | 3,4,5,6 | Add count matcher branch in `verify_file_ac()` (line 749) with numeric validation, comparison logic for gt/gte/lt/lte | [x] |
| 3 | 1,2,10 | Create `tools/tests/test_ac_verifier_count_equals.py` with positive, negative, and non-numeric Expected test cases | [x] |
| 4 | 11 | Create `tools/tests/test_ac_verifier_numeric.py` with test cases for gt/gte/lt/lte matchers | [x] |
| 5 | 12 | Update `tools/tests/test_ac_verifier_unknown_fallback.py` to remove assertions expecting count_equals, gt, gte to fail | [x] |
| 6 | 8 | Run `pytest tools/tests/test_ac_verifier_normal.py` to verify existing matchers still pass (regression check) | [x] |
| 7 | 7 | Remove count_equals and gt/gte/lt/lte entries from Known Limitations table in `.claude/skills/testing/SKILL.md` | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Implementation Steps

#### Step 1: Extend PatternType Enum (Task#1 - AC#9)

**File**: `tools/ac-static-verifier.py`
**Location**: Line 40 (PatternType class definition)

Add new enum value:
```python
class PatternType(Enum):
    """Pattern type classification for AC verification."""
    LITERAL = auto()
    REGEX = auto()
    GLOB = auto()
    COMPLEX_METHOD = auto()
    COUNT = auto()  # NEW: For count_equals, gt, gte, lt, lte
    UNKNOWN = auto()
```

#### Step 2: Update classify_pattern() Method (Task#1 - AC#9)

**File**: `tools/ac-static-verifier.py`
**Location**: Line 93 (before `return PatternType.UNKNOWN`)

Add recognition logic:
```python
# Count/Numeric matchers: count_equals, gt, gte, lt, lte
if ac.matcher.lower() in ("count_equals", "gt", "gte", "lt", "lte"):
    return PatternType.COUNT

return PatternType.UNKNOWN
```

#### Step 3: Add Count Matcher Branch in verify_file_ac() (Task#2 - AC#3,4,5,6)

**File**: `tools/ac-static-verifier.py`
**Location**: Line 749 (after `elif matcher == "not_exists"`, before the final `else`)

Add new elif branch (47 lines):
```python
elif matcher in ("count_equals", "gt", "gte", "lt", "lte"):
    # Numeric comparison matchers
    # For file type, count files matching the glob/path pattern
    # Expected column contains the numeric threshold

    # Validate Expected is numeric
    try:
        expected_count = int(ac.expected)
    except ValueError:
        return {
            "ac_number": ac.ac_number,
            "result": "FAIL",
            "details": {
                "error": f"Expected value must be numeric for {matcher} matcher, got: {ac.expected}",
                "file_path": file_path,
                "matcher": matcher,
                "matched_files": []
            }
        }

    actual_count = len(matched_files) if file_exists else 0

    # Perform numeric comparison
    if matcher == "count_equals":
        passed = actual_count == expected_count
    elif matcher == "gt":
        passed = actual_count > expected_count
    elif matcher == "gte":
        passed = actual_count >= expected_count
    elif matcher == "lt":
        passed = actual_count < expected_count
    elif matcher == "lte":
        passed = actual_count <= expected_count
    else:
        passed = False

    return {
        "ac_number": ac.ac_number,
        "result": "PASS" if passed else "FAIL",
        "details": {
            "file_path": file_path,
            "matcher": matcher,
            "expected_count": expected_count,
            "actual_count": actual_count,
            "matched_files": matched_files
        }
    }
```

**Critical**: This branch must be inserted BEFORE the final `else:` clause that handles unknown matchers.

#### Step 4: Create test_ac_verifier_count_equals.py (Task#3 - AC#1,2,10)

**File**: `tools/tests/test_ac_verifier_count_equals.py` (NEW)

Create test file with 3 test cases:
1. **Positive case**: Pattern found in expected number of files → PASS
2. **Negative case**: Pattern found in different number of files → FAIL (verify `result["result"] == "FAIL"`)
3. **Non-numeric Expected**: Expected value is non-numeric → FAIL with error message containing "non-numeric" or similar

Use existing test_ac_verifier_normal.py as structural reference (test feature files in `tools/tests/fixtures/`).

#### Step 5: Create test_ac_verifier_numeric.py (Task#4 - AC#11)

**File**: `tools/tests/test_ac_verifier_numeric.py` (NEW)

Create test file with 8+ test cases:
- Positive case for `gt` (actual > expected)
- Negative case for `gt` (actual ≤ expected)
- Positive case for `gte` (actual ≥ expected)
- Negative case for `gte` (actual < expected)
- Positive case for `lt` (actual < expected)
- Negative case for `lt` (actual ≥ expected)
- Positive case for `lte` (actual ≤ expected)
- Negative case for `lte` (actual > expected)
- Non-numeric Expected edge case

#### Step 6: Update test_ac_verifier_unknown_fallback.py (Task#5 - AC#12)

**File**: `tools/tests/test_ac_verifier_unknown_fallback.py`
**Location**: Lines 56-92, 156-167 (approximately - search for test cases using count_equals, gt, gte)

Remove or update test cases that assert these matchers return UNKNOWN or FAIL:
- count_equals tests (if present)
- gt tests (if present)
- gte tests (if present)

**Verification**: After update, `pytest tools/tests/test_ac_verifier_unknown_fallback.py` must pass.

#### Step 7: Run Regression Test (Task#6 - AC#8)

**Command**: `pytest tools/tests/test_ac_verifier_normal.py`

**Expected**: All tests pass (existing matchers unaffected by changes).

If tests fail: STOP → Report to user.

#### Step 8: Update Known Limitations (Task#7 - AC#7)

**File**: `.claude/skills/testing/SKILL.md`
**Location**: Lines 432-433 (approximately - search for "Known Limitations" table)

Remove these entries from the Known Limitations table:
- ac-static-verifier: gt/gte/lt/lte (comparison matchers)
- ac-static-verifier: count_equals

**Verification**: After update, `Grep(.claude/skills/testing/SKILL.md)` with pattern "ac-static-verifier: count_equals" should return no results.

### Rollback Plan

**Type**: Standard git revert (infra feature)

If issues are discovered after implementation:
1. `git revert <commit-hash>` to undo changes
2. Restore Known Limitations entries in testing SKILL.md
3. Remove test files created (test_ac_verifier_count_equals.py, test_ac_verifier_numeric.py)

**Note**: Since this is an additive feature (new matchers, no breaking changes to existing matchers), rollback impact is minimal. Existing features verified manually will continue to work, and ac-static-verifier will revert to reporting UNKNOWN for these matchers.

### AC Details

**AC#1: count_equals matcher PASS (Pos)**
- Tests that count_equals correctly counts file matches and returns PASS when count matches expected
- Test file: `tools/tests/test_ac_verifier_count_equals.py`
- Verifies the core counting logic: pattern searched across files, number of matched files compared to expected integer

**AC#2: count_equals matcher FAIL on mismatch (Neg)**
- Verifies negative case: when match count differs from expected, result is FAIL
- Checks test file contains assertion for FAIL result, ensuring both positive and negative paths are tested

**AC#3: gt matcher implemented**
- Verifies `_verify_content()` has an explicit branch handling `gt` (greater than) matcher
- Semantics: count of files matching pattern > expected integer

**AC#4: gte matcher implemented**
- Verifies `_verify_content()` has an explicit branch handling `gte` (greater than or equal) matcher
- Semantics: count of files matching pattern >= expected integer

**AC#5: lt matcher implemented**
- Verifies `_verify_content()` has an explicit branch handling `lt` (less than) matcher
- Semantics: count of files matching pattern < expected integer

**AC#6: lte matcher implemented**
- Verifies `_verify_content()` has an explicit branch handling `lte` (less than or equal) matcher
- Semantics: count of files matching pattern <= expected integer

**AC#7: Known Limitations removed**
- After implementation, the testing SKILL Known Limitations table must no longer list count_equals or gt/gte/lt/lte as unsupported
- Both entries ("ac-static-verifier: gt/gte/lt/lte" and "ac-static-verifier: count_equals") must be removed

**AC#8: Existing matchers preserved**
- Regression check: existing test_ac_verifier_normal.py still passes
- Ensures contains, not_contains, matches, not_matches, exists, not_exists are unaffected

**AC#9: classify_pattern handles new matchers**
- Verifies `classify_pattern()` method has been updated to recognize count_equals, gt, gte, lt, lte
- These matchers should no longer return PatternType.UNKNOWN

**AC#10: Non-numeric Expected returns FAIL**
- Edge case: when Expected value cannot be parsed as integer (e.g., "abc"), verifier returns FAIL with descriptive error
- Test file must include a test case for non-numeric Expected values with "non-numeric" in description or assertion

**AC#11: New matcher tests pass (gt/gte/lt/lte)**
- Dedicated test file for numeric comparison matchers: `tools/tests/test_ac_verifier_numeric.py`
- Tests positive cases (comparison true) and negative cases (comparison false) for all four matchers

**AC#12: Unknown fallback tests updated**
- Existing `test_ac_verifier_unknown_fallback.py` tests that assert gt/gte return UNKNOWN must be updated
- After update, tests should either remove those assertions or change expected behavior
- File must still pass pytest execution
