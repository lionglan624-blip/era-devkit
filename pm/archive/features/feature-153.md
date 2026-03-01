# Feature 153: Kojo Test Framework Fixes

## Status: [DONE]

## Type: engine

## Background

### Problem
Two issues discovered during Feature 135 implementation:

1. **Multi-file execution bug**: `--unit` with multiple JSON files causes "CSV directory not found" error
2. **TALENT isolation bug**: Default setup sets `TALENT:恋人+恋慕` for all characters, preventing isolated testing of individual TALENT branches (恋慕/思慕/なし)

### Goal
Fix both issues to enable proper kojo test execution.

### Context
- Discovered in: Feature 135 (COM_20 キスする)
- Blocking: Phase 4-5 smoke test completion
- Impact: All future kojo features requiring multi-branch testing

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Multi-file test execution | output | contains | "passed" | [x] |
| 2 | TALENT isolation (人妻:15) | output | contains | "TALENT:TARGET:15=1" | [x] |
| 3 | TALENT isolation (ツンデレ:14) | output | contains | "TALENT:TARGET:14=1" | [x] |
| 4 | TALENT isolation (なし) | output | not_contains | "TALENT:TARGET:16" | [x] |
| 5 | Build succeeds | build | succeeds | - | [x] |

> **Note**: AC#2-3 originally mislabeled as 恋慕/思慕. Corrected to match actual TALENT indices (see Talent.csv).

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Fix multi-file JSON path handling in KojoTestMode | [x] |
| 2 | 2 | Change TALENT setup to isolate 恋慕 state from JSON only (no defaults) | [x] |
| 3 | 3 | Change TALENT setup to isolate 思慕 state from JSON only (no defaults) | [x] |
| 4 | 4 | Change TALENT setup to isolate なし state from JSON only (no defaults) | [x] |
| 5 | 5 | Build verification | [x] |

---

## Technical Notes

### Issue 1: Multi-file execution
- Location: `engine/uEmuera/KojoTestMode.cs` (probable)
- Symptom: Second file onward fails with "CSV directory not found"
- Cause: Likely working directory reset between tests

### Issue 2: TALENT isolation
- Location: `engine/uEmuera/KojoTestMode.cs` (probable)
- Current behavior: `[KojoTest] Set TALENT:恋人+恋慕 for K1-K10 characters`
- Expected behavior: Only set TALENTs specified in JSON `state` field

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-20 | implementer | Fixed multi-file path handling (Path.GetFullPath) | SUCCESS |
| 2025-12-20 | implementer | Removed unconditional TALENT:16+3 setup | SUCCESS |
| 2025-12-20 | ac-tester | AC#1 Multi-file test (glob pattern) | PASS |
| 2025-12-20 | ac-tester | AC#2-4 TALENT isolation verification | PASS |
| 2025-12-20 | ac-tester | AC#5 Build verification | PASS |

---

## Links

- [index-features.md](index-features.md)
- Blocked feature: [feature-135.md](feature-135.md)
