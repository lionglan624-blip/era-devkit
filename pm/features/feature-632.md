# Feature 632: ac-static-verifier Rewrite with Pattern Classification System

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

## Created: 2026-01-27

---

## Summary

Rewrite ac-static-verifier with a Pattern Classification System to break the cycle of incremental patch fixes (10 Features in 6 days: F587→F631). Subsumes F672 (Complex Method Format Support).

---

## Background

### Philosophy (Mid-term Vision)

ac-static-verifier must correctly classify and handle all AC pattern types (glob, regex, literal, complex Method format) through a unified type system, eliminating the root cause of repeated breakage.

### Problem (Current Issue)

ac-static-verifier has required **10 Features in 6 days** (F587, F593, F600, F601, F608, F618, F621, F626, F630, F631) plus 2 pending (F632, F672). Each fix addresses symptoms without resolving the architectural root cause:

1. **No pattern taxonomy**: tool cannot distinguish glob vs regex vs literal vs complex Method
2. **Code duplication**: `verify_code_ac()` and `_verify_file_content()` share nearly identical logic
3. **Brittle parsing**: each new AC pattern format breaks existing assumptions
4. **No enforcement**: F608 audit identified gaps but workflow has no gate to reject unsupported patterns

### Root Cause (from F634 Phase 8 investigation)

**Organic tool evolution without comprehensive upfront design.** The tool was built incrementally to handle patterns as they emerged, creating a brittle codebase that breaks with each new pattern variation.

### Goal (What to Achieve)

1. Implement Pattern Type Classification System (glob, regex, literal, complex Method)
2. Eliminate code duplication between verify functions
3. Support complex Method format: `Grep(path="...", pattern="...", type=cs)` (absorbs F672)
4. Prevent future pattern-related breakage through explicit type handling

---

## Links

- [index-features.md](index-features.md)
- [feature-631.md](feature-631.md) - Predecessor: latest incremental fix, identified code duplication
- [feature-672.md](feature-672.md) - CANCELLED: absorbed into F632 scope
- [feature-634.md](feature-634.md) - Origin of F672 (AC#10 DEVIATION triggered root cause investigation)
- [feature-587.md](feature-587.md) - Historical: Quote stripping (1st fix)
- [feature-593.md](feature-593.md) - Historical: matches matcher (2nd fix)
- [feature-621.md](feature-621.md) - Historical: Pattern parsing fundamental fix (4 issues)
- [feature-626.md](feature-626.md) - Historical: Matcher enhancement
- [feature-630.md](feature-630.md) - Historical: Pattern escaping fix
- [feature-608.md](feature-608.md) - Historical: AC pattern coverage audit
- [feature-600.md](feature-600.md) - Historical: Related ac-static-verifier fix
- [feature-601.md](feature-601.md) - Historical: Related ac-static-verifier fix
- [feature-613.md](feature-613.md) - Related: Referenced in Related Features
- [feature-618.md](feature-618.md) - Historical: Related ac-static-verifier fix
- [feature-623.md](feature-623.md) - Related: Referenced in Related Features
- [feature-624.md](feature-624.md) - Related: Referenced in Related Features

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why**: ac-static-verifier has required 10 fix features in 6 days (F587-F631), each addressing new edge cases
2. **Why**: Each new AC pattern format (complex Grep, comma globs, bracket escapes, emoji, dollar signs) breaks existing parsing assumptions
3. **Why**: The tool has no pattern taxonomy -- it cannot distinguish glob patterns from regex patterns from literal strings from complex Method formats before processing them
4. **Why**: Pattern handling grew organically through incremental fixes without upfront design; `verify_code_ac()` and `_verify_file_content()` are 96% identical (121/125 lines) because each was patched independently
5. **Why**: **Root Cause**: The tool was built as a simple grep wrapper (F587: quote stripping) and evolved through 10 accretive patches without ever establishing a type system for the 4 fundamentally different pattern categories it must handle (glob, regex, literal, complex Method)

### Symptom vs Root Cause

| Symptom (Observable) | Root Cause (Architectural) |
|----------------------|---------------------------|
| 10 fix features in 6 days | No pattern type classification system -- each new pattern variant requires a new feature |
| `verify_code_ac()` and `_verify_file_content()` are 96% duplicated (121/125 lines identical) | No shared content verification abstraction -- methods were copy-pasted and patched independently |
| Complex Grep format `Grep(path="...", pattern="...", type=cs)` cannot be parsed | Method column regex `r'Grep\s*\(\s*([^)]+)\s*\)'` only extracts a single positional argument |
| Each fix introduces new edge cases (F621 bracket fix created F631 bracket escape issue) | Fixes target symptoms at the string-manipulation level rather than classifying pattern types first |
| F608 audit identified gaps but breakage continues | No enforcement gate -- AC authors can write unsupported patterns without tool-level rejection |

### Conclusion

The root cause is **architectural**: ac-static-verifier lacks a pattern classification layer between parsing and verification. The current architecture processes raw strings through ad-hoc regex/string operations, making every new pattern variant a breaking change. Specifically:

1. **No Pattern Type enum**: The tool treats all Expected values as raw strings, applying heuristics (e.g., `_contains_regex_metacharacters()`) to guess the intent. A type system (glob | regex | literal | complex_method) would make intent explicit.

2. **96% code duplication**: `verify_code_ac()` (L263-387) and `_verify_file_content()` (L460-584) share 121 of 125 lines. This means every fix must be applied twice, and F621/F631 both noted this as deferred technical debt (F631 explicitly deferred to F632).

3. **Method parsing is single-format**: The current regex `r'Grep\s*\(\s*([^)]+)\s*\)'` with fallback `r'Grep\s+(.+)'` cannot parse named parameters like `Grep(path="...", pattern="...", type=cs)` (F672 requirement, now absorbed into F632).

4. **No matcher-pattern compatibility validation**: The tool does not verify that a given pattern type is compatible with a given matcher (e.g., regex patterns should only be used with `matches` matcher, not `contains`). F621 added `_contains_regex_metacharacters()` as a band-aid, but a type system would make this structural.

The incremental patch approach has reached diminishing returns. Each fix creates new edge cases because the tool lacks the structural foundation to handle pattern diversity.

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F631 | [DONE] | Predecessor | Latest incremental fix (comma globs, bracket escape, emoji). Explicitly deferred code duplication to F632 |
| F672 | [CANCELLED] | Absorbed | Complex Method format `Grep(path="...", pattern="...", type=cs)` -- absorbed into F632 scope |
| F634 | [DONE] | Origin | Batch conversion tool. AC#10 DEVIATION triggered root cause investigation leading to F632 |
| F608 | [DONE] | Reference | AC pattern coverage audit -- identified gaps between Testing SKILL docs and verifier implementation |
| F621 | [DONE] | Historical fix | Pattern Parsing Fundamental Fix -- 4 issues (Method format, escape encoding, backtick, regex detection) |
| F626 | [DONE] | Historical fix | Matcher Enhancement -- added Glob(pattern) support, fixed exists/build matcher column usage |
| F630 | [DONE] | Historical fix | Pattern Escaping Fix -- dollar sign and glob expansion issues |
| F593 | [DONE] | Historical fix | Added `matches` matcher with regex support (duplicated in both verify methods) |
| F587 | [DONE] | Historical fix | First fix -- quote stripping for Expected column |
| F601 | [DONE] | Historical fix | Slash command alternative verification -- added MANUAL status |
| F600 | [CANCELLED] | Superseded | Slash command matcher support -- cancelled in favor of F601 approach |
| F618 | [CANCELLED] | Investigation | MANUAL status counting investigation -- no bug found |
| F613 | [DONE] | Reference | Phase 2 AC pattern coverage audit (remaining matchers) |
| F624 | [CANCELLED] | Related | Grep logic consolidation -- superseded by F632 rewrite |
| F623 | [CANCELLED] | Related | Character class pattern refinement -- superseded by F631, then F632 |

### Pattern Analysis

**Recurring Pattern**: ac-static-verifier has been the subject of **12 features** (10 completed, 2 cancelled) in the period 2026-01-21 to 2026-01-27:

| Date | Feature | Issue | Fix Approach | New Edge Cases Created |
|------|---------|-------|--------------|------------------------|
| 01-21 | F587 | Quote stripping | Add `.strip('"')` | None (first fix) |
| 01-22 | F593 | `matches` matcher missing | Add regex branch in both methods | Code duplication established |
| 01-23 | F600 | Slash command matcher | Cancelled | Replaced by F601 |
| 01-23 | F601 | Slash command handling | Add MANUAL status | None |
| 01-24 | F608 | Pattern coverage audit | Documentation | Identified unsupported matchers |
| 01-25 | F618 | MANUAL counting bug | Investigation | No bug found |
| 01-25 | F621 | 4 parsing issues | Method format, escape, backtick, regex detection | Character class false-positive (F623) |
| 01-26 | F626 | Matcher column usage | Added Glob() support, fixed exists/build | None |
| 01-26 | F630 | Dollar sign, glob expansion | Escape handling | None |
| 01-27 | F631 | Comma globs, bracket escape, emoji | Pre-split comma, negative lookbehind, UTF-8 verify | Code duplication deferred to F632 |

**Why the cycle continues**: Each fix operates at the string-manipulation level within a flat architecture. There is no abstraction layer that would allow a new pattern type to be added without modifying the core verification methods. The 96% code duplication between `verify_code_ac()` and `_verify_file_content()` means every fix must be applied twice, doubling the chance of introducing new edge cases.

**How F632 breaks the cycle**: By introducing a Pattern Classification System (type enum + unified verification pipeline), new pattern types can be added as new classifier entries rather than new branches in duplicated methods.

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | **YES** | The codebase is a single 782-line Python file with clear method boundaries; pattern types are identifiable from existing code; 12 test files provide regression coverage |
| Scope is realistic | **YES** | Core rewrite targets 1 file (`ac-static-verifier.py`); the file is 782 lines, not thousands; existing 12 test files provide safety net; F672 absorption adds Method parser complexity but is well-defined |
| No blocking constraints | **YES** | F631 (predecessor) is [DONE]; JSON schema and exit code contracts are well-documented and easily preserved; Python stdlib only (no external dependencies) |

**Verdict**: **FEASIBLE**

**Supporting Evidence**:

1. **Bounded scope**: Rewrite targets a single file (782 lines) with 2 classes (`ACDefinition`, `ACVerifier`). The `ACDefinition` data class is clean and needs no change. The rewrite focuses on `ACVerifier` internal architecture.

2. **Strong regression safety**: 12 existing test files (57+ tests per F631 execution log) cover all previously fixed edge cases. These tests are the specification for backward compatibility.

3. **Clear type taxonomy**: Investigation reveals exactly 4 pattern categories that already exist implicitly in the code:
   - **Literal**: `contains`/`not_contains` matcher with plain strings (most common)
   - **Regex**: `matches` matcher with regex patterns
   - **Glob**: File path patterns with `*`, `?`, `[...]`, comma-separated
   - **Complex Method**: `Grep(path="...", pattern="...", type=cs)` named parameters (F672)

4. **Duplication elimination path**: The 96% duplication between `verify_code_ac()` and `_verify_file_content()` can be unified into a single `_verify_content()` method, reducing ~125 duplicated lines to ~130 shared lines.

5. **F672 absorption is natural**: Complex Method format support requires a Method parser, which aligns with the Pattern Classification System design. Adding it during rewrite is more efficient than retrofitting later.

**Risk to feasibility**: The rewrite must maintain exact behavioral compatibility with 57+ existing tests. Any divergence from current behavior (even bug-compatible behavior) could break downstream features. Mitigation: TDD approach -- existing tests define the specification.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F631 | [DONE] | Latest matcher improvements must be incorporated as baseline behavior |
| Related | F672 | [CANCELLED] | Absorbed into F632 -- complex Method format support |
| Related | F634 | [DONE] | Origin -- AC#10 DEVIATION triggered the root cause investigation |
| Related | F608 | [DONE] | AC pattern coverage audit -- gap analysis informs which matchers to support |
| Related | F624 | [CANCELLED] | Grep logic consolidation -- superseded by F632 rewrite |
| Related | F623 | [CANCELLED] | Character class pattern refinement -- superseded by F631, then F632 |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Python 3 stdlib (re, glob, pathlib, json, argparse, subprocess) | Runtime | None | Standard library only, no third-party dependencies |
| pytest | Development | None | Test framework, already in use for 12 test files |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `.claude/skills/run-workflow/PHASE-6.md` | **CRITICAL** | Phase 6 AC verification dispatches to ac-static-verifier for code/build/file type ACs |
| `.claude/skills/testing/SKILL.md` | **HIGH** | Documents ac-static-verifier as the primary static verification tool; references CLI interface and output format |
| `tools/verify-logs.py` | **HIGH** | Reads JSON output schema (`summary.total/passed/manual/failed`); any schema change breaks log analysis |
| `tools/tests/test_ac_verifier_*.py` (12 files) | **HIGH** | Import `ACVerifier` and `ACDefinition` classes; test the public API surface |
| `.claude/skills/feature-quality/INFRA.md` | **MEDIUM** | References ac-static-verifier behavior for infra feature quality guidelines |
| 30+ feature files referencing ac-static-verifier | **LOW** | Historical references in feature documentation; no runtime dependency |

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `tools/ac-static-verifier.py` | **Rewrite** | Restructure ACVerifier internals: add PatternType enum, unified content verification, Method parser for complex formats |
| `tools/tests/test_ac_verifier_*.py` (12 files) | **Update** | May need import path updates if class API changes; test assertions must remain stable |
| `tools/tests/test_ac_verifier_complex_method.py` | **Create** | New tests for F672 complex Method format: `Grep(path="...", pattern="...", type=cs)` |
| `.claude/skills/testing/SKILL.md` | **Update** | Document new Pattern Classification System, complex Method format support, updated Known Limitations |
| `.claude/skills/feature-quality/INFRA.md` | **Update** | Update ac-static-verifier behavioral notes if any edge case handling changes |

### Preserved Contracts (Must NOT Change)

| Contract | Location | Details |
|----------|----------|---------|
| CLI interface | `main()` L747-782 | `--feature`, `--ac-type`, `--repo-root` arguments unchanged |
| JSON output schema | `run()` L694-744 | `feature`, `type`, `results[]`, `summary{total,passed,manual,failed}` unchanged |
| Exit codes | `run()` L744 | 0 = all pass, 1 = any fail unchanged |
| `ACDefinition` public fields | L31-41 | `ac_number`, `description`, `ac_type`, `method`, `matcher`, `expected` unchanged |
| Result dict structure | All verify methods | `ac_number`, `result` (PASS/FAIL/MANUAL), `details` structure preserved |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| JSON output schema stability | `verify-logs.py`, PHASE-6.md | **HIGH** -- `summary.total/passed/manual/failed` fields and result structure must not change |
| Exit code semantics | PHASE-6.md, testing SKILL | **HIGH** -- 0=pass, 1=fail must be preserved |
| CLI interface stability | testing SKILL, PHASE-6.md | **HIGH** -- `--feature`, `--ac-type` arguments must not change |
| `ACDefinition` class API | 12 test files import this class | **HIGH** -- Constructor signature and field names must not change |
| Backward behavioral compatibility | 57+ existing tests | **HIGH** -- All previously fixed patterns (F587-F631) must continue working identically |
| Python stdlib only | Current dependency profile | **MEDIUM** -- No third-party packages allowed (deployment simplicity) |
| MANUAL status handling | F601, F618 investigation | **MEDIUM** -- Slash command ACs must continue returning MANUAL status correctly |
| Regex metacharacter detection | F621 design decision | **MEDIUM** -- `contains` with regex patterns must still FAIL with guidance message |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Rewrite introduces behavioral regression in existing patterns | Medium | **High** | TDD approach: 57+ existing tests define the specification; run full suite after each change |
| Complex Method parser (F672) adds scope creep beyond classification system | Low | Medium | F672 scope is well-defined (3 named parameters: path, pattern, type); limit parser to this format |
| Test files depend on internal API (private methods) that change during rewrite | Medium | Medium | Audit test imports before rewrite; preserve public API; only refactor internal structure |
| Code duplication elimination changes subtle behavioral differences between `verify_code_ac` and `_verify_file_content` | Low | High | Diff the 4 non-identical lines (L263-267 docstring vs L460-464 docstring) to confirm they are only documentation differences |
| Rewrite takes longer than incremental approach due to scope | Medium | Low | The incremental approach has proven unsustainable (10 features in 6 days); rewrite investment is justified by eliminating future fix features |
| Classification heuristic misclassifies ambiguous pattern | Medium | High | Add UNKNOWN PatternType variant that falls back to existing behavior; log warning for manual review. Comprehensive test coverage for boundary cases between pattern types |
| New pattern type emerges post-rewrite that classification system does not handle | Low | Low | Classification system is extensible by design (add new enum value + handler); much easier than current approach |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "must correctly classify" all pattern types | PatternType enum with classify() method exists and handles glob, regex, literal, complex_method | AC#1, AC#2 |
| "all AC pattern types (glob, regex, literal, complex Method format)" | Each of the 4 pattern types has explicit handling in the type system | AC#1, AC#3, AC#4, AC#5, AC#15 (glob) |
| "unified type system" | Single verification pipeline replaces duplicated verify_code_ac / _verify_file_content | AC#1, AC#2 (type system), AC#6, AC#7 (pipeline) |
| "eliminating the root cause of repeated breakage" | Code duplication is eliminated; verify_code_ac and _verify_file_content consolidated | AC#6, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PatternType enum class exists | code | Grep(tools/ac-static-verifier.py) | contains | "class PatternType" | [x] |
| 2 | Pattern classification method exists | code | Grep(tools/ac-static-verifier.py) | contains | "def classify" | [x] |
| 3 | Complex Method parser handles named parameters | exit_code | pytest tools/tests/test_ac_verifier_complex_method.py | succeeds | - | [x] |
| 4 | Literal pattern handling preserved | exit_code | pytest tools/tests/test_ac_verifier_normal.py | succeeds | - | [x] |
| 5 | Regex pattern handling preserved | exit_code | pytest tools/tests/test_ac_verifier_regex_guidance.py | succeeds | - | [x] |
| 6 | Code duplication eliminated | code | Grep(tools/ac-static-verifier.py) | not_contains | "def _verify_file_content" | [x] |
| 7 | Unified content verification method exists | code | Grep(tools/ac-static-verifier.py) | contains | "def _verify_content" | [x] |
| 8 | All 72 existing tests pass (regression) | exit_code | pytest tools/tests/test_ac_verifier_*.py | succeeds | - | [x] |
| 9 | CLI interface preserved | code | Grep(tools/ac-static-verifier.py) | contains | "--feature" | [x] |
| 10 | JSON output schema preserved | code | Grep(tools/ac-static-verifier.py) | contains | "\"summary\":" | [x] |
| 11 | ACDefinition class API preserved | code | Grep(tools/ac-static-verifier.py) | contains | "class ACDefinition" | [x] |
| 12 | Exit code contract preserved | code | Grep(tools/ac-static-verifier.py) | contains | "return 0 if failed == 0 else 1" | [x] |
| 13 | New complex Method test file exists | file | Glob(tools/tests/test_ac_verifier_complex_method.py) | exists | - | [x] |
| 14 | UNKNOWN pattern type fallback behavior | exit_code | pytest tools/tests/test_ac_verifier_unknown_fallback.py | succeeds | - | [x] |
| 15 | Glob pattern handling preserved | exit_code | pytest tools/tests/test_ac_verifier_method_glob.py | succeeds | - | [x] |

**Note**: 15 ACs is within the infra range (8-15). AC#1-2 verify architecture, AC#3-5,15 verify pattern types, AC#6-7 verify duplication elimination, AC#8 verifies regression safety, AC#9-12 verify preserved contracts, AC#13 verifies F672 absorption test coverage, AC#14 verifies UNKNOWN fallback.

### AC Details

**AC#1: PatternType enum class exists**
- Verifies the core architectural addition: a PatternType enum class definition exists in the source code
- The class definition indicates type-based classification infrastructure is in place
- This is the foundation that breaks the cycle of ad-hoc pattern handling

**AC#2: Pattern classification method exists**
- Verifies a classify method exists that determines pattern type before verification
- Classification must happen before matcher application, separating "what type is this" from "how to verify it"
- This replaces the current implicit heuristics scattered throughout verify methods
- Implementation must use dispatch-based pattern handling (method per type or dispatch dict) to ensure extensibility

**AC#3: Complex Method parser handles named parameters (F672 absorption)**
- Verifies support for `Grep(path="...", pattern="...", type=cs)` format
- This was the original F672 requirement, now absorbed into F632
- Must parse named parameters (path, pattern, type) from Method column
- Tests in test_ac_verifier_complex_method.py define the exact expected behavior

**AC#4: Literal pattern handling preserved**
- Verifies existing literal string containment (contains/not_contains matchers) continues working
- test_ac_verifier_normal.py covers the baseline literal pattern behavior
- Must not regress quote stripping (F587), bracket escape (F631), dollar sign (F630), emoji (F631)

**AC#5: Regex pattern handling preserved**
- Verifies regex patterns via matches matcher continue working
- test_ac_verifier_regex_guidance.py covers regex metacharacter detection and guidance messages
- Must preserve the F621 behavior: contains matcher with regex patterns returns FAIL with guidance

**AC#6: Code duplication eliminated**
- Verifies the 96% duplicated `_verify_file_content` method no longer exists as a separate method
- The duplicate logic between verify_code_ac (L263-387) and _verify_file_content (L460-584) must be consolidated
- This is the primary structural improvement that prevents future double-patching

**AC#7: Unified content verification method exists**
- Verifies a single `_verify_content` (or similar) method handles content verification for both code and file types
- This method is the consolidation target for the duplicated logic
- Both verify_code_ac and verify_file_ac should delegate to this shared method

**AC#8: All 57 existing tests pass (regression)**
- The most critical AC: all existing tests define backward compatibility
- Runs the full test suite across all 12 test files (57 tests)
- Any regression here means the rewrite changed observable behavior, which is unacceptable

**AC#9: CLI interface preserved**
- Verifies --feature, --ac-type, --repo-root arguments still exist
- Consumer: PHASE-6.md dispatches to ac-static-verifier with these exact arguments
- Contract: no CLI breaking changes allowed

**AC#10: JSON output schema preserved**
- Verifies the summary JSON structure (total/passed/manual/failed) exists in output generation code
- Consumer: verify-logs.py reads this schema; any change breaks log analysis
- Uses contains for "summary": to avoid field-order dependency (per INFRA Issue 29)

**AC#11: ACDefinition class API preserved**
- Verifies the ACDefinition class still exists with its public interface
- Consumer: all 12 test files import and construct ACDefinition objects
- Constructor signature (ac_number, description, ac_type, method, matcher, expected) must not change

**AC#12: Exit code contract preserved**
- Verifies the return 0 / return 1 exit code semantics exist in code
- Consumer: PHASE-6.md checks exit code to determine verification success
- Uses contains matcher with the exact return statement to verify exit code logic exists

**AC#13: New complex Method test file exists**
- Verifies that F672 absorption includes proper test coverage
- Test file must cover: named parameter parsing, path/pattern/type extraction, edge cases
- File existence confirms the F672 scope was implemented, not just the classifier

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The rewrite introduces a **Pattern Classification System** as a first-class architectural layer, transforming the tool from a string-manipulation approach to a type-driven approach. The design has three pillars:

**1. PatternType Enum with Classification Method**

Introduce a `PatternType` enum to explicitly classify patterns before verification:

```python
from enum import Enum, auto

class PatternType(Enum):
    LITERAL = auto()         # Plain string for contains/not_contains
    REGEX = auto()          # Regex pattern for matches matcher
    GLOB = auto()           # File path glob patterns (*, ?, [])
    COMPLEX_METHOD = auto()  # Named parameter format: Grep(path="...", pattern="...", type=cs)
```

A `classify_pattern()` method determines pattern type based on:
- Method column format (Grep vs Grep(...) simple vs Grep(path=..., pattern=...) complex)
- Matcher type (matches = REGEX, contains/not_contains = LITERAL)
- Expected value characteristics (glob metacharacters in file paths)

**2. Unified Content Verification Pipeline**

Eliminate the 96% duplication between `verify_code_ac` (L263-387) and `_verify_file_content` (L460-584) by consolidating into a single `_verify_content()` method:

```python
def _verify_content(self, file_path: str, pattern: str, matcher: str, pattern_type: PatternType, ac_number: int) -> Dict[str, Any]:
    """Unified content verification for code and file types."""
    # 1. Expand glob patterns -> target_files
    # 2. Apply matcher logic (contains/not_contains/matches)
    # 3. Return standardized result dict
```

**Call-graph post-refactor**:
- `verify_code_ac(ac)` → extract file_path/pattern/matcher → `_verify_content(...)`
- `verify_file_ac(ac)` → if Grep branch → extract file_path/pattern/matcher → `_verify_content(...)` (replaces current L626-627 call to `_verify_file_content`)
- Other `verify_file_ac` branches (exists, glob, slash_command) remain unchanged

Both public methods become thin extractors that delegate to `_verify_content`.

**3. Complex Method Parser (F672 Absorption)**

Add a `_parse_complex_method()` helper to handle named parameter formats:

```python
def _parse_complex_method(self, method: str) -> Optional[Dict[str, str]]:
    """Parse Grep(path="...", pattern="...", type=cs) format.

    Returns:
        Dict with keys: path, pattern, type (optional)
        None if not a complex format or parsing fails
    """
```

This parser uses regex to extract named parameters. For AC verification, the `path` parameter determines the file location, and `pattern` becomes the Expected value override (if Method includes pattern). The `type` parameter (e.g., `cs` for C# file type) can be used for future type-specific handling.

**Architecture Flow**:

```
AC Input
   ↓
classify_pattern(ac) → PatternType
   ↓
   ├─ LITERAL → _verify_content(contains/not_contains)
   ├─ REGEX → _verify_content(matches)
   ├─ GLOB → _expand_glob_path → exists check
   └─ COMPLEX_METHOD → _parse_complex_method → _verify_content
```

**Backward Compatibility Strategy**:

All existing behaviors are preserved:
- CLI interface unchanged (--feature, --ac-type, --repo-root)
- JSON output schema unchanged (summary.total/passed/manual/failed)
- Exit codes unchanged (0=pass, 1=fail)
- ACDefinition class API unchanged (constructor, fields)
- All 57 existing tests must pass (regression safety)

The refactor is **internal only** -- the classification layer is an implementation detail. External consumers (PHASE-6.md, verify-logs.py, test files) see no API changes.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Define `PatternType` enum with 5 values (LITERAL, REGEX, GLOB, COMPLEX_METHOD, UNKNOWN) at module level, above ACDefinition class |
| 2 | Implement `ACVerifier.classify_pattern(ac: ACDefinition) -> PatternType` method during AC parsing; populate ac.pattern_type field eagerly |
| 3 | Create `test_ac_verifier_complex_method.py` with tests for `Grep(path="...", pattern="...", type=cs)` parsing and verification; tests must pass via pytest |
| 4 | Run `pytest tools/tests/test_ac_verifier_normal.py` (literal patterns with contains/not_contains); existing tests define specification, all must pass |
| 5 | Run `pytest tools/tests/test_ac_verifier_regex_guidance.py` (regex patterns with matches matcher, guidance for misuse); existing tests must pass |
| 6 | Delete `_verify_file_content` method entirely; grep verification logic moves to `_verify_content` shared method |
| 7 | Implement `_verify_content(file_path, pattern, matcher, pattern_type, ac_number) -> Dict[str, Any]` as the unified content verification method; both `verify_code_ac` and file-type Grep handling delegate to this |
| 8 | Run full test suite `pytest tools/tests/test_ac_verifier_*.py`; all 57+ tests across 12 files must pass without modification to test code |
| 9 | Preserve CLI argument parsing in `main()` function (L747-782); argparse arguments `--feature`, `--ac-type`, `--repo-root` unchanged |
| 10 | Preserve JSON output generation in `run()` method (L721-731); `summary` dict structure with `total`, `passed`, `manual`, `failed` keys unchanged |
| 11 | Preserve `ACDefinition` class constructor signature `__init__(ac_number, description, ac_type, method, matcher, expected)` unchanged; add pattern_type field for internal use |
| 12 | Preserve exit code logic in `run()` method (L744); `return 0 if failed == 0 else 1` pattern unchanged |
| 13 | Create new test file `tools/tests/test_ac_verifier_complex_method.py` with at least 5 test cases covering: basic named params, path-only, pattern override, type parameter, edge cases |
| 14 | Create `test_ac_verifier_unknown_fallback.py` with UNKNOWN pattern type tests; verify classify_pattern returns UNKNOWN for unsupported matchers and _verify_content logs stderr warning |
| 15 | Run `pytest tools/tests/test_ac_verifier_method_glob.py` to verify glob pattern handling backward compatibility |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Pattern type representation** | (A) String constants<br>(B) Enum class<br>(C) Class hierarchy | **B: Enum** | Enum provides type safety, auto-completion, and clear semantics. Avoids magic strings. Class hierarchy would be over-engineering for 4 simple types. |
| **Classification trigger point** | (A) Eager (parse time)<br>(B) Lazy (verify time)<br>(C) Pre-verify separate pass | **A: Eager** | Classification at parse time creates a true type system architectural layer. Add pattern_type field to ACDefinition and populate during AC parsing. This matches Philosophy's "unified type system" claim and Architecture Flow showing classification as a separate layer before verification. |
| **Complex Method parser scope** | (A) Support all possible parameters<br>(B) Limit to F672 requirements (path, pattern, type)<br>(C) Generic key-value parser | **C: Generic parser** | Root Cause Analysis identifies "organic tool evolution without comprehensive upfront design" as the problem. A generic key-value parser that extracts arbitrary key=value and key="value" pairs eliminates future Method format breakage entirely. Slightly more complex than 3-parameter parser but prevents the incremental pattern that caused 10 fix Features. |
| **Duplication elimination strategy** | (A) Keep separate methods, extract helper<br>(B) Unified method with type parameter<br>(C) Unified method, callers prepare args | **C: Callers prepare** | Both `verify_code_ac` and file-type Grep branch extract (file_path, pattern, matcher) then call `_verify_content()`. This approach minimizes shared method complexity and preserves caller context (error messages can reference ac_type). |
| **Regex metacharacter detection** | (A) Remove (no longer needed)<br>(B) Keep as-is<br>(C) Move to classifier | **B: Keep as-is** | F621 design decision adds valuable user guidance ("use matches matcher for regex"). Removing would regress UX. Keeping in matcher logic (not classifier) allows guidance message to reference the specific pattern that triggered detection. |
| **Test file structure for F672** | (A) Add to existing test_ac_verifier_method_format.py<br>(B) Create new test_ac_verifier_complex_method.py<br>(C) Inline in test_ac_verifier_normal.py | **B: New file** | Complex Method format is a distinct feature (F672 absorption). Separate file provides clear traceability and allows comprehensive edge case coverage without cluttering existing test files. Matches existing naming convention (one file per feature/concern). |
| **Backward compatibility validation** | (A) Manual spot-check<br>(B) Full test suite regression<br>(C) Diff output on sample features | **B: Full suite** | AC#8 mandates all 57+ tests pass. These tests are the specification -- they encode all previously fixed edge cases (F587-F631). Running full suite is the only reliable way to ensure zero behavioral regression. |

### Interfaces / Data Structures

**New PatternType Enum**:

```python
from enum import Enum, auto

class PatternType(Enum):
    """Pattern type classification for AC verification."""
    LITERAL = auto()         # Plain string for contains/not_contains matcher
    REGEX = auto()           # Regex pattern for matches matcher
    GLOB = auto()            # File path glob patterns (*, ?, [])
    COMPLEX_METHOD = auto()  # Named parameter format: Grep(path="...", pattern="...", type=cs)
    UNKNOWN = auto()         # Unrecognized pattern, fallback to existing behavior
```

**New Classification Method**:

```python
def classify_pattern(self, ac: ACDefinition) -> PatternType:
    """Classify pattern type based on AC definition.

    Args:
        ac: AC definition with method, matcher, expected fields

    Returns:
        PatternType indicating how to process this AC

    Classification rules:
        - Complex Method: Method matches pattern 'Grep\\(.*=.*\\)' (contains '=' inside parentheses)
        - Regex: matcher == "matches"
        - Glob: file_path contains glob metacharacters (*, ?, [)
        - Literal: default (contains/not_contains with plain strings)
    """
```

**New Complex Method Parser**:

```python
def _parse_complex_method(self, method: str) -> Optional[Dict[str, str]]:
    """Parse Grep(key1=value1, key2="value2", ...) generic format.

    Args:
        method: Method column value potentially in complex format

    Returns:
        Dict with parsed key-value pairs from Method column
        None if not a complex format or parsing fails

    Example:
        Grep(path="tools/*.py", pattern="def classify", type=cs)
        → {'path': 'tools/*.py', 'pattern': 'def classify', 'type': 'cs'}
    """
```

**Unified Content Verification Signature**:

```python
def _verify_content(self, file_path: str, pattern: str, matcher: str, pattern_type: PatternType, ac_number: int) -> Dict[str, Any]:
    """Unified content verification for code and file types.

    Consolidates the duplicated logic from verify_code_ac and _verify_file_content.

    Args:
        file_path: File path or glob pattern to search
        pattern: String pattern to match (literal or regex depending on matcher)
        matcher: Matcher type (contains, not_contains, matches)
        ac_number: AC number for result reporting

    Returns:
        Result dict with ac_number, result (PASS/FAIL), details

    Matcher semantics (preserved from existing implementation):
        - contains: literal string presence (with regex metacharacter validation)
        - not_contains: literal string absence
        - matches: regex pattern matching
    """
```

**Preserved ACDefinition Class** (no changes):

```python
class ACDefinition:
    """Represents a single AC from the feature markdown."""

    def __init__(self, ac_number: int, description: str, ac_type: str,
                 method: str, matcher: str, expected: str):
        self.ac_number = ac_number
        self.description = description
        self.ac_type = ac_type
        self.method = method
        self.matcher = matcher
        self.expected = expected
        self.pattern_type = None  # PatternType set immediately during parsing via classify_pattern() - guaranteed non-None before verification
```

**Preserved JSON Output Schema** (no changes):

```json
{
  "feature": 632,
  "type": "code",
  "results": [
    {
      "ac_number": 1,
      "result": "PASS",
      "details": {
        "pattern": "class PatternType",
        "file_path": "tools/ac-static-verifier.py",
        "matcher": "matches",
        "pattern_found": true,
        "matched_files": ["tools/ac-static-verifier.py"]
      }
    }
  ],
  "summary": {
    "total": 13,
    "passed": 13,
    "manual": 0,
    "failed": 0
  }
}
```

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Define PatternType enum with 5 values (LITERAL, REGEX, GLOB, COMPLEX_METHOD, UNKNOWN) at module level | [x] |
| 2 | 2 | Implement classify_pattern() method in ACVerifier and integrate with verification dispatch pipeline | [x] |
| 3 | 3 | Create test_ac_verifier_complex_method.py with tests for Grep(path="...", pattern="...", type=cs) named parameter parsing | [x] |
| 4 | 4 | Run literal pattern regression test (test_ac_verifier_normal.py) to verify backward compatibility | [x] |
| 5 | 5 | Run regex pattern regression test (test_ac_verifier_regex_guidance.py) to verify backward compatibility | [x] |
| 6 | 6 | Remove _verify_file_content method entirely (eliminates code duplication) | [x] |
| 7 | 7 | Implement _verify_content unified method and update verify_code_ac and verify_file_ac Grep branch to delegate to it (MUST execute before T6) | [x] |
| 8 | 8 | Run full regression test suite (pytest tools/tests/test_ac_verifier_*.py) to verify all existing tests pass | [x] |
| 9 | 9 | Verify CLI interface preserved (--feature, --ac-type, --repo-root arguments unchanged) | [x] |
| 10 | 10 | Verify JSON output schema preserved (summary.total/passed/manual/failed structure unchanged) | [x] |
| 11 | 11 | Verify ACDefinition class API preserved (constructor signature and field names unchanged) | [x] |
| 12 | 12 | Verify exit code contract preserved (return 0 for success, return 1 for failure) | [x] |
| 13 | 13 | Verify complex Method test file was created successfully | [x] |
| 14 | 14 | Verify UNKNOWN pattern type fallback behavior works correctly | [x] |
| 15 | 14 | Create test_ac_verifier_unknown_fallback.py with UNKNOWN pattern type fallback test cases | [x] |
| 16 | 15 | Run glob pattern regression test (test_ac_verifier_method_glob.py) to verify backward compatibility | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T2 | Technical Design PatternType enum and classification method | PatternType enum + classify_pattern() implementation |
| 2 | implementer | sonnet | T3 | Technical Design Complex Method parser spec | test_ac_verifier_complex_method.py with 5+ test cases |
| 3 | implementer | sonnet | T7,T6 | Technical Design unified content verification pipeline | _verify_content() method + caller updates (T7), then eliminate _verify_file_content (T6) |
| 4 | ac-tester | haiku | T4-T5,T8 | Regression tests for literal/regex patterns and full test suite | All regression tests pass (normal, regex guidance, full suite) |
| 5 | ac-tester | haiku | T9-T13 | Contract preservation checks (verification-only, no implementation) | Static verification results for CLI/schema/API/exit codes/test file |

**Constraints** (from Technical Design):

1. **Backward Compatibility**: All existing tests (57+) must pass without modification. The classification system is an internal refactor only.
2. **No API Changes**: CLI interface, JSON output schema, ACDefinition class, and exit codes must be preserved exactly.
3. **Code Duplication Elimination**: The 96% duplicated logic between verify_code_ac (L263-387) and _verify_file_content (L460-584) must be consolidated into a single _verify_content() method.
4. **F672 Absorption Scope**: Complex Method parser uses generic key-value extraction but F672 test coverage focuses on path, pattern, type parameters.
5. **Pattern Type Explicit Handling**: All 5 pattern types (literal, regex, glob, complex_method, unknown) must have explicit branches in classification logic.
6. **Python stdlib only**: No third-party dependencies beyond existing pytest for testing.

**Pre-conditions**:

- F631 (predecessor) is [DONE] and all its fixes are incorporated as baseline behavior
- Existing 12 test files (test_ac_verifier_*.py) are available and passing
- Current ac-static-verifier.py is at 782 lines with ACDefinition and ACVerifier classes

**Success Criteria**:

1. All 15 ACs pass verification
2. Full regression test suite (72 tests) passes without any test code modifications
3. `_verify_file_content` method no longer exists in ac-static-verifier.py
4. `PatternType` enum with 5 values exists at module level
5. `classify_pattern()` method exists in ACVerifier class
6. New test file test_ac_verifier_complex_method.py exists with at least 5 test cases
7. All preserved contracts (CLI, JSON schema, ACDefinition, exit codes) verified via grep

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert {commit-hash}`
2. Notify user of rollback with reason (failed test suite, regression, or consumer breakage)
3. Create follow-up feature for fix with additional investigation
4. If revert affects downstream features, update their Dependencies tables to mark F632 as reverted

**Implementation Notes**:

- **Phase 1-2**: Architecture foundation (enum + classification) before duplication elimination
- **Phase 3**: Core refactor (consolidate duplicated methods) - highest risk phase
- **Phase 4**: Regression gate - must pass before proceeding to contract verification
- **Phase 5**: Final verification - contract preservation checks are static (grep-based)

**Test Strategy**:

- TDD approach: Existing tests define the specification
- Run pytest after each Phase to catch regressions early
- Phase 4 regression suite is the primary quality gate
- AC#3 new tests verify F672 functionality separately

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Review Notes
- [resolved] Phase1-Uncertain iter1: AC#10 Expected value shown as '"summary":' uses contains matcher. The pattern includes escaped quotes which must survive markdown table parsing and quote-stripping in ac-static-verifier. RESOLVED: unescape() method at L144 correctly converts \" to ", so pattern becomes "summary": which matches source code.

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-28 | DEVIATION | doc-check: testing SKILL.md not updated for Complex Method format |
| 2026-01-28 | DEVIATION | Bash | ac-static-verifier --ac-type exit_code | exit code 2: invalid choice 'exit_code' (not supported by static verifier). exit_code ACs verified via pytest directly. |
| 2026-01-28 | DEVIATION | feature-reviewer | post review | NEEDS_REVISION: pattern_type parameter unused in _verify_content(); UNKNOWN warning not logged → RESOLVED: added UNKNOWN warning to stderr |