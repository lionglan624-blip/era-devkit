# Feature 817: ac-static-verifier pipe-escaping fix

## Status: [DONE]
<!-- fl-reviewed: 2026-02-24T06:27:31Z -->

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
| Parent Feature | F804 |
| Discovery Phase | Phase 7 |
| Timestamp | 2026-02-24 |

### Observable Symptom
ac-static-verifier.py treats `|` in AC regex patterns as literal pipe characters instead of regex alternation, causing 12/47 code-type ACs to FAIL with pattern errors ("unterminated subpattern", zero matches). This has been documented in 14+ features (F468, F593, F612, F635, F733, F737, F755, F774, F777, F787, F792, F796, F801, F804).

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python src/tools/python/ac-static-verifier.py --feature 804 --ac-type code` |
| Exit Code | 1 |
| Error Output | `Invalid regex pattern: missing ), unterminated subpattern at position 49` |
| Expected | 47/47 PASS |
| Actual | 35/47 PASS (12 FAIL from pipe-escaping) |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Tool source with pipe-escaping bug |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | - |

### Parent Session Observations
The pipe-escaping bug has been a recurring issue across 14+ features since F468. Manual ac-tester verification works correctly as a workaround. The root cause is in how ac-static-verifier.py parses and escapes AC regex patterns from feature markdown tables where `|` serves as both table delimiter and regex alternation.

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Tooling reliability supports the feature development workflow. ac-static-verifier is the SSOT for automated AC verification; when it produces false failures, developers lose trust and fall back to manual workarounds, undermining the entire automated quality gate.

### Problem (Current Issue)
ac-static-verifier.py has two asymmetric data paths for pattern extraction: (A) the Expected column, which passes through `unescape()` at line 792, and (B) complex method `pattern=` parameters, which are extracted verbatim at line 416 without any unescape processing. Because markdown feature files use double-escaped sequences (`\\(`, `\\[`, `\\.`, `\\w`, `\\?`) to represent regex metacharacters, patterns from path (B) arrive at Python's `re` module with corrupted semantics -- `\\(` becomes "literal backslash + unclosed group" instead of "escaped literal parenthesis", producing "unterminated subpattern" errors. Additionally, `unescape()` itself only handles 4 escape sequences (`\"`, `\\[`, `\\]`, `\|`) and lacks support for `\\(`, `\\)`, `\\.`, and other common regex metacharacters. This has caused 12/47 code-type AC failures in F804 alone and has been a recurring issue across 14+ features since F468.

### Goal (What to Achieve)
Apply `unescape()` to complex method `pattern=` parameters and extend `unescape()` to handle all markdown-escaped regex metacharacters (`\\(`, `\\)`, `\\.`, `\\w`, `\\?`), so that both data paths produce correct regex patterns for Python's `re` module. Retire the INFRA.md Issue 14 workaround once the fix is verified. Also consume the `glob` parameter dead code in `_extract_grep_params()` (C5), since it was identified during investigation as a dead code path in the same function being fixed.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do 12/47 code ACs fail? | Regex patterns contain double-escaped metacharacters (`\\(`, `\\[`, `\\.`) that cause compilation errors or wrong semantics in Python's `re` module | feature-804.md:1067, feature-817.md:25 |
| 2 | Why are patterns double-escaped? | The `pattern=` value from complex method parsing is passed directly to `re.search()` without calling `unescape()` | ac-static-verifier.py:416, :521 |
| 3 | Why is `unescape()` not called on complex method patterns? | `_extract_grep_params()` extracts the pattern from `_parse_complex_method()` verbatim, while `unescape()` is only applied to `ac.expected` during table parsing | ac-static-verifier.py:416 vs :792 |
| 4 | Why are there two separate code paths? | F737/F792 added `unescape()` to the Expected column path but the complex method path (added in F632 era) was not updated | ac-static-verifier.py:792, feature-792.md:67-68 |
| 5 | Why (Root)? | The parser has two asymmetric data paths -- (A) Expected column through table-parser + unescape, and (B) complex method `pattern=` through `_parse_complex_method` only -- and `unescape()` was only applied to path (A), leaving path (B) with raw markdown escapes | ac-static-verifier.py:204-220, :328-335, :416, :792 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | "unterminated subpattern at position 49" and zero-match failures on complex method ACs | Two asymmetric data paths where `unescape()` is applied to Expected column but not to complex method `pattern=` parameters |
| Where | Python `re.search()` at runtime (ac-static-verifier.py:521) | `_extract_grep_params()` at ac-static-verifier.py:416 (missing unescape call) and `unescape()` at :220 (incomplete escape set) |
| Fix | Use `.*` instead of metacharacters in AC patterns (INFRA.md Issue 14 workaround) | Apply `unescape()` to complex method patterns and extend `unescape()` to handle `\\(`, `\\)`, `\\.`, `\\w`, `\\?` |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F792 | [DONE] | Fixed Expected column unescape path; this feature completes the complex method path |
| F737 | [DONE] | Added quote-aware pipe handling and state machine parser |
| F632 | [DONE] | Introduced complex method support in ac-static-verifier |
| F804 | [DONE] | Trigger feature: 12/47 code ACs failed due to this bug |
| F801 | [DONE] | Affected feature: silent truncation examples from bare pipe in Expected |
| F798 | [DONE] | Prior fix: re.MULTILINE and Format A parser improvements |
| F787 | [DONE] | Encountered same class of issue pre-F792 |
| F468 | [DONE] | First known occurrence of this bug class |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Root cause identified | FEASIBLE | Two asymmetric code paths at ac-static-verifier.py:416 vs :792 |
| Fix scope bounded | FEASIBLE | Single `unescape()` call insertion at line 416 + extension of `unescape()` function at line 220 |
| Infrastructure exists | FEASIBLE | `unescape()` function already exists; needs extension, not creation |
| Backward compatible | FEASIBLE | `unescape()` is additive; str.replace is idempotent |
| Test infrastructure | FEASIBLE | Existing test files: test_ac_verifier_pipe_escape.py, test_ac_verifier_complex_method.py |
| No external dependencies | FEASIBLE | Pure Python, no new packages |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| F804 AC verification | HIGH | 12/47 code ACs currently fail; fix enables full verification |
| ac-static-verifier reliability | HIGH | Eliminates a class of false failures affecting 14+ features |
| INFRA.md Issue 14 workaround | MEDIUM | Can be retired once fix is verified |
| Existing passing ACs | LOW | unescape is idempotent; no regression expected |
| ac_ops.py | LOW | Shares parsing code but does not execute regex; NOT affected |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `unescape()` must be idempotent | Code contract; simple-method patterns already processed via Expected column | str.replace chain is naturally idempotent |
| Must not double-unescape Expected column | ac-static-verifier.py:763,:792 | Table parser already strips `\` before unescape; complex method path needs separate unescape call |
| `unescape()` must handle all markdown escape conventions | Evidence from F804 ACs: `\\(`, `\\)`, `\\[`, `\\]`, `\\.`, `\\w`, `\\?` | Current function only handles `\"`, `\\[`, `\\]`, `\|`; needs extension |
| Quoted vs unquoted pattern values | ac-static-verifier.py:328-335 | `_parse_complex_method` extracts verbatim; unescape must be applied after extraction |
| Python regex `\\` semantics | Python re module | `\\(` in regex = literal backslash + unclosed group; `\(` = escaped literal paren |
| Regex `\|` is valid in Python | Python re module | `\|` equals `|` in Python regex; real failures come from `\\(`, `\\.`, etc. |
| CommonMark escaping applies only to ASCII punctuation | CommonMark spec (Explorer 2) | `\\` before ASCII punctuation (`(`, `)`, `[`, `]`, `.`, `?`, etc.) is a markdown escape; `\b`, `\s`, `\d` must NOT be converted -- they are regex character classes, not markdown escapes. **Exception**: `\\w` is included per C3 spec despite not being CommonMark punctuation, because F804 AC#55 evidence requires it and the conversion `\\w` → `\w` produces a valid Python regex word-class (idempotent, low risk) |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Over-broad unescape changes semantics for correctly-escaped patterns | LOW | MEDIUM | Only unescape markdown conventions; audit existing features |
| General `\\` to `\` conversion affects non-markdown escapes | LOW | MEDIUM | Apply targeted replacements, not blanket `\\` stripping |
| Breaks existing passing ACs | LOW | HIGH | Full pytest suite regression |
| Regex `\\` (literal backslash match) becomes impossible after unescape | LOW | LOW | Use `\\\\` (4 backslashes) in markdown for literal backslash |
| glob fix introduces path filtering bugs | LOW | LOW | Add unit tests for glob parameter |
| AC authors continue using wrong escaping conventions | MEDIUM | MEDIUM | Document correct conventions after fix |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| F804 code AC pass rate | `python src/tools/python/ac-static-verifier.py --feature 804 --ac-type code` | 35/47 PASS (12 FAIL) | Pre-fix baseline |
| pytest ac-verifier suite | `python -m pytest src/tools/python/tests/test_ac_verifier_*.py -v` | All PASS | Must not regress |

**Baseline File**: `.tmp/baseline-817.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Fix must apply unescape to complex method pattern= path | Root cause (line 416) | Verify `\\(` in complex method pattern becomes `\(` after processing |
| C2 | Fix must not break Expected column unescape | F792 behavior (line 792) | Regression test: Expected column patterns still work |
| C3 | unescape must handle `\\(`, `\\)`, `\\.`, `\\w`, `\\?` beyond current 4 patterns | Evidence from F804 AC#9, #37, #38, #39, #55 | Test each new escape type individually |
| C4 | Existing test suite must pass | F792 test guarantee | pytest regression on all test_ac_verifier_*.py |
| C5 | glob parameter from complex method must be consumed | Dead code at _extract_grep_params (2/3 explorers) | AC for glob filtering producing correct file subset |
| C6 | ac_ops.py is NOT affected | Does not execute regex (line 31-79) | No AC needed for ac_ops.py |
| C7 | ACs must be verifiable by the FIXED tool | Bootstrapping constraint | Use pytest-based ACs, not self-referential ac-static-verifier |

### Constraint Details

**C1: Complex Method Pattern Unescape**
- **Source**: Investigation of ac-static-verifier.py:416 -- pattern extracted from _parse_complex_method is used raw
- **Verification**: Grep for `unescape` near line 416 in ac-static-verifier.py after fix
- **AC Impact**: Must test complex method patterns with `\\(` and verify they compile as valid regex

**C2: Expected Column Backward Compatibility**
- **Source**: F792 established unescape at line 792 for Expected column
- **Verification**: Run existing test_ac_verifier_pipe_escape.py tests
- **AC Impact**: Regression AC must verify Expected column patterns still work unchanged

**C3: Extended unescape Coverage**
- **Source**: F804 ACs use `\\(` (AC#9, #37), `\\[` (AC#19), `\\.` (AC#38, #55), `\\w` (AC#55), `\\?` (AC#39)
- **Verification**: Unit test each escape sequence through unescape()
- **AC Impact**: Individual test cases for each new escape type

**C4: Test Suite Regression**
- **Source**: Existing test infrastructure in src/tools/python/tests/
- **Verification**: `python -m pytest src/tools/python/tests/test_ac_verifier_*.py`
- **AC Impact**: All existing tests must continue to pass

**C5: glob Parameter Support**
- **Source**: _extract_grep_params at line 404-416 only reads path and pattern; glob is parsed but never consumed
- **Verification**: Create complex method AC with glob parameter and verify file filtering
- **AC Impact**: Test that glob restricts search to matching files only

**C6: ac_ops.py Not Affected**
- **Source**: ac_ops.py shares split_pipe_row but does not call re.search/re.findall on patterns
- **Verification**: Code inspection (no regex execution in ac_ops.py)
- **AC Impact**: No AC needed; documented as out-of-scope

**C7: Bootstrapping Constraint**
- **Source**: ac-static-verifier is the tool being fixed; cannot use it to verify its own fix
- **Verification**: Use pytest as verification mechanism
- **AC Impact**: All ACs should use pytest or direct Python execution, not ac-static-verifier itself

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F792 | [DONE] | Provides unescape() infrastructure and Expected column fix |
| Predecessor | F737 | [DONE] | Provides quote-aware pipe handling state machine |
| Related | F632 | [DONE] | Introduced complex method support in ac-static-verifier |
| Related | F804 | [DONE] | Trigger feature; 12/47 code ACs fail due to this bug |
| Related | F801 | [DONE] | Contains silent truncation examples from bare pipe in Expected |

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

<!-- fc-phase-3-completed -->

## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "ac-static-verifier is the SSOT for automated AC verification" | Both data paths (Expected column and complex method pattern=) must produce correct regex patterns | AC#1, AC#2, AC#3 |
| "when it produces false failures, developers lose trust and fall back to manual workarounds, undermining the entire automated quality gate" | Patterns with markdown-escaped metacharacters must compile and match correctly in Python re module | AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#12 |
| "Retire the INFRA.md Issue 14 workaround" | The workaround documentation must be updated to reflect the fix | AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | unescape applied to complex method pattern | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `unescape\(.*pattern` | [x] |
| 2 | Expected column unescape preserved (regression) | exit_code | python -m pytest src/tools/python/tests/test_ac_verifier_pipe_escape.py src/tools/python/tests/test_ac_verifier_escape.py -v | succeeds | - | [x] |
| 3 | Complex method test suite passes | exit_code | python -m pytest src/tools/python/tests/test_ac_verifier_complex_method.py -v | succeeds | - | [x] |
| 4 | unescape handles escaped parenthesis | exit_code | python -m pytest src/tools/python/tests/ -k "unescape_paren" -v | succeeds | - | [x] |
| 5 | unescape handles escaped dot | exit_code | python -m pytest src/tools/python/tests/ -k "unescape_dot" -v | succeeds | - | [x] |
| 6 | unescape handles escaped word char | exit_code | python -m pytest src/tools/python/tests/ -k "unescape_word" -v | succeeds | - | [x] |
| 7 | unescape handles escaped question mark | exit_code | python -m pytest src/tools/python/tests/ -k "unescape_question" -v | succeeds | - | [x] |
| 8 | Complex method pattern with escaped metachar end-to-end | exit_code | python -m pytest src/tools/python/tests/ -k "complex_method_unescape" -v | succeeds | - | [x] |
| 9 | Full existing test suite passes (regression) | exit_code | python -m pytest src/tools/python/tests/test_ac_verifier_*.py -v | succeeds | - | [x] |
| 10 | INFRA.md Issue 14 workaround updated | file | Grep(.claude/skills/feature-quality/INFRA.md) | matches | `Issue 14.*F817.*no longer` | [x] |
| 11 | glob parameter consumed in _extract_grep_params | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `parsed\.get.*glob` | [x] |
| 12 | glob parameter functionally filters files | exit_code | python -m pytest src/tools/python/tests/ -k "glob_filtering" -v | succeeds | - | [x] |

### AC Details

**AC#1: unescape applied to complex method pattern**
- **Test**: Grep `src/tools/python/ac-static-verifier.py` for regex `unescape\(.*pattern`
- **Expected**: Pattern found -- confirms `unescape()` is called on the pattern value extracted from complex method parsing. The fix should apply `unescape()` to the pattern retrieved via `parsed.get('pattern', ...)` in `_extract_grep_params()`.
- **Rationale**: Root cause fix (C1). The asymmetric data path where complex method patterns bypass `unescape()` is the primary bug. This AC verifies the fix is applied to the correct code path. Pattern verified as currently NOT matching (RED state confirmed).

**AC#2: Expected column unescape preserved (regression)**
- **Test**: `python -m pytest src/tools/python/tests/test_ac_verifier_pipe_escape.py src/tools/python/tests/test_ac_verifier_escape.py -v`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Backward compatibility (C2). The Expected column path through `unescape()` must continue working. These existing test files cover pipe escaping and quote escaping in the Expected column. Uses exit_code type per C7 bootstrapping constraint.

**AC#3: Complex method test suite passes**
- **Test**: `python -m pytest src/tools/python/tests/test_ac_verifier_complex_method.py -v`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Regression guard (C4). Complex method parsing (path=, pattern=, type=) must continue working after the unescape insertion. This existing test file covers basic, path-only, type-parameter, override, whitespace, and quoted-value variants.

**AC#4: unescape handles escaped parenthesis**
- **Test**: `python -m pytest src/tools/python/tests/ -k "unescape_paren" -v`
- **Expected**: Test passes, verifying `unescape(r'\\(')` returns `r'\('` and `unescape(r'\\)')` returns `r'\)'`
- **Rationale**: Extended escape coverage (C3, C7). `\\(` in markdown becomes "literal backslash + unclosed group" in Python regex, causing "unterminated subpattern" errors. This is the most common failure pattern in F804 (AC#9, #37). Test name verified as not existing yet (RED state).

**AC#5: unescape handles escaped dot**
- **Test**: `python -m pytest src/tools/python/tests/ -k "unescape_dot" -v`
- **Expected**: Test passes, verifying `unescape(r'\\.')` returns `r'\.'`
- **Rationale**: Extended escape coverage (C3, C7). `\\.` in markdown should become `\.` (escaped literal dot) in regex. Affects F804 AC#38, #55.

**AC#6: unescape handles escaped word char**
- **Test**: `python -m pytest src/tools/python/tests/ -k "unescape_word" -v`
- **Expected**: Test passes, verifying `unescape(r'\\w')` returns `r'\w'`
- **Rationale**: Extended escape coverage (C3, C7). `\\w` should become `\w` (word character class). Affects F804 AC#55.

**AC#7: unescape handles escaped question mark**
- **Test**: `python -m pytest src/tools/python/tests/ -k "unescape_question" -v`
- **Expected**: Test passes, verifying `unescape(r'\\?')` returns `r'\?'`
- **Rationale**: Extended escape coverage (C3, C7). `\\?` should become `\?` (zero-or-one quantifier). Affects F804 AC#39.

**AC#8: Complex method pattern with escaped metachar end-to-end**
- **Test**: `python -m pytest src/tools/python/tests/ -k "complex_method_unescape" -v`
- **Expected**: Test passes, verifying that a complex method AC with `pattern="def _extract.*\\("` correctly compiles and matches after unescape processing through the full `_extract_grep_params()` pipeline.
- **Rationale**: Integration test (C1, C3, C7). Verifies the end-to-end fix: `_parse_complex_method` extracts pattern -> `unescape()` processes it -> `re.search()` compiles and executes without error. This is the exact failure path that caused 12/47 FAIL in F804.

**AC#9: Full existing test suite passes (regression)**
- **Test**: `python -m pytest src/tools/python/tests/test_ac_verifier_*.py -v`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Full regression (C4). All 22 existing test files must pass after the fix. This is the comprehensive regression gate ensuring no existing functionality is broken.

**AC#10: INFRA.md Issue 14 workaround updated**
- **Test**: Grep `.claude/skills/feature-quality/INFRA.md` for pattern `Issue 14.*F817.*no longer`
- **Expected**: Pattern found, indicating the Issue 14 workaround documentation has been updated to reference F817 and note the workaround is no longer necessary. Uses conjunctive pattern requiring Issue 14 context, F817 reference, and retirement language in sequence.
- **Rationale**: Goal item 4. The Issue 14 workaround ("Use `.*` instead of literal pipe") was necessary because ac-static-verifier could not handle pipe/metacharacter escaping. With the fix, this workaround should reference F817 and note the reduced scope. The strengthened matcher verifies both the F817 reference and semantic retirement language appear together, preventing a false pass from an unrelated mention. Pattern verified as currently NOT matching in INFRA.md (RED state confirmed).

**AC#12: glob parameter functionally filters files**
- **Test**: `python -m pytest src/tools/python/tests/ -k "glob_filtering" -v`
- **Expected**: Test passes, verifying that a complex method AC with `glob="*.py"` restricts file search to .py files only. Test constructs an `ACDefinition` with `method='Grep(path="src/tools/python/", glob="*.py", pattern="import")'`, calls `verify_code_ac()`, and asserts that results only include .py files (no .md, .txt, etc.).
- **Rationale**: Behavioral verification for C5 (glob parameter consumed). AC#11 verifies the code reads `parsed.get('glob')`; AC#12 verifies the glob actually restricts file filtering end-to-end. Per C7 bootstrapping constraint, uses pytest.

**AC#11: glob parameter consumed in _extract_grep_params**
- **Test**: Grep `src/tools/python/ac-static-verifier.py` for regex `parsed\.get.*glob`
- **Expected**: Pattern found -- confirms the `glob` parameter from `_parse_complex_method()` is read via `parsed.get(...)` and used in `_extract_grep_params()` for file filtering.
- **Rationale**: Dead code fix (C5). The glob parameter is parsed by `_parse_complex_method()` but never consumed by `_extract_grep_params()`. This AC verifies the glob parameter is actually read and used. Pattern uses escaped dot (`\.`) for literal match. Verified as currently NOT matching (RED state confirmed).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Apply `unescape()` to complex method `pattern=` parameters | AC#1 |
| 2 | Extend `unescape()` to handle `\\(`, `\\)`, `\\.`, `\\w`, `\\?` | AC#4, AC#5, AC#6, AC#7 |
| 3 | Both data paths produce correct regex patterns for Python's `re` module | AC#2, AC#3, AC#8, AC#9 |
| 4 | Retire the INFRA.md Issue 14 workaround | AC#10 |
| 5 | Consume `glob` parameter from complex method (C5 dead code fix) | AC#11, AC#12 |

**Note**: 12 ACs is within the infra type range (8-15). C5 (glob parameter, AC#11) is included because it was identified as dead code during investigation and is a low-cost fix within the same `_extract_grep_params()` function. C6 (ac_ops.py not affected) requires no AC per constraint specification. All `matches`-type ACs (AC#1, AC#10, AC#11) verified as RED state. All pytest ACs (AC#4-AC#9, AC#12) are exit_code type per C7. (pattern currently not found in codebase). All pytest ACs use `exit_code` type per C7 bootstrapping constraint and template CRITICAL rule.

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

Three focused changes to `src/tools/python/ac-static-verifier.py`, each targeting one of the identified root causes:

**Change 1 - Apply `unescape()` to complex method pattern (C1, primary bug)**

In `_extract_grep_params()`, after extracting and backtick-stripping the `pattern` from the parsed complex method dict (currently lines 416-419), call `self.unescape(pattern)`. This mirrors the existing unescape call applied to `expected_raw` at line 792 during table parsing, making the two data paths symmetric.

The new code after the backtick-strip block:
```python
# Unescape markdown escape sequences in complex method pattern
# (mirrors Expected column unescape at line 792; unescape markdown escapes from parsed complex method pattern)
if pattern:
    pattern = self.unescape(pattern)
```

This directly satisfies C1 and unblocks all F804 ACs that had `\\(`, `\\.`, `\\w`, or `\\?` in their complex-method `pattern=` values. The code `self.unescape(pattern)` satisfies AC#1's grep pattern `unescape\(.*pattern`.

**Change 2 - Extend `unescape()` to cover additional markdown escape sequences (C3)**

The existing `unescape()` chain at line 220 handles only 4 sequences. Extend it with the sequences that appear in F804 ACs:

| Added sequence | Result | Source |
|----------------|--------|--------|
| `\\(` | `\(` | F804 AC#9, AC#37 - escaped open-paren |
| `\\)` | `\)` | F804 symmetry with `\\(` |
| `\\.` | `\.` | F804 AC#38, AC#55 - escaped dot |
| `\\?` | `\?` | F804 AC#39 - escaped question mark |
| `\\w` | `\w` | F804 AC#55 - word character class (per C3 spec; not CommonMark punctuation but included per explicit constraint) |

Add these as additional `.replace()` calls in the existing chain. Order does not matter for correctness because the sequences are all distinct two-character combinations.

**Change 3 - Consume `glob` parameter in `_extract_grep_params()` (C5)**

Currently `_parse_complex_method()` parses the `glob` parameter but `_extract_grep_params()` never reads `parsed.get('glob', ...)`. The fix incorporates the glob filter into the `file_path` string before it is passed to `_expand_glob_path()`:

```python
glob_param = parsed.get('glob')
if glob_param and file_path and not any(c in file_path for c in ['*', '?', '[']):
    file_path = file_path.rstrip('/') + '/' + glob_param
```

This works because `_expand_glob_path()` already handles `*` glob patterns via Python's `glob.glob()`. The guard `not any(c in file_path ...)` prevents double-globbing when `file_path` already has glob characters.

**Test additions (AC#4-AC#8, AC#11)**

New test file `src/tools/python/tests/test_ac_verifier_unescape_metachar.py` with five test functions:
- `test_unescape_paren` — verifies `unescape(r'\\(')` returns `r'\('` and `unescape(r'\\)')` returns `r'\)'`
- `test_unescape_dot` — verifies `unescape(r'\\.')` returns `r'\.'`
- `test_unescape_word` — verifies `unescape(r'\\w')` returns `r'\w'`
- `test_unescape_question` — verifies `unescape(r'\\?')` returns `r'\?'`
- `test_complex_method_unescape` — end-to-end test: creates `ACDefinition` with `method='Grep(path="src/tools/python/ac-static-verifier.py", pattern="def _extract.*\\(")'`, calls `verify_code_ac()`, verifies result is PASS (the pattern `def _extract.*\(` compiles and matches `def _extract_grep_params(` in ac-static-verifier.py)

These function names match the pytest `-k` filters used in AC#4-AC#8.

**Documentation update (AC#10)**

Update `.claude/skills/feature-quality/INFRA.md` Issue 14 section to note that the `.*` substitution workaround is no longer necessary after F817 fixes the complex-method pattern path. Add a sentence referencing F817.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Change 1 adds `pattern = self.unescape(pattern)`. The code `self.unescape(pattern)` contains `unescape(` followed by `pattern`, satisfying AC#1's grep pattern `unescape\(.*pattern`. |
| 2 | The Expected column unescape path (line 792) is not modified. Change 2 is purely additive new `.replace()` calls. Existing tests in `test_ac_verifier_pipe_escape.py` and `test_ac_verifier_escape.py` continue to pass. |
| 3 | Change 1 adds `unescape()` in complex method path but existing test patterns (`class PatternType`, `class ACVerifier`, etc.) contain none of the new escape sequences, so unescape is a no-op for them. All `test_ac_verifier_complex_method.py` tests continue to pass. |
| 4 | New `test_unescape_paren` function in `test_ac_verifier_unescape_metachar.py`; selected by `-k "unescape_paren"`. |
| 5 | New `test_unescape_dot` function; selected by `-k "unescape_dot"`. |
| 6 | New `test_unescape_word` function; selected by `-k "unescape_word"`. |
| 7 | New `test_unescape_question` function; selected by `-k "unescape_question"`. |
| 8 | New `test_complex_method_unescape` end-to-end function; selected by `-k "complex_method_unescape"`. Uses `pattern="def _extract.*\\("` in complex method; after Changes 1+2, unescape converts `\\(` to `\(`, regex compiles cleanly, matches file content. |
| 9 | Changes 1-3 are additive. New test file adds new tests, all passing. All 22 existing test files plus the new file pass. |
| 10 | INFRA.md Issue 14 updated with text containing `F817` and `no longer necessary`, satisfying the grep pattern. |
| 11 | Change 3 adds `parsed.get('glob')` call in `_extract_grep_params()`. Grep for `parsed.get.*glob` finds this line. |
| 12 | New `test_glob_filtering` function in `test_ac_verifier_unescape_metachar.py`; selected by `-k "glob_filtering"`. Uses `Grep(path="src/tools/python/", glob="*.py", pattern="import")` to verify glob filtering restricts results to .py files. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where to call `unescape()` in complex method path | A: In `_extract_grep_params` after backtick-strip, B: Inside `_parse_complex_method` at value-store time, C: In `_verify_content` at consumption | A (after backtick-strip in `_extract_grep_params`) | Mirrors the Expected column path structure (unescape after extraction, before use). `_parse_complex_method` is a pure key-value parser and should not apply semantic transforms. `_verify_content` receives already-processed patterns. |
| Glob parameter implementation strategy | A: Add fourth return value to `_extract_grep_params`, B: Incorporate glob into composite file_path string before `_expand_glob_path`, C: Filter after `_expand_glob_path` in `_verify_content` | B (composite file_path) | Zero change to return type and all call sites. `_expand_glob_path` already handles `*` glob patterns via Python's `glob.glob()`. Option A cascades changes through 2 call sites. Option C requires adding glob awareness to `_verify_content`. |
| Whether to add `\\w` to `unescape()` | A: Add it (per C3 spec), B: Exclude it (not CommonMark punctuation), C: Treat as Upstream Issue | A (add per C3 spec) | C3 explicitly lists `\\w`. F804 AC#55 uses it. The conversion `\\w` → `\w` produces a valid Python regex word-class. Risk is low (idempotent; only affects strings containing `\\w`). Comment in code documents it is per-spec, not CommonMark. |
| New test file vs. appending to existing | A: New file `test_ac_verifier_unescape_metachar.py`, B: Append to `test_ac_verifier_escape.py` | A (new dedicated file) | Existing `test_ac_verifier_escape.py` covers the quote-escape scenario from F792. A dedicated file keeps test organization consistent with the one-file-per-concern pattern used across 22 existing files. Also required so `-k "unescape_paren"` etc. filters are unambiguous across the test suite. |

### Interfaces / Data Structures

No new interfaces or data structures are introduced. All changes are surgical modifications to two existing methods in `src/tools/python/ac-static-verifier.py` plus one new test file.

**Modified: `ACVerifier.unescape(s: str) -> str`** (line 220 area)

Extended `.replace()` chain — new entries appended:
```python
return (s
    .replace(r'\"', '"')
    .replace(r'\\[', r'\[')
    .replace(r'\\]', r'\]')
    .replace(r'\|', '|')
    .replace(r'\\(', r'\(')   # NEW F817: escaped open-paren (CommonMark punctuation escape)
    .replace(r'\\)', r'\)')   # NEW F817: escaped close-paren
    .replace(r'\\.', r'\.')   # NEW F817: escaped dot
    .replace(r'\\?', r'\?')   # NEW F817: escaped question mark
    .replace(r'\\w', r'\w')   # NEW F817: word-class (per C3 spec; not CommonMark but per F804 evidence)
)
```

**Modified: `ACVerifier._extract_grep_params()`** — two insertion blocks after existing backtick-strip logic (currently line 419):
```python
# Unescape markdown escape sequences in complex method pattern
# (mirrors Expected column unescape at line 792; unescape markdown escapes from parsed complex method pattern)
if pattern:
    pattern = self.unescape(pattern)

# Consume glob parameter: filter path by glob suffix if present
glob_param = parsed.get('glob')
if glob_param and file_path and not any(c in file_path for c in ['*', '?', '[']):
    file_path = file_path.rstrip('/') + '/' + glob_param
```

**New file: `src/tools/python/tests/test_ac_verifier_unescape_metachar.py`**

Structure mirrors existing test files (importlib dynamic load, `repo_root` via `Path(__file__).parent.parent.parent`):
- `test_unescape_paren()` — unit test for `\\(` and `\\)` sequences
- `test_unescape_dot()` — unit test for `\\.` sequence
- `test_unescape_word()` — unit test for `\\w` sequence
- `test_unescape_question()` — unit test for `\\?` sequence
- `test_complex_method_unescape()` — integration test using `verify_code_ac()` with `pattern="def _extract.*\\("` against `src/tools/python/ac-static-verifier.py` (which contains `def _extract_grep_params(` matching `def _extract.*\(`)

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| (empty) | - | No upstream issues. AC#1 pattern `unescape\(.*pattern` is directly satisfied by the code `self.unescape(pattern)` without requiring specific comment wording. |


<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | In `_extract_grep_params()`, after backtick-stripping the `pattern` value, insert `if pattern: pattern = self.unescape(pattern)` with a comment containing `unescape markdown escapes from parsed complex method pattern` | | [x] |
| 2 | 2 | Run `python -m pytest src/tools/python/tests/test_ac_verifier_pipe_escape.py src/tools/python/tests/test_ac_verifier_escape.py -v` and confirm all tests pass (regression guard for Expected column unescape path) | | [x] |
| 3 | 3 | Run `python -m pytest src/tools/python/tests/test_ac_verifier_complex_method.py -v` and confirm all tests pass (regression guard for complex method parsing) | | [x] |
| 4 | 4 | Create `src/tools/python/tests/test_ac_verifier_unescape_metachar.py` and add `test_unescape_paren()` verifying `unescape(r'\\(')` returns `r'\('` and `unescape(r'\\)')` returns `r'\)'` | | [x] |
| 5 | 5 | Add `test_unescape_dot()` to `src/tools/python/tests/test_ac_verifier_unescape_metachar.py` verifying `unescape(r'\\.')` returns `r'\.'` | | [x] |
| 6 | 6 | Add `test_unescape_word()` to `src/tools/python/tests/test_ac_verifier_unescape_metachar.py` verifying `unescape(r'\\w')` returns `r'\w'` | | [x] |
| 7 | 7 | Add `test_unescape_question()` to `src/tools/python/tests/test_ac_verifier_unescape_metachar.py` verifying `unescape(r'\\?')` returns `r'\?'` | | [x] |
| 8 | 8 | Add `test_complex_method_unescape()` integration test to `src/tools/python/tests/test_ac_verifier_unescape_metachar.py`: construct an `ACDefinition` with `method='Grep(path="src/tools/python/ac-static-verifier.py", pattern="def _extract.*\\(")'`, call `verify_code_ac()`, assert result is PASS | | [x] |
| 9 | 9 | Extend `unescape()` at line 220 area with five new `.replace()` calls: `\\(` → `\(`, `\\)` → `\)`, `\\.` → `\.`, `\\?` → `\?`, `\\w` → `\w`, each annotated with `# NEW F817` comment | | [x] |
| 10 | 10 | Update `.claude/skills/feature-quality/INFRA.md` Issue 14 section to note the workaround is no longer necessary after F817 and add a sentence referencing `F817` | | [x] |
| 11 | 11 | In `_extract_grep_params()`, after extracting `file_path`, add `glob_param = parsed.get('glob')` and apply it as a suffix to `file_path` when `file_path` has no existing glob characters | | [x] |
| 12 | 12 | Add `test_glob_filtering()` to `src/tools/python/tests/test_ac_verifier_unescape_metachar.py`: construct ACDefinition with `method='Grep(path="src/tools/python/", glob="*.py", pattern="import")'`, call `verify_code_ac()`, assert only .py files are searched | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-817.md Tasks T9 | `unescape()` extended with 5 new sequences in `src/tools/python/ac-static-verifier.py` |
| 2 | implementer | sonnet | feature-817.md Tasks T1 | `unescape()` call inserted in `_extract_grep_params()` with required comment text |
| 3 | implementer | sonnet | feature-817.md Tasks T11 | `glob_param = parsed.get('glob')` consumed and applied in `_extract_grep_params()` |
| 4 | implementer | sonnet | feature-817.md Tasks T4, T5, T6, T7, T8, T12 | `src/tools/python/tests/test_ac_verifier_unescape_metachar.py` created with 6 test functions (unescape_paren, unescape_dot, unescape_word, unescape_question, complex_method_unescape, glob_filtering) |
| 5 | implementer | sonnet | feature-817.md Tasks T10 | `.claude/skills/feature-quality/INFRA.md` Issue 14 updated with F817 reference |
| 6 | ac-tester | sonnet | feature-817.md AC#1–AC#12 | All 12 ACs verified PASS; T2 and T3 regression runs confirmed |

### Pre-conditions

- F792 is [DONE]: `unescape()` exists in `src/tools/python/ac-static-verifier.py` at ~line 220
- F737 is [DONE]: quote-aware pipe handling state machine is present
- All existing `src/tools/python/tests/test_ac_verifier_*.py` tests pass before starting

### Execution Order

1. Phase 1 (T9) FIRST — extend `unescape()` before calling it from the new path, so Phase 2 immediately benefits from the full escape set
2. Phase 2 (T1) SECOND — insert `unescape()` call in `_extract_grep_params()`; this is the primary bug fix
3. Phase 3 (T11) THIRD — consume `glob` parameter; independent change in the same function
4. Phase 4 (T4–T8, T12) FOURTH — create test file with all 6 functions; do not create partial file
5. Phase 5 (T10) FIFTH — update INFRA.md; independent doc change
6. Phase 6 (T2, T3 regression + all ACs) LAST — run full verification after all code and doc changes are complete

### Build Verification Steps

After Phases 1–3 (code changes to `src/tools/python/ac-static-verifier.py`):
```bash
python -m pytest src/tools/python/tests/test_ac_verifier_*.py -v
```
Expected: all existing tests PASS (no regressions).

After Phase 4 (new test file added):
```bash
python -m pytest src/tools/python/tests/test_ac_verifier_unescape_metachar.py -v
```
Expected: all 5 new tests PASS.

### Error Handling

- If any existing test regresses after Phase 1–3: STOP → report to user before continuing
- If `test_complex_method_unescape` fails in Phase 4: verify that Phases 1 and 2 changes are both in place before debugging Phase 4
- If `parsed.get('glob')` is already consumed (Phase 3): verify line numbers against current file; do not duplicate

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists -> OK (file created during /run)
- Option B: Referenced Feature exists -> OK
- Option C: Phase exists in architecture.md -> OK
- Missing Task for Option A -> FL FAIL
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
| 2026-02-24 | Phase4 | implementer | T9,T1,T11: unescape extension + call + glob | SUCCESS |
| 2026-02-24 | Phase4 | implementer | T4-T8,T12: test file creation (6 tests) | SUCCESS |
| 2026-02-24 | Phase4 | implementer | T10: INFRA.md Issue 14 update | SUCCESS |
| 2026-02-24 | Phase4 | orchestrator | Regression: 130/130 tests PASS | SUCCESS |
| 2026-02-24 | Phase7 | ac-tester | AC#1-AC#12 verification | 12/12 PASS |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2 iter1: Implementation Contract AC#1 Comment Wording Constraint | Removed misleading constraint section — code `self.unescape(pattern)` already satisfies AC#1 grep pattern
- [fix] Phase2 iter1: AC#10 matcher | Strengthened from `F817` to `Issue 14.*F817|F817.*no longer` for semantic retirement verification
- [fix] Phase2 iter1: Tasks section | Moved AC Coverage Rule comment from before to after Tasks table per template
- [resolved-applied] Phase2 iter1: [CON-001] Technical Constraints states CommonMark escaping applies only to ASCII punctuation (\\b, \\s, \\d must NOT be converted) but C3/AC#6/Task#9 explicitly include \\w (a regex character class, not ASCII punctuation). Contradiction needs resolution: either remove \\w or update constraint to justify exception.
- [fix] PostLoop iter3: Technical Constraints | Added \\w exception clause with F804 evidence justification — resolves CON-001 contradiction
- [fix] Phase2 iter2: AC#8/Task#8/Technical Design | Corrected test pattern from `public.*Extract\\(` to `def _extract.*\\(` — Python has no `public` keyword; original pattern would never match
- [fix] Phase1-RefCheck iter1: Links section | Added Links for F774, F777, F796 (existing files) and expanded archived comment to include F593, F612, F635, F733, F755
- [fix] Phase2 iter3: Goal section | Added glob parameter dead code consumption (C5) to Goal section for traceability from Goal Item 5 in Goal Coverage table
- [fix] Phase2 iter4: AC#12 added | New AC verifying INFRA.md Issue 14 workaround guidance annotated with retirement language; strengthens Goal Item 4 coverage
- [fix] Phase2 iter5: AC#12→glob behavioral, old AC#12 removed | Replaced redundant AC#12 (overlapping with AC#10) with glob behavioral test (AC#13→AC#12 renumbered). Added Task#12 for test_glob_filtering. Updated all cross-references.
- [fix] PostLoop iter3: Technical Constraints | Added \\w exception clause with F804 evidence justification — resolves CON-001 contradiction
- [fix] Phase2 iter4(reloop): AC#10 matcher | Changed from disjunctive `Issue 14.*F817|F817.*no longer` to conjunctive `Issue 14.*F817.*no longer` — alternation allowed either branch alone to pass
- [fix] Phase2 iter5(reloop): Technical Design | Corrected test_complex_method_unescape description from 'matching Extract\(' to 'matching def _extract.*\(' — consistent with AC Details and actual file content

---

<!-- fc-phase-6-completed -->

## Links

[Predecessor: F792](feature-792.md) - Fixed Expected column unescape path; this feature completes the complex method path
[Related: F804](feature-804.md) - Trigger feature; 12/47 code ACs fail due to this bug
[Related: F801](feature-801.md) - Contains silent truncation examples from bare pipe in Expected
[Related: F798](feature-798.md) - Prior fix: re.MULTILINE and Format A parser improvements
[Related: F787](feature-787.md) - Encountered same class of issue pre-F792
[Related: F774](feature-774.md) - Affected feature: encountered same pipe-escaping issue
[Related: F777](feature-777.md) - Affected feature: encountered same pipe-escaping issue
[Related: F796](feature-796.md) - Affected feature: encountered same pipe-escaping issue
<!-- F737, F632, F468, F593, F612, F635, F733, F755: feature files do not exist in repo (archived). Referenced in Observable Symptom or Related Features table only. -->
