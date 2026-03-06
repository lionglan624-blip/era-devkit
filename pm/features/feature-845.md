# Feature 845: Full AC pattern catalog scan across all features

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T13:02:49Z -->

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
ac-static-verifier parser was built incrementally across F818, F832, F834, F838, F842, each addressing specific use cases. No validation exists that the parser handles the union of all AC pattern syntaxes across 275+ feature files.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | N/A (gap analysis) |
| Exit Code | N/A |
| Error Output | N/A |
| Expected | Parser validated against all existing AC patterns |
| Actual | Each feature only validated its own AC patterns |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Parser code |
| pm/features/ | 275+ feature files with AC definitions |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | - |

### Parent Session Observations
Run ac-static-verifier across all 275+ features to discover any remaining pattern parsing gaps. This validates the Philosophy claim that the verifier handles "the full range of regex syntax that AC authors use." Should produce a catalog of all unique AC pattern syntaxes encountered.

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)
ac-static-verifier is the SSOT for automated AC verification. Incremental parser extensions must be validated against the union of all existing AC patterns to prevent regression gaps. A batch scan mode provides the validation surface to measure and report parser completeness across the full feature catalog, enabling targeted fixes.

### Problem (Current Issue)
The ac-static-verifier was architecturally designed as a single-feature verification tool (`--feature {ID}` required, `ac-static-verifier.py:1392-1396`). Because each enhancement (F818, F832, F834, F838, F841, F842) only addressed patterns found during individual feature execution, no validation step exists to confirm the parser handles the union of all AC pattern syntaxes across the feature catalog. Since there is no batch/scan-all mode and no CI job running the verifier across all features, regressions in parser coverage are invisible until a specific feature fails during `/run`. Additionally, approximately 78 older features (F085-F255 range) use a legacy 6-column AC table format without the Method column, which would cause silent misparse due to hard-indexed column extraction at `ac-static-verifier.py:989-993`.

### Goal (What to Achieve)
Add a `--scan-all` batch mode with `--parse-only` capability to ac-static-verifier that scans all feature files, reports AC pattern type distribution and matcher distribution, and reports parse failures without executing build commands. Output a machine-readable JSON catalog of pattern type counts, matcher counts, per-AC-type counts, and categorized parse errors.

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is there no validation that the parser handles all AC pattern syntaxes? | Because the verifier only operates on a single feature at a time via `--feature {ID}` | `ac-static-verifier.py:1392-1396` |
| 2 | Why is it single-feature only? | Because it was built as a verification tool for the `/run` workflow, not as a linter or catalog tool | `ac-static-verifier.py:1335-1385` (run method processes single file) |
| 3 | Why were individual fixes sufficient until now? | Because each enhancement (F818/F832/F834/F838/F841/F842) focused on fixing specific failures found during individual feature execution | Feature history chain |
| 4 | Why are regressions on other features invisible? | Because there is no CI or scheduled job that runs the verifier across all features | No batch CLI option exists |
| 5 | Why (Root)? | The tool's architecture assumes single-feature invocation; no batch scan mode or catalog output was designed | `ac-static-verifier.py:1388-1428` (main function, no --scan-all) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Unknown parser coverage gaps across 275+ features | Single-feature architecture with no batch validation mode |
| Where | Individual feature failures during `/run` | CLI design (`--feature` required, no `--scan-all`) |
| Fix | Manually test each feature one at a time | Add `--scan-all --parse-only` batch mode with JSON catalog output |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F842 | [DONE] | Direct predecessor -- pattern parsing enhancements |
| F844 | [DONE] | Sibling -- `_UNESCAPE_RULES` catch-all refactoring |
| F841 | [DONE] | Cross-repo build CWD resolution in verifier |
| F838 | [DONE] | Cross-repo verifier path resolution |
| F834 | [DONE] | Format C guard DRY consolidation |
| F832 | [DONE] | Earlier verifier pattern work |
| F818 | [DONE] | Initial ac-static-verifier creation |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Parser maturity | FEASIBLE | 6 PatternTypes handled, 16 unescape rules, 3 count formats (`ac-static-verifier.py:109-134`) |
| Batch mode addition | FEASIBLE | `run()` method is self-contained; batch wrapper iterates over feature files |
| Parse-only mode | FEASIBLE | Verification dispatch is cleanly separated from parsing (`ac-static-verifier.py:1320-1333`) |
| Legacy format handling | FEASIBLE | Header detection at `ac-static-verifier.py:919` can be extended for format auto-detection |
| Test infrastructure | FEASIBLE | 29 existing test files provide regression safety net |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Parser reliability | HIGH | Batch scan reveals unknown parse failures across full catalog |
| AC workflow | MEDIUM | Catalog output enables proactive pattern gap detection before `/run` |
| Future verifier changes | MEDIUM | Batch scan serves as regression test for parser enhancements |
| Existing single-feature mode | LOW | Must not alter existing behavior |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Only code/build/file AC types are verifiable | `ac-static-verifier.py:1400-1401` | output/variable/test/exit_code types excluded from catalog |
| Legacy 6-column AC format (~78 features) | Features in F085-F255 range | Column index mismatch at `ac-static-verifier.py:977-999` |
| Build ACs execute real commands via WSL | `ac-static-verifier.py:1070-1167` | Full scan would be slow and side-effectful without parse-only mode |
| Python glob does not support brace expansion | stdlib limitation | Patterns like `{SelectCaseNode,CaseBranch}.cs` will not expand natively |
| ac-lint-patterns validates only Expected column | `ac_ops.py:2292-2378` | Complementary but non-overlapping with verifier's Method column parsing |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Silent misparse of old-format features | HIGH | HIGH | Header format auto-detection to skip or correctly parse legacy tables |
| Build AC execution during scan causes long runtime | MEDIUM | MEDIUM | `--parse-only` flag skips verification dispatch entirely |
| Batch scan exposes many parser failures requiring triage | MEDIUM | MEDIUM | Catalog output categorizes failures by type for prioritized fixing |
| Edge case patterns break existing parser (brace expansion, dual-Grep, compound matchers) | LOW | HIGH | Run full existing test suite after changes; edge cases documented in F844 |
| Cross-repo file ACs fail if environment variables not set | MEDIUM | LOW | Catalog flags environment-dependent ACs separately |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Total feature files | `ls pm/features/feature-*.md \| wc -l` | ~284 | Approximate; exact count at scan time |
| code\|matches occurrences | `grep -c 'code\|matches' pm/features/feature-*.md` | ~908 across 64 features | Explorer 2 data |
| count matcher occurrences | `grep -c 'count_equals\|gte\|lte' pm/features/feature-*.md` | ~478 across 71 features | Explorer 2 data |
| Verifier AC types supported | CLI choices | 3 (code, build, file) | `ac-static-verifier.py:1400` |

**Baseline File**: `_out/tmp/baseline-845.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Batch mode must not alter existing single-feature behavior | CLI backward compatibility | Test both single-feature and batch modes; existing tests must pass |
| C2 | Old-format features must be explicitly handled | ~78 legacy features (F085-F255) | Verify skip or correct parse with format auto-detection |
| C3 | Catalog output must be machine-readable JSON | Consistency with tooling | Verify JSON schema of scan output |
| C4 | Parse-only mode must not execute builds | `verify_build_ac` runs subprocess | No build command execution in catalog scan |
| C5 | Scan must differentiate parse failures from verification failures | Catalog usability | Distinct error categories in output JSON |
| C6 | Catalog must cover all 3 static AC types | code/build/file verifier scope | Separate counts per type in catalog output |

### Constraint Details

**C1: Backward Compatibility**
- **Source**: Existing CLI interface (`--feature` + `--ac-type` required)
- **Verification**: Run existing 29 test files after changes
- **AC Impact**: ACs must verify that single-feature mode still works identically

**C2: Legacy Format Handling**
- **Source**: Investigation found ~78 features using 6-column format without Method column
- **Verification**: Parse `feature-175.md` and `feature-087.md` to confirm format difference
- **AC Impact**: ACs must verify that legacy features are either correctly parsed or explicitly skipped with report entry
- **Collection Members**: Two known legacy formats: (1) 6-column without Method column (F085-F255 range), (2) modern 7-column with Method column (F268+)

**C3: JSON Catalog Output**
- **Source**: Tooling consistency; machine-readable output enables downstream automation
- **Verification**: Validate scan output against expected JSON structure
- **AC Impact**: ACs must verify JSON output contains pattern type distribution, matcher counts, and error list

**C4: Parse-Only Mode**
- **Source**: Build ACs invoke `dotnet build` via WSL subprocess (`ac-static-verifier.py:1070-1167`)
- **Verification**: Run scan with `--parse-only` and confirm no subprocess calls
- **AC Impact**: ACs must verify build commands are not executed during parse-only scan

**C5: Error Categorization**
- **Source**: Parse failures (malformed table) vs verification failures (pattern mismatch) require different triage
- **Verification**: Introduce intentionally malformed AC and confirm categorization
- **AC Impact**: ACs must verify distinct error types in catalog JSON

**C6: AC Type Coverage**
- **Source**: Verifier dispatches to `verify_code_ac`, `verify_build_ac`, `verify_file_ac`
- **Verification**: Catalog must enumerate ACs of each type separately
- **AC Impact**: ACs must verify per-type counts in catalog output

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F842 | [DONE] | Pattern parsing enhancements (completed; no blocking dependency) |
| Related | F844 | [DONE] | `_UNESCAPE_RULES` catch-all refactoring (independent scope) |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | INFORMATIONAL | F{ID} depends on this feature. |
| Related | F{ID} <-> This | INFORMATIONAL | Related work, no blocking dependency. |
-->

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "SSOT for automated AC verification" | Verifier must handle all AC patterns in the catalog, not just individually tested ones | AC#7, AC#8 |
| "must be validated against the union of all existing AC patterns" | Batch scan mode that iterates all feature files | AC#1, AC#2, AC#3, AC#4, AC#9 |
| "parser completeness across the full feature catalog" | Scan output reports per-feature parse results covering all features | AC#5, AC#6, AC#10, AC#12, AC#14, AC#15, AC#16 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --scan-all CLI flag accepted | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `--scan-all` | [x] |
| 2 | --parse-only CLI flag accepted | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `--parse-only` | [x] |
| 3 | Scan iterates all feature files via glob | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `glob.*feature-` | [x] |
| 4 | Error categorization distinguishes parse vs verification errors | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `parse_error|format_error` | [x] |
| 5 | JSON catalog output contains pattern_types key | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `pattern_types` | [x] |
| 6 | JSON catalog output contains matcher_distribution key | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `matcher_distribution` | [x] |
| 7 | JSON catalog output contains parse_errors list | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `parse_errors` | [x] |
| 8 | Legacy format detection for non-Method AC tables | code | Grep(src/tools/python/ac-static-verifier.py) | matches | `detect_legacy|is_legacy` | [x] |
| 9 | Parse-only mode skips verification dispatch | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `parse_only` | [x] |
| 10 | Scan output contains per-type AC counts | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `per_type` | [x] |
| 11 | Backward compatibility: --feature without --ac-type fails | exit_code | python src/tools/python/ac-static-verifier.py --feature 845 | fails | - | [x] |
| 12 | Scan-all with parse-only exits 0 on success | exit_code | python src/tools/python/ac-static-verifier.py --scan-all --parse-only --repo-root C:/Era/devkit | equals | 0 | [x] |
| 13 | Scan JSON output is valid JSON | exit_code | python src/tools/python/ac-static-verifier.py --scan-all --parse-only --repo-root C:/Era/devkit --output _out/tmp/scan-845.json && python -c "import json; json.load(open('_out/tmp/scan-845.json'))" | equals | 0 | [x] |
| 14 | Scan catalogs features with AC tables | code | Grep(src/tools/python/ac-static-verifier.py) | contains | `features_scanned` | [x] |
| 15 | Scan JSON output contains meaningful content | exit_code | python src/tools/python/ac-static-verifier.py --scan-all --parse-only --repo-root C:/Era/devkit --output _out/tmp/scan-845.json && python -c "import json; d=json.load(open('_out/tmp/scan-845.json')); assert d['features_scanned']>200; assert len(d['pattern_types'])>0; assert len(d['matcher_distribution'])>0; assert sum(d['per_type'].values())>0; assert sum(d['pattern_types'].values())>=sum(d['per_type'].values()); print('Content valid')" | equals | 0 | [x] |
| 16 | Parse errors have valid categorized structure | exit_code | python -c "import json; d=json.load(open('_out/tmp/scan-845.json')); errs=d['parse_errors']; assert len(errs)>0; assert all(e['error'] in ('parse_error','format_error') for e in errs); assert all('feature' in e and 'detail' in e for e in errs); assert any(e['error']=='format_error' for e in errs); print(f'All {len(errs)} errors categorized')" | equals | 0 | [x] |
| 17 | Single-feature mode works after refactoring | exit_code | python src/tools/python/ac-static-verifier.py --feature 845 --ac-type code --repo-root C:/Era/devkit | equals | 0 | [x] |

### AC Details

**AC#4: Error categorization distinguishes parse vs verification errors**
- **Test**: Grep for `parse_error|format_error` in the verifier source
- **Expected**: Catalog JSON uses distinct error category keys (e.g., `parse_error` for malformed tables, `format_error` for legacy format issues) to enable prioritized triage
- **Rationale**: C5 constraint requires distinct error categories in output JSON. Parse failures (malformed table) vs format issues (legacy 6-column) require different triage approaches

**AC#10: Scan output contains per-type AC counts**
- **Test**: Grep for `per_type` key in the JSON output construction
- **Expected**: JSON output includes counts for each of the 3 static AC types (code, build, file)
- **Rationale**: C6 constraint requires separate counts per AC type. Derivation: 3 types from `ac-static-verifier.py:1400` choices list

**AC#12: Scan-all with parse-only exits 0 on success**
- **Test**: Run the verifier in scan-all parse-only mode against the actual feature catalog
- **Expected**: Exit code 0 indicating successful parse of all feature files (parse failures reported in JSON, not as exit failure)
- **Rationale**: C4 constraint requires no build execution; C5 requires parse failures in output not as crash. This is the integration test that validates Philosophy claim of "parser completeness across the full feature catalog"

**AC#13: Scan JSON output is valid JSON**
- **Test**: Run scan and validate output with Python json.load
- **Expected**: Exit code 0 from json.load validation
- **Rationale**: C3 constraint requires machine-readable JSON output

**AC#15: Scan JSON output contains meaningful content**
- **Test**: Run scan-all parse-only, load JSON, assert features_scanned > 200, pattern_types non-empty, matcher_distribution non-empty, per_type total > 0, and pattern_types total >= per_type total (documented invariant: non-static types in pattern_types but not per_type)
- **Expected**: Exit code 0 from assertion validation
- **Rationale**: Validates Philosophy claim "parser completeness across the full feature catalog" by verifying the scan actually processed features and extracted pattern types, matcher distributions, and per-type AC counts. AC#12 only checks exit code; AC#13 only checks JSON syntax; AC#15 validates semantic content including matcher_distribution (Goal item 8), per_type (Goal item 12), and the pattern_types >= per_type invariant

**AC#16: Parse errors have valid categorized structure**
- **Test**: Load scan JSON, assert parse_errors is non-empty (≥1 entry expected from ~78 legacy features), all entries have error type in (parse_error, format_error), contain feature + detail fields, and at least one format_error entry exists (legacy detection fires)
- **Expected**: Exit code 0 from assertion validation
- **Rationale**: Validates C5 constraint (error categorization) at runtime. Non-empty assertion guards against silent legacy detection failure. format_error presence assertion verifies Task#2 (detect_legacy_format) actually works at runtime against real legacy features. Complements AC#4 (code presence) and AC#8 (legacy detection code) with runtime behavioral validation

**AC#17: Single-feature mode works after refactoring**
- **Test**: Run verifier in single-feature mode with valid args (--feature 845 --ac-type code)
- **Expected**: Exit code 0
- **Rationale**: Validates C1 constraint (backward compatibility) success path. AC#11 only tests the failure case (missing --ac-type). AC#17 verifies that the parse_ac_table() refactoring does not break the single-feature code path end-to-end

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Add `--scan-all` batch mode | AC#1, AC#3, AC#12 |
| 2 | Add `--parse-only` capability | AC#2, AC#9 |
| 3 | Scan all feature files | AC#3, AC#14 |
| 4 | Report AC pattern type distribution | AC#5 |
| 5 | Report parse failures without executing build commands | AC#7, AC#9 |
| 6 | Machine-readable JSON catalog output | AC#13, AC#15 |
| 7 | Pattern type distribution in output | AC#5 |
| 8 | Matcher distribution in output | AC#6 |
| 9 | Parse error list in output | AC#7 |
| 10 | Constraint C2: Legacy format handling | AC#8 |
| 11 | Constraint C1: Backward compatibility | AC#11, AC#17 |
| 12 | Constraint C6: Per-type AC counts | AC#10 |
| 13 | Constraint C5: Error categorization | AC#4, AC#16 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Extract the AC table parsing logic from `ACVerifier.parse_feature_markdown()` into a standalone module-level function `parse_ac_table(feature_path: Path) -> tuple[list[dict], list[dict]]` that returns `(ac_list, errors)` without filtering by ac_type. This function parses ALL AC rows regardless of type (code/build/file), which is required because `parse_feature_markdown()` filters by `self.ac_type` at line 999 — a dummy ACVerifier would only collect one type per pass. Both `ACVerifier.parse_feature_markdown()` and `CatalogScanner._scan_feature_file()` call this shared function. `ACVerifier` applies its ac_type filter after calling `parse_ac_table()`; `CatalogScanner` uses the unfiltered results.

Add a `CatalogScanner` class to `ac-static-verifier.py` that implements `--scan-all --parse-only` batch mode. The scanner calls `parse_ac_table()` per feature file, wraps it in a glob-based iteration loop, accumulates catalog statistics, and writes a JSON catalog to `--output` (or stdout if omitted). Verification dispatch (`verify_code_ac`, `verify_build_ac`, `verify_file_ac`) is never called in parse-only mode — the scanner only exercises the parse path.

Define module-level constants: `STATIC_AC_TYPES = ['code', 'build', 'file']` referenced by both argparse choices and `CatalogScanner`'s `per_type` initialization, and `ERROR_CATEGORIES = ['parse_error', 'format_error']` referenced when creating error entries. Both ensure new values are added in one place (SSOT).

Extract `classify_pattern_type(method, matcher, expected) -> str` as a standalone module-level function from `ACVerifier.classify_pattern()`. `ACVerifier.classify_pattern(ac)` is then refactored to delegate: `classify_pattern_type(ac.method, ac.matcher, ac.expected_value)`, eliminating duplicated classification logic. `parse_ac_table()` calls `classify_pattern_type()` directly with the raw field strings.

Extract `unescape()` (and `unescape_for_regex_pattern`, `unescape_for_literal_search` if referenced by `classify_pattern_type`) as module-level functions from `ACVerifier` `@staticmethod` methods. `ACVerifier`'s versions become thin delegations to the module-level functions, matching the `classify_pattern_type` extraction pattern. `parse_ac_table()` calls the module-level `unescape()` when constructing `ACDefinition` objects from raw table fields.

`parse_ac_table()` contains the full pipe-splitting state machine (currently lines 930-973 of `parse_feature_markdown()`). `ACVerifier.parse_feature_markdown()` becomes a thin wrapper: calls `parse_ac_table(self.feature_file)`, filters results by `self.ac_type`, and returns the filtered list. The pipe-splitting logic exists in exactly one place (`parse_ac_table`).

The `main()` function is refactored to branch on `--scan-all`: when present, it instantiates `CatalogScanner` and calls `scanner.run()`; when absent, it falls through to the existing `ACVerifier` single-feature path unchanged. A guard ensures either `--scan-all` or `--feature` is provided: `if not args.scan_all and not args.feature: parser.error('Either --scan-all or --feature is required')`. The `--ac-type` requirement is enforced via manual guard for single-feature path only, satisfying backward compatibility (AC#11).

Legacy format detection is added to `parse_feature_markdown()` (or a thin wrapper): if the `| AC# |` header line does not contain the word `Method`, the table is classified as legacy format and all its rows are recorded as `format_error` entries in the catalog instead of parsed AC rows. This satisfies AC#8 (detect_legacy / is_legacy) and C2.

**JSON catalog structure** (AC#5, AC#6, AC#7, AC#10, AC#14):
```json
{
  "features_scanned": 284,
  "pattern_types": {"REGEX": 420, "LITERAL": 88, "GLOB": 60, "COUNT": 120, "COMPLEX_METHOD": 30, "UNKNOWN": 5},
  "matcher_distribution": {"matches": 400, "not_matches": 20, "contains": 80, "gte": 90, ...},
  "per_type": {"code": 550, "build": 80, "file": 90},
  "parse_errors": [
    {"feature": "123", "ac_number": 3, "error": "parse_error", "detail": "..."},
    {"feature": "087", "ac_number": null, "error": "format_error", "detail": "AC table missing Method column"}
  ]
}
```

Error categories:
- `parse_error`: row parsing failed (malformed pipe structure, non-integer AC#)
- `format_error`: AC table header detected but lacks Method column (legacy 6-column format)

This satisfies AC#4 (parse_error|format_error distinct keys), AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#14.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `--scan-all` argument to `argparse.ArgumentParser` in `main()` |
| 2 | Add `--parse-only` argument to `argparse.ArgumentParser` in `main()` |
| 3 | `CatalogScanner.run()` uses `glob.glob(str(repo_root / "pm" / "features" / "feature-*.md"))` to iterate files |
| 4 | JSON catalog uses distinct keys `parse_error` and `format_error` for error entries |
| 5 | JSON output dict contains key `pattern_types` built by accumulation in `CatalogScanner` |
| 6 | JSON output dict contains key `matcher_distribution` built by accumulation in `CatalogScanner` |
| 7 | JSON output dict contains key `parse_errors` list (includes both `parse_error` and `format_error` entries) |
| 8 | `parse_feature_markdown()` extended with legacy detection: checks `| AC# |` header line for absence of `Method` word; sets `is_legacy = True` / calls `detect_legacy()` helper |
| 9 | `CatalogScanner` is always parse-only by design (no `verify_ac()` dispatch); `parse_only` flag retained in CLI for explicit intent but `CatalogScanner` never calls verification |
| 10 | JSON output dict contains key `per_type` with counts for `code`, `build`, `file` |
| 11 | `--feature` moved to mutually exclusive group (with `--scan-all`); `--ac-type` `required=True` replaced with manual guard that errors when `--feature` is used without `--ac-type`. Single-feature path behavior unchanged |
| 12 | `CatalogScanner.run()` returns exit code 0 on successful scan (parse errors go into JSON, not exit-1) |
| 13 | JSON written by `CatalogScanner` is produced via `json.dump()` with valid structure |
| 14 | JSON output dict contains key `features_scanned` (integer count of feature files processed) |
| 15 | Run `--scan-all --parse-only` integration test: assert `features_scanned > 200`, `len(pattern_types) > 0`, `len(matcher_distribution) > 0`, and `sum(per_type.values()) > 0` in JSON output |
| 16 | Run parse_errors structure validation: assert all entries have valid `error` type (`parse_error` or `format_error`) and required fields (`feature`, `detail`), with at least one `format_error` entry |
| 17 | Run single-feature mode with valid args (`--feature 845 --ac-type code`) and verify exit 0 after `parse_ac_table()` refactoring |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Scan-all as new class vs method on ACVerifier | A: method on ACVerifier, B: standalone CatalogScanner class | B: standalone CatalogScanner class | ACVerifier requires feature_id and ac_type at construction; CatalogScanner has different lifecycle (no feature_id, no ac_type filter). Single-responsibility avoids retrofitting scan logic into single-feature class. |
| Legacy format detection location | A: inside parse_feature_markdown(), B: separate detect_legacy_format() helper | B: separate helper `detect_legacy_format(header_line)` called from parse_feature_markdown() and from CatalogScanner | Keeps parse_feature_markdown() clean; the helper returns bool and is referenced by name in the source satisfying AC#8 `detect_legacy\|is_legacy` pattern |
| parse_errors list scope | A: only parse_error (malformed rows), B: both parse_error and format_error | B: unified parse_errors list with typed entries | AC#7 requires a single `parse_errors` list; AC#4 requires distinct error categories — use a `"error"` field inside each entry to hold `parse_error` or `format_error` |
| --scan-all with --feature | A: mutually exclusive, B: --scan-all ignores --feature, C: error if both | A: mutually exclusive via argparse `add_mutually_exclusive_group` | Prevents ambiguity; aligns with AC#11 which verifies `--feature` without `--ac-type` still fails |
| Output destination for catalog | A: stdout only, B: --output path, C: fixed path | B: `--output` optional arg, stdout if omitted | Machine-readable downstream automation (C3) benefits from file output; stdout fallback keeps ad-hoc usage simple |
| Exit code when parse errors found | A: exit 1 if any parse error, B: exit 0, errors in JSON | B: exit 0, all errors cataloged in JSON | AC#12 explicitly requires exit 0 on successful scan; C5 says errors are categorized in output, not as crash |

### Interfaces / Data Structures

```python
class CatalogScanner:
    """Batch scanner for all feature files. Implements --scan-all --parse-only mode."""

    def __init__(self, repo_root: Path,
                 output_path: Optional[Path] = None, verbose: bool = False):
        self.repo_root = repo_root
        self.output_path = output_path     # AC#13: JSON written here or stdout
        self.verbose = verbose
        # AC#9: CatalogScanner is always parse-only (no verify_ac dispatch).
        # Verification dispatch is ACVerifier's responsibility.

    def run(self) -> int:
        """Scan all feature files and produce JSON catalog. Returns exit code (0 = success)."""
        ...

    def _scan_feature_file(self, feature_path: Path) -> dict:
        """Parse a single feature file via parse_ac_table().

        Extracts feature ID from filename path, calls parse_ac_table(),
        and returns {'feature_id': str, 'ac_list': [...], 'errors': [...]}
        with feature ID injected into each error entry.
        """
        ...


# Module-level constants: single source for AC types and error categories
STATIC_AC_TYPES = ['code', 'build', 'file']
ERROR_CATEGORIES = ['parse_error', 'format_error']


def unescape(value: str) -> str:
    """Unescape pipe-escaped markdown table cell value.

    Extracted from ACVerifier.unescape() @staticmethod.
    Used by parse_ac_table() when constructing ACDefinition from raw table fields.
    """
    ...


def unescape_for_regex_pattern(value: str) -> str:
    """Unescape value for regex pattern matching (applies _UNESCAPE_RULES).

    Extracted from ACVerifier.unescape_for_regex_pattern() @staticmethod.
    """
    ...


def unescape_for_literal_search(value: str) -> str:
    """Unescape value for literal string search.

    Extracted from ACVerifier.unescape_for_literal_search() @staticmethod.
    """
    ...


def classify_pattern_type(method: str, matcher: str, expected: str) -> 'PatternType':
    """Classify an AC's pattern type from its Method/Matcher/Expected fields.

    Extracted from ACVerifier.classify_pattern() for use by both ACVerifier and
    CatalogScanner. Returns PatternType enum value. CatalogScanner uses
    PatternType.name as the dict key for JSON serialization.
    """
    ...


def parse_ac_table(feature_path: Path) -> tuple[list[dict], list[dict]]:
    """Extract all AC rows from a feature file without filtering by ac_type.

    Returns (ac_list, errors) where ac_list contains ACDefinition objects
    (reusing the existing dataclass — no parallel dict representation).
    Each ACDefinition has pattern_type set via classify_pattern_type().

    Used by both ACVerifier (post-filters by ac_type) and CatalogScanner (uses unfiltered).
    """
    ...


def detect_legacy_format(header_line: str) -> bool:
    """Return True if AC table header lacks 'Method' column (legacy 6-column format).

    AC#8: referenced by name to match `detect_legacy|is_legacy` grep pattern.
    """
    return "Method" not in header_line
```

**JSON catalog schema** (output from `CatalogScanner.run()`):
```python
{
    "features_scanned": int,           # AC#14
    "pattern_types": {                 # AC#5: PatternType name -> count
        "REGEX": int,
        "LITERAL": int,
        "GLOB": int,
        "COUNT": int,
        "COMPLEX_METHOD": int,
        "UNKNOWN": int,
    },
    "matcher_distribution": {          # AC#6: matcher string -> count
        "matches": int,
        "not_matches": int,
        # ... all matchers encountered
    },
    "per_type": {                      # AC#10: static AC types only (code/build/file)
        "code": int,                   # Note: sum(per_type) may be < sum(pattern_types)
        "build": int,                  # because non-static types (exit_code, output, etc.)
        "file": int,                   # are included in pattern_types but not per_type
    },
    "parse_errors": [                  # AC#7: unified error list
        {
            "feature": str,            # feature ID or filename
            "ac_number": Optional[int],
            "error": str,              # "parse_error" | "format_error"  (AC#4)
            "detail": str,
        }
    ],
}
```

**CLI extension** (added to existing `main()`):
```python
# Mutually exclusive: --scan-all vs --feature
group = parser.add_mutually_exclusive_group()
group.add_argument("--scan-all", action="store_true",   # AC#1
                   help="Scan all feature files and produce catalog")
group.add_argument("--feature", help="Feature ID (e.g., 268)")

parser.add_argument("--parse-only", action="store_true",  # AC#2
                    help="Parse only; skip verification dispatch (use with --scan-all)")
parser.add_argument("--output",
                    help="Output path for JSON catalog (--scan-all only; default: stdout)")

# Guards (after arg parsing)
if not args.scan_all and not args.feature:
    parser.error('Either --scan-all or --feature is required')
if not args.scan_all and args.feature and not args.ac_type:
    parser.error('--ac-type is required when --feature is used')
if args.parse_only and not args.scan_all:
    parser.error('--parse-only requires --scan-all')
if args.scan_all and not args.parse_only:
    parser.error('--scan-all requires --parse-only')
if args.output and not args.scan_all:
    parser.error('--output requires --scan-all')
```

**Glob pattern** (satisfies AC#3 `glob.*feature-`):
```python
import glob as glob_module
feature_files = sorted(glob_module.glob(
    str(self.repo_root / "pm" / "features" / "feature-*.md")
))
```

### Upstream Issues

<!-- No upstream issues identified during Technical Design. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2 | Add `--scan-all` and `--parse-only` CLI arguments to `argparse.ArgumentParser` in `main()` via `add_mutually_exclusive_group` for `--scan-all`/`--feature`, and standalone `--parse-only` and `--output` args | | [x] |
| 2 | 8, 17 | Add module-level helpers: `detect_legacy_format(header_line) -> bool`, `classify_pattern_type(method, matcher, expected) -> PatternType` (extracted from `ACVerifier.classify_pattern()`), `parse_ac_table(feature_path) -> tuple[list, list]` (extracts all ACs with pattern_type classification, without ac_type filtering), and `STATIC_AC_TYPES` constant. Refactor `ACVerifier.parse_feature_markdown()` to call `parse_ac_table()` then filter by `self.ac_type` | | [x] |
| 3 | 3, 4, 5, 6, 7, 9, 10, 14 | Implement `CatalogScanner` class: `_scan_feature_file()` calls `parse_ac_table()`, `run()` uses `glob.glob` to iterate `feature-*.md` files, accumulates `features_scanned`, `pattern_types`, `matcher_distribution`, `per_type` counts (initialized from `STATIC_AC_TYPES`), and a `parse_errors` list with typed entries (`parse_error` / `format_error`); skips verification dispatch when `parse_only=True` | | [x] |
| 4 | 11, 12, 13, 15, 16, 17 | Wire `main()` to branch on `--scan-all` flag: instantiate `CatalogScanner` and call `scanner.run()`, write JSON catalog to `--output` path or stdout, exit 0 on successful scan (parse errors go into JSON, not exit 1); single-feature path unchanged so `--feature` without `--ac-type` still fails | | [x] |

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
| 1 | implementer | sonnet | feature-845.md Tasks + Technical Design | `detect_legacy_format()`, `unescape()` (+ helpers), `classify_pattern_type()`, `parse_ac_table()`, `STATIC_AC_TYPES`, and `ERROR_CATEGORIES` added as module-level to `ac-static-verifier.py`; `ACVerifier` `@staticmethod` unescape methods and `classify_pattern()` become delegations; `ACVerifier.parse_feature_markdown()` refactored to call `parse_ac_table()` |
| 2 | implementer | sonnet | Phase 1 output + Interfaces/Data Structures section | `CatalogScanner` class implemented using `parse_ac_table()` and `STATIC_AC_TYPES` |
| 3 | implementer | sonnet | Phase 2 output + CLI extension spec | `main()` wired to branch on `--scan-all` with guards for no-argument and --feature-without-ac-type cases |
| 4 | tester | sonnet | All AC definitions + updated `ac-static-verifier.py` | All 17 ACs verified PASS |

### Pre-conditions

- `ac-static-verifier.py` exists at `src/tools/python/ac-static-verifier.py`
- `pm/features/feature-*.md` files exist (284+ at time of writing)
- Existing 29 test files in `src/tools/python/tests/` pass before modifications begin
- `_out/tmp/` directory exists (create if not present)

### Execution Order

**Step 1: Add module-level helpers and extract parsing logic (Task 2)**

Add top-level functions and constant (before `ACVerifier` class) to `ac-static-verifier.py`:

```python
# Module-level constants
STATIC_AC_TYPES = ['code', 'build', 'file']
ERROR_CATEGORIES = ['parse_error', 'format_error']


def detect_legacy_format(header_line: str) -> bool:
    """Return True if AC table header lacks 'Method' column (legacy 6-column format).

    AC#8: referenced by name to match `detect_legacy|is_legacy` grep pattern.
    """
    return "Method" not in header_line


def parse_ac_table(feature_path: Path) -> tuple[list['ACDefinition'], list[dict]]:
    """Extract all AC rows from a feature file without filtering by ac_type.

    Returns (ac_list, errors) where ac_list contains ACDefinition objects
    (reusing the existing dataclass to avoid parallel data representations).
    Called by both ACVerifier and CatalogScanner.
    Uses detect_legacy_format() to classify legacy tables as format_error.
    Each ACDefinition has pattern_type set via classify_pattern_type().
    """
    ...
```

Refactor `ACVerifier.classify_pattern()` to delegate to `classify_pattern_type()`. Refactor `ACVerifier.unescape()` (and related `@staticmethod` unescape helpers) to delegate to the corresponding module-level functions. Refactor `ACVerifier.parse_feature_markdown()` to call `parse_ac_table()` (which returns `List[ACDefinition]`) and then filter results by `self.ac_type`. The `parse_ac_table()` function constructs `ACDefinition` objects directly, reusing the existing dataclass to avoid parallel dict representations, calling module-level `unescape()` for field processing. Call `detect_legacy_format()` inside `parse_ac_table()` when the `| AC# |` header line is detected. If it returns `True`, record a `format_error` entry (using `ERROR_CATEGORIES`) instead of parsing AC rows.

**Step 2: Add CLI arguments (Task 1)**

In `main()`, replace the standalone `--feature` argument with a mutually exclusive group:

```python
group = parser.add_mutually_exclusive_group()
group.add_argument("--scan-all", action="store_true",
                   help="Scan all feature files and produce catalog")
group.add_argument("--feature", help="Feature ID (e.g., 268)")

parser.add_argument("--parse-only", action="store_true",
                    help="Parse only; skip verification dispatch (use with --scan-all)")
parser.add_argument("--output",
                    help="Output path for JSON catalog (--scan-all only; default: stdout)")
```

Ensure `--ac-type` is validated only for the single-feature path. Add guards after arg parsing:

```python
if not args.scan_all and not args.feature:
    parser.error("Either --scan-all or --feature is required")
if not args.scan_all and args.feature and not args.ac_type:
    parser.error("--ac-type is required when --feature is used")
if args.parse_only and not args.scan_all:
    parser.error("--parse-only requires --scan-all")
if args.output and not args.scan_all:
    parser.error("--output requires --scan-all")
```

**Step 3: Implement `CatalogScanner` class (Task 3)**

Add `CatalogScanner` class to `ac-static-verifier.py` after `ACVerifier`:

```python
class CatalogScanner:
    """Batch scanner for all feature files. Implements --scan-all --parse-only mode."""

    def __init__(self, repo_root: Path,
                 output_path: Optional[Path] = None, verbose: bool = False):
        self.repo_root = repo_root
        self.output_path = output_path
        self.verbose = verbose

    def run(self) -> int:
        """Scan all feature files and produce JSON catalog. Returns exit code (0 = success)."""
        feature_files = sorted(glob_module.glob(
            str(self.repo_root / "pm" / "features" / "feature-*.md")
        ))
        # Accumulate catalog statistics
        features_scanned = 0
        pattern_types: dict = {}
        matcher_distribution: dict = {}
        per_type: dict = {t: 0 for t in STATIC_AC_TYPES}
        parse_errors: list = []

        for feature_path in feature_files:
            result = self._scan_feature_file(Path(feature_path))
            features_scanned += 1
            feature_id = result.get("feature_id", "unknown")
            parse_errors.extend(result.get("errors", []))
            for ac in result.get("ac_list", []):
                # Accumulate pattern_types, matcher_distribution, per_type
                # ac is an ACDefinition object (not a dict)
                pt_raw = getattr(ac, "pattern_type", None)
                if pt_raw is None:
                    parse_errors.append({"feature": feature_id, "ac_number": getattr(ac, "ac_number", None), "error": "parse_error", "detail": "pattern_type is None (classify_pattern_type failure)"})
                    continue
                pt = pt_raw.name
                pattern_types[pt] = pattern_types.get(pt, 0) + 1
                m = getattr(ac, "matcher", "")
                if m:
                    matcher_distribution[m] = matcher_distribution.get(m, 0) + 1
                t = getattr(ac, "ac_type", "")
                if t in per_type:
                    per_type[t] = per_type[t] + 1

        catalog = {
            "features_scanned": features_scanned,
            "pattern_types": pattern_types,
            "matcher_distribution": matcher_distribution,
            "per_type": per_type,
            "parse_errors": parse_errors,
        }

        json_str = json.dumps(catalog, indent=2)
        if self.output_path:
            Path(self.output_path).write_text(json_str, encoding="utf-8")
        else:
            print(json_str)
        return 0

    def _scan_feature_file(self, feature_path: Path) -> dict:
        """Parse a single feature file via parse_ac_table().

        Extracts feature ID from filename, calls parse_ac_table(),
        returns {'ac_list': [...], 'errors': [...]} with feature ID
        injected into each error entry.
        """
        ...
```

The `_scan_feature_file` method extracts the feature ID from the filename path (e.g., `feature-087.md` → `"087"`), calls `parse_ac_table(feature_path)` to get all ACs and errors without ac_type filtering, and injects the feature ID into each error entry. `CatalogScanner` is always parse-only — verification dispatch is `ACVerifier`'s responsibility.

**Step 4: Wire `main()` to branch on `--scan-all` (Task 4)**

After argument parsing in `main()`, add:

```python
if args.scan_all:
    repo_root = Path(args.repo_root) if args.repo_root else Path.cwd()
    output_path = Path(args.output) if args.output else None
    scanner = CatalogScanner(
        repo_root=repo_root,
        output_path=output_path,
        verbose=getattr(args, "verbose", False),
    )
    sys.exit(scanner.run())
# else: fall through to existing ACVerifier single-feature path
```

The single-feature path (`--feature` + `--ac-type`) remains completely unchanged.

**Step 5: Verify existing tests still pass (AC#11 backward compatibility)**

Run existing test suite after all changes:

```bash
python -m pytest src/tools/python/tests/ -v
```

All 29 pre-existing tests must pass.

**Step 6: Run integration verification (AC#12, AC#13)**

```bash
python src/tools/python/ac-static-verifier.py --scan-all --parse-only --repo-root C:/Era/devkit --output _out/tmp/scan-845.json
python -c "import json; json.load(open('_out/tmp/scan-845.json')); print('JSON valid')"
```

Both commands must exit 0.

### Build Verification Steps

1. Run `python src/tools/python/ac-static-verifier.py --scan-all --parse-only --repo-root C:/Era/devkit` — exit code must be 0
2. Run `python src/tools/python/ac-static-verifier.py --feature 845` — must fail (missing `--ac-type`), satisfying AC#11
3. Run existing test suite — all tests pass

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix

### Error Handling

- If `detect_legacy_format()` is called and returns `True` for a feature file's AC table header: record `{"feature": "{ID}", "ac_number": None, "error": "format_error", "detail": "AC table missing Method column"}` in `parse_errors` and skip all AC rows in that table
- If a specific AC row fails to parse (malformed pipe structure, non-integer AC#): record `{"feature": "{ID}", "ac_number": None, "error": "parse_error", "detail": "{exception message}"}` in `parse_errors` and continue scanning remaining files
- Any unhandled exception during `_scan_feature_file()` is caught, logged as `parse_error`, and the scan continues; the overall exit code remains 0
- If `--output` path directory does not exist: raise `FileNotFoundError` before scan begins (fail fast)

---

## Mandatory Handoffs

<!-- CRITICAL: Every out-of-scope issue discovered during implementation MUST be recorded here.
     Options:
     - Option A: Record here with Destination = specific feature ID
     - Option B: Record here with Destination = "New Feature" (created during Phase 9)
     - Option C: Fix in-scope if trivial (< 5 min) and directly related
     Validation: Pre-commit hook checks for empty Mandatory Handoffs in [DONE] features.
     DRAFT Creation: When creating destination features, use Deviation Context template. -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T13:10 | PHASE_START | orchestrator | Phase 1 Initialize | Feature 845 [WIP] |
<!-- run-phase-1-completed -->
| 2026-03-06T13:15 | PHASE_START | orchestrator | Phase 2 Investigation | Explorer dispatched |
| 2026-03-06T13:15 | PHASE_END | orchestrator | Phase 2 Investigation | Complete, infra skip→Phase 4 |
<!-- run-phase-2-completed -->
| 2026-03-06T13:20 | PHASE_START | orchestrator | Phase 4 Implementation | Tasks 1-4 dispatched |
| 2026-03-06T13:27 | PHASE_END | implementer | Phase 4 Implementation | All 4 Tasks [x], 0 deviations |
<!-- run-phase-4-completed -->
| 2026-03-06T13:30 | PHASE_START | orchestrator | Phase 7 Verification | AC lint + ac-tester |
| 2026-03-06T13:35 | AC_VERIFY | ac-tester | 17/17 ACs | All PASS |
| 2026-03-06T13:35 | PHASE_END | orchestrator | Phase 7 Verification | 0 deviations |
<!-- run-phase-7-completed -->
| 2026-03-06T13:40 | PHASE_START | orchestrator | Phase 8 Post-Review | feature-reviewer dispatched |
| 2026-03-06T13:41 | PHASE_END | orchestrator | Phase 8 Post-Review | READY, 8.2 skip (no extensibility), 8.3 N/A |
<!-- run-phase-8-completed -->
| 2026-03-06T13:50 | PHASE_END | orchestrator | Phase 9 Report | 0 deviations, 17/17 PASS |
<!-- run-phase-9-completed -->
| 2026-03-06T13:55 | PHASE_START | orchestrator | Phase 10 Finalize | Finalizer [DONE], commit 9f42c79 |
| 2026-03-06T14:00 | DEVIATION | CodeRabbit | classify_pattern_type(ac_type,...) | Major: wrong arg (ac_type→method) in parse_ac_table L347 |
| 2026-03-06T14:02 | FIX | orchestrator | ac_type→method | Fixed, amend commit 4fb6aa4 |
| 2026-03-06T14:05 | CodeRabbit | 0 findings | Re-review after fix | - |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: AC Coverage row AC#11 | AC#11 AC Coverage contradicted Implementation Contract (required=True vs manual guard)
- [fix] Phase2-Review iter1: Philosophy Derivation / AC table | Added AC#15 for runtime JSON content validation (features_scanned > 200, pattern_types non-empty)
- [fix] Phase2-Review iter2: Philosophy Derivation / AC table | Added AC#16 for parse_errors structure validation (error type categorization)
- [fix] Phase2-Review iter2: Goal section | Revised Goal wording from "catalogs unique AC pattern syntaxes" to "reports AC pattern type distribution and matcher distribution"
- [fix] Phase2-Review iter2: Links section | Fixed F844 status [DRAFT] → [PROPOSED]
- [fix] Phase2-Review iter3: Technical Design JSON example | Removed separate legacy_features key, merged format_error entries into unified parse_errors list
- [fix] Phase2-Review iter3: AC#16 | Added assert len(errs)>0 to prevent vacuous truth on empty parse_errors
- [fix] Phase2-Review iter4: Related Features + Dependencies + Links | Synced F844 status to actual [WIP]
- [fix] Phase3-Maintainability iter5: Technical Design Approach | Extracted parse_ac_table() standalone function to avoid ac_type filtering issue in CatalogScanner
- [fix] Phase3-Maintainability iter5: Technical Design Interfaces | Added STATIC_AC_TYPES module-level constant for OCP extensibility
- [fix] Phase3-Maintainability iter5: AC#16 | Added format_error presence assertion for runtime legacy detection validation
- [fix] Phase3-Maintainability iter5: Implementation Contract | Added no-argument guard and updated phase descriptions for parse_ac_table refactoring
- [fix] Phase3-Maintainability iter5: CLI extension | Added guard for neither --scan-all nor --feature provided
- [fix] Phase2-Review iter6: AC#15 | Added matcher_distribution non-empty assertion for Goal item 8 runtime validation
- [fix] Phase2-Review iter6: AC table | Added AC#17 for single-feature mode success path (C1 backward compat)
- [fix] Phase2-Review iter7: Technical Design Interfaces | Added classify_pattern_type() standalone function and specified pattern_type field in parse_ac_table() return schema
- [fix] Phase3-Maintainability iter8: Technical Design | ACVerifier.classify_pattern() delegates to standalone classify_pattern_type() (SSOT)
- [fix] Phase3-Maintainability iter8: Technical Design | Moved imports to module-level, removed inline import glob/json
- [fix] Phase3-Maintainability iter8: Technical Design | CatalogScanner always parse-only, removed parse_only parameter (undefined non-parse-only path)
- [fix] Phase3-Maintainability iter8: Technical Design | Added ERROR_CATEGORIES module-level constant for OCP
- [fix] Phase3-Maintainability iter8: Technical Design | Specified _scan_feature_file() boundary (feature ID extraction + error injection)
- [fix] Phase2-Review iter9: AC#15 | Added per_type sum > 0 assertion for Goal item 12 (C6) runtime validation
- [fix] Phase3-Maintainability iter10: CLI guards | Added --parse-only requires --scan-all and --output requires --scan-all guards
- [fix] Phase3-Maintainability iter10: Technical Design | Added explicit ACDefinition field mapping for classify_pattern_type delegation
- [fix] Phase3-Maintainability iter10: JSON schema | Documented per_type vs pattern_types count discrepancy (non-static AC types)
- [fix] Phase1-RefCheck iter10: Related Features / Dependencies / Links | F844 status [WIP] → [DONE]
- [fix] Phase3-Maintainability iter11: Technical Design Interfaces | parse_ac_table returns List[ACDefinition] instead of list[dict] (eliminate parallel data representation)
- [fix] Phase3-Maintainability iter11: Technical Design CLI | Added --scan-all requires --parse-only guard (make flag meaningful, reserve future batch verification path)
- [fix] Phase3-Maintainability iter1: Technical Design Approach + Implementation Contract | Extract unescape() and helpers as module-level functions for parse_ac_table() use
- [fix] Phase3-Maintainability iter1: Mandatory Handoffs | Added tracking entry for deferred full batch verification mode (--scan-all without --parse-only)
- [fix] Phase2-Review iter2: Mandatory Handoffs + CLI guard | Removed TBD handoff entry and "reserved for future" language (speculative deferred work violates TBD Prohibition)
- [fix] Phase3-Maintainability iter3: Technical Design Approach | Specified parse_ac_table() contains full pipe-splitting logic; parse_feature_markdown() becomes thin wrapper
- [fix] Phase3-Maintainability iter3: Technical Design Interfaces | classify_pattern_type() returns PatternType enum, not str
- [resolved-applied] Phase3-Maintainability iter3: pattern_types vs per_type scope clarification (resolved: iter10 JSON schema comment already documents discrepancy)
- [fix] Phase3-Maintainability iter3: Task#2 AC mapping | Added AC#17 to Task#2 for parse_ac_table() delegation validation
- [fix] Phase2-Review iter4: Review Notes | Resolved [pending] pattern_types vs per_type item (already addressed by iter10 JSON schema comment)
- [fix] Phase3-Maintainability iter5: CatalogScanner.run() pseudocode | Fixed PatternType enum to .name string conversion for JSON serialization
- [fix] Phase3-Maintainability iter6: Philosophy | Changed "ensure" to "measure and report" (aligns with AC#16 which expects parse errors)
- [fix] Phase3-Maintainability iter6: CatalogScanner.run() pseudocode | Added None pattern_type guard with parse_error recording (Fail Fast)
- [fix] Phase3-Maintainability iter6: AC#15 | Added pattern_types >= per_type invariant assertion
- [fix] Phase3-Maintainability iter7: CatalogScanner.run() pseudocode | Added feature_id extraction from _scan_feature_file result
- [fix] Phase3-Maintainability iter7: Interfaces section | Added unescape(), unescape_for_regex_pattern(), unescape_for_literal_search() signatures

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->
## Links

[Related: F842](feature-842.md) - Direct predecessor — pattern parsing enhancements [DONE]
[Related: F844](feature-844.md) - Sibling — `_UNESCAPE_RULES` catch-all refactoring [DONE]
[Related: F841](feature-841.md) - Cross-repo build CWD resolution in verifier [DONE]
[Related: F838](feature-838.md) - Cross-repo verifier path resolution [DONE]
[Related: F834](feature-834.md) - Format C guard DRY consolidation [DONE]
[Related: F832](feature-832.md) - Earlier verifier pattern work [DONE]
[Related: F818](feature-818.md) - Initial ac-static-verifier creation [DONE]
