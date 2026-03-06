# Feature 834: ac-static-verifier Format C Guard DRY Consolidation and unescape() Investigation

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T00:50:14Z -->

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

## Background

### Philosophy (Mid-term Vision)
ac-static-verifier.py is the SSOT for AC verification logic. Each verification concern (Format C guard resolution, markdown unescaping, regex unescaping) should have exactly one implementation site, ensuring that modifications to any concern propagate consistently across all entry points (`verify_code_ac`, `verify_file_ac`).

### Problem (Current Issue)
Format C guard logic (Grep-count-matcher pattern-to-expected resolution) is duplicated verbatim at two call sites: `verify_code_ac` (lines 907-917) and `verify_file_ac` (lines 1059-1067). This duplication exists because Format C support was added incrementally during F832, and the guard was copy-pasted to both entry points rather than extracted into a shared helper. Future modifications to the guard logic must be applied to both sites, creating a structural divergence risk.

Additionally, `unescape()` at line 255 replaces `\|` with `|`, which was designed for markdown table pipe escaping in Expected column values. However, `_extract_grep_params` at line 491 also calls `unescape()` on Method-column regex patterns, where `\|` is a valid regex escape for literal pipe. This context mismatch causes silent corruption: a regex pattern like `(foo\|bar)` (meaning literal pipe) becomes `(foo|bar)` (meaning alternation). The `\\[` -> `\[` and `\\]` -> `\]` rules are safe because the double-backslash markdown encoding preserves the single-backslash regex escape after conversion.

### Goal (What to Achieve)
1. Extract the duplicated Format C guard logic into a helper method `_resolve_count_expected(ac, pattern)` returning `Optional[int]`, called from both `verify_code_ac` and `verify_file_ac`.
2. Fix `unescape()` pipe handling so that Method-column regex patterns preserve `\|` as a regex escape, while Expected-column values continue to have `\|` converted to `|` for markdown compatibility.

---

## Root Cause Analysis

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is Format C guard logic duplicated? | Both `verify_code_ac` and `verify_file_ac` need to resolve Format C parameters before calling `_verify_content` | `ac-static-verifier.py:907-917`, `:1059-1067` |
| 2 | Why wasn't it extracted during F832? | Format C was added incrementally; the guard was copy-pasted to both entry points as a quick fix | F832 commit `e0c5ede` |
| 3 | Why does `_extract_grep_params` not include the guard? | `_extract_grep_params` returns `(file_path, pattern, error_result)` and the Format C guard depends on `ac.matcher` and `ac.expected` post-extraction, making it a separate concern | `ac-static-verifier.py:445-516` |
| 4 | Why does `unescape()` corrupt `\|` in regex patterns? | `unescape()` was designed for markdown table pipe escaping in Expected column, but is also called on Method-column regex patterns at line 491 | `ac-static-verifier.py:255`, `:491` |
| 5 | Why (Root)? | No context-aware unescaping exists -- `unescape()` applies markdown-context rules uniformly regardless of whether input is Expected-column text or Method-column regex | `ac-static-verifier.py:230-260` (single static method for both contexts) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Duplicated 8-line Format C guard blocks; `\|` silently stripped from regex patterns | No shared helper for Format C resolution; `unescape()` lacks context-awareness for markdown vs regex escaping |
| Where | `verify_code_ac` and `verify_file_ac` entry points; `_extract_grep_params` call to `unescape()` | Incremental F832 addition without DRY extraction; single `unescape()` serving dual purposes |
| Fix | Copy-paste guard to new entry points; avoid `\|` in patterns | Extract `_resolve_count_expected()` helper; use separate unescape path for Method-column patterns |

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F832 | [DONE] | Direct predecessor; introduced Format C guard logic, explicitly deferred consolidation |
| F817 | [DONE] | Added `unescape()` metacharacter handling (`\\(`, `\\.`, `\\?`, `\\w`) |
| F804 | [DONE] | Established `\\w` word-class unescape rule |
| F818 | [DONE] | Introduced `_parse_complex_method` and complex method value extraction; added cross-repo/WSL support |
| F829 | [DONE] | Discovery context for F834 |

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Code locality | FEASIBLE | Both changes are in a single file (`ac-static-verifier.py`) with well-defined boundaries |
| Test infrastructure | FEASIBLE | Existing test files cover Format C and unescape; adding test cases is straightforward |
| Behavioral risk | FEASIBLE | Helper extraction is mechanical (identical code); unescape fix requires separate path but `unescape_for_literal_search()` at line 264 shows existing pattern |
| Complexity | FEASIBLE | Low complexity -- 8-line extraction + targeted unescape context fix |

**Verdict**: FEASIBLE

## Impact Analysis

| Area | Impact | Description |
|------|:------:|-------------|
| ac-static-verifier.py | MEDIUM | Refactor of Format C guard logic into helper; modification of unescape behavior for Method-column patterns |
| Existing AC test suite | LOW | All existing tests must continue to pass; new tests added for gap coverage |
| AC definitions using `\|` in Method | LOW | Currently latent bug; fix prevents future corruption when such patterns are used |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| `unescape()` is `@staticmethod` | `ac-static-verifier.py:230` | Helper or variant must also be static or accept explicit parameters |
| `unescape()` serves dual purpose (Expected column + Method column) | Lines 491, 868 | Fix must differentiate context or use separate unescape path |
| `unescape_for_literal_search()` exists as precedent | `ac-static-verifier.py:264-280` | Shows awareness of context-dependent unescaping; new variant can follow same pattern |
| Table parser already handles `\|` outside quotes | `ac-static-verifier.py:837-839` | Method-column patterns inside quotes bypass pipe backslash stripping |
| `_verify_content` expected_count parameter | `ac-static-verifier.py:518` | Helper must produce same `Optional[int]` value as current inline logic |
| `pattern != ac.expected` guard must be preserved | Lines 913, 1063 | Edge case where pattern equals expected must be handled identically |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| `\|` fix breaks Expected-column pipe unescaping | MEDIUM | HIGH | Use separate unescape path for Method-column patterns; keep existing `unescape()` for Expected column |
| Refactored helper has subtly different condition logic | LOW | MEDIUM | Exact line-for-line extraction; existing test suite covers Format C paths |
| `\|` in Method-column patterns is rare/nonexistent in current ACs | HIGH | LOW | Bug is latent; test proves the fix works for future usage |
| Future divergence of extracted helper and callers | LOW | LOW | Single call site eliminates the risk entirely |

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Format C guard occurrences | `grep -c "pattern != ac.expected" ac-static-verifier.py` | 2 | Should become 1 after extraction (only in helper) |
| `unescape` pipe rule | `grep -c "replace.*\\\\|.*|" ac-static-verifier.py` | 1 | Current rule at line 255 |
| Test file count | `ls src/tools/python/tests/test_ac_verifier_*.py` | 24 files | All must pass after changes |

**Baseline File**: `_out/tmp/baseline-834.txt`

---

## AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `_resolve_count_expected` must be called from BOTH `verify_code_ac` and `verify_file_ac` | DRY requirement | AC must verify both call sites delegate to helper |
| C2 | Expected column `\|` -> `|` must still work | Backward compatibility | AC must verify Expected column pipe unescaping unchanged |
| C3 | Method-column `\|` must preserve backslash for regex | Bug fix goal | AC must verify regex `\|` preserved in Method-column patterns |
| C4 | All 24 existing test files must pass | Regression safety | Full test suite regression gate required |
| C5 | Duplicated Format C guard block must no longer exist | DRY verification | Use count_equals to verify only 1 occurrence of the guard logic |

### Constraint Details

**C1: Helper Called from Both Sites**
- **Source**: Investigation found identical 8-line blocks at `verify_code_ac:907-917` and `verify_file_ac:1059-1067`
- **Verification**: Grep for helper call in both methods
- **AC Impact**: AC must verify the helper method exists AND is invoked from both entry points

**C2: Expected Column Backward Compatibility**
- **Source**: `unescape()` line 255 is correct for Expected column (markdown table `\|` -> `|`)
- **Verification**: Test with Expected column containing `\|` and verify it becomes `|`
- **AC Impact**: AC must include positive test for Expected column pipe handling

**C3: Method-Column Regex Preservation**
- **Source**: `_extract_grep_params` at line 491 calls `unescape()` on Method-column patterns, corrupting `\|`
- **Verification**: Test with Method-column pattern containing `\|` and verify backslash is preserved
- **AC Impact**: AC must include test case for `\|` in regex context from Method column

**C4: Full Test Suite Regression**
- **Source**: 24 test files in `src/tools/python/tests/test_ac_verifier_*.py`
- **Verification**: Run full test suite
- **AC Impact**: AC must verify all existing tests pass

**C5: DRY Verification**
- **Source**: Current duplication at two sites
- **Verification**: Count occurrences of the guard logic pattern
- **AC Impact**: count_equals to verify exactly 1 occurrence of the resolution logic

---

<!-- fc-phase-2-completed -->

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F832 | [DONE] | Introduced Format C guard logic (commit `e0c5ede`); explicitly deferred consolidation to this feature |

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

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "each verification concern...should have exactly one implementation site" | Format C guard logic must exist in exactly one place (the helper) | AC#1, AC#5 |
| "each verification concern (Format C guard resolution, markdown unescaping, regex unescaping) should have exactly one implementation site" | Method-column regex patterns must use a dedicated unescape path separate from Expected-column markdown unescaping | AC#7, AC#9, AC#10 |
| "modifications to any concern propagate consistently across all entry points" | Both `verify_code_ac` and `verify_file_ac` must delegate to the shared helper | AC#2, AC#3 |
| "ac-static-verifier.py is the SSOT for AC verification logic" | All existing tests must continue to pass after refactoring | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | `_resolve_count_expected` helper method exists | code | Grep(src/tools/python/ac-static-verifier.py, pattern="def _resolve_count_expected") | matches | `def _resolve_count_expected` | [x] |
| 2 | `verify_code_ac` calls `_resolve_count_expected` | code | Grep(src/tools/python/ac-static-verifier.py, pattern="_resolve_count_expected") | gte | 3 | [x] |
| 3 | `verify_file_ac` calls `_resolve_count_expected` | code | Grep(src/tools/python/ac-static-verifier.py, pattern="self._resolve_count_expected|self\\.resolve_count_expected") | gte | 2 | [x] |
| 4 | Helper returns Optional[int] | code | Grep(src/tools/python/ac-static-verifier.py, pattern="_resolve_count_expected.*Optional") | matches | `_resolve_count_expected.*Optional` | [x] |
| 5 | Duplicated Format C guard comment eliminated | code | Grep(src/tools/python/ac-static-verifier.py, pattern="pass expected_count separately") | count_equals | 1 | [x] |
| 5b | Duplicated Format C guard logic eliminated | code | Grep(src/tools/python/ac-static-verifier.py, pattern="isdigit.*pattern != ac.expected") | count_equals | 1 | [x] |
| 6 | Inline `expected_count = None` assignment eliminated from callers | code | Grep(src/tools/python/ac-static-verifier.py, pattern="expected_count = None") | count_equals | 1 | [x] |
| 7 | Method-column `\|` preserved as regex escape | exit_code | pytest src/tools/python/tests/ -k "pipe" --tb=short -q | succeeds | - | [x] |
| 8 | All existing test files pass (regression) | exit_code | pytest src/tools/python/tests/test_ac_verifier*.py --tb=short -q | succeeds | - | [x] |
| 9 | `unescape()` retains markdown pipe escape rule | code | Grep(src/tools/python/ac-static-verifier.py, pattern="replace.*markdown pipe escapes") | matches | `replace.*markdown pipe escapes` | [x] |
| 10 | `_extract_grep_params` does not call `unescape()` on pattern | code | Grep(src/tools/python/ac-static-verifier.py, pattern="pattern = self.unescape\\(pattern\\)") | not_matches | `pattern = self.unescape(pattern)` | [x] |
| 11 | Shared `_UNESCAPE_RULES` list is the single source for unescape rules | code | Grep(src/tools/python/ac-static-verifier.py, pattern="_UNESCAPE_RULES") | gte | 3 | [x] |

### AC Details

**AC#2: `verify_code_ac` calls `_resolve_count_expected`**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="_resolve_count_expected")`
- **Expected**: `gte 3` (1 definition + at least 2 call sites in `verify_code_ac` and `verify_file_ac`)
- **Rationale**: Constraint C1 requires the helper to be called from BOTH entry points. 3 = 1 def + 2 calls minimum.
- **Derivation**: 1 `def _resolve_count_expected` line + 1 call in `verify_code_ac` + 1 call in `verify_file_ac` = minimum 3 occurrences.

**AC#3: `verify_file_ac` calls `_resolve_count_expected`**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="self._resolve_count_expected|self\\.resolve_count_expected")`
- **Expected**: `gte 2` (both `verify_code_ac` and `verify_file_ac` must call it)
- **Rationale**: Constraint C1 verification from caller perspective. Pattern matches `self._resolve_count_expected(` or `self.resolve_count_expected(` call expressions.
- **Derivation**: 1 call in `verify_code_ac` + 1 call in `verify_file_ac` = 2 minimum call sites.

**AC#5: Duplicated Format C guard comment eliminated**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="pass expected_count separately")`
- **Expected**: `count_equals 1` (only in the helper method's docstring/comment, not at two inline sites)
- **Rationale**: Constraint C5 — DRY verification. Currently 2 occurrences (baseline). After extraction, the comment should appear only once (in the helper).
- **Derivation**: Baseline = 2 occurrences at lines 908 and 1060. After refactoring = 1 occurrence in `_resolve_count_expected` helper.

**AC#5b: Duplicated Format C guard logic eliminated**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="isdigit.*pattern != ac.expected")`
- **Expected**: `count_equals 1` (only in `_resolve_count_expected` helper, not at two inline sites)
- **Rationale**: Complementary to AC#5 — verifies actual guard logic code is consolidated, not just comments.
- **Derivation**: Currently 2 occurrences (lines ~913, ~1063). After refactoring = 1 in helper.

**AC#6: Inline `expected_count = None` assignment eliminated from callers**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="expected_count = None")`
- **Expected**: `count_equals 1` (only in the helper, not at two inline sites)
- **Rationale**: Complementary DRY check to AC#5. Currently 2 occurrences (lines 909, 1061). After extraction, the initialization should exist only in the helper.
- **Derivation**: Baseline = 2 occurrences at lines 909 and 1061. After refactoring = 1 occurrence in `_resolve_count_expected` helper.

**AC#11: Shared `_UNESCAPE_RULES` list is the single source for unescape rules**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="_UNESCAPE_RULES")`
- **Expected**: `gte 3` (1 definition + at least 2 usages in `unescape()` and `unescape_for_regex_pattern()`)
- **Rationale**: Ensures unescape rules are defined once and shared across all variants. Adding a new rule requires editing only `_UNESCAPE_RULES`.
- **Derivation**: 1 `_UNESCAPE_RULES = [...]` definition + 1 reference in `unescape()` + 1 reference in `unescape_for_regex_pattern()` = minimum 3 occurrences.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extract duplicated Format C guard logic into `_resolve_count_expected(ac, pattern)` returning `Optional[int]`, called from both `verify_code_ac` and `verify_file_ac` | AC#1, AC#2, AC#3, AC#4, AC#5, AC#5b, AC#6, AC#8 |
| 2 | Fix `unescape()` pipe handling with shared rule list so Method-column regex patterns preserve `\|` while Expected-column values continue `\|` to `|` conversion | AC#7, AC#8, AC#9, AC#10, AC#11 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Two independent, mechanical changes to `src/tools/python/ac-static-verifier.py`:

**Change 1 — Extract `_resolve_count_expected` helper (DRY)**

The identical 4-line conditional block (lines 907-914 and 1061-1064) is extracted verbatim into a new instance method `_resolve_count_expected(ac, pattern) -> Optional[int]`. Both `verify_code_ac` and `verify_file_ac` replace their inline block with a single call to this helper. The helper owns the `expected_count = None` initialization and the Format C guard logic, so neither caller needs to re-declare the variable. The method signature is `def _resolve_count_expected(self, ac: ACDefinition, pattern: str) -> Optional[int]`.

**Change 2 — Shared unescape rule list with context-aware application**

`_extract_grep_params` currently calls `self.unescape(pattern)` at line 491. `unescape()` includes `r'\|' -> '|'`, which is correct for Expected-column markdown text but corrupts regex patterns where `\|` means literal pipe. The fix introduces a class-level `_UNESCAPE_RULES` list of `(from, to)` tuples and a `_PIPE_RULE` constant. `unescape()` applies all rules (including pipe). A new static method `unescape_for_regex_pattern(s: str) -> str` applies all rules **except** `_PIPE_RULE`. Both methods draw from the shared `_UNESCAPE_RULES` list, ensuring new rules are added in exactly one place. `_extract_grep_params` switches from `self.unescape(pattern)` to `self.unescape_for_regex_pattern(pattern)`. The `\|` rule in `unescape()` gains an inline comment labeling it `"markdown pipe escapes"`, satisfying AC#9.

**Unescape Strategy** (three variants, one shared rule source):

| Variant | Context | Rules Applied | Used By |
|---------|---------|---------------|---------|
| `unescape()` | Expected column (markdown text) | All `_UNESCAPE_RULES` | `_verify_content`, Expected column processing |
| `unescape_for_regex_pattern()` | Method column (regex patterns) | All except `_PIPE_RULE` | `_extract_grep_params` |
| `unescape_for_literal_search()` | Literal search patterns | Subset (existing, unchanged) | Literal search matchers |

New escape rules MUST be added to `_UNESCAPE_RULES` only. Both `unescape()` and `unescape_for_regex_pattern()` automatically pick up new rules.

**Why this approach satisfies all 11 ACs:**
- The helper method signature and `-> Optional[int]` return type annotation satisfy AC#1 and AC#4.
- The helper is called from both `verify_code_ac` and `verify_file_ac`, satisfying AC#2 and AC#3.
- The inline blocks are removed, so the guard comment `"pass expected_count separately"` and the `expected_count = None` assignment each appear exactly once (inside the helper), satisfying AC#5 and AC#6.
- A new test file `test_ac_verifier_pipe_regex.py` with a `-k "pipe"` selectable test verifies that `\|` in a Method-column pattern is preserved, satisfying AC#7.
- All existing test expectations remain satisfied because `unescape_for_regex_pattern` differs from `unescape` only by omitting the pipe rule, which no existing test pattern relies upon from the Method column, satisfying AC#8.
- The inline comment in `unescape()` includes `"markdown pipe escapes"`, satisfying AC#9.
- `_extract_grep_params` no longer contains `self.unescape(pattern)`, satisfying AC#10.
- The shared `_UNESCAPE_RULES` list is the single source for rules, and `unescape_for_regex_pattern` draws from it, satisfying AC#11.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `def _resolve_count_expected(self, ac: ACDefinition, pattern: str) -> Optional[int]:` as an instance method |
| 2 | Both `verify_code_ac` and `verify_file_ac` call `self._resolve_count_expected(ac, pattern)` — total occurrences of the name in the file ≥ 3 (1 def + 2 calls) |
| 3 | Both call sites use the `self._resolve_count_expected(...)` form — 2 `self.` prefixed call expressions |
| 4 | Method signature includes `-> Optional[int]` return type annotation |
| 5 | The comment `"pass expected_count separately"` is moved into the helper's docstring; both former inline sites are replaced by the single call, leaving exactly 1 occurrence |
| 5b | Guard logic pattern `isdigit.*pattern != ac.expected` appears only in the helper (count_equals 1) |
| 6 | `expected_count = None` initialization lives only inside `_resolve_count_expected`; the two former call sites assign via the return value (`expected_count = self._resolve_count_expected(...)`) |
| 7 | New test file `test_ac_verifier_pipe_regex.py` with a test function name containing `"pipe"`, verifying that a Grep pattern containing `\|` reaches `_verify_content` with the backslash preserved |
| 8 | `_extract_grep_params` uses `unescape_for_regex_pattern(pattern)` which draws from shared `_UNESCAPE_RULES` excluding pipe; no existing test pattern uses `\|` in the Method column, so all 24 test files continue to pass |
| 9 | `unescape()` `\|` replacement line gains inline comment `# markdown pipe escapes` (or equivalent text matching the grep pattern) |
| 10 | The string `pattern = self.unescape(pattern)` is removed from `_extract_grep_params` and replaced with `pattern = self.unescape_for_regex_pattern(pattern)` |
| 11 | `_UNESCAPE_RULES` is referenced by both `unescape()` and `unescape_for_regex_pattern()` — ≥ 3 occurrences (1 definition + 2 usages) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Scope of `_resolve_count_expected` | staticmethod vs instance method | Instance method | Takes `ac: ACDefinition` parameter; follows pattern of other `verify_*` methods that take `ac` |
| Placement of helper | Near `_extract_grep_params`, near `verify_code_ac`, near `_verify_content` | Before `verify_code_ac` (after `_verify_content`) | Pre-processing step for `_verify_content`'s `expected_count` param; co-located with its consumer |
| Unescape fix strategy | (A) Add `is_regex` flag to `unescape()`, (B) New variant with shared rule list, (C) Inline skip at call site | (B) Shared `_UNESCAPE_RULES` list + new variant `unescape_for_regex_pattern()` | Follows `unescape_for_literal_search()` precedent for variant naming; shared rule list eliminates DRY debt by ensuring new rules are added in exactly one place |
| Which rules to omit in new variant | Only `\|` rule, or also bracket/paren rules | Only `\|` rule (via `_PIPE_RULE` constant) | `\\[` -> `\[` and similar rules are safe for regex: they convert markdown double-backslash encoding to single-backslash which regex expects; only `\|` has a context conflict |
| Test file for pipe-in-regex | Extend `test_ac_verifier_pipe_escape.py` vs new file | New file `test_ac_verifier_pipe_regex.py` | Existing file tests Expected-column pipe unescaping; new concern (Method-column regex preservation) deserves a dedicated file for discoverability |

### Interfaces / Data Structures

**New method: `_resolve_count_expected`**

```python
def _resolve_count_expected(self, ac: ACDefinition, pattern: str) -> Optional[int]:
    """Resolve Format C expected_count from AC definition and extracted pattern.

    For count matchers (count_equals, gt, gte, lt, lte), when the Expected column
    contains a bare integer AND the pattern was extracted from the Method column
    (i.e., pattern != ac.expected), pass expected_count separately to _verify_content.

    This guard (pattern != ac.expected) distinguishes Format C (pattern from Method
    column) from Format A/B where ac.expected IS the pattern.

    Args:
        ac: AC definition with matcher and expected fields
        pattern: The regex/literal pattern extracted by _extract_grep_params

    Returns:
        int if matcher is a count matcher AND ac.expected is a bare integer AND
        pattern differs from ac.expected; None otherwise
    """
    expected_count = None
    if ac.matcher.lower() in ("count_equals", "gt", "gte", "lt", "lte"):
        if ac.expected.strip().isdigit() and pattern != ac.expected:
            expected_count = int(ac.expected.strip())
    return expected_count
```

**Class-level shared rule list: `_UNESCAPE_RULES`**

```python
# Shared unescape rules — single source for all unescape variants.
# New rules MUST be added here only. Both unescape() and unescape_for_regex_pattern() draw from this list.
_UNESCAPE_RULES = [
    (r'\"', '"'),
    (r'\\[', r'\['),
    (r'\\]', r'\]'),
    (r'\\(', r'\('),
    (r'\\)', r'\)'),
    (r'\\.', r'\.'),
    (r'\\?', r'\?'),
    (r'\\w', r'\w'),
]
_PIPE_RULE = (r'\|', '|')  # markdown pipe escapes — Expected column only
```

**Updated `unescape()`** — uses `_UNESCAPE_RULES` + `_PIPE_RULE`:

```python
@staticmethod
def unescape(s: str) -> str:
    for from_str, to_str in ACVerifier._UNESCAPE_RULES:
        s = s.replace(from_str, to_str)
    s = s.replace(*ACVerifier._PIPE_RULE)  # markdown pipe escapes
    return s
```

**New method: `unescape_for_regex_pattern`** — uses `_UNESCAPE_RULES` only (excludes pipe rule):

```python
@staticmethod
def unescape_for_regex_pattern(s: str) -> str:
    r"""Unescape backslash escape sequences for Method-column regex patterns.

    Applies all _UNESCAPE_RULES EXCEPT _PIPE_RULE, because in regex context
    \| means literal pipe and must be preserved as-is.

    Args:
        s: Pattern string from Method column with potential markdown escape sequences

    Returns:
        String with markdown escapes processed, regex pipe escape preserved
    """
    for from_str, to_str in ACVerifier._UNESCAPE_RULES:
        s = s.replace(from_str, to_str)
    return s
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 4, 5, 5b, 6 | Add `_resolve_count_expected(self, ac, pattern) -> Optional[int]` instance method to `ac-static-verifier.py` with docstring containing "pass expected_count separately", `expected_count = None` initialization, and `-> Optional[int]` return annotation | | [x] |
| 2 | 2, 3 | Update `verify_code_ac` and `verify_file_ac` to replace inline Format C guard block with `expected_count = self._resolve_count_expected(ac, pattern)` call | | [x] |
| 3 | 9, 10, 11 | Add `_UNESCAPE_RULES` class-level shared rule list and `_PIPE_RULE` constant; refactor `unescape()` to use shared rules; add `unescape_for_regex_pattern` static method using shared rules excluding pipe; update `_extract_grep_params` to call `unescape_for_regex_pattern(pattern)` instead of `unescape(pattern)` | | [x] |
| 4 | 7 | Create `src/tools/python/tests/test_ac_verifier_pipe_regex.py` with test function name containing "pipe" verifying that `\|` in a Method-column Grep pattern is preserved as regex escape after `unescape_for_regex_pattern` | | [x] |
| 5 | 8 | Run full regression suite `pytest src/tools/python/tests/test_ac_verifier*.py --tb=short -q` and verify all tests pass | | [x] |

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

**Example**:
```markdown
| 1 | 1 | Add API endpoint | | [ ] |        ← KNOWN: Expected response format is specified
| 2 | 2 | Calculate aggregates | [I] | [ ] | ← UNCERTAIN: Actual totals unknown until implemented
| 3 | 3 | Format report | | [ ] |           ← KNOWN: Uses Task 2's output (determined after Task 2)
```

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-834.md Tasks 1-2, `src/tools/python/ac-static-verifier.py` | `_resolve_count_expected` method added; `verify_code_ac` and `verify_file_ac` updated |
| 2 | implementer | sonnet | feature-834.md Task 3, `src/tools/python/ac-static-verifier.py` | `_UNESCAPE_RULES` + `_PIPE_RULE` added; `unescape()` refactored to use shared rules; `unescape_for_regex_pattern` added; `_extract_grep_params` updated |
| 3 | implementer | sonnet | feature-834.md Task 4 | `src/tools/python/tests/test_ac_verifier_pipe_regex.py` created |
| 4 | tester | sonnet | feature-834.md Task 5, `src/tools/python/tests/` | Regression suite result |

### Pre-conditions

- `ac-static-verifier.py` must be read in full before editing to locate exact insertion points
- All edits are in a single file (`src/tools/python/ac-static-verifier.py`) except the new test file
- No other files are modified

### Execution Order

**Phase 1 — Extract `_resolve_count_expected` helper (Tasks 1-2)**

1. Read `src/tools/python/ac-static-verifier.py` to locate:
   - The inline Format C guard block in `verify_code_ac` (search for `pass expected_count separately`)
   - The identical block in `verify_file_ac`
   - An appropriate insertion point just before `verify_code_ac` (after `_verify_content`)

2. Insert `_resolve_count_expected` instance method at the located insertion point using the exact signature and docstring from Technical Design section "Interfaces / Data Structures".

3. In `verify_code_ac`, replace the inline guard block with:
   ```python
   expected_count = self._resolve_count_expected(ac, pattern)
   ```

4. In `verify_file_ac`, replace the identical inline guard block with:
   ```python
   expected_count = self._resolve_count_expected(ac, pattern)
   ```

5. Verify: `grep -c "_resolve_count_expected" ac-static-verifier.py` returns ≥ 3 (1 def + 2 calls).

**Phase 2 — Add shared unescape rules and `unescape_for_regex_pattern` (Task 3)**

1. Read `unescape()` at its current location to confirm the `\|` replacement line and all existing rules.

2. Add class-level `_UNESCAPE_RULES` list and `_PIPE_RULE` constant before `unescape()`, using the exact implementation from Technical Design "Interfaces / Data Structures".

3. Refactor `unescape()` to iterate over `_UNESCAPE_RULES` and apply `_PIPE_RULE`, using the exact implementation from Technical Design.

4. Insert `unescape_for_regex_pattern` as a `@staticmethod` immediately after `unescape()`, iterating `_UNESCAPE_RULES` only (excluding pipe rule), using the exact implementation from Technical Design.

5. In `_extract_grep_params`, replace `self.unescape(pattern)` with `self.unescape_for_regex_pattern(pattern)`.

6. Verify: `grep "pattern = self.unescape(pattern)" ac-static-verifier.py` returns no matches.
7. Verify: `grep -c "_UNESCAPE_RULES" ac-static-verifier.py` returns ≥ 3.

**Phase 3 — Create pipe-regex test file (Task 4)**

1. Create `src/tools/python/tests/test_ac_verifier_pipe_regex.py`.

2. The test function name MUST contain the word `pipe` (for `-k "pipe"` selector).

3. Test scenario: construct an AC definition with a Grep Method-column pattern containing `\|`, invoke `_extract_grep_params` (or `unescape_for_regex_pattern` directly), and assert the backslash is preserved in the output.

4. The test must be selectable via `pytest -k "pipe"`.

**Phase 4 — Full regression (Task 5)**

Run:
```
pytest src/tools/python/tests/test_ac_verifier*.py --tb=short -q
```

All 24 existing test files plus the new file must pass. STOP if any test fails.

### Build Verification Steps

```bash
# After Phase 1-2: verify AC coverage conditions
grep -c "_resolve_count_expected" src/tools/python/ac-static-verifier.py
# Expected: >= 3

grep -c "pass expected_count separately" src/tools/python/ac-static-verifier.py
# Expected: 1

grep -c "expected_count = None" src/tools/python/ac-static-verifier.py
# Expected: 1

grep "pattern = self.unescape(pattern)" src/tools/python/ac-static-verifier.py
# Expected: no output (not_matches)

grep "markdown pipe escapes" src/tools/python/ac-static-verifier.py
# Expected: 1 match

grep -c "_UNESCAPE_RULES" src/tools/python/ac-static-verifier.py
# Expected: >= 3

grep -c "isdigit.*pattern != ac.expected" src/tools/python/ac-static-verifier.py
# Expected: 1

# After Phase 3-4: regression
pytest src/tools/python/tests/test_ac_verifier*.py --tb=short -q
pytest src/tools/python/tests/ -k "pipe" --tb=short -q
```

### Success Criteria

- All 11 ACs pass (AC#1-6, 5b, 7-11)
- `_resolve_count_expected` appears ≥ 3 times in the file (1 def + 2 calls)
- `self._resolve_count_expected(` appears ≥ 2 times (both call sites)
- `pass expected_count separately` appears exactly 1 time (in helper docstring only)
- `isdigit.*pattern != ac.expected` appears exactly 1 time (in helper only)
- `expected_count = None` appears exactly 1 time (in helper body only)
- `pattern = self.unescape(pattern)` does not appear in `_extract_grep_params`
- `markdown pipe escapes` appears in `unescape()`
- `_UNESCAPE_RULES` appears ≥ 3 times (1 definition + 2 usages in unescape methods)
- `unescape_for_regex_pattern` method exists and is a `@staticmethod`
- `_resolve_count_expected.*Optional` matches in the file (return type annotation)
- New test file `test_ac_verifier_pipe_regex.py` exists and is selectable via `-k "pipe"`
- Full test suite passes

### Rollback Plan

If issues arise after implementation:
1. Revert all changes with `git checkout -- src/tools/python/ac-static-verifier.py`
2. Remove the new test file: `git rm src/tools/python/tests/test_ac_verifier_pipe_regex.py`
3. Report failure to user with specific failing AC number and error output
4. Do NOT attempt autonomous workarounds

### Error Handling

| Situation | Action |
|-----------|--------|
| Test failure after Phase 1-2 (helper extraction) | STOP — Report failing test + diff to user |
| `grep -c "expected_count = None"` returns 2 after Phase 1 | Helper insertion missed; old inline site not removed |
| `grep "pattern = self.unescape(pattern)"` still matches after Phase 2 | `_extract_grep_params` edit not applied |
| New pipe test fails in Phase 4 | `unescape_for_regex_pattern` incorrectly omits a rule or retains the `\|` rule |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

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
| 2026-03-06T01:10:00Z | START | initializer | Status [REVIEWED] → [WIP] | READY |
<!-- run-phase-1-completed -->
| 2026-03-06T01:12:00Z | PHASE_2 | explorer | Investigation complete | READY |
<!-- run-phase-2-completed -->
| 2026-03-06T01:15:00Z | PHASE_4 | implementer | Tasks 1-5 complete (148/148 pass) | SUCCESS |
<!-- run-phase-4-completed -->
| 2026-03-06T01:18:00Z | PHASE_7 | ac-tester | 11/11 ACs PASS (code 9/9 + exit_code 2/2) | SUCCESS |
<!-- run-phase-7-completed -->
| 2026-03-06T01:20:00Z | PHASE_8 | feature-reviewer | Quality OK, 8.2 skipped (no extensibility), 8.3 N/A | READY |
<!-- run-phase-8-completed -->
| 2026-03-06T01:22:00Z | DEVIATION | Bash | ac-static-verifier --ac-type exit_code | exit 2 (invalid argument; exit_code not a valid ac-type) |
| 2026-03-06T01:25:00Z | PHASE_10 | finalizer | [WIP] → [DONE], commit 1484f25 | READY_TO_COMMIT |
| 2026-03-06T01:26:00Z | CodeRabbit | 0 findings | - |
<!-- run-phase-10-completed -->

---

## Review Notes

- [fix] Phase2-Review iter1: Baseline Measurement section | Missing Baseline File line after table
- [fix] Phase2-Review iter1: Philosophy Derivation table | Unescape-related ACs (AC#7, AC#9, AC#10) not traced to Philosophy claim
- [fix] Phase3-Maintainability iter2: Technical Design > unescape_for_regex_pattern | DRY debt — refactored to shared _UNESCAPE_RULES list
- [fix] Phase3-Maintainability iter2: Technical Design > unescape strategy | Added unescape strategy documentation table
- [fix] Phase3-Maintainability iter2: AC Definition Table | Added AC#5b (guard logic pattern count) for DRY verification robustness
- [fix] Phase3-Maintainability iter3: AC Details section | Removed duplicate AC#5b Details block

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

<!-- fc-phase-6-completed -->

## Links
- [Predecessor: F832](feature-832.md) - ac-static-verifier Numeric Expected Parsing Fix
- [Related: F817](feature-817.md) - unescape() metacharacter handling
- [Related: F804](feature-804.md) - \\w word-class unescape rule
- [Related: F818](feature-818.md) - Complex method parsing and cross-repo support
- [Related: F829](feature-829.md) - Discovery context for F834
