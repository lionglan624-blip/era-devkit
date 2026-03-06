# Feature 842: ac-static-verifier Pattern Parsing Enhancements

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T09:48:28Z -->

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
| Parent Feature | F841 |
| Discovery Phase | Phase 7 (Verification) |
| Timestamp | 2026-03-06 |

### Observable Symptom
ac-static-verifier fails (exit 1) on 4 valid code-type ACs due to pattern parsing limitations: multiline=true parameter ignored, escaped quotes mangled, pipe alternation not supported with gte matcher.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python src/tools/python/ac-static-verifier.py --feature 841 --ac-type code` |
| Exit Code | 1 |
| Error Output | `11/15 passed, 0 manual` |
| Expected | 15/15 passed (all patterns match) |
| Actual | AC#1,5,14,16 FAIL due to pattern parsing |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Verifier code — pattern extraction and matching logic |
| pm/features/feature-841.md | AC definitions that triggered the failures |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| Manual Grep verification | PASS | Confirms implementation correct; verifier limitation |

### Parent Session Observations
Three distinct pattern parsing gaps identified: (1) `multiline=true` parameter in Grep() Method column is ignored — verifier uses single-line matching only, causing `[\s\S]*` patterns to fail. (2) Escaped quotes in pattern strings (e.g., `pattern="\"cd \" in build_command"`) are mangled during AC table parsing, extracting just `\` as the pattern. (3) `\|` pipe escaping used as regex alternation in patterns with gte matcher is treated as literal text, finding 0 matches instead of alternating.

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
ac-static-verifier is the SSOT for automated AC verification across all feature types. Its pattern parsing pipeline must handle the full range of regex syntax that AC authors use in Method column definitions, including cross-line patterns, escaped delimiters, and alternation operators. Incremental parser extensions must be validated against the union of all existing AC patterns to prevent regression gaps.

### Problem (Current Issue)
Three independent gaps in the ac-static-verifier pattern parsing pipeline cause 4 AC failures (AC#1,5,14,16 of F841) because the parser was built incrementally across F818, F832, F834, and F838, with each feature addressing a specific use case. Specifically: (a) the quoted-value parser in `_parse_complex_method` (ac-static-verifier.py:475) terminates on the first `"` character without checking for preceding backslash escapes, causing patterns like `"\"cd \" in build_command"` to extract only `\` as the pattern; (b) `_UNESCAPE_RULES` (ac-static-verifier.py:285-294) lacks entries for `\\s`->`\s`, `\\S`->`\S`, `\\d`->`\d`, `\\D`->`\D`, so `[\\s\\S]*` patterns remain double-escaped and fail to match; (c) `_extract_grep_params` (ac-static-verifier.py:541-593) parses the `multiline=true` parameter but discards it, and `_verify_content` (ac-static-verifier.py:674) uses only `re.MULTILINE` (which affects `^`/`$` anchors) without `re.DOTALL` (which makes `.` match newlines), so cross-line `[\s\S]*` patterns cannot span lines even after unescape is fixed; (d) `unescape_for_regex_pattern` (ac-static-verifier.py:337) preserves `\|` as regex literal pipe, but when the same pattern flows through count/gte matchers, `\|` should represent alternation.

### Goal (What to Achieve)
Fix the three pattern parsing gaps in ac-static-verifier so that all 15/15 F841 code-type ACs pass: add escape-aware quote parsing in `_parse_complex_method`, add missing regex character class unescape rules (`\\s`, `\\S`, `\\d`, `\\D`, `\\W`, `\\b`, `\\B`, `\\A`, `\\Z`) to `_UNESCAPE_RULES`, propagate `multiline=true` parameter through `_extract_grep_params` (via `GrepParams` NamedTuple) to `_verify_content` (via `re_flags` parameter) for `re.DOTALL` support, and resolve the `\|` dual-semantics issue for count/gte matchers with diagnostic warning.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do AC#1,5,14,16 of F841 fail? | Patterns extracted from Method column do not match expected file content | `python ac-static-verifier.py --feature 841 --ac-type code` exits 1, 11/15 passed |
| 2 | Why don't the patterns match? | Three distinct extraction/matching bugs corrupt or limit patterns | ac-static-verifier.py:475, :285-294, :541-593, :674 |
| 3 | Why do these bugs exist? | Each parser component was designed for a narrow use case: simple quoted values, limited unescape rules, single-line matching | F818 (complex method), F804 (`\\w` only), F834 (`\|` preservation) |
| 4 | Why weren't these gaps caught earlier? | No prior AC definitions exercised escaped quotes in patterns, `[\s\S]` cross-line matching, or `\|` alternation with count matchers | F841 is the first feature combining all three patterns |
| 5 | Why (Root)? | The parser was built incrementally across 4 features (F818, F832, F834, F838) without cross-feature pattern regression testing, so each new capability was only validated against its own use cases | No integration test suite covering the union of all supported AC pattern syntaxes |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 4 AC verification failures (11/15 passed) | Three independent parser gaps: missing unescape rules, no escape-aware quote parsing, multiline parameter ignored |
| Where | ac-static-verifier exit code 1 on F841 ACs | `_parse_complex_method`:475, `_UNESCAPE_RULES`:285-294, `_extract_grep_params`:541-593 |
| Fix | Manually verify ACs with grep (workaround) | Fix all three parser gaps with regression tests |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F841 | [DONE] | Parent feature whose ACs triggered the 4 failures |
| F834 | [DONE] | DRY consolidation of unescape rules, `\|` preservation design |
| F832 | [DONE] | Positional arg parsing for Grep methods |
| F818 | [DONE] | Cross-repo and WSL support; introduced complex method parsing |
| F804 | [DONE] | Added `\\w` unescape rule to `_UNESCAPE_RULES` |
| F838 | [DONE] | Cross-repo verifier path resolution |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Escaped quote handling | FEASIBLE | ~5 lines change at `_parse_complex_method`:475; standard escape-aware parsing |
| Missing unescape rules | FEASIBLE | Add 4 entries to `_UNESCAPE_RULES`:285-294; follows F804 pattern |
| `multiline=true` propagation | FEASIBLE | ~15 lines through `_extract_grep_params` to `_verify_content`; `re.DOTALL` opt-in |
| Pipe alternation in count/gte | FEASIBLE | Convention: bare `|` inside quoted patterns (table parser already protects via `in_quotes`); document rather than code-change |
| Backward compatibility | FEASIBLE | All changes are additive; existing `\|` preservation test must pass |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| AC verification accuracy | HIGH | Fixes 4 false-negative failures in F841; enables correct verification of cross-line and escaped-quote patterns |
| Future AC authoring | MEDIUM | Expands supported AC syntax, reducing need for manual verification workarounds |
| Existing AC definitions | LOW | All changes are additive; no existing AC patterns affected |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `\|` preservation for regex literal pipe | F834 + test_ac_verifier_pipe_regex.py | Cannot naively strip `\|` to `|` in all contexts |
| `_UNESCAPE_RULES` is SSOT for unescape | ac-static-verifier.py:283-284 | New rules MUST be added there, not ad-hoc |
| `_extract_grep_params` return signature | Multiple callers depend on current return format | Adding `multiline` requires extending return without breaking callers |
| `re.MULTILINE` already used everywhere | ac-static-verifier.py:674,704,774 | `multiline=true` parameter maps to `re.DOTALL`, naming may confuse; parameter name is intuitive for AC authors |
| Table parser vs method parser separation | ac-static-verifier.py:886-896 vs 470-479 | Table parser handles `\"` correctly but method parser does not; fixes are independent |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Breaking `\|` regex literal pipe tests | MEDIUM | HIGH | Context-aware handling; existing test_ac_verifier_pipe_regex.py as regression gate |
| `re.DOTALL` changing `.` match behavior | LOW | MEDIUM | Opt-in only when `multiline=true` explicitly specified in AC |
| Unescape rule interactions | LOW | LOW | Add comprehensive set (`\\s`, `\\S`, `\\d`, `\\D`) now to prevent incremental additions |
| `_extract_grep_params` API change breaking callers | LOW | MEDIUM | Use optional parameter or dict return to maintain backward compatibility |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| F841 AC pass rate | `python src/tools/python/ac-static-verifier.py --feature 841 --ac-type code` | 11/15 passed | AC#1,5,14,16 FAIL |

**Baseline File**: `_out/tmp/baseline-842.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Cannot use verifier to verify its own fixes | Self-referential | Unit tests as primary verification method |
| C2 | `\|` preservation must be maintained for regex `matches` patterns | test_ac_verifier_pipe_regex.py | Regression test required; separate behavior for matches vs count/gte |
| C3 | `multiline` parameter name maps to `re.DOTALL` semantics | ac-static-verifier.py:674 | AC should clarify DOTALL behavior, not `re.MULTILINE` (already default) |
| C4 | Quoted-value fix must handle both escaped and non-escaped quotes | General robustness | Test both `"simple"` and `"with \"escaped\" quotes"` |
| C5 | `_UNESCAPE_RULES` additions must not break existing rules | SSOT constraint at ac-static-verifier.py:283-284 | Regression ACs needed for existing rules |

### Constraint Details

**C1: Self-Referential Testing**
- **Source**: ac-static-verifier verifies AC definitions; fixing its own bugs cannot be verified by itself
- **Verification**: Run unit tests directly via pytest
- **AC Impact**: All ACs should use `dotnet test` equivalent (pytest) or unit-test patterns, not the verifier itself

**C2: Pipe Dual Semantics**
- **Source**: F834 design decision preserved `\|` as regex literal pipe; test_ac_verifier_pipe_regex.py:20-23 validates this
- **Verification**: Run existing pipe regex test suite after changes
- **AC Impact**: Must test both `matches` matcher (pipe = literal) and `count`/`gte` matchers (pipe = alternation) separately

**C3: Multiline Parameter Semantics**
- **Source**: `re.MULTILINE` (already used at line 674) only affects `^`/`$`; `re.DOTALL` makes `.` match newlines; `multiline=true` in AC should enable `re.DOTALL`
- **Verification**: Test that `.` matches `\n` only when `multiline=true` is specified
- **AC Impact**: Test default behavior unchanged (no DOTALL) AND opt-in behavior works

**C4: Escaped Quote Parsing**
- **Source**: `_parse_complex_method`:475 terminates on any `"` without backslash check
- **Verification**: Parse `pattern="\"cd \" in cmd"` and verify full pattern extracted
- **AC Impact**: Test extraction of patterns with embedded escaped quotes

**C5: Unescape Rule Completeness**
- **Source**: `_UNESCAPE_RULES`:285-294 has `\\w`->`\w` but lacks `\\s`, `\\S`, `\\d`, `\\D`
- **Verification**: Confirm `[\\s\\S]` unescapes to `[\s\S]` after rule addition
- **AC Impact**: Test each new rule independently and in combination with existing rules

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F841 | [DONE] | Parent feature whose AC definitions triggered the 4 failures |
| Related | F834 | [DONE] | Unescape rule consolidation and `\|` preservation architecture |
| Related | F832 | [DONE] | Positional arg parsing for Grep methods |
| Related | F804 | [DONE] | Added `\\w` unescape rule (pattern for F842 additions) |
| Related | F818 | [DONE] | Cross-repo and WSL support; introduced complex method parsing |
| Related | F838 | [DONE] | Cross-repo verifier path resolution |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | INFORMATIONAL | F{ID} depends on this feature. Update when this completes. |
| Related | Bidirectional | INFORMATIONAL | Features share context but no blocking dependency. |
-->

---

<!-- fc-phase-2-completed -->
<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "ac-static-verifier is the SSOT for automated AC verification" | Verifier must pass all F841 code-type ACs after fixes | AC#1 |
| "must handle the full range of regex syntax" | Escaped quotes, character classes, multiline, alternation all supported | AC#2, AC#3, AC#5, AC#7 |
| "must handle the full range of regex syntax" | Unescape rules cover all common regex character classes | AC#4 |
| "Incremental parser extensions must be validated against the union of all existing AC patterns" | Existing unescape rules and pipe regex behavior must not regress; triggering feature (F841) fully verified; full pytest suite as union regression gate | AC#1, AC#8, AC#9, AC#10, AC#13 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | F841 code-type ACs all pass (15/15) | exit_code | pytest src/tools/python/tests/test_f841_verification.py | equals | 0 | [x] |
| 2 | Escaped quote parsing extracts full pattern | exit_code | pytest src/tools/python/tests/test_ac_verifier_escaped_quote.py | equals | 0 | [x] |
| 3 | All 9 new unescape rules present (`\\s`, `\\S`, `\\d`, `\\D`, `\\W`, `\\b`, `\\B`, `\\A`, `\\Z`) | exit_code | pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py | equals | 0 | [x] |
| 4 | `_UNESCAPE_RULES` contains at least 17 rules | exit_code | pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py::test_unescape_rules_count | equals | 0 | [x] |
| 5 | `multiline=true` parameter enables `re.DOTALL` (`.+` matches across newlines) | exit_code | pytest src/tools/python/tests/test_ac_verifier_dotall.py::test_dotall_with_multiline_param | equals | 0 | [x] |
| 6 | DOTALL only applied conditionally (`.+` does NOT match newlines without `multiline=true`) | exit_code | pytest src/tools/python/tests/test_ac_verifier_dotall.py::test_no_dotall_without_multiline_param | equals | 0 | [x] |
| 7 | Pipe alternation works with count/gte matchers | exit_code | pytest src/tools/python/tests/test_ac_verifier_pipe_count.py | equals | 0 | [x] |
| 8 | Existing pipe regex preservation test passes | exit_code | pytest src/tools/python/tests/test_ac_verifier_pipe_regex.py | equals | 0 | [x] |
| 9 | Existing unescape metachar test passes | exit_code | pytest src/tools/python/tests/test_ac_verifier_unescape_metachar.py | equals | 0 | [x] |
| 10 | Existing escape test suite passes | exit_code | pytest src/tools/python/tests/test_ac_verifier_escape.py | equals | 0 | [x] |
| 11 | No backslash-escape-unaware quote termination in `_parse_complex_method` | code | Grep(src/tools/python/ac-static-verifier.py, pattern="params_str\\[i\\] != quote_char") | not_matches | naive quote check | [x] |
| 12 | Diagnostic warning emitted when `\|` used with count/gte matcher | exit_code | pytest src/tools/python/tests/test_ac_verifier_pipe_count.py::test_pipe_escape_warning_with_gte | equals | 0 | [x] |
| 13 | Full pytest suite passes (union regression gate) | exit_code | pytest src/tools/python/tests/ -v | equals | 0 | [x] |
| 14 | Pipe convention documented in ac-matcher-mapping | code | Grep(pm/reference/ac-matcher-mapping.md, pattern="bare.*\\|.*alternation") | matches | pipe convention | [x] |

### AC Details

**AC#4: `_UNESCAPE_RULES` contains at least 17 rules**
- **Test**: `pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py::test_unescape_rules_count` imports `ACVerifier._UNESCAPE_RULES` and asserts `len() >= 17`
- **Expected**: exit code 0
- **Rationale**: Current 8 rules + 9 new rules (`\\s`->`\s`, `\\S`->`\S`, `\\d`->`\d`, `\\D`->`\D`, `\\W`->`\W`, `\\b`->`\b`, `\\B`->`\B`, `\\A`->`\A`, `\\Z`->`\Z`) = 17 minimum. Derivation: existing 8 (line 286-293: `\"`, `\\[`, `\\]`, `\\(`, `\\)`, `\\.`, `\\?`, `\\w`) + 9 new character class/anchor rules. References constraint C5. Consistent with AC Design Constraint C1 (unit tests as primary verification).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Escape-aware quote parsing in `_parse_complex_method` | AC#1, AC#2, AC#10, AC#11 |
| 2 | Missing regex character class unescape rules (`\\s`, `\\S`, `\\d`, `\\D`) | AC#1, AC#3, AC#4, AC#9 |
| 3 | Propagate `multiline=true` for `re.DOTALL` support | AC#1, AC#5, AC#6 |
| 4 | Resolve `\|` dual-semantics for count/gte matchers | AC#1, AC#7, AC#8, AC#12, AC#14 |
| 5 | Union regression validation (cross-feature pattern safety) | AC#13 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Four targeted fixes to `ac-static-verifier.py` address the three independent pattern parsing gaps identified in the Problem statement. All changes are additive and opt-in; no existing behavior is removed.

**Fix 1 — Escape-aware quote parsing in `_parse_complex_method`** (AC#2, AC#11)
The inner `while` loop at line 475 terminates on any `quote_char` character without checking for a preceding backslash. Replace the naive loop with an escape-aware walk: advance past `\X` pairs as a unit, only terminate on an unescaped `quote_char`. After extraction, un-double the escape sequences (e.g., `\"` → `"`).

**Fix 2 — Add nine missing unescape rules to `_UNESCAPE_RULES`** (AC#3, AC#4)
Following the F804 pattern (which added `\\w` → `\w`), append nine new entries to `_UNESCAPE_RULES` at line 285-294:
- `(r'\\s', r'\s')`, `(r'\\S', r'\S')`, `(r'\\d', r'\d')`, `(r'\\D', r'\D')`
- `(r'\\W', r'\W')` (counterpart of existing `\\w`), `(r'\\b', r'\b')`, `(r'\\B', r'\B')` (word-boundary assertions)
- `(r'\\A', r'\A')`, `(r'\\Z', r'\Z')` (string anchors — Zero Debt Upfront)
Both `unescape()` and `unescape_for_regex_pattern()` draw from `_UNESCAPE_RULES` automatically (SSOT at line 283-284). This brings the total to 17 rules, satisfying AC#4 `gte 17`.

**Fix 3 — Propagate `multiline=true` parameter to enable `re.DOTALL`** (AC#5, AC#6)
`_extract_grep_params` already parses all named parameters via `_parse_complex_method` but discards `multiline`. Replace the 3-tuple return with a `GrepParams` NamedTuple: `class GrepParams(NamedTuple): file_path: Optional[str]; pattern: Optional[str]; error_result: Optional[Dict]; use_dotall: bool = False`. When `parsed.get('multiline') == 'true'`, set `use_dotall=True`. Update `_verify_content` to accept `re_flags: int = re.MULTILINE` instead of a boolean — the caller computes the full flag set: `flags = re.MULTILINE | (re.DOTALL if use_dotall else 0)` and passes it. The `_verify_content` method uses `re_flags` directly at all three match sites (lines ~674, ~704, ~774). This eliminates positional coupling in the return type and makes future flag/parameter additions non-breaking. Update both call sites (`verify_code_ac` at line 1004 and the second call at line 1154) to destructure `GrepParams` and pass `re_flags`. Default is `re.MULTILINE` only, so all existing behavior is unchanged.

**Fix 4 — Pipe alternation for count/gte matchers** (AC#7, AC#8)
The `\|` dual-semantics issue is resolved by convention with diagnostic enforcement: bare `|` inside quoted patterns in the AC table (e.g., `pattern="foo|bar"`) already passes through `_parse_complex_method` unescaped and works correctly as regex alternation. The `\|` preservation in `unescape_for_regex_pattern` is correct for `matches` matchers (literal pipe). For count/gte matchers, AC authors must use bare `|` inside quoted patterns, not `\|`. A diagnostic warning is added in `_verify_content`: when matcher is count/gte and the pattern contains literal `\|`, emit a stderr warning like `WARNING: AC#N pattern contains \| with {matcher} matcher; use bare | for alternation`. This makes the convention discoverable without breaking existing behavior. The new test file `test_ac_verifier_pipe_count.py` verifies that `pattern="foo|bar"` matches either `foo` or `bar` content with a gte matcher.

**New test files** (one per AC pair, following existing naming pattern):
- `test_f841_verification.py` — runs the full F841 code-type AC suite via subprocess (AC#1)
- `test_ac_verifier_escaped_quote.py` — escape-aware quote parsing in complex method patterns (AC#2)
- `test_ac_verifier_unescape_charclass.py` — new `\\s`, `\\S`, `\\d`, `\\D` rules (AC#3, AC#4)
- `test_ac_verifier_dotall.py` — `multiline=true` enables DOTALL; default unchanged (AC#5, AC#6)
- `test_ac_verifier_pipe_count.py` — bare pipe alternation with count/gte matchers (AC#7)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | `test_f841_verification.py` runs `ac-static-verifier.py --feature 841 --ac-type code` via subprocess and asserts exit code 0; all four fixes together make F841 15/15 pass |
| 2 | `test_ac_verifier_escaped_quote.py` constructs an AC with `pattern="\"cd \" in build_command"` via `_parse_complex_method` and asserts full pattern extracted (not just `\`) |
| 3 | `test_ac_verifier_unescape_charclass.py` calls `ACVerifier.unescape_for_regex_pattern(r'[\\s\\S]*')` and asserts result equals `r'[\s\S]*'`; also tests each of the 9 new rules independently |
| 4 | `test_ac_verifier_unescape_charclass.py::test_unescape_rules_count` imports `ACVerifier._UNESCAPE_RULES` and asserts `len() >= 17` (8 existing + 9 new) |
| 5 | `test_ac_verifier_dotall.py` creates a temp file with a newline in content, uses `Grep(..., multiline=true)` with `matches` matcher and a `.+` pattern, asserts PASS |
| 6 | Same test file: `test_no_dotall_without_multiline_param` omits `multiline=true`, uses `.+` pattern (`.` does NOT match `\n` without DOTALL), asserts FAIL |
| 7 | `test_ac_verifier_pipe_count.py` creates a temp file with `foo` and `bar` content, uses `Grep(..., pattern="foo|bar")` with `gte` matcher, asserts count matches |
| 8 | Existing `test_ac_verifier_pipe_regex.py` must continue to pass after all changes (regression gate for `\|` preservation in `matches` matcher) |
| 9 | Existing `test_ac_verifier_unescape_metachar.py` must continue to pass after adding 4 new rules to `_UNESCAPE_RULES` |
| 10 | Existing `test_ac_verifier_escape.py` must continue to pass (covers escaped-quote parsing in Expected column, not Method column) |
| 11 | `test_ac_verifier_escaped_quote.py` also verifies that escape-aware parsing does NOT leave a backslash-terminated loop; the `not_matches` AC#11 pattern `params_str\[i\] != quote_char` checks that the naive loop condition no longer exists in the source |
| 12 | `test_ac_verifier_pipe_count.py::test_pipe_escape_warning_with_gte` creates a temp file, uses `Grep(..., pattern="foo\|bar")` with `gte` matcher, captures stderr and asserts WARNING message is emitted about `\|` with count/gte matcher |
| 13 | Full `pytest src/tools/python/tests/ -v` run serves as union regression gate — confirms no existing test regresses from the 4 parser fixes |
| 14 | `Grep(pm/reference/ac-matcher-mapping.md, pattern="bare.*\\|.*alternation")` confirms the pipe convention is documented for AC authors |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Return signature for `_extract_grep_params` multiline propagation | A: 4-tuple `(path, pattern, error, use_dotall)`, B: return dict, C: attach to ACDefinition, D: GrepParams NamedTuple | D: GrepParams NamedTuple | Eliminates positional coupling; future parameter additions (e.g., `case_sensitive`) are non-breaking; NamedTuple supports both attribute access and destructuring |
| `_verify_content` flag threading | A: new `use_dotall: bool` parameter, B: `re_flags: int = re.MULTILINE`, C: global flag | B: `re_flags: int = re.MULTILINE` | Future flag additions (e.g., `re.IGNORECASE`) only change the caller, not `_verify_content`'s signature; keeps method pure |
| Pipe alternation in count/gte matchers | A: strip `\|` to `|` for count/gte, B: convention (bare `|` in quoted patterns), C: context-aware dual mode | B: convention | Avoids breaking the `\|` preservation invariant (C2 constraint); bare `|` already passes through correctly; no code change needed |
| Escape-aware quote loop implementation | A: character-by-character with backslash check, B: regex-based extraction, C: Python `shlex` | A: character walk | Consistent with existing manual parsing style at lines 428-520; regex is fragile for nested structures; `shlex` handles shell quoting differently |
| New unescape rules scope | A: `\\s`, `\\S`, `\\d`, `\\D` only, B: also add `\\W`, `\\b`, `\\B`, `\\A`, `\\Z` | B (full): 9 rules (`\\s`, `\\S`, `\\d`, `\\D`, `\\W`, `\\b`, `\\B`, `\\A`, `\\Z`) | Zero Debt Upfront: `\\W` is counterpart of existing `\\w`, `\\b`/`\\B` are common word-boundary patterns, `\\A`/`\\Z` are string anchors. Including all eliminates incremental additions. Catch-all regex refactoring tracked as Mandatory Handoff. |

### Interfaces / Data Structures

No new interfaces. The changes affect three private methods in `ACVerifier`:

**`_parse_complex_method` (line 475 — escape-aware quote loop)**
```python
# Before (naive):
while i < len(params_str) and params_str[i] != quote_char:
    i += 1

# After (escape-aware):
while i < len(params_str):
    if params_str[i] == '\\' and i + 1 < len(params_str):
        i += 2  # skip escaped character pair
        continue
    if params_str[i] == quote_char:
        break
    i += 1
```

**`_UNESCAPE_RULES` (line 285-294 — 4 new entries)**
```python
_UNESCAPE_RULES = [
    (r'\"', '"'),
    (r'\\[', r'\['),
    (r'\\]', r'\]'),
    (r'\\(', r'\('),
    (r'\\)', r'\)'),
    (r'\\.', r'\.'),
    (r'\\?', r'\?'),
    (r'\\w', r'\w'),
    (r'\\s', r'\s'),   # NEW
    (r'\\S', r'\S'),   # NEW
    (r'\\d', r'\d'),   # NEW
    (r'\\D', r'\D'),   # NEW
    (r'\\W', r'\W'),   # NEW
    (r'\\b', r'\b'),   # NEW
    (r'\\B', r'\B'),   # NEW
    (r'\\A', r'\A'),   # NEW
    (r'\\Z', r'\Z'),   # NEW
]
```

**`_extract_grep_params` return type extension**
```python
from typing import NamedTuple

class GrepParams(NamedTuple):
    file_path: Optional[str]
    pattern: Optional[str]
    error_result: Optional[Dict[str, Any]]
    use_dotall: bool = False

# Before:
def _extract_grep_params(self, ac: ACDefinition) -> tuple[Optional[str], Optional[str], Optional[Dict[str, Any]]]:
    ...
    return file_path, pattern, None

# After:
def _extract_grep_params(self, ac: ACDefinition) -> GrepParams:
    ...
    use_dotall = (parsed.get('multiline', '').lower() == 'true') if parsed else False
    return GrepParams(file_path, pattern, None, use_dotall)
```

**`_verify_content` re_flags parameter**
```python
# Before:
def _verify_content(self, file_path, pattern, matcher, pattern_type, ac_number, expected_count=None):
    ...
    if re.search(pattern, content, re.MULTILINE) is not None:

# After:
def _verify_content(self, file_path, pattern, matcher, pattern_type, ac_number, expected_count=None, re_flags=re.MULTILINE):
    ...
    if re.search(pattern, content, re_flags) is not None:
```

The `re_flags` parameter replaces the inline `re.MULTILINE` at lines 674, 704, and 774. Callers compute flags: `re_flags = re.MULTILINE | (re.DOTALL if params.use_dotall else 0)`. Future flag additions (e.g., `re.IGNORECASE`) only change the caller, not `_verify_content`'s signature.

### Upstream Issues

<!-- Optional: Issues discovered during design that require upstream changes (AC gaps, constraint gaps, interface gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#6 tests `test_no_dotall_without_multiline_param` but `[\s\S]` in Python regex matches newlines even without `re.DOTALL` (DOTALL only affects `.`, not `\s\S`). The AC's actual purpose is to confirm DOTALL is NOT hardcoded globally — a pattern that requires DOTALL (like `.` matching `\n`) would better demonstrate this. However, the AC title and test file name are correct; the test should use `.+` as the cross-line pattern (requires DOTALL) rather than `[\s\S]+` (doesn't need it). | AC#6 `test_ac_verifier_dotall.py::test_no_dotall_without_multiline_param` | Update the DOTALL tests to use `r'.+'` as the cross-line pattern (without `re.DOTALL`, `.` does NOT match `\n`); this properly validates both the opt-in (AC#5) and the non-default (AC#6) behavior. Note: `[\s\S]*` patterns in F841 work because Python's `re` evaluates `\s` and `\S` at character level — `re.DOTALL` is irrelevant for them. The F841 AC#1,5,14,16 failures were caused by the unescape and quote-parsing bugs (Fix 1 and Fix 2), not by missing DOTALL. Fix 3 (DOTALL propagation) is still a valid enhancement for future AC patterns that rely on `.` matching `\n`. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 2, 11 | Fix escape-aware quote parsing in `_parse_complex_method`: write `test_ac_verifier_escaped_quote.py` (RED), then replace naive loop with backslash-skip walk (GREEN) | | [x] |
| 2 | 3, 4 | Add 9 unescape rules (`\\s`, `\\S`, `\\d`, `\\D`, `\\W`, `\\b`, `\\B`, `\\A`, `\\Z`) to `_UNESCAPE_RULES`: write `test_ac_verifier_unescape_charclass.py` (RED), then append rules to `_UNESCAPE_RULES` (GREEN) | | [x] |
| 3 | 5, 6 | Propagate `multiline=true` to `re.DOTALL` in `_extract_grep_params` and `_verify_content`: write `test_ac_verifier_dotall.py` (RED), then introduce `GrepParams` NamedTuple return and add `re_flags` parameter to `_verify_content` (GREEN) | | [x] |
| 4 | 7, 8, 12 | Write `test_ac_verifier_pipe_count.py` verifying bare `|` alternation with count/gte matchers and `\|` diagnostic warning emission; run existing `test_ac_verifier_pipe_regex.py` to confirm `\|` preservation regression passes | | [x] |
| 5 | 1 | Write `test_f841_verification.py` (subprocess call to `ac-static-verifier.py --feature 841 --ac-type code`) and run to confirm 15/15 pass | | [x] |
| 6 | 9, 10, 13 | Run full pytest suite (`pytest src/tools/python/tests/ -v`) as union regression gate — covers existing regression tests (AC#9, AC#10) and all other existing tests (AC#13) | | [x] |
| 7 | 14 | Document pipe convention and multiline parameter semantics in `pm/reference/ac-matcher-mapping.md`: (a) for count/gte matchers, use bare `|` for alternation; `\|` is literal pipe only for matches/not_matches; (b) `multiline=true` in Grep Method column enables `re.DOTALL` (dot matches newlines), not `re.MULTILINE` (which is always on) | | [x] |

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

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-842.md Tasks 1-3, ac-static-verifier.py | Fixed parser: escape-aware quote loop, 9 new unescape rules, GrepParams NamedTuple + re_flags parameter for multiline→DOTALL propagation; 3 new test files |
| 2 | implementer | sonnet | feature-842.md Tasks 4 and 7, ac-static-verifier.py test suite, ac-matcher-mapping.md | Pipe diagnostic warning in _verify_content; test_ac_verifier_pipe_count.py (alternation + warning tests); pipe_regex regression confirmed; pipe/multiline convention documentation |
| 3 | tester | sonnet | All new + existing test files | Full pytest suite confirming AC#1-14 all pass |

### Pre-conditions

- `src/tools/python/ac-static-verifier.py` readable
- Existing test suite at `src/tools/python/tests/` accessible
- `pm/features/feature-841.md` accessible for subprocess verification in Task 6

### Execution Order

**Task 1 — Escape-aware quote parsing (AC#2, AC#11)**

1. Read `_parse_complex_method` inner quote loop at ac-static-verifier.py line ~475
2. Write `src/tools/python/tests/test_ac_verifier_escaped_quote.py`:
   - Test: parse `Grep(file.py, pattern="\"cd \" in build_command")` extracts pattern `"cd " in build_command` (not `\`)
   - Test: parse simple `pattern="foo"` still works (regression)
3. Run test — confirm FAIL (RED)
4. Replace naive `while params_str[i] != quote_char` loop with escape-aware walk:
   ```python
   while i < len(params_str):
       if params_str[i] == '\\' and i + 1 < len(params_str):
           i += 2
           continue
       if params_str[i] == quote_char:
           break
       i += 1
   ```
5. Run test — confirm PASS (GREEN)
6. Verify AC#11: `Grep(src/tools/python/ac-static-verifier.py, pattern="params_str\\[i\\] != quote_char")` returns `not_matches` (naive check removed)

**Task 2 — Add unescape rules (AC#3, AC#4)**

1. Read `_UNESCAPE_RULES` at ac-static-verifier.py lines ~285-294
2. Write `src/tools/python/tests/test_ac_verifier_unescape_charclass.py`:
   - Test: `unescape_for_regex_pattern(r'[\\s\\S]*')` returns `r'[\s\S]*'`
   - Test each rule independently: `\\s`→`\s`, `\\S`→`\S`, `\\d`→`\d`, `\\D`→`\D`, `\\W`→`\W`, `\\b`→`\b`, `\\B`→`\B`, `\\A`→`\A`, `\\Z`→`\Z`
   - Test `test_unescape_backslash_b_raw_string`: verify `unescape(r'\\b')` produces the two-character string `\b` (backslash + b), not the backspace character `\x08` (confirms raw string handling is correct)
   - Test `test_unescape_rules_count`: import `ACVerifier._UNESCAPE_RULES` and assert `len() >= 17`
3. Run test — confirm FAIL (RED)
4. Append 9 entries to `_UNESCAPE_RULES`:
   ```python
   (r'\\s', r'\s'),
   (r'\\S', r'\S'),
   (r'\\d', r'\d'),
   (r'\\D', r'\D'),
   (r'\\W', r'\W'),
   (r'\\b', r'\b'),
   (r'\\B', r'\B'),
   (r'\\A', r'\A'),
   (r'\\Z', r'\Z'),
   ```
5. Run test — confirm PASS (GREEN)
6. Verify AC#4: total rule count in `_UNESCAPE_RULES` is now >= 17

**Task 3 — Propagate multiline=true to re.DOTALL (AC#5, AC#6)**

1. Read `_extract_grep_params` return signature and `_verify_content` at ac-static-verifier.py
2. Write `src/tools/python/tests/test_ac_verifier_dotall.py`:
   - `test_dotall_with_multiline_param`: create temp file with `"line1\nline2"`, use `Grep(..., multiline=true)` with pattern `.+line2`, assert PASS
   - `test_no_dotall_without_multiline_param`: same file, omit `multiline=true`, use `.+line2` pattern (`.` does NOT match `\n` without DOTALL), assert FAIL
3. Run test — confirm FAIL (RED)
4. Define `GrepParams(NamedTuple)` with fields `file_path`, `pattern`, `error_result`, `use_dotall: bool = False`
5. Refactor `_extract_grep_params` to return `GrepParams` instead of 3-tuple:
   - `use_dotall = (parsed.get('multiline', '').lower() == 'true') if parsed else False`
   - `return GrepParams(file_path, pattern, None, use_dotall)`
6. Change `_verify_content` parameter from inline `re.MULTILINE` to `re_flags: int = re.MULTILINE`
7. Replace `re.MULTILINE` at lines ~674, ~704, ~774 with `re_flags`
8. Update BOTH call sites (`verify_code_ac` at line ~1004 AND `_verify_file_content` at line ~1154) to use attribute access (e.g., `params = self._extract_grep_params(ac); params.file_path, params.pattern, params.use_dotall`) — NOT tuple unpacking — compute `re_flags = re.MULTILINE | (re.DOTALL if params.use_dotall else 0)`, and pass it to `_verify_content`
9. Verify both call sites are updated: `Grep(ac-static-verifier.py, pattern="re_flags")` should match at least 2 call sites plus the `_verify_content` signature
10. Run test — confirm PASS (GREEN)

**Task 4 — Pipe alternation convention test + diagnostic warning (AC#7, AC#8, AC#12)**

1. Write `src/tools/python/tests/test_ac_verifier_pipe_count.py`:
   - `test_pipe_alternation_with_gte`: create temp file with content `foo\nbar\nbaz`, use `Grep(..., pattern="foo|bar")` with `gte` matcher expecting 2, assert PASS (bare `|` is regex alternation)
   - `test_pipe_escape_warning_with_gte`: create temp file, use pattern with `\|` and `gte` matcher, capture stderr, assert WARNING message emitted
2. Run `test_ac_verifier_pipe_count.py` — confirm `test_pipe_alternation_with_gte` PASS, `test_pipe_escape_warning_with_gte` FAIL (RED — no warning yet)
3. In `_verify_content`, add diagnostic warning: when matcher in (count_equals, gt, gte, lt, lte) and `\\|` in pattern, emit `print(f"WARNING: AC#{ac_number} pattern contains \\| with {matcher} matcher; use bare | for alternation", file=sys.stderr)`
4. Run `test_ac_verifier_pipe_count.py` — confirm all PASS (GREEN)
5. Run `test_ac_verifier_pipe_regex.py` — confirm PASS (regression: `\|` preserved for `matches` matcher)

**Task 5 — Full F841 verification (AC#1)**

1. Write `src/tools/python/tests/test_f841_verification.py`:
   - Use subprocess to run `python src/tools/python/ac-static-verifier.py --feature 841 --ac-type code`
   - Assert exit code 0
   - Assert stdout contains `15/15 passed`
2. Run test — confirm PASS (all four fixes together achieve 15/15)
3. If FAIL: Check which F841 ACs still fail; identify which Fix (1-4) did not resolve them; STOP and report

**Task 6 — Full pytest suite (AC#9, AC#10, AC#13)**

1. Run `pytest src/tools/python/tests/ -v` — confirm all tests PASS
2. If any existing test FAILS: STOP and report to user — do not modify existing tests

**Task 7 — Document pipe convention and multiline semantics (AC#14)**

1. Read `pm/reference/ac-matcher-mapping.md`
2. Add a section documenting: (a) for count/gte matchers, use bare `|` for alternation; `\|` is literal pipe only for matches/not_matches matchers; (b) `multiline=true` in Grep Method column enables `re.DOTALL` (dot matches newlines), not `re.MULTILINE` (which is always on)
3. Verify `Grep(pm/reference/ac-matcher-mapping.md, pattern="bare.*\\|.*alternation")` matches

### Build Verification Steps

```bash
# All new tests
pytest src/tools/python/tests/test_ac_verifier_escaped_quote.py -v
pytest src/tools/python/tests/test_ac_verifier_unescape_charclass.py -v
pytest src/tools/python/tests/test_ac_verifier_dotall.py -v
pytest src/tools/python/tests/test_ac_verifier_pipe_count.py -v

# Regression suite
pytest src/tools/python/tests/test_ac_verifier_pipe_regex.py -v
pytest src/tools/python/tests/test_ac_verifier_unescape_metachar.py -v
pytest src/tools/python/tests/test_ac_verifier_escape.py -v

# F841 end-to-end
pytest src/tools/python/tests/test_f841_verification.py -v

# Full test suite (no regressions)
pytest src/tools/python/tests/ -v
```

### Success Criteria

- All 5 new test files PASS
- Full pytest suite passes (union regression gate)
- Pipe convention and multiline semantics documented in `pm/reference/ac-matcher-mapping.md`
- `python src/tools/python/ac-static-verifier.py --feature 841 --ac-type code` exits 0 with 15/15 passed
- No existing test in `src/tools/python/tests/` regresses

### Error Handling

| Scenario | Action |
|----------|--------|
| Task 5 regression test fails | STOP — existing tests are read-only, do not modify |
| Task 6 still shows < 15/15 after all fixes | STOP — report remaining failures with exact AC numbers |
| `_extract_grep_params` call site not found | STOP — report; do not guess line numbers |
| `re.DOTALL` test behavior unexpected | STOP — report; AC#6 Upstream Issues notes `[\s\S]` vs `.` distinction |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| `_verify_content` matches/not_matches code duplication (lines 665-724) | Existing technical debt; extracting shared helper reduces triple-site flag propagation | New Feature | F843 | (created during /run) | [x] | 作成済み |
| `_UNESCAPE_RULES` catch-all regex refactoring | Replace per-class entries with generic regex `r'\\\\([sSdDwWbBAZ])' → r'\\1'` | New Feature | F844 | (created during /run) | [x] | 作成済み |
| Full AC pattern catalog scan across all 275+ features | Validates parser handles union of all existing AC pattern syntaxes (Philosophy claim) | New Feature | F845 | (created during /run) | [x] | 作成済み |

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
| 2026-03-06 | PHASE_START | orchestrator | Phase 1 Initialize | READY:842:infra |
<!-- run-phase-1-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 2 Investigation | Explorer dispatched, no deviations |
<!-- run-phase-2-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 3 Skip (infra type) | Type routing: infra → skip |
<!-- run-phase-3-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 4 Implementation | Tasks 5-6 completed; F841 AC#14 \| syntax fixed (bare pipe); 280/280 pytest passed |
<!-- run-phase-4-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 7 Verification | 14/14 ACs PASS; code 2/2, exit_code 12/12; 280/280 pytest; 0 deviations |
<!-- run-phase-7-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 8 Post-Review | 8.1 READY; 8.2 skipped (no extensibility); 8.3 N/A |
<!-- run-phase-8-completed -->
| 2026-03-06 | DEVIATION | Bash | pytest (bare) | exit 127 — command not found; retried with python -m pytest |
| 2026-03-06 | DEVIATION | Bash | ac-static-verifier --ac-type exit_code | exit 2 — invalid choice; exit_code not supported by static verifier |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase3-Maintainability iter1: Technical Design > _extract_grep_params return type | 4-tuple → GrepParams NamedTuple for extensibility
- [fix] Phase3-Maintainability iter1: Technical Design > _verify_content flags | use_dotall: bool → re_flags: int for extensibility
- [fix] Phase3-Maintainability iter1: Technical Design > Fix 4 pipe convention | Added diagnostic warning for \| with count/gte matchers
- [fix] Phase3-Maintainability iter1: AC#4 matcher | Fragile grep → pytest unit test for rule count verification
- [fix] Phase3-Maintainability iter1: Unescape rules scope | 4 rules → 7 rules (added \\W, \\b, \\B) per Zero Debt Upfront
- [fix] Phase2-Review iter2: AC Definition Table / Goal item 4 | Added AC#12 for diagnostic warning verification
- [fix] Phase2-Review iter2: Tasks table, Task 3 description | Updated to reference GrepParams NamedTuple + re_flags parameter
- [fix] Phase3-Maintainability iter3: Task 4 Execution Order | Added diagnostic warning implementation step + moved from Phase 1 to Phase 2 in Implementation Contract
- [fix] Phase3-Maintainability iter3: Task 3 Execution Order | Added explicit both-call-site verification step (line ~1004 and ~1154)
- [fix] Phase3-Maintainability iter3: Philosophy Derivation | Narrowed regression claim to match AC coverage (added AC#1 to row)
- [fix] Phase3-Maintainability iter3: Task 2 Execution Order | Added \\b raw string verification test case
- [fix] Phase3-Maintainability iter4: Philosophy Coverage | Added AC#13 full pytest suite as union regression gate
- [fix] Phase3-Maintainability iter4: Task 4 Execution Order | Reordered to TDD RED→GREEN (write tests first)
- [fix] Phase3-Maintainability iter4: Task 3 Step 8 | Clarified attribute access vs tuple unpacking for GrepParams
- [fix] Phase3-Maintainability iter4: Pipe convention | Added AC#14 + Task 8 for ac-matcher-mapping documentation
- [fix] Phase3-Maintainability iter5: Unescape rules scope | 7 rules → 9 rules (added \\A, \\Z) per Zero Debt Upfront
- [fix] Phase3-Maintainability iter5: _verify_content duplication | Added Mandatory Handoff F843 for helper extraction
- [fix] Phase3-Maintainability iter5: _UNESCAPE_RULES catch-all | Added Mandatory Handoff F844 for regex refactoring
- [fix] Phase3-Maintainability iter5: AC pattern catalog | Added Mandatory Handoff F845 for full scan
- [fix] Phase3-Maintainability iter5: Task 5 merged into Task 7 | Eliminated orphan regression task (subsumed by full pytest suite)
- [fix] Phase3-Maintainability iter5: Task 8 expanded | Added multiline=true parameter documentation alongside pipe convention
- [fix] Phase3-Maintainability iter6: Implementation Contract | Added Task 7 to Phase 2 scope (pipe/multiline docs)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->
## Links

[Related: F841](feature-841.md) - Parent feature whose ACs triggered the 4 failures
[Related: F834](feature-834.md) - Unescape rule consolidation and `\|` preservation architecture
[Related: F832](feature-832.md) - Positional arg parsing for Grep methods
[Related: F804](feature-804.md) - Added `\\w` unescape rule (pattern for F842 additions)
[Related: F818](feature-818.md) - Cross-repo and WSL support; introduced complex method parsing
[Related: F838](feature-838.md) - Cross-repo verifier path resolution

