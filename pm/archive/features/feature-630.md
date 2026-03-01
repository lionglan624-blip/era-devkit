# Feature 630: ac-static-verifier Pattern Escaping Fix

## Status: [DONE]

## Type: infra

## Background

### Philosophy
> Reliable AC verification requires tools that correctly handle all valid patterns without manual workarounds.

### Context
F628 implementation revealed ac-static-verifier limitations:
1. `$` character in patterns causes false FAIL (interpreted as regex anchor)
2. Glob patterns like `*.cs` in file paths not properly expanded

These issues force manual verification, defeating the tool's purpose.

## Problem Statement

ac-static-verifier (`tools/ac-static-verifier.py`) fails to verify valid ACs due to pattern handling bugs:

| Issue | Example | Expected | Actual |
|-------|---------|----------|--------|
| `$` escape | `$"Character{id}"` | PASS (found in code) | FAIL (regex anchor) |
| Glob in path | `Era.Core/Characters/*.cs` | Expand and search | File not found |

## Scope

### In Scope
- Fix `$` and other regex special characters in code type patterns
- Fix glob pattern expansion in file type paths
- Add test cases for edge patterns

### Out of Scope
- New matcher types (tracked in testing SKILL Known Limitations)
- Performance optimization

## Dependencies

| Feature | Relationship | Status |
|---------|:------------:|:------:|
| - | - | - |

## Links

- [Pattern Parsing Fundamental Fix](feature-621.md)
- [Matcher Enhancement](feature-626.md)
- [Character Data Service (trigger)](feature-628.md)
- [MANUAL Status Counting (related investigation)](feature-618.md)
- [Post-Phase Review (related)](feature-625.md)

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Reliable AC verification" | Verification must produce consistent, correct results for all patterns | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#9, AC#10 |
| "correctly handle all valid patterns" | `$` character patterns must work; glob patterns in paths must expand | AC#1, AC#2, AC#3, AC#5, AC#6, AC#7, AC#10 |
| "without manual workarounds" | Tool must automate verification without user intervention | AC#4, AC#8, AC#9 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | `$` character in pattern correctly matched | test | pytest(tools/tests/test_ac_verifier_dollar.py) | succeeds | - | [x] |
| 2 | Glob pattern in file path expands | test | pytest(tools/tests/test_ac_verifier_glob_content.py) | succeeds | - | [x] |
| 3 | Multiple glob matches searched | test | pytest(tools/tests/test_ac_verifier_glob_content.py) | succeeds | - | [x] |
| 4 | Existing tests still pass | test | pytest(tools/tests/test_ac_verifier*.py) | succeeds | - | [x] |
| 5 | Python native search primary for contains matcher | code | Grep(tools/ac-static-verifier.py) | not_contains | "subprocess.run(['grep'" | [x] |
| 6 | Glob expansion in verify_code_ac | code | Grep(tools/ac-static-verifier.py) | contains | "glob_module.glob" | [x] |
| 7 | Glob expansion in _verify_file_content | code | Grep(tools/ac-static-verifier.py) | contains | "glob_module.glob" | [x] |
| 8 | Backward compatibility: literal paths still work | test | pytest(tools/tests/test_ac_verifier_glob_content.py) | succeeds | - | [x] |
| 9 | JSON output schema preserved | code | Grep(tools/ac-static-verifier.py) | contains | "\"summary\":" | [x] |
| 10 | Python native search primary in _verify_file_content | code | Grep(tools/ac-static-verifier.py) | not_contains | "subprocess.run(['grep'" | [x] |

**Note**: 10 ACs within typical infra range (8-15).

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Implement two independent fixes to address the root causes identified:

**Fix 1: Use Python native search for all `contains` matcher patterns**
- Replace subprocess-based grep/rg calls with Python's native `pattern in content` string search for `contains` matcher
- This eliminates Windows command-line argument handling issues with special characters like `$`
- Python native search is inherently literal (no regex interpretation) and handles all special characters correctly
- Maintains `-F` (fixed string) semantics expected by `contains` matcher

**Fix 2: Add glob expansion for file paths in content verification**
- Apply glob expansion logic (similar to verify_file_ac lines 585-591) to both verify_code_ac() and _verify_file_content()
- Expand glob patterns before file existence check
- When multiple files match, search all files and PASS if pattern found in ANY file
- Preserve backward compatibility by only expanding when glob characters (`*`, `?`, `[`) are present AND path doesn't exist as literal

**Rationale**:
- **Python native > subprocess**: Avoids platform-specific escaping issues, simpler code path, already exists as fallback (line 246-248)
- **Glob expansion reuse**: verify_file_ac already has working implementation (lines 585-591), apply same logic to content verification
- **Backward compatibility**: Explicit glob detection ensures literal paths continue working
- **Minimal change**: Both fixes are surgical modifications to existing functions without breaking API or output schema

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Python native search `pattern in content` correctly handles `$` without subprocess escaping issues - test with fixture containing `$"Character{id}"` |
| 2 | Glob expansion with `glob.glob(str(target_path))` before file existence check in verify_code_ac() - test with `Era.Core/Characters/*.cs` style paths |
| 3 | Multi-file search: iterate all glob matches, PASS if pattern found in any - test with multiple matching files |
| 4 | Regression protection: Execute existing tests to verify no breaking changes. No explicit implementation action required - validation task only. |
| 5 | Implementation verification: Grep for `pattern in content` in verify_code_ac() and _verify_file_content() confirms Python native search |
| 6 | Implementation verification: Grep for `glob.glob` in verify_code_ac() confirms glob expansion added |
| 7 | Implementation verification: Grep for `glob.glob` in _verify_file_content() confirms glob expansion added |
| 8 | Backward compatibility: Glob expansion only when `has_glob_pattern = any(c in file_path for c in ['*', '?', '['])` is True (no literal existence check) |
| 9 | Output schema preservation: No changes to result dict structure or summary fields - JSON schema remains identical |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| How to fix `$` issue | A) Fix subprocess escaping; B) Use Python native search; C) Use shell=True | B) Python native | Subprocess escaping is platform-dependent and complex. Python native is simple, reliable, already exists as fallback. shell=True has security implications. |
| Where to apply Python native | A) Only for patterns with `$`; B) All `contains` matcher; C) All matchers | B) All `contains` matcher | Simplifies code (no special cases), eliminates all subprocess issues for literal search, maintains `-F` semantics. `matches` matcher still uses subprocess for regex support. |
| Glob expansion approach | A) Rewrite glob logic; B) Extract to shared function; C) Duplicate verify_file_ac logic | B) Extract to shared function | Zero Debt Upfront: 将来のglob展開変更が1箇所で済む。共通関数`_expand_glob_path()`を作成。 |
| Multi-file search behavior | A) Search only first match; B) Search all matches; C) Error on multiple matches | B) Search all matches | Most permissive, aligns with grep's recursive behavior, reduces false negatives. Document in AC#3. |
| Backward compatibility check | A) Always expand glob; B) Expand only if glob chars present; C) Expand only if literal path doesn't exist | B) Expand if glob chars present | Safe: literal paths without `*?[` are never expanded. Option C (check literal existence first) adds unnecessary file I/O. |

### Implementation Details

**verify_code_ac() changes**:
1. Add glob expansion after `target_file = self.repo_root / file_path` assignment and before existence check (similar to verify_file_ac glob logic)
2. Replace subprocess calls with Python native search for `contains` matcher:
   ```python
   if matcher == "contains":
       with open(target_file, 'r', encoding='utf-8') as f:
           content = f.read()
           pattern_found = pattern in content
   ```
3. Keep subprocess logic for `matches` matcher (uses regex, not literal)

**_verify_file_content() changes**:
1. Add glob expansion after `target_file = self.repo_root / file_path` assignment and before existence check (identical to verify_code_ac)
2. Replace subprocess calls with Python native search for `contains` matcher (identical to verify_code_ac)

**Glob expansion pattern** (reused from verify_file_ac):
```python
has_glob_pattern = any(c in file_path for c in ['*', '?', '['])
if has_glob_pattern:
    import glob as glob_module
    matches = list(glob_module.glob(str(target_file), recursive=True))
    if not matches:
        return {"result": "FAIL", "error": f"No files match glob pattern: {file_path}"}
    # Search all matched files
else:
    # Direct path check (existing logic)
```

**Edge cases handled**:
- No glob matches: Return FAIL with descriptive error (not "File not found")
- Multiple glob matches: Search all files, PASS if any contains pattern
- Literal paths with special chars: glob expansion only if `has_glob_pattern` is True
- Encoding errors: Python native search uses `encoding='utf-8'`, consistent with existing fallback

### AC Details

**AC#1: `$` character in pattern correctly matched**
- Test pattern `$"Character{id}"` (C# interpolated string syntax) against test fixture
- Verifies fix for Root Cause 1: `$` passed correctly to search mechanism
- Uses pytest fixture with known content containing `$` patterns
- Edge case: `$` at end of pattern (regex anchor position) must also work

**AC#2: Glob pattern in file path expands**
- Test `Grep(Era.Core/Characters/*.cs)` style patterns
- Verifies glob expansion applied before file existence check
- Expected: Files matching `*.cs` are searched for pattern
- Edge case: No matches should return appropriate error, not "File not found"

**AC#3: Multiple glob matches searched**
- When glob expands to multiple files, search all files
- PASS if pattern found in ANY matched file
- Verifies multi-file search behavior documented in Risks section

**AC#4: Existing tests still pass**
- Regression protection for F621, F626 fixes
- All existing test_ac_verifier_*.py tests must continue passing
- Ensures backward compatibility with existing AC verification

**AC#5: Python native search used for $ patterns**
- Implementation uses `pattern in content` (Python native) for reliability
- Avoids subprocess `$` escaping issues on Windows
- Verification: Grep for `pattern in content` in ac-static-verifier.py

**AC#6: Glob expansion in verify_code_ac**
- verify_code_ac() function must call glob.glob() for path expansion
- Verifies fix for Root Cause 2 in code type verification
- Path: `tools/ac-static-verifier.py` verify_code_ac function

**AC#7: Glob expansion in _verify_file_content**
- _verify_file_content() function must call glob.glob() for path expansion
- Verifies fix for Root Cause 2 in file content verification
- Both functions need glob expansion for comprehensive fix

**AC#8: Backward compatibility: literal paths still work**
- Non-glob paths (e.g., `Era.Core/KojoEngine.cs`) must continue working
- Glob expansion only when `*`, `?`, or `[` present (no literal path existence check per Key Decisions)
- Risk mitigation: Explicit glob detection per Risks table

**AC#9: JSON output schema preserved**
- Output JSON structure must maintain `summary.total/passed/manual/failed` fields
- Technical Constraint from F621, verify-logs.py dependency
- Prevents breaking dependent tooling

**AC#10: Python native search used in _verify_file_content**
- Implementation uses `pattern in content` (Python native) for reliability in _verify_file_content()
- Avoids subprocess `$` escaping issues on Windows for file content verification
- Verification: Grep for `pattern in content` in _verify_file_content() function
- Complements AC#5 (verify_code_ac) to ensure both functions use consistent approach

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 5 | Replace subprocess grep with Python native search for `contains` matcher in verify_code_ac() | [x] |
| 2 | 10 | Replace subprocess grep with Python native search for `contains` matcher in _verify_file_content() | [x] |
| 3 | 6 | Add glob expansion logic to verify_code_ac() before file existence check | [x] |
| 4 | 7 | Add glob expansion logic to _verify_file_content() before file existence check | [x] |
| 5 | 9 | Verify JSON output schema preserved by checking result dict structure contains required fields | [x] |
| 6 | 1 | Create test_ac_verifier_dollar.py with `$` character test cases | [x] |
| 7 | 2,3,8 | Create test_ac_verifier_glob_content.py with glob expansion tests | [x] |
| 8 | 4 | Run all existing test_ac_verifier*.py tests to verify backward compatibility | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks. AC#7 maps to Task#4 (glob expansion in _verify_file_content). AC#10 maps to Task#2 (Python native search in _verify_file_content). AC#2,3,8 grouped in Task#7 as single test file covers all three scenarios. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-4 | Modified ac-static-verifier.py |
| 2 | implementer | sonnet | Tasks 6-7 | New test files |
| 3 | ac-tester | haiku | Tasks 5, 8 | JSON schema verification + Test results |

**Constraints** (from Technical Design):
1. Python native search only for `contains` matcher (not `matches` - preserves regex support)
2. Glob expansion only when `*`, `?`, `[` present in path (backward compatibility)
3. JSON output schema must remain identical (verify-logs.py dependency)
4. Multi-file glob matches: Search ALL files, PASS if pattern found in ANY
5. No changes to `matches` matcher subprocess logic (uses regex, requires subprocess)

**Pre-conditions**:
- F621 and F626 fixes are in place (pattern parsing, Method column support)
- Python glob module available (standard library)
- All existing test_ac_verifier*.py tests passing

**Execution Steps**:
1. **Task 1**: Modify verify_code_ac() subprocess calls
   - Replace subprocess grep call with Python native search for `contains` matcher
   - Pattern: `with open(target_file, 'r', encoding='utf-8') as f: content = f.read(); pattern_found = pattern in content`
   - Keep subprocess logic for `matches` matcher (regex support)

2. **Task 2**: Modify _verify_file_content() subprocess calls
   - Apply identical Python native search change for `contains` matcher
   - Maintain consistency with verify_code_ac() implementation

3. **Task 3**: Add glob expansion to verify_code_ac()
   - Copy glob expansion logic from verify_file_ac
   - Insert after `target_file = self.repo_root / file_path` and before existence check
   - Check `has_glob_pattern = any(c in file_path for c in ['*', '?', '['])`
   - If True: expand with `glob.glob(str(target_path), recursive=True)`
   - If no matches: return FAIL with "No files match glob pattern" error
   - If multiple matches: search all files, PASS if pattern found in ANY

4. **Task 4**: Add glob expansion to _verify_file_content()
   - Apply identical glob expansion logic from Task 3
   - Insert after `target_file = self.repo_root / file_path` and before existence check
   - Maintain consistency with verify_code_ac() implementation

5. **Task 5**: Verify JSON schema preservation
   - Check result dict structure in modified functions
   - Ensure `summary.total`, `summary.passed`, `summary.manual`, `summary.failed` fields unchanged
   - No new fields added to output schema

6. **Task 6**: Create test_ac_verifier_dollar.py
   - Test case 1: Pattern with `$` at end (e.g., `$"string"`)
   - Test case 2: Pattern with `$` in middle (e.g., `$"var{id}"`)
   - Test case 3: Multiple `$` patterns in same file
   - Use pytest fixtures with known content containing `$` patterns

7. **Task 7**: Create test_ac_verifier_glob_content.py
   - Test case 1: Single glob match with pattern found (AC#2)
   - Test case 2: Multiple glob matches with pattern in one file (AC#3)
   - Test case 3: Literal path without glob characters still works (AC#8)
   - Test case 4: Glob pattern with no matches returns appropriate error
   - Use temporary directory with test files matching glob patterns

8. **Task 8**: Run regression tests
   - Execute: `pytest tools/tests/test_ac_verifier*.py`
   - Verify all existing tests pass (F621, F626 fixes preserved)
   - Expected: 0 failures

**Success Criteria**:
- All 10 ACs pass verification
- `pytest tools/tests/test_ac_verifier*.py` returns 0 failures
- `$` patterns (e.g., `$"Character{id}"`) correctly matched
- Glob patterns (e.g., `Era.Core/Characters/*.cs`) expand and search multiple files
- JSON output schema unchanged (verify-logs.py continues working)
- Literal paths without glob characters continue working

**Error Handling**:
- If Task 8 fails: Review changed code against Technical Design constraints
- If new tests fail: Check test fixture setup and expected values
- If glob expansion breaks existing behavior: Verify `has_glob_pattern` check is correct
- After 3 consecutive failures: STOP and report to user

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback and issue details
3. Create follow-up feature to address root cause
4. Document rollback reason in Execution Log

**Rollback Conditions**:
- verify-logs.py fails due to JSON schema change
- Existing ACs in other features fail due to subprocess behavior change
- Performance degradation with glob expansion (e.g., `**/` recursive patterns)

**Recovery Steps**:
1. Restore subprocess logic for `contains` matcher (revert Tasks 1-2)
2. Remove glob expansion from verify_code_ac() and _verify_file_content() (revert Tasks 3-4)
3. Keep new test files but mark as expected to fail until re-implementation
4. Update F630 status to [BLOCKED] with rollback documentation

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: F628 AC#8 `$"Character{characterId.Value}"` pattern causes FAIL when pattern exists in code
2. Why: `_contains_regex_metacharacters()` function (line 98) detects `$` at end of string as regex anchor via pattern `r'\$$'`
3. Why: The detection logic checks if pattern ends with `$` (`\$$` = end of string), not if `$` appears inside the pattern
4. Why: The original design (F621) focused on unambiguous regex patterns like `^start` and `end$`, assuming literal `$` would not appear in Expected values
5. Why: C# interpolated string syntax `$"..."` was not considered when designing the regex metacharacter detection

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `$"Character{characterId.Value}"` pattern returns FAIL | `_contains_regex_metacharacters()` line 98 `r'\$$'` falsely triggers on patterns containing `$` followed by `"` because `"` is stripped during parsing |
| `Grep(Era.Core/Characters/*.cs)` returns "File not found" | `_verify_file_content()` and `verify_code_ac()` extract file path from Method column but check `target_file.exists()` without glob expansion - glob patterns treated as literal paths |

### Conclusion

**Two distinct root causes confirmed by manual verification:**

1. **`$` character handling**: CONFIRMED as real bug. When `subprocess.run(['grep', '-F', '-c', pattern, file], shell=False)` is called on Windows, patterns containing `$` are not passed correctly to grep. The `-F` (fixed string) mode should prevent regex interpretation, but Windows command-line argument handling corrupts the `$` character before grep receives it.

   **Evidence**: `grep -F -c '$"Character{characterId.Value}"' file.cs` succeeds in bash (rc=0, count=1), but fails via Python subprocess with `shell=False` (rc=1, count=0). Using `shell=True` or Python native search (`pattern in content`) works correctly.

2. **Glob pattern in file paths**: CONFIRMED. When Method column contains `Grep(Era.Core/Characters/*.cs)`, the code extracts `Era.Core/Characters/*.cs` and checks `(self.repo_root / file_path).exists()` (lines 204-206 in verify_code_ac, lines 418-421 in _verify_file_content). This checks if a literal file named `Era.Core/Characters/*.cs` exists, which it does not. Glob expansion is only applied in `verify_file_ac()` for `exists` matcher (lines 585-591), not for content search paths.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F621 | [DONE] | Predecessor | Pattern Parsing Fundamental Fix - established `_contains_regex_metacharacters()` function |
| F626 | [DONE] | Predecessor | Matcher Enhancement - added `Glob(pattern)` support for `exists` matcher in Method column |
| F628 | [DONE] | Trigger | Character Data Service - revealed AC verification limitations with `$` and glob patterns |
| F618 | [CANCELLED] | Related investigation | MANUAL Status Counting - investigated similar ac-static-verifier issue |
| F625 | [DONE] | Related | Post-Phase Review - also revealed ac-static-verifier Method column issues |

### Pattern Analysis

**Recurring Pattern**: ac-static-verifier has been modified multiple times (F618, F621, F626) as new AC definition patterns emerge that the tool doesn't handle:
- F621: Method format `Grep(path)` support, escape sequence handling
- F626: `Glob(pattern)` in Method for `exists` matcher, Method column for `build` matcher
- F630: Glob expansion for content search paths (code/file types with `contains`/`matches`)

**Prevention Strategy**: When adding new AC patterns, verify ac-static-verifier handles them before feature completion.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | **YES** | Both issues have clear fixes: Python native search for `$`, glob expansion for paths |
| Scope is realistic | **YES** | Two well-defined bugs with straightforward solutions |
| No blocking constraints | **YES** | F626 completed, builds on existing codebase |

**Verdict**: FEASIBLE

**Verification Results** (2025-01-26):
- `$` issue: CONFIRMED - `subprocess.run(['grep', '-F', pattern], shell=False)` on Windows fails to pass `$` correctly to grep
- Glob issue: CONFIRMED - `Era.Core/Characters/*.cs` treated as literal path, returns "File not found"

**Fix Strategy**:
1. `$` issue: Use Python native `pattern in content` search (already exists as fallback, line 246-248)
2. Glob issue: Apply glob expansion before file existence check in `verify_code_ac()` and `_verify_file_content()`

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F621 | [DONE] | Pattern Parsing Fundamental Fix - base pattern handling |
| Predecessor | F626 | [DONE] | Matcher Enhancement - `Glob(pattern)` support for exists matcher |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Python glob module | Runtime | None | Standard library, already imported in verify_file_ac |
| ripgrep (rg) | Optional | Low | Fallback to grep/Python exists |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `.claude/skills/run-workflow/PHASE-6.md` | CRITICAL | AC verification core tool |
| `tools/verify-logs.py` | HIGH | JSON output schema dependency |
| `tools/tests/test_ac_verifier_*.py` | MEDIUM | Regression tests must continue passing |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `tools/ac-static-verifier.py` | Update | Add glob expansion for content search paths in verify_code_ac() and _verify_file_content() |
| `tools/tests/test_ac_verifier_glob_content.py` | Create | Test cases for glob pattern expansion in Grep(path/*.ext) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| JSON schema stability | F621, verify-logs.py | HIGH - `summary.total/passed/manual/failed` must not change |
| Exit code semantics | PHASE-6.md | HIGH - 0=pass, 1=fail must be preserved |
| Backward compatibility | Existing ACs | HIGH - Non-glob paths must continue working |
| F621/F626 preservation | Previous fixes | MEDIUM - Do not break existing pattern handling |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Glob expansion returns multiple files unexpectedly | Medium | Medium | Document behavior: search all matched files, PASS if pattern found in ANY |
| Performance degradation with many glob matches | Low | Low | Glob patterns are typically narrow (specific directory + extension) |
| Existing ACs with literal `*` in path break | Very Low | High | Explicit glob detection: only expand if `*`, `?`, `[` present AND path does not exist as literal |
| `$` issue is real but different location | Medium | Low | Detailed verification step before implementation |

---

## Review Notes

- [resolved-applied] Phase1 iter3: AC#10 not mapped in Philosophy Derivation table - uncertain if implementation verification ACs need Philosophy mapping
- [resolved-invalid] Phase1 iter3: AC#8 mapping redundancy - 現状(AC#8→Task#7)が1:1原則に適合。Task#8追加は原則違反
- [resolved-skipped] Phase1 iter3: Pre-conditions verification - エントリ条件として信頼。Task#8は変更後の検証用
- [resolved-applied] Phase1 iter4: Review Notes mandatory化 - 全[pending]解決まで/run不可に変更
- [resolved-applied] Phase2 iter5: Code duplication in glob logic - 共通関数`_expand_glob_path()`に抽出する設計に変更
- [resolved-applied] Phase2 iter5: Function duplication - 共通関数`_search_pattern_in_file()`に抽出する設計に変更
- [resolved-applied] Phase2 iter6: Task#5 verification assignment - Phase 3 (ac-tester)に移動
- [resolved-applied] Phase2 iter6: AC#9 regex pattern fragility - `"summary":`のcontainsチェックに変更（順序非依存）
- [resolved-invalid] Phase2 iter7: Function extraction opportunity - Issue 5と重複、既に解決済み
- [resolved-applied] Phase1 iter8: AC#5/AC#10 verify primary method not fallback - changed to verify subprocess removal
- [resolved-applied] Phase2 iter10: Review Notes leak prevention - Issue 3でmandatory化、全pending解決済み

## Handoff

| Item | Destination | Notes |
|------|-------------|-------|
| - | - | - |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-26 20:17 | START | implementer | Tasks 1-4 | - |
| 2026-01-26 20:17 | END | implementer | Tasks 1-4 | SUCCESS |
| 2026-01-26 20:20 | START | implementer | Tasks 6-7 | - |
| 2026-01-26 20:20 | END | implementer | Tasks 6-7 | SUCCESS |
| 2026-01-26 | DEVIATION | feature-reviewer | post mode | NEEDS_REVISION: Code duplication (major) - glob expansion + Python native search logic duplicated across 3 functions |
| 2026-01-26 20:29 | START | implementer | Refactor: Extract common functions | - |
| 2026-01-26 20:29 | END | implementer | Refactor: Extract common functions | SUCCESS |
| 2026-01-26 | REVIEW | feature-reviewer | post mode (2nd) | OK |
| 2026-01-26 | REVIEW | feature-reviewer | doc-check mode | OK |
| 2026-01-26 | CHECK | orchestrator | SSOT update check | N/A (no new types/interfaces/commands) |
