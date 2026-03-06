# Feature 832: ac-static-verifier Numeric Expected Parsing Fix

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T23:21:43Z -->

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
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
ac-static-verifier.py is the SSOT for automated AC verification across all feature types. Its parser must correctly handle all AC Method column formats used in feature files, ensuring that count-type matchers (gte, gt, lte, lt, count_equals) reliably extract patterns and expected counts regardless of whether Grep arguments use positional, named, or mixed parameter styles.

### Problem (Current Issue)
`_parse_complex_method()` in ac-static-verifier.py fails to parse the mixed positional+keyword format `Grep(path, pattern="...")` because its key reader (line 348-360) only supports alphanumeric and underscore characters. When encountering a positional path like `pm/features/feature-829.md`, it reads `pm` as a key name, hits `/` instead of `=`, and breaks the parsing loop, returning an empty dict which becomes `None` at line 419. This causes `_extract_grep_params()` to fall through to the simple parser (line 471-487), which sets `pattern = ac.expected` (the bare numeric value like `"12"`). The Format C guard at lines 886/1036 (`pattern != ac.expected`) then evaluates to False because both values are identical, leaving `expected_count` as None. Subsequently, `_verify_content()` attempts Format A/B regex parsing on the bare number, which matches neither format, producing the error: "Expected value must be in '`pattern` = N' or 'Pattern (N)' format for content-type gte matcher".

### Goal (What to Achieve)
Fix `_parse_complex_method()` to correctly handle mixed positional+keyword argument format `Grep(positional_arg, key="value")`, so that the pattern is extracted from the Method column's `pattern=` parameter rather than defaulting to `ac.expected`. Ensure both `verify_code_ac` (line 880-890) and `verify_file_ac` (line 1032-1040) code paths work correctly with all five count matchers. Add regression tests covering the positional path format with bare numeric Expected values.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does the error "Expected value must be in '`pattern` = N' or 'Pattern (N)' format" occur? | `_verify_content` receives `expected_count=None` and `pattern="12"`, and neither Format A nor Format B regex matches the bare number `"12"` | `ac-static-verifier.py:637-653` |
| 2 | Why is `expected_count` None? | The Format C guard at line 1036 (`pattern != ac.expected`) evaluates to False because both are `"12"`, so the guard does not set `expected_count` | `ac-static-verifier.py:1036` |
| 3 | Why does `pattern` equal `ac.expected`? | `_extract_grep_params` fell through to the simple parser (line 487) which sets `pattern = ac.expected` because `_parse_complex_method` returned None | `ac-static-verifier.py:469-487` |
| 4 | Why did `_parse_complex_method` return None? | The parser reads `pm` as a key name from the positional path `pm/features/...`, hits `/` instead of `=`, breaks the loop, and returns an empty dict which becomes None | `ac-static-verifier.py:348-360,419` |
| 5 | Why (Root)? | `_parse_complex_method` was designed to handle only `key=value` pairs. It cannot handle positional first arguments (bare values without a key prefix) which are used in the mixed format `Grep(path, pattern="...")` | `ac-static-verifier.py:348-361` (key reader: alphanumeric+underscore only) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Error message about Expected format when using gte matcher with bare numeric Expected | `_parse_complex_method` key reader cannot handle positional arguments, only `key=value` pairs |
| Where | `_verify_content()` error branch (line 649-653) | `_parse_complex_method()` character-by-character parser (line 348-361) |
| Fix | Patch the Format C guard to not rely on `pattern != ac.expected` | Teach `_parse_complex_method` to recognize positional first arguments before `key=value` pairs |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F829 | [DONE] | Discovery context; ACs triggered the bug with positional path + bare numeric Expected |
| F818 | [DONE] | Added cross-repo support to ac-static-verifier; introduced `_parse_complex_method` |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Root cause identified | FEASIBLE | Complete 5-level trace from input to error message with specific line references |
| Fix scope bounded | FEASIBLE | Single file `ac-static-verifier.py`, one parser function + two guard sites |
| Regression testable | FEASIBLE | Can create ACDefinition objects with Grep+pattern+bare numeric for unit tests |
| Side effects controllable | FEASIBLE | Fix is additive (handle positional args); existing tests cover key=value format |
| No external dependencies | FEASIBLE | Self-contained in ac-static-verifier.py |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| AC verification correctness | HIGH | All ACs using `Grep(path, pattern="...")` with count matchers currently fail silently or with error |
| Existing AC compatibility | LOW | Fix is additive; existing `Grep(path="...", pattern="...")` format continues to work |
| Feature verification pipeline | MEDIUM | Features like F829 with positional Grep ACs can be verified automatically after fix |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Duplicated Format C logic at two sites | `verify_code_ac` (line 880-890) and `verify_file_ac` (line 1032-1040) | Both sites must be fixed consistently |
| Must preserve existing key=value parsing | Existing ACs using `Grep(path="...", pattern="...")` | Fix must handle both positional and named first argument formats |
| `unescape()` applied to parsed patterns | Line 464 | After fixing parsing, `unescape()` on regex patterns from Method column may corrupt metacharacters like `\|`; verify post-fix |
| Backward compatibility with Format A/B | Existing ACs using `` `pattern` = N `` and `Pattern (N)` formats | Fix must not break these existing Expected value formats |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Fix in `_parse_complex_method` breaks existing key=value-only callers | LOW | HIGH | Existing test suite covers named-param format; run all ac_verifier tests after fix |
| `unescape()` corrupts regex patterns from Method column | MEDIUM | MEDIUM | After primary fix, verify with patterns containing `\\|`, `\\[`, etc.; may need separate unescape path |
| Duplicated Format C logic diverges during fix | LOW | LOW | Fix both sites identically or extract shared helper |
| Additional mixed formats exist beyond positional-first | LOW | MEDIUM | Audit AC tables for Grep method patterns before finalizing fix |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Existing ac_verifier tests | `python -m pytest src/tools/python/tests/test_ac_verifier_*.py -v 2>&1 \| tail -1` | All pass (pre-fix baseline) | Must remain passing after fix |
| Grep positional format ACs | `grep -c "Grep(pm/" pm/features/feature-829.md` | 7+ ACs in F829 alone | These currently fail verification |

**Baseline File**: `_out/tmp/baseline-832.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Must test positional path format `Grep(path, pattern="...")` | Root cause analysis | Existing tests only cover `Grep(path=..., pattern=...)`; must add positional format tests |
| C2 | Must test all 5 count matchers | Feature scope | gte, gt, lte, lt, count_equals all share the same code path through Format C |
| C3 | Must test both verify_code_ac and verify_file_ac paths | Two affected sites (lines 880-890, 1032-1040) | Type=code and Type=file both have the Format C guard bug |
| C4 | Must not break existing Glob-based numeric tests | Regression protection | Existing tests in test_ac_verifier_numeric.py must still pass |
| C5 | Must not break existing named-param Grep tests | Backward compatibility | Existing tests in test_ac_verifier_count_equals_content.py must still pass |

### Constraint Details

**C1: Positional Path Format Support**
- **Source**: All 3 investigations traced root cause to `_parse_complex_method` failing on positional first argument
- **Verification**: Create ACDefinition with Method=`Grep(pm/features/feature-829.md, pattern="\\| OB-[0-9]+ \\|")` and Expected=`12`
- **AC Impact**: AC must use the exact positional format that triggers the bug, not the named `path=` format

**C2: All Count Matchers Coverage**
- **Source**: gte, gt, lte, lt, count_equals all flow through the same Format C guard code
- **Verification**: Each matcher type should be tested with bare numeric Expected
- **AC Impact**: At minimum test gte and count_equals; ideally all 5

**C3: Both Verification Entry Points**
- **Source**: Identical Format C guard exists in both `verify_code_ac` and `verify_file_ac`
- **Verification**: Test with Type=code and Type=file ACs
- **AC Impact**: Must have ACs exercising both code paths

**C4: Existing Glob Test Regression**
- **Source**: All 10 existing numeric matcher tests use `Glob()` method
- **Verification**: Run existing test_ac_verifier_numeric.py suite after fix
- **AC Impact**: Existing test suite pass is a required AC

**C5: Existing Named-Param Grep Regression**
- **Source**: Existing tests in test_ac_verifier_count_equals_content.py use `Grep(path=..., pattern=...)`
- **Verification**: Run existing test suite after fix
- **AC Impact**: Existing test suite pass is a required AC

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F829 | [DONE] | Discovery context; ACs triggered the bug with positional path + bare numeric Expected |
| Related | F818 | [DONE] | Added cross-repo support to ac-static-verifier; introduced `_parse_complex_method` |

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
| "parser must correctly handle all AC Method column formats" | Positional+keyword mixed format must be parsed successfully | AC#1, AC#2, AC#3 |
| "count-type matchers (gte, gt, lte, lt, count_equals) reliably extract patterns and expected counts" | All 5 count matchers must work with positional path + bare numeric Expected | AC#4, AC#5 |
| "regardless of whether Grep arguments use positional, named, or mixed parameter styles" | Named-param format must continue to work (regression) | AC#7, AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | _parse_complex_method handles positional first arg | code | Grep(src/tools/python/ac-static-verifier.py, pattern="positional") | matches | `positional` | [x] |
| 2 | verify_code_ac works with positional path + gte + bare numeric Expected | exit_code | pytest src/tools/python/tests/test_ac_verifier_count_equals_content.py -k "positional_path_code" | succeeds | - | [x] |
| 3 | verify_file_ac works with positional path + gte + bare numeric Expected | exit_code | pytest src/tools/python/tests/test_ac_verifier_count_equals_content.py -k "positional_path_file" | succeeds | - | [x] |
| 4 | All 5 count matchers work with positional path format | exit_code | pytest src/tools/python/tests/test_ac_verifier_count_equals_content.py -k "positional_path_all_matchers" | succeeds | - | [x] |
| 5 | New regression tests pass | exit_code | pytest src/tools/python/tests/test_ac_verifier_count_equals_content.py -k "positional" | succeeds | - | [x] |
| 6 | Both Format C guard sites use consistent logic | code | Grep(src/tools/python/ac-static-verifier.py, pattern="pattern != ac.expected") | count_equals | 2 | [x] |
| 7 | Existing numeric matcher tests pass | exit_code | pytest src/tools/python/tests/test_ac_verifier_numeric.py | succeeds | - | [x] |
| 8 | Existing named-param Grep tests pass | exit_code | pytest src/tools/python/tests/test_ac_verifier_count_equals_content.py -k "not positional" | succeeds | - | [x] |
| 9 | Existing complex method tests pass | exit_code | pytest src/tools/python/tests/test_ac_verifier_complex_method.py | succeeds | - | [x] |

### AC Details

**AC#1: _parse_complex_method handles positional first arg**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="positional")`
- **Expected**: The parser function must contain logic to handle positional (non-key=value) first arguments
- **Rationale**: Root cause is that `_parse_complex_method` only reads `key=value` pairs. The fix must add positional argument handling, which should be identifiable by the word "positional" in comments or variable names in the parser function.

**AC#6: Both Format C guard sites use consistent logic**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="pattern != ac.expected")` count_equals 2
- **Expected**: Exactly 2 occurrences (one in `verify_code_ac` at ~line 886, one in `verify_file_ac` at ~line 1036)
- **Rationale**: Both guard sites must remain consistent. If the fix changes the guard logic, both sites must be updated identically. count_equals 2 verifies no divergence.
- **Derivation**: 2 = verify_code_ac (1) + verify_file_ac (1)

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Fix `_parse_complex_method` to handle mixed positional+keyword format | AC#1 |
| 2 | Pattern extracted from Method's `pattern=` param, not defaulting to `ac.expected` | AC#2, AC#3 |
| 3 | Both `verify_code_ac` and `verify_file_ac` work correctly with all five count matchers | AC#2, AC#3, AC#4, AC#6 |
| 4 | Add regression tests covering positional path format with bare numeric Expected | AC#5 |
| 5 | Do not break existing Glob-based numeric tests | AC#7 |
| 6 | Do not break existing named-param Grep tests | AC#8, AC#9 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The root cause is that `_parse_complex_method()` starts every iteration of its parse loop by reading an alphanumeric+underscore key. When the first argument is a bare positional value like `pm/features/feature-829.md`, the parser reads `pm` as a key, hits `/` instead of `=`, and breaks out of the loop returning an empty dict (None). This causes `_extract_grep_params()` to fall through to the simple parser, which sets `pattern = ac.expected` — the bare number — collapsing Format C detection.

The fix adds a positional argument detection step at the top of the parse loop's first iteration. Before attempting to read a `key=value` pair, the parser checks whether the current position starts a positional argument (a value token not followed by `=`). If detected, the value is stored under the key `_positional_0` (or a positional slot name), and parsing continues for subsequent `key=value` pairs. `_extract_grep_params()` then maps `_positional_0` → `path` and named `path=` → `path`, preserving backward compatibility.

This approach is purely additive: existing `Grep(path="...", pattern="...")` and `Grep(path=..., pattern=...)` formats continue working unchanged. The positional detection branch only activates when the very first token cannot start a `key=value` pair (i.e., the character after the key-word is not `=`).

Both Format C guard sites (`verify_code_ac` lines 883-887 and `verify_file_ac` lines 1034-1037) already have correct logic (`pattern != ac.expected`). After the parser fix, `_extract_grep_params()` will return the `pattern=` value from the Method column rather than falling back to `ac.expected`, so `pattern != ac.expected` will evaluate to True and `expected_count` will be set correctly. The two guard sites do not need modification — they remain identical and AC#6 (`count_equals 2` for `"pattern != ac.expected"`) stays satisfied.

Regression tests (AC#2–AC#5) are added to `test_ac_verifier_count_equals_content.py` using the existing test structure: create a temp file, build an `ACDefinition` with `method="Grep(pm/..., pattern=...)"` and a bare numeric Expected, call `verify_code_ac` or `verify_file_ac`, and assert PASS.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add positional argument handling inside `_parse_complex_method()`. The word "positional" must appear in a variable name or comment in the parser function body so the Grep search finds it. |
| 2 | Add `test_positional_path_code()` in `test_ac_verifier_count_equals_content.py` using `method="Grep(src/tools/python/ac-static-verifier.py, pattern=...)"`, matcher `gte`, bare numeric Expected. Passes after the parser fix. |
| 3 | Add `test_positional_path_file()` in the same file using `ac_type="file"` and `verify_file_ac()` call path. |
| 4 | Add `test_positional_path_all_matchers()` covering all five count matchers (`gte`, `gt`, `lte`, `lt`, `count_equals`) with the positional path format. |
| 5 | All three new tests use `-k "positional"` marker; pytest `-k "positional"` passes when all new tests pass. |
| 6 | The two Format C guard conditionals remain unchanged (`pattern != ac.expected` at both sites). No new occurrences are introduced. AC already satisfied in existing code; fix must not disturb the guard expression text. |
| 7 | Existing `test_ac_verifier_numeric.py` tests still pass — fix is additive and does not touch Glob-based code paths. |
| 8 | Existing non-positional tests in `test_ac_verifier_count_equals_content.py` still pass — named-param parsing is unchanged. |
| 9 | Existing `test_ac_verifier_complex_method.py` tests still pass — named-param parsing is unchanged. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where to detect positional arg | (A) In `_parse_complex_method`, (B) In `_extract_grep_params` with regex pre-check | A: inside `_parse_complex_method` | `_parse_complex_method` is the single parser for all complex formats. Fixing it there propagates to all callers automatically; regex pre-check in `_extract_grep_params` would be a second parser creating two sources of truth. |
| Positional slot naming | (A) Use key name `_positional_0`, (B) Immediately alias to `path` inside parser | B: alias to `path` inside `_extract_grep_params` at the caller level | Keeping `_positional_0` as raw key preserves generality of the parser (future positional arg 1 etc.); `_extract_grep_params` maps `_positional_0` → `path` with a one-liner. |
| Format C guard modification | (A) Keep `pattern != ac.expected` guard unchanged, (B) Change guard logic | A: keep unchanged | After the parser fix `pattern` will differ from `ac.expected` whenever a `pattern=` kwarg is present. The existing guard is logically correct; no change needed. AC#6 verifies count stays at exactly 2. |
| Test file destination | (A) New test file `test_ac_verifier_positional.py`, (B) Append to existing `test_ac_verifier_count_equals_content.py` | B: append to existing | AC#2–AC#5 use `-k "positional"` filter which works against the existing file. Keeping count-related tests together is consistent with current test file naming convention. AC#8 uses `-k "not positional"` against the same file, which confirms existing tests are unaffected. |
| `unescape()` on positional-path patterns | (A) Apply same `unescape()` as named-param path, (B) Skip for positional path value, apply for pattern value | A: apply uniformly | The `pattern=` value from a positional-format call still comes through markdown and may have `\\|` → `\|` escaping. `unescape()` is already called for all complex-method patterns at line 464; positional parsing must reach the same code path. |

### Interfaces / Data Structures

No new interfaces or data structures. The fix modifies the character-by-character parser loop in `_parse_complex_method()` (lines 339-419) and adds a positional-slot-to-`path` mapping in `_extract_grep_params()` (lines 441-489). All changes are within the existing `ACVerifier` class.

**Parser change sketch** (inside the `while i < len(params_str):` loop, before the key-reading block):

Extract the peek-ahead logic into a helper method `_is_key_value_start(params_str, pos)` that returns `True` if the token at `pos` is the start of a `key=value` pair (alphanumeric/underscore key followed by `=`), `False` otherwise. This keeps `_parse_complex_method` readable and the peek logic independently testable.

```python
def _is_key_value_start(self, params_str, pos):
    """Check if position starts a key=value pair (not a positional argument)."""
    peek = pos
    while peek < len(params_str) and (params_str[peek].isalnum() or params_str[peek] == '_'):
        peek += 1
    # skip whitespace after candidate key
    while peek < len(params_str) and params_str[peek].isspace():
        peek += 1
    return peek < len(params_str) and params_str[peek] == '='

# In _parse_complex_method, positional detection (supports multiple positional args):
positional_index = 0
# ... inside the while loop:
if not self._is_key_value_start(params_str, i):
    # positional argument: read until comma
    pos_start = i
    while i < len(params_str) and params_str[i] != ',':
        i += 1
    result[f'_positional_{positional_index}'] = params_str[pos_start:i].strip()
    positional_index += 1
    # skip comma
    if i < len(params_str) and params_str[i] == ',':
        i += 1
    continue
```

**`_extract_grep_params` mapping addition** (after the `if parsed:` branch, before `file_path = parsed.get('path')`):

```python
# Map positional first argument to 'path' if named 'path' not present
if '_positional_0' in parsed and 'path' not in parsed:
    parsed['path'] = parsed['_positional_0']
```

This preserves exact behavior for existing `path=` named callers, and handles the new positional case.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#10 was removed (vacuous `not_matches` — pattern doesn't exist pre-fix, test trivially passes). Upstream issue resolved by deletion. | AC Definition Table | AC#10 deleted per feature-validator V3 finding. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 6 | Fix `_parse_complex_method()` in `ac-static-verifier.py`: add positional first-argument detection before the key-reading block (extract peek-ahead into `_is_key_value_start()` helper), store value under `_positional_0` key; add `_positional_0` → `path` mapping in `_extract_grep_params()`. Both Format C guard sites (`pattern != ac.expected`) remain unchanged. Verify `unescape()` does not corrupt regex metacharacters (`\\|`, `\\[`) in positional-path patterns. | | [x] |
| 2 | 2 | Add `test_positional_path_code()` to `test_ac_verifier_count_equals_content.py`: build ACDefinition with `method="Grep(src/tools/python/ac-static-verifier.py, pattern=...)"`, matcher `gte`, bare numeric Expected; call `verify_code_ac`; assert PASS. | | [x] |
| 3 | 3 | Add `test_positional_path_file()` to `test_ac_verifier_count_equals_content.py`: same as T2 but with `ac_type="file"`; call `verify_file_ac`; assert PASS. | | [x] |
| 4 | 4, 5 | Add `test_positional_path_all_matchers()` to `test_ac_verifier_count_equals_content.py`: loop or individual assertions covering all five count matchers (`gte`, `gt`, `lte`, `lt`, `count_equals`) with the positional path format; assert all PASS. | | [x] |
| 5 | 7, 8, 9 | Run full existing test suites: `test_ac_verifier_numeric.py`, `test_ac_verifier_count_equals_content.py -k "not positional"`, `test_ac_verifier_complex_method.py`; verify all pass. | | [x] |

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
| 1 | implementer | sonnet | `src/tools/python/ac-static-verifier.py` | Fixed `_parse_complex_method()` and `_extract_grep_params()` |
| 2 | implementer | sonnet | `src/tools/python/tests/test_ac_verifier_count_equals_content.py` | 3 new test functions added |
| 3 | implementer | sonnet | All `test_ac_verifier_*.py` suites + `ac-static-verifier.py` | Confirmation: all tests pass, no tech debt markers |

### Pre-conditions

- `src/tools/python/ac-static-verifier.py` must be readable (no merge conflicts)
- `src/tools/python/tests/test_ac_verifier_count_equals_content.py` must exist and all existing tests must pass before starting

### Execution Order

**Step 1 (Task 1): Fix parser in `ac-static-verifier.py`**

Add a private helper method `_is_key_value_start(params_str, pos)` that checks if position starts a `key=value` pair. Inside `_parse_complex_method()`, at the top of the `while i < len(params_str):` loop, before the existing key-reading block, insert positional argument detection using the helper:

```python
def _is_key_value_start(self, params_str, pos):
    """Check if position starts a key=value pair (not a positional argument)."""
    peek = pos
    while peek < len(params_str) and (params_str[peek].isalnum() or params_str[peek] == '_'):
        peek += 1
    while peek < len(params_str) and params_str[peek].isspace():
        peek += 1
    return peek < len(params_str) and params_str[peek] == '='

# In _parse_complex_method, positional detection (counter-based for future extensibility):
positional_index = 0
# ... inside the while loop:
if not self._is_key_value_start(params_str, i):
    pos_start = i
    while i < len(params_str) and params_str[i] != ',':
        i += 1
    result[f'_positional_{positional_index}'] = params_str[pos_start:i].strip()
    positional_index += 1
    if i < len(params_str) and params_str[i] == ',':
        i += 1
    continue
```

In `_extract_grep_params()`, after the `if parsed:` branch and before `file_path = parsed.get('path')`, add:

```python
# Map positional first argument to 'path' if named 'path' not present
if '_positional_0' in parsed and 'path' not in parsed:
    parsed['path'] = parsed['_positional_0']
```

Do NOT modify the Format C guard expressions (`pattern != ac.expected`) in `verify_code_ac` or `verify_file_ac`.

After applying the fix, verify `unescape()` does not corrupt regex metacharacters (`\\|`, `\\[`) in positional-path patterns by testing with patterns like `pattern="\\|test"`. If corruption is confirmed, track via Mandatory Handoffs.

**Step 2 (Tasks 2–4): Add regression tests to `test_ac_verifier_count_equals_content.py`**

Add the following three test functions. Each must use the positional format `Grep(path, pattern="...")` with a bare numeric Expected.

- `test_positional_path_code()`: Use a real file path, `gte` matcher, bare numeric Expected (e.g., `"1"`), verify `verify_code_ac` returns PASS.
- `test_positional_path_file()`: Same with `ac_type="file"`, call `verify_file_ac`.
- `test_positional_path_all_matchers()`: Cover `gte`, `gt`, `lte`, `lt`, `count_equals` — all with positional path format.

**Step 3 (Task 5): Verify full regression suite**

Run:
```
python -m pytest src/tools/python/tests/test_ac_verifier_numeric.py -v
python -m pytest src/tools/python/tests/test_ac_verifier_count_equals_content.py -v
python -m pytest src/tools/python/tests/test_ac_verifier_complex_method.py -v
```

Confirm all pass. Confirm no `TODO|FIXME|HACK` in changed code.

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix

### Success Criteria

- All 9 ACs pass
- All pre-existing `test_ac_verifier_*.py` tests continue to pass
- No technical debt markers in modified file

### Error Handling

- If existing tests break after parser change: STOP → report which tests fail and why before attempting any fix
- If `unescape()` corrupts regex patterns from positional-path Method column: STOP → report to user (tracked as risk in Risks section)
- 3 consecutive failures → STOP → escalate to user per Escalation Policy

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|-------------|--------|
| Duplicated Format C guard logic in verify_code_ac and verify_file_ac | Pre-existing DRY violation; both sites share identical Grep-count-matcher logic | F834 | F834 | Post-/run | [x] | 作成済み |
| unescape() may corrupt regex metacharacters from positional-path Method column | Risk #2 (MEDIUM likelihood); needs dedicated investigation if confirmed during T5 | F834 | F834 | Post-/run (conditional) | [x] | 作成済み |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T10:00 | PHASE_START | orchestrator | Phase 1 Initialize | READY:832:infra |
<!-- run-phase-1-completed -->
| 2026-03-06T10:02 | PHASE_COMPLETE | orchestrator | Phase 2 Investigation | Explorer confirmed all locations |
<!-- run-phase-2-completed -->
| 2026-03-06T10:05 | PHASE_COMPLETE | orchestrator | Phase 4 Implementation | All 5 Tasks [x], 25+3 tests pass |
<!-- run-phase-4-completed -->
| 2026-03-06T10:07 | SKIP | orchestrator | Phase 5-6 | infra type: skip to Phase 7 |
<!-- run-phase-5-completed -->
<!-- run-phase-6-completed -->
| 2026-03-06T10:10 | PHASE_COMPLETE | orchestrator | Phase 7 Verification | 9/9 AC PASS, 0 DEVIATION |
<!-- run-phase-7-completed -->
| 2026-03-06T10:12 | PHASE_COMPLETE | orchestrator | Phase 8 Post-Review | READY (8.2/8.3 skip: no new extensibility) |
<!-- run-phase-8-completed -->
| 2026-03-06T10:15 | PHASE_COMPLETE | orchestrator | Phase 9 Report | All PASS, 0 DEVIATION, approved |
<!-- run-phase-9-completed -->
| 2026-03-06T10:16 | CodeRabbit | 0 findings | - |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: pm/features/feature-832.md:445 | Mandatory Handoffs table missing Transferred and Result columns (template compliance)
- [fix] Phase2-Review iter1: pm/features/feature-832.md (after Review Notes) | Missing Improvement Log section (template compliance)
- [fix] Phase3-Maintainability iter2: pm/features/feature-832.md:445 | Duplicated Format C guard logic — added Mandatory Handoff entries for consolidation and unescape() risk
- [fix] Phase3-Maintainability iter2: pm/features/feature-832.md:282 | Extract peek-ahead into _is_key_value_start() helper for readability
- [fix] Phase3-Maintainability iter2: pm/features/feature-832.md:333 | Added unescape() verification and helper extraction to Task 1 description
- [fix] Phase3-Maintainability iter3: pm/features/feature-832.md:452-453 | Updated Mandatory Handoff Destination IDs from '—' to F834 (created feature-834.md)
- [fix] Phase3-Maintainability iter3: pm/features/feature-832.md:297-308 | Generalized positional arg detection from single _positional_0 to counter-based _positional_N
- [resolved-skipped] Phase3-Maintainability iter3: Format C guard consolidation (extract shared helper _resolve_count_expected) — deferred to F834 per user decision (POST-LOOP). Parser fix and DRY consolidation are independent concerns.
- [fix] Phase7-FinalRefCheck iter4: Links section | Added F834 link (referenced in Mandatory Handoffs but missing from Links)

<!-- fc-phase-6-completed -->

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 832 (2026-03-06)
- [applied] wbs-generator Mandatory Handoffs テーブル 5列→7列 (Transferred, Result追加) → `.claude/agents/wbs-generator.md`
- [applied] wbs-generator Output Structure に Improvement Log セクション追加 → `.claude/agents/wbs-generator.md`
- [revised] quality-fixer V1n: Improvement Log セクション欠如検出を追加 (P2適用後はほぼ発火しない) → `.claude/agents/quality-fixer.md`
- [rejected] C16 warning→blocking昇格 — FC段階でのDestination強制は品質低下リスク
- [rejected] predecessor file重複読み取り削減 — F822 lessonで既対策済み、F832への影響証拠なし

---

## Links
- [Related: F829](feature-829.md) - Phase 22 Deferred Obligations Consolidation (discovery context)
- [Related: F818](feature-818.md) - Cross-repo support for ac-static-verifier (introduced `_parse_complex_method`)
- [Successor: F834](feature-834.md) - ac-static-verifier Format C Guard DRY Consolidation and unescape() Investigation
