# Feature 745: Selective Recovery Merge (F742 Output)

## Status: [CANCELLED]

### Cancellation Reason

完全復旧断念。手動復旧を開始したため。

## Type: infra

## Background

### Philosophy (Mid-term Vision)

**Pragmatic recovery: Merge verified improvements from F742 recovery output, not bulk replacement.**

F733-F744 attempted automated complete recovery but failed due to chain gaps causing broken code. The recovery output exists and contains valuable content (6px borders, additional CSS classes, etc.). Instead of fixing the tool, manually merge the valuable parts.

---

### Problem (Current Issue)

F742 achieved 9.32% skip rate but full merge produced broken code:
- 790 files merged
- 27 Dashboard tests failed
- ReferenceErrors: `CHAIN_WAITER_TIMEOUT_MS`, `validateCommand` not defined
- Root cause: chain gaps skipped definitions, kept usages

**However**: The recovery output contains genuine improvements that HEAD is missing.

### Goal (What to Achieve)

Selectively merge F742 recovery output:
1. Identify files where recovery has content HEAD is missing
2. For each file, compare and extract only the missing parts
3. Validate each merge (tests pass, no undefined references)
4. Preserve HEAD's working definitions

---

## Technical Design

### Approach: Diff-based Selective Merge

```
For each recovered file:
1. Compare recovery vs HEAD
2. If recovery has MORE content:
   a. Identify specific additions (CSS classes, functions, etc.)
   b. Check if additions reference undefined symbols
   c. If safe, merge additions into HEAD
   d. Run tests after each file
3. If HEAD has content recovery lacks:
   a. Keep HEAD version (recovery has chain gaps)
```

### Priority Files (Known Missing Content)

| File | Recovery | HEAD | Missing in HEAD |
|------|:--------:|:----:|-----------------|
| main.css | 1738行 | 1664行 | 6px borders, .tree-session-*, .tree-running-label |
| claudeService.js | 1838行 | 1497行 | **Needs careful review** - definitions may be missing in recovery |
| logger.js | 132行 | 66行 | Additional logging functionality |
| ExecutionPanel.jsx | 268行 | 231行 | UI improvements |
| LogViewer.jsx | 93行 | 57行 | Enhanced log display |

### Merge Strategy by File Type

| Type | Strategy |
|------|----------|
| **CSS** | Safe to merge additions (no code dependencies) |
| **JS/JSX** | Must verify all references are defined |
| **Config** | Compare and merge carefully |
| **Markdown** | Usually HEAD is newer, skip |

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | main.css has 6px borders | output | contains | "6px solid" | [ ] |
| 2 | main.css has .tree-running-label | output | contains | ".tree-running-label" | [ ] |
| 3 | Dashboard tests pass | test | succeeds | npm test (132 pass) | [ ] |
| 4 | No ReferenceError in merged code | output | not_contains | "is not defined" | [ ] |
| 5 | Frontend tests pass | test | succeeds | npm test frontend (224 pass) | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | - | Generate detailed diff for priority files | [ ] |
| 2 | 1,2 | Merge main.css CSS additions (safe - no code deps) | [ ] |
| 3 | 3 | Run backend tests after main.css merge | [ ] |
| 4 | - | Review claudeService.js diff - identify safe additions | [ ] |
| 5 | 4 | Merge claudeService.js safe additions only | [ ] |
| 6 | 3,4 | Run tests after claudeService.js merge | [ ] |
| 7 | - | Review and merge other priority files | [ ] |
| 8 | 5 | Final validation - all tests pass | [ ] |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F742 | [DRAFT] | Recovery output exists at .tmp/recovery/ |
| Related | F735 | [DONE] | Dashboard Frontend Recovery (partial) |

---

## Links

- [F742](feature-742.md) - Session Extractor Complete Recovery (source of recovery files)
- [F735](feature-735.md) - Dashboard Frontend Recovery (test-driven partial recovery)
- [F743](feature-743.md) - Fundamental Redesign (validation-first, not implemented)
- [F744](feature-744.md) - Incident-Point Recovery (reviewed, not implemented)

---

## Review Notes

### Why Selective Merge Instead of Tool Fix

| Approach | Effort | Risk | Outcome |
|----------|--------|------|---------|
| Fix session extractor | High | Unknown | May still fail |
| Selective merge | Medium | Low | Known working result |

The recovery output exists. The valuable content is identifiable. Manual merge with validation is more reliable than attempting to fix the fundamental chain gap problem.

### Lessons from F742 Merge Failure

1. **Never bulk merge** - validate each file
2. **CSS is safe** - no code dependencies
3. **JS needs careful review** - definitions may be missing
4. **Tests are the gate** - no merge without passing tests

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-03 | DRAFT | Created after F742/F743/F744 analysis showed tool fix is impractical |

