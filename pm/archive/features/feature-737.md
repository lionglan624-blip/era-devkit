# Feature 737: ac-static-verifier Escaped Quote and Pipe Regex Handling

## Status: [DONE]

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

## Background

### Philosophy (Mid-term Vision)
Reliable automated AC verification infrastructure that correctly handles all markdown-legal Expected value patterns.

### Problem (Current Issue)
ac-static-verifier.py fails to correctly parse AC Expected values containing:
1. Escaped double quotes (e.g., `"\"Write\""`) - the backslash-escaped quotes cause incorrect pattern extraction
2. Pipe characters in regex patterns (e.g., `medium\|low`) - the pipe is not properly handled as regex alternation

Discovered during F733 verification where AC#6 (`"Write"` contains check) and AC#23 (`medium|low` matches check) both reported FAIL despite the patterns being present in the target files.

### Goal (What to Achieve)
Fix ac-static-verifier.py to correctly handle escaped quotes in Expected column values and pipe characters in regex patterns.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| - | - | - | No dependencies |

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: AC#6 (`"Write"` contains check) and AC#23 (`medium|low` matches check) both report FAIL despite patterns being present in target files
2. Why: The Expected values extracted from the markdown AC table are incorrect (truncated or malformed)
3. Why: The markdown table parser at line 549 uses `line.split("|")` which splits on ALL pipe characters including those inside Expected values, and line 565 uses `strip('"')` which greedily removes ALL leading/trailing `"` characters including escaped ones
4. Why: The parser treats markdown table delimiters (`|`) and quote-wrapping (`"..."`) as simple character-level operations without understanding escape semantics
5. Why: The original parser was designed for simple Expected values and never handled the case where Expected values themselves contain pipe characters or escaped double quotes that interact with the stripping logic

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| AC#6 reports FAIL for `"Write"` contains check | `strip('"')` at line 565 greedily removes ALL trailing `"` chars, consuming both the wrapper quote and the escaped quote, producing `"Write\` instead of `"Write"` |
| AC#23 reports FAIL for `medium\|low` matches check | `line.split("\|")` at line 549 splits on the `\|` pipe inside the Expected value, truncating it to `"medium\` |

### Conclusion

There are two distinct bugs in `parse_feature_markdown()` (lines 518-584 of `tools/ac-static-verifier.py`):

**Bug 1: Greedy `strip('"')` on line 565**

The Expected column value `"\"Write\""` in the raw markdown contains these characters: `"`, `\`, `"`, `W`, `r`, `i`, `t`, `e`, `\`, `"`, `"`. When `strip('"')` is applied, it removes:
- 1 leading `"` (correct)
- 2 trailing `"` characters (incorrect - strips both the wrapper `"` AND the escaped quote's `"`)

Result: `\"Write\` → after `unescape()`: `"Write\` (malformed, missing closing quote). The search then looks for the literal string `"Write\` which does not exist in the target file.

**Fix approach**: Replace `strip('"')` with logic that removes only one matching pair of outer quotes (e.g., check if string starts and ends with `"`, then slice `[1:-1]`).

**Bug 2: Naive `line.split("|")` on line 549**

The markdown row `| 23 | ... | matches | "medium\|low" | [x] |` is split on every `|` character. The `|` inside `medium\|low` is treated as a column delimiter, producing `parts[6]` = `"medium\` and `parts[7]` = `low"`. The Expected value is truncated.

**Fix approach**: Implement pipe-aware splitting that respects escaped pipes (`\|`) or quoted regions within cells. Alternatively, since `\|` in regex means literal pipe alternation, the parser could rejoin split segments when a cell appears to be split mid-quote.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F722 | [DONE] | Same component | Added count_equals/gt/gte/lt/lte matchers to ac-static-verifier. Different parsing area (matcher dispatch, not Expected parsing) |
| F733 | [DONE] | Discovery trigger | The two failing ACs (AC#6, AC#23) in F733 exposed these bugs. Manually verified as PASS |

### Pattern Analysis

This is the first time these specific parsing edge cases have been encountered. Previous verifier enhancements (F722) focused on matcher logic, not Expected value parsing. The pattern suggests that as AC definitions grow more complex (regex patterns, quoted strings), the simple split-and-strip parser becomes increasingly fragile. A more robust parsing approach (quote-aware, escape-aware) would prevent future similar issues.

## Feasibility

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Both bugs are in `parse_feature_markdown()` (lines 549, 565). Fix is localized to table parsing logic. |
| Scope is realistic | YES | Two targeted fixes: (1) replace `strip('"')` with single-pair quote removal, (2) implement pipe-aware splitting or rejoin logic |
| No blocking constraints | YES | No predecessor dependencies. Existing test suite in `tools/tests/` provides regression safety |

**Verdict**: FEASIBLE

## Impact

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ac-static-verifier.py | Update | Fix `parse_feature_markdown()` line 549 (pipe splitting) and line 565 (quote stripping) |
| tools/tests/test_ac_verifier_escape.py | Update | Add test cases for escaped quotes inside Expected values (end-to-end parsing) |

This fix enables:
1. Correct verification of ACs with Expected values containing double quotes (e.g., `"\"Write\""` for matching `"Write"` in code)
2. Correct verification of ACs with Expected values containing pipe characters in regex patterns (e.g., `medium\|low` for regex alternation)
3. Reliable automated AC verification for F733 and any future features using these patterns

## Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Must not break existing AC parsing | 20 existing test files in tools/tests/ | HIGH - regression tests must continue to pass |
| Markdown table format is the AC definition source | CLAUDE.md AC Definition Format | MEDIUM - fix must work within markdown table constraints |
| `unescape()` method must remain compatible | Used by both parsing and verification paths | LOW - changes should be in parsing, not unescape itself |
| Unquoted Expected values with pipes not supported | State machine requires quotes to detect pipe delimiters inside cells | LOW - AC format recommends quoting Expected values containing special characters |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Quote removal change breaks other Expected formats | Low | Medium | Existing 20 test files provide comprehensive regression coverage |
| Pipe-aware splitting introduces new parsing edge cases | Low | Medium | Limit fix to `\|` escape handling; add targeted test cases |
| Other unescaped special chars in Expected values | Low | Low | Track as separate feature if discovered; this fix addresses only `"` and `\|` |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "correctly handle escaped quotes" | Parser must preserve escaped double quotes in Expected values after stripping outer quotes | AC#1, AC#2 |
| "correctly handle pipe characters in regex patterns" | Parser must not split on pipes inside Expected values (regex alternation) | AC#3, AC#4 |
| "Must not break existing AC parsing" (Constraint) | All existing tests must continue to pass after the fix | AC#5, AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Escaped quote parsing: single pair outer quote removal | exit_code | pytest tools/tests/test_ac_verifier_escape.py -k "parse_escaped_quote_expected" | succeeds | - | [x] |
| 2 | Escaped quote end-to-end: Expected value `"Write"` extracted correctly from markdown row | exit_code | pytest tools/tests/test_ac_verifier_escape.py -k "parse_markdown_escaped_quote" | succeeds | - | [x] |
| 3 | Pipe in Expected: regex alternation `medium\|low` not split by pipe delimiter | exit_code | pytest tools/tests/test_ac_verifier_escape.py -k "parse_markdown_pipe_in_expected" | succeeds | - | [x] |
| 4 | Pipe in Expected: multi-pipe regex `a\|b\|c` preserved intact | exit_code | pytest tools/tests/test_ac_verifier_escape.py -k "parse_markdown_multi_pipe" | succeeds | - | [x] |
| 5 | Regression: all existing ac-static-verifier tests pass | exit_code | pytest tools/tests/ | succeeds | - | [x] |
| 6 | Regression: simple Expected values without escapes still parsed correctly | exit_code | pytest tools/tests/test_ac_verifier_escape.py -k "parse_markdown_simple_expected" | succeeds | - | [x] |
| 7 | State machine split equivalence: produces identical parts arrays as split("|") for normal rows | exit_code | pytest tools/tests/test_ac_verifier_escape.py -k "state_machine_split_equivalence" | succeeds | - | [x] |

### AC Details

**AC#1: Escaped quote parsing - single pair outer quote removal**
Verifies that the fix for Bug 1 correctly removes only one pair of outer quotes instead of greedily stripping all `"` characters. The test should construct a raw Expected string like `"\"Write\""` (as it appears after cell extraction), apply the new quote-removal logic, and assert the result is `\"Write\"` (which after unescape becomes `"Write"`). This directly targets the `strip('"')` bug on line 565.

**AC#2: Escaped quote end-to-end markdown row parsing**
Verifies the full pipeline from raw markdown table row to final Expected value. A markdown row containing `| 6 | desc | code | Grep(path) | contains | "\"Write\"" | [ ] |` should be parsed by `parse_feature_markdown()` and yield an ACDefinition with `expected == '"Write"'`. This is an integration-level test ensuring the quote removal and unescape steps work together correctly.

**AC#3: Pipe in Expected - regex alternation preserved**
Verifies that the fix for Bug 2 prevents `line.split("|")` from splitting on escaped pipes inside Expected values. A markdown row containing `| 23 | desc | code | Grep(path) | matches | "medium\|low" | [ ] |` should parse to an ACDefinition with `expected == 'medium\|low'` (after unescape). This directly targets the naive split bug on line 549.

**AC#4: Pipe in Expected - multi-pipe regex preserved**
Extends AC#3 to handle multiple pipe characters in a single regex pattern (e.g., `a\|b\|c`). This guards against partial fixes that only handle a single pipe occurrence. The parser must rejoin or avoid splitting all escaped pipes, not just the first one.

**AC#5: Regression - all existing tests pass**
Runs the entire test suite under `tools/tests/` to ensure the parsing changes do not break any existing functionality. The 20 existing test files cover backtick handling, bracket escapes, glob patterns, numeric matchers, binary file handling, and more. All must continue to pass.

**AC#6: Regression - simple Expected values unchanged**
Verifies that Expected values without any special characters (no escaped quotes, no pipes) continue to be parsed correctly after the fix. This guards against over-engineering the new parsing logic in a way that breaks the common case (e.g., simple string like `"hello world"` must still parse to `hello world`).

**AC#7: State machine split equivalence**
Verifies that the new quote-aware state machine produces identical `parts` arrays as the original `split("|")` for normal markdown rows without quotes or pipes in Expected values. Tests several representative normal AC rows to ensure the refactored splitting logic is a drop-in replacement and doesn't alter parts array indexing assumptions (e.g., `parts[6]` for Expected value).

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The fix requires two targeted changes in `parse_feature_markdown()` (lines 518-584):

**Bug 1 Fix: Replace greedy `strip('"')` with single-pair quote removal (line 565)**

Current code:
```python
expected_raw = parts[6].strip('"').strip('`')
```

Fixed code:
```python
expected_raw = parts[6]
# Remove single pair of outer double quotes (maintain original order: quotes first)
if expected_raw.startswith('"') and expected_raw.endswith('"') and len(expected_raw) >= 2:
    expected_raw = expected_raw[1:-1]
# Remove outer backticks (legacy support)
if expected_raw.startswith('`') and expected_raw.endswith('`'):
    expected_raw = expected_raw[1:-1]
```

**Rationale**: `strip('"')` removes ALL leading and trailing `"` characters, not just one pair. For `"\"Write\""`, this produces `\"Write\` (missing the closing `"`). The fix explicitly checks for matching start/end quotes and slices exactly one character from each end.

**Bug 2 Fix: Implement quote-aware pipe splitting (line 549)**

Current code:
```python
parts = [p.strip() for p in line.split("|")]
```

Fixed code:
```python
# Split on pipes, but respect quoted regions (don't split pipes inside quotes)
parts = []
current_part = ""
in_quotes = False
i = 0
while i < len(line):
    char = line[i]
    if char == '"':
        # Count consecutive backslashes before the quote
        num_backslashes = 0
        j = i - 1
        while j >= 0 and line[j] == '\\':
            num_backslashes += 1
            j -= 1
        # Even number of backslashes means quote is NOT escaped
        if num_backslashes % 2 == 0:
            in_quotes = not in_quotes
        current_part += char
    elif char == '|' and not in_quotes:
        # Pipe outside quotes - split here
        parts.append(current_part.strip())
        current_part = ""
    else:
        current_part += char
    i += 1
# Add final part
if current_part or not parts:
    parts.append(current_part.strip())
```

**Rationale**: The naive `split("|")` treats all `|` characters as column delimiters, even those inside Expected values like `"medium\|low"`. The fix implements a state machine that tracks whether we're inside a double-quoted region. When `in_quotes=True`, `|` characters are treated as part of the cell content, not as delimiters. The escaped quote detection (`line[i-1] != '\\'`) ensures that `\"` inside a quoted region doesn't toggle the quote state.

**Alternative considered**: Regex-based splitting with lookahead/lookbehind was considered but rejected due to complexity and edge case fragility (e.g., nested quotes, escaped quotes). The state machine approach is more explicit and debuggable.

### AC Coverage

| AC# | Coverage | Verification |
|:---:|----------|--------------|
| AC#1 | Bug 1 fix | Unit test: construct raw string `"\"Write\""`, apply new quote-removal logic, assert result is `\"Write\"` (which unescape converts to `"Write"`) |
| AC#2 | Bug 1 fix | Integration test: parse markdown row `\| 6 \| desc \| code \| Grep(path) \| contains \| "\"Write\"" \| [ ] \|`, assert ACDefinition.expected == `"Write"` |
| AC#3 | Bug 2 fix | Integration test: parse markdown row `\| 23 \| desc \| code \| Grep(path) \| matches \| "medium\|low" \| [ ] \|`, assert ACDefinition.expected == `medium\|low` (after unescape) |
| AC#4 | Bug 2 fix | Integration test: parse markdown row with `"a\|b\|c"`, assert ACDefinition.expected == `a\|b\|c` (multiple pipes preserved) |
| AC#5 | Both fixes | Regression test: run entire `pytest tools/tests/` suite, assert all pass (existing tests validate no breakage) |
| AC#6 | Bug 1 fix | Unit test: parse markdown row with simple Expected `"hello world"`, assert ACDefinition.expected == `hello world` (no regression on simple case) |
| AC#7 | Bug 2 fix | Unit test: compare state machine split results with `split("|")` for normal markdown rows (no special chars), assert identical parts arrays |

### Key Decisions

**Decision 1: State machine vs regex for pipe splitting**

- **Chosen**: State machine (character-by-character traversal with quote tracking)
- **Rejected**: Regex with negative lookbehind `(?<!\\)\|`
- **Rationale**: The regex approach cannot distinguish pipes inside vs outside quoted regions (it only detects escaped pipes). The markdown table format uses quotes to delimit cell values containing special characters, so we must track quote boundaries. State machine is more maintainable and handles nested/escaped quotes explicitly.

**Decision 2: Order of backtick and quote stripping**

- **Chosen**: Maintain original order (quotes first, then backticks) with single-pair removal
- **Original**: Current production code does `strip('"').strip('`')` (quotes first)
- **Change**: Replace greedy `strip()` with single-pair removal `[1:-1]` while preserving processing order
- **Rationale**: Order reversal is unnecessary behavioral change without proven benefit. Maintaining original order minimizes risk of subtle regressions for untested edge cases.

**Decision 3: Escaped quote detection in state machine**

- **Chosen**: Count consecutive backslashes before quote character and check parity
- **Approach**: Even count (including 0) = unescaped quote; odd count = escaped quote
- **Handles**: All escape levels (`\"`, `\\"`, `\\\"`, etc.) correctly without limitations

**Decision 4: Test placement and implementation**

- **Chosen**: Extend existing `test_ac_verifier_escape.py` with new test functions for AC#1-#7
- **Implementation**: Use `tmp_path` pytest fixture to create temporary feature markdown files for integration tests (AC#2, AC#3, AC#4, AC#7). Unit tests (AC#1, AC#6) can call parse logic directly with string inputs.
- **Rationale**: The test file already exists and handles escape-related testing. Adding new functions maintains consistency with existing test organization.

**Decision 5: `\|` unescape behavior**

- **Chosen**: `\|` in Expected values is NOT unescaped to `|` by design
- **Behavior**: After pipe-aware split and quote removal, `medium\|low` preserves the backslash-pipe sequence
- **Rationale**: In regex context (matches matcher), `\|` represents an escaped pipe character (literal `|` match in some regex flavors, alternation in others like Python). The `unescape()` method only handles quote escapes (`\"` → `"`), not regex escapes. Users requiring true regex alternation can use `"medium|low"` (quoted to protect from markdown pipe splitting).
- **Constraint**: Expected values containing unescaped pipe characters for alternation must be quoted in markdown table to prevent incorrect parsing.

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Fix Bug 1: Replace greedy strip('"') with single-pair quote removal logic in parse_feature_markdown() line 565 | [x] |
| 2 | 3,4 | Fix Bug 2: Replace naive split("|") with quote-aware state machine splitting in parse_feature_markdown() line 549 | [x] |
| 3 | 1,2,3,4,6,7 | Add test cases to test_ac_verifier_escape.py for escaped quotes and pipes in Expected values | [x] |
| 4 | 5 | Run full regression test suite (pytest tools/tests/) to verify no breakage | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T3 | AC#1-#4, AC#6-#7 test specifications | New failing test functions in test_ac_verifier_escape.py (RED) |
| 2 | implementer | sonnet | T1 | Technical Design Bug 1 Fix | Updated parse_feature_markdown() with single-pair quote removal (GREEN) |
| 3 | implementer | sonnet | T2 | Technical Design Bug 2 Fix | Updated parse_feature_markdown() with quote-aware pipe splitting (GREEN) |
| 4 | ac-tester | haiku | T4 | AC#5 test command | Regression test results |

**Constraints** (from Technical Design):
1. Must preserve backward compatibility with existing AC parsing (20 test files)
2. Quote-aware splitting must handle escaped quotes (`\"`) correctly
3. State machine must track quote boundaries to distinguish pipes inside vs outside quoted regions
4. Changes limited to parse_feature_markdown() function (lines 549, 565)

**Pre-conditions**:
- tools/ac-static-verifier.py exists and is functional
- tools/tests/test_ac_verifier_escape.py exists (created in F722)
- Existing 20 test files in tools/tests/ provide regression coverage

**Success Criteria**:
- All 7 ACs marked `[x]` (PASS)
- F733 AC#6 (`"Write"` contains check) and AC#23 (`medium|low` matches check) now report correct results
- No regression in existing test suite (20 test files all pass)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert {commit-hash}`
2. Notify user of rollback with issue description
3. Create follow-up feature for fix with additional investigation of edge cases

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-02 | /fc | Generated Root Cause, ACs, Technical Design, Tasks via /fc workflow |
| 2026-02-02 07:41 | T3 | Added test cases for AC#1-#4, AC#6-#7 to test_ac_verifier_escape.py (RED phase) |
| 2026-02-02 07:55 | T1 | Replaced greedy strip('"') with single-pair quote removal on line 565. Tests AC#1, AC#2, AC#6 now PASS (GREEN phase) |
| 2026-02-02 07:44 | T2 | Replaced naive split("\|") with quote-aware state machine splitting on line 549. All 10 tests PASS (GREEN phase) |
| 2026-02-02 | Phase 4 | T4: Full regression suite - all tests pass |
| 2026-02-02 | AC Verification | All 7 ACs verified PASS: AC#1-7 all exit_code 0. AC#5 (full regression): 112/112 tests pass |

## Links

- [F722](feature-722.md) - ac-static-verifier count matchers (same component)
- [F733](feature-733.md) - Dashboard Recovery Plan (discovery trigger)
