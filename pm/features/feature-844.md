# Feature 844: _UNESCAPE_RULES catch-all regex refactoring

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T11:29:34Z -->

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
| Parent Feature | F842 |
| Discovery Phase | Phase 4 (Implementation) |
| Timestamp | 2026-03-06 |

### Observable Symptom
`_UNESCAPE_RULES` in ac-static-verifier.py grows by adding individual entries for each regex character class (e.g., `\\s`->`\s`, `\\S`->`\S`). F842 added 9 rules, bringing total to 17. This per-class enumeration approach does not scale and risks missing new character classes.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | N/A (design observation) |
| Exit Code | N/A |
| Error Output | N/A |
| Expected | Generic regex pattern handles all character class unescaping |
| Actual | 17 individual tuple entries in `_UNESCAPE_RULES` |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | `_UNESCAPE_RULES` at lines ~285-310 |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | - |

### Parent Session Observations
Replace per-class entries with generic regex `r'\\\\([sSdDwWbBAZ])' → r'\\1'` to handle all single-character escape sequences uniformly. Must preserve existing behavior for multi-character escapes like `\\[`, `\\]`, `\\"`.

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->
## Background

### Philosophy (Mid-term Vision)
ac-static-verifier is the SSOT for automated AC verification. Its unescape pipeline should use scalable patterns rather than enumerated rules, so that new regex character classes are handled automatically without code changes.

### Problem (Current Issue)
The `_UNESCAPE_RULES` list in ac-static-verifier.py contains 17 individual tuple entries (lines 307-325), because the original design used `str.replace()` which requires explicit enumeration of every escape sequence. When F842 added 9 character-class entries (`\\s`, `\\S`, `\\d`, `\\D`, `\\W`, `\\b`, `\\B`, `\\A`, `\\Z`), they were added as individual tuples following the existing pattern, since no refactoring pass introduced a generic `re.sub()` approach. Of the 17 entries, 10 follow the identical uniform pattern `\\X` -> `\X` for a single letter X (lines 315-324 including `\\w` from F804), while 7 are structural/punctuation escapes with heterogeneous from/to mappings (lines 308-314). The uniform entries can be collapsed into a single `re.sub()` call.

### Goal (What to Achieve)
Replace the 10 uniform single-letter `_UNESCAPE_RULES` entries with a single `re.sub()` call using a generic regex pattern, reducing enumeration burden and enabling automatic handling of future single-letter escape sequences. Preserve the 7 structural/punctuation rules and `_PIPE_RULE` separation unchanged.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are there 17 individual tuple entries? | Because F842 added 9 character-class entries as individual tuples, following the existing enumeration pattern | ac-static-verifier.py:315-324 |
| 2 | Why did F842 add them as individual tuples? | Because the established `_UNESCAPE_RULES` list (F834) used `str.replace()` tuples, and each deviation fix added the minimum necessary rule | pm/features/feature-834.md, pm/features/feature-842.md |
| 3 | Why does `_UNESCAPE_RULES` use `str.replace()`? | Because the original 8 entries were heterogeneous (different from/to structures like `\"` -> `"`, `\\[` -> `\[`), where enumeration was adequate | ac-static-verifier.py:307-314 |
| 4 | Why was no generic pattern introduced when uniform entries appeared? | Because each feature (F804, F842) was a deviation fix targeting specific AC failures, not a design pass | pm/features/feature-842.md:30-31 |
| 5 | Why (Root)? | The unescape pipeline grew reactively per-failure rather than being designed with a hybrid approach (str.replace for heterogeneous + re.sub for uniform) | ac-static-verifier.py:305-306 comment codifies enumeration |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 17 individual tuple entries in `_UNESCAPE_RULES`, 10 of which are uniform single-letter patterns | `str.replace()` loop inherently requires enumeration; no refactoring step introduced `re.sub()` when uniform entries were added |
| Where | ac-static-verifier.py:307-325 | Design choice at ac-static-verifier.py:305-306 ("New rules MUST be added here only") codifying enumeration |
| Fix | Add more tuples for each new character class | Replace 10 uniform entries with a single `re.sub()` call; keep 7 structural rules as `str.replace()` |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F842 | [WIP] | Parent -- added the 9 character-class rules that motivate this refactoring |
| F834 | [DONE] | Established `_UNESCAPE_RULES` as shared list SSOT |
| F817 | [DONE] | Added metachar rules (`\\(`, `\\)`, `\\.`, `\\?`) |
| F804 | [DONE] | Origin of `\\w` rule |
| F792 | [DONE] | Original pipe/quote unescape work |
| F841 | [DONE] | Grandparent feature |
| F845 | [PROPOSED] | Sibling deviation -- full AC pattern catalog scan |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Technical complexity | FEASIBLE | Replacing 10 uniform tuples with one `re.sub()` is straightforward; `_contains_regex_metacharacters` at line 391 already uses regex as precedent |
| Risk level | FEASIBLE | `\b` backspace hazard is well-understood and has existing guard test (test:76-84) |
| Scope containment | FEASIBLE | 2 files affected, no cross-repo changes |
| Test coverage | FEASIBLE | Existing test suite covers all 17 escapes; update count assertion only |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| ac-static-verifier unescape pipeline | MEDIUM | Replaces 10 entries with 1 re.sub() call; changes iteration logic |
| Existing AC verification | LOW | All currently-verified ACs continue to pass (behavioral preservation) |
| Future extensibility | HIGH | New single-letter escape sequences handled automatically without code changes |
| Test suite | LOW | Count assertion update; behavior tests unchanged |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `\b` must produce `\b` (2 chars), not `\x08` (backspace) | test_ac_verifier_unescape_charclass.py:76-84 | `re.sub()` replacement must use raw string `r'\1'` |
| Multi-char structural escapes must remain as explicit `str.replace()` rules | ac-static-verifier.py:308-314 | Bracket/paren are regex metachar; cannot merge into generic pattern |
| `_PIPE_RULE` separation must be preserved | ac-static-verifier.py:326,351,369 | Context-dependent (excluded from regex variant); unaffected by change |
| `str.replace()` order matters for heterogeneous rules | ac-static-verifier.py:349-351 | Heterogeneous rules must stay ordered in list |
| `unescape_for_literal_search` is independent | ac-static-verifier.py:372-388 | No change needed; has its own hardcoded replacements |
| Character class in regex must be well-defined | Risk: too-broad regex catches structural escapes | Must use explicit char class, not `[a-zA-Z]` |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| `re.sub()` replacement produces interpreted escape (e.g., `\b` -> backspace) | LOW | HIGH | Use raw string `r'\1'` in replacement; existing guard test at test:76-84 catches this |
| Catch-all regex matches too broadly (e.g., catches structural escapes) | LOW | MEDIUM | Use explicit character class `[sSdDwWbBAZ]` or similar restricted set |
| Count-based test assertion breaks | HIGH | LOW | Update `len >= 17` assertion to reflect reduced rule count or verify pattern existence instead |
| `re.sub()` ordering interaction with remaining `str.replace()` rules | LOW | MEDIUM | Apply `re.sub()` after `str.replace()` loop for heterogeneous rules |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| _UNESCAPE_RULES entry count | `grep -c "^        (r'" src/tools/python/ac-static-verifier.py` | 17 | Lines 307-325 (tuples in list) |
| Uniform single-letter entries | Manual count of `\\X -> \X` pattern entries | 10 | Lines 315-324 (\\w, \\s, \\S, \\d, \\D, \\W, \\b, \\B, \\A, \\Z) |
| Structural/punctuation entries | Manual count of heterogeneous entries | 7 | Lines 308-314 |
| Unescape test count | `grep -c "def test_" src/tools/python/tests/test_ac_verifier_unescape_charclass.py` | (from test file) | Existing behavioral tests |

**Baseline File**: `_out/tmp/baseline-844.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Generic pattern must handle all currently-tested single-letter escapes (s, S, d, D, w, W, b, B, A, Z) | ac-static-verifier.py:315-324 | AC must verify all 10 letters still unescape correctly |
| C2 | `\b` must remain `\b` (2 chars), not `\x08` (backspace) | test_ac_verifier_unescape_charclass.py:76-84 | Explicit assertion needed in AC |
| C3 | 7 structural/punctuation rules must be preserved unchanged | ac-static-verifier.py:308-314 | Verify 7 rules remain as str.replace() tuples |
| C4 | Generic pattern must handle letters NOT in the current explicit list | Feature goal (extensibility) | Test a previously-uncovered letter (e.g., `\\G`) |
| C5 | Rule count must decrease from 17 | Feature goal (simplification) | Verify count < 17 |
| C6 | `_PIPE_RULE` separation must be preserved | ac-static-verifier.py:326,351,369 | Verify `_PIPE_RULE` still separate |
| C7 | `unescape_for_literal_search` must not be modified | ac-static-verifier.py:372-388 | Scope exclusion -- verify unchanged |

### Constraint Details

**C1: Single-letter escape behavioral preservation**
- **Source**: All 3 investigations confirmed 10 entries (lines 315-324) follow uniform `\\X -> \X` pattern
- **Verification**: Run existing test suite; all character-class unescape tests must pass
- **AC Impact**: AC must verify each of s, S, d, D, w, W, b, B, A, Z still produces correct output

**C2: Backspace hazard prevention**
- **Source**: Python interprets `\b` as backspace (`\x08`) in non-raw strings; test at line 76-84 guards against this
- **Verification**: `assert unescape(r'\\b') == r'\b'` and `len(result) == 2`
- **AC Impact**: Explicit test that `\\b` produces 2-char `\b`, not 1-char `\x08`

**C3: Structural rule preservation**
- **Source**: Lines 308-314 have heterogeneous from/to mappings (e.g., `\"` -> `"`, `\\[` -> `\[`) that cannot be generalized
- **Verification**: Grep for the 7 structural tuples still present in `_UNESCAPE_RULES`
- **AC Impact**: Count or grep verification that structural rules remain as explicit tuples

**C4: Extensibility for uncovered letters**
- **Source**: Feature goal -- generic pattern should handle future character classes without code changes
- **Verification**: Test with a letter not in the original 10 (e.g., `\\G`, `\\x`)
- **AC Impact**: AC must test at least one novel letter to prove genericity

**C5: Rule count reduction**
- **Source**: Feature goal -- 10 uniform entries replaced by 1 re.sub() call
- **Verification**: Count entries in `_UNESCAPE_RULES` list
- **AC Impact**: Verify count <= 8 (7 structural + at most 1 quote rule)

**C6: Pipe rule separation**
- **Source**: `_PIPE_RULE` is context-dependent (applied in `unescape()` but not `unescape_for_regex_pattern()`)
- **Verification**: Grep for `_PIPE_RULE` still defined separately
- **AC Impact**: Verify `_PIPE_RULE` is not merged into the generic pattern

**C7: Literal search independence**
- **Source**: `unescape_for_literal_search` (lines 372-388) is a separate method with its own hardcoded replacements
- **Verification**: Diff the method before/after; must be unchanged
- **AC Impact**: Scope guard -- AC may verify method body is unchanged or simply exclude from scope

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F842 | [DONE] | Logical ordering -- F844 refactors the rules F842 added; should complete first |
| Related | F834 | [DONE] | Established `_UNESCAPE_RULES` as shared SSOT list |
| Related | F817 | [DONE] | Added metachar rules (\\(, \\), \\., \\?) |
| Related | F804 | [DONE] | Origin of `\\w` rule |
| Related | F845 | [PROPOSED] | Sibling deviation -- full AC pattern catalog scan |

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
| "should use scalable patterns rather than enumerated rules" | Generic re.sub() replaces 10 individual entries | AC#1, AC#2 |
| "handled automatically without code changes" | Novel single-letter escapes work without adding entries | AC#5 |
| "SSOT for automated AC verification" | Existing behavioral tests continue to pass | AC#6 |
| "should use scalable patterns" | Structural/punctuation rules preserved as explicit entries | AC#3 |
| "should use scalable patterns rather than enumerated rules" | _PIPE_RULE separation unchanged | AC#4 |
| "SSOT for automated AC verification" | unescape_for_literal_search scope guard (unchanged) | AC#7 |
| "should use scalable patterns rather than enumerated rules" | Rule count quantitatively reduced | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Generic re.sub() call exists in ac-static-verifier.py | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `re\.compile` | [x] |
| 2 | Individual single-letter entries removed from _UNESCAPE_RULES | code | Grep(src/tools/python/ac-static-verifier.py, pattern="^        \\(r'\\\\\\\\[a-zA-Z]") | lte | 7 | [x] |
| 3 | 7 structural/punctuation rules preserved in _UNESCAPE_RULES | code | Grep(src/tools/python/ac-static-verifier.py, pattern="^        \\(r'") | gte | 7 | [x] |
| 4 | _PIPE_RULE remains separate from _UNESCAPE_RULES | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `_PIPE_RULE` | [x] |
| 5 | Novel letter extensibility (e.g., \\G handled without code change) | exit_code | pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py | succeeds | - | [x] |
| 6 | All existing unescape tests pass | exit_code | pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py | succeeds | - | [x] |
| 7 | unescape_for_literal_search method unchanged | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `def unescape_for_literal_search` | [x] |
| 8 | _UNESCAPE_RULES total entry count reduced | code | Grep(src/tools/python/ac-static-verifier.py, pattern="^        \\(r'") | lte | 8 | [x] |

### AC Details

**AC#2: Individual single-letter entries removed from _UNESCAPE_RULES**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="^        \\(r'\\\\\\\\[a-zA-Z]")`
- **Expected**: `lte 7` — after refactoring, no single-letter `\\X -> \X` tuples should remain as individual entries. Current count is 10 (lines 315-324: \\w, \\s, \\S, \\d, \\D, \\W, \\b, \\B, \\A, \\Z). Post-refactoring, 0 expected. Threshold set at 0 but lte 7 allows for edge cases.
- **Derivation**: 10 uniform entries at lines 315-324 currently exist. Goal is to replace all 10 with a single re.sub() call. Threshold 7 = total structural entries (lines 308-314) that have backslash-letter-like patterns but are heterogeneous.
- **Rationale**: Verifies the core refactoring goal — individual character class entries replaced by generic pattern.

**AC#3: 7 structural/punctuation rules preserved in _UNESCAPE_RULES**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="^        \\(r'")`
- **Expected**: `gte 7` — at least 7 entries must remain in `_UNESCAPE_RULES` (the 7 structural/punctuation rules). Combined with AC#8 (`lte 8`), this constrains total to 7-8 entries, proving structural rules were not removed.
- **Derivation**: 7 heterogeneous entries at lines 308-314: (1) `\"` -> `"`, (2) `\\[` -> `\[`, (3) `\\]` -> `\]`, (4) `\\(` -> `\(`, (5) `\\)` -> `\)`, (6) `\\.` -> `\.`, (7) `\\?` -> `\?`. These cannot be generalized because their from/to mappings are heterogeneous. AC#2 further ensures single-letter entries are gone, so the remaining 7-8 entries must be structural.
- **Rationale**: Ensures refactoring does not remove structural/punctuation rules. Works in concert with AC#2 and AC#8 to triangulate correctness. Constraint C3.

**AC#8: _UNESCAPE_RULES total entry count reduced**
- **Test**: `Grep(src/tools/python/ac-static-verifier.py, pattern="^        \\(r'")`
- **Expected**: `lte 8` — after refactoring, _UNESCAPE_RULES should contain at most 8 entries (7 structural + 1 quote rule = 8 max). Current baseline is 17.
- **Derivation**: Current 17 entries = 7 structural/punctuation + 10 uniform single-letter. After replacing 10 uniform with re.sub(), remaining count should be 7-8. Threshold 8 accommodates the quote rule which may or may not be separate. Constraint C5 requires count < 17.
- **Rationale**: Quantitative proof that rule count was reduced, fulfilling the simplification goal.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Replace 10 uniform single-letter entries with a single re.sub() call | AC#1, AC#2, AC#6 |
| 2 | Using a generic regex pattern | AC#1, AC#5 |
| 3 | Reducing enumeration burden | AC#8 |
| 4 | Enabling automatic handling of future single-letter escape sequences | AC#5 |
| 5 | Preserve the 7 structural/punctuation rules | AC#3 |
| 6 | Preserve _PIPE_RULE separation unchanged | AC#4 |
| 7 | unescape_for_literal_search scope guard (unchanged) | AC#7 |

<!-- fc-phase-4-completed -->

---

## Technical Design

### Approach

Replace the 10 uniform single-letter entries in `_UNESCAPE_RULES` with a single `re.sub()` call using a generic character class pattern. The 7 structural/punctuation rules (lines 308-314: `\"`, `\\[`, `\\]`, `\\(`, `\\)`, `\\.`, `\\?`) remain as `str.replace()` tuples in `_UNESCAPE_RULES` unchanged.

**Implementation pattern**:

```python
# Structural/punctuation rules stay in _UNESCAPE_RULES (7 entries)
_UNESCAPE_RULES = [
    (r'\"', '"'),
    (r'\\[', r'\['),
    (r'\\]', r'\]'),
    (r'\\(', r'\('),
    (r'\\)', r'\)'),
    (r'\\.', r'\.'),
    (r'\\?', r'\?'),
]
_PIPE_RULE = (r'\|', '|')  # markdown pipe escapes — Expected column only

# Generic single-letter unescape: \\X -> \X for any letter
# Applied in unescape() and unescape_for_regex_pattern() after the str.replace() loop.
# Uses raw string r'\\\1' to produce literal backslash + captured letter.
# Safety: r'\1' alone would yield just the letter (e.g., 'b' not '\b').
# r'\\\1' yields backslash + captured group — correct for all single-letter escapes.
import re
_SINGLE_LETTER_UNESCAPE_PATTERN = re.compile(r'\\\\([a-zA-Z])')

@staticmethod
def _apply_single_letter_unescape(s: str) -> str:
    return ACVerifier._SINGLE_LETTER_UNESCAPE_PATTERN.sub(r'\\\1', s)
```

Both `unescape()` and `unescape_for_regex_pattern()` apply the `str.replace()` loop for structural rules first, then call `_apply_single_letter_unescape()`. The `unescape_for_literal_search()` method is not touched.

The `[a-zA-Z]` character class is chosen (not the restricted `[sSdDwWbBAZ]`) to satisfy C4 (extensibility for future single-letter escapes without code changes). Structural escape characters (`[`, `]`, `(`, `)`, `.`, `?`, `"`) are all non-alphabetic, so `[a-zA-Z]` cannot accidentally match them.

**`\b` backspace hazard**: The replacement string `r'\\\1'` is a raw Python string. In `re.sub()`, `\\\1` means: literal backslash (from `\\`) followed by group 1 (from `\1`). This produces the 2-char sequence `\b` when the captured group is `b`. Python never interprets this as `\x08` because the replacement is assembled by `re.sub()` post-match, not parsed as a Python string escape.

**`test_unescape_rules_count` update**: The existing test asserts `len(ACVerifier._UNESCAPE_RULES) >= 17`. After refactoring the list shrinks to 7 entries. This test must be updated to assert `<= 8` (or changed to verify the `re.sub()` pattern exists). See Upstream Issues.

**Novel letter test (AC#5)**: Add `test_unescape_rule_slash_G` to `test_ac_verifier_unescape_charclass.py` verifying that `unescape_for_regex_pattern(r'\\G')` returns `r'\G'`. This letter has no dedicated entry in `_UNESCAPE_RULES` and must pass via the generic `re.sub()` path.

This approach satisfies all 8 ACs:
- AC#1: `re.sub` call present in ac-static-verifier.py
- AC#2: No `(r'\\\\[a-zA-Z]'` pattern tuples in `_UNESCAPE_RULES` (count = 0, which is lte 7)
- AC#3: All 7 structural entries remain as explicit tuples (gte 7)
- AC#4: `_PIPE_RULE` still defined separately
- AC#5: Novel letter test passes (pytest passes)
- AC#6: All existing unescape behavioral tests pass
- AC#7: `def unescape_for_literal_search` unchanged
- AC#8: Total `_UNESCAPE_RULES` tuple count = 7, which is lte 8

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `re.compile(r'\\\\([a-zA-Z])')` and call `.sub()` in `_apply_single_letter_unescape()` helper method; `re.sub` is present in the file |
| 2 | Remove all 10 single-letter tuple entries from `_UNESCAPE_RULES`; count of tuples matching `^        (r'\\\\[a-zA-Z]"` becomes 0 (lte 7) |
| 3 | Keep all 7 structural/punctuation tuples in `_UNESCAPE_RULES` unchanged (lines 308-314 patterns preserved) |
| 4 | `_PIPE_RULE` class variable remains as a separate tuple; not merged into `_UNESCAPE_RULES` or the generic pattern |
| 5 | Add `test_unescape_rule_slash_G` test to `test_ac_verifier_unescape_charclass.py`; passes because `re.sub()` handles `\\G` generically |
| 6 | All existing `test_unescape_rule_slash_*` tests pass because `re.sub()` with `[a-zA-Z]` handles all 10 letters (s,S,d,D,w,W,b,B,A,Z) |
| 7 | `unescape_for_literal_search` method body left untouched; grep finds `def unescape_for_literal_search` unchanged |
| 8 | `_UNESCAPE_RULES` list shrinks from 17 to 7 entries; grep count of `^        (r'` is 7 (lte 8) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Character class scope | A: `[sSdDwWbBAZ]` (explicit current 10), B: `[a-zA-Z]` (all letters), C: `[a-z]` (lowercase only) | B: `[a-zA-Z]` | Satisfies C4 (future letters handled automatically); structural escapes are all non-alphabetic so no collision risk |
| re.sub placement | A: Replace `str.replace()` loop entirely, B: Run re.sub after str.replace() loop, C: Run re.sub before str.replace() loop | B: After loop | Structural rules (brackets, parens, dot, question mark) must run first as str.replace() tuples per constraint C3; ordering is safe because structural chars are non-alphabetic |
| Replacement string | A: `r'\1'` (group only), B: `r'\\\1'` (backslash + group), C: lambda function | B: `r'\\\1'` | Produces `\X` (backslash + letter); `r'\1'` alone would strip the backslash and produce bare letter; lambda adds complexity without benefit |
| Static vs class-level pattern | A: `static readonly` class variable, B: local `re.compile()` inside method, C: module-level | A: class variable `_SINGLE_LETTER_UNESCAPE_PATTERN` | Matches project pattern — `_UNESCAPE_RULES` and `_PIPE_RULE` are class-level; compile once at class definition time |
| Novel letter for AC#5 | A: `\\G` (grep boundary anchor), B: `\\x` (hex escape prefix), C: `\\p` (Unicode property) | A: `\\G` | Simple 1-char letter not in current set; `\\G` is a valid regex anchor in some engines making it a realistic future addition |
| Existing count-test update | A: Update assertion to `<= 8`, B: Change to verify pattern exists, C: Delete test | A: Update assertion | Preserves the intent of structural count verification while reflecting post-refactor state |

### Interfaces / Data Structures

No new interfaces. Changes are internal to `ACVerifier` class.

**Modified class variables** (ac-static-verifier.py):

```python
# BEFORE (17 entries):
_UNESCAPE_RULES = [
    (r'\"', '"'),        # structural
    (r'\\[', r'\['),     # structural
    (r'\\]', r'\]'),     # structural
    (r'\\(', r'\('),     # structural
    (r'\\)', r'\)'),     # structural
    (r'\\.', r'\.'),     # structural
    (r'\\?', r'\?'),     # structural
    (r'\\w', r'\w'),     # uniform — REMOVE
    # ... 9 more uniform entries — REMOVE
]

# AFTER (7 entries — structural only):
_UNESCAPE_RULES = [
    (r'\"', '"'),
    (r'\\[', r'\['),
    (r'\\]', r'\]'),
    (r'\\(', r'\('),
    (r'\\)', r'\)'),
    (r'\\.', r'\.'),
    (r'\\?', r'\?'),
]
_PIPE_RULE = (r'\|', '|')  # unchanged

# NEW class variable:
_SINGLE_LETTER_UNESCAPE_PATTERN = re.compile(r'\\\\([a-zA-Z])')
```

**Modified methods** (ac-static-verifier.py):

```python
@staticmethod
def unescape(s: str) -> str:
    # ... docstring updated to remove individual letter entries, add generic note ...
    for from_str, to_str in ACVerifier._UNESCAPE_RULES:
        s = s.replace(from_str, to_str)
    s = ACVerifier._SINGLE_LETTER_UNESCAPE_PATTERN.sub(r'\\\1', s)  # generic: \\X -> \X
    s = s.replace(*ACVerifier._PIPE_RULE)
    return s

@staticmethod
def unescape_for_regex_pattern(s: str) -> str:
    # ... docstring unchanged ...
    for from_str, to_str in ACVerifier._UNESCAPE_RULES:
        s = s.replace(from_str, to_str)
    s = ACVerifier._SINGLE_LETTER_UNESCAPE_PATTERN.sub(r'\\\1', s)  # generic: \\X -> \X
    return s
```

**New test** (test_ac_verifier_unescape_charclass.py):

```python
def test_unescape_rule_slash_G(self):
    r"""\\G -> \G (novel letter not in original 10 — proves generic extensibility)."""
    result = ACVerifier.unescape_for_regex_pattern(r'\\G')
    assert result == r'\G', f"Expected r'\\G', got: {result!r}"
```

**Updated test** (test_ac_verifier_unescape_charclass.py):

```python
def test_unescape_rules_count(self):
    """_UNESCAPE_RULES must contain at most 8 entries (7 structural + at most 1 quote)."""
    count = len(ACVerifier._UNESCAPE_RULES)
    assert count <= 8, (
        f"Expected at most 8 unescape rules, found {count}. "
        f"Uniform single-letter entries should be handled by _SINGLE_LETTER_UNESCAPE_PATTERN."
    )
```

### Upstream Issues

<!-- Optional: Issues discovered during design that require upstream changes (AC gaps, constraint gaps, interface gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| `test_unescape_rules_count` in `test_ac_verifier_unescape_charclass.py` asserts `count >= 17`; after refactoring `_UNESCAPE_RULES` shrinks to 7. The test will fail. This is an expected implementation-time update, not an AC gap — implementation Task must include updating this assertion. | Technical Constraints (C5) | Implementation task: update `test_unescape_rules_count` assertion to `count <= 8` and update docstring to reflect post-refactor intent. Not an AC table change. |
| AC#5 and AC#6 use `exit_code` Type with Matcher `matches` and Expected `passed`. Per ac-matcher-mapping.md, `exit_code` only supports `equals`, `succeeds`, `fails` — `matches` is not a valid combination. ac-static-verifier.py `verify_ac()` would dispatch `exit_code` to the `else` branch returning `FAIL` with "Unknown AC type: exit_code" (line 1329-1333). The intent is clearly "pytest exits 0 / passes". | AC Definition Table AC#5, AC#6 | Change both AC#5 and AC#6 to: Type=`exit_code`, Method=`pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py`, Matcher=`succeeds`, Expected=`-` |

<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2, 3, 4, 7, 8 | Refactor `_UNESCAPE_RULES` in `ac-static-verifier.py`: remove the 10 uniform single-letter tuples (\\w, \\s, \\S, \\d, \\D, \\W, \\b, \\B, \\A, \\Z), add `_SINGLE_LETTER_UNESCAPE_PATTERN = re.compile(r'\\\\([a-zA-Z])')` as a class variable, add `_apply_single_letter_unescape()` static method using `r'\\\1'` replacement, and call it in both `unescape()` and `unescape_for_regex_pattern()` after the str.replace loop | | [x] |
| 2 | 5 | Update `test_ac_verifier_unescape_charclass.py`: update `test_unescape_rules_count` assertion from `>= 17` to `<= 8` with updated docstring, and add `test_unescape_rule_slash_G` test verifying `unescape_for_regex_pattern(r'\\G')` returns `r'\G'` | | [x] |
| 3 | 5, 6 | Run `pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py` and verify all tests pass (novel \\G test + all existing unescape tests) | | [x] |

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
| 1 | implementer | sonnet | ac-static-verifier.py (lines ~305-335 and `unescape()`, `unescape_for_regex_pattern()` methods) | Refactored `_UNESCAPE_RULES` (7 structural entries only), new `_SINGLE_LETTER_UNESCAPE_PATTERN` class variable, new `_apply_single_letter_unescape()` method, updated `unescape()` and `unescape_for_regex_pattern()` |
| 2 | implementer | sonnet | test_ac_verifier_unescape_charclass.py | Updated `test_unescape_rules_count` assertion (`<= 8`) + new `test_unescape_rule_slash_G` test |
| 3 | tester | sonnet | pytest command | All tests pass (green) |

### Pre-conditions

- F842 must be [DONE] before executing this feature (Predecessor dependency)
- Read `src/tools/python/ac-static-verifier.py` lines ~305-335 and the `unescape()` and `unescape_for_regex_pattern()` methods before making changes

### Execution Steps

**Step 1: Refactor `_UNESCAPE_RULES` in ac-static-verifier.py**

1.1. Locate `_UNESCAPE_RULES` class variable (lines ~307-325 as of F842 completion)

1.2. Remove the 10 uniform single-letter tuples. The list entries to REMOVE are:
- `(r'\\w', r'\w')`
- `(r'\\s', r'\s')`
- `(r'\\S', r'\S')`
- `(r'\\d', r'\d')`
- `(r'\\D', r'\D')`
- `(r'\\W', r'\W')`
- `(r'\\b', r'\b')`
- `(r'\\B', r'\B')`
- `(r'\\A', r'\A')`
- `(r'\\Z', r'\Z')`

1.3. Keep the 7 structural/punctuation tuples UNCHANGED (regardless of whether their ordering or exact spelling differs slightly from the design — preserve whatever is in the file):
- `(r'\"', '"')` (quote)
- `(r'\\[', r'\[')` (open bracket)
- `(r'\\]', r'\]')` (close bracket)
- `(r'\\(', r'\(')` (open paren)
- `(r'\\)', r'\)')` (close paren)
- `(r'\\.', r'\.')` (dot)
- `(r'\\?', r'\?')` (question mark)

1.4. Add `_SINGLE_LETTER_UNESCAPE_PATTERN` class variable immediately after `_UNESCAPE_RULES` (before `_PIPE_RULE`):
```python
_SINGLE_LETTER_UNESCAPE_PATTERN = re.compile(r'\\\\([a-zA-Z])')
```

1.5. Add `_apply_single_letter_unescape()` static method to the `ACVerifier` class:
```python
@staticmethod
def _apply_single_letter_unescape(s: str) -> str:
    return ACVerifier._SINGLE_LETTER_UNESCAPE_PATTERN.sub(r'\\\1', s)
```

1.6. In `unescape()` method: add `s = ACVerifier._apply_single_letter_unescape(s)` AFTER the `str.replace()` loop for `_UNESCAPE_RULES` and BEFORE the `_PIPE_RULE` replacement. The replacement string `r'\\\1'` is a raw Python string — it produces backslash + captured group, ensuring `\b` produces the 2-char sequence `\b` (not backspace `\x08`).

1.7. In `unescape_for_regex_pattern()` method: add `s = ACVerifier._apply_single_letter_unescape(s)` AFTER the `str.replace()` loop for `_UNESCAPE_RULES` (same pattern as `unescape()`).

1.8. Do NOT modify `unescape_for_literal_search()` — this method is explicitly out of scope.

1.9. Verify `_PIPE_RULE` remains as a separate class variable (not merged into `_UNESCAPE_RULES` or the generic pattern).

**Step 2: Update test file**

2.1. In `test_ac_verifier_unescape_charclass.py`, locate `test_unescape_rules_count` method.

2.2. Update the assertion from `count >= 17` (or `len(...) >= 17`) to `count <= 8` (or `len(...) <= 8`). Update the docstring to reflect post-refactor intent: `"_UNESCAPE_RULES must contain at most 8 entries (7 structural + at most 1 quote)"`.

2.3. Update the failure message to reference `_SINGLE_LETTER_UNESCAPE_PATTERN` as the replacement mechanism.

2.4. Add new test method `test_unescape_rule_slash_G` to the same test class:
```python
def test_unescape_rule_slash_G(self):
    r"""\\G -> \G (novel letter not in original 10 — proves generic extensibility)."""
    result = ACVerifier.unescape_for_regex_pattern(r'\\G')
    assert result == r'\G', f"Expected r'\\G', got: {result!r}"
```

**Step 3: Run tests**

3.1. Run: `pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py -v`

3.2. All tests must PASS, including `test_unescape_rule_slash_G` (novel letter) and all pre-existing `test_unescape_rule_slash_*` tests.

3.3. If any test fails: STOP → report failure details to user before proceeding.

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix

### Success Criteria

- `_UNESCAPE_RULES` contains exactly 7 entries (structural/punctuation only)
- `_SINGLE_LETTER_UNESCAPE_PATTERN` class variable present with `re.compile(r'\\\\([a-zA-Z])')`
- `unescape()` and `unescape_for_regex_pattern()` call `_apply_single_letter_unescape()` after the str.replace loop
- `unescape_for_literal_search()` is unchanged
- `_PIPE_RULE` remains separate
- All pytest tests pass (including new `test_unescape_rule_slash_G`)

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

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
| 2026-03-06 | PHASE_START | orchestrator | Phase 1 Initialize | F844 [REVIEWED]->[WIP] |
<!-- run-phase-1-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 2 Investigation | Explorer: all targets located |
<!-- run-phase-2-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 4 Implementation | 3 Tasks done, 13/13 tests PASSED |
<!-- run-phase-4-completed -->
| 2026-03-06 | DEVIATION | ac-static-verifier | AC#1 code verification | exit 1: AC#1 Expected `re\.sub` doesn't match `re.compile().sub()` pattern per Technical Design |
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 7 Verification | 8/8 ACs PASS, 1 DEVIATION (AC#1 Expected fixed) |
<!-- run-phase-7-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 8 Post-Review | READY (5/5), 8.2 skip (no extensibility), 8.3 N/A |
<!-- run-phase-8-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 9 Report & Approval | User approved |
<!-- run-phase-9-completed -->
| 2026-03-06 | CodeRabbit | 3 Minor (修正不要) | Doc wording: success criteria count, risk mitigation string, F842 status in Related Features |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [info] Phase1-DriftChecked: F842 (Predecessor)
- [fix] Phase2-Review iter1: Goal Coverage Verification, row 5 | AC#7 incorrectly listed as covering structural rule preservation (AC#7 is unescape_for_literal_search scope guard)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 844 (2026-03-06)
- [applied] ac-designer step 11.4.0: Goal Coverage AC#説明文クロスチェック指示追加 → `.claude/agents/ac-designer.md`
- [applied] imp-analyzer共有セッション検知: `shared_features`フィールド + `_detect_shared_features()` + `[shared]`マーカー表示 → `src/tools/python/imp-analyzer.py`
- [revised] post-code-write.ps1 dotnet呼び出しWSLラップ (revised: 直接dotnet→WSL経由に変更、--blame-hang-timeout追加) → `.claude/hooks/post-code-write.ps1`
- [rejected] quality-fixer C38 (Goal Coverage AC#クロスチェック) — ac-designerで対処済み（Proposal D）、quality-fixerの「100% deterministic」制約に違反

### /imp 844 (2026-03-06) — run 2
- [applied] P1: predecessor_context実体ファイル化 — Write()ステップ追加+下流Read()参照 → `fl-workflow/PHASE-2.md`, `fl-workflow/PHASE-3.md`, `fl-workflow/PHASE-7.md`, `run-workflow/PHASE-1.md`, `run-workflow/PHASE-8.md`
- [rejected] P2: RUN Phase 7 AC Expected vs Technical Design pre-check — 単発のFC品質問題、ac-designer step 11.4.0で上流対策済み
- [rejected] P3: Hook error regex fallback統一 — 各hookは意図的にpayload特性に合わせたパース戦略を使い分け、実質的gapなし

---

<!-- fc-phase-6-completed -->
## Links

[Predecessor: F842](feature-842.md) - Parent -- added the 9 character-class rules that motivate this refactoring
[Related: F834](feature-834.md) - Established `_UNESCAPE_RULES` as shared SSOT list
[Related: F817](feature-817.md) - Added metachar rules (\\(, \\), \\., \\?)
[Related: F804](feature-804.md) - Origin of `\\w` rule
[Related: F792](feature-792.md) - Original pipe/quote unescape work
[Related: F841](feature-841.md) - Grandparent feature
[Related: F845](feature-845.md) - Sibling deviation -- full AC pattern catalog scan
