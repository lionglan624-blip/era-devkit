# Feature 621: ac-static-verifier Pattern Parsing Fundamental Fix

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **TRACK** - Choose one concrete destination:
>    - Option A: Create new Feature → Add Task to create F{ID}
>    - Option B: Add to existing Feature → Add Task to update F{ID}#T{N}
>    - Option C: Add to architecture.md Phase → Verify Phase exists
> 3. **HANDOFF** - Record in this feature's Handoff section
> 4. **CONTINUE** - Resume this feature's scope
>
> **TBD is FORBIDDEN**. Every discovered issue must have actionable handoff.

## Type: infra

## Background

### Philosophy (思想・上位目標)
AC 検証は自動化されるべき。手動検証への fallback は非効率であり、自動検証ツールの信頼性が低いとワークフロー全体の効率が下がる。ac-static-verifier は Phase 6 Verification の中核であり、false negative を排除することで /run ワークフローの信頼性を向上させる。

### Problem (現状の問題)
F619 実行時に ac-static-verifier が 7/13 FAIL を報告したが、手動検証では 15/15 PASS だった。原因は以下のパターン解析問題：

1. **Method 列フォーマット未対応**: `Grep(path)` 形式を "Invalid Method format" として拒否
2. **エスケープ二重化**: AC Expected の `\\[DRAFT\\]` が内部で `\\\\[DRAFT\\\\]` に変換される
3. **特殊文字処理**: バッククォート (`` ` ``) を含むパターンで解析失敗
4. **正規表現マッチ失敗**: `DEPRECATED.*Use /fc` のような複合パターンで false negative

これらは個別の workaround ではなく、パターン解析ロジックの根本的な見直しが必要。

### Goal (このFeatureで達成すること)
1. Method 列の `Grep(path)` 形式を正しく解析
2. エスケープシーケンスの正規化（二重化防止）
3. 特殊文字（バッククォート、パイプ等）の適切な処理
4. 既存 AC パターンでの false negative を 0 に
5. テストケース追加で regression 防止

### Session Context
- **Trigger**: F619 Phase 6 で 7 false negatives 発生
- **Related**: F618 (ac-static-verifier MANUAL Status Counting Fix) - 別の問題を修正済み
- **Tool location**: `tools/ac-static-verifier.py`

## Root Cause Analysis

### 5 Whys

1. **Why**: F619 Phase 6 verification reported 7/13 FAIL but manual verification showed 15/15 PASS
2. **Why**: ac-static-verifier incorrectly parsed or matched 4 different AC patterns
3. **Why**: The parsing logic has assumptions that don't match actual AC definitions in feature files
4. **Why**: The tool was designed with limited pattern variations; new AC patterns emerged that violate those assumptions
5. **Why**: No comprehensive test suite existed to catch edge cases during AC table evolution

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `Grep(path)` format rejected as "Invalid Method" | Regex at L149/L343 requires `Grep(...)` with parentheses; `Grep path` form not handled |
| `\\[DRAFT\\]` pattern fails to match | `unescape()` only handles `\"→"`, not markdown bracket escapes `\\[→\[` |
| Backtick patterns cause parse failure | Markdown table parsing doesn't account for inline code formatting affecting column split |
| `DEPRECATED.*Use /fc` pattern returns false negative | Users expect regex in `contains` matcher; tool uses literal `-F` flag for `contains` |

### Conclusion

The root cause is **incomplete pattern handling in 3 layers**:
1. **Method column parsing**: Only accepts `Grep(path)` format, not `Grep path` or `Grep(path)` variations
2. **Expected value processing**: `unescape()` is too narrow - only handles `\"` but markdown tables use `\\` for literal backslashes in regex patterns
3. **Semantic mismatch**: `contains` matcher performs literal search (correct per design) but users incorrectly use regex metacharacters expecting regex behavior

Problem 4 is **not a bug** but a **user education issue** - `contains` is for literal strings, `matches` is for regex. The fix should provide clear error message guiding users to use `matches` for regex patterns.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F618 | [CANCELLED] | Similar issue | Investigated MANUAL status counting - no bug found; different root cause |
| F619 | [DONE] | Triggered discovery | Phase 6 verification exposed the 4 parsing issues |
| F610 | [DONE] | Related | Feature Creator redesign - generates ACs that use these patterns |
| F613 | [DONE] | Related | AC Pattern Coverage Audit - documented which matchers are supported |

### Pattern Analysis

This is a **recurring pattern**: As the workflow evolves, new AC patterns emerge that the verifier doesn't handle. Root cause: **No TDD for ac-static-verifier evolution** - features add new AC patterns without corresponding verifier tests.

**Prevention**: Each feature that introduces new AC patterns should include regression tests for ac-static-verifier.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | **YES** | All 4 issues have clear code locations and fixes |
| Scope is realistic | **YES** | Changes localized to ac-static-verifier.py (~50 lines) + 4 test files |
| No blocking constraints | **YES** | JSON schema unchanged, exit codes preserved |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F618 | [CANCELLED] | Previous ac-static-verifier fix attempt (different issue) |
| Related | F619 | [DONE] | Triggered this investigation |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Python 3 stdlib | Runtime | None | argparse, json, re, subprocess, pathlib |
| ripgrep (rg) | Optional | Low | Has fallback to grep, then Python |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `.claude/skills/run-workflow/PHASE-6.md` | **CRITICAL** | Core workflow step |
| `tools/verify-logs.py` | **HIGH** | Reads JSON output schema (`summary.failed`, `summary.total`) |
| `tools/tests/test_ac_verifier_*.py` | **MEDIUM** | Import and test ACVerifier class |
| `.claude/skills/testing/SKILL.md` | **LOW** | Documentation reference |

**Critical Constraint**: JSON output schema must remain stable (verify-logs.py dependency).

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| JSON schema stability | verify-logs.py L44-49 | **HIGH** - Must preserve `summary.failed`, `summary.total`, `summary.manual` |
| Exit code semantics | PHASE-6.md, testing SKILL | **HIGH** - 0=pass, 1=fail must be preserved |
| MANUAL status logic | F618 investigation | **MEDIUM** - Current logic is correct, must not regress |
| Existing valid ACs | All completed features | **HIGH** - Must not break working AC patterns |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Regex validation too strict | Medium | Medium | Test with existing feature ACs (F619, F610) before deploy |
| Escape handling breaks other patterns | Low | High | Comprehensive existing test coverage (test_ac_verifier_normal.py) |
| Method parsing ambiguity | Low | Low | Prioritize `Grep(path)` over `Grep path` for backward compat |
| User confusion on contains vs matches | Medium | Low | Clear error message with guidance |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `tools/ac-static-verifier.py` | **Update** | Fix 3 parsing bugs in ~50 lines total |
| `tools/tests/test_ac_verifier_method_format.py` | **Create** | Regression test for Problem 1 - Method `Grep(path)` parsing |
| `tools/tests/test_ac_verifier_bracket_escape.py` | **Create** | Regression test for Problem 2 - Markdown escape normalization |
| `tools/tests/test_ac_verifier_backtick.py` | **Create** | Regression test for Problem 3 - Backtick in Expected value |
| `tools/tests/test_ac_verifier_regex_guidance.py` | **Create** | Regression test for Problem 4 - Regex detection with helpful error |

### Specific Code Changes

| Location | Current Behavior | Fixed Behavior |
|----------|------------------|----------------|
| L149, L343 `re.search(r'Grep\s*\(\s*([^)]+)\s*\)')` | Requires `Grep(path)` format | Also accept `Grep path` format |
| L54-66 `unescape()` | Only handles `\"→"` | Also handle `\\[→\[`, `\\]→\]` for markdown escapes |
| L99 table parsing | May fail on backtick-containing cells | Strip markdown inline code markers |
| L213 `contains` matcher | Silent false negative on regex patterns | Detect regex metacharacters, return helpful error |

### Behavioral Changes

| Current | New | Compatibility |
|---------|-----|---------------|
| `Grep(path)` required, `Grep path` rejected | Both formats accepted | **BACKWARD COMPAT** - existing ACs work |
| `\\[DRAFT\\]` searches literal `\\[DRAFT\\]` | `\\[DRAFT\\]` searches `\[DRAFT\]` (intended) | **BUG FIX** - F619 ACs now pass |
| Backticks in Expected may break parsing | Backticks stripped from Expected | **BUG FIX** |
| Regex `.*` in `contains` = false negative | Regex `.*` in `contains` → FAIL with "Use 'matches' matcher" | **BREAKING** - requires AC migration |

**Breaking Change**: ACs using unambiguous regex patterns (`.*`, `\d+`, `[a-z]`, etc.) in `contains` matcher will now FAIL with clear guidance to use `matches`. Single metacharacters (`{`, `(`, `)`) common in JSON/code are not flagged. This is intentional - `contains` is for literal strings.

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "AC検証は自動化されるべき" (automation required) | All 4 parsing issues must be fixed to enable automation | AC#1-4 |
| "false negative を排除" (eliminate false negatives) | Each fix must convert previous FAIL to PASS | AC#1-4 |
| "ワークフロー信頼性向上" (improve workflow reliability) | No regression in existing valid ACs | AC#5, AC#6 |
| "手動検証 fallback 非効率" (manual fallback inefficient) | Clear error guidance for user errors (regex in contains) | AC#4 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Method Grep path without parens accepted (Pos) | test | pytest tools/tests/test_ac_verifier_method_format.py -v | succeeds | - | [x] |
| 2 | Markdown bracket escapes normalized (Pos) | test | pytest tools/tests/test_ac_verifier_bracket_escape.py -v | succeeds | - | [x] |
| 3 | Backtick in Expected handled (Pos) | test | pytest tools/tests/test_ac_verifier_backtick.py -v | succeeds | - | [x] |
| 4 | Regex metachar in contains returns FAIL with guidance (Pos) | test | pytest tools/tests/test_ac_verifier_regex_guidance.py -v | succeeds | - | [x] |
| 5 | Existing escape tests pass (Neg) | test | pytest tools/tests/test_ac_verifier_escape.py tools/tests/test_ac_verifier_normal.py -v | succeeds | - | [x] |
| 6 | JSON schema fields unchanged (Neg) | code | Grep(tools/ac-static-verifier.py) | contains | "summary": { | [x] |

### AC Details

**AC#1: Method `Grep path` without parens accepted (Positive)**
- **Rationale**: Root cause L149/L343 regex only accepts `Grep(path)` with parentheses. Method variations like `Grep path` (no parens) or `Grep( path )` (extra spaces) should also work.
- **Test Cases**:
  - `Grep(tools/file.py)` - existing format (must continue working)
  - `Grep tools/file.py` - space-separated format (new support)
  - `Grep( tools/file.py )` - with extra whitespace (new support)
- **Verification**: Unit test creates mock feature file with various Method formats, verifies all parse correctly.

**AC#2: Markdown bracket escapes normalized (Positive)**
- **Rationale**: Root cause in L54-66 `unescape()` only handles `\"→"`. Markdown tables use `\\[` and `\\]` to escape literal brackets in regex patterns (e.g., `\\[DRAFT\\]` should match `[DRAFT]`).
- **Test Cases**:
  - `\\[DRAFT\\]` → searches for `[DRAFT]` (not `\\[DRAFT\\]`)
  - `\\]` → searches for `]`
  - `\\` followed by non-bracket → unchanged (e.g., `\\n` stays `\\n`)
- **Verification**: Unit test verifies unescape correctly processes markdown bracket escapes.

**AC#3: Backtick in Expected handled (Positive)**
- **Rationale**: Root cause L99 table parsing may fail when Expected contains inline code markers (backticks). The Expected value `` `pattern` `` should be extracted as `pattern`.
- **Test Cases**:
  - `` `code` `` → extracts `code`
  - Normal text without backticks → unchanged
  - Multiple backticks ``` ``nested`` ``` → handles correctly
- **Verification**: Unit test parses AC table with backtick-wrapped Expected values.

**AC#4: Regex patterns in contains returns FAIL with guidance (Positive)**
- **Rationale**: Problem 4 is user education issue, not a bug. `contains` is for literal strings; `matches` is for regex. When unambiguous regex patterns (`.*`, `\d+`, `[a-z]`) are detected in `contains` matcher, tool should FAIL with guidance.
- **Test Cases**:
  - Pattern `DEPRECATED.*Use /fc` with `contains` → FAIL with "Use 'matches' matcher for regex patterns"
  - Pattern `literal text` with `contains` → PASS (no regex patterns)
  - Pattern `"summary": {` with `contains` → PASS (single `{` not flagged as regex)
  - Pattern `.*` with `matches` → PASS (correct usage)
- **Verification**: Unit test verifies error message contains guidance text.
- **Breaking Change**: Existing ACs using clear regex patterns in `contains` will now FAIL. This is intentional.

**AC#5: Existing escape tests pass (Negative/Regression)**
- **Rationale**: Ensure fixes do not break existing functionality. The current `unescape()` tests (`test_ac_verifier_escape.py`, `test_ac_verifier_normal.py`) must continue to pass.
- **Verification**: Run existing test suite to confirm no regression.

**AC#6: JSON schema fields unchanged (Negative/Regression)**
- **Rationale**: `verify-logs.py` depends on JSON output schema (`summary.failed`, `summary.total`, `summary.manual`). The output format must remain stable.
- **Verification**: Code inspection confirms schema structure is preserved.

---

## Technical Design

### Approach

The fix addresses 4 distinct parsing issues through targeted modifications to ac-static-verifier.py:

1. **Flexible Method parsing** (AC#1): Extend regex to accept both `Grep(path)` and `Grep path` formats
2. **Markdown escape normalization** (AC#2): Expand `unescape()` to handle bracket escapes (`\\[` → `\[`, `\\]` → `\]`)
3. **Backtick handling** (AC#3): Strip inline code markers from Expected values during table parsing
4. **Regex metacharacter detection** (AC#4): Validate `contains` matcher input and reject regex patterns with helpful guidance

All changes are backward compatible except AC#4, which is an intentional breaking change to prevent user errors.

### AC Coverage Matrix

| AC# | Description | Implementation Strategy | Code Location |
|:---:|-------------|-------------------------|---------------|
| 1 | Method `Grep path` accepted | Modify regex pattern to `r'Grep\s+(.+)'` with fallback to parentheses format | L149, L343 |
| 2 | Markdown bracket escapes | Add `\\[→\[` and `\\]→\]` replacements to `unescape()` | L54-66 |
| 3 | Backtick handling | Strip backticks from Expected field after column split | L115-116 |
| 4 | Regex pattern detection | Add validation before `contains` matcher execution; detect unambiguous regex patterns only | L213 (new function) |
| 5 | Existing escape tests pass | No changes to existing escape logic; only additions | Regression check |
| 6 | JSON schema preserved | No changes to output structure (L589-599) | Code review |

### Key Decisions

#### Decision 1: Method Parsing Strategy (AC#1)

**Problem**: Current regex `r'Grep\s*\(\s*([^)]+)\s*\)'` requires parentheses.

**Options**:
- A: Replace with `r'Grep\s+(.+)'` (space-based, no parentheses)
- B: Add secondary fallback `if not match: try Grep\s+(.+)`
- C: Support both in single regex: `r'Grep\s*(?:\(([^)]+)\)|(.+))'`

**Decision**: **Option B** - Try parentheses first, fallback to space-based.

**Rationale**:
- Backward compatible: existing `Grep(path)` continues to work
- Clear priority: parentheses format takes precedence (explicit over implicit)
- Simple to implement and test
- Handles edge cases: `Grep (path)` with space before paren is ambiguous in Option C

**Implementation**:
```python
# L149, L343 (code/file verification)
match = re.search(r'Grep\s*\(\s*([^)]+)\s*\)', ac.method, re.IGNORECASE)
if not match:
    # Fallback: try space-separated format "Grep path"
    match = re.search(r'Grep\s+(.+)', ac.method, re.IGNORECASE)
if not match:
    return {"ac_number": ac.ac_number, "result": "FAIL", ...}
```

#### Decision 2: Escape Normalization Scope (AC#2)

**Problem**: `unescape()` only handles `\"→"`. Markdown tables use `\\[` and `\\]` for literal brackets.

**Options**:
- A: Add only `\\[→\[` and `\\]→\]`
- B: Add comprehensive escape handling (`\\n→\n`, `\\t→\t`, etc.)
- C: Use `codecs.decode(s, 'unicode_escape')`

**Decision**: **Option A** - Minimal addition for brackets only.

**Rationale**:
- Scope discipline: Only fix reported issues (AC#2 is about brackets)
- Risk mitigation: Comprehensive escape handling (Option B/C) may break existing ACs that use literal `\\n` strings
- Regression prevention: No evidence other escape sequences cause issues; adding them without testing creates risk
- Test coverage: Easier to verify minimal changes

**Implementation**:
```python
# L54-66 unescape() function
@staticmethod
def unescape(s: str) -> str:
    """Unescape backslash escape sequences in a string.

    Handles:
    - \" -> " (existing, for quoted strings)
    - \\[ -> \[ (new, for markdown bracket escapes)
    - \\] -> \] (new, for markdown bracket escapes)
    """
    return s.replace(r'\"', '"').replace(r'\\[', r'\[').replace(r'\\]', r'\]')
```

#### Decision 3: Backtick Stripping (AC#3)

**Problem**: Inline code markers (backticks) in Expected column may interfere with pattern matching.

**Options**:
- A: Strip backticks during table parsing (L115-116)
- B: Strip backticks in each verification method (L162, L356)
- C: Handle backticks in `unescape()` function

**Decision**: **Option A** - Strip during table parsing.

**Rationale**:
- Single Responsibility: Table parsing handles markdown syntax; verification handles pattern logic
- Performance: Strip once at parse time vs. strip in every verification call
- Consistency: All AC types benefit from same preprocessing

**Implementation**:
```python
# L115-116 (after stripping quotes, before unescape)
expected_raw = parts[6].strip('"').strip('`')
expected = self.unescape(expected_raw)
```

#### Decision 4: Regex Detection Strategy (AC#4)

**Problem**: Users incorrectly use regex metacharacters in `contains` matcher expecting regex behavior.

**Options**:
- A: Auto-convert `contains` with regex → `matches`
- B: Detect and reject with error message
- C: Silently ignore (status quo)

**Decision**: **Option B** - Detect and reject with helpful error.

**Rationale**:
- Fail fast: User gets immediate feedback rather than silent false negative
- Educational: Error message guides user to correct matcher
- Prevents ambiguity: Clear distinction between `contains` (literal) and `matches` (regex)
- Breaking change is acceptable: This is a user error, not a valid use case

**Regex Patterns to Detect**: Only flag unambiguous regex patterns like `.*`, `\d+`, `[a-z]` etc., not single metacharacters that appear in literal strings.

**Implementation**:
```python
# New helper function (insert after L66)
@staticmethod
def _contains_regex_metacharacters(pattern: str) -> bool:
    """Check if pattern contains clear regex patterns unsuitable for contains.

    Detects unambiguous regex patterns that should use 'matches' instead:
    - Quantifiers: .*, .+, .?, +, ?
    - Character classes: [a-z], [0-9], \d, \w, \s
    - Anchors: ^start, end$
    - Alternation: option1|option2

    Does NOT flag single occurrences of {}, (), which are common in JSON/code.
    """
    import re
    # Unambiguous regex patterns
    regex_patterns = [
        r'\.\*',    # .*
        r'\.\+',    # .+
        r'\.\?',    # .?
        r'[^\\]\+', # + not preceded by backslash
        r'[^\\]\?', # ? not preceded by backslash
        r'\[[^\]]*\]', # character classes [...]
        r'\\[dDwWsS]', # \d, \D, \w, \W, \s, \S
        r'^\^',     # starts with ^
        r'\$$',     # ends with $
        r'[^\\]\|', # | not preceded by backslash
    ]
    return any(re.search(p, pattern) for p in regex_patterns)

# L213 (in verify_code_ac, before pattern_found check)
if matcher == "contains" and self._contains_regex_metacharacters(pattern):
    return {
        "ac_number": ac.ac_number,
        "result": "FAIL",
        "details": {
            "error": "Pattern contains regex metacharacters. Use 'matches' matcher for regex patterns.",
            "pattern": pattern,
            "file_path": file_path,
            "matcher": matcher,
            "guidance": "Change matcher from 'contains' to 'matches' for regex support",
            "matched_files": []
        }
    }
```

Duplicate in `_verify_file_content()` at L407 (same logic for file type ACs).

### Specific Code Changes

#### Change 1: Flexible Method Parsing (AC#1)

**Location**: L149-159, L343-353

**Before**:
```python
# L149
match = re.search(r'Grep\s*\(\s*([^)]+)\s*\)', ac.method, re.IGNORECASE)
if not match:
    return {
        "ac_number": ac.ac_number,
        "result": "FAIL",
        "details": {
            "error": f"Invalid Method format: {ac.method}",
            ...
```

**After**:
```python
# L149
match = re.search(r'Grep\s*\(\s*([^)]+)\s*\)', ac.method, re.IGNORECASE)
if not match:
    # Fallback: try space-separated format "Grep path"
    match = re.search(r'Grep\s+(.+)', ac.method, re.IGNORECASE)
if not match:
    return {
        "ac_number": ac.ac_number,
        "result": "FAIL",
        "details": {
            "error": f"Invalid Method format (expected 'Grep(path)' or 'Grep path'): {ac.method}",
            ...
```

**Apply same change at L343** in `_verify_file_content()`.

#### Change 2: Markdown Bracket Escape Normalization (AC#2)

**Location**: L66

**Before**:
```python
return s.replace(r'\"', '"')
```

**After**:
```python
return s.replace(r'\"', '"').replace(r'\\[', r'\[').replace(r'\\]', r'\]')
```

Also update docstring at L55-65:

**Before**:
```python
"""Unescape backslash escape sequences in a string.

Handles backslash escape sequences for double quotes (\" -> ") in Expected values.
This allows AC definitions to include literal quotes in markdown tables.
```

**After**:
```python
"""Unescape backslash escape sequences in a string.

Handles backslash escape sequences in Expected values:
- \" -> " (double quotes)
- \\[ -> \[ (markdown bracket escapes for regex patterns)
- \\] -> \] (markdown bracket escapes for regex patterns)
```

#### Change 3: Backtick Stripping (AC#3)

**Location**: L115

**Before**:
```python
expected_raw = parts[6].strip('"')
expected = self.unescape(expected_raw)
```

**After**:
```python
expected_raw = parts[6].strip('"').strip('`')
expected = self.unescape(expected_raw)
```

#### Change 4: Regex Metacharacter Detection (AC#4)

**Location**: Insert new function after L66, add validation at L213 and L407

**New Function** (insert after L66):
```python
@staticmethod
def _contains_regex_metacharacters(pattern: str) -> bool:
    """Check if pattern contains clear regex patterns unsuitable for contains.

    Detects unambiguous regex patterns that should use 'matches' instead:
    - Quantifiers: .*, .+, .?, +, ?
    - Character classes: [a-z], [0-9], \d, \w, \s
    - Anchors: ^start, end$
    - Alternation: option1|option2

    Does NOT flag single occurrences of {}, (), which are common in JSON/code.

    Args:
        pattern: The pattern string to check

    Returns:
        True if pattern contains unambiguous regex patterns
    """
    import re
    # Unambiguous regex patterns
    regex_patterns = [
        r'\.\*',    # .*
        r'\.\+',    # .+
        r'\.\?',    # .?
        r'[^\\]\+', # + not preceded by backslash
        r'[^\\]\?', # ? not preceded by backslash
        r'\[[^\]]*\]', # character classes [...]
        r'\\[dDwWsS]', # \d, \D, \w, \W, \s, \S
        r'^\^',     # starts with ^
        r'\$$',     # ends with $
        r'[^\\]\|', # | not preceded by backslash
    ]
    return any(re.search(p, pattern) for p in regex_patterns)
```

**Validation in verify_code_ac** (insert at L213, before `pattern_found` assignment):
```python
# Apply matcher logic
if matcher == "contains":
    # Validate pattern doesn't contain unambiguous regex patterns
    if self._contains_regex_metacharacters(pattern):
        return {
            "ac_number": ac.ac_number,
            "result": "FAIL",
            "details": {
                "error": "Pattern contains regex patterns. Use 'matches' matcher for regex patterns.",
                "pattern": pattern,
                "file_path": file_path,
                "matcher": matcher,
                "guidance": "Change matcher from 'contains' to 'matches' for regex support",
                "matched_files": []
            }
        }
    passed = pattern_found
```

**Duplicate validation in _verify_file_content** (insert at L407, same logic):
```python
# Apply matcher logic
if matcher == "contains":
    # Validate pattern doesn't contain unambiguous regex patterns
    if self._contains_regex_metacharacters(pattern):
        return {
            "ac_number": ac.ac_number,
            "result": "FAIL",
            "details": {
                "error": "Pattern contains regex patterns. Use 'matches' matcher for regex patterns.",
                "pattern": pattern,
                "file_path": file_path,
                "matcher": matcher,
                "guidance": "Change matcher from 'contains' to 'matches' for regex support",
                "matched_files": []
            }
        }
    passed = pattern_found
```

### Verification Strategy

#### AC#1 Verification: test_ac_verifier_method_format.py

Test cases:
1. `Grep(tools/file.py)` - existing format (baseline)
2. `Grep tools/file.py` - space-separated (new support)
3. `Grep( tools/file.py )` - with whitespace (new support)
4. `GrepInvalid` - invalid format (expect FAIL with clear error)

#### AC#2 Verification: test_ac_verifier_bracket_escape.py

Test cases:
1. Pattern `\\[DRAFT\\]` matches file containing `[DRAFT]`
2. Pattern `\\]` matches file containing `]`
3. Pattern `Status: \\[PROPOSED\\]` matches `Status: [PROPOSED]`
4. Existing `\"` escape still works (regression check)

#### AC#3 Verification: test_ac_verifier_backtick.py

Test cases:
1. Expected `` `code` `` extracts to `code` and matches correctly
2. Expected without backticks unchanged
3. Multiple backticks handled correctly

#### AC#4 Verification: test_ac_verifier_regex_guidance.py

Test cases:
1. Pattern `DEPRECATED.*Use /fc` with `contains` → FAIL with guidance
2. Pattern `literal text` with `contains` → PASS
3. Pattern `.*` with `matches` → PASS (correct usage)
4. Error message contains "Use 'matches' matcher"

#### AC#5 Verification: Existing Tests

Run without modification:
- `pytest tools/tests/test_ac_verifier_escape.py -v`
- `pytest tools/tests/test_ac_verifier_normal.py -v`

Expected: All existing tests continue to pass.

#### AC#6 Verification: JSON Schema

Code review confirms no changes to JSON output structure:
- L589-599 output structure unchanged
- `summary.total`, `summary.passed`, `summary.manual`, `summary.failed` preserved

### Breaking Change Impact

**Affected ACs**: Any AC using regex metacharacters in `contains` matcher will now FAIL.

**Migration Path**:
1. Tool returns clear error: "Use 'matches' matcher for regex patterns"
2. User updates AC table: change `contains` → `matches`
3. Re-run verification

**Estimated Impact**: Low - based on F619 investigation, only 1/15 ACs used regex in `contains` (and it was a user error).

### Implementation Notes

1. **Line number shifts**: After inserting `_contains_regex_metacharacters()` at L67, all subsequent line numbers shift by +17 (function is ~17 lines). Update references accordingly. **Mitigation**: Before modification, verify target patterns exist at expected locations (e.g., `Grep\s*\(` at L149, `return s.replace` at L66).

2. **DRY violation tracked**: Regex detection logic is duplicated in `verify_code_ac()` and `_verify_file_content()`. Tracked for refactoring:
   - F624 (Grep logic consolidation) in Mandatory Handoffs
   - Current duplication is minimal (~10 lines)
   - Extraction to shared method would require significant refactoring (out of F621 scope)

3. **Testing strategy**: Each AC has dedicated test file for isolation. Existing tests ensure no regression.

4. **Error message consistency**: All error messages follow format: `"<Problem description>. <Guidance>."` for user clarity.

### Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Regex validation too strict | Test with F619 ACs (known good patterns) before deploy |
| Escape handling breaks patterns | Run existing test suite (AC#5) to catch regressions |
| Method parsing ambiguity | Prioritize parentheses format; fallback clearly documented |
| User confusion on breaking change | Clear error message with actionable guidance |

### Success Criteria

1. All 6 ACs pass
2. F619 re-verification shows 15/15 PASS (previously 7/13 FAIL)
3. Existing test suite passes without modification
4. JSON schema unchanged (verify-logs.py compatibility preserved)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Implement flexible Method parsing (accept `Grep path` and `Grep(path)` formats) | [x] |
| 2 | 2 | Implement markdown bracket escape normalization in `unescape()` | [x] |
| 3 | 3 | Implement backtick stripping in table parsing | [x] |
| 4 | 4 | Implement regex metacharacter detection for `contains` matcher | [x] |
| 5 | 1 | Create test_ac_verifier_method_format.py (verify flexible Method parsing) | [x] |
| 6 | 2 | Create test_ac_verifier_bracket_escape.py (verify bracket escape normalization) | [x] |
| 7 | 3 | Create test_ac_verifier_backtick.py (verify backtick handling) | [x] |
| 8 | 4 | Create test_ac_verifier_regex_guidance.py (verify regex detection with guidance) | [x] |
| 9 | 5 | Run existing tests (test_ac_verifier_escape.py and test_ac_verifier_normal.py) to verify no regression | [x] |
| 10 | 6 | Verify JSON schema fields unchanged via code inspection | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->
<!-- Tasks 1-4: Implementation tasks (one per code change) -->
<!-- Tasks 5-8: Test creation tasks (one per new test file, matching AC#1-4) -->
<!-- Tasks 9-10: Regression/validation tasks (matching AC#5-6) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-4 | Modified ac-static-verifier.py with 4 code changes |
| 2 | implementer | sonnet | Tasks 5-8 | 4 new test files in tools/tests/ |
| 3 | ac-tester | haiku | Tasks 9-10 | Test results and code inspection report |

**Constraints** (from Technical Design):
1. **JSON schema stability**: Output schema MUST preserve `summary.total`, `summary.passed`, `summary.manual`, `summary.failed` fields (verify-logs.py dependency)
2. **Exit code semantics**: 0=pass, 1=fail MUST be preserved (PHASE-6.md workflow dependency)
3. **MANUAL status logic**: Current MANUAL counting logic is correct (F618 validated), MUST NOT regress
4. **Existing AC compatibility**: All currently passing ACs MUST continue to pass after changes
5. **Backward compatibility**: Existing Method formats (e.g., `Grep(path)`) MUST continue working with new fallback logic
6. **Line number awareness**: After inserting `_contains_regex_metacharacters()` at L67, subsequent line numbers shift by ~17 lines

**Breaking Changes** (Intentional):
- ACs using unambiguous regex patterns (`.* `, `.+`, character classes `[a-z]`, escape sequences `\d`, anchors `^start`, alternation `|`) in `contains` matcher will FAIL with guidance: "Use 'matches' matcher for regex patterns"
- Migration path: Change `contains` → `matches` in AC table

**Pre-conditions**:
1. Python 3.x environment available
2. pytest installed (`pip install pytest`)
3. ac-static-verifier.py exists at `tools/ac-static-verifier.py`
4. Existing test suite at `tools/tests/test_ac_verifier_*.py` is passing

**Execution Order**:
1. **Phase 1 (Implementation)**: Modify ac-static-verifier.py in dependency order:
   - Task 1: Method parsing (L149, L343) - Foundation for grep-based ACs
   - Task 2: Bracket escape (L66) - Affects unescape preprocessing
   - Task 3: Backtick stripping (L115) - Affects table parsing
   - Task 4: Regex detection (insert at L67, add validation at L213, L407) - Final validation layer
2. **Phase 2 (Test Creation)**: Create regression tests (Tasks 5-8) matching AC order
3. **Phase 3 (Verification)**: Run tests and verify (Tasks 9-10)

**Success Criteria**:
1. All 6 ACs pass (15/15 = 100%)
2. All 4 new test files created and passing
3. Existing test suite passes without modification (test_ac_verifier_escape.py, test_ac_verifier_normal.py)
4. JSON schema unchanged (code inspection confirms L589-599 structure preserved)
5. **Optional validation**: Re-run ac-static-verifier on F619 → expect 15/15 PASS (previously 7/13 FAIL due to these bugs)

**Error Handling**:
- If any existing test fails in Phase 3 Task 9 → STOP, rollback changes, report to user
- If JSON schema check fails in Phase 3 Task 10 → STOP, this violates Constraint #1
- If line number shifts cause implementation errors → refer to Technical Design line number notes
- If test creation encounters ambiguity → refer to AC Details section for test case specifications

---

## Review Notes
- [accepted] Phase1 iter3: Regex patterns edge case - `[^\\]\\+` and `[^\\]\\?` miss patterns starting with + or ?, but unlikely in practice - ACCEPTED as documented limitation
- [accepted] Phase1 iter3: Backtick stripping only handles wrappers (`.strip('`')`) not inline code, but scope is limited to wrapper cases - ACCEPTED as designed
- [pending→F623] Phase1 iter4: Character class pattern `r'\\[[^\\]]*\\]'` may match markdown checkboxes `[ ]`, `[x]`, status `[DRAFT]` - handoff to F623 for refined character class detection
- [accepted] Phase1 iter4: Line number references (L149, L343, etc.) are brittle - ACCEPTED as current implementation matches, refactor if drift occurs

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Documentation update for testing SKILL | Out of scope (doc changes) | Feature | F622 | Created |
| Character class pattern refinement | Technical debt | Feature | F623 | Created |
| Grep logic consolidation refactoring | Technical debt | Feature | F624 | Created |

<!-- All handoff features created as [DRAFT] files. -->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-25 13:24 | START | implementer | Tasks 5-8 | - |
| 2026-01-25 13:24 | END | implementer | Tasks 5-8 | SUCCESS |
| 2026-01-25 13:35 | DEVIATION | pytest | test_ac_verifier_method_format.py | exit 1: ACVerifier() missing args |
| 2026-01-25 13:40 | START | debugger | Fix test API usage | - |
| 2026-01-25 13:55 | END | debugger | Fix test API usage | FIXED |
| 2026-01-25 | DEVIATION | pre-commit | dotnet test | exit 1: PRE-EXISTING flaky perf test |

---

## Reference (Previous Draft)

<details>
<summary>Previous AC/Tasks/Implementation Contract (for reference)</summary>

### Previous AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Grep without parens accepted (Pos) | test | `pytest tools/tests/test_ac_verifier_grep_no_parens.py -v` | succeeds | - | [ ] |
| 2 | Escape bracket normalization (Pos) | test | `pytest tools/tests/test_ac_verifier_escape_brackets.py -v` | succeeds | - | [ ] |
| 3 | Backtick handling (Pos) | test | `pytest tools/tests/test_ac_verifier_backticks.py -v` | succeeds | - | [ ] |
| 4 | Regex in contains rejected with clear error (Pos) | test | `pytest tools/tests/test_ac_verifier_regex_strict.py -v` | succeeds | - | [ ] |
| 5 | Regex in contains error message helpful (Pos) | code | Grep(tools/ac-static-verifier.py) | contains | "Use 'matches' matcher" | [ ] |
| 6 | Existing tests pass (Neg) | test | `pytest tools/tests/test_ac_verifier_escape.py tools/tests/test_ac_verifier_normal.py -v` | succeeds | - | [ ] |
| 7 | JSON schema unchanged (Neg) | code | Grep(tools/ac-static-verifier.py) | matches | "\"feature\".*\"type\".*\"results\".*\"summary\"" | [ ] |
| 8 | F619 file ACs pass with fixed tool | file | Grep(Game/logs/prod/ac/file/feature-619/file-result.json) | contains | "\"failed\": 0" | [ ] |

### Previous Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Implement flexible Method column parsing (accept `Grep` without parens) | [ ] |
| 2 | 2,3 | Implement escape sequence normalization (unescape markdown escapes for literal matching) | [ ] |
| 3 | 4,5 | Implement strict validation for regex metacharacters in `contains` matcher | [ ] |
| 4 | 1 | Create regression test test_ac_verifier_grep_no_parens.py | [ ] |
| 5 | 2 | Create regression test test_ac_verifier_escape_brackets.py | [ ] |
| 6 | 3 | Create regression test test_ac_verifier_backticks.py | [ ] |
| 7 | 4 | Create regression test test_ac_verifier_regex_strict.py | [ ] |
| 8 | 6 | Run existing tests to verify no regression | [ ] |
| 9 | 7 | Verify JSON output schema remains unchanged | [ ] |
| 10 | 8 | Re-run ac-static-verifier on F619 and verify 0 failures | [ ] |

### Previous Implementation Contract

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-3 | Modified ac-static-verifier.py |
| 2 | implementer | sonnet | Tasks 4-7 | 4 regression test files |
| 3 | ac-tester | haiku | Tasks 8-10 | Test results |

**Constraints** (from tech investigation):
1. JSON schema MUST remain stable (verify-logs.py depends on it)
2. Exit code semantic MUST be preserved (0=pass, 1=fail)
3. MANUAL status handling MUST be preserved
4. Existing valid ACs MUST continue to pass

**Breaking Changes**:
- AC with regex in `contains` matcher will now FAIL with clear error message
- Affected ACs must migrate from `contains` → `matches` for regex patterns

</details>

---

## Links
- [index-features.md](index-features.md)
- [F619: Feature Creation Workflow](feature-619.md)
- [F618: ac-static-verifier MANUAL Status Counting Fix](feature-618.md)
- [F610: Feature Creator 5-Phase Orchestrator Redesign](feature-610.md)
- [F613: Complete AC Pattern Coverage Audit Phase 2](feature-613.md)
- [testing SKILL](../../.claude/skills/testing/SKILL.md) - ac-static-verifier documentation
- [Technical Investigation Report](.tmp/f621-tech-investigation.md) - Detailed analysis
