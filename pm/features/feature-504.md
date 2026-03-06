# Feature 504: ac-static-verifier Grep Support for File Type

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

## Created: 2026-01-14

---

## Summary

Add Grep method support to `Type: file` ACs in ac-static-verifier.py, enabling content verification (contains/not_contains) for file type ACs.

**Scope**: Single tool modification (`tools/ac-static-verifier.py`)

---

## Background

### Philosophy (Mid-term Vision)

**AC Verification Completeness** - All AC types should be verifiable by ac-static-verifier without manual intervention. **SSOT**: ac-static-verifier.py is the single source of truth for static AC verification logic; feature-{ID}.md AC tables define what to verify.

### Problem (Current Issue)

F493 exposed a tool limitation:
- AC table: `| 2 | Phase 1 reviewed | file | Grep | contains | "## Phase 1" |`
- Tool behavior: `Type: file` → `verify_file_ac()` → only supports `exists`/`not_exists`
- Result: "Unknown matcher: contains" error despite valid AC definition

The AC definition is logical (file + Grep + contains = file content verification), but the tool doesn't support this combination.

### Goal (What to Achieve)

1. **Extend `verify_file_ac()`** to check Method column
2. **When Method=Grep**: Use content verification (like `verify_code_ac`)
3. **Support matchers**: `contains`, `not_contains` for file type with Grep method
4. **Backward compatible**: `exists`/`not_exists` continue to work

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Grep method detected | code | Grep(tools/ac-static-verifier.py) | contains | .method.lower() | [x] |
| 2 | Contains matcher works | code | Grep(tools/ac-static-verifier.py) | contains | not_contains | [x] |
| 3 | File+Grep AC verifies | file | Grep(Game/agents/feature-504.md) | contains | Format Decision | [x] |
| 4 | file+Glob ACs work | build | - | succeeds | python tools/ac-static-verifier.py --feature 223 --ac-type file | [x] |
| 5 | Build succeeds | build | - | succeeds | dotnet build engine/uEmuera.Headless.csproj | [x] |
| 6a | No TODO comments | code | Grep(tools/ac-static-verifier.py) | not_contains | TODO | [x] |
| 6b | No FIXME comments | code | Grep(tools/ac-static-verifier.py) | not_contains | FIXME | [x] |
| 6c | No HACK comments | code | Grep(tools/ac-static-verifier.py) | not_contains | HACK | [x] |

### AC Details

**AC#1-2**: Code changes present
- Test: Grep patterns in ac-static-verifier.py
- Expected: Method detection and matcher support code exists

**AC#3**: File+Grep AC verifies
- Test: Self-verification using F504's own "Format Decision" section
- Expected: AC verifier successfully finds "Format Decision" in this feature file
- Note: F493's file+Grep ACs use old format and require separate migration (out of scope)

**AC#4**: Backward compatibility
- Test: `python tools/ac-static-verifier.py --feature 223 --ac-type file`
- Expected: Exit code 0 (F223's `file | Glob | exists` ACs pass without breaking)

**AC#5**: Build verification
- Test: `dotnet build`
- Expected: No build errors (Python tool, but verify overall project health)

**AC#6a-6c**: No technical debt introduced
- Test: Grep for TODO, FIXME, HACK individually in modified file
- Expected: 0 matches each (literal string search via -F flag)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add Grep method detection in verify_file_ac() | [x] |
| 2 | 2 | Implement _verify_file_content() with contains/not_contains matcher support | [x] |
| 3 | 3 | Verify self-test: file+Grep AC finds "Format Decision" | [x] |
| 4 | 4 | Verify backward compatibility with F223 file+exists ACs | [x] |
| 5 | 5 | Run dotnet build to verify overall project health | [x] |
| 6 | 6a,6b,6c | Verify no TODO/FIXME/HACK comments in modified file | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Modification Target

`tools/ac-static-verifier.py` line 284-340 (`verify_file_ac` method)

### Logic Change

```python
def verify_file_ac(self, ac: ACDefinition) -> Dict[str, Any]:
    # NEW: Check if Method contains "Grep"
    if "grep" in ac.method.lower():
        # Delegate to content verification logic
        return self._verify_file_content(ac)

    # EXISTING: File existence check (unchanged)
    file_path = ac.expected
    matcher = ac.matcher.lower()
    # ... existing logic ...
```

### New Helper Method

```python
def _verify_file_content(self, ac: ACDefinition) -> Dict[str, Any]:
    """Verify file content using grep when Method=Grep(path) for file type."""
    # Extract file path from Method field: Grep(path) -> path
    match = re.search(r'Grep\s*\(\s*([^)]+)\s*\)', ac.method, re.IGNORECASE)
    if not match:
        return {"ac_number": ac.ac_number, "result": "ERROR",
                "details": {"error": f"Invalid Method format: {ac.method}. Expected: Grep(path)",
                           "pattern": ac.expected, "matched_files": []}}

    file_path = self.repo_root / match.group(1).strip()
    if not file_path.exists():
        return {"ac_number": ac.ac_number, "result": "FAIL",
                "details": {"error": f"File not found: {file_path}",
                           "pattern": ac.expected, "file_path": str(file_path),
                           "matcher": ac.matcher.lower(), "matched_files": []}}

    pattern = ac.expected
    matcher = ac.matcher.lower()

    # Execute grep (same logic as verify_code_ac)
    try:
        result = subprocess.run(
            ["rg", "-F", "--count", pattern, str(file_path)],
            capture_output=True, text=True, encoding='utf-8', errors='replace'
        )
        pattern_found = result.returncode == 0
    except FileNotFoundError:
        # Fallback to grep
        try:
            result = subprocess.run(
                ["grep", "-F", "-c", pattern, str(file_path)],
                capture_output=True, text=True, encoding='utf-8', errors='replace'
            )
            pattern_found = result.returncode == 0
        except (FileNotFoundError, UnicodeDecodeError):
            # Neither rg nor grep available or encoding issues, fallback to Python
            with open(file_path, 'r', encoding='utf-8') as f:
                pattern_found = pattern in f.read()

    # Apply matcher logic
    if matcher == "contains":
        passed = pattern_found
    elif matcher == "not_contains":
        passed = not pattern_found
    else:
        return {"ac_number": ac.ac_number, "result": "FAIL",
                "details": {"error": f"Unknown matcher: {matcher}",
                           "pattern": pattern, "file_path": str(file_path),
                           "matcher": matcher, "matched_files": []}}

    return {"ac_number": ac.ac_number, "result": "PASS" if passed else "FAIL",
            "details": {"pattern": pattern, "file_path": str(file_path),
                       "matcher": matcher, "pattern_found": pattern_found}}
```

### Format Decision

**Decided Format**: Use `Grep(path)` in Method column (consistent with `code` type):
```
| N | Description | Type | Method | Matcher | Expected | Status |
| - | Phase 1 reviewed | file | Grep(path/to/file.md) | contains | ## Phase 1 | [ ] |
```

**Rationale**: Consistent with code type's `Grep(path)` format. Allows static parsing without AC Details lookup.

**Migration Required**: F493's file+Grep ACs need format update (add file path to Method column).

### Rollback Plan

If issues arise:
1. `git revert` the commit
2. Manual verification remains available as fallback
3. Create follow-up feature for alternative approach

---

## Impact Analysis

| Component | Impact | Notes |
|-----------|--------|-------|
| do.md Phase 7 | Uses ac-static-verifier for AC verification | Extended capability |
| F493 file+Grep ACs | Require format migration | Out of scope; separate feature needed |
| Future review features | Can use file+Grep pattern | New capability enabled |
| testing SKILL | May need update to document new format | Post-implementation |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Discovered-by | F493 | Code Review Phase 1-4 exposed this limitation |
| Related | F223 | Existing file type AC feature with 7-column format (backward compatibility test) |

---

## Links

- [feature-493.md](feature-493.md) - Discovery feature
- [feature-223.md](feature-223.md) - Backward compatibility test reference
- [ac-static-verifier.py](../../tools/ac-static-verifier.py) - Target tool

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-14 FL iter1**: [applied] Open Question → Format Decision (Grep(path) format decided, consistent with code type)
- **2026-01-14 FL iter1**: [applied] AC#3 changed from F493 test to self-verification (F493's old format requires separate migration)
- **2026-01-14 FL iter1**: [applied] AC#4 changed from F341 to F150 (F341 uses file+contains which isn't backward compat test)
- **2026-01-14 FL iter1**: [applied] Implementation Contract helper method completed with actual logic
- **2026-01-14 FL iter1**: [applied] Added Impact Analysis section per INFRA.md Issue 6
- **2026-01-14 FL iter2**: [applied] AC#1,2 Expected changed to literal strings (ac-static-verifier uses -F literal matching)
- **2026-01-14 FL iter2**: [applied] AC#6 split into AC#6a,6b,6c for individual TODO/FIXME/HACK checks
- **2026-01-14 FL iter2**: [applied] Tasks expanded to 1:1 AC mapping (3 → 6 tasks)
- **2026-01-14 FL iter2**: [applied] _verify_file_content() implementation completed with full logic
- **2026-01-14 FL iter3**: [applied] AC#4 changed from F150 to F223 (F150 uses 6-column format without Method column)
- **2026-01-14 FL iter3**: [applied] Added 残課題 for F493 AC format migration
- **2026-01-14 FL iter4**: [applied] AC#4 Type changed from exit_code to build (exit_code not supported by static verifier)
- **2026-01-14 FL iter4**: [applied] Task#4 updated from F150 to F223
- **2026-01-14 FL iter4**: [applied] 残課題 Target Feature changed to concrete destination (F493 残課題)
- **2026-01-14 FL iter5**: [applied] AC#4,5 build type format fixed (Expected field should contain full command per verify_build_ac)
- **2026-01-14 FL iter6**: [applied] _verify_file_content() removed .strip('"') to match verify_code_ac() behavior (quote handling consistency)
- **2026-01-14 FL iter6**: [resolved] Phase3-Maintainability - Code duplication acceptable. Rationale: (1) Different entry points serve different verification paths (Type=code vs Type=file+Grep); (2) Extracting common grep logic would require refactoring verify_code_ac() which is out of scope; (3) Minimal duplication (~15 lines) does not justify abstraction overhead
- **2026-01-14 FL iter7**: [applied] Error message format standardized in _verify_file_content() to match verify_code_ac() pattern
- **2026-01-14 FL iter8**: [applied] Regex pattern updated to match verify_code_ac() (re.search with whitespace handling, .strip() on group)
- **2026-01-14 FL iter8**: [applied] Error returns aligned with verify_code_ac() format (added pattern, matched_files keys)
- **2026-01-14 FL iter9**: [applied] Fallback chain updated to match verify_code_ac() (rg → grep → Python with UnicodeDecodeError handling)
- **2026-01-14 FL iter9**: [applied] AC#4 description changed to "file+Glob ACs work" for precision (user decision)

---

## 残課題

| Task | Reason | Target Phase | Target Feature |
|------|--------|:------------:|:--------------:|
| F493 AC format migration | F493's file+Grep ACs use old format without file path in Method column | Phase 15 | F493 残課題 (add to F493's own tracking) |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | opus | Created from F493 残課題 | PROPOSED |
| 2026-01-14 21:58 | START | implementer | Task 1-2 | - |
| 2026-01-14 21:58 | END | implementer | Task 1-2 | SUCCESS |
| 2026-01-14 21:59 | DEVIATION | opus | AC#1-2 test | FAIL (quotes in Expected) |
| 2026-01-14 21:59 | END | opus | AC table fix | Fixed quotes |
| 2026-01-14 22:00 | START | ac-tester | Verify ACs | - |
| 2026-01-14 22:00 | END | ac-tester | Verify ACs | PASS 8/8 |
| 2026-01-14 22:01 | START | feature-reviewer | post | - |
| 2026-01-14 22:01 | END | feature-reviewer | post | READY |
| 2026-01-14 22:01 | START | feature-reviewer | doc-check | - |
| 2026-01-14 22:01 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION (testing SKILL) |
| 2026-01-14 22:02 | END | opus | SSOT update | testing SKILL.md updated |
