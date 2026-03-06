# Feature 702: ac-static-verifier Binary File Handling

## Status: [DONE]

## Type: infra

## Created: 2026-01-31

---

## Summary

Fix ac-static-verifier.py to handle binary files gracefully when scanning directories. Currently fails with `'utf-8' codec can't decode byte` error when encountering binary files (e.g., `.cache`, `.dll` in `obj/` directories).

---

## Background

Discovered during F683 execution. When running `python tools/ac-static-verifier.py --feature 683 --ac-type code`, the tool encounters binary files in `obj/` directories and fails with a utf-8 decode error.

**Initial hypothesis** (incorrect): The error was attributed to Japanese filenames in `Game/YAML/Kojo/`.

**Actual cause**: The error occurs when reading binary files in `Era.Core/obj/` directories as UTF-8 text.

The tool should:
1. Skip binary files during content search (filter by extension or detect binary content)
2. Use error handling to skip unreadable files gracefully with warnings

---

## Links

- [feature-683.md](feature-683.md) - Discovered during F683 execution (DEVIATION)
- [feature-699.md](feature-699.md) - Added directory path support (rglob recursion) - directly related
- [feature-630.md](feature-630.md) - Prior glob pattern support

---

## Notes

- PRE-EXISTING issue - existed since F699 added directory path support with `rglob('*')`
- Impact: Cannot use ac-static-verifier for automated AC verification when scanning directories that contain binary files
- Workaround: Manual verification (used in F683)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: ac-static-verifier fails with `'utf-8' codec can't decode byte 0xa4 in position 13` when running `--feature 683 --ac-type code`
2. Why: The `_search_pattern_native` method calls `open(file_path, 'r', encoding='utf-8')` on binary files
3. Why: The `_expand_glob_path` method returns ALL files in a directory (including binary files) when using `rglob('*')`
4. Why: F699 added directory path support with `rglob('*')` without filtering out binary files
5. Why: The original design assumed all scanned files would be text files; binary file handling was not considered

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `'utf-8' codec can't decode byte` error during AC verification | `_search_pattern_native` attempts to read binary files (e.g., `Era.Core/obj/Release/net10.0/Era.Core.assets.cache`) as UTF-8 text |
| Error initially attributed to Japanese filenames | Files in `Game/YAML/Kojo/` with Japanese names actually read correctly; the error came from binary `.cache` files in `obj/` directories |
| AC verification fails with exit code 1 | No filtering for text-only files when recursively enumerating directory contents |

### Conclusion

The root cause is **missing binary file filtering** in the directory enumeration logic added by F699. When `_expand_glob_path` uses `target_file.rglob('*')` for directory paths, it returns all files including:
- Compiled binaries (`.dll`, `.exe`, `.pdb`)
- Cache files (`.cache`, `.assets.cache`)
- NuGet artifacts in `obj/` directories

The specific failing file is `Era.Core/obj/Release/net10.0/Era.Core.assets.cache` which is a binary file starting with `PKGA` header.

**Important correction**: The original feature title "Japanese Filename Support" was based on a misdiagnosis. Japanese-named YAML files in `Game/YAML/Kojo/` read correctly as UTF-8. The issue is entirely about binary file content, not filename encoding.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F699 | [DONE] | Root cause introducer | Added `rglob('*')` directory enumeration without binary file filtering |
| F683 | [DONE] | Discovery source | AC#3 `Grep(Era.Core/)` triggered the error during verification |
| F630 | [DONE] | Prior glob support | Added glob pattern expansion but did not address binary files (predates directory support) |

### Pattern Analysis

This is the same pattern as F699's original issue (directory vs file distinction) but at a different level:
- F699 fixed: Directory paths treated as files (PermissionError)
- F702 fixes: Binary files treated as text files (UnicodeDecodeError)

Both issues stem from the same root: insufficient path/file type filtering in `_expand_glob_path`.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Standard approaches: file extension filtering (skip `.cache`, `.dll`, etc.) or binary content detection (check for null bytes or magic headers) |
| Scope is realistic | YES | Single method modification: either filter in `_expand_glob_path` by extension, or add `try/except` in `_search_pattern_native` with skip logic |
| No blocking constraints | YES | No external dependencies; pure Python implementation |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F699 | [DONE] | Introduced the directory enumeration with `rglob('*')` |
| Related | F683 | [DONE] | Discovery source; manually verified ACs due to this bug |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Python pathlib | Runtime | None | Standard library, already used |
| Python mimetypes | Runtime | None | Standard library, could use for extension-based filtering |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| All features using `Grep(directory/)` in AC table where directory contains binary files | HIGH | AC verification will work correctly after fix |
| F683 (retroactively) | NONE | Already completed with manual workaround |
| Future C# project AC verification | HIGH | Era.Core, engine, tools all have `obj/` directories with binary files |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ac-static-verifier.py | Update | Add binary file filtering in `_expand_glob_path` or error handling in `_search_pattern_native` |
| tools/tests/test_ac_verifier_binary.py | Create | New test file for binary file handling |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Backward compatibility | Existing ACs | MEDIUM - Must not break existing file path or glob pattern handling |
| Performance | Large directories with many files | LOW - Extension-based filtering is O(1) per file; minimal overhead |
| False positives | Text files with unusual extensions | LOW - Use common binary extension list; text files will still work |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Breaking existing tests | Low | High | Run all existing ac-verifier tests before and after change |
| Missing binary extension in filter list | Medium | Low | Use comprehensive list; add fallback try/except for edge cases |
| False positive skip (text file with binary extension) | Low | Medium | Log skipped files for debugging; consider content-based detection as fallback |
| Performance on large binary-heavy directories | Low | Low | Extension filtering is cheap; no content read for skipped files |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "gracefully handle binary files" | Tool must not crash on binary files in directory scan | AC#1, AC#2 |
| "skip binary files during content search" | Binary files filtered out before UTF-8 read | AC#3, AC#4 |
| "error handling to skip unreadable files" | UnicodeDecodeError caught and logged | AC#5 |
| "gracefully with warnings" | Skipped files reported in output | AC#6 |
| "must not break existing file path or glob pattern handling" | Backward compatibility preserved | AC#7, AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Era.Core directory scan succeeds | exit_code | pytest | succeeds | tools/tests/test_ac_verifier_binary.py::test_eracore_directory_no_crash | [x] |
| 2 | obj directory with binaries handled | exit_code | pytest | succeeds | tools/tests/test_ac_verifier_binary.py::test_obj_directory_binaries_skipped | [x] |
| 3 | Binary extension filtering exists | code | Grep(tools/ac-static-verifier.py) | contains | "BINARY_EXTENSIONS" | [x] |
| 4 | Binary files excluded from file list | exit_code | pytest | succeeds | tools/tests/test_ac_verifier_binary.py::test_binary_files_filtered_by_extension | [x] |
| 5 | UnicodeDecodeError handled gracefully | code | Grep(tools/ac-static-verifier.py) | contains | "UnicodeDecodeError" | [x] |
| 6 | Skipped files logged to stderr | exit_code | pytest | succeeds | tools/tests/test_ac_verifier_binary.py::test_skipped_files_warning_output | [x] |
| 7 | Existing directory tests pass | exit_code | pytest | succeeds | tools/tests/test_ac_verifier_directory.py | [x] |
| 8 | Existing glob patterns work | exit_code | pytest | succeeds | tools/tests/test_ac_verifier_method_glob.py | [x] |
| 9 | No technical debt markers | code | Grep(tools/ac-static-verifier.py) | not_contains | "TODO|FIXME|HACK" | [x] |

**Note**: 9 ACs is within the typical infra range (8-15).

### AC Details

**AC#1: Era.Core directory scan succeeds**
- Test: Run verifier with `Grep(Era.Core/)` path (the actual failing case from F683)
- Expected: Exit code 0, no UnicodeDecodeError
- Coverage: Validates the original bug is fixed

**AC#2: obj directory with binaries handled**
- Test: Create temp directory structure with obj/ containing .dll, .cache, .pdb files
- Expected: Verifier processes directory without crash
- Coverage: Validates binary-heavy build output directories work

**AC#3: Binary extension filtering exists**
- Test: Grep for BINARY_EXTENSIONS constant in source
- Expected: A set/list of binary extensions (.dll, .exe, .cache, .pdb, etc.) defined
- Coverage: Validates design choice of extension-based filtering implemented

**AC#4: Binary files excluded from file list**
- Test: Create directory with mixed text and binary files, verify _expand_glob_path excludes binaries
- Expected: Only text files returned in expanded file list
- Coverage: Validates filtering happens at correct layer

**AC#5: UnicodeDecodeError handled gracefully**
- Test: Grep for exception handler in source
- Expected: try/except block catches UnicodeDecodeError as fallback
- Coverage: Defense in depth - even if extension filtering misses a file, error is handled

**AC#6: Skipped files logged to stderr**
- Test: Run verifier on directory with binary files, capture stderr
- Expected: Warning message indicates files were skipped (e.g., "Skipping binary file: X")
- Coverage: Validates graceful warning output per Background requirement

**AC#7: Existing directory tests pass**
- Test: Run all tests in test_ac_verifier_directory.py
- Expected: All 4 existing tests pass (no regression)
- Coverage: Backward compatibility for directory path handling

**AC#8: Existing glob patterns work**
- Test: Run all tests in test_ac_verifier_method_glob.py
- Expected: All existing glob pattern tests pass (no regression)
- Coverage: Backward compatibility for glob pattern handling

**AC#9: No technical debt markers**
- Test: Grep for TODO|FIXME|HACK in modified file
- Pattern: `TODO|FIXME|HACK`
- Expected: Pattern not found (no new debt introduced)
- Coverage: Code quality

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Two-layer defense**: Extension-based filtering (primary) + exception handling (fallback).

**Layer 1: Extension-based filtering in `_expand_glob_path`**
- Define `BINARY_EXTENSIONS` constant at module level with comprehensive list of binary file extensions
- Filter binary files BEFORE they reach `_search_pattern_native` (early exit strategy)
- Only impacts directory enumeration path (`rglob('*')` branch) - no change to glob patterns or literal paths
- Logging to stderr for skipped binary files (visibility into filtering behavior)

**Layer 2: Exception handling in `_search_pattern_native` and `_verify_content`**
- Add `try/except UnicodeDecodeError` around file read operations
- If binary file passes through Layer 1 (edge case: unusual extension), catch decode error gracefully
- Log warning and skip file instead of crashing
- Defense-in-depth: handles future binary file types not in BINARY_EXTENSIONS list

**Why this approach satisfies the ACs:**
- AC#1-2 (integration tests): Two-layer defense ensures no crashes on binary-heavy directories
- AC#3-4 (extension filtering): Layer 1 explicitly implements BINARY_EXTENSIONS and filters in `_expand_glob_path`
- AC#5-6 (error handling): Layer 2 explicitly catches UnicodeDecodeError and logs warnings
- AC#7-8 (regression): Extension filtering only impacts directory path branch; glob patterns and literal paths unchanged
- AC#9 (no debt): Clean implementation with no temporary workarounds

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Integration test: Call verifier with actual `Era.Core/` directory path (contains `obj/` with binary files). Two-layer defense prevents crash. Verify exit code 0. |
| 2 | Unit test: Create temp directory with `obj/` subdirectory containing `.dll`, `.cache`, `.pdb` files. Verify `_expand_glob_path` returns only text files and tool exits successfully. |
| 3 | Code verification: Define `BINARY_EXTENSIONS` set at module level (after imports, before classes). Include extensions: `.dll`, `.exe`, `.pdb`, `.cache`, `.obj`, `.so`, `.dylib`, `.bin`, `.dat`, `.pack`, `.idx`, `.a`, `.lib`, `.exp`, `.ilk`, `.assets.cache` |
| 4 | Unit test: Create temp directory with mixed `.py` (text) and `.dll` (binary) files. Call `_expand_glob_path` on directory. Assert returned list contains only `.py` files, excludes `.dll` files. |
| 5 | Code verification: Grep for `except UnicodeDecodeError` in `_search_pattern_native`. Add try/except around `open()` and `f.read()` calls. Log warning to stderr with filename. Continue to next file (skip, don't fail). Also add same handler to `_verify_content` for `matches` matcher path. |
| 6 | Unit test: Create directory with binary file (`.dll` with binary content). Run verifier, capture stderr. Assert stderr contains warning message like `WARNING: Skipping binary file: {filename}` or `WARNING: Failed to read {filename} (binary file?)`. |
| 7 | Regression test: Run `pytest tools/tests/test_ac_verifier_directory.py`. All 4 existing tests must pass without modification. Extension filtering only affects binary files, not the `.py`/`.txt` files used in existing tests. |
| 8 | Regression test: Run `pytest tools/tests/test_ac_verifier_method_glob.py`. Glob pattern expansion in `_expand_glob_path` unchanged for glob patterns (only directory path branch modified). |
| 9 | Static verification: Grep source file for `TODO\|FIXME\|HACK` pattern. Verify no matches. Implementation should be production-quality with no deferred decisions. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Filtering location** | A) Filter in `_expand_glob_path` (early), B) Filter in `_search_pattern_native` (late), C) Filter in both | **A + exception handling in B** | Early filtering prevents unnecessary processing. Exception handling in B provides defense-in-depth for edge cases. |
| **Binary detection method** | A) Extension-based (`.dll`, `.exe`), B) Content-based (magic headers, null bytes), C) Both | **A (extension) + exception fallback** | Extension filtering is fast (O(1) per file, no disk I/O). Content detection requires reading bytes (slower). Exception handling catches missed cases. |
| **Extension list scope** | A) Minimal (only `.cache`, `.dll`), B) Comprehensive (all common binary types), C) Configurable | **B (comprehensive)** | Prevent future issues across different build systems (C#, Python, Unity). Include: compiled binaries, debug symbols, cache files, static libraries, NuGet artifacts. |
| **Error handling strategy** | A) Skip silently, B) Log warning and skip, C) Fail fast | **B (log warning and skip)** | AC#6 explicitly requires warning output. Silent skip hides issues. Fail fast defeats the purpose of graceful handling. |
| **Backward compatibility** | A) Change all paths, B) Only modify directory enumeration | **B (only directory branch)** | AC#7-8 require regression tests to pass. Glob patterns and literal file paths work correctly. Only directory enumeration (`rglob('*')`) introduced the bug. Minimize change surface. |

### Implementation Details

#### Module-level Constant

```python
# Binary file extensions to skip during directory enumeration
# These files cannot be read as UTF-8 text and would cause UnicodeDecodeError
BINARY_EXTENSIONS = {
    # Compiled binaries and executables
    '.dll', '.exe', '.so', '.dylib', '.a', '.lib',
    # Debug and symbol files
    '.pdb', '.exp', '.ilk',
    # Build artifacts and cache files
    '.cache', '.assets.cache', '.obj', '.bin', '.dat',
    # Package and archive internals
    '.pack', '.idx',
}
```

**Location**: After imports (line ~30), before `PatternType` enum definition.

#### Layer 1: Filter in `_expand_glob_path`

**Current code** (lines 134-138):
```python
# NEW: Directory detection and recursive enumeration
if target_file.is_dir():
    # Recursively enumerate all files in directory
    all_files = [f for f in target_file.rglob('*') if f.is_file()]
    return True, None, all_files
```

**Modified code**:
```python
# NEW: Directory detection and recursive enumeration
if target_file.is_dir():
    # Recursively enumerate all files in directory, excluding binary files
    all_files = []
    skipped_count = 0
    for f in target_file.rglob('*'):
        if f.is_file():
            # Skip binary files by extension
            if f.suffix.lower() in BINARY_EXTENSIONS:
                skipped_count += 1
                continue
            all_files.append(f)

    # Log if binary files were skipped
    if skipped_count > 0:
        print(f"INFO: Skipped {skipped_count} binary file(s) in {file_path}", file=sys.stderr)

    return True, None, all_files
```

**Edge case handling**: `f.suffix` returns empty string for files without extension (e.g., `Makefile`). Empty string not in `BINARY_EXTENSIONS`, so extensionless files treated as text (safe default for repository files).

#### Layer 2: Exception handling in `_search_pattern_native`

**Current code** (lines 160-166):
```python
for file_path in files:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
        if pattern in content:
            pattern_found = True
            matched_files.append(str(file_path.relative_to(self.repo_root)))
```

**Modified code**:
```python
for file_path in files:
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            if pattern in content:
                pattern_found = True
                matched_files.append(str(file_path.relative_to(self.repo_root)))
    except UnicodeDecodeError:
        # Binary file not caught by extension filter (unusual extension)
        print(f"WARNING: Skipping binary file: {file_path}", file=sys.stderr)
        continue
```

#### Layer 2: Exception handling in `_verify_content` (matches matcher)

**Current code** (lines 406-409):
```python
for tf in target_files:
    with open(tf, 'r', encoding='utf-8') as f:
        content = f.read()
        if re.search(pattern, content) is not None:
```

**Modified code** (add try/except wrapper):
```python
for tf in target_files:
    try:
        with open(tf, 'r', encoding='utf-8') as f:
            content = f.read()
            if re.search(pattern, content) is not None:
                pattern_found = True
                matched_files.append(str(tf.relative_to(self.repo_root)))
    except UnicodeDecodeError:
        # Binary file not caught by extension filter
        print(f"WARNING: Skipping binary file: {tf}", file=sys.stderr)
        continue
```

**Note**: Need to verify the complete structure of the `matches` branch to ensure proper indentation and flow control.

### Test Strategy

**New test file**: `tools/tests/test_ac_verifier_binary.py`

Test cases:
1. `test_eracore_directory_no_crash`: Integration test with actual `Era.Core/` directory
2. `test_obj_directory_binaries_skipped`: Temp directory with `obj/` containing `.dll`, `.cache`, `.pdb`
3. `test_binary_files_filtered_by_extension`: Direct unit test of `_expand_glob_path` with mixed files
4. `test_skipped_files_warning_output`: Verify stderr warning messages
5. `test_unicode_error_fallback`: Force UnicodeDecodeError with unusual extension (e.g., `.zzz` binary file)

### Constraints Satisfied

| Constraint | How Satisfied |
|------------|---------------|
| Backward compatibility | Only directory enumeration branch modified. Glob patterns and literal paths unchanged. |
| Performance | Extension check is O(1) string comparison. No file content reading for binary files. |
| False positives | Comprehensive extension list minimizes risk. Exception fallback catches edge cases. |

### Data Structures

**Module-level constant**:
```python
BINARY_EXTENSIONS: set[str]  # Immutable set for O(1) lookup
```

**No changes to existing classes or methods signatures**. Only internal implementation modified.

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 3,5 | Add binary file handling to ac-static-verifier.py (BINARY_EXTENSIONS constant + exception handling in _expand_glob_path and _search_pattern_native) | [x] |
| 2 | 1,2,4,6 | Create test_ac_verifier_binary.py with 5 test cases (Era.Core integration, obj directory, extension filtering, warning output, UnicodeDecodeError fallback) | [x] |
| 3 | 7 | Run existing directory tests (regression verification) | [x] |
| 4 | 8 | Run existing glob tests (regression verification) | [x] |
| 5 | 9 | Verify no technical debt markers in modified file | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Technical Design Layer 1 & 2 specs | Modified ac-static-verifier.py |
| 2 | implementer | sonnet | T2 | Technical Design Test Strategy | New test_ac_verifier_binary.py |
| 3 | ac-tester | haiku | T3-T5 | Test commands from ACs | Test results |

**Constraints** (from Technical Design):

1. **Extension-based filtering (Layer 1)**: Filter binary files in `_expand_glob_path` before they reach file read operations. Only impacts directory enumeration branch (`rglob('*')`), no change to glob patterns or literal paths.
2. **Exception handling (Layer 2)**: Add `try/except UnicodeDecodeError` in both `_search_pattern_native` and `_verify_content` (matches matcher path) as defense-in-depth fallback.
3. **Comprehensive extension list**: Include `.dll`, `.exe`, `.pdb`, `.cache`, `.obj`, `.so`, `.dylib`, `.bin`, `.dat`, `.pack`, `.idx`, `.a`, `.lib`, `.exp`, `.ilk`, `.assets.cache` in BINARY_EXTENSIONS set.
4. **Logging to stderr**: Log skipped binary files with `INFO:` prefix (count summary in `_expand_glob_path`) and `WARNING:` prefix (individual files in exception handler).
5. **Backward compatibility**: Regression tests (AC#7-8) must pass. No changes to glob pattern or literal file path handling logic.

**Pre-conditions**:
- Python environment with pytest installed
- Repository with existing test suite (test_ac_verifier_directory.py, test_ac_verifier_method_glob.py)
- Era.Core/ directory exists with obj/ subdirectory containing binary build artifacts

**Success Criteria**:
- All 5 new test cases in test_ac_verifier_binary.py pass
- All 4 existing tests in test_ac_verifier_directory.py pass (no regression)
- All existing tests in test_ac_verifier_method_glob.py pass (no regression)
- No `TODO|FIXME|HACK` markers in ac-static-verifier.py
- Running ac-static-verifier on Era.Core/ directory succeeds with exit code 0

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation into edge cases

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 | Phase 1 | Initialize: [REVIEWED] → [WIP] |
| 2026-01-31 | Phase 2 | Investigation: Explorer confirmed code structure, line numbers |
| 2026-01-31 | Phase 4 | T1: implementer modified ac-static-verifier.py (BINARY_EXTENSIONS + exception handling) |
| 2026-01-31 | Phase 4 | T2: implementer created test_ac_verifier_binary.py (5 test cases) |
| 2026-01-31 | Phase 4 | T3-T5: ac-tester regression + debt verification - all PASS |
| 2026-01-31 | Phase 6 | AC verification: 9/9 PASS |
| 2026-01-31 | Phase 7 | Post-review: quality OK, doc-check OK, SSOT N/A |
| 2026-01-31 | DEVIATION | Bash | CMD syntax in Git Bash | exit code 2 (shell syntax error, not feature-related) |
