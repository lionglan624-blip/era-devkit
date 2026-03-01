# Feature 699: ac-static-verifier Directory Path Support

## Status: [DONE]

## Type: infra

---

## Background

### Problem (Current Issue)

`ac-static-verifier.py` fails with "Permission denied" when AC Method specifies a directory path (e.g., `Grep(tools/YamlSchemaGen/)`). The tool attempts to open the directory as a file instead of recursively searching within it.

**Discovery**: F697 execution - AC#4 verification failed, requiring manual Grep workaround.

### Goal (What to Achieve)

1. ac-static-verifier handles directory paths by recursively searching files within
2. Existing AC definitions with directory paths work without modification

---

## Links

- [feature-697.md](feature-697.md) - 発見元

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: ac-static-verifier fails with "Permission denied" when AC Method specifies `Grep(tools/YamlSchemaGen/)`.
2. Why: The `_search_pattern_native` method calls `open(file_path, 'r')` on a directory path.
3. Why: The `_expand_glob_path` method returns the directory path directly when it exists and has no glob patterns.
4. Why: There is no check for whether the resolved path is a file or directory before attempting to open it.
5. Why: The tool was designed for file paths only; directory path support was not considered in the original implementation.

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| "Permission denied" error when path ends with `/` | `_search_pattern_native` method attempts `open()` on a directory path without detecting it is a directory, which raises `PermissionError` on Windows |
| AC verification fails with exit code 1 | No directory detection in `_expand_glob_path`; directories pass the `exists()` check but fail when opened as files |

### Conclusion

The root cause is that `_expand_glob_path` at line 130-133 checks `target_file.exists()` but does not distinguish between files and directories. When a directory path like `tools/YamlSchemaGen/` is provided:

1. `has_glob_pattern = False` (no `*`, `?`, or `[`)
2. `target_file.exists() = True` (directory exists)
3. Returns `[Path('tools/YamlSchemaGen')]` as a single "file"
4. `_search_pattern_native` then calls `open(file_path, 'r')` which fails with `PermissionError`

The fix requires detecting directories and recursively discovering files within them when no glob pattern is present.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F697 | [DONE] | Discovery source | AC#4 `Grep(tools/YamlSchemaGen/)` triggered this issue |
| F644 | [DONE] | Previous occurrence | Same issue documented in Execution Log and Mandatory Handoffs |
| F630 | [DONE] | Added glob support | Glob pattern expansion was added but directory recursion was not |

### Pattern Analysis

This is not a recurring pattern from past fixes. It's a gap in the original `_expand_glob_path` implementation that only handles two cases:
1. Paths with glob patterns (`*`, `?`, `[`) - uses `glob.glob()` for expansion
2. Literal file paths - uses direct `exists()` check

The third case (directory paths without glob patterns) was never implemented.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Standard Python directory detection (`Path.is_dir()`) and file enumeration (`Path.rglob('*')` or `os.walk()`) can be used |
| Scope is realistic | YES | Single method modification (`_expand_glob_path`) with clear logic: detect directory, enumerate files recursively |
| No blocking constraints | YES | No external dependencies; pure Python implementation |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F697 | [DONE] | Discovery source; F699 was created from F697 handoff |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Python pathlib | Runtime | None | Standard library, already used |
| Python glob | Runtime | None | Standard library, already used |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| All features using `Grep(directory/)` in AC table | HIGH | AC verification will work correctly after fix |
| F697 (retroactively) | NONE | Already completed with manual workaround |
| F644 (retroactively) | NONE | Already completed with manual workaround |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ac-static-verifier.py | Update | Add directory detection and recursive file enumeration in `_expand_glob_path` |
| tools/tests/test_ac_verifier_directory.py | Create | New test file for directory path handling |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Backward compatibility | Existing ACs | MEDIUM - Must not break existing file path or glob pattern handling |
| Windows compatibility | PermissionError behavior | LOW - Already confirmed behavior; fix applies same on all platforms |
| Performance | Large directories | LOW - Use lazy enumeration or depth limit if needed |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Breaking existing tests | Low | High | Run all existing ac-verifier tests before and after change |
| Performance on large directories | Low | Medium | Use `Path.rglob('*')` with file-only filter; consider depth limit for safety |
| Mixed directory/file path detection | Low | Medium | Use `Path.is_dir()` explicitly, not rely on path string ending with `/` |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "handles directory paths by recursively searching files within" | Directory detection: `Path.is_dir()` check added to `_expand_glob_path` | AC#1, AC#2 |
| "recursively searching files" | Recursive file enumeration for directories | AC#3, AC#4 |
| "Existing AC definitions with directory paths work without modification" | Backward compatibility: existing file/glob paths still work | AC#5, AC#6 |
| "No breaking existing tests" (Risk mitigation) | All existing tests pass | AC#7 |
| "Performance on large directories" (Risk mitigation) | Uses file-only filter | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Directory detection added | code | Grep(tools/ac-static-verifier.py) | contains | "is_dir()" | [x] |
| 2 | Directory path returns files, not directory itself | test | pytest tools/tests/test_ac_verifier_directory.py::test_directory_path_returns_files | succeeds | - | [x] |
| 3 | Recursive file enumeration for directory | code | Grep(tools/ac-static-verifier.py) | matches | "rglob|iterdir|walk" | [x] |
| 4 | Directory with nested subdirectories works | test | pytest tools/tests/test_ac_verifier_directory.py::test_directory_with_nested_subdirs | succeeds | - | [x] |
| 5 | Existing glob pattern still works | test | pytest tools/tests/test_ac_verifier_glob_content.py | succeeds | - | [x] |
| 6 | Existing literal file path still works | test | pytest tools/tests/test_ac_verifier_normal.py | succeeds | - | [x] |
| 7 | All existing ac-verifier tests pass | exit_code | pytest tools/tests/test_ac_verifier*.py | succeeds | - | [x] |
| 8 | Files-only filter applied (no directories in results) | test | pytest tools/tests/test_ac_verifier_directory.py::test_files_only_no_directories | succeeds | - | [x] |
| 9 | New test file exists | file | Glob(tools/tests/test_ac_verifier_directory.py) | exists | - | [x] |
| 10 | No technical debt markers | code | Grep(tools/ac-static-verifier.py) | not_contains | "TODO\|FIXME\|HACK" | [x] |

**Note**: 10 ACs within infra range (8-15).

### AC Details

**AC#1: Directory detection added**
- Why: Root cause is missing `is_dir()` check in `_expand_glob_path`
- Verification: Code contains `is_dir()` call for detecting directories
- Rationale: Direct verification of fix implementation

**AC#2: Directory path returns files, not directory itself**
- Why: Core functionality - directory path must expand to files within it
- Test: Pass `tools/YamlSchemaGen/` → must return files like `*.cs`, not the directory path
- Edge case: Empty directory should return empty list, not fail

**AC#3: Recursive file enumeration for directory**
- Why: Directories may have nested structure; shallow enumeration would miss files
- Pattern: `rglob`, `iterdir`, or `os.walk` indicates recursive enumeration
- Rationale: Regex alternation matches any of the standard Python recursive enumeration methods

**AC#4: Directory with nested subdirectories works**
- Why: Verifies recursive behavior works correctly for nested structures
- Test: Directory with `subdir/file.txt` structure must find the nested file
- Edge case: Multiple nesting levels

**AC#5: Existing glob pattern still works**
- Why: Backward compatibility - glob patterns like `*.cs` must not be broken
- Regression test: Run existing glob content tests

**AC#6: Existing literal file path still works**
- Why: Backward compatibility - direct file paths must not be broken
- Regression test: Run existing normal path tests

**AC#7: All existing ac-verifier tests pass**
- Why: Comprehensive regression safety net
- Test: All test_ac_verifier*.py files pass

**AC#8: Files-only filter applied**
- Why: Content search on directories would fail; only files should be returned
- Test: Directory containing subdirectory must not include the subdirectory in results
- Rationale: `_search_pattern_native` opens files with `open()`, so directories must be excluded

**AC#9: New test file exists**
- Why: Test coverage for new functionality
- File: `tools/tests/test_ac_verifier_directory.py`

**AC#10: No technical debt markers**
- Why: Clean implementation without deferred work
- Pattern: Standard TODO|FIXME|HACK check

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The fix requires modifying the `_expand_glob_path` method to detect when the resolved path is a directory and recursively enumerate files within it. The implementation will:

1. **Directory detection**: Add `Path.is_dir()` check after `target_file.exists()` succeeds
2. **Recursive file enumeration**: Use `Path.rglob('*')` with file-only filtering when directory detected
3. **Files-only filter**: Filter results to include only files (not directories) to prevent downstream `open()` failures

This approach maintains backward compatibility by preserving existing behavior for glob patterns and literal file paths while adding directory handling as a third case.

**Rationale**: The root cause is that `_expand_glob_path` treats directories as files when no glob pattern is present. Adding an explicit directory check and recursive enumeration solves this without affecting existing functionality.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `is_dir()` method call in `_expand_glob_path` after `exists()` check |
| 2 | Create pytest test with temporary directory path, verify returned list contains files only |
| 3 | Use `Path.rglob('*')` for recursive enumeration (grep will match `rglob` pattern) |
| 4 | Create pytest test with nested subdirectory structure (e.g., `subdir/file.txt`), verify file is found |
| 5 | Run existing `test_ac_verifier_glob_content.py` as regression test |
| 6 | Run existing `test_ac_verifier_normal.py` as regression test |
| 7 | Run all `test_ac_verifier*.py` files with `pytest tools/tests/test_ac_verifier*.py` |
| 8 | Filter rglob results with `.is_file()` to exclude directories from returned list |
| 9 | Create `tools/tests/test_ac_verifier_directory.py` file |
| 10 | Grep verification on `tools/ac-static-verifier.py` with `not_contains` matcher |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Recursive enumeration method | `rglob('*')`, `iterdir()` + recursion, `os.walk()` | `Path.rglob('*')` | Most concise, returns Path objects directly, consistent with pathlib usage in codebase |
| File filtering approach | Filter at enumeration, Filter before return, Filter in caller | Filter before return | Centralized filtering ensures no directories leak to downstream code |
| Empty directory handling | Return empty list, Raise error | Return empty list | Consistent with existing glob pattern behavior (no matches → empty list) |
| Directory detection order | Check `is_dir()` before `exists()`, Check `is_dir()` after `exists()` | After `exists()` | Minimizes filesystem calls; only check directory if path exists |
| Depth limit | Add depth limit, Unlimited recursion | Unlimited recursion | Match glob behavior (no depth limit); performance concern is low (ac-verifier operates on specific project paths) |

### Implementation Logic

The modified `_expand_glob_path` method will follow this logic flow:

```python
def _expand_glob_path(self, file_path: str) -> tuple[bool, Optional[str], List[Path]]:
    # ... existing comma-separated handling ...

    target_file = self.repo_root / file_path
    has_glob_pattern = any(c in file_path for c in ['*', '?', '['])

    if has_glob_pattern:
        # Existing glob pattern handling (unchanged)
        import glob as glob_module
        matches = list(glob_module.glob(str(target_file), recursive=True))
        if not matches:
            return False, f"No files match glob pattern: {file_path}", []
        return True, None, [Path(m) for m in matches]
    else:
        # Direct path check
        if not target_file.exists():
            return False, f"File not found: {file_path}", []

        # NEW: Directory detection and recursive enumeration
        if target_file.is_dir():
            # Recursively enumerate all files in directory
            all_files = [f for f in target_file.rglob('*') if f.is_file()]
            return True, None, all_files

        # Existing: literal file path
        return True, None, [target_file]
```

**Edge Cases Handled**:
- **Empty directory**: Returns empty list (consistent with "no glob matches" behavior)
- **Directory with subdirectories**: `rglob('*')` finds all nested files; `.is_file()` filter excludes directories
- **Symbolic links**: `rglob` follows symlinks by default (consistent with existing file path behavior)

### Interfaces / Data Structures

No new interfaces or data structures required. The change is localized to `_expand_glob_path` method signature and behavior:

**Method Signature** (unchanged):
```python
def _expand_glob_path(self, file_path: str) -> tuple[bool, Optional[str], List[Path]]
```

**Behavior Change**:
- **Before**: Directory path → returns `[Path(directory)]` → downstream `open()` fails with PermissionError
- **After**: Directory path → returns `[Path(file1), Path(file2), ...]` → downstream `open()` succeeds for each file

### Test Structure

**New Test File**: `tools/tests/test_ac_verifier_directory.py`

Test cases to implement:
1. `test_directory_path_returns_files`: Directory path expands to files, not directory itself
2. `test_directory_with_nested_subdirs`: Nested directory structure finds all files
3. `test_files_only_no_directories`: Subdirectories are excluded from results
4. `test_empty_directory_returns_empty_list`: Empty directory returns `[]` without error

Test pattern follows existing test structure (see `test_ac_verifier_glob_content.py`):
- Use `tempfile.TemporaryDirectory()` for test isolation
- Create `ACDefinition` objects with `Grep(directory_path/)` format
- Instantiate `ACVerifier` with temp directory as repo root
- Verify results with assertions

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,3 | Modify `_expand_glob_path` to detect directories and recursively enumerate files | [x] |
| 2 | 9 | Create new test file `tools/tests/test_ac_verifier_directory.py` with test structure | [x] |
| 3 | 2,4,8 | Implement directory-specific test cases (returns files, nested subdirs, files-only filter) | [x] |
| 4 | 5 | Run existing glob pattern regression test | [x] |
| 5 | 6 | Run existing literal file path regression test | [x] |
| 6 | 7 | Run all existing ac-verifier tests as regression suite | [x] |
| 7 | 10 | Verify no technical debt markers in implementation | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Technical Design implementation logic | Modified `tools/ac-static-verifier.py` with directory detection and recursive file enumeration |
| 2 | implementer | sonnet | T2 | Testing SKILL test structure patterns | New file `tools/tests/test_ac_verifier_directory.py` |
| 3 | implementer | sonnet | T3 | Test case specifications from AC Details | Implemented test cases in test_ac_verifier_directory.py |
| 4 | ac-tester | haiku | T4-T7 | AC verification commands | Test results confirming all ACs pass |

**Constraints** (from Technical Design):

1. Use `Path.rglob('*')` for recursive enumeration (consistent with pathlib usage)
2. Apply `.is_file()` filter before returning results (prevent downstream `open()` failures)
3. Preserve existing behavior for glob patterns and literal file paths (backward compatibility)
4. Add directory check AFTER `exists()` check (minimize filesystem calls)

**Pre-conditions**:

- `tools/ac-static-verifier.py` exists and has `_expand_glob_path` method
- Existing test files `test_ac_verifier_glob_content.py` and `test_ac_verifier_normal.py` exist and pass
- Python pathlib and glob modules available (standard library)

**Success Criteria**:

- All 10 ACs pass verification
- Existing regression tests continue to pass
- Directory paths like `Grep(tools/YamlSchemaGen/)` work without modification
- No technical debt markers in implementation

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation
4. Restore manual workaround pattern (use `Grep(tools/YamlSchemaGen/*.cs)` instead of directory paths)

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 07:15 | T2 | Created test_ac_verifier_directory.py with test structure (4 test stubs following reference pattern) |
| 2026-01-31 07:17 | T3 | Implemented all 4 directory-specific test cases; all tests pass |
| 2026-01-31 | DEVIATION | feature-reviewer | doc-check NEEDS_REVISION: testing SKILL.md not updated for directory path support |

---
